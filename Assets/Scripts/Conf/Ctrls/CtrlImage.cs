using System;
using UnityEngine;
using UnityEngine.UI;

namespace WestBay
{
	[ExecuteInEditMode]
	public class CtrlImage : MonoBehaviour
	{
		public Selectable SpriteStateComponent = null;

		public string SpriteName;

		//[HideInInspector]
		public string PicPath;

		/// <summary>
		/// 指定模块
		/// </summary>
		public string ModuleName;

		public string[] SpriteStateNames;

		//[HideInInspector]
		public string[] SpriteStatePicPath;

		private Image _img = null;

		#region Path

		private string Mod_S;
		private string AssetsS;
		private string PlatfromS;
		private string Mod_PlatformS;
		private string ResourcesS;
		private string DefaultS;
		private string LanguageS;
		private string DefaultLanguageS;

		/// <summary>
		/// Assets/Mod_
		/// </summary>
		private string JudgeResP;

		/// <summary>
		/// Assets/Mod_Platform
		/// </summary>
		private string Mod_PlatformP;

		/// <summary>
		/// /Resources
		/// </summary>
		private string ResourcesP;

		/// <summary>
		/// /Default/
		/// </summary>
		private string DefaultP;

		/// <summary>
		/// Resources/Default/
		/// </summary>
		private string DefaultP1;

		/// <summary>
		/// Assets/Mod_Platform/Resources/Default/
		/// </summary>
		private string DefaultP2;

		/// <summary>
		/// Resources/language/en/
		/// </summary>
		private string LanguageP1;

		/// <summary>
		/// Assets/Mod_Platform/Resources/language/en/
		/// </summary>
		private string LanguageP2;

		private void InitPath()
		{
			Mod_S = "Mod_";
			AssetsS = "Assets";
			PlatfromS = App.SharedModule;
			Mod_PlatformS = $"{App.ModNamePrefix}{App.SharedModule}";
			ResourcesS = "Resources";
			DefaultS = "default";
			LanguageS = "language";
			DefaultLanguageS = "en";

			JudgeResP = $"{AssetsS}/{Mod_S}";
			Mod_PlatformP = $"{AssetsS}/{Mod_PlatformS}";
			ResourcesP = $"/{ResourcesS}";
			DefaultP = $"/{DefaultS}/";
			DefaultP1 = $"{ResourcesS}/{DefaultS}/";
			DefaultP2 = $"{Mod_PlatformP}/{DefaultP1}";
			LanguageP1 = $"{ResourcesS}/{LanguageS}/{DefaultLanguageS}/";
			LanguageP2 = $"{Mod_PlatformP}/{LanguageP1}";
		}

		private string ModulePath(string moduleName, bool notDefault = false)
		{
			if (string.IsNullOrWhiteSpace(moduleName))
			{
				Debug.LogError("ModuleName is null");
				return null;
			}
			string defPath = $"{AssetsS}/{Mod_S}{moduleName}/{DefaultP1}";
			string lanPath = $"{AssetsS}/{Mod_S}{moduleName}/{LanguageP1}";
			return notDefault ? lanPath : defPath;
		}

		#endregion Path

		private void Awake()
		{
			InitPath();
			SetIPic();
		}

#if UNITY_EDITOR

		[ExecuteInEditMode]
		private void Update()
		{
			if (Application.isPlaying) return;
			SetIPic();
		}

#endif

		private void SetIPic()
		{
			if (string.IsNullOrEmpty(ThemeMgr.CurrentTheme)) return;
			_img = gameObject.GetComponent<Image>();
			if (_img == null && SpriteStateComponent == null) return;

			SetImg();

			SetSpriteSwap();
		}

		private string SetImagePath(Sprite sp)
		{
#if UNITY_EDITOR
			if (sp == null) return "";
			return UnityEditor.AssetDatabase.GetAssetPath(sp);
#else
            return "";
#endif
		}

		private UnityEngine.Object[] LoadSprites(string path)
		{
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(path)) return null;
			return UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
#else
            return null;
#endif
		}

		private string PicNameNormalize(string picName)
		{
			if (picName.IndexOf(ThemeMgr.ThemeSplitMarker) < picName.Length - ThemeMgr.ThemeSplitMarker.Length)
				return picName.Remove(picName.IndexOf(ThemeMgr.ThemeSplitMarker) + ThemeMgr.ThemeSplitMarker.Length);

			return picName;
		}

		private int FindChar(string str, string cha, int num)
		{
			int x = str.IndexOf("/");
			for (int i = 0; i < num - 1; i++)
			{
				x = str.IndexOf(cha, x + 1);
			}
			return x;
		}

		#region Image

