using UnityEngine;
using UnityEngine.EventSystems;

namespace WestBay
{
	public class UIBase
	{
		public ThemeSet Theme { get; set; }

		public GameObject gameObject { get; set; }
		public Transform transform { get; set; }
		public RectTransform rectTransform { get; set; }

		protected virtual void OnPointEnter(BaseEventData eventData)
		{
		}

		protected virtual void OnPointExit(BaseEventData eventData)
		{
		}

		protected virtual void OnUpdate()
		{
		}

		public UIBase()
		{
		}

		/// <summary>
		/// 设置锚点
		/// </summary>
		/// <param name="vector"></param>
		public void SetPivot(Vector2 vector)
		{
			if (rectTransform == null) return;
			rectTransform.pivot = vector;
		}

		public void SetLocalPosition(Vector3 localPos)
		{
			if (transform == null) return;
			transform.localPosition = localPos;
		}

		/// <summary>
		/// 设置固定位置
		/// </summary>
		/// <param name="vector"></param>
		public void SetAnchoredPosition(Vector2 vector)
		{
			if (rectTransform == null) return;
			rectTransform.anchoredPosition = vector;
		}

		public void SetTransform(Transform trans, string goName = null)
		{
			transform = trans;
			gameObject = trans.gameObject;
			rectTransform = trans.GetComponent<RectTransform>();
			if (!string.IsNullOrEmpty(goName)) gameObject.name = goName;
		}

		/// <summary>
		/// 实例化预制体
		/// </summary>
		/// <param name="prefab">预制体Object</param>
		/// <param name="parent">父物体</param>
		/// <param name="goName">自定义的GameObject名字</param>
		/// <returns></returns>
		public GameObject SetTransform(UnityEngine.Object prefab, Transform parent, string goName = null)
		{
			gameObject = GameObject.Instantiate(prefab as GameObject, parent);
			transform = gameObject.transform;
			rectTransform = transform.GetComponent<RectTransform>();

			if (!string.IsNullOrEmpty(goName)) gameObject.name = goName;

			return gameObject;
		}

		/// <summary>
		/// 实例化GameObject
		/// </summary>
		/// <param name="prefab">GameObject</param>
		/// <param name="parent">父物体</param>
		/// <param name="goName">自定义的GameObject名字</param>
		/// <returns></returns>
		public GameObject SetTransform(GameObject prefab, Transform parent, string goName = null)
		{
			gameObject = NewObject(prefab, parent, goName);
			transform = gameObject.transform;
			rectTransform = transform.GetComponent<RectTransform>();
			return gameObject;
		}

		/// <summary>
		/// 实例化GameObject
		/// </summary>
		/// <param name="prefab">GameObject</param>
		/// <param name="parent">父物体</param>
		/// <param name="goName">自定义的GameObject名字</param>
		/// <returns></returns>
		public GameObject NewObject(GameObject prefab, Transform parent, string goName = null)
		{
			var item = GameObject.Instantiate(prefab, parent);
			if (!string.IsNullOrEmpty(goName)) item.name = goName;
			return item;
		}

		public void SetExpand(GameObject go)
		{
			var goRect = go.GetComponent<RectTransform>();
			goRect.anchorMin = Vector2.zero;
			goRect.anchorMax = Vector2.one;
			goRect.anchoredPosition = Vector2.zero;
			goRect.sizeDelta = Vector2.zero;
			goRect.pivot = Vector2.one;
		}
	}
}