using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	public class FlyButton : Button
	{
		private bool _isSelected { get; set; }

		[SerializeField]
		private float _clickTime;

		public float ClickTime
		{
			get
			{
				return _clickTime;
			}
			set
			{
				_clickTime = value;
			}
		}

		private float _countdown { get; set; }

		[SerializeField]
		private bool _uiFollow;

		public bool UIFollow
		{
			get { return _uiFollow; }
			set
			{
				_uiFollow = value;
				if (_uiFollow)
				{
					OnSetProperty();
				}
				else
				{
					SetSubUI(0);
				}
			}
		}

		[SerializeField]
		private bool _isCtrlTextControl;

		public bool IsCtrlTextControl
		{
			get { return _isCtrlTextControl; }
			set
			{
				_isCtrlTextControl = value;
				if (_subCtrlText != null) _subCtrlText.enabled = value;
				if (!_uiFollow) return;
				if (_isCtrlTextControl)
				{
					DoCtrlTextSwap(_subStringColors.NormalColor);
				}
				else
				{
					DoTextSwap(_subColors.normalColor);
				}
			}
		}

		[SerializeField]
		private Text _subText;

		public Text SubText
		{
			get
			{
				return _subText;
			}
			set
			{
				_subText = value;
			}
		}

		[SerializeField]
		private ColorBlock _subColors = ColorBlock.defaultColorBlock;

		public ColorBlock SubColors
		{
			get
			{
				return _subColors;
			}
			set
			{
				_subColors = value;
			}
		}

		[SerializeField]
		private CtrlText _subCtrlText;

		public CtrlText SubCtrlText
		{
			get
			{
				return _subCtrlText;
			}
			set
			{
				_subCtrlText = value;
			}
		}

		[SerializeField]
		private StringColorBlock _subStringColors = StringColorBlock.DefaultStringColorBlock;

		public StringColorBlock SubStringColors
		{
			get { return _subStringColors; }
			set { _subStringColors = value; }
		}

		[SerializeField]
		private Image _subImage;

		public Image SubImage
		{
			get { return _subImage; }
			set { _subImage = value; }
		}

		[SerializeField]
		private SpriteState _subSpriteState;

		public SpriteState SubSpriteState
		{
			get
			{
				return _subSpriteState;
			}
			set
			{
				_subSpriteState = value;
				OnSetProperty();
			}
		}

		private void OnSetProperty()
		{
			if (!gameObject.activeInHierarchy)
				return;

			Color tintColor;
			string tintStringColor;
			Sprite transitionSprite;
			switch (currentSelectionState)
			{
				case SelectionState.Normal:
					tintColor = _subColors.normalColor;
					tintStringColor = _subStringColors.NormalColor;
					transitionSprite = null;
					break;

				case SelectionState.Highlighted:
					tintColor = _subColors.highlightedColor;
					tintStringColor = _subStringColors.HighlightedColor;
					transitionSprite = _subSpriteState.highlightedSprite;
					break;

				case SelectionState.Pressed:
					tintColor = _subColors.pressedColor;
					tintStringColor = _subStringColors.PressedColor;
					transitionSprite = _subSpriteState.pressedSprite;
					break;

				case SelectionState.Selected:
					tintColor = _subColors.selectedColor;
					tintStringColor = _subStringColors.SelectedColor;
					transitionSprite = _subSpriteState.selectedSprite;
					break;

				case SelectionState.Disabled:
					tintColor = _subColors.disabledColor;
					tintStringColor = _subStringColors.DisabledColor;
					transitionSprite = _subSpriteState.disabledSprite;
					break;

				default:
					tintColor = Color.white;
					tintStringColor = "Title3";
					transitionSprite = null;
					break;
			}

			DoSpriteSwap(transitionSprite);
			if (_isCtrlTextControl)
			{
				DoCtrlTextSwap(tintStringColor);
			}
			else
			{
				DoTextSwap(tintColor);
			}
		}

		private void DoSpriteSwap(Sprite newSprite)
		{
			if (_subImage == null) return;

			_subImage.overrideSprite = newSprite;
		}

		private void DoTextSwap(Color newColor)
		{
			if (_subText == null) return;

			_subText.color = newColor;
		}

		private void DoCtrlTextSwap(string newColor)
		{
			if (_subCtrlText == null || !_uiFollow) return;

			_subCtrlText.ColorType = newColor;
		}

		private void SetSubUI(int state)
		{
			if (state == 0 || !_uiFollow)
			{
				DoSpriteSwap(null);
				if (IsCtrlTextControl)
				{
					DoCtrlTextSwap(_subStringColors.NormalColor);
				}
				else
				{
					DoTextSwap(_subColors.normalColor);
				}
			}
			else
			{
				DoSpriteSwap(_subSpriteState.highlightedSprite ?? null);
				if (IsCtrlTextControl)
				{
					DoCtrlTextSwap(_subStringColors.HighlightedColor);
				}
				else
				{
					DoTextSwap(_subColors.highlightedColor);
				}
			}
		}

		protected override void InstantClearState()
		{
			base.InstantClearState();
			SetSubUI(0);
			_isSelected = false;
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			SetSubUI(1);
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			if (_isSelected) return;

			SetSubUI(0);
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			if (_countdown > 0) return;

			base.OnPointerClick(eventData);
			if (_clickTime > 0)
			{
				StartCoroutine(OnClickCountDown());
			}
		}

		private IEnumerator OnClickCountDown()
		{
			_countdown = _clickTime;

			while (_countdown > 0)
			{
				_countdown -= Time.unscaledDeltaTime;
				yield return null;
			}

			yield break;
		}

		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			_isSelected = true;
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			base.OnDeselect(eventData);
			_isSelected = false;
			SetSubUI(0);
		}

		public override void OnSubmit(BaseEventData eventData)
		{
			if (_countdown > 0) return;

			base.OnSubmit(eventData);
			if (_clickTime > 0)
			{
				StartCoroutine(OnClickCountDown());
			}
		}
	}
}