		private void SetImg()
		{
			if (_img == null)
			{
				SpriteName = null;
				PicPath = null;
				return;
			}

			string picName = _img.mainTexture.name;
			if (!Application.isPlaying && picName.CompareTo("UnityWhite") == 0) return;
			string oriPicName = PicNameNormalize(picName);
#if UNITY_EDITOR

			if (!Application.isPlaying)
			{
				PicPath = SetImagePath(_img.sprite);
				if (string.IsNullOrEmpty(PicPath)) return;

				SpriteName = _img.sprite.name;
				string editorPicPath = PicPath;
				if (!editorPicPath.Contains(DefaultP))
				{
					int idx = FindChar(editorPicPath, "/", 4);
					editorPicPath = editorPicPath.Remove(idx, 3).Insert(idx + 1, $"{LocalMgr.CurrentCulture}/");
				}
				if (picName.Contains(ThemeMgr.ThemeSplitMarker))
				{
					if (ThemeMgr.CurrentTheme.CompareTo(ThemeMgr.DefaultTheme) == 0)
					{
						editorPicPath = editorPicPath.Replace(picName, $"{oriPicName}");
					}
					else
					{
						editorPicPath = editorPicPath.Replace(picName, $"{oriPicName}{ThemeMgr.CurrentTheme}");
					}
				}

				UnityEngine.Object[] newAltas = LoadSprites(editorPicPath);
				if (newAltas == null || newAltas.Length == 0)
				{
					Debug.LogError($"未找到当前主题图集，此图片的引用位于场景内的{name}物体上。建议检查一下图集路径下是否存在与当前主题{ThemeMgr.CurrentTheme}和语言{LocalMgr.CurrentCulture}对应的图集文件{picName}。注：{DefaultS}路径不用考虑语言");
					return;
				}

				Sprite newOne = null;
				if (newAltas.Length == 2)
				{
					newOne = newAltas[1] as Sprite;
				}
				else
				{
					newOne = Array.Find(newAltas, (s) => s.name == SpriteName) as Sprite;
				}

				if (newOne == null)
				{
					Debug.LogError($"当前主题没这个图片，此图片的引用位于场景内的{name}物体上。建议检查一下各个主题的{picName}图集下的子图{SpriteName}是否正确对应，否则可能导致风格切换错误。");
					return;
				}
				_img.sprite = newOne;
				return;
			}

#endif

			if (string.IsNullOrEmpty(PicPath)) return;
			if (!PicPath.Contains(JudgeResP)) return;
			string moduleName = ModuleName;
			Sprite[] abNewAltas;

			if (PicPath.Contains(Mod_PlatformP))
			{
				moduleName = PlatfromS;
				if (PicPath.Contains(DefaultP1))
				{
					PicPath = PicPath.Replace(DefaultP2, "").ToLower();
					abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, PicPath);
				}
				else
				{
					PicPath = PicPath.Replace(LanguageP2, "").ToLower();
					abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, PicPath, true);
				}
			}
			else
			{
				//无指定模块时，获取当前模块
				if (string.IsNullOrEmpty(moduleName)) moduleName = Lobby.Ins.GetThisModuleName();
				if (PicPath.Contains(DefaultP1))
				{
					PicPath = PicPath.Replace(ModulePath(moduleName), "").ToLower();
					abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, PicPath);
				}
				else
				{
					PicPath = PicPath.Replace(ModulePath(moduleName, true), "").ToLower();
					abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, PicPath, true);
				}
			}

			if (abNewAltas == null) return;

			Sprite abNewOne = null;
			if (abNewAltas.Length == 1)
			{
				abNewOne = abNewAltas[0] as Sprite;
			}
			else
			{
				abNewOne = Array.Find(abNewAltas, (t) => t.name == SpriteName);
			}

			if (abNewOne == null) return;
			_img.sprite = abNewOne;

			return;
		}

		#endregion Image

		#region SpriteState

		private void SetSpriteSwap(int idx, Sprite sp)
		{
			SpriteStatePicPath[idx] = SetImagePath(sp);
			if (string.IsNullOrEmpty(SpriteStatePicPath[idx]))
			{
				SpriteStateNames[idx] = "";
				return;
			}
			SpriteStateNames[idx] = sp.name;
		}

		private void SetSpriteSwap()
		{
			if (SpriteStateComponent == null)
			{
				SpriteStateNames = null;
				SpriteStatePicPath = null;
				return;
			}
			if (SpriteStateComponent.transition != Selectable.Transition.SpriteSwap) return;

			SpriteState SS = SpriteStateComponent.spriteState;
			string[] picName = new string[3];
			string[] oriPicName = new string[3];

			for (int i = 0; i < 3; i++)
			{
				switch (i)
				{
					case 0:
						picName[i] = SS.highlightedSprite == null ? "" : SS.highlightedSprite.texture.name;
						break;

					case 1:
						picName[i] = SS.pressedSprite == null ? "" : SS.pressedSprite.texture.name;
						break;

					case 2:
						picName[i] = SS.disabledSprite == null ? "" : SS.disabledSprite.texture.name;
						break;
				}

				oriPicName[i] = PicNameNormalize(picName[i]);

#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					if (SpriteStateNames == null || SpriteStateNames.Length == 0) SpriteStateNames = new string[3];
					if (SpriteStatePicPath == null || SpriteStatePicPath.Length == 0) SpriteStatePicPath = new string[3];
					switch (i)
					{
						case 0:
							SetSpriteSwap(0, SS.highlightedSprite);
							break;

						case 1:
							SetSpriteSwap(1, SS.pressedSprite);
							break;

						case 2:
							SetSpriteSwap(2, SS.disabledSprite);
							break;
					}

					string editorPicPath = SpriteStatePicPath[i];
					if (string.IsNullOrEmpty(editorPicPath)) continue;
					if (!editorPicPath.Contains(DefaultP))
					{
						int idx = FindChar(editorPicPath, "/", 3);
						editorPicPath = editorPicPath.Remove(idx, 3).Insert(idx + 1, $"{LocalMgr.CurrentCulture}/");
					}
					if (editorPicPath.Contains(ThemeMgr.ThemeSplitMarker))
					{
						if (ThemeMgr.CurrentTheme.CompareTo(ThemeMgr.DefaultTheme) == 0)
						{
							editorPicPath = editorPicPath.Replace(picName[i], $"{oriPicName[i]}");
						}
						else
						{
							editorPicPath = editorPicPath.Replace(picName[i], $"{oriPicName[i]}{ThemeMgr.CurrentTheme}");
						}
					}
					UnityEngine.Object[] newAltas = LoadSprites(editorPicPath);

					if (newAltas == null || newAltas.Length == 0)
					{
						Debug.LogError($"未找到当前主题图集，此图片的引用位于场景内{name}物体的Selectable组件上。建议检查一下图集路径下是否存在与当前主题{ThemeMgr.CurrentTheme}和语言{LocalMgr.CurrentCulture}对应的图集文件{picName}。注：default路径不用考虑语言");
						return;
					}

					Sprite newOne = null;
					if (newAltas.Length == 2)
					{
						newOne = newAltas[1] as Sprite;
					}
					else
					{
						newOne = Array.Find(newAltas, (s) => s.name == SpriteStateNames[i]) as Sprite;
					}

					if (newOne == null)
					{
						Debug.LogError($"当前主题没这个图片，此图片的引用位于场景内{name}物体的Selectable组件上。建议检查一下各个主题的{picName}图集下的子图{SpriteName}是否正确对应，否则可能导致风格切换错误。");
						return;
					}

					switch (i)
					{
						case 0:
							SS.highlightedSprite = newOne;

							break;

						case 1:
							SS.pressedSprite = newOne;
							break;

						case 2:
							SS.disabledSprite = newOne;
							break;
					}

					continue;
				}
#endif

				if (string.IsNullOrEmpty(SpriteStatePicPath[i])) continue;
				if (!SpriteStatePicPath[i].Contains(JudgeResP)) continue;
				string moduleName = "";
				Sprite[] abNewAltas;

				if (SpriteStatePicPath[i].Contains(Mod_PlatformP))
				{
					moduleName = PlatfromS;
					if (SpriteStatePicPath[i].Contains(DefaultP1))
					{
						SpriteStatePicPath[i] = SpriteStatePicPath[i].Replace(DefaultP2, "").ToLower();
						abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, SpriteStatePicPath[i]);
					}
					else
					{
						SpriteStatePicPath[i] = SpriteStatePicPath[i].Replace(LanguageP2, "").ToLower();
						abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, SpriteStatePicPath[i], true);
					}
				}
				else
				{
					moduleName = Lobby.Ins.GetThisModuleName();
					if (SpriteStatePicPath[i].Contains(DefaultP1))
					{
						SpriteStatePicPath[i] = SpriteStatePicPath[i].Replace(ModulePath(moduleName), "").ToLower();
						abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, SpriteStatePicPath[i]);
					}
					else
					{
						SpriteStatePicPath[i] = SpriteStatePicPath[i].Replace(ModulePath(moduleName, true), "").ToLower();
						abNewAltas = ResourceLoader.Ins.SpritesGet(moduleName, SpriteStatePicPath[i], true);
					}
				}
				Sprite abNewOne = null;
				if (abNewAltas.Length == 1)
				{
					abNewOne = abNewAltas[0] as Sprite;
				}
				else
				{
					abNewOne = Array.Find(abNewAltas, (t) => t.name == SpriteStateNames[i]);
				}
				if (abNewOne == null) continue;

				switch (i)
				{
					case 0:
						SS.highlightedSprite = abNewOne;
						break;

					case 1:
						SS.pressedSprite = abNewOne;
						break;

					case 2:
						SS.disabledSprite = abNewOne;
						break;
				}
			}

			SpriteStateComponent.spriteState = SS;
		}

		#endregion SpriteState
	}
}