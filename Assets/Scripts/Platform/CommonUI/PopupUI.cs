using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	public class PopupUI
	{
		public Transform Transform { get; set; }
		public Button OK { get; set; }
		public Button NO { get; set; }
		public Button YES { get; set; }
		public Button ScreenBtn { get; set; }
		public Text Text { get; set; }

		public void InitWithTransform(Transform transform)
		{
			Transform = transform;

			Text = GetUI(Transform, "Text").GetComponent<Text>();
			OK = GetUI(Transform, "ButtonOK").GetComponent<Button>();
			YES = GetUI(Transform, "ButtonYES").GetComponent<Button>();
			NO = GetUI(Transform, "ButtonNO").GetComponent<Button>();
			ScreenBtn = GetUI(Transform, "ScreenBtn").GetComponent<Button>();
			MonoHelper.AddEventTriggerEvent(OK, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(OK, EventTriggerType.PointerExit, HightCloseBtn);
			MonoHelper.AddEventTriggerEvent(YES, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(YES, EventTriggerType.PointerExit, HightCloseBtn);
			MonoHelper.AddEventTriggerEvent(NO, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(NO, EventTriggerType.PointerExit, HightCloseBtn);
			this.SaveStartImage();
		}

		public void RemoveListener()
		{
			YES.onClick.RemoveAllListeners();
			NO.onClick.RemoveAllListeners();
			OK.onClick.RemoveAllListeners();
			ScreenBtn.onClick.RemoveAllListeners();
		}

		public void AddListener(Action<LobbyUI.PopButton> onBtnClicked)
		{
			YES.onClick.AddListener(() =>
			{
				RemoveListener();
				Transform.gameObject.SetActive(false);
				onBtnClicked?.Invoke(LobbyUI.PopButton.YES);
				InitImage();
			});
			NO.onClick.AddListener(() =>
			{
				RemoveListener();
				Transform.gameObject.SetActive(false);
				onBtnClicked?.Invoke(LobbyUI.PopButton.NO);
				InitImage();
			});
			OK.onClick.AddListener(() =>
			{
				RemoveListener();
				Transform.gameObject.SetActive(false);
				onBtnClicked?.Invoke(LobbyUI.PopButton.OK);
				InitImage();
			});
			ScreenBtn.onClick.AddListener(() =>
			{
				RemoveListener();
				Transform.gameObject.SetActive(false);
				onBtnClicked?.Invoke(LobbyUI.PopButton.OK);
				InitImage();
			});
		}

		private Sprite YesBtn_n;
		private Sprite YesBtn_s;

		public void SaveStartImage()
		{
			Sprite btn_n = Array.Find<Sprite>(LobbyUI.Ins.DefaultRes, (s) => s.name == "common_btn_yes_n");
			Sprite btn_s = Array.Find<Sprite>(LobbyUI.Ins.DefaultRes, (s) => s.name == "common_btn_yes_s");
			YesBtn_n = btn_n;
			YesBtn_s = btn_s;
		}

		private void InitImage()
		{
			YES.transform.GetChild(0).GetComponent<Image>().sprite = YesBtn_n;
			SpriteState ss = YES.transform.GetChild(0).GetComponent<Selectable>().spriteState;
			ss.disabledSprite = YesBtn_s;
			YES.transform.GetChild(0).GetComponent<Selectable>().spriteState = ss;
			YES.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
		}

		private Transform GetUI(Transform ui, string name)
		{
			var Tr = ui.Find(name);
			return Tr;
		}

		#region 按钮移入移出高亮事件

		private void HightLightBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null && point.pointerEnter.transform.childCount != 0)
			{
				point.pointerEnter.transform.GetChild(0).GetComponent<Selectable>().interactable = false;
			}
		}

		private void HightCloseBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null && point.pointerEnter.transform.childCount != 0)
			{
				point.pointerEnter.transform.GetChild(0).GetComponent<Selectable>().interactable = true;
			}
		}

		#endregion 按钮移入移出高亮事件
	}
}