using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	public class FlyToggle : Toggle
	{
		private bool _isSelected;
		private bool _isPointEnter;
		private bool _interactable;
		private bool _isBreakPointExit;

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
				if (_uiFollow)
				{
					OnSetProperty();
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
				SetSubUI(isOn ? 1 : 0);
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
		private bool _isCheckmarkFollow;

		public bool IsCheckmarkFollow
		{
			get { return _isCheckmarkFollow; }
			set { _isCheckmarkFollow = value; }
		}

		[SerializeField]
		private Sprite _checkmarkHighlightedSprite;

		public Sprite CheckmarkHighlightedSprite
		{
			get { return _checkmarkHighlightedSprite; }
			set { _checkmarkHighlightedSprite = value; }
		}

		[SerializeField]
		private Sprite _checkmarkPressedSprite;

		public Sprite CheckmarkPressedSprite
		{
			get { return _checkmarkPressedSprite; }
			set { _checkmarkPressedSprite = value; }
		}

		protected override void InstantClearState()
		{
			base.InstantClearState();
			_isSelected = false;
			_isPointEnter = false;
			_interactable = interactable;
			_isBreakPointExit = false;
		}

		public void OnSetProperty()
		{
			if (!gameObject.activeInHierarchy) return;

			Color tintColor;
			string tintStringColor;
			Sprite transitionSprite;
			switch (currentSelectionState)
			{
				case SelectionState.Normal:
					tintColor = isOn ? _subColors.selectedColor : _subColors.normalColor;
					tintStringColor = isOn ? _subStringColors.SelectedColor : _subStringColors.NormalColor;
					transitionSprite = isOn ? _subSpriteState.selectedSprite : null;
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

		private void DoCheckmaskSwap(Sprite newSprite)
		{
			if (!_isCheckmarkFollow || graphic == null) return;

			((Image)graphic).overrideSprite = newSprite;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SetSubUI(isOn ? 1 : 0);
			onValueChanged.AddListener(OnToggleValueChange);
		}

		private void OnToggleValueChange(bool isOn)
		{
			if (!interactable) return;
			SetSubUI(isOn ? 1 : 0);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			onValueChanged.RemoveAllListeners();
		}

		private void DoTextSwap(Color newColor)
		{
			if (_subText == null) return;

			_subText.color = newColor;
		}

		private void DoCtrlTextSwap(string newColor)
		{
			if (_subCtrlText == null) return;

			_subCtrlText.ColorType = newColor;
		}

		private void SetSubUI(int state)
		{
			if ((state == 0 && !_isPointEnter) || !_uiFollow)
			{
				DoSpriteSwap(null);
				if (_isCtrlTextControl)
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
				if (_isCtrlTextControl)
				{
					DoCtrlTextSwap(_subStringColors.HighlightedColor);
				}
				else
				{
					DoTextSwap(_subColors.highlightedColor);
				}
			}
		}

		public void OnEditorValueChanged()
		{
			if (_uiFollow)
			{
				SetSubUI(isOn ? 1 : 0);
				if (_interactable != interactable)
				{
					_interactable = interactable;
					OnSetProperty();
				}
			}
			else
			{
				SetSubUI(0);
			}
		}

		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (!interactable) return;

			if (_checkmarkHighlightedSprite != null) DoCheckmaskSwap(_checkmarkHighlightedSprite);
			base.OnPointerEnter(eventData);
			_isPointEnter = true;
			SetSubUI(1);
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			if (_isBreakPointExit)
			{
				_isBreakPointExit = false;
				return;
			}
			if (!interactable || (!eventData.fullyExited && eventData.pointerCurrentRaycast.gameObject)) return;

			DoCheckmaskSwap(null);
			base.OnPointerExit(eventData);
			_isPointEnter = false;
			if (isOn || _isSelected) return;

			SetSubUI(0);
		}

		public override void OnSelect(BaseEventData eventData)
		{
			if (!interactable) return;

			base.OnSelect(eventData);
			_isSelected = true;
			SetSubUI(1);
		}

		public override void OnDeselect(BaseEventData eventData)
		{
			if (!interactable) return;

			base.OnDeselect(eventData);
			_isSelected = false;
			if (!isOn)
			{
				SetSubUI(0);
			}
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			if (!interactable) return;

			if (isOn && _checkmarkHighlightedSprite != null) DoCheckmaskSwap(_checkmarkPressedSprite);
			base.OnPointerDown(eventData);
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			if (!interactable) return;

			if (isOn) DoCheckmaskSwap(_checkmarkHighlightedSprite);
			base.OnPointerUp(eventData);
		}

		public void SetBreakPointExit()
		{
			_isBreakPointExit = true;
		}
	}
}