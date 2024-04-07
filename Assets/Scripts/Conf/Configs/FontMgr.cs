using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WestBay
{
	public class FontMgr
	{
		public enum FontWeight
		{
			Regular,
			Medium,
			Heavy
		}

		public enum FontEXT
		{
			otf,
			ttf,
			fon,
			font,
			ttc,
			eot,
			woff,
			woff2
		}

		public static string CurrentFontName { get; set; }

		public static void Init()
		{
			if (!Application.isPlaying) return;
			//todo 根据配置
			if (LocalMgr.CurrentCulture.CompareTo("cn") == 0)
			{
				CurrentFontName = "AlibabaPuHuiTi";
			}
			else
			{
				CurrentFontName = "Roboto";
			}
		}

		public static Font GetFont(string fontName, FontWeight fontWeight, FontEXT fontEXT)
		{
			if (ResourceLoader.Ins == null) return Resources.GetBuiltinResource<Font>("Arial.ttf");

			var font = ResourceLoader.Ins.FontGet(App.SharedModule, $"fonts/{fontName}/{fontName}-{fontWeight}.{fontEXT}", $"{fontName}-{fontWeight}");
			return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
		}

		public static Font GetFont(string fontName, FontEXT fontEXT)
		{
			if (ResourceLoader.Ins == null) return Resources.GetBuiltinResource<Font>("Arial.ttf");
			var font = ResourceLoader.Ins.FontGet(App.SharedModule, $"fonts/{fontName}/{fontName}.{fontEXT}", $"{fontName}");
			return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
		}
	}
}