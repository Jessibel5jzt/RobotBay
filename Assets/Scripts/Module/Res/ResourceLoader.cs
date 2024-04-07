using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 模块资源管理
	/// </summary>
	public class ResourceLoader : Singleton<ResourceLoader>
	{
		/// <summary>
		/// 开始服务
		/// </summary>
		public void StartMgr()
		{
			InitConfig();

			MB.Ins.StartCoroutine(RemvingLoader());
			PreloadAllModule();
		}

		#region INNER FUNCTIONS

		private readonly object lockObj = new object();

		private IEnumerator RemvingLoader()
		{
			while (true)
			{
				yield return new WaitForSeconds(60);

				lock (lockObj)
				{
					foreach (var pair in _modResDict)
					{
						var ResLdr = pair.Value;
						//Loader里还有资源，就留着。
						if (ResLdr.Has()) continue;
						_modResDict.Remove(pair.Key);
						Log($"ModuleLoader {pair.Key} was removed.");
						break;
					}
				}
			}
		}

		private Dictionary<string, ModResLoader> _modResDict = new Dictionary<string, ModResLoader>();

		private ModResLoader GetModResLoader(string modName, bool isKeep = false)
		{
			if (!_modResDict.TryGetValue(modName, out ModResLoader result))
			{
				//lock (_modResDict)
				{
					result = new ModResLoader(modName, isKeep);
					_modResDict.Add(modName, result);
				}
			}

			return result;
		}

		#endregion INNER FUNCTIONS

		#region Preload Module Resource

		/// <summary>
		/// 预加载模块名列表
		/// </summary>
		private readonly List<ModResLoader> _preloadModList = new List<ModResLoader>();

		private const string Preload_Config_Key = "Resource_Preload";

		/// <summary>
		/// Preload all module AB from config file
		/// </summary>
		private void PreloadAllModule()
		{
			//skip in editor
			if (Application.isEditor) return;

			_preloadModList.Clear();

			List<string> moduleNameList = new List<string>();
			GetPreloadModule(moduleNameList, IniMgr.Config.GetValue(Preload_Config_Key));

			for (var i = 0; i < moduleNameList.Count; ++i)
			{
				var moduleName = moduleNameList[i];
				PreloadModule(moduleName, false);
			}

			PreloadModule(0);
		}

		private void GetPreloadModule(List<string> moduleList, string configValue, string modPrefix = "")
		{
			var moduleArray = configValue.Split(',');
			AddPreloadModule(moduleList, moduleArray, modPrefix);
		}

		private void AddPreloadModule(List<string> moduleList, string[] moduleArray, string modPrefix)
		{
			for (var i = 0; i < moduleArray.Length; ++i)
			{
				var moduleName = $"{modPrefix}{moduleArray[i]}";
				moduleList.Add(moduleName);
			}
		}

		/// <summary>
		/// Load AB file from module forlder
		/// </summary>
		/// <param name="moduleName"></param>
		public void PreloadModule(string moduleName, bool isAutoLoading = true)
		{
			string persistPath = PathUtil.GetPersistPath();
			string modulePath = $"{persistPath}/{moduleName.ToLower()}";
			if (!Directory.Exists(modulePath)) return;

			var files = Directory.GetFiles(modulePath, "*.ab", SearchOption.AllDirectories);
			bool isLoad = false;
			var moduleLoader = GetModResLoader(moduleName, true);
			for (var j = 0; j < files.Length; ++j)
			{
				string filePath = files[j];
				if (!File.Exists(filePath)) continue;

				filePath = filePath.Replace('\\', '/').Substring(persistPath.Length + 1 + moduleName.Length + 1);
				string[] nameArray = filePath.Split('_');

				if (nameArray.Length > 1)
				{
					if (nameArray[1] == "wav" || nameArray[1] == "mp3") continue;

					//ab file about theme
					if (filePath.Contains(ThemeMgr.ThemeSplitMarker) && !nameArray[0].EndsWith(ThemeMgr.ThemeSplitMarker)) continue;
					//skip ab file in language forlder
					if (filePath.Contains("resources/language/")) continue;

					isLoad = true;
					moduleLoader.AddLoading(new AssetRef(moduleName, "", filePath, false));
				}
			}

			//Start Loading
			if (isLoad)
			{
				if (isAutoLoading) moduleLoader.StartLoading();
				if (!_preloadModList.Contains(moduleLoader))
				{
					_preloadModList.Add(moduleLoader);
				}
			}
		}

		/// <summary>
		/// 按顺序加载模块
		/// </summary>
		/// <param name="index"></param>
		private void PreloadModule(int index)
		{
			var count = _preloadModList.Count;
			if (count > 0 && index < count)
			{
				var loader = _preloadModList[index];
				loader.StartLoading(() =>
				{
					PreloadModule(++index);
				});
			}
		}

		/// <summary>
		/// 预加载进度
		/// </summary>
		public float PreloadProgress
		{
			get
			{
				if (_preloadModList.Count == 0) return 1;

				var total = 0;
				var current = 0.0f;
				for (int i = 0; i < _preloadModList.Count; ++i)
				{
					var moduleLoader = _preloadModList[i];
					total += moduleLoader.LoadTotal;

					current += moduleLoader.LoadedCount;
				}

				return current / total;
			}
		}

		#endregion Preload Module Resource

		#region Resource add/get

		/// <summary>
		/// 获取AssetBundle名称
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="fileName"></param>
		/// <param name="extension"></param>
		/// <returns></returns>
		private string GetAssetBundleName(string moduleName, string fileName, string extension)
		{
			return $"{fileName}_{extension}_{moduleName}.ab";
		}

		/// <summary>
		/// 获取AseetBundle路径
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt"></param>
		/// <param name="isNotDefault"></param>
		/// <returns></returns>
		internal string GetAssetBundlePath(string moduleName, string folderFileNameExt, bool isNotDefault)
		{
			FileHelper.GetNameExt(folderFileNameExt, out string fileName, out string extension);

			var abFileName = GetAssetBundleName(moduleName, fileName, extension);
			return GetAssetBundlePath(abFileName, isNotDefault);
		}

		/// <summary>
		/// 获取AssetBundle路径
		/// </summary>
		/// <param name="abFileName"></param>
		/// <param name="isNotDefault"></param>
		/// <returns></returns>
		private string GetAssetBundlePath(string abFileName, bool isNotDefault)
		{
			string result = $"resources/default/{abFileName}";
			if (isNotDefault)
			{
				result = $"resources/language/en/{abFileName}";
			}

			return result;
		}

		/// <summary>
		/// AssetBundle 实际路径
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="abFilePath"></param>
		/// <returns></returns>
		internal string GetAssetBundleFullPath(string modName, string abFilePath)
		{
			string themePath = ThemeMgr.TransPath(modName, LocalMgr.TransPath(modName, abFilePath));
			return PathUtil.GetPersistPath(modName.ToLower(), themePath);
		}

		/// <summary>
		/// 获取AssetFile路径
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="abFilePath"></param>
		/// <returns></returns>
		internal string GetAssetFilePath(string moduleName, string abFilePath)
		{
			FileHelper.GetNameExt(abFilePath, out string subPath, out string extension);
			string result;

			if (subPath.StartsWith("resources/")) subPath = subPath.Substring("resources/".Length);
			var array = subPath.Split('_');
			if (array.Length >= 3)
			{
				extension = array[array.Length - 2];
				result = subPath.Substring(0, subPath.Length - $"_{extension}_{moduleName}".Length);

				if (extension.Equals("unity") || extension.Equals("bytes"))
				{
					result = "";
				}
			}
			else
			{
				result = subPath;
				if (extension.Equals("meta")) result = "";
			}

			return result;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt">
		/// path start after Module/Resources/en/
		/// </param>
		public void ResourceAdd(string moduleName, string folderFileNameExt, bool isNotDefault, bool isKeep = false)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.AddLoading(folderFileNameExt, isNotDefault, isKeep);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileName">
		/// if file name is "userinfo.txt", then only pass "userinfo"
		/// path start after Module/Resources/en/texts/
		/// </param>
		public void TextAdd(string moduleName, string folderFileNameExt, bool isNotDefault = false)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.AddLoading(folderFileNameExt, isNotDefault, false);
		}

		/// <summary>
		/// 添加prefab
		/// </summary>
		/// <param name="folderFileName">
		/// if file name is "userinfo.prefab", then only pass "userinfo"
		/// path start after Module/Prefabs/
		/// </param>
		public void PrefabAdd(string moduleName, string folderFileName)
		{
			var modResLoader = GetModResLoader(moduleName);

			var abFileName = GetAssetBundleName(moduleName, $"prefabs/{folderFileName}", "prefab");
			string abPath = GetAssetBundlePath(abFileName, false);
			modResLoader.AddLoading(new AssetRef(moduleName, "", abPath, false));
		}

		/// <summary>
		/// 添加需要加载的场景
		/// </summary>
		/// <param name="folderFileName">
		/// if scene file name is "Town.unity" the only pass "Town"
		/// path start after Module/Scenes/
		/// </param>
		public void SceneAdd(string moduleName, string folderFileName)
		{
			var modResLoader = GetModResLoader(moduleName);

			var abFileName = GetAssetBundleName(moduleName, $"Scenes/{folderFileName}", "unity");
			modResLoader.AddLoading(new AssetRef(moduleName, "", abFileName, false));
		}

		/// <summary>
		/// 添加需要加载的目录，目录内所有资源都添加。
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="folderName">文件夹名</param>
		public void FolderAdd(string moduleName, string folderName)
		{
			FolderAdd(moduleName, folderName, null);
		}

		/// <summary>
		/// 添加需要加载的目录，并排除不需要加载的目录，目录内所有资源都添加。
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="folderName">文件夹名</param>
		/// <param name="exceptFolders">过滤的文件夹名字数组</param>
		/// <param name="isFuzzy">是否模糊查询</param>
		public void FolderAdd(string moduleName, string folderName, string[] exceptFolders, bool isFuzzy)
		{
			FolderAdd(moduleName, folderName, null, exceptFolders, isFuzzy);
		}

		/// <summary>
		/// 添加需要加载的目录，目录内所有资源都添加，除了exceptPostfix。
		/// </summary>
		/// <param name="moduleName">模块名</param>
		/// <param name="folderName">文件夹名</param>
		/// <param name="exceptPostfix">["mp3", "ogg"]</param>
		public void FolderAdd(string moduleName, string folderName, string[] exceptPostfix, string[] exceptFolders = null, bool isFuzzy = true)
		{
			string modulePath = PathUtil.GetPersistPath(moduleName);
			string moduleFolderPath = $"{modulePath}/{folderName}";

			string[] filePaths = Directory.GetFiles(moduleFolderPath, "*.ab");
			if (App.IsEditor)
			{
				List<string> list = new List<string>();
				filePaths = Directory.GetFiles(moduleFolderPath);
				for (int i = 0; i < filePaths.Length; ++i)
				{
					var path = filePaths[i].Replace('\\', '/');
					var extension = Path.GetExtension(path);
					if (extension.Equals(".meta")) continue;

					var newPath = $"{path.Substring(0, path.LastIndexOf('/'))}/{Path.GetFileNameWithoutExtension(path)}_{extension.Substring(1)}_{moduleName}.ab";
					list.Add(newPath);
				}

				filePaths = list.ToArray();
			}

			List<string> exceptList = new List<string>();
			if (exceptPostfix != null)
			{
				foreach (var except in exceptPostfix)
				{
					exceptList.Add($"_{except}_{moduleName}.ab".ToLower());
				}
			}

			foreach (string path in filePaths)
			{
				var pathRel = FileHelper.NormalizePath(path.Substring(modulePath.Length + 1));
				if (exceptList.Count > 0)
				{
					bool isSkip = false;
					foreach (var except in exceptList)
					{
						if (path.EndsWith(except))
						{
							isSkip = true;
							break;
						}
					}

					if (isSkip) continue;
				}

				if (pathRel.Contains(ThemeMgr.ThemeSplitMarker))
				{
					if (ThemeMgr.CurrentTheme == "light")
					{
						if (!pathRel.Contains($"{ThemeMgr.ThemeSplitMarker}_")) continue;
					}
					else
					{
						if (!pathRel.Contains($"{ThemeMgr.ThemeSplitMarker}{ThemeMgr.CurrentTheme}")) continue;
					}

					int sharpIdx = pathRel.LastIndexOf(ThemeMgr.ThemeSplitMarker);
					string tempStr = pathRel;
					for (int i = sharpIdx + ThemeMgr.ThemeSplitMarker.Length; i < pathRel.Length; i++)
					{
						if (pathRel[i].ToString().CompareTo("_") == 0) break;
						tempStr = tempStr.Remove(sharpIdx + ThemeMgr.ThemeSplitMarker.Length, 1);
					}

					pathRel = tempStr;
				}

				var modResLoader = GetModResLoader(moduleName);
				modResLoader.AddLoading(new AssetRef(moduleName, "", pathRel, false));
			}

			string[] dirs = Directory.GetDirectories(moduleFolderPath);
			foreach (string aDir in dirs)
			{
				var dir = aDir.Substring(modulePath.Length + 1);
				if (CheckContain(folderName.Replace('\\', '/'), dir.Replace('\\', '/'), exceptFolders, isFuzzy))
				{
					continue;
				}

				FolderAdd(moduleName, dir, exceptPostfix, exceptFolders, isFuzzy);
			}
		}

		/// <summary>
		/// 判断是否要过滤文件夹
		/// </summary>
		/// <param name="folderName">前缀</param>
		/// <param name="checkSource">检查的路径</param>
		/// <param name="exceptFolders">过滤的文件夹名字数组</param>
		/// <param name="isFuzzy">是否模糊查询</param>
		/// <returns></returns>
		private bool CheckContain(string folderName, string checkSource, string[] exceptFolders, bool isFuzzy)
		{
			var contain = false;
			if (exceptFolders == null || exceptFolders.Length <= 0)
			{
				return contain;
			}

			foreach (var folder in exceptFolders)
			{
				contain = isFuzzy ? checkSource.Contains(folder) : checkSource.Equals(Path.Combine(folderName, folder).Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
				if (contain)
				{
					break;
				}
			}

			return contain;
		}

		public UnityEngine.Object ResourceGet(string moduleName, string folderFileNameExt)
		{
			var result = LoadAsset<UnityEngine.Object>(moduleName, folderFileNameExt, folderFileNameExt.Contains("/language/"));
			if (result == null)
			{
				Debug.LogError($"Cant find Resource {folderFileNameExt}");
			}

			return result;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt">
		/// path start after Module/Resources/en/
		/// </param>
		/// <returns></returns>
		public Sprite[] SpritesGet(string moduleName, string folderFileNameExt, bool isNotDefault = false)
		{
			var result = LoadAllAssets<Sprite>(moduleName, folderFileNameExt, isNotDefault);
			if (result == null)
			{
				Debug.LogError($"Cant find Sprites {folderFileNameExt}");
			}

			return result;
		}

		public RuntimeAnimatorController AnimatorGet(string moduleName, string folderFileNameExt, string controllerName)
		{
			var result = LoadAsset<RuntimeAnimatorController>(moduleName, $"{folderFileNameExt}/{controllerName}", false);
			if (result == null)
			{
				Debug.LogError($"Cant find Animator {folderFileNameExt}/{controllerName}");
			}

			return result;
		}

		public Material MaterialGet(string moduleName, string folderFileNameExt, string materialName)
		{
			var result = LoadAsset<Material>(moduleName, folderFileNameExt, false);
			if (result == null)
			{
				Debug.LogError($"Cant find Material {folderFileNameExt}");
			}

			return result;
		}

		/// <summary>
		/// 获得animation
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt"></param>
		/// <param name="animationName"></param>
		/// <returns></returns>
		public AnimationClip AnimationGet(string moduleName, string folderFileNameExt, string animationName)
		{
			var result = LoadAsset<AnimationClip>(moduleName, folderFileNameExt, false);
			if (result == null)
			{
				Debug.LogError($"Can't find Animation {folderFileNameExt}/{animationName}");
			}

			return result;
		}

		/// <summary>
		/// 获得Audio
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt"></param>
		/// <param name="audioName"></param>
		/// <returns></returns>
		public AudioClip AudioClipGet(string moduleName, string folderFileNameExt, string audioName)
		{
			var result = LoadAsset<AudioClip>(moduleName, folderFileNameExt, false);
			if (result == null)
			{
				Debug.LogError($"can't find AudioClip {folderFileNameExt}");
			}

			return result;
		}

		public Font FontGet(string moduleName, string folderFileNameExt, string fontName)
		{
			var result = LoadAsset<Font>(moduleName, folderFileNameExt, false);
			if (result == null)
			{
				Debug.LogError($"can't find Font {folderFileNameExt} in {moduleName}");
			}

			return result;
		}

		/// <summary>
		/// 文本txt获取
		/// </summary>
		/// <param name="folderFileName">
		/// if file name is "userinfo.txt", then only pass "userinfo"
		/// path start after Module/Resources/en/texts/
		/// </param>
		public string TextGet(string moduleName, string folderFileNameExt, bool isNotDefault = false)
		{
			string result = null;
			var textAsset = LoadAsset<TextAsset>(moduleName, folderFileNameExt, isNotDefault);
			if (textAsset != null)
			{
				result = textAsset.text;
			}
			else
			{
				Debug.LogError($"Cant find Text {folderFileNameExt}.txt");
			}

			return result;
		}

		/// <summary>
		/// 配置文本获取
		/// </summary>
		/// <param name="folderFileName">
		/// if file name is "userinfo.ini", then only pass "userinfo"
		/// path start after Module/Resources/en/texts/
		/// </param>
		public IniFile IniGet(string moduleName, string folderFileName, bool isNotDefault = false)
		{
			IniFile result = new IniFile();

			var textAsset = LoadAsset<TextAsset>(moduleName, $"texts/{folderFileName}", isNotDefault);
			if (textAsset != null)
			{
				result.SetBuffer(textAsset.text);
			}
			else
			{
				Debug.LogError($"Cant find Ini {folderFileName}.ini");
			}

			return result;
		}

		/// <summary>
		/// 获取已加载的prefab
		/// </summary>
		/// <param name="pbName">
		/// if file name is "userinfo.prefab", then only pass "userinfo"
		/// path start after Module/Prefabs/
		/// </param>
		public UnityEngine.Object PrefabGet(string moduleName, string folderFileName)
		{
			var result = LoadAsset<UnityEngine.Object>(moduleName, $"prefabs/{folderFileName}.prefab", false);
			if (result == null)
			{
				Debug.LogError($"can't find prefab {folderFileName} in {moduleName}");
			}

			return result;
		}

		public Shader ShaderGet(string moduleName, string folderFileName)
		{
			var result = LoadAsset<Shader>(moduleName, folderFileName, false);
			if (result == null)
			{
				Debug.LogError($"can't find shader {folderFileName} in {moduleName}");
			}

			return result;
		}

		/// <summary>
		/// 获取文件配置内容
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileName"></param>
		/// <returns></returns>
		public string GetConf(string moduleName, string folderFileName)
		{
			string fileFullPath = $"/{folderFileName}";
			if (App.IsEditor)
			{
				fileFullPath = $"{Application.dataPath}/Mod_{moduleName.ToLower()}{fileFullPath}";
			}
			else
			{
				fileFullPath = $"{PathUtil.GetPersistPath()}/{moduleName.ToLower()}{fileFullPath}";
			}

			return FileHelper.ReadFile(fileFullPath);
		}

		public async void GetAudioClip(string moduleName, string folderFileNameExt, Action<AudioClip> callback)
		{
			var abPath = GetAssetBundlePath(moduleName, folderFileNameExt, false);
			var assetFilePath = GetAssetFilePath(moduleName, abPath);
			if (App.IsEditor)
			{
				var clip = Resources.Load<AudioClip>(assetFilePath);
				callback?.Invoke(clip);
			}
			else
			{
				var fileFullPath = GetAssetBundleFullPath(moduleName, abPath);
				await LoadABAsync(fileFullPath, (ab) =>
				{
					if (ab != null)
					{
						callback?.Invoke(ab.LoadAsset<AudioClip>(Path.GetFileName(folderFileNameExt)));
					}
					else
					{
						callback?.Invoke(null);
					}
				});
			}
		}

		#endregion Resource add/get

		/// <summary>
		/// 开始加载
		/// </summary>
		/// <param name="moduleName"></param>
		public void StartLoading(string moduleName)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.StartLoading();
		}

		/// <summary>
		/// 是否加载完成
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public bool IsLoaded(string moduleName)
		{
			var modResLoader = GetModResLoader(moduleName, true);
			return modResLoader.IsLoaded();
		}

		/// <summary>
		/// 模块加载进度
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public float LoadingProgress(string moduleName)
		{
			float result = 0;
			var modResLoader = GetModResLoader(moduleName, true);
			if (modResLoader != null)
			{
				result = modResLoader.LoadProgress;
			}

			return result;
		}

		/// <summary>
		/// 强制卸载资源（协程）
		/// </summary>
		public void Unload(string moduleName)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.RemoveAll();
		}

		/// <summary>
		/// 一帧之内全部清理
		/// </summary>
		public void UnloadOnce(string moduleName)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.RemoveAllOnce(_unloadDelay);
		}

		/// <summary>
		/// 获取AB
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="folderFileNameExt"></param>
		/// <returns></returns>
		private AssetBundle Get(string moduleName, string folderFileNameExt)
		{
			var modResLoader = GetModResLoader(moduleName);
			return modResLoader.Get(folderFileNameExt);
		}

		private AssetRef GetAsset(string moduleName, string folderFileNameExt)
		{
			var modResLoader = GetModResLoader(moduleName);
			return modResLoader.GetAsset(folderFileNameExt);
		}

		private T LoadAsset<T>(string moduleName, string folderFileNameExt, bool isNotDefault = false) where T : UnityEngine.Object
		{
			var abPath = GetAssetBundlePath(moduleName, folderFileNameExt, isNotDefault);
			var assetRef = GetAsset(moduleName, abPath);
			if (assetRef == null)
			{
				return null;
			}

			if (App.IsEditor)
			{
				return (T)assetRef.AssetObject;
			}
			else
			{
				return assetRef.AB.LoadAsset<T>(Path.GetFileName(folderFileNameExt));
			}
		}

		private T[] LoadAllAssets<T>(string moduleName, string folderFileNameExt, bool isNotDefault = false) where T : UnityEngine.Object
		{
			var abPath = GetAssetBundlePath(moduleName, folderFileNameExt, isNotDefault);
			var assetRef = GetAsset(moduleName, abPath);
			if (assetRef == null)
			{
				return null;
			}

			if (App.IsEditor)
			{
				return Resources.LoadAll<T>(assetRef.AssetFilePath);
			}
			else
			{
				return assetRef.AB.LoadAllAssets<T>();
			}
		}

		/// <summary>
		/// 添加场景到BuildSettings
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="scenePath"></param>
		/// <param name="sceneName"></param>
		internal bool AddScene2BuildSettings(string moduleName, string sceneName)
		{
			bool result = false;
#if UNITY_EDITOR
			var scenes = UnityEditor.EditorBuildSettings.scenes;
			List<UnityEditor.EditorBuildSettingsScene> settingSceneList = new List<UnityEditor.EditorBuildSettingsScene>();
			bool isContain = false;

			foreach (var scene in scenes)
			{
				if (string.IsNullOrEmpty(scene.path)) continue;
				if (scene.path.Contains(sceneName))
				{
					isContain = true;
					scene.enabled = true;
				}
				settingSceneList.Add(scene);
			}

			if (!isContain)
			{
				string sceneFullPath = $"Assets/Mod_{moduleName}/Scenes/{sceneName}.unity";
				var settingsScene = new UnityEditor.EditorBuildSettingsScene(sceneFullPath, true);
				settingSceneList.Add(settingsScene);

				result = true;
			}

			UnityEditor.EditorBuildSettings.scenes = settingSceneList.ToArray();
#endif
			return result;
		}

		/// <summary>
		/// 保留资源
		/// </summary>
		public void Keep(string moduleName, string folderFileNameExt = null)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.Keep(folderFileNameExt);
		}

		/// <summary>
		/// 取消保留资源
		/// </summary>
		/// <param name="folderFileNameExt">全部应用，留空。否则指定资源名称</param>
		public void Unkeep(string moduleName, string folderFileNameExt = null)
		{
			var modResLoader = GetModResLoader(moduleName);
			modResLoader.Unkeep(folderFileNameExt);
		}

		#region AssetBundle 加载

		#region 协程加载

		/// <summary>
		/// 加载AB
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="fullPath"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static IEnumerator LoadAB(string modName, string fullPath, Action<AssetBundle> callback)
		{
			if (!File.Exists(fullPath))
			{
				callback(null);
				yield break;
			}

			AssetBundle assetBundle = null;
			try
			{
				assetBundle = AssetBundle.LoadFromFile(fullPath);
				if (assetBundle == null)
				{
					Debug.LogError($"{modName} can't load {fullPath}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"{modName}ResourceLoader:{e}");
			}

			yield return assetBundle;

			callback(assetBundle);
		}

		public static IEnumerator LoadAsset(string modName, string fullPath, Action<UnityEngine.Object> callback)
		{
			UnityEngine.Object result = null;
			if (string.IsNullOrWhiteSpace(fullPath))
			{
				callback(result);
				yield break;
			}

			try
			{
				result = Resources.Load(fullPath);
				if (result == null)
				{
					Debug.LogError($"{modName} can't load {fullPath}");
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"{modName}ResourceLoader:{e}");
			}

			yield return result;

			callback(result);
		}

		#endregion 协程加载

		#region 异步加载

		/// <summary>
		/// 加载Code Bytes
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="subPathABFullName"></param>
		/// <param name="assetName"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask LoadABBytesAsync(string modName, string subPathABFullName, string assetName, Action<byte[]> callback)
		{
			var ab = ResourceLoader.Ins.Get(modName, subPathABFullName);
			if (ab != null)
			{
				byte[] bytes = ab.LoadAsset<TextAsset>($"{assetName}.bytes").bytes;
				callback(bytes);
			}
			else
			{
				string fullPath = ResourceLoader.Ins.GetAssetBundleFullPath(modName, subPathABFullName);
				await LoadABAsync(fullPath, (result) =>
				{
					if (result != null)
					{
						byte[] bytes = result.LoadAsset<TextAsset>($"{assetName}.bytes").bytes;
						result.Unload(true);

						callback(bytes);
					}
					else
					{
						callback(null);
					}
				});
			}
		}

		/// <summary>
		/// 异步加载AB
		/// </summary>
		/// <param name="modName"></param>
		/// <param name="subPathABFullName"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask LoadABAsync(string fullPath, Action<AssetBundle> callback)
		{
			AssetBundleCreateRequest abRequest = null;
			try
			{
				abRequest = AssetBundle.LoadFromFileAsync(fullPath);
				while (!abRequest.isDone)
				{
					await UniTask.Delay(10);
				}

				callback(abRequest?.assetBundle);
			}
			catch (Exception e)
			{
				Debug.LogError($"ResourceLoader:{e.ToString()}");
				if (abRequest != null && abRequest.assetBundle != null)
				{
					abRequest.assetBundle.Unload(true);
				}

				callback(null);
			}
		}

		#endregion 异步加载

		#endregion AssetBundle 加载

		/// <summary>
		/// Log
		/// </summary>
		/// <param name="log"></param>
		public static void Log(string log)
		{
			if (IniMgr.Config.GetValue("Resource_Log").Equals("1"))
			{
				Debug.Log(log);
			}
		}

		/// <summary>
		/// 卸载延迟时间
		/// </summary>
		private int _unloadDelay = 5;

		/// <summary>
		/// 初始化配置
		/// </summary>
		private void InitConfig()
		{
			string value = IniMgr.Config.GetValue("Resource_Unload_Delay");
			if (!string.IsNullOrEmpty(value))
			{
				_unloadDelay = int.Parse(value);
			}
		}
	}//class ResourceMgr
}//namespace