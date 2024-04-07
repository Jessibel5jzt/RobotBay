using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WestBay
{
	public class InitialUpdateUI
	{
		private GameObject _info;
		private Slider _slider;
		private Text _updateMessage;
		private Text _updateProgress;

		public InitialUpdateUI()
		{
			InitComponent();
		}

		private void InitComponent()
		{
			_info = GameObject.Find("Canvas/Bg/Info");
			_slider = _info.transform.Find("Slider").GetComponent<Slider>();
			_updateMessage = _info.transform.Find("UpdateMessage").GetComponent<Text>();
			_updateProgress = _info.transform.Find("Progress").GetComponent<Text>();
		}

		public void ShowUI(bool isShow)
		{
			_info.gameObject.SetActive(isShow);
		}

		public void Begin(string msg = "", bool showProgress = false, bool isFade = true)
		{
			_updateProgress.gameObject.SetActive(showProgress);
			_updateMessage.gameObject.SetActive(true);
			_slider.gameObject.SetActive(true);

			if (isFade) _updateMessage.DOFade(0.3f, 2).SetLoops(-1, LoopType.Yoyo);
			SetProgress(0);
			SetMessage(msg);
		}

		public void ShowSlider(bool isShow)
		{
			_slider.gameObject.SetActive(isShow);
		}

		public void SetProgress(float progress)
		{
			_slider.value = progress;
			_updateProgress.text = $"{progress:f1}%";
		}

		public void SetMessage(string message)
		{
			_updateMessage.text = message;
		}

		public void Finish()
		{
			SetProgress(0f);
			_updateProgress.gameObject.SetActive(false);
			_updateMessage.gameObject.SetActive(false);
			_slider.gameObject.SetActive(false);
		}
	}
}