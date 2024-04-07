using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace WestBay
{
	public class WebReqHelper
	{
		/// <summary>
		/// 服务URL
		/// </summary>
		/// <param name="api">接口</param>
		/// <returns></returns>
		public static string GetDasApiUrl(string api)
		{
			StringBuilder result = new StringBuilder();

			if (WebAPI.IsTest)
			{
				result.Append(WebAPI.DAS_Url_LocalTest);
			}
			else
			{
				if (string.IsNullOrEmpty(WebAPI.Web_DAS_Url)) return string.Empty;
				result.Append(WebAPI.Web_DAS_Url);
			}
			result.Append(api);
			return result.ToString();
		}

		/// <summary>
		/// Doop URL
		/// </summary>
		/// <param name="api">接口</param>
		/// <returns></returns>
		public static string GetDoopApiUrl(string api)
		{
			StringBuilder result = new StringBuilder();

			if (WebAPI.IsTest)
			{
				result.Append(WebAPI.DOOP_Url_LocalTest);
			}
			else
			{
				if (string.IsNullOrEmpty(WebAPI.Web_DOOP_Url)) return string.Empty;
				result.Append(WebAPI.Web_DOOP_Url);
			}
			result.Append(api);
			return result.ToString();
		}

		public static string GetWebTestServerUrl(string api)
		{
			StringBuilder result = new StringBuilder();

			if (string.IsNullOrEmpty(WebAPI.DOOP_Url_LocalTest)) return string.Empty;

			result.Append(WebAPI.DOOP_Url_LocalTest);
			result.Append(api);
			return result.ToString();
		}

		#region 服务器列表

		public static string ServerType
		{
			get
			{
				if (IniMgr.Config != null) return IniMgr.Config.GetValue("ServerType");
				else return "debug";
			}
		}

		public static event Action<bool> OnServerConnectEvent;

		/// <summary>
		///请求是否完成
		/// </summary>
		public static bool IsRequestDone { get; private set; } = false;

		/// <summary>
		/// 服务是否可用
		/// </summary>
		public static bool IsServerAvailable => IsRequestDone && _isDasAvailable;

		/// <summary>
		/// DOOP服务是否可用
		/// </summary>
		public static bool IsDoopAvailable => IsRequestDone && _isDoopAvailable;

		private static bool _isRequest = true;
		private static int _reqeustTimes = 0;
		private static int _requestTimeMax = 3;
		private static bool _isDasAvailable = false;
		private static bool _isDoopAvailable = false;

		/// <summary>
		/// 指定地址是否可访问
		/// </summary>
		/// <param name="url"></param>
		/// <param name="timeOut"></param>
		/// <returns></returns>
		public static async UniTask<bool> IsHostAvailable(string url, int timeOut = 10)
		{
			var result = await PostRequestAsync(UnityWebRequest.Head(url), timeOut);

			return result;
		}

		public static void StopRequest()
		{
			_isRequest = false;
		}

		/// <summary>
		/// Web服务器请求
		/// </summary>
		public static async UniTaskVoid WebServerRequest()
		{
			IsRequestDone = false;
			while (_isRequest)
			{
				var result = await RequestServerList();
				if (result)
				{
					OnServerConnectEvent?.Invoke(true);
					IsRequestDone = true;

					ReqeustPublicIP();
					break;
				}
				else
				{
					_reqeustTimes++;
					if (_reqeustTimes <= _requestTimeMax)
					{
						Debug.Log($"WebReqHelper: 开始重试请求:{_reqeustTimes}");
						await UniTask.Delay(10 * 000);
					}
					else
					{
						Debug.Log($"WebReqHelper: 重试失败，无网络服务！");
						OnServerConnectEvent?.Invoke(false);
						IsRequestDone = true;
						break;
					}
				}
			}

			Debug.Log($"WebReqHelper: 完成服务端请求！");
		}

		/// <summary>
		/// 获取web服务器列表
		/// </summary>
		/// <returns></returns>
		private static async UniTask<bool> RequestServerList()
		{
			string serverListUrl = WebAPI.Server_List_Url;
			if (string.IsNullOrEmpty(WebAPI.Server_Host))
			{
				serverListUrl = $"{WebAPI.Server_Default_Host}{WebAPI.Server_List_API}";
			}

			if (WebAPI.IsTest)
			{
				serverListUrl = $"{WebAPI.DAS_Url_LocalTest}{WebAPI.Server_List_API}";
			}

			Debug.Log("地址：" + serverListUrl);
			try
			{
				var resData = await PostRequestByJson(serverListUrl, "{}");
				if (resData.IsSuccess && resData.Data.Count > 0)
				{
					ParseServerData(resData.Data);
				}
				else
				{
					//网络错误或者其他httperror
					Debug.Log("WebReqHelper: network error/http error");
				}
			}
			catch (Exception ex)
			{
				Debug.Log("WebReqHelper error =" + ex.ToString());
				return false;
			}

			Debug.Log($"[{ServerType}][DAS]WebReqHelper: {WebAPI.Web_DAS_Url},{WebAPI.Web_Store_Url},{_isDasAvailable}");
			Debug.Log($"[{ServerType}][DOOP]WebReqHelper: {WebAPI.Web_DOOP_Url},{_isDoopAvailable}");

			return _isDasAvailable;
		}

		private static void ParseServerData(JsonData resJsonData)
		{
			if (Enum.IsDefined(typeof(WebSeverType), ServerType))
			{
				var url = string.Empty;
				var frontUrl = string.Empty;
				var dataDic = (IDictionary)resJsonData;
				if (dataDic.Contains(ServerType))
				{
					var serverData = resJsonData[ServerType];
					var serverDic = (IDictionary)serverData;

					if (serverDic.Contains(WebAPI.ServerName_DAS))
					{
						var dasData = serverData[WebAPI.ServerName_DAS];
						var dasDic = (IDictionary)dasData;
						if (dasDic.Contains("ip"))
						{
							url = $"http://{dasData["ip"]}";
						}
						if (dasDic.Contains("frontIp"))
						{
							frontUrl = $"http://{dasData["frontIp"]}";
						}
						WebAPI.Web_DAS_Url = url;
						WebAPI.Web_Store_Url = frontUrl;
					}
					if (serverDic.Contains(WebAPI.ServerName_DOOP))
					{
						var doopData = serverData[WebAPI.ServerName_DOOP];
						var dasDic = (IDictionary)doopData;
						if (dasDic.Contains("ip"))
						{
							url = $"http://{doopData["ip"]}";
						}
						WebAPI.Web_DOOP_Url = url;
					}
				}
				_isDasAvailable = !string.IsNullOrWhiteSpace(WebAPI.Web_DAS_Url);
				_isDoopAvailable = !string.IsNullOrWhiteSpace(WebAPI.Web_DOOP_Url);
			}
			else
			{
				Debug.Log("WebReqHelper: config is error");
			}
		}

		public enum WebSeverType
		{
			debug,
			release
		}

		#endregion 服务器列表

		#region 公网IP

		/// <summary>
		/// 公网IP
		/// </summary>
		private static void ReqeustPublicIP()
		{
			var response = PostRequest(WebAPI.Public_IP_Url, null, (responseResult, responseText) =>
			{
				if (!string.IsNullOrEmpty(responseText))
				{
					WebAPI.Public_IP = responseText.TrimEnd();
				}
			});
		}

		#endregion 公网IP

		#region 不依赖具体业务web请求接口

		/// <summary>
		/// Web request timeout
		/// </summary>
		private const int WEB_REQUEST_TIMEOUT = 10;

		/// <summary>
		/// 从服务端请求文本数据
		/// </summary>
		/// <param name="url"></param>
		/// <param name="dataDict"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask PostRequest(string url, Dictionary<string, string> dataDict, Action<bool, string> callback)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				callback?.Invoke(false, "");

				return;
			}

			UnityWebRequest webRequest;
			if (dataDict == null)
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				webRequest = UnityWebRequest.Post(url, FormData(dataDict));
			}

			using (webRequest)
			{
				var response = await PostRequestAsync(webRequest);
				if (response != null)
				{
					Log($"url:{url},respone:{response.text}");
					callback?.Invoke(true, response.text);
				}
				else
				{
					callback?.Invoke(false, "");
				}
			}
		}

		/// <summary>
		/// 从服务端请求文本数据
		/// </summary>
		/// <param name="url"></param>
		/// <param name="dataDict"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask PostRequestByJson(string url, string json, Action<bool, string> callback)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				callback?.Invoke(false, "");

				return;
			}

			UnityWebRequest webRequest;
			if (string.IsNullOrWhiteSpace(json))
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				byte[] databyte = Encoding.UTF8.GetBytes(json);
				webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
				webRequest.uploadHandler = new UploadHandlerRaw(databyte);
				webRequest.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
			}
			webRequest.downloadHandler = new DownloadHandlerBuffer();
			webRequest.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
			await webRequest.SendWebRequest();

			using (webRequest)
			{
				while (!webRequest.isDone)
				{
					await UniTask.Delay(10);
				}

				if (IsWebReqeustError(webRequest))
				{
					callback?.Invoke(false, "");
				}
				else
				{
					Debug.Log($"url:{url},respone:{webRequest.downloadHandler.text}");
					callback?.Invoke(true, webRequest.downloadHandler.text);
				}
			}
		}

		/// <summary>
		/// 从服务端请求文本数据
		/// </summary>
		/// <param name="url"></param>
		/// <param name="dataDict"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask<WebResponseData> PostRequestByJson(string url, string json)
		{
			var response = new WebResponseData("");
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				return response;
			}

			UnityWebRequest webRequest;
			if (string.IsNullOrWhiteSpace(json))
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				byte[] databyte = Encoding.UTF8.GetBytes(json);
				webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
				webRequest.uploadHandler = new UploadHandlerRaw(databyte);
				webRequest.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
			}
			webRequest.downloadHandler = new DownloadHandlerBuffer();
			webRequest.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
			await webRequest.SendWebRequest();

			using (webRequest)
			{
				while (!webRequest.isDone)
				{
					await UniTask.Delay(10);
				}

				if (IsWebReqeustError(webRequest))
				{
					return response;
				}
				else
				{
					Log($"url:{url},respone:{webRequest.downloadHandler.text}");
					response = new WebResponseData(webRequest.downloadHandler.text);
					return response;
				}
			}
		}

		public static async UniTask<byte[]> GetRequest(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				return null;
			}

			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				var response = await PostRequestAsync(webRequest);
				if (response != null)
				{
					return response.data;
				}
				else
				{
					return null;
				}
			}
		}

		public static byte[] GetRequestSync(string url)
		{
			byte[] result = new byte[0];

			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				webRequest.SendWebRequest();
				while (!webRequest.isDone)
				{
					if (IsWebReqeustError(webRequest))
					{
						Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
					}
					else
					{
						result = webRequest.downloadHandler.data;
					}
				}
			}

			return result;
		}

		public static async UniTask<byte[]> GetRequest(string url, Action<ulong> callback)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				return null;
			}

			using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
			{
				var asyncOperation = webRequest.SendWebRequest();
				while (!asyncOperation.isDone)
				{
					callback?.Invoke(webRequest.downloadedBytes);
					await UniTask.Delay(10);
				}
				callback?.Invoke(webRequest.downloadedBytes);
				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
					return null;
				}
				else
				{
					return webRequest.downloadHandler.data;
				}
			}
		}

		public static async UniTask PostRequestWithForm(string url, WWWForm form, Action<bool, string> callback)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				callback?.Invoke(false, "");

				return;
			}

			UnityWebRequest webRequest;
			if (form == null)
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				webRequest = UnityWebRequest.Post(url, form);
			}

			using (webRequest)
			{
				var response = await PostRequestAsync(webRequest);
				if (response != null)
				{
					Log($"url:{url},respone:{response.text}");
					callback?.Invoke(true, response.text);
				}
				else
				{
					callback?.Invoke(false, "");
				}
			}
		}

		public static async UniTask PostRequest(string url, Action<bool, string> callback, Action<float> progress = null, WWWForm form = null, Dictionary<string, string> headers = null)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				callback?.Invoke(false, "");

				return;
			}

			UnityWebRequest webRequest;
			if (form == null)
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				webRequest = UnityWebRequest.Post(url, form);
			}

			using (webRequest)
			{
				if (headers != null && headers.Count > 0)
				{
					foreach (var item in headers)
					{
						webRequest.SetRequestHeader(item.Key, item.Value);
					}
				}
				await webRequest.SendWebRequest();

				while (!webRequest.isDone)
				{
					Debug.Log("is uploading");
					progress?.Invoke(webRequest.downloadProgress);
					await UniTask.Delay(3000);
				}

				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
					callback?.Invoke(false, "");
				}
				else
				{
					Log($"url:{url},respone:{webRequest.downloadHandler.text}");
					callback?.Invoke(true, webRequest.downloadHandler.text);
				}
			}
		}

		/// <summary>
		/// 返回通用Web服务端响应结构
		/// </summary>
		/// <param name="url"></param>
		/// <param name="dataDict"></param>
		/// <returns></returns>
		public static async UniTask<WebResponseData> PostRequest(string url, Dictionary<string, string> dataDict)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				return new WebResponseData("");
			}

			UnityWebRequest webRequest;
			if (dataDict == null)
			{
				webRequest = UnityWebRequest.Post(url, "");
			}
			else
			{
				webRequest = UnityWebRequest.Post(url, FormData(dataDict));
			}

			using (webRequest)
			{
				var response = await PostRequestAsync(webRequest);
				if (response != null)
				{
					Log($"url:{url},respone:{response.text}");
					return new WebResponseData(response.text);
				}
				else
				{
					return new WebResponseData("");
				}
			}
		}

		/// <summary>
		/// 从服务端请求图片数据
		/// </summary>
		/// <param name="url"></param>
		/// <param name="dataDict"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static async UniTask PostRequestTexture(string url, Action<bool, Texture2D> callback)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				callback?.Invoke(false, null);

				return;
			}

			UnityWebRequest webRequest = new UnityWebRequest(url);
			DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture(true);
			webRequest.downloadHandler = downloadHandlerTexture;

			var result = await PostRequestAsync(webRequest);
			if (result != null)
			{
				callback?.Invoke(true, downloadHandlerTexture.texture);
			}
			else
			{
				callback?.Invoke(false, null);
			}
		}

		private static async UniTask<DownloadHandler> PostRequestAsync(UnityWebRequest webRequest)
		{
			DownloadHandler result = null;
			try
			{
				webRequest.timeout = WEB_REQUEST_TIMEOUT;
				Log($"PostRequestAsync:{webRequest.url}");
				var asyncOperation = webRequest.SendWebRequest();
				while (!asyncOperation.isDone)
				{
					await UniTask.Delay(1000);
				}

				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
				}
				else
				{
					result = webRequest.downloadHandler;
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"PostRequestAsync:{ex},url:{webRequest.url}");
			}

			return result;
		}

		private static async UniTask<DownloadHandler> DownloadHandlerPostRequestAsync(UnityWebRequest webRequest, Action<float> progress = null)
		{
			DownloadHandler downResult = null;
			try
			{
				Log($"PostRequestAsync:{webRequest.url}");
				AsyncOperation request = webRequest.SendWebRequest();
				while (!request.isDone)
				{
					progress?.Invoke(request.progress);
					await UniTask.Delay(100);
				}

				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error === {webRequest.error}, url:{webRequest.url}");
				}
				else
				{
					downResult = webRequest.downloadHandler;
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"PostRequestAsync:{ex},url:{webRequest.url}");
			}

			return downResult;
		}

		private static async UniTask<bool> PostRequestAsync(UnityWebRequest webRequest, int timeOut = 0)
		{
			bool result = false;
			if (timeOut == 0) timeOut = WEB_REQUEST_TIMEOUT;
			try
			{
				webRequest.timeout = timeOut;
				await webRequest.SendWebRequest();
				while (!webRequest.isDone)
				{
					await UniTask.Delay(100);
				}

				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
				}
				else
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"PostRequestAsync:{ex.ToString()},url:{webRequest.url}");
			}

			return result;
		}

		public static async UniTask<ulong> HeadRequestAsync(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				Debug.Log($"error === url invalid");
				return 0;
			}

			ulong length = 0;
			try
			{
				UnityWebRequest webRequest = UnityWebRequest.Head(url);
				await webRequest.SendWebRequest();
				while (!webRequest.isDone)
				{
					await UniTask.Delay(1000);
				}

				if (IsWebReqeustError(webRequest))
				{
					Debug.Log($"error=== {webRequest.error},url:{webRequest.url}");
				}
				else
				{
					ulong.TryParse(webRequest.GetResponseHeader("Content-Length"), out length);
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"PostRequestAsync:{ex},url:{url}");
			}

			return length;
		}

		private static WWWForm FormData(Dictionary<string, string> dataDict)
		{
			if (dataDict == null) return new WWWForm();

			WWWForm form = new WWWForm();
			foreach (var item in dataDict)
			{
				if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value)) continue;

				try
				{
					form.AddField(item.Key, item.Value);
				}
				catch (Exception e)
				{
					Debug.Log($"FormData:{e}");
					continue;
				}
			}

			return form;
		}

		private static bool IsWebReqeustError(UnityWebRequest webRequest)
		{
			var requestResult = webRequest.result;
			return !string.IsNullOrEmpty(webRequest.error)
				|| requestResult == UnityWebRequest.Result.ConnectionError
				|| requestResult == UnityWebRequest.Result.ProtocolError;
		}

		#endregion 不依赖具体业务web请求接口

		private static void Log(string log)
		{
			if (!WebAPI.Web_Log_Enable) return;

			Debug.Log(log);
		}
	}
}