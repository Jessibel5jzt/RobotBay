namespace WestBay
{
	internal class DBMConfig : DBMgrT<DBMConfig, DBEConfig>
	{
		public DBMConfig(string path) : base(new DBEConfig())
		{
			SetSH(path);
		}

		public string GetValue(string key)
		{
			var Val = SelectData1(new DBEConfig { Key = key });
			return Val == null ? "" : (string)Val.Value;
		}

		public bool SetValue(string key, string value)
		{
			var Val = SelectData1(new DBEConfig { Key = key });
			if (Val == null) return false;
			Val.Value = value;
			return ModifyData(Val);
		}
	}
}