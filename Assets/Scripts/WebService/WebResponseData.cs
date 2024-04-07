using LitJson;
using System;

namespace WestBay
{
	/// <summary>
	/// Web服务器响应数据结构
	/// </summary>
	public struct WebResponseData
	{
		public string Code { get; set; }
		public string Msg { get; set; }
		public JsonData Data { get; set; }

		/// <summary>
		/// 是否成功
		/// </summary>
		public bool IsSuccess => Code.Equals("0") || Code.Equals("2");

		public WebResponseData(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				Code = "-1";
				Msg = "Error";
				Data = "";
			}
			else
			{
				try
				{
					JsonData jsonData = JsonMapper.ToObject(content);

					Code = JsonHelper.ReadFromJson(jsonData, "code");
					Msg = JsonHelper.ReadFromJson(jsonData, "msg");
					Data = jsonData["data"];
				}
				catch (Exception e)
				{
					Debug.Log($"WebRespone:解析数据失败! {e.ToString()}");
					Code = "-1";
					Msg = "Error";
					Data = "";
				}
			}
		}
	}
}