using LitJson;
using System.Collections;
using System.Collections.Generic;

namespace WestBay
{
	public class ConfigMgr : Singleton<ConfigMgr>
	{
		public ConfigMgr()
		{
			//先初始化
			IniMgr.Init();
		}

		/// <summary>
		/// 初始化各配置
		/// </summary>
		public void Init()
		{
			LocalMgr.Init();
			ThemeMgr.Init();
			FontMgr.Init();

			UpgradeCheck();
		}

		/// <summary>
		/// 升级检测
		/// </summary>
		private void UpgradeCheck()
		{
		}

		#region Feature

		/// <summary>
		/// 加载配置化数据
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="robotType"></param>
		/// <param name="jsonKey"></param>
		/// <returns></returns>
		public Dictionary<string, FeatureItem> LoadFeature(string moduleName, string robotType, string jsonKey)
		{
			Dictionary<string, FeatureItem> result = new Dictionary<string, FeatureItem>();

			JsonData localFeature = LoadFeatureJson(moduleName, "feature.json", jsonKey);
			if (localFeature == null)
			{
				return result;
			}

			JsonData robotFeature = LoadFeatureJson($"{robotType}Train", $"feature{moduleName.ToLower()}.json", jsonKey);
			if (robotFeature == null)
			{
				robotFeature = LoadFeatureJson($"{robotType}Train", $"f{moduleName.ToLower()}.json", "Cnf");
			}

			Dictionary<string, FeatureItem> robotFeatureDic = GetRobotFeature(robotFeature);
			for (int i = 0; i < localFeature.Count; i++)
			{
				FeatureItem localFeatureItem = new FeatureItem();
				localFeatureItem.Decode(localFeature[i]);
				if (string.IsNullOrWhiteSpace(localFeatureItem.Key)) continue;

				if (robotFeatureDic != null && robotFeatureDic.TryGetValue(localFeatureItem.Key, out FeatureItem robotFeatureItem))
				{
					localFeatureItem.Value = robotFeatureItem.Value;
				}
				result.Add(localFeatureItem.Key, localFeatureItem);
			}

			return result;
		}

		/// <summary>
		/// 获取当前机型对应的配置
		/// </summary>
		/// <param name="featureJson"></param>
		/// <returns></returns>
		private Dictionary<string, FeatureItem> GetRobotFeature(JsonData featureJson)
		{
			if (featureJson == null) return null;

			Dictionary<string, FeatureItem> result = new Dictionary<string, FeatureItem>();
			for (int i = 0; i < featureJson.Count; i++)
			{
				FeatureItem item = new FeatureItem();
				item.Decode(featureJson[i]);
				result.Add(item.Key, item);
			}

			return result;
		}

		/// <summary>
		/// 获取配置化Json
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="fileName"></param>
		/// <param name="jsonKey"></param>
		/// <returns></returns>
		private JsonData LoadFeatureJson(string moduleName, string fileName, string jsonKey)
		{
			JsonData result = null;
			var content = ResourceLoader.Ins.GetConf(moduleName, fileName);
			if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(jsonKey))
			{
				Debug.Log($"Local {moduleName} config ERRO: {fileName} file is not exist!");
			}
			else
			{
				if (((IDictionary)JsonMapper.ToObject(content)).Contains(jsonKey))
				{
					result = JsonMapper.ToObject(content)[jsonKey];
				}
			}

			return result;
		}

		#endregion Feature
	}
}