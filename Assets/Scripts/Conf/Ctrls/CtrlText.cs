using UnityEngine;
using UnityEngine.UI;

namespace WestBay
{
	[ExecuteInEditMode]
	public class CtrlText : MonoBehaviour
	{
		public float preferredWidth;

		public float preferredHeight;

		[Tooltip("该Text组件的key是否在Shared 上")]
		public bool isPlatform = false;

		[Tooltip("该Text组件颜色是否根据主题更新")]
		public bool IsThemeColor = true;

		[Tooltip("是否要区别换行符")]
		public bool IsLinefeed = false;

		/// <summary>
		/// 不要用这个修改
		/// </summary>
		[Tooltip("颜色在配置表的key值")]
		[SerializeReference]
		private string colorType = "Title1";

		[Tooltip("读取所填模块的ini")]
		public string moduleName;

		private string startType;

		[SerializeField]
		private string _key;

		public string Key
		{
			get { return _key; }
			set
			{
				if (_key != value)
				{
					_key = value;
					SetText();
				}
			}
		}

		public string ColorType
		{
			get { return colorType; }
			set
			{
				colorType = value;
				SetColor();
			}
		}

		[HideInInspector]
		public string ColorTypeDefault;

		[Tooltip("高亮颜色")]
		public string ColorTypeHighlight;

		private void Awake()
		{
			ColorTypeDefault = startType = colorType;
			if (Application.isPlaying)
			{
				SetColor();
			}
			SetText();
		}

		private void SetColor()
		{
			if (!IsThemeColor) return;
			if (string.IsNullOrWhiteSpace(ColorType)) return;
			if (ThemeMgr.CurrentTheme == null) return;
			if (ThemeMgr.ThemeColor == null || ThemeMgr.ThemeColor.Count == 0)
			{
				ThemeMgr.SetThemeColor();
				if (ThemeMgr.ThemeColor.Count == 0 || ThemeMgr.ThemeColor == null) return;
			}
			if (ThemeMgr.ThemeColor.Count == 0) return;
			if (ThemeMgr.ThemeColor.TryGetValue(ColorType, out ThemeMgr.ColorItem colorStr))
			{
				ColorUtility.TryParseHtmlString(colorStr.Color, out Color NowColor);
				if (GetComponent<TextPro>())
				{
					var CompTextPro = GetComponent<TextPro>();
					CompTextPro.color = NowColor;
				}
				else
				{
					var CompText = GetComponent<Text>();
					CompText.color = NowColor;
				}
			}
			else
			{
				Debug.LogWarning("未找到当前设置的颜色类型");
			}
		}

		private void SetText()
		{
			if (string.IsNullOrEmpty(_key)) return;
			string LocalText;
			if (string.IsNullOrWhiteSpace(moduleName))
			{
				if (isPlatform)
				{
					if (Application.isPlaying)
					{
						LocalText = "LobbyUI.Ins.GetPlatformText(_key)";
					}
					else
					{
						LocalText = LocalTextMgr.GetModuleValue(_key, App.SharedModule);
					}
				}
				else
				{
					LocalText = LocalTextMgr.GetValue(_key);
				}
			}
			else
			{
				LocalText = LocalTextMgr.GetModuleValue(_key, moduleName);
			}

			var CompText = GetComponent<TextPro>() ?? GetComponent<Text>();
			if (LocalText != CompText.text)
			{
				CompText.text = LocalText;
			}
			if (IsLinefeed)
			{
				CompText.text = CompText.text.Replace("\\n", "\n");
			}
			preferredWidth = CompText.preferredWidth;
			preferredHeight = CompText.preferredHeight;
		}

		private void ShowKey()
		{
			var CompText = GetComponent<TextPro>() ?? GetComponent<Text>();

			if (string.IsNullOrEmpty(_key)) return;

			string LocalText = $"[{_key}]";

			if (LocalText != CompText.text)
			{
				CompText.text = LocalText;
			}
		}

		private void RestoreText()
		{
			SetText();
		}

		private void Update()
		{
			if (!Application.isEditor || Application.isPlaying) return;
			if (colorType != startType)
			{
				SetColor();
				startType = colorType;
			}
			SetText();
		}
	}//class
}//namespace