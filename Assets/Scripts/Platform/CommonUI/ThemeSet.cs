using System;
using UnityEngine;

namespace WestBay
{
	public class ThemeSet
	{
		public Sprite WeChat_sprite { get; private set; }

		public Color HighlightText_color { get; private set; }
		public Color NormalText_color { get; private set; }

		private Sprite[] _altas;
		private ModuleMethod _method;

		public ThemeSet(ModuleMethod methond)
		{
			_method = methond;
		}

		public void Init()
		{
			SetSprites();
			SetColor();
			SetAltas();
		}

		public Sprite GetSprite(string name)
		{
			return SerchSprite(_altas, name);
		}

		private Sprite[] GetAltas(string altasName)
		{
			return _method.SpritesGet($"images/{altasName}.png");
		}

		private void SetSprites()
		{
			_altas = GetAltas($"PanelPic{ThemeMgr.ThemeSplitMarker}");
		}

		private Color ParseColor(string html)
		{
			if (ColorUtility.TryParseHtmlString(html, out Color color))
			{
				return color;
			}

			Debug.LogError($"Parse Error: {html}");
			return Color.white;
		}

		/// <summary>
		/// ÑÕÉ«
		/// </summary>
		private void SetColor()
		{
			HighlightText_color = ParseColor("#0086D1");
			NormalText_color = ParseColor("#BDBFCA");
		}

		private Sprite SerchSprite(Sprite[] altas, string spriteName)
		{
			if (altas == null) return Sprite.Create(new Texture2D(100, 100), new Rect(Vector2.zero, new Vector2(100, 100)), Vector2.zero);

			return Array.Find(altas, (s) => s.name == spriteName);
		}

		/// <summary>
		/// Í¼¼¯Í¼Æ¬
		/// </summary>
		private void SetAltas()
		{
			Sprite[] defaultRes = LobbyUI.Ins.DefaultRes;
			WeChat_sprite = SerchSprite(_altas, "wechat");
		}
	}
}