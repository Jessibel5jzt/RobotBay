using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WestBay
{
	public sealed class Mqtt
	{
		private readonly MqttClient _client;
		private readonly Dictionary<string, Action<string>> _subscribeActions;
		public bool IsConnected => _client.IsConnected;

		public Mqtt(string ip, int port)
		{
			_client = new MqttClient(ip, port, false, new X509Certificate(), MqttSslProtocols.TLSv1_2);
			_client.MqttMsgPublishReceived += ClientMqttMsgPublishReceived;
			_client.MqttMsgPublished += ClientMqttMsgPublished;
			_client.MqttMsgSubscribed += ClientMqttMsgSubscribed;
			_client.ConnectionClosed += ConnectionClosed;
			_subscribeActions = new Dictionary<string, Action<string>>();
		}

		private void ConnectionClosed(object sender, EventArgs e)
		{
			Debug.Log("Mqtt ConnectionClosed");
		}

		public bool Connect(string clientId, string userName, string passward, bool cleanSession = false, ushort keepAlivePeriod = 60)
		{
			byte result = _client.Connect(clientId, userName, passward, cleanSession, keepAlivePeriod);
			Debug.Log($"Connect£º{result}");

			return _client.IsConnected;
		}

		public void Subscribe(string topic, Action<string> call)
		{
			if (!_subscribeActions.TryGetValue(topic, out Action<string> cb))
			{
				_subscribeActions.Add(topic, call);
				_client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
			}
			else
			{
				Debug.Log($"Subscribe topic:{topic} is already subscribe");
			}
		}

		public void UnSubscribe(string topic)
		{
			_subscribeActions.Remove(topic);
			_client.Unsubscribe(new string[] { topic });
		}

		public void Publish(string topic, byte[] data)
		{
			_client.Publish(topic, data, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
		}

		private void ClientMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			string topic = e.Topic;
			if (_subscribeActions.TryGetValue(topic, out Action<string> call))
			{
				string msg = System.Text.Encoding.Default.GetString(e.Message);
				call(msg);
			}
		}

		private void ClientMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
		{
			Debug.Log("topic published");
		}

		private void ClientMqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
		{
			Debug.Log("topic subscribed");
		}

		public void Destroy()
		{
			if (_client.IsConnected) _client.Disconnect();
			_subscribeActions.Clear();
			_client.MqttMsgPublishReceived -= ClientMqttMsgPublishReceived;
			_client.MqttMsgPublished -= ClientMqttMsgPublished;
			_client.MqttMsgSubscribed -= ClientMqttMsgSubscribed;
			_client.ConnectionClosed -= ConnectionClosed;
		}
	}
}