using LitJson;
using System;
using System.Collections.Generic;

namespace WestBay
{
	public class MqttMgr : Singleton<MqttMgr>
	{
		private Mqtt _mqttClient;

		public MqttMgr()
		{
		}

		public void Init(string productId, string userName, string password)
		{
			_productId = productId;
		}

		public bool StartMgr(string machineId, Mqtt client)
		{
			_mqttClient = client;

			var isConnect = ConnectServer();
			if (isConnect)
			{
				SubscribeCommand();
				GetProperty(new string[] { MqttAPI.SettingProperty.Language, MqttAPI.SettingProperty.Theme }, (msg) =>
				{
					ApplySetting(msg);
				});

				Debug.Log($"<color=green>[Mqtt]</color> 连接成功！");
			}
			else
			{
				Debug.Log($"<color=red>[Mqtt]</color> 连接失败！");
			}

			return isConnect;
		}

		public void StopMgr()
		{
		}

		private string _productId;
		private string _userName;
		private string _password;

		private bool ConnectServer()
		{
			var clientId = $"{_productId}";
			var time = DateTime.Now.ToString();
			_mqttClient.Connect(clientId, _userName, $"{_password}&{time}", false, 8 * 60 * 60);
			Debug.Log($"Mqtt connect result {_mqttClient.IsConnected}");

			return _mqttClient.IsConnected;
		}

		private bool IsRunning => _mqttClient != null && _mqttClient.IsConnected;

		#region 业务

		internal void OnModuleEnter(string moduleName)
		{
			if (!IsRunning) return;
		}

		private void ApplySetting(string msg)
		{
			if (!IsRunning) return;

			var jsonMsg = JsonMapper.ToObject(msg);
			var jsonData = jsonMsg["data"];
			if (JsonHelper.Contains(jsonData, MqttAPI.SettingProperty.Language))
			{
				var newLanguage = JsonHelper.ReadFromJson(jsonData, MqttAPI.SettingProperty.Language, "cn");
			}

			if (JsonHelper.Contains(jsonData, MqttAPI.SettingProperty.Theme))
			{
				var newTheme = JsonHelper.ReadFromJson(jsonData, MqttAPI.SettingProperty.Theme, ThemeMgr.DefaultTheme);
				if (newTheme.Equals(ThemeMgr.Themes.light.ToString()) || newTheme.Equals(ThemeMgr.Themes.dark.ToString()))
				{
				}
			}
		}

		#endregion 业务

		#region 事件

		private readonly Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();

		public void PublishEvent(string methodName, JsonData jsonValue, Action<string> callback = null)
		{
			if (!IsRunning) return;

			var msgId = GetMsgId();
			var jsonText = GetPublishEventData(msgId, methodName, jsonValue);
			var topic = GetTopic("event/publish");

			_callbacks.Add(msgId, callback);
			_mqttClient.Subscribe(GetTopicReply(topic), (msg) =>
			 {
				 var jsonMsg = JsonMapper.ToObject(msg);
				 var msgIdCB = jsonMsg["mid"].ToString();
				 _callbacks.TryGetValue(msgIdCB, out Action<string> aCallback);
				 aCallback?.Invoke(msg);
				 _callbacks.Remove(msgIdCB);
				 Debug.Log($"PublishEvent:{msg}");
			 });

			_mqttClient.Publish(topic, System.Text.Encoding.Default.GetBytes(jsonText));
		}

		private string GetPublishEventData(string msgId, string methodName, JsonData jsonValue)
		{
			JsonData jsonParams = new JsonData
			{
				["time"] = DateTime.Now.ToString(),
				["value"] = jsonValue
			};

			JsonData jsonData = new JsonData
			{
				["mid"] = msgId,
				["method"] = methodName,
				["params"] = jsonParams
			};

			return JsonMapper.ToJson(jsonData);
		}

		#endregion 事件

		#region 属性

		public void GetProperty(string[] propertyArray, Action<string> callback = null)
		{
			if (!IsRunning) return;

			JsonData jsonParams = new JsonData();
			for (int i = 0; i < propertyArray.Length; ++i)
			{
				jsonParams.Add(propertyArray[i]);
			}

			var msgId = GetMsgId();
			var jsonText = GetPublishPropertyData(msgId, "", jsonParams);
			var topic = GetTopic("property/get");

			_callbacks.Add(msgId, callback);
			_mqttClient.Subscribe(GetTopicReply(topic), (msg) =>
			{
				var jsonMsg = JsonMapper.ToObject(msg);
				var msgIdCB = jsonMsg["mid"].ToString();
				_callbacks.TryGetValue(msgIdCB, out Action<string> aCallback);
				_callbacks.Remove(msgIdCB);
				aCallback?.Invoke(msg);

				Debug.Log($"GetProperty:{msg}");
			});

			_mqttClient.Publish(topic, System.Text.Encoding.Default.GetBytes(jsonText));
		}

		public void PublishProperty(string key, string value, Action<string> callback = null)
		{
			if (!IsRunning) return;

			Dictionary<string, string> dic = new Dictionary<string, string>
			{
				{ key, value }
			};
			PublishProperty(dic, callback);
		}

		public void PublishProperty(Dictionary<string, string> propertyDic, Action<string> callback = null)
		{
			if (!IsRunning) return;

			JsonData jsonParams = new JsonData();
			foreach (var item in propertyDic)
			{
				SetProperty(jsonParams, item.Key, item.Value);
			}

			PublishProperty("", jsonParams, callback);
		}

		private void PublishProperty(string methodName, JsonData jsonParams, Action<string> callback = null)
		{
			var msgId = GetMsgId();
			var jsonText = GetPublishPropertyData(msgId, methodName, jsonParams);
			var topic = GetTopic("property/publish");

			_callbacks.Add(msgId, callback);
			_mqttClient.Subscribe(GetTopicReply(topic), (msg) =>
			{
				var jsonMsg = JsonMapper.ToObject(msg);
				var msgIdCB = jsonMsg["mid"].ToString();
				_callbacks.TryGetValue(msgIdCB, out Action<string> aCallback);
				_callbacks.Remove(msgIdCB);
				Debug.Log($"PublishProperty:{msg}");
			});
			_mqttClient.Publish(topic, System.Text.Encoding.Default.GetBytes(jsonText));
		}

		private void SetProperty(JsonData jsonParams, string key, string value)
		{
			JsonData jsonProperty = new JsonData
			{
				["value"] = value,
				["time"] = DateTime.Now.ToString()
			};

			jsonParams[key] = jsonProperty;
		}

		private string GetPublishPropertyData(string msgId, string methodName, JsonData jsonParams)
		{
			JsonData jsonData = new JsonData
			{
				["mid"] = msgId,
				["method"] = methodName,
				["params"] = jsonParams
			};

			return JsonMapper.ToJson(jsonData);
		}

		#endregion 属性

		#region 命令

		private readonly Dictionary<string, Action<string>> _cmdCallbacks = new Dictionary<string, Action<string>>();

		public bool RegisterCommand(string method, Action<string> callback, bool isForce = false)
		{
			if (!IsRunning) return false;

			bool result = false;
			if (_cmdCallbacks.TryGetValue(method, out Action<string> aCallback))
			{
				if (isForce)
				{
					_cmdCallbacks[method] = callback;
					result = true;
				}
			}
			else
			{
				_cmdCallbacks.Add(method, callback);
				result = true;
			}

			return result;
		}

		private void SubscribeCommand()
		{
			var topic = GetTopic("command/publish");
			_mqttClient.Subscribe(topic, (msg) =>
			{
				Debug.Log($"SubscribeCommand:{msg}");

				var jsonMsg = JsonMapper.ToObject(msg);
				string method = jsonMsg["method"].ToString();
				if (_cmdCallbacks.TryGetValue(method, out Action<string> callback))
				{
					callback.Invoke(msg);
				}
				else
				{
					Debug.Log($"SubscribeCommand:{method} no excute");
				}
			});
		}

		public void PublishCommand(bool isSuccess, JsonData jsonMsg)
		{
			if (!IsRunning) return;

			var jsonText = GetPublishCommandData(isSuccess, jsonMsg);
			var topic = GetTopic("command/publish_reply");

			_mqttClient.Publish(topic, System.Text.Encoding.Default.GetBytes(jsonText));
		}

		private string GetPublishCommandData(bool isSuccess, JsonData jsonMsg)
		{
			JsonData jsonData = new JsonData
			{
				["code"] = isSuccess ? 200 : -1,
				["mid"] = jsonMsg["mid"],
				["method"] = jsonMsg["method"],
				["params"] = jsonMsg["params"]
			};

			return JsonMapper.ToJson(jsonData);
		}

		#endregion 命令

		private string GetTopic(string name)
		{
			return $"/{_productId}/{name}";
		}

		private int _msgId = 0;

		private string GetMsgId()
		{
			_msgId++;
			return _msgId.ToString();
		}

		private string GetTopicReply(string topic)
		{
			return $"{topic}";
		}
	}
}