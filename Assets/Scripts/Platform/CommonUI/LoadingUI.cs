using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WestBay
{
	public class LoadingUI : UIBase
	{
		private Image _imgLoading;
		private Image _imgBG;
		private bool _isShow = false;

		public LoadingUI(Transform transform)
		{
			InitComponent(transform);
		}

		private void InitComponent(Transform trans)
		{
			transform = trans;
			var subObj = transform.Find("Image_BG").gameObject;
			_imgBG = subObj.GetComponent<Image>();

			subObj = transform.Find("Image_Loading").gameObject;
			_imgLoading = subObj.GetComponent<Image>();
		}

		public void Show(bool isShow)
		{
			_isShow = isShow;

			transform.gameObject.SetActive(true);
			_imgLoading.gameObject.SetActive(false);
			if (isShow)
			{
				//Delay(3, () =>
				//{
				_imgLoading.gameObject.SetActive(true);
				//});
			}
			else
			{
				transform.gameObject.SetActive(false);
			}
		}
	}
}