using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace WestBay
{
	/// <summary>
	///管理当前场景中所有加载的Module
	/// </summary>
	public class ModuleLoader
	{
		public ModuleLoader()
		{
		}

		/// <summary>
		/// 加载模块实例
		/// </summary>
		/// <param name="moduleName">模块名称</param>
		/// <param name="callback">回调方法</param>
		public void LoadDLL(string moduleName, Action<Module> callback)
		{
			string moduleClass = $"{moduleName}.{moduleName}";
			Module module = null;

			Type type = null;
			if (GetTypeFromDLL(moduleClass, ref type))
			{
				module = CreateInstance<Module>(type);
				callback(IsModuleValid(moduleName, module) ? module : null);
				return;
			}

			LoadDLLAsync(moduleName, moduleName, (module) =>
			{
				callback(IsModuleValid(moduleName, module) ? module : null);
			}).Forget();
		}

		/// <summary>
		/// 创建并返回场景类
		/// </summary>
		/// <param name="sceneName">场景类的名称</param>
		/// <returns>场景类</returns>
		public Scene GetSceneClass(Module module, string sceneName)
		{
			if (module == null) return null;
			Scene scene = null;
			string SceneClass = $"{module.Name}.{sceneName}";

			Type type = null;
			if (GetTypeFromDLL(SceneClass, ref type))
			{
				scene = CreateInstance<Scene>(type);
			}

			if (scene == null) return null;
			if (scene.Name != sceneName) return null;
			scene._set_Module(module);

			return scene;
		}

		/// <summary>
		/// 当前加载Module列表
		/// </summary>
		private readonly Dictionary<string, Module> _liteModules = new Dictionary<string, Module>();

		/// <summary>
		/// 删除的模块列表
		/// </summary>
		private readonly List<Module> _deleteModList = new List<Module>();

		/// <summary>
		/// 新加载的模块列表
		/// </summary>
		private readonly List<Module> _newModList = new List<Module>();

		/// <summary>
		/// 加载中模块列表
		/// </summary>
		private readonly List<string> _loadingModList = new List<string>();

		/// <summary>
		/// 获取模块实例
		/// </summary>
		/// <param name="moduleName">模块名称</param>
		/// <returns>模块实例</returns>
		public Module LiteModuleGet(string moduleName)
		{
			_liteModules.TryGetValue(moduleName, out Module mod);

			return mod;
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Update()
		{
			foreach (var LM in _liteModules.Values)
			{
				try
				{
					LM.Update();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			for (int i = 0; i < _deleteModList.Count; ++i)
			{
				var mod = _deleteModList[i];
				_liteModules.Remove(mod.Name);
			}

			for (int i = 0; i < _newModList.Count; ++i)
			{
				var mod = _newModList[i];
				_liteModules.Add(mod.Name, mod);
			}

			_deleteModList.Clear();
			_newModList.Clear();
		}

		/// <summary>
		/// 加载轻模块实例
		/// </summary>
		/// <param name="moduleName">模块名称</param>
		/// <param name="callback">回调方法</param>
		public void LoadDLLLite(string moduleName, Action<Module> callback)
		{
			if (IsModLoad(moduleName)) return;

			if (_liteModules.ContainsKey(moduleName))
			{
				Debug.LogWarning($"LoadDLL {moduleName}.dll is already loading / loaded.");
				return;
			}

			_loadingModList.Add(moduleName);
			LoadDLL(moduleName, (mod) =>
			{
				if (mod != null)
				{
					_newModList.Add(mod);
				}

				_loadingModList.Remove(moduleName);
				callback(mod);
			});
		}

		/// <summary>
		/// 判断模块是否在加载中
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public bool IsModLoad(string moduleName)
		{
			foreach (var mod in _newModList)
			{
				if (mod.Name == moduleName) return true;
			}

			if (_liteModules.ContainsKey(moduleName)) return true;
			if (_loadingModList.Contains(moduleName)) return true;

			return false;
		}

		/// <summary>
		/// 移除模块
		/// </summary>
		/// <param name="module">模块实例</param>
		public void LiteModuleUnload(Module module)
		{
			if (_liteModules.ContainsValue(module))
			{
				_deleteModList.Add(module);
			}
			else
			{
				if (_newModList.Contains(module)) _newModList.Remove(module);
				if (_loadingModList.Contains(module.Name)) _loadingModList.Remove(module.Name);
			}
		}

		private async UniTaskVoid LoadDLLAsync(string moduleName, string dllName, Action<Module> callbakc)
		{
			string dllAB = $"codes/{dllName}_bytes_{moduleName}.ab";

			byte[] dllBytes = null;
			await ResourceLoader.LoadABBytesAsync(moduleName, dllAB, dllName, (bytes) =>
			{
				if (bytes == null)
				{
					Debug.LogError($"LoadDLL error : {dllAB}");
					callbakc(null);
				}
				else
				{
					dllBytes = bytes;
				}
			});

			if (dllBytes != null)
			{
				var assembly = DllLoader.Instance.LoadModuleDll(dllName, dllBytes);
				var module = CreateInstance<Module>(assembly.GetType(dllName));
				callbakc?.Invoke(module);
			}
			else
			{
				callbakc?.Invoke(null);
			}
		}

		/// <summary>
		/// 模块是否有效
		/// </summary>
		/// <param name="moduleName">模块名称</param>
		/// <param name="module">模块实例</param>
		/// <returns>有效标识</returns>
		private bool IsModuleValid(string moduleName, Module module)
		{
			if (module == null)
			{
				Debug.LogError($"LoadDLL class {moduleName}.{moduleName} doesn't exist.");
				return false;
			}
			if (module.Name != moduleName)
			{
				Debug.LogError($"LoadDLL Module name is deffrent, expecting {moduleName} but got {module.Name}");
				return false;
			}

			return true;
		}

		#region EDITOR

		private T CreateInstance<T>(Type type) where T : class
		{
			T obj = Activator.CreateInstance(type) as T;
			if (obj == null)
			{
				Debug.LogWarning($"LoadDLL Cant create instance : {type.Name}");
				return null;
			}

			return obj;
		}

		/// <summary>
		/// 获取类型
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		private bool GetTypeFromDLL(string typeName, ref Type type)
		{
			bool result = true;
			type = Type.GetType(typeName);
			if (type == null)
			{
				type = GetTypeFromFile(typeName);
				if (type == null)
				{
					result = false;
					Debug.LogWarning($"LoadDLL Cant find type : {typeName}");
				}
			}

			return result;
		}

		/// <summary>
		/// 从当前项目程序集中获取类型
		/// </summary>
		/// <param name="nameClass"></param>
		/// <returns></returns>
		private Type GetTypeFromFile(string nameClass)
		{
			string assetsPath = UnityEngine.Application.dataPath;
			string projPath = assetsPath.Substring(0, assetsPath.LastIndexOf('/'));
			string dllPath = $"{projPath}/Library/ScriptAssemblies/FFTAI.RUPS.dll";
			System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFile(dllPath);

			return asm.GetType(nameClass);
		}

		#endregion EDITOR
	}//class
}//namespace