using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WestBay
{
	public class ThemeMgr
	{
		public enum Themes
		{
			light,
			dark
		}

		/// <summary>
		/// 当前主题
		/// </summary>
		public static string CurrentTheme { get; set; }

		public static readonly string ThemeSplitMarker = "&)theme(&";

		/// <summary>
		/// light
		/// </summary>
		public static readonly string DefaultTheme = "light";

		public static Dictionary<string, ColorItem> ThemeColor { get; set; }

		/// <summary>
		///
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="lanPath"></param>
		/// <returns></returns>
		public static string TransPath(string moduleName, string lanPath)
		{
			if (lanPath.Contains(ThemeSplitMarker))
			{
				string thmPath = lanPath.Insert(lanPath.LastIndexOf(ThemeSplitMarker) + ThemeSplitMarker.Length, CurrentTheme);
				string fullStlPath = PathUtil.GetPersistPath(moduleName, thmPath);
				if (!File.Exists(fullStlPath))
				{
					string lightPath = thmPath.Replace(CurrentTheme, "");
					return lightPath;
				}
				return thmPath;
			}

			return lanPath;
		}

		public static void Init()
		{
			if (!Application.isPlaying) return;

			//CurrentTheme = ConfigMgr.Ins.Prefs.Theme;
			SetCusor();
			SetThemeColor();
		}

		public static void SetThemeColor()
		{
			ThemeColor = new Dictionary<string, ColorItem>();
			var content = FileHelper.ReadFile(PathUtil.GetPersistPath(App.SharedModule, "Theme.json"));
			if (string.IsNullOrWhiteSpace(content))
			{
				Debug.Log("ThemeMgr SetThemeColor theme file is empty!");
				return;
			}

			var themeJson = JsonMapper.ToObject(content);
			var themeDic = (IDictionary)themeJson;
			if (!themeDic.Contains(CurrentTheme)) { Debug.LogError("未找到当前主题颜色配置"); }
			var curThemeJson = themeJson[CurrentTheme];
			curThemeJson.SetJsonType(JsonType.Array);
			foreach (JsonData curThemeItemJson in curThemeJson)
			{
				var curThemeItemDic = (IDictionary)curThemeItemJson;
				var colorTypeItem = new ColorItem();
				if (curThemeItemDic.Contains("color_type") && curThemeItemDic.Contains("color"))
				{
					colorTypeItem.ColorType = curThemeItemJson["color_type"].ToString();
					colorTypeItem.Color = curThemeItemJson["color"].ToString();
					ThemeColor.Add(colorTypeItem.ColorType, colorTypeItem);
				}
			}
		}

		public static bool IsExist(string theme)
		{
			return Enum.TryParse(theme, true, out Themes flag);
		}

		/// <summary>
		/// 设置自定义光标
		/// </summary>
		public static void SetCusor()
		{
			if (!IniMgr.Config.GetValue("Conf_Cursor_Enable").Equals("1")) return;

			Cursor.SetCursor(Resources.Load<Texture2D>("Images/Cursor"), Vector2.zero, CursorMode.ForceSoftware);
		}

		/// <summary>
		/// 获取当前主题中缀
		/// </summary>
		/// <returns></returns>
		public static string GetThemeInfix()
		{
			StringBuilder result = new StringBuilder(ThemeSplitMarker);

			if (!CurrentTheme.Equals(DefaultTheme, StringComparison.Ordinal))
			{
				result.Append(CurrentTheme);
			}

			return result.ToString();
		}

		public class ColorItem
		{
			/// <summary>
			///
			/// </summary>
			public string ColorType { get; set; }

			/// <summary>
			///
			/// </summary>
			public string Color { get; set; }
		}
	}
}