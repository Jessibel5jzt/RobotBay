namespace WestBay
{
	public class ConfigModel
	{
		public string Key { get; set; }
		public string Value { get; set; }

		public ConfigModel()
		{ }

		public ConfigModel(string key, string value)
		{
			Key = key;
			Value = value;
		}

		internal ConfigModel(DBEConfig dbData)
		{
			Key = DatabaseMgr.ReadFromDBE(dbData.Key);
			Value = DatabaseMgr.ReadFromDBE(dbData.Value);
		}

		internal DBEConfig EncodeDB()
		{
			return new DBEConfig
			{
				Key = Key,
				Value = Value,
			};
		}
	}
}