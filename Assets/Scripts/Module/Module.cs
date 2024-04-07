namespace WestBay
{
	/// <summary>
	/// 子模块需要继承此类
	/// </summary>
	/// @ingroup CoreApi
	public class Module : ModuleMethod
	{
		/// <summary>
		/// 子类需要重写此方法，以便框架可以获取模块名称
		/// </summary>
		public override string Name
		{ get { return ""; } }

		/// <summary>
		/// 返回本模块是否可以切入
		/// </summary>
		/// <returns>是否可以切入</returns>
		public virtual bool IsReady()
		{ return false; }

		/// <summary>
		/// 被加载到内存时。此时还没有进入模块。
		/// </summary>
		public virtual void OnLoad()
		{ }

		/// <summary>
		/// 刚进入模块。此时OnUpdate开始调用
		/// </summary>
		protected override void OnEnter(object arg)
		{ }

		/// <summary>
		/// 每帧被调用。OnEnter执行之后才开始。
		/// </summary>
		protected override void OnUpdate()
		{ }

		/// <summary>
		/// 系统暂停了
		/// </summary>
		protected override void OnPause()
		{ }

		/// <summary>
		/// 暂停取消
		/// </summary>
		protected override void OnResume()
		{ }

		/// <summary>
		/// 收到结束请求
		/// </summary>
		protected override void OnExit()
		{ }

		/// <summary>
		/// 马上就要被卸载了
		/// </summary>
		public virtual void OnDestroy()
		{ }
	}
}