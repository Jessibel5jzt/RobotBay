using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WestBay
{
	/// <summary>
	/// 一些通用UI 和 默认UI，比如弹出框，游戏内通用UI等。
	/// </summary>
	/// @ingroup CoreApi
	public class LobbyUI : Singleton<LobbyUI>
	{
		#region 通用Popup

		/// <summary>
		/// 显示对话框
		/// </summary>
		/// <param name="text">需要显示的文字</param>
		/// <param name="pt">对话框类型</param>
		/// <param name="onBtnClicked">按钮事件回调</param>
		/// <param name="imageName">DefatulUI中的图片名称</param>
		public void PopupShow(string text, PopupType pt, Action<PopButton> onBtnClicked = null, string imageName = null)
		{
			if (_popupUI == null) return;
			_popupUI.Transform.gameObject.SetActive(true);
			if (imageName != null)
			{
				Sprite btn_n = Array.Find<Sprite>(DefaultRes, (s) => s.name == imageName);
				if (btn_n != null)
					_popupUI.YES.transform.GetChild(0).GetComponent<Image>().sprite = btn_n;
				Sprite btn_s = Array.Find<Sprite>(DefaultRes, (s) => s.name == (imageName + "s"));
				if (btn_s != null)
				{
					SpriteState SS = _popupUI.YES.transform.GetChild(0).GetComponent<Selectable>().spriteState;
					SS.disabledSprite = btn_s;
					_popupUI.YES.transform.GetChild(0).GetComponent<Selectable>().spriteState = SS;
					_popupUI.YES.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
				}
			}

			switch (pt)
			{
				case PopupType.OK:
					_popupUI.NO.gameObject.SetActive(false);
					_popupUI.YES.gameObject.SetActive(false);
					_popupUI.OK.gameObject.SetActive(true);
					_popupUI.ScreenBtn.gameObject.SetActive(true);
					break;

				case PopupType.YESNO:
					_popupUI.OK.gameObject.SetActive(false);
					_popupUI.NO.gameObject.SetActive(true);
					_popupUI.YES.gameObject.SetActive(true);
					_popupUI.ScreenBtn.gameObject.SetActive(false);
					break;

				case PopupType.Tips:
					_popupUI.OK.gameObject.SetActive(false);
					_popupUI.NO.gameObject.SetActive(false);
					_popupUI.YES.gameObject.SetActive(false);
					_popupUI.ScreenBtn.gameObject.SetActive(false);
					break;

				default:
					break;
			}
			_popupUI.OK.transform.GetChild(0).GetComponent<Selectable>().interactable = true;
			_popupUI.YES.transform.GetChild(0).GetComponent<Selectable>().interactable = true;
			_popupUI.NO.transform.GetChild(0).GetComponent<Selectable>().interactable = true;

			_popupUI.Text.text = text;
			_popupUI.AddListener(onBtnClicked);
		}

		/// <summary>
		/// 对话框按钮
		/// </summary>
		public enum PopButton
		{
			/// <summary>
			/// 是
			/// </summary>
			YES,

			/// <summary>
			/// 否
			/// </summary>
			NO,

			/// <summary>
			/// 好的
			/// </summary>
			OK,
		}

		/// <summary>
		/// 对话框类型
		/// </summary>
		public enum PopupType
		{
			/// <summary>
			/// 通知型：只有一个按钮 “好的”
			/// </summary>
			OK,

			/// <summary>
			/// 选择型：有两个按钮 “是”，“否”
			/// </summary>
			YESNO,

			/// <summary>
			/// 提示型：没有按钮
			/// </summary>
			Tips,
		}

		#endregion 通用Popup

		#region Loading UI

		/// <summary>
		/// 显示或隐藏Loading弹窗。在进行耗时的等待任务前，推荐显示出来。
		/// </summary>
		public void LoadingShow(bool show = true)
		{
			_loadingUI.Show(show);
		}

		#endregion Loading UI

		#region 更新UI

		public OnUpadateClosed ClosedDel { get; set; }

		public delegate void OnUpadateClosed();

		private void OnUpdateClose()
		{
			_updateUI.ClosedUpdateUI();
			ClosedDel?.Invoke();
		}

		private void OnUpdateFinish()
		{
			ResetEXE();
		}

		/// <summary>
		/// 显示更新界面
		/// </summary>
		public void UpdateShow()
		{
			if (_updateUI == null) return;
			_updateUI.OpenUpdateUI();
		}

		public void UpdateRetry()
		{
			if (_updateUI == null) return;
			_updateUI.Retry();
		}

		public void UpdateClosed()
		{
			if (_updateUI == null) return;
			_updateUI.ClosedUpdateUI();
		}

		public void UpdateProgress(float progress)
		{
			if (_updateUI == null) return;
			_updateUI.UpdateProgress(progress);
		}

		public void UpdateInfo(string msg = null)
		{
			if (_updateUI == null) return;
			_updateUI.UpdateInfo(msg);
		}

		public void UpdateUnderlyError(string msg = null, UnityAction btn1Callback = null, UnityAction btn2Callback = null)
		{
			if (_updateUI == null) return;
			_updateUI.ShowUpdateError(msg, btn1Callback, btn2Callback);
		}

		public void UpdateFinish(UnityAction btnCallback = null, string msg = null)
		{
			if (_updateUI == null) return;
			_updateUI.UpdateFinish(btnCallback, msg);
		}

		#endregion 更新UI

		public Transform DefaultUI { get; set; }
		public Sprite[] DefaultRes { get; set; }
		private IniFile _defaultText;

		private PopupUI _popupUI;
		private UpdateUI _updateUI;
		private LoadingUI _loadingUI;

		public const string SharedModule = "Shared";

		public void StartMgr()
		{
			InitMgr();
			Debug.Log($"<color=green>[LobbyUI]</color> 服务启动");
		}

		public void StopMgr()
		{
		}

		private void InitMgr()
		{
			DefaultRes = ResourceLoader.Ins.SpritesGet(SharedModule, $"images/DefaultUI{ThemeMgr.ThemeSplitMarker}.png");
			_defaultText = IniMgr.LoadModuleLanguageFile(SharedModule, "ui");
			DefaultUI = GameObject.Instantiate(ResourceLoader.Ins.PrefabGet(SharedModule, "DefaultUI") as GameObject, MB.Ins.transform).transform;

			//Loading
			var transform = GetUI(DefaultUI, "LoadingPanel");
			_loadingUI = new LoadingUI(transform);

			//Popup
			Transform dlgTr = GetUI(DefaultUI, "Dialogure");
			_popupUI = new PopupUI();
			_popupUI.InitWithTransform(dlgTr);

			//Update
			_updateUI = new UpdateUI();
		}

		private Transform GetUI(Transform ui, string name)
		{
			var Tr = ui.Find(name);
			return Tr;
		}

		public string GetPlatformText(string key)
		{
			var val = _defaultText.GetValue(key);
			if (string.IsNullOrEmpty(val)) return $"[{key}]";
			return val;
		}

		/// <summary>
		/// 获取动态创建窗口层级
		/// </summary>
		/// <returns></returns>
		public int GetValideSiblingIndex()
		{
			var index = -1;
			if (_loadingUI.transform != null)
			{
				index = _loadingUI.transform.GetSiblingIndex() - 1;
			}
			return index;
		}

		#region 重启

		/// <summary>
		/// 重新启动程序
		/// </summary>
		public void ResetEXE()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsPlayer:
					Debug.Log("Windows");
					Reset();
					break;

				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
				case RuntimePlatform.LinuxPlayer:
					Debug.Log("自行重启");
					break;

				default:
					break;
			}
		}

		private int _delayTime = 1000; //延时重启时间

		/// <summary>
		/// pc重启
		/// </summary>
		private void Reset()
		{
			string[] strs = new string[]
			   {
				  "@echo off",
				  "echo wscript.sleep {0} > sleep.vbs",
				  "start /wait sleep.vbs",
				  "start /d \"{0}\" {1}",
				  "del /f /s /q sleep.vbs",
				  "exit"
			   };

			string path = Application.dataPath;
			path = path.Remove(path.LastIndexOf("/")) + "/";
			string name = $"{Application.productName}.exe";
			strs[1] = string.Format(strs[1], _delayTime);
			strs[3] = string.Format(strs[3], path, name);

			string batPath = Application.dataPath + "/../restart.bat";
			if (File.Exists(batPath))
			{
				File.Delete(batPath);
			}

			using (FileStream fileStream = File.OpenWrite(batPath))
			{
				using (StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.GetEncoding("UTF-8")))
				{
					foreach (string s in strs)
					{
						writer.WriteLine(s);
					}
					writer.Close();
				}
			}

			Application.Quit();
			Application.OpenURL(batPath);
		}

		#endregion 重启
	}
}