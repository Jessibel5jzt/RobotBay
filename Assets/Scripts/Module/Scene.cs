using UnityEngine;
using UnityEngine.EventSystems;

namespace WestBay
{
	/// <summary>
	/// 模块场景继承此类
	/// </summary>
	/// @ingroup CoreApi
	public class Scene : SceneMethod
	{
		/// <summary>
		/// 子类需要重写此方法，以便框架可以获取模块名称
		/// </summary>
		public override string Name
		{ get { return ""; } }

		/// <summary>
		/// 返回场景是否可以切入
		/// </summary>
		/// <returns>是否可以切入</returns>
		public virtual bool IsReadey()
		{ return false; }

		/// <summary>
		/// 被加载到内存时。此时还没有进入模块。
		/// </summary>
		public virtual void OnAwake()
		{ }

		/// <summary>
		/// 刚进入场景。执行后OnUpdate开始调用
		/// </summary>
		public virtual void OnStart()
		{ }

		/// <summary>
		/// 在此初始化所有UI元素
		/// </summary>
		public virtual void OnRegisterCtrls()
		{ }

		/// <summary>
		/// 每帧被调用
		/// </summary>
		public virtual void OnUpdate()
		{ }

		/// <summary>
		/// 系统暂停了
		/// </summary>
		public virtual void OnPause()
		{ }

		/// <summary>
		/// 暂停取消
		/// </summary>
		public virtual void OnResume()
		{ }

		/// <summary>
		/// 收到请求结束，执行后，OnUpdate不在被调用
		/// </summary>
		public virtual void OnExit()
		{ }

		/// <summary>
		/// 马上就要被卸载了
		/// </summary>
		public virtual void OnDestroy()
		{ }

		///@name MonoBehaviour 回调
		///场景里的所有下列事件均会被收到
		///@{
		public virtual void MBAwake(GameObject scriptObject)
		{ }

		public virtual void MBOnDestroy(GameObject scriptObject)
		{ }

		public virtual void MBStart(GameObject scriptObject)
		{ }

		public virtual void MBReset(GameObject scriptObject)
		{ }

		public virtual void MBUpdate(GameObject scriptObject)
		{ }

		public virtual void MBOnDisable(GameObject scriptObject)
		{ }

		public virtual void MBOnEnable(GameObject scriptObject)
		{ }

		public virtual void MBFixedUpdate(GameObject scriptObject)
		{ }

		public virtual void MBLateUpdate(GameObject scriptObject)
		{ }

		public virtual void OnAnimatorIK(GameObject scriptObject, int layerIndex)
		{ }

		public virtual void OnAnimatorMove(GameObject scriptObject)
		{ }

		public virtual void OnAudioFilterRead(GameObject scriptObject, float[] data, int channels)
		{ }

		public virtual void OnBecameInvisible(GameObject scriptObject)
		{ }

		public virtual void OnBecameVisible(GameObject scriptObject)
		{ }

		public virtual void OnControllerColliderHit(GameObject scriptObject, ControllerColliderHit hit)
		{ }

		public virtual void OnJointBreak(GameObject scriptObject, float breakForce)
		{ }

		public virtual void OnJointBreak2D(GameObject scriptObject, Joint2D joint)
		{ }

		public virtual void OnMouseDown(GameObject scriptObject)
		{ }

		public virtual void OnMouseDrag(GameObject scriptObject)
		{ }

		public virtual void OnMouseEnter(GameObject scriptObject)
		{ }

		public virtual void OnMouseExit(GameObject scriptObject)
		{ }

		public virtual void OnMouseOver(GameObject scriptObject)
		{ }

		public virtual void OnMouseUp(GameObject scriptObject)
		{ }

		public virtual void OnMouseUpAsButton(GameObject scriptObject)
		{ }

		public virtual void OnParticleCollision(GameObject scriptObject, GameObject other)
		{ }

		public virtual void OnParticleTrigger(GameObject scriptObject)
		{ }

		public virtual void OnParticleSystemStopped(GameObject scriptObject)
		{ }

		public virtual void OnPostRender(GameObject scriptObject)
		{ }

		public virtual void OnPreCull(GameObject scriptObject)
		{ }

		public virtual void OnPreRender(GameObject scriptObject)
		{ }

		public virtual void OnRenderImage(GameObject scriptObject, RenderTexture source, RenderTexture destination)
		{ }

		public virtual void OnRenderObject(GameObject scriptObject)
		{ }

		public virtual void OnTransformChildrenChanged(GameObject scriptObject)
		{ }

		public virtual void OnTransformParentChanged(GameObject scriptObject)
		{ }

		public virtual void OnTriggerEnter(GameObject scriptObject, Collider other)
		{ }

		public virtual void OnTriggerExit(GameObject scriptObject, Collider other)
		{ }

		public virtual void OnTriggerStay(GameObject scriptObject, Collider other)
		{ }

		public virtual void OnWillRenderObject(GameObject scriptObject)
		{ }

		public virtual void OnCollisionEnter(GameObject scriptObject, Collision collision)
		{ }

		public virtual void OnCollisionEnter2D(GameObject scriptObject, Collision2D collision)
		{ }

		public virtual void OnCollisionStay(GameObject scriptObject, Collision collision)
		{ }

		public virtual void OnCollisionExit(GameObject scriptObject, Collision collision)
		{ }

		public virtual void OnCollisionExit2D(GameObject scriptObject, Collision2D collision)
		{ }

		public virtual void OnCollisionStay2D(GameObject scriptObject, Collision2D collision)
		{ }

		public virtual void OnTriggerEnter2D(GameObject scriptObject, Collider2D collision)
		{ }

		public virtual void OnTriggerStay2D(GameObject scriptObject, Collider2D collision)
		{ }

		public virtual void OnTriggerExit2D(GameObject scriptObject, Collider2D collision)
		{ }

		public virtual void OnPointerEnter(GameObject go, PointerEventData eventData)
		{ }

		public virtual void OnPointerExit(GameObject go, PointerEventData eventData)
		{ }

		public virtual void OnPointerClick(GameObject go, PointerEventData eventData)
		{ }

		///@}
	}
}