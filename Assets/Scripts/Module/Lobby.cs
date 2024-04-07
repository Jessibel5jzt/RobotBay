using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace WestBay
{
	/// <summary>
	/// 模块管理类
	/// </summary>
	/// @ingroup CoreApi
	public class Lobby : Singleton<Lobby>
	{
		/// <summary>
		/// Raise when module start load
		/// </summary>
		internal event Action<string> OnLoadEvent;

		/// <summary>
		/// Raise when module load finished
		/// </summary>
		internal event Action<Module> OnLoadedEvent;

		/// <summary>
		/// Raise when enter module scene
		/// </summary>
		public event Action<Module> OnEnterEvent;

		private readonly ModuleLoader _moduleLoader;
		private Module ThisModule { get; set; }
		private Module NextModule = null;
		private Module LastModule = null;
		private string LastModuleName = null;

		/// <summary>
		/// 模块传入参数
		/// </summary>
		private object _moduleArg = null;

		/// <summary>
		/// 模块加载标识
		/// </summary>
		private bool _isModuleLoading = false;

		public Lobby()
		{
			_moduleLoader = new ModuleLoader();
		}

		public void StartMgr()
		{
			PreloadLiteMod();
			Debug.Log($"<color=green>[Lobby]</color> 服务启动");
		}

		public void StopMgr()
		{
			UnloadAllLiteModule();
		}

		#region Pre Load LiteMod

		/// <summary>
		/// 是否预加载
		/// </summary>
		private bool IsPreload => IniMgr.Config.GetValue($"Lobby_Preload_Enable").Equals("1");

		/// <summary>
		/// Preload all module AB from config file
		/// </summary>
		public void PreloadLiteMod()
		{
			if (!IsPreload) return;

			List<string> moduleList = new List<string>();
			PreloadFromConfig("Lobby_Preload_LiteMod", moduleList);

			for (var i = 0; i < moduleList.Count; ++i)
			{
				var modName = moduleList[i];
				string modPath = PathUtil.GetPersistPath(modName);
				if (Directory.Exists(modPath))
				{
					LiteModuleLoad(modName, (module) =>
					{
						if (module != null)
						{
							MB.Ins.StartCoroutine(LiteModuleEnter(module));
						}
						else
						{
							Debug.LogError($"[Lobby]: LiteModule {modName} preload Error!");
						}
					});
				}
			}
		}

		/// <summary>
		/// Load module name list from ini file[Config]
		/// </summary>
		/// <param name="configKey"></param>
		/// <param name="isDevice"></param>
		/// <param name="moduleList"></param>
		private void PreloadFromConfig(string configKey, List<string> moduleList)
		{
			string value = IniMgr.Config.GetValue(configKey);
			if (!string.IsNullOrEmpty(value))
			{
				string[] moduleArray = value.Split(',');
				for (var i = 0; i < moduleArray.Length; ++i)
				{
					string moduleName = $"{moduleArray[i]}";
					moduleList.Add(moduleName);
				}
			}
		}

		private void UnloadAllLiteModule()
		{
			if (!IsPreload) return;

			List<string> moduleList = new List<string>();
			PreloadFromConfig("Lobby_Preload_LiteMod", moduleList);

			for (var i = 0; i < moduleList.Count; ++i)
			{
				var modName = moduleList[i];
				LiteModuleUnload(modName);
			}
		}

		#endregion Pre Load LiteMod

		#region Lite Module

		/// <summary>
		/// 轻模块加载
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="callback"></param>
		public void LiteModuleLoad(string moduleName, Action<Module> callback = null)
		{
			Debug.Log($"LiteModuleLoad -> {moduleName}");

			_moduleLoader.LoadDLLLite(moduleName, (module) =>
			{
				module?.OnLoad();
				callback?.Invoke(module);
			});
		}

		/// <summary>
		/// 轻模块加载并进入
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="arg">传放参数</param>
		/// <param name="callback">回调方法</param>
		public void LiteModuleLoadAndEnter(string moduleName, object arg = null, Action<Module> callback = null)
		{
			Debug.Log($"LiteModuleLoad -> {moduleName}");

			_moduleLoader.LoadDLLLite(moduleName, (module) =>
			{
				if (module != null)
				{
					module.OnLoad();
					MB.Ins.StartCoroutine(LiteModuleEnter(module, arg, callback));
				}
				else
				{
					callback?.Invoke(null);
				}
			});
		}

		/// <summary>
		/// 判断模块是否加载（加载中）
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public bool IsLiteModuleLoad(string moduleName)
		{
			return _moduleLoader.IsModLoad(moduleName);
		}

		/// <summary>
		/// 卸载轻模块
		/// </summary>
		/// <param name="module">模块对象</param>
		public void LiteModuleUnload(Module module)
		{
			Debug.Log($"LiteModuleUnload -> {module.Name}");
			module.Exit();

			_moduleLoader.LiteModuleUnload(module);
			ResourceLoader.Instance.UnloadOnce(module.Name);
		}

		/// <summary>
		/// 卸载轻模块
		/// </summary>
		/// <param name="moduleName">模块名</param>
		public void LiteModuleUnload(string moduleName)
		{
			Module module = _moduleLoader.LiteModuleGet(moduleName);
			if (module != null)
			{
				LiteModuleUnload(module);
			}
		}

		/// <summary>
		/// 轻模块是否加载好（资源加载）
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="arg">传放参数</param>
		/// <param name="callback">回调方法</param>
		/// <returns></returns>
		private IEnumerator LiteModuleEnter(Module module, object arg = null, Action<Module> callback = null)
		{
			while (!LightModuleIsReady(module))
			{
				yield return null;
			}
			AddTranslation(module.Name);
			module.Enter(arg);
			callback?.Invoke(module);
		}

		/// <summary>
		/// 检测轻模块加载状态
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		private bool LightModuleIsReady(Module module)
		{
			if (module == null) return false;
			bool isReady = false;
			try
			{
				isReady = module.IsReady();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return isReady;
		}

		#endregion Lite Module

		private Module GetThisModule()
		{ return ThisModule; }

		/// <summary>
		/// 得到当前模块的名称
		/// </summary>
		/// <returns></returns>
		public string GetThisModuleName()
		{
			if (ThisModule == null) return App.SharedModule;
			return ThisModule.Name;
		}

		#region Module load and enter

		private Action<Module> _moduleLoadCallback;

		/// <summary>
		/// 加载模块并进入场景
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="arg">约定为Dictionary<string,string>类型</param>
		/// <param name="callback"></param>
		/// <param name="isSame">是否跳过与上个模块重名的判断<bool>类型</param>
		public void LoadAndEnterScene(string moduleName, object arg = null, Action<Module> callback = null, bool isSame = false)
		{
			Debug.Log($"NextModuleLoad -> {moduleName}");
			if (_isModuleLoading || !isSame && ThisModule.Name.Equals(moduleName))
			{
				return;
			}

			OnLoadEvent?.Invoke(moduleName);
			_isModuleLoading = true;
			_moduleArg = arg;
			_moduleLoadCallback = callback;
			_moduleLoader.LoadDLL(moduleName, (module) =>
			{
				if (module != null)
				{
					NextModule = module;
					module.OnLoad();

					OnLoadedEvent?.Invoke(module);
					MB.Ins.StartCoroutine(CheckModuleIsReady());
				}
				else
				{
					LoadException(moduleName);
				}
			});
		}

		public void LoadException(string moduleName)
		{
			if (_isModuleLoading)
			{
				//error
				_moduleLoadCallback?.Invoke(null);

				NextModule = null;
				_isModuleLoading = false;
				_moduleLoadCallback = null;
			}
		}

		/// <summary>
		/// 游戏加载失败弹框
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="args">约定为Dictionary<string,string>类型</param>
		/// <param name="callback"></param>
		/// <param name="isSame">是否跳过与上个模块重名的判断<bool>类型</param>
		public void ModuleLoading(string moduleName, object args = null, Action<bool> callBack = null, bool isSame = false)
		{
			LoadAndEnterScene(moduleName, args, (mod) =>
			{
				if (mod == null)
				{
					//LobbyUI.Ins.PopupShow(LobbyUI.Ins.GetPlatformText("LoadTimeOut"), LobbyUI.PopupType.YESNO, (LobbyUI.PopButton btn) =>
					//{
					//	if (btn == LobbyUI.PopButton.YES)
					//	{
					//		ResourceLoader.Ins.UnloadOnce(moduleName);
					//		ModuleLoading(moduleName, args, callBack, isSame);
					//	}
					//	else if (btn == LobbyUI.PopButton.NO)
					//	{
					//		ResourceLoader.Ins.UnloadOnce(moduleName);
					//		callBack?.Invoke(false);
					//	}
					//});
				}
				else
				{
					callBack?.Invoke(true);
				}
			}, isSame);
		}

		private IEnumerator CheckModuleIsReady()
		{
			while (!NextModuleIsReady())
			{
				yield return null;
			}

			ModuleEnter();
		}

		/// <summary>
		/// 直接进入下一个模块。里面不会判断模块是否加载完成。需要自己判断。
		/// </summary>
		private void ModuleEnter()
		{
			MB.Ins.StopCoroutine("CheckModuleIsReady");
			if (NextModule != null)
			{
				LastModule = ThisModule;
				LastModuleName = LastModule?.Name;
				ThisModule = NextModule;
				NextModule = null;
				var mn = GetThisModuleName();
				Debug.Log($"NextModuleEnter -> {mn}");
				try
				{
					LastModule?.Exit();
					AddTranslation(mn);
					ThisModule.Enter(_moduleArg);
					_moduleLoadCallback?.Invoke(ThisModule);
					OnEnterEvent?.Invoke(ThisModule);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					_moduleLoadCallback?.Invoke(null);
				}
			}
			else
			{
				_moduleLoadCallback?.Invoke(null);
			}

			_moduleLoadCallback = null;
		}

		#endregion Module load and enter

		/// <summary>
		/// 加载下一个模块。模块一次只能加载一个，并且只有切换后才能再次加载。
		/// </summary>
		public void NextModuleLoad(string moduleName, Action<Module> callback = null)
		{
			NextModuleLoad(moduleName, true, callback);
		}

		/// <summary>
		/// 加载下一个模块。模块一次只能加载一个，并且只有切换后才能再次加载。
		/// </summary>
		/// <param name="isShowLoading">是否显示加载指示</param>
		public void NextModuleLoad(string moduleName, bool isShowLoading, Action<Module> callback = null)
		{
			Debug.Log($"NextModuleLoad -> {moduleName}");
			if (_isModuleLoading)
			{
				return;
			}

			OnLoadEvent?.Invoke(moduleName);
			_isModuleLoading = true;
			_moduleLoadCallback = callback;
			_moduleLoader.LoadDLL(moduleName, (module) =>
			{
				if (module != null)
				{
					NextModule = module;
					module.OnLoad();
					module.IsShowLoading = isShowLoading;

					OnLoadedEvent?.Invoke(module);
				}
				else
				{
					//error
					LoadException(moduleName);
				}
			});
		}

		/// <summary>
		/// 下一个模块是否加载完成
		/// </summary>
		public bool NextModuleIsReady()
		{
			if (NextModule == null) return false;

			bool NMIR = false;
			try
			{
				NMIR = NextModule.IsReady();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return NMIR;
		}

		/// <summary>
		/// 直接进入下一个模块。里面不会判断模块是否加载完成。需要自己判断。
		/// </summary>
		public void NextModuleEnter()
		{
			MB.Ins.StopCoroutine("CheckReadyAndEnter");
			if (NextModule == null)
			{
				_moduleLoadCallback?.Invoke(null);
				return;
			}

			LastModule = ThisModule;
			LastModuleName = LastModule?.Name;
			ThisModule = NextModule;
			NextModule = null;

			if (ThisModule == null) return;

			var mn = GetThisModuleName();
			Debug.Log($"NextModuleEnter -> {mn}");
			try
			{
				_moduleLoadCallback?.Invoke(LastModule);
				LastModule?.Exit();
				AddTranslation(mn);
				ThisModule.Enter(_moduleArg);
				OnEnterEvent?.Invoke(ThisModule);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				_moduleLoadCallback?.Invoke(null);
			}
			_moduleLoadCallback = null;
		}

		/// <summary>
		/// 如果下一个模块准备完成，则进入
		/// </summary>
		public void NextModuleEnterWhenReady(object arg = null)
		{
			_moduleArg = DeepCopy(arg);
			MB.Ins.StartCoroutine(CheckReadyAndEnter());
		}

		/// <summary>
		/// 重新加载上一个模块，把上一个模块当作下一个模块。
		/// </summary>
		public void NextModuleLoadFromLast()
		{
			if (LastModuleName != null)
			{
				Debug.Log($"NextModuleLoadFromLast ( {LastModuleName} )");
			}
			else
			{
				Debug.Log($"NextModuleLoadFromLast ( null ), nothing happening.");
				return;
			}

			NextModuleLoad(LastModuleName);
		}

		/// <summary>
		/// 加载模块内指定的场景
		/// </summary>
		/// <param name="aSceneName">场景名称</param>
		/// <returns></returns>
		public Scene SceneLoad(string aSceneName)
		{
			if (ThisModule == null)
			{
				Debug.LogError($"SceneLoad {aSceneName} ThisModLdr or .Name is null.");
				return null;
			}

			var scene = _moduleLoader.GetSceneClass(ThisModule, aSceneName);
			if (scene == null)
			{
				Debug.LogError($"SceneLoad class {aSceneName} is null.");
				return null;
			}

			var sceneName = $"{aSceneName}_{ThisModule.Name}";
			if (App.IsEditor) ResourceLoader.Ins.AddScene2BuildSettings(ThisModule.Name, sceneName);

			SceneAsync = SceneManager.LoadSceneAsync(sceneName);
			if (SceneAsync == null)
			{
				Debug.LogError($"SceneLoad SceneAsync {sceneName} is null.");
				return null;
			}
			SceneAsync.allowSceneActivation = false;

			//场景加载 STEP 1/4
			sceneName = aSceneName;
			Debug.Log($"SceneLoad -> {sceneName}");
			MB.Ins.StartCoroutine(CheckSceneIsDone());

			return scene;
		}

		/// <summary>
		/// 判断模块内场景是否加载完毕
		/// </summary>
		/// <returns></returns>
		public bool SceneIsLoaded()
		{
			if (ThisModule == null) return false;
			if (SceneEntering) return true;
			if (SceneAsync == null) return false;

			//场景加载 STEP 2/4
			if (SceneAsync.progress >= 0.9f)
			{
				ThisModule?.SceneNextAwakeOnce();
				return true;
			}

			return false;
		}

		/// <summary>
		/// 进入模块内加载的场景。会显示Loading指示。
		/// </summary>
		public void SceneEnter()
		{
			SceneEnter(true);
		}

		/// <summary>
		/// 进入模块内加载的场景
		/// <paramref name="isShowLoading"> 是否显示Loading指示</paramref>
		/// </summary>
		public void SceneEnter(bool isShowLoading)
		{
			if (SceneEntering) return;

			//场景加载 STEP 3/4
			if (SceneAsync == null) return;
			SceneAsync.allowSceneActivation = true;

			Debug.Log($"SceneEnter : SceneEntering");
			SceneEntering = true;
		}

		/// <summary>
		/// 通知当前模块已经暂停。子模块的Module.Pause()函数会被调用。
		/// </summary>
		public void ModulePause()
		{
			if (ThisModule == null) return;
			try
			{
				ThisModule.Pause();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// 通知当前模块，已经恢复。子模块的Module.Resume()函数会被调用。
		/// </summary>
		public void ModuleResume()
		{
			if (ThisModule == null) return;
			try
			{
				ThisModule.Resume();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// 通知当前模块，即将退出。子模块的Module.Exit()函数会被调用。
		/// </summary>
		public void ModuleExit()
		{
			if (ThisModule == null) return;
			try
			{
				ThisModule.Exit();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		#region PRIVATE

		private AsyncOperation SceneAsync = null;
		private bool SceneEntering = false;

		private IEnumerator CheckSceneIsDone()
		{
			while (!SceneAsync.isDone)
			{
				yield return null;
			}

			//场景加载 STEP 4/4
			SceneEntering = false;
			SceneAsync = null;

			ThisModule?.OnSceneNextEntered();
			_isModuleLoading = false;
		}

		private IEnumerator CheckReadyAndEnter()
		{
			while (!NextModuleIsReady())
			{
				yield return null;
			}
			NextModuleEnter();
		}

		protected override void OnNew()
		{
			MB.Ins.StartCoroutine(Update());
		}

		private IEnumerator Update()
		{
			while (true)
			{
				_moduleLoader.Update();
				ThisModule?.Update();

				if (LastModule != null)
				{
					UnloadLastModule();
				}

				yield return null;
			}
		}

		private void UnloadLastModule()
		{
			LastModule.SceneThisExit();
			try
			{
				LastModule.OnDestroy();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			ResourceLoader.Instance.UnloadOnce(LastModule.Name);
			_moduleLoader.LiteModuleUnload(LastModule);
			LastModule = null;
		}

		private object DeepCopy(object obj)
		{
			if (obj == null)
				return null;

			if (obj.GetType().IsSerializable)
			{
				using (Stream objectStream = new MemoryStream())
				{
					IFormatter formatter = new BinaryFormatter();
					formatter.Serialize(objectStream, obj);
					objectStream.Seek(0, SeekOrigin.Begin);
					return formatter.Deserialize(objectStream);
				}
			}
			else
			{
				return obj;
			}
		}

		private void AddTranslation(string modName)
		{
			if (!LocalTextMgr.ContainsTranslation(modName))
			{
				LocalTextMgr.AddTranslationIni(modName, IniMgr.LoadModuleLanguageFile(modName, "ui"));
			}
		}

		#endregion PRIVATE
	}//class
}//namespace