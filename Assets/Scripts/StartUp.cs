using Cysharp.Threading.Tasks;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace WestBay
{
	public class StartUp : MonoBehaviour
	{
		#region 只在 编辑器中 有效

		[Header("只在 编辑器中 有效：")]
		[SerializeField]
		[FieldLabel("启动模块名")]
		private string FirstModule = null;

		[SerializeField]
		[Tooltip("不选则使用DLL")]
		[FieldLabel("使用C#脚本")]
		private bool NoUseDll = true;

		[Tooltip("不选则使用Assetbundle包")]
		[SerializeField]
		[FieldLabel("编辑器模式")]
		private bool _isEditor = false;

		#endregion 只在 编辑器中 有效

		private void Awake()
		{
			System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
			System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo("en-US");

			BootLogoVideo();
			Debug._New(PathUtil.GetPersistPath());
		}

		private void Start()
		{
			if (Application.isEditor)
			{
				App.UseModuleScript = NoUseDll;
				App.IsEditor = _isEditor;
				if (_isEditor) App.UseModuleScript = true;
			}

			UnzipOr1stModule().Forget();
		}

		#region Boot Logo Video

		private bool _isDBReady = false;
		private bool _isPlayComplete = false;

		private void BootLogoVideo()
		{
			var videoPlayer = (VideoPlayer)Camera.main.GetComponent(typeof(VideoPlayer));
			if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
			{
				videoPlayer.loopPointReached += PlayFinish;
			}
			else
			{
				PlayFinish(videoPlayer);
			}
		}

		private void PlayFinish(VideoPlayer videoPlayer)
		{
			videoPlayer.Stop();

			_isPlayComplete = true;
			if (DBUpdateMgr.Ins != null && DBUpdateMgr.Ins.DBUpdateUI != null && !_isDBReady)
			{
				DBUpdateMgr.Ins.DBUpdateUI.ShowUI(true);
			}

			videoPlayer.loopPointReached -= PlayFinish;
		}

		#endregion Boot Logo Video

		private async UniTaskVoid UnzipOr1stModule()
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				var infoPath = Application.persistentDataPath + "/installation.txt";
				File.WriteAllText(infoPath, System.AppDomain.CurrentDomain.BaseDirectory);
			}
			await UniTask.Delay(1);
			//todo 解压

			//var updateSmMgr = gameObject.GetComponent<UpdateSubmoduleMgr>();
			//if (updateSmMgr.IsNeedUpdate())
			//{
			//	var result = await updateSmMgr.BeginUpdate();
			//	if (!result) return;
			//}

			////进入解压
			//if (UnpackMgr.NeedUnzip())
			//{
			//	var Um = gameObject.GetComponent<UnpackMgr>();
			//	Um.Run();
			//}
			//else
			//{
			//	Init();
			//}
		}

		public void Init()
		{
			ConfigMgr._New();
			MB._New();

			CreateReporter();
			NewSingleton();
			//ResourceLoader.Ins.PreloadModule(LobbyUI.SharedModule);

			//WebReqHelper.WebServerRequest().Forget();
			StartCoroutine(LoadAndChange());

			//MB.Ins.InitSubsystemEvent();
		}

		private IEnumerator LoadAndChange()
		{
			SetFirstModuleName();

			//等待动画播放完毕
			yield return new WaitUntil(() => _isPlayComplete);

			//等待Shared资源加载完毕
			yield return new WaitUntil(() => ResourceLoader.Ins.IsLoaded(LobbyUI.SharedModule));

			LobbyUI.Ins.StartMgr();
			Lobby.Ins.StartMgr();

			//等待数据库初始化完毕
			yield return new WaitUntil(() => _isDBReady);

			DatabaseMgr.Ins.StartMgr();

			var preloadUI = new InitialUpdateUI();
			var msg = LobbyUI.Ins.GetPlatformText("preloadTip");
			preloadUI.Begin($"{msg}0%", false, false);
			ResourceLoader.Ins.StartMgr();

			//等待预加载完毕
			while (true)
			{
				var progress = ResourceLoader.Ins.PreloadProgress * 100;
				if (progress > 100) progress = 100.1f;

				preloadUI.SetMessage($"{msg}{(int)progress}%");
				preloadUI.SetProgress(progress);
				if (progress >= 100) break;

				yield return null;
			}

			if (true)//"ConfigMgr.Ins.Prefs.AutoLogin.Equals()"
			{
				NextModuleLoad(FirstModule);
			}
			else
			{
				//todo 自动登录
				//var isAutoLoggedin = false;
				//CheckAutoLogin(() =>
				//{
				//	isAutoLoggedin = true;
				//});

				////等待自动登录
				//yield return new WaitUntil(() => isAutoLoggedin);
			}
			FirstModuleEnter();
		}

		private void SetFirstModuleName()
		{
			if (Application.isEditor && (!string.IsNullOrEmpty(FirstModule)))
			{
				//Use FirstModule directly
			}
			else
			{
				FirstModule = IniMgr.Config.GetValue("startscene");
			}
		}

		private void NextModuleLoad(string moduleName = "")
		{
			if (string.IsNullOrWhiteSpace(moduleName))
			{
				moduleName = FirstModule;
			}

			Lobby.Ins.NextModuleLoad(moduleName);
		}

		private void CreateReporter()
		{
			if (App.IsDebug)
			{
				var reporterObj = Resources.Load<GameObject>("Prefabs/LogsViewer");
				if (reporterObj != null)
				{
					Instantiate(reporterObj);
				}
			}
		}

		private void NewSingleton()
		{
			DBUpdateMgr._New();
			DBMgr._New();

			//数据库：
			if (DBUpdateMgr.Ins.IsNeedUpdate())
			{
				DBUpdateMgr.Ins.StartUpdate((isFinish) =>
				{
					_isDBReady = isFinish;
				});
			}
			else
			{
				DBMgr.Ins.InitGenuineDB();
				_isDBReady = true;
			}

			//各子系统
			ResourceLoader._New();
			ConfigMgr.Ins.Init();
			Lobby._New();
			DllLoader._New();
			LobbyUI._New();
			DatabaseMgr._New();
			MqttMgr._New();

			//其他添加：
			NotificationCenter._New();
		}

		private void CheckAutoLogin(UnityAction onChecked)
		{
			//var userInfo = UserMgr.Ins.RecentLoginUsers()[0];
			//var account = string.IsNullOrEmpty(userInfo.Phone) ? userInfo.Email : userInfo.Phone;
			//var password = userInfo.Password;
			//var isRemember = userInfo.IsRemember == "1";
			//UserMgr.Ins.Login(account, password, isRemember, (ret, user) =>
			//{
			//	if (ret)
			//	{
			//		//设置当前用户
			//		//医院版用户不能登陆，只能管理员和治疗师登陆
			//		if (user.Identity == 1 || UserMgr.Ins.IsDeveloper())
			//		{
			//			//治疗师登陆
			//			UserMgr.Ins.DoctorDataSync(() =>
			//			{
			//				if (!UserMgr.Ins.IsMatchPrefs)
			//				{
			//					LobbyUI.Ins.PopupShow(LocalTextMgr.GetValue("settingTip"), LobbyUI.PopupType.OK, (btn) =>
			//					{
			//						if (btn == LobbyUI.PopButton.OK)
			//						{
			//							ConfigMgr.Ins.Prefs.Language = UserMgr.Ins.CurrentDoctor.Language;
			//							ConfigMgr.Ins.Prefs.Theme = UserMgr.Ins.CurrentDoctor.Theme;
			//							NextModuleLoad("User");
			//							onChecked?.Invoke();
			//						}
			//					}, "icon_account_n_2");
			//				}
			//				else
			//				{
			//					NextModuleLoad("User");
			//					onChecked?.Invoke();
			//				}
			//			});
			//		}
			//		else if (user.Identity == 0)
			//		{
			//			//病人可以登录
			//			//进入下一个模块
			//			NextModuleLoad("Hall");
			//			onChecked?.Invoke();
			//		}
			//	}
			//	else
			//	{
			//		LobbyUI.Ins.PopupShow(LocalTextMgr.GetValue(user == null ? "errormm" : "brerror"), LobbyUI.PopupType.OK, null, "icon_account_n_2");
			//	}
			//});
		}

		private void FirstModuleEnter()
		{
			MustConnectShow((result) =>
			{
				if (result)
				{
					Lobby.Ins.NextModuleEnterWhenReady();
				}
				else
				{
					FirstModuleEnter();
				}
			});
		}

		/// <summary>
		/// 连接设备提示
		/// </summary>
		/// <returns></returns>
		private void MustConnectShow(System.Action<bool> callback = null)
		{
			if (IsConnectValid())
			{
				callback?.Invoke(true);
			}
			else
			{
				LobbyUI.Ins.PopupShow(LocalTextMgr.GetValue("FaultInfo"), LobbyUI.PopupType.OK, (btn) =>
				{
					if (btn == LobbyUI.PopButton.OK)
					{
						callback?.Invoke(IsConnectValid());
					}
				}, "btn_update_n_2");
			}
		}

		/// <summary>
		/// 连接是否有效
		/// </summary>
		/// <returns></returns>
		private bool IsConnectValid()
		{
			string value = IniMgr.Config.GetValue("Robot_FFTAI_Key");
			if (!string.IsNullOrEmpty(value) && string.Compare(value, "3s*GCU#G9bx&N2fEFsEUFc*qZytArKss") == 0)
			{
				return true;
			}

			return false;
		}
	}//class
}//namespace