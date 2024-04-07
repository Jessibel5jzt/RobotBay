using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WestBay
{
	/// <summary>
	/// 为模块类提供基础功能
	/// </summary>
	public class ModuleMethod
	{
		/// <summary>
		/// 子类复写，实现子模块名字
		/// </summary>
		public virtual string Name
		{ get { return ""; } }

		/// <summary>
		/// 是否处于暂停状态
		/// </summary>
		public bool IsPaused { get; private set; }

		/// <summary>
		/// 暂停系统
		/// </summary>
		public void Pause()
		{
			IsPaused = true;
			OnPause();
			SceneThisPause();
		}

		/// <summary>
		/// 取消暂停
		/// </summary>
		public void Resume()
		{
			IsPaused = false;
			OnResume();
			SceneThisResume();
		}

		/// <summary>
		/// 请求退出
		/// </summary>
		public void Exit()
		{
			IsEntered = false;
			OnExit();
			SceneThisExit();
		}

		public void Enter(object arg)
		{
			OnEnter(arg);
			IsEntered = true;
		}

		private bool IsEntered = false;
		public bool IsShowLoading = true;

		public void Update()
		{
			if (!IsEntered) return;
			OnUpdate();
			SceneThisUpdate();
		}

		#region Resources Functions

		/// <summary>
		/// 资源加载
		/// </summary>
		/// <param name="pathName">
		/// 路径为 resources/en/ 之后的内容。
		/// 如：image/test.jpg
		/// </param>
		public void ResourceAdd(string pathName, bool isNotDefault = false, bool isKeep = false)
		{
			ResourceLoader.Ins.ResourceAdd(Name, pathName, isNotDefault, isKeep);
		}

		/// <summary>
		/// 资源获取
		/// </summary>
		/// <param name="path">
		/// 路径为 resources/en/ 之后的内容。
		/// 如：image/test.jpg
		/// </param>
		/// <returns></returns>
		public UnityEngine.Object ResourceGet(string pathName)
		{
			return ResourceLoader.Ins.ResourceGet(Name, pathName);
		}

		/// <summary>
		/// 返回当前模块图集
		/// </summary>
		/// <param name="pathName">
		/// 路径为 resources/en/ 之后的内容。
		/// 如：image/test.jpg
		/// </param>
		public Sprite[] SpritesGet(string pathName, bool NotDefault = false)
		{
			return ResourceLoader.Ins.SpritesGet(Name, pathName, NotDefault);
		}

		/// <summary>
		/// 返回当前模块动画
		/// </summary>
		/// <param name="pathName"></param>
		/// <returns></returns>
		public RuntimeAnimatorController AnimatorGet(string pathName, string controllerName)
		{
			return ResourceLoader.Ins.AnimatorGet(Name, pathName, controllerName);
		}

		/// <summary>
		/// 返回材质球
		/// </summary>
		/// <param name="pathName"></param>
		/// <returns></returns>
		public Material MaterialGet(string pathName, string matrialName)
		{
			return ResourceLoader.Ins.MaterialGet(Name, pathName, matrialName);
		}

		/// <summary>
		/// 返回Animation
		/// </summary>
		/// <param name="pathName"></param>
		/// <param name="animationName"></param>
		/// <returns></returns>
		public AnimationClip AnimationGet(string pathName, string animationName)
		{
			return ResourceLoader.Ins.AnimationGet(Name, pathName, animationName);
		}

		/// <summary>
		/// 返回Audio
		/// </summary>
		/// <param name="pathName"></param>
		/// <param name="audioName"></param>
		/// <returns></returns>
		public AudioClip AudioClipGet(string pathName, string audioName)
		{
			return ResourceLoader.Ins.AudioClipGet(Name, pathName, audioName);
		}

		/// <summary>
		/// 返回platform下图集
		/// </summary>
		/// <param name="pathName">路径为 resources/en/ 之后的内容。如：image/test.jpg</param>
		/// <param name="NotDefault">默认是default路径，传true意味着文件存放于language路径下</param>
		public Sprite[] PlatformSpritesGet(string pathName, bool NotDefault = false)
		{
			return ResourceLoader.Ins.SpritesGet(App.SharedModule, pathName, NotDefault);
		}

		/// <summary>
		/// 文本txt添加加载。
		/// </summary>
		/// <param name="pathName">路径为 resources/en/texts/ 之后的内容。如：username.txt 则填 username</param>
		/// <param name="NotDefault">默认是default路径，传true意味着文件存放于language路径下</param>
		public void TextAdd(string pathName, bool NotDefault = false)
		{
			ResourceLoader.Ins.TextAdd(Name, pathName, NotDefault);
		}

		/// <summary>
		/// 文本txt获取
		/// </summary>
		/// <param name="pathName">路径为 resources/en/texts/ 之后的内容。如：abc.txt</param>
		/// <param name="NotDefault">默认是default路径，传true意味着文件存放于language路径下</param>
		public string TextGet(string pathName, bool NotDefault = false)
		{
			return ResourceLoader.Ins.TextGet(Name, pathName, NotDefault);
		}

		/// <summary>
		/// 配置ini文本获取。
		/// </summary>
		/// <param name="pathName">路径为 resources/en/texts/ 之后的ini文件内容。 如：abc.ini 则填 abc</param>
		/// <param name="NotDefault">默认是default路径，传true意味着文件存放于language路径下</param>
		public IniFile IniGet(string pathName, bool NotDefault = false)
		{
			return ResourceLoader.Ins.IniGet(Name, pathName, NotDefault);
		}

		/// <summary>
		/// 添加prefab
		/// </summary>
		/// <param name="pbPath"></param>
		public void PrefabAdd(string pbPath)
		{
			ResourceLoader.Ins.PrefabAdd(Name, pbPath);
		}

		/// <summary>
		/// 获取已加载的prefab
		/// </summary>
		/// <param name="pbName"></param>
		/// <returns></returns>
		public UnityEngine.Object PrefabGet(string pbPath)
		{
			return ResourceLoader.Ins.PrefabGet(Name, pbPath);
		}

		public UnityEngine.Object PlatformPrefabGet(string pbPath)
		{
			return ResourceLoader.Ins.PrefabGet(App.SharedModule, pbPath);
		}

		/// <summary>
		/// 添加需要加载的场景
		/// </summary>
		/// <param name="sceneName"></param>
		public void SceneAdd(string sceneName)
		{
			ResourceLoader.Ins.SceneAdd(Name, sceneName);
		}

		/// <summary>
		/// 添加需要加载的目录。
		/// </summary>
		/// <param name="folderName"></param>
		public void FolderAdd(string folderName)
		{
			ResourceLoader.Ins.FolderAdd(Name, folderName);
		}

		/// <summary>
		/// 添加需要加载的目录，并排除不需要加载的目录
		/// </summary>
		/// <param name="folderName">文件夹名字</param>
		/// <param name="exceptFolders">过滤的文件夹名字数组</param>
		/// <param name="isFuzzy">是否模糊查询</param>
		public void FolderAdd(string folderName, string[] exceptFolders, bool isFuzzy)
		{
			ResourceLoader.Ins.FolderAdd(Name, folderName, exceptFolders, isFuzzy);
		}

		/// <summary>
		/// 添加完毕，开始集体加载。
		/// </summary>
		public void ResStartLoad()
		{
			ResourceLoader.Ins.StartLoading(Name);
		}

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		/// <returns></returns>
		public bool ResIsAllLoaded()
		{
			return ResourceLoader.Ins.IsLoaded(Name);
		}

		/// <summary>
		/// 在模块切出后，也不要移除
		/// </summary>
		/// <param name="path"></param>
		public void ResKeep(string path)
		{
			ResourceLoader.Ins.Keep(Name, path);
		}

		#endregion Resources Functions

		#region Scene Function

		private Scene LastScene = null;
		private Scene ThisScene = null;
		private Scene NextScene = null;

		public void SceneThisUpdate()
		{
			try
			{
				ThisScene?.OnUpdate();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void SceneThisExit()
		{
			if (ThisScene != null)
			{
				LastScene = ThisScene;
				ThisScene = null;
				try
				{
					LastScene.OnExit();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		private bool SceneAwakeCalled = false;

		public void SceneNextLoad(string sceneName)
		{
			SceneAwakeCalled = false;
			NextScene = Lobby.Ins.SceneLoad(sceneName);
		}

		public void SceneNextAwakeOnce()
		{
			if (NextScene == null) return;
			if (SceneAwakeCalled) return;
			NextScene.OnAwake();
			SceneAwakeCalled = true;
			Debug.Log($"Scene loaded, waiting scene to ready");
		}

		public bool SceneNextIsReady()
		{
			if (NextScene == null) return false;
			if (!Lobby.Ins.SceneIsLoaded())
			{
				return false;
			}
			bool IsReady = false;

			try
			{
				IsReady = NextScene.IsReadey();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return IsReady;
		}

		public void SceneNextShow()
		{
			SceneNextShow(IsShowLoading);
		}

		public void SceneNextShow(bool isShowLoading)
		{
			if (NextScene == null)
			{
				Debug.Log("SceneNextShow NextScene == null");
				return;
			}
			Lobby.Ins.SceneEnter(isShowLoading);
		}

		public void OnSceneNextEntered()
		{
			if (NextScene == null) return;

			try
			{
				NextScene.OnStart();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			ThisScene = NextScene;
			NextScene = null;

			try
			{
				ThisScene.OnRegisterCtrls();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			try
			{
				LastScene?.OnDestroy();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			LastScene = null;
			Debug.Log("Scene entered.");
		}

		public void SceneThisPause()
		{
			try
			{
				ThisScene?.OnPause();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void SceneThisResume()
		{
			try
			{
				ThisScene?.OnResume();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		#endregion Scene Function

		#region MB Messages

		public void MBAwake(GameObject go)
		{ ThisScene?.MBAwake(go); }

		public void MBOnDestroy(GameObject go)
		{ ThisScene?.MBOnDestroy(go); }

		public void MBStart(GameObject go)
		{ ThisScene?.MBStart(go); }

		public void MBReset(GameObject go)
		{ ThisScene?.MBReset(go); }

		public void MBUpdate(GameObject go)
		{ ThisScene?.MBUpdate(go); }

		public void MBOnDisable(GameObject go)
		{ ThisScene?.MBOnDisable(go); }

		public void MBOnEnable(GameObject go)
		{ ThisScene?.MBOnEnable(go); }

		public void MBFixedUpdate(GameObject go)
		{ ThisScene?.MBFixedUpdate(go); }

		public void MBLateUpdate(GameObject go)
		{ ThisScene?.MBLateUpdate(go); }

		public void OnAnimatorIK(GameObject go, int layerIndex)
		{ ThisScene?.OnAnimatorIK(go, layerIndex); }

		public void OnAnimatorMove(GameObject go)
		{ ThisScene?.OnAnimatorMove(go); }

		public void OnAudioFilterRead(GameObject go, float[] data, int channels)
		{ ThisScene?.OnAudioFilterRead(go, data, channels); }

		public void OnBecameInvisible(GameObject go)
		{ ThisScene?.OnBecameInvisible(go); }

		public void OnBecameVisible(GameObject go)
		{ ThisScene?.OnBecameVisible(go); }

		public void OnControllerColliderHit(GameObject go, ControllerColliderHit hit)
		{ ThisScene?.OnControllerColliderHit(go, hit); }

		public void OnJointBreak(GameObject go, float breakForce)
		{ ThisScene?.OnJointBreak(go, breakForce); }

		public void OnJointBreak2D(GameObject go, Joint2D joint)
		{ ThisScene?.OnJointBreak2D(go, joint); }

		public void OnMouseDown(GameObject go)
		{ ThisScene?.OnMouseDown(go); }

		public void OnMouseDrag(GameObject go)
		{ ThisScene?.OnMouseDrag(go); }

		public void OnMouseEnter(GameObject go)
		{ ThisScene?.OnMouseEnter(go); }

		public void OnMouseExit(GameObject go)
		{ ThisScene?.OnMouseExit(go); }

		public void OnMouseOver(GameObject go)
		{ ThisScene?.OnMouseOver(go); }

		public void OnMouseUp(GameObject go)
		{ ThisScene?.OnMouseUp(go); }

		public void OnMouseUpAsButton(GameObject go)
		{ ThisScene?.OnMouseUpAsButton(go); }

		public void OnParticleCollision(GameObject go, GameObject other)
		{ ThisScene?.OnParticleCollision(go, other); }

		public void OnParticleTrigger(GameObject go)
		{ ThisScene?.OnParticleTrigger(go); }

		public void OnParticleSystemStopped(GameObject go)
		{ ThisScene?.OnParticleSystemStopped(go); }

		public void OnPostRender(GameObject go)
		{ ThisScene?.OnPostRender(go); }

		public void OnPreCull(GameObject go)
		{ ThisScene?.OnPreCull(go); }

		public void OnPreRender(GameObject go)
		{ ThisScene?.OnPreRender(go); }

		public void OnRenderImage(GameObject go, RenderTexture source, RenderTexture destination)
		{ ThisScene?.OnRenderImage(go, source, destination); }

		public void OnRenderObject(GameObject go)
		{ ThisScene?.OnRenderObject(go); }

		public void OnTransformChildrenChanged(GameObject go)
		{ ThisScene?.OnTransformChildrenChanged(go); }

		public void OnTransformParentChanged(GameObject go)
		{ ThisScene?.OnTransformParentChanged(go); }

		public void OnTriggerEnter(GameObject go, Collider other)
		{ ThisScene?.OnTriggerEnter(go, other); }

		public void OnTriggerExit(GameObject go, Collider other)
		{ ThisScene?.OnTriggerExit(go, other); }

		public void OnTriggerStay(GameObject go, Collider other)
		{ ThisScene?.OnTriggerStay(go, other); }

		public void OnWillRenderObject(GameObject go)
		{ ThisScene?.OnWillRenderObject(go); }

		public void OnCollisionEnter(GameObject go, Collision collision)
		{ ThisScene?.OnCollisionEnter(go, collision); }

		public void OnCollisionEnter2D(GameObject go, Collision2D collision)
		{ ThisScene?.OnCollisionEnter2D(go, collision); }

		public void OnCollisionStay(GameObject go, Collision collision)
		{ ThisScene?.OnCollisionStay(go, collision); }

		public void OnCollisionExit(GameObject go, Collision collision)
		{ ThisScene?.OnCollisionExit(go, collision); }

		public void OnCollisionExit2D(GameObject go, Collision2D collision)
		{ ThisScene?.OnCollisionExit2D(go, collision); }

		public void OnCollisionStay2D(GameObject go, Collision2D collision)
		{ ThisScene?.OnCollisionStay2D(go, collision); }

		public void OnTriggerEnter2D(GameObject go, Collider2D collision)
		{ ThisScene?.OnTriggerEnter2D(go, collision); }

		public void OnTriggerStay2D(GameObject go, Collider2D collision)
		{ ThisScene?.OnTriggerStay2D(go, collision); }

		public void OnTriggerExit2D(GameObject go, Collider2D collision)
		{ ThisScene?.OnTriggerExit2D(go, collision); }

		public void OnPointerEnter(GameObject go, PointerEventData eventData)
		{ ThisScene?.OnPointerEnter(go, eventData); }

		public void OnPointerExit(GameObject go, PointerEventData eventData)
		{ ThisScene?.OnPointerExit(go, eventData); }

		public void OnPointerClick(GameObject go, PointerEventData eventData)
		{ ThisScene?.OnPointerClick(go, eventData); }

		#endregion MB Messages

		protected virtual void OnPause()
		{ }

		protected virtual void OnResume()
		{ }

		protected virtual void OnExit()
		{ }

		protected virtual void OnUpdate()
		{ }

		protected virtual void OnEnter(object arg)
		{ }
	}
}