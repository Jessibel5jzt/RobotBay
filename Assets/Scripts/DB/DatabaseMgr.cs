namespace WestBay
{
	/// <summary>
	/// 数据库管理类，维护各业务模块
	/// </summary>
	public class DatabaseMgr : Singleton<DatabaseMgr>
	{
		public DatabaseMgr()
		{
		}

		public void StartMgr()
		{
			Debug.Log($"<color=green>[Database]</color> 服务启动");
		}

		public void StopMgr()
		{
		}

		public static string ReadFromDBE(DBV value, string defaultValue = "")
		{
			string result = defaultValue;
			if (value != null)
			{
				result = value.ToString();
			}

			return result;
		}

		public static string IsNullCheck(string value)
		{
			if (value == null) return "";
			return value;
		}
	}
}