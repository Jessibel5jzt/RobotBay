using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	public class UpdateUI
	{
		//UI
		private Transform _updatePanel;

		private Transform _mainParent;

		//更新流程信息
		private Transform _updateInfo;

		private Slider _progress_UpdateInfo_Slider;
		private Text _progress_SliderText;
		private Text _progress_UpdateInfo;
		private Text _title;

		//更新结果
		private Transform _updateResult;

		private Button _button1;
		private Button _button2;
		private Text _result;

		private Selectable _b1;
		private Selectable _b2;

		private Transform _updateFinish;
		private Button _button3;
		private Selectable _b3;
		private Text _finishResult;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="panel"></param>
		public void InitPanel(Transform panel)
		{
			_updatePanel = panel;
			UIComponent();
		}

		private void UIComponent()
		{
			_mainParent = _updatePanel.Find("Parent");
			_updateInfo = _mainParent.Find("UpdateInfo");
			_progress_UpdateInfo_Slider = _updateInfo.transform.Find("Slider").GetComponent<Slider>();
			_progress_UpdateInfo_Slider.value = 0;
			_progress_SliderText = _progress_UpdateInfo_Slider.transform.Find("Fill Area/Fill/Image/Progress").GetComponent<Text>();
			_progress_UpdateInfo = _updateInfo.transform.Find("Progress").GetComponent<Text>();
			_progress_UpdateInfo.text = "";
			_title = _updateInfo.Find("Title").GetComponent<Text>();

			_updateResult = _mainParent.Find("UpdateResult");
			_button1 = _updateResult.transform.Find("Button_1").GetComponent<Button>();
			_button2 = _updateResult.transform.Find("Button_2").GetComponent<Button>();
			_b1 = _button1.transform.GetChild(0).GetComponent<Selectable>();
			_b2 = _button2.transform.GetChild(0).GetComponent<Selectable>();
			_result = _updateResult.Find("Title").GetComponent<Text>();

			_updateFinish = _mainParent.Find("UpdateFinish");
			_button3 = _updateFinish.transform.Find("Button_3").GetComponent<Button>();
			_b3 = _button3.transform.GetChild(0).GetComponent<Selectable>();
			_finishResult = _updateFinish.Find("TitleFinish").GetComponent<Text>();

			MonoHelper.AddEventTriggerEvent(_button1, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(_button1, EventTriggerType.PointerExit, HightCloseBtn);
			MonoHelper.AddEventTriggerEvent(_button2, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(_button2, EventTriggerType.PointerExit, HightCloseBtn);
			MonoHelper.AddEventTriggerEvent(_button3, EventTriggerType.PointerEnter, HightLightBtn);
			MonoHelper.AddEventTriggerEvent(_button3, EventTriggerType.PointerExit, HightCloseBtn);
		}

		public void UpdateProgress(float progress)
		{
			if (!_updateInfo.gameObject.activeInHierarchy) return;
			_progress_UpdateInfo_Slider.value = progress;
			_progress_UpdateInfo.text = $"{progress:f1}%";
			_progress_SliderText.text = $"{progress:f1}%";
		}

		public void UpdateInfo(string msg)
		{
			if (!_updateInfo.gameObject.activeInHierarchy) return;
			_title.text = msg;
		}

		public void UpdateDownloadInfo(string msg)
		{
			if (!_updateInfo.gameObject.activeInHierarchy) return;
			_title.text = $"{LobbyUI.Ins.GetPlatformText("Downloading")}\n{msg}";
		}

		public void InitUpdateUI()
		{
			if (_updatePanel == null)
			{
				var transform = GameObject.Instantiate(ResourceLoader.Ins.PrefabGet(LobbyUI.SharedModule, "UpdatePanel") as GameObject, LobbyUI.Ins.DefaultUI).transform;
				transform.SetSiblingIndex(LobbyUI.Ins.DefaultUI.childCount - 3);
				InitPanel(transform);
			}
		}

		public void OpenUpdateUI(int type = 0)
		{
			InitUpdateUI();
			if (type == 0)
			{
				_title.text = LobbyUI.Ins.GetPlatformText("Updating");
			}

			//打开界面
			_updatePanel.gameObject.SetActive(true);
			_updateInfo.gameObject.SetActive(true);
			_updateResult.gameObject.SetActive(false);
			_updateFinish.gameObject.SetActive(false);
		}

		public void Retry()
		{
			InitUpdateUI();
			_updateInfo.gameObject.SetActive(true);
			_updateResult.gameObject.SetActive(false);
		}

		public void ClosedUpdateUI()
		{
			if (_updatePanel != null)
			{
				_updatePanel.gameObject.SetActive(false);

				GameObject.Destroy(_updatePanel.gameObject);
				_updatePanel = null;
			}
		}

		public void ShowUpdateError(string msg = null, UnityAction btn1Callback = null, UnityAction btn2Callback = null)
		{
			Debug.Log(msg);
			_updateInfo.gameObject.SetActive(false);
			_updateResult.gameObject.SetActive(true);
			if (msg == null)
				_result.text = LobbyUI.Ins.GetPlatformText("DownError");
			else
				_result.text = msg;

			if (btn1Callback != null)
			{
				_button1.onClick.RemoveAllListeners();
				_button1.onClick.AddListener(btn1Callback);
			}
			if (btn2Callback != null)
			{
				_button2.onClick.RemoveAllListeners();
				_button2.onClick.AddListener(btn2Callback);
			}
			_b1.interactable = true;
			_b2.interactable = true;
		}

		public void UpdateFinish(UnityAction finishCallback = null, string msg = null)
		{
			_updateInfo.gameObject.SetActive(false);
			_updateFinish.gameObject.SetActive(true);

			if (msg == null)
			{
				_finishResult.text = string.Empty;
			}

			if (finishCallback != null)
			{
				MB.Ins.StartCoroutine(FinishCountDown(3, msg, finishCallback));
			}
		}

		private IEnumerator FinishCountDown(int time, string msg, UnityAction action = null)
		{
			while (time > 0)
			{
				_finishResult.text = string.Format(msg, time);
				yield return new WaitForSeconds(1f);
				time -= 1;
			}
			ClosedUpdateUI();
			action?.Invoke();
		}

		#region 按钮移入移出高亮事件

		private void HightLightBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null)
			{
				OnPointerEnter(point.pointerEnter.gameObject.name);
			}
		}

		private void HightCloseBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null)
			{
				OnPointerExit(point.pointerEnter.gameObject.name);
			}
		}

		private void OnPointerEnter(string name)
		{
			switch (name)
			{
				case "Button_1":
					_b1.interactable = false;
					break;

				case "Button_2":
					_b2.interactable = false;
					break;

				case "Button_3":
					_b3.interactable = false;
					break;

				default:
					break;
			}
		}

		private void OnPointerExit(string name)
		{
			switch (name)
			{
				case "Button_1":
					_b1.interactable = true;
					break;

				case "Button_2":
					_b2.interactable = true;
					break;

				case "Button_3":
					_b3.interactable = true;
					break;

				default:
					break;
			}
		}

		#endregion 按钮移入移出高亮事件
	}
}