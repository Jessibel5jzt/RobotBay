using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WestBay
{
	public class ModResLoader
	{
		public bool IsKeep { get; private set; }

		public ModResLoader(string modName)
		{
			_modName = modName;
			IsKeep = false;
		}

		public ModResLoader(string modName, bool isKeep)
		{
			_modName = modName;
			IsKeep = isKeep;
		}

		public bool Has()
		{ return _assets.Count > 0; }

		public int Remains()
		{ return _assets.Count; }

		public bool IsRemoving()
		{ return _removeStatus == RemoveStatus.Removing; }

		/// <summary>
		/// 根据路径获取AB
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public AssetBundle Get(string path)
		{
			path = path.ToLower();
			if (!_assets.ContainsKey(path)) return null;

			var abRef = _assets[path];
			if (abRef.NeedRemove()) return null;

			return abRef.AB;
		}

		public AssetRef GetAsset(string path)
		{
			path = path.ToLower();
			_assets.TryGetValue(path, out AssetRef result);

			return result;
		}

		/// <summary>
		/// 留着，不要清理这个资源
		/// </summary>
		/// <param name="path">null则全部</param>
		public void Keep(string path)
		{
			if (path == null)
			{
				foreach (var Pair in _assets)
				{
					Pair.Value.Keep = true;
				}
			}
			else
			{
				if (!_assets.ContainsKey(path)) return;

				var abRef = _assets[path];
				abRef.Keep = true;
			}
		}

		/// <summary>
		/// 不用保留这个资源
		/// </summary>
		/// <param name="path">全部应用，留空。否则指定资源名称</param>
		public void Unkeep(string path = null)
		{
			if (string.IsNullOrEmpty(path))
			{
				foreach (var Pair in _assets)
				{
					Pair.Value.Keep = false;
				}
			}
			else
			{
				if (!_assets.ContainsKey(path)) return;

				var abRef = _assets[path];
				abRef.Keep = false;
			}
		}

		public void AddLoading(string folderFileNameExt, bool isNotDefault = false, bool isKeep = false)
		{
			if (!isKeep) isKeep = this.IsKeep;
			string abPath = ResourceLoader.Ins.GetAssetBundlePath(_modName, folderFileNameExt, isNotDefault);
			AddLoading(new AssetRef(_modName, folderFileNameExt, abPath, isKeep));
		}

		public void AddLoading(AssetRef assetRef)
		{
			var path = assetRef.AssetBundleFilePath;
			var isKeep = assetRef.Keep;
			if (_assets.TryGetValue(path, out AssetRef abRef))
			{
				abRef.IncRef();
				ResourceLoader.Log($"[{_modName}] LOADED :{path} (cnt:{abRef.RefCount})");
			}
			else
			{
				//加载队列
				_loaderQueue.Enqueue(assetRef);
				ResourceLoader.Log($"[{_modName}] WILL LOADING : {path} isKeep:{isKeep} (idx:{_loaderQueue.Count})");
			}

			_loadTotal = _loaderQueue.Count;
		}

		/// <summary>
		/// 是否加载完成
		/// </summary>
		/// <returns></returns>
		public bool IsLoaded()
		{ return _loaderQueue.Count == 0; }

		/// <summary>
		/// 加载进度
		/// </summary>
		public float LoadProgress
		{
			get
			{
				float result;
				if (_loaderQueue.Count == 0)
				{
					result = 0;
				}
				else
				{
					result = 1 - (float)_loaderQueue.Count / _loadTotal;
				}
				return result;
			}
		}

		/// <summary>
		/// 开始加载所有资源
		/// </summary>
		public void StartLoading(Action onComplete = null)
		{
			StopRemoving();
			StartLoadingAll(onComplete);
		}

		/// <summary>
		/// 停掉卸载线程
		/// </summary>
		private void StopRemoving()
		{
			_removeStatus = RemoveStatus.NotStarted;
			if (_removeCoroutine != null)
			{
				MB.Ins.StopCoroutine(_removeCoroutine);
				_removeCoroutine = null;
			}
		}

		public async UniTaskVoid LoadSprites(string folderFileNameExt, bool isDefault, Action<Sprite[]> callback)
		{
			string abPath = ResourceLoader.Ins.GetAssetBundlePath(_modName, folderFileNameExt, isDefault);
			if (App.IsEditor)
			{
				string assetFilePath = ResourceLoader.Ins.GetAssetFilePath(_modName, abPath);
				callback?.Invoke(Resources.LoadAll<Sprite>(assetFilePath));
			}
			else
			{
				string abFullPath = ResourceLoader.Ins.GetAssetBundleFullPath(_modName, abPath).ToLower();
				if (!File.Exists(abFullPath))
				{
					callback?.Invoke(null);
				}
				else
				{
					await ResourceLoader.LoadABAsync(abFullPath, (ab) =>
					{
						if (ab != null)
						{
							var assetRef = new AssetRef
							{
								AB = ab
							};
							_assets.Add(abPath, assetRef);
							callback?.Invoke(ab.LoadAllAssets<Sprite>());
						}
						else
						{
							callback?.Invoke(null);
						}
					});
				}
			}
		}

		public async UniTaskVoid LoadSprite(string folderFileNameExt, bool isDefault, Action<Sprite> callback)
		{
			string abPath = ResourceLoader.Ins.GetAssetBundlePath(_modName, folderFileNameExt, isDefault);
			if (App.IsEditor)
			{
				string assetFilePath = ResourceLoader.Ins.GetAssetFilePath(_modName, abPath);
				UnityEngine.Object obj = Resources.Load(assetFilePath, typeof(Sprite));
				if (obj != null)
				{
					var sprite = GameObject.Instantiate(obj) as Sprite;
					callback?.Invoke(sprite);
				}
				else
				{
					callback?.Invoke(null);
				}
			}
			else
			{
				string abFullPath = ResourceLoader.Ins.GetAssetBundleFullPath(_modName, abPath);
				if (!File.Exists(abFullPath))
				{
					callback?.Invoke(null);
				}
				else
				{
					await ResourceLoader.LoadABAsync(abFullPath, (ab) =>
					{
						if (ab != null)
						{
							var assetRef = new AssetRef
							{
								AB = ab
							};
							_assets.Add(abPath, assetRef);
							callback?.Invoke(ab.LoadAsset<Sprite>(Path.GetFileName(folderFileNameExt)));
						}
						else
						{
							callback?.Invoke(null);
						}
					});
				}
			}
		}

		#region AssetBundle 卸载

		private Coroutine _removeCoroutine = null;
		private RemoveStatus _removeStatus = RemoveStatus.NotStarted;

		private enum RemoveStatus
		{
			NotStarted, //Added, but not remove yet
			Removing, //removing... can not add during removing.
			DelayRemove,
		}

		/// <summary>
		/// 强制彻底卸载。
		/// 包括所有非Keep的资源。
		/// 在卸载协程中，按计划，分布缓慢卸载
		/// </summary>
		public void RemoveAll()
		{
			bool isNeed = false;
			foreach (var Pair in _assets)
			{
				if (!Pair.Value.Keep)
				{
					isNeed = true;
					Pair.Value.ResetRef();
					ResourceLoader.Log($"[{_modName}] WILL REMOVING : {Pair.Key}");
				}
			}

			if (isNeed)
			{
				RemovingAllStart();
			}
		}

		private void RemovingAllStart()
		{
			if (_removeCoroutine != null) return;

			_removeCoroutine = MB.Ins.StartCoroutine(RemoveAllCould());
		}

		private const float Wait2Begin = 60 * 1 * 60; //10min后开始卸载
		private const float WaitEach = 3f;//每3s卸一个
		private const int GCTriggerCnt = 16;
		private static int RemovedCnt = 0;
		private readonly object lockObj = new object();

		private IEnumerator RemoveAllCould()
		{
			if (_removeStatus == RemoveStatus.Removing) yield break;
			_removeStatus = RemoveStatus.Removing;

			//Wait2Begin秒 后在开始卸载
			yield return new WaitForSeconds(Wait2Begin);

			while (true)
			{
				bool AllRemoved = true;

				lock (lockObj)
				{
					foreach (var Pair in _assets)
					{
						var abRef = Pair.Value;
						//if ((!Ac.Keep) && Ac.NeedRemove())
						if (!abRef.Keep)
						{
							if (_removeStatus == RemoveStatus.NotStarted) break;

							abRef.AB.Unload(false);
							_assets.Remove(Pair.Key);
							RemovedCnt++;
							ResourceLoader.Log($"[{_modName}] Removed : {Pair.Key} (idx:{RemovedCnt})");
							AllRemoved = false;
							//慢慢卸载，如果又再次进入此模块，则可以不着急
							yield return new WaitForSeconds(WaitEach);
							break;
						}
					}
				}

				if (RemovedCnt >= GCTriggerCnt)
				{
					RemovedCnt = 0;
					GC.Collect();
				}

				if (AllRemoved)
				{
					GC.Collect();
					_removeStatus = RemoveStatus.NotStarted;
					yield break;
				}

				yield return null;
			}//while
		}

		/// <summary>
		/// 强制彻底卸载。
		/// 包括所有非Keep的资源。
		/// 在函数返回前全部卸载，（也许）会造成卡顿。
		/// </summary>
		public void RemoveAllOnce(float delay)
		{
			StopRemoving();
			_removeCoroutine = MB.Ins.StartCoroutine(RemoveAllOnceDelay(delay));
		}

		private IEnumerator RemoveAllOnceDelay(float delay)
		{
			yield return new WaitForSeconds(delay);

			var newPathAB = new Dictionary<string, AssetRef>();
			if (_assets != null)
			{
				foreach (var Pair in _assets)
				{
					var abRef = Pair.Value;
					if (!abRef.Keep)
					{
						abRef.AB?.Unload(false);
						ResourceLoader.Log($"[{_modName}] Removed : {Pair.Key} ");
					}
					else
					{
						newPathAB.Add(Pair.Key, Pair.Value);
					}
				}
			}
			_assets = newPathAB;
			GC.Collect();
		}

		#endregion AssetBundle 卸载

		#region AssetBundle 加载

		private readonly string _modName;
		private Dictionary<string, AssetRef> _assets = new Dictionary<string, AssetRef>();
		private readonly Queue<AssetRef> _loaderQueue = new Queue<AssetRef>();
		private int _loadTotal = 0;
		public int LoadTotal => _loadTotal;
		public int LoadedCount => _loadTotal - _loaderQueue.Count;

		private void StartLoadingAll(Action onComplete)
		{
			MB.Ins.StartCoroutine(LoadAll(onComplete));
		}

		private IEnumerator LoadAll(Action onComplete)
		{
			while (_loaderQueue.Count > 0)
			{
				var assetRef = _loaderQueue.Peek();
				var path = assetRef.AssetBundleFilePath;
				if (_assets.TryGetValue(path, out AssetRef asset))
				{
					asset.IncRef();
					_loaderQueue.Dequeue();
					ResourceLoader.Log($"[{_modName}] LOADED :{path} (cnt:{asset.RefCount})");
					yield return null;
				}
				else
				{
					if (!assetRef.IsLoading)
					{
						assetRef.IsLoading = true;
						if (App.IsEditor)
						{
							yield return ResourceLoader.LoadAsset(_modName, assetRef.AssetFilePath, (obj) =>
							{
								if (obj != null)
								{
									assetRef.AssetObject = obj;
									assetRef.IsLoading = false;
									assetRef.IncRef();
									_assets.Add(path, assetRef);
								}

								_loaderQueue.Dequeue();
								// Log($"[{ModName}] LOADED :{Path} (1)");
							});
						}
						else
						{
							yield return ResourceLoader.LoadAB(_modName, assetRef.AssetBundleFileFullPath, (ab) =>
							{
								if (ab != null)
								{
									assetRef.AB = ab;
									assetRef.IsLoading = false;
									assetRef.IncRef();
									_assets.Add(path, assetRef);
								}

								_loaderQueue.Dequeue();
								//ResourceLoader.Log($"[{_modName}] LOADED :{path}");
							});
						}
					}
				}
			}
			onComplete?.Invoke();
		}

		#endregion AssetBundle 加载
	}//class ModResLdr
}