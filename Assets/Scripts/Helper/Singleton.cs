namespace WestBay
{
	/// <summary>
	/// 需要手动调用New()。原因是本类没必要支持多线程。所以只有这样才能安全。
	/// </summary>
	/// <typeparam name="T">子类</typeparam>
	public abstract class Singleton<T> where T : Singleton<T>, new()
	{
		public static T Instance { get; private set; } = null;
		public static T Ins
		{ get { return Instance; } }

		public static T _New()
		{
			if (Instance != null) return Instance;
			Instance = new T();
			Instance.OnNew();
			return Instance;
		}

		/// <summary>
		/// 此函数里不能调用其他 Singleton 类
		/// </summary>
		protected virtual void OnNew()
		{ }
	}//class
}//namespace