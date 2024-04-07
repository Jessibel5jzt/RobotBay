using LitJson;
using System.Collections;

namespace WestBay
{
	public class FeatureItem
	{
		public string Id { get; set; }
		public string Key { get; set; }
		public string Value { get; set; }

		public void Decode(JsonData jsonData)
		{
			if (((IDictionary)jsonData).Contains("id"))
			{
				Id = jsonData["id"].ToString();
			}
			if (((IDictionary)jsonData).Contains("feature_key"))
			{
				Key = jsonData["feature_key"].ToString();
			}
			if (((IDictionary)jsonData).Contains("feature_value"))
			{
				Value = jsonData["feature_value"].ToString();
			}
		}
	}

	public enum FeatureValueType
	{
		None = -1,

		/// <summary>
		/// 默认值类型
		/// </summary>
		Default = 0,

		/// <summary>
		/// Json字符串
		/// </summary>
		Json,
	}
}