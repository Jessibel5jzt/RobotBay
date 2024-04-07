//using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace WestBay
{
	public class DllLoader : Singleton<DllLoader>
	{
		private static Dictionary<string, byte[]> _assetDatas = new Dictionary<string, byte[]>();

		/// <summary>
		/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
		/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
		/// </summary>
		public void LoadMetadataForAOTAssemblies(string aotDllName, byte[] dllBytes)
		{
			/// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
			/// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
			///

			//HomologousImageMode mode = HomologousImageMode.SuperSet;
			//LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
			//Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");

			// 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
		}

		public Assembly LoadModuleDll(string dllName, byte[] dllBytes)
		{
			Assembly assembly;
			LoadMetadataForAOTAssemblies(dllName, dllBytes);
#if !UNITY_EDITOR
			assembly = Assembly.Load(dllBytes);
#else
			assembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dllName);
#endif
			return assembly;
		}
	}
}