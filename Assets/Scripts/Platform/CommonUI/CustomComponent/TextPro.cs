using UnityEngine;
using UnityEngine.UI;
using System;

namespace WestBay
{
	public class TextPro : Text
	{
		public string TestNewProperty;

		protected override void OnEnable()
		{
			base.OnEnable();
			if (!Application.isPlaying && Application.isEditor) return;

			if (font == null) return;
			if (string.IsNullOrEmpty(FontMgr.CurrentFontName)) return;
			if (font.name.Contains("-"))
			{
				string[] fontinfo = font.name.Split(new char[] { '-' });
				var fw = (FontMgr.FontWeight)Enum.Parse(typeof(FontMgr.FontWeight), fontinfo[1]);
				if (font.name.Contains(FontMgr.CurrentFontName)) return;
				//todo 根据配置
				if (LocalMgr.CurrentCulture == "en")
				{
					font = FontMgr.GetFont("Roboto", fw, FontMgr.FontEXT.ttf);
				}
				else
				{
					font = FontMgr.GetFont("AlibabaPuHuiTi", fw, FontMgr.FontEXT.otf);
				}
			}
		}
	}
}