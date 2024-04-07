using LitJson;

namespace WestBay
{
	public class ToggleClass
	{
		public bool IsSelect;
		public string Name;

		public ToggleClass(bool isSelect, string name)
		{
			IsSelect = isSelect;
			Name = name;
		}

		public void Decode(JsonData jsonData)
		{
			this.IsSelect = bool.Parse(JsonHelper.ReadFromJson(jsonData, "IsSelect", "False"));
			this.Name = JsonHelper.ReadFromJson(jsonData, "Name", "");
		}

		public JsonData Encode()
		{
			JsonData result = new JsonData
			{
				["IsSelect"] = IsSelect,
				["Name"] = Name
			};

			return result;
		}
	}
}