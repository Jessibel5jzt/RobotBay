namespace WestBay
{
	internal class BasedOnMB<T> : UnityEngine.MonoBehaviour where T : UnityEngine.MonoBehaviour
	{
		public static T Ins = null;

		protected bool Exiting = false;

		public void OnDestroy()
		{
			Exiting = true;
		}

		public static T AddComponent(string name)
		{
			UnityEngine.GameObject GO = UnityEngine.GameObject.Find(DDOLName);
			if (GO == null)
			{
				GO = new UnityEngine.GameObject(DDOLName);
				if (GO == null)
				{
					UnityEngine.Debug.LogError("Can not Create " + DDOLName);
					return null;
				}
				DontDestroyOnLoad(GO);
			}

			Ins = GO.AddComponent<T>();

			if (Ins == null)
			{
				UnityEngine.Debug.LogError($"Can not AddComponent<{name}>");
			}

			return Ins;
		}

		private static string DDOLName = "__westbay  __mbsingleton__";
	}//class
}//namespace