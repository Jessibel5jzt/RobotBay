using UnityEngine;

namespace WestBay
{
	public class AssetRef
	{
		public AssetBundle AB = null;
		public int RefCount { get; private set; } = 0;
		public bool Keep { get; set; } = false;

		public bool NeedRemove()
		{ return RefCount <= 0; }

		public void IncRef()
		{ RefCount++; }

		public void DecRef()
		{ RefCount--; }

		public void ResetRef()
		{ RefCount = 0; }

		public AssetRef()
		{ }

		public AssetRef(string moduleName, string assetFilePath, string abFilePath, bool isKeep)
		{
			AssetBundleFilePath = abFilePath.ToLower();
			Keep = isKeep;

			AssetBundleFileFullPath = ResourceLoader.Ins.GetAssetBundleFullPath(moduleName, AssetBundleFilePath);
			AssetFilePath = ResourceLoader.Ins.GetAssetFilePath(moduleName, AssetBundleFilePath);
		}

		/// <summary>
		/// 资源相对路径
		/// </summary>
		public string AssetFilePath { get; private set; }

		/// <summary>
		/// 资源AB的相对路径
		/// </summary>
		public string AssetBundleFilePath { get; private set; }

		/// <summary>
		/// 资源AB的完整路径
		/// </summary>
		public string AssetBundleFileFullPath { get; private set; }

		/// <summary>
		/// 是否在加载中
		/// </summary>
		public bool IsLoading { get; set; }

		/// <summary>
		/// 资源对象
		/// </summary>
		public UnityEngine.Object AssetObject { get; set; }
	}//class ABRef
}