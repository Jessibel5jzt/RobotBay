using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	/// <summary>
	/// Mono相关帮助类
	/// </summary>
	public class MonoHelper
	{
		/// <summary>
		/// 屏幕外位置，减少SetActive消耗
		/// </summary>
		public static Vector3 OutScreenPostion = new Vector3(10000, 10000, 10000);

		#region EventTrigger事件

		/// <summary>
		/// Add object trigger event
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="eventTriggerType"></param>
		/// <param name="callback"></param>
		public static void AddEventTriggerEvent(Component obj, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback = null)
		{
			EventTrigger.Entry entry = null;
			EventTrigger trigger = obj.GetComponent<EventTrigger>();

			if (trigger != null) // 已有EventTrigger
			{
				// 查找是否已经存在要注册的事件
				foreach (EventTrigger.Entry existingEntry in trigger.triggers)
				{
					if (existingEntry.eventID == eventTriggerType)
					{
						entry = existingEntry;
						break;
					}
				}
			}
			else
			{
				trigger = obj.gameObject.AddComponent<EventTrigger>();
			}

			// 如果这个事件不存在，就创建新的实例
			if (entry == null)
			{
				entry = new EventTrigger.Entry();
				entry.eventID = eventTriggerType;
				entry.callback = new EventTrigger.TriggerEvent();
			}

			if (callback == null)
			{
				if (eventTriggerType == EventTriggerType.PointerEnter)
				{
					entry.callback.AddListener(HightLightBtn);
				}
				else if (eventTriggerType == EventTriggerType.PointerExit)
				{
					entry.callback.AddListener(HightCloseBtn);
				}
			}
			else
			{
				entry.callback.AddListener(callback);
			}

			trigger.triggers.Add(entry);
		}

		/// <summary>
		/// Remove object trigger event
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="eventTriggerType"></param>
		/// <param name="callback"></param>
		public static void RemoveEventTriggerEvent(Component obj, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback = null)
		{
			EventTrigger trigger = obj.GetComponent<EventTrigger>();
			if (trigger != null)
			{
				foreach (EventTrigger.Entry existingEntry in trigger.triggers)
				{
					if (existingEntry.eventID == eventTriggerType)
					{
						if (callback == null)
						{
							existingEntry.callback.RemoveAllListeners();
						}
						else
						{
							existingEntry.callback.RemoveListener(callback);
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Clear all object trigger event
		/// </summary>
		/// <param name="obj"></param>
		public static void ClearAllTriggerEvent(Component obj)
		{
			EventTrigger trigger = obj.GetComponent<EventTrigger>();
			if (trigger != null)
			{
				foreach (EventTrigger.Entry existingEntry in trigger.triggers)
				{
					existingEntry.callback.RemoveAllListeners();
				}
				trigger.triggers.Clear();
			}
		}

		/// <summary>
		/// 添加按钮移入事件
		/// </summary>
		/// <param name="arg0"></param>
		private static void HightLightBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null && point.pointerEnter.transform.childCount > 0)
			{
				point.pointerEnter.transform.GetChild(0).GetComponent<Selectable>().interactable = false;
			}
		}

		/// <summary>
		/// 删除移入高亮事件
		/// </summary>
		/// <param name="arg0"></param>
		private static void HightCloseBtn(BaseEventData arg0)
		{
			PointerEventData point = arg0 as PointerEventData;
			if (point.pointerEnter.gameObject != null && point.pointerEnter.transform.childCount > 0)
			{
				point.pointerEnter.transform.GetChild(0).GetComponent<Selectable>().interactable = true;
			}
		}

		#endregion EventTrigger事件

		#region 控件唤醒，移入，移出

		/// <summary>
		/// 从代码唤醒事件
		/// </summary>
		/// <param name="InvokeObj"></param>
		public static bool InvokeEvent(GameObject InvokeObj)
		{
			if (InvokeObj == null) return false;
			if (EventSystem.current == null) return false;
			if (!IsEventObjectFirst(InvokeObj)) return false;

			bool result = false;
			try
			{
				var eventData = new PointerEventData(EventSystem.current)
				{
					pointerEnter = InvokeObj,
					pointerPress = InvokeObj
				};

				if (InvokeObj.GetComponent<EventTrigger>() != null)
				{
					result = ExecuteEvents.Execute<IPointerClickHandler>(InvokeObj, eventData, ExecuteEvents.pointerClickHandler);
				}
				else if (InvokeObj.GetComponent<Button>() != null)
				{
					EventSystem.current.SetSelectedGameObject(InvokeObj);
					result = ExecuteEvents.Execute<ISubmitHandler>(InvokeObj, eventData, ExecuteEvents.submitHandler);
					EventSystem.current.SetSelectedGameObject(null);
				}
				else if (InvokeObj.GetComponent<Dropdown>() != null)
				{
					result = ExecuteEvents.Execute<IDropHandler>(InvokeObj, eventData, ExecuteEvents.dropHandler);
				}
				else if (InvokeObj.GetComponent<PointClickHandler>() != null)
				{
					result = ExecuteEvents.Execute<IPointerClickHandler>(InvokeObj, eventData, ExecuteEvents.pointerClickHandler);
				}
				else
				{
					result = ExecuteEvents.Execute<ISubmitHandler>(InvokeObj, eventData, ExecuteEvents.submitHandler);
				}
			}
			catch (Exception e)
			{
				Debug.Log($"InvokeEvent exception {e.ToString()}");
			}

			return result;
		}

		/// <summary>
		/// 控件移入
		/// </summary>
		/// <param name="invokeObj"></param>
		public static void PointerEnterEvent(GameObject invokeObj, GameObject pointerObj)
		{
			if (invokeObj == null) return;

			var eventData = new PointerEventData(EventSystem.current)
			{
				pointerEnter = pointerObj,
				position = pointerObj.transform.position,
			};

			if (invokeObj.GetComponent<EventTrigger>() != null)
			{
				invokeObj.GetComponent<EventTrigger>().OnPointerEnter(eventData);
			}

			if (invokeObj.GetComponent<Button>() != null)
			{
				invokeObj.GetComponent<Button>().OnPointerEnter(eventData);
			}

			if (invokeObj.GetComponent<Dropdown>() != null)
			{
				invokeObj.GetComponent<Dropdown>().OnPointerEnter(eventData);
			}

			if (invokeObj.GetComponent<InputField>() != null)
			{
				invokeObj.GetComponent<InputField>().OnPointerEnter(eventData);
			}

			if (invokeObj.GetComponent<Toggle>() != null)
			{
				invokeObj.GetComponent<Toggle>().OnPointerEnter(eventData);
			}
			if (invokeObj.GetComponent<PointEnterHandler>() != null)
			{
				invokeObj.GetComponent<PointEnterHandler>().OnPointerEnter(eventData);
				return;
			}
		}

		/// <summary>
		/// 控件移出
		/// </summary>
		/// <param name="invokeObj"></param>
		/// <param name="pointerObj"></param>
		public static void PointExitEvent(GameObject invokeObj, GameObject pointerObj)
		{
			if (invokeObj == null) return;

			var eventData = new PointerEventData(EventSystem.current)
			{
				pointerEnter = pointerObj,
				pointerPress = pointerObj,
				position = pointerObj.transform.position,
			};

			if (invokeObj.GetComponent<EventTrigger>() != null)
			{
				invokeObj.GetComponent<EventTrigger>().OnPointerExit(eventData);
			}

			if (invokeObj.GetComponent<Button>() != null)
			{
				invokeObj.GetComponent<Button>().OnPointerExit(eventData);
				invokeObj.GetComponent<Button>().OnDeselect(eventData);
			}

			if (invokeObj.GetComponent<Dropdown>() != null)
			{
				invokeObj.GetComponent<Dropdown>().OnPointerExit(eventData);
			}

			if (invokeObj.GetComponent<InputField>() != null)
			{
				invokeObj.GetComponent<InputField>().OnPointerExit(eventData);
			}

			if (invokeObj.GetComponent<Toggle>() != null)
			{
				invokeObj.GetComponent<Toggle>().OnPointerExit(eventData);
			}
			if (invokeObj.GetComponent<PointExitHandler>() != null)
			{
				invokeObj.GetComponent<PointExitHandler>().OnPointerExit(eventData);
				return;
			}
		}

		/// <summary>
		/// 是否是第一个事件对象
		/// </summary>
		/// <param name="eventObj"></param>
		/// <returns></returns>
		public static bool IsEventObjectFirst(GameObject eventObj)
		{
			if (eventObj == null) return false;

			bool result = false;

			Vector3 position = ScreenCenterPosition(eventObj.GetComponent<RectTransform>());
			var objects = GetPointRaycastObjects(position);
			if (objects.Count > 0)
			{
				for (int i = 0; i < objects.Count; ++i)
				{
					var gameObject = objects[i].gameObject;
					if (IsTransformChild(eventObj.transform, gameObject.transform)
						|| GetEventObjectInParent(gameObject.transform) == eventObj)
					{
						result = true;
						break;
					}
					else if (IsTransfomRaycastBy(eventObj.transform, gameObject.transform))
					{
						result = false;
						break;
					}
				}
			}
			else
			{
				result = true;
			}
			return result;
		}

		/// <summary>
		/// 检测事件是否被遮挡
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="raycastTransform"></param>
		/// <returns></returns>
		private static bool IsTransfomRaycastBy(Transform transform, Transform raycastTransform)
		{
			bool result = false;

			Image image = raycastTransform.GetComponent<Image>();
			if (image == null || !image.raycastTarget) return result;

			Rect raycastRect = ScreenRect(raycastTransform.GetComponent<RectTransform>());
			Rect rect = ScreenRect(transform.GetComponent<RectTransform>());

			if (raycastRect.Overlaps(rect))
			{
				result = true;
			}

			return result;
		}

		/// <summary>
		/// 是否是指定父节点的子结点
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="child"></param>
		/// <returns></returns>
		public static bool IsTransformChild(Transform parent, Transform child)
		{
			bool result = false;

			if (child.transform.parent == null) return false;

			Transform[] transforms = child.GetComponentsInParent<Transform>();
			for (int i = 0; i < transforms.Length; ++i)
			{
				if (transforms[i] == parent)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion 控件唤醒，移入，移出

		/// <summary>
		/// 获取指定位置指定事件对象列表
		/// </summary>
		/// <returns></returns>
		public static List<GameObject> GetPointEventObjects(Vector3 position)
		{
			List<GameObject> result = new List<GameObject>();

			var objects = GetPointRaycastObjects(position);
			foreach (var obj in objects)
			{
				var eventTransform = GetEventObjectInParent(obj.gameObject.transform);
				if (eventTransform != null && !result.Contains(eventTransform.gameObject))
				{
					result.Add(eventTransform.gameObject);
				}
			}

			return result;
		}

		/// <summary>
		/// 获取指定位置指定事件对象
		/// </summary>
		/// <returns></returns>
		public static Transform GetPointEventObject(Vector3 position)
		{
			Transform result = null;

			var objects = GetPointRaycastObjects(position);
			if (objects.Count > 0)
			{
				var eventTransform = GetEventObjectInParent(objects[0].gameObject.transform);
				if (eventTransform != null)
				{
					result = eventTransform;
				}
			}

			return result;
		}

		/// <summary>
		/// 获取含有事件对象
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static Transform GetEventObjectInParent(Transform transform)
		{
			if (transform == null) return null;

			var eventTrigger = transform.GetComponentInParent<EventTrigger>();
			if (eventTrigger != null)
			{
				return eventTrigger.transform;
			}

			var button = transform.GetComponentInParent<Button>();
			if (button != null)
			{
				return button.transform;
			}

			var toggle = transform.GetComponentInParent<Toggle>();
			if (toggle != null)
			{
				return toggle.transform;
			}

			return null;
		}

		/// <summary>
		/// 获取指定位置射线对象列表
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public static List<RaycastResult> GetPointRaycastObjects(Vector3 position)
		{
			List<RaycastResult> result = new List<RaycastResult>();
			if (EventSystem.current != null)
			{
				PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
				pointerEventData.position = position;
				pointerEventData.eligibleForClick = true;
				EventSystem.current.RaycastAll(pointerEventData, result);
			}

			return result;
		}

		/// <summary>
		/// 获取GameObjectHierarchy中的全路径
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static string GetGameObjectPath(Transform transform)
		{
			string path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			return path;
		}

		/// <summary>
		/// 事件对象是否可见
		/// </summary>
		/// <param name="eventObj"></param>
		/// <returns></returns>
		public static bool IsObjectVisible(Transform transform)
		{
			bool result = true;

			if (!transform.gameObject.activeSelf)
			{
				result = false;
				return result;
			}

			if (!transform.gameObject.activeInHierarchy)
			{
				result = false;
				return result;
			}

			if (Camera.main != null)
			{
				var screenPos = transform.position;
				if (screenPos.x < 0 || screenPos.x > Screen.width
					|| screenPos.y < 0 || screenPos.y > Screen.height)
				{
					result = IsInCameraView(Camera.main, transform.position);
				}
			}

			return result;
		}

		/// <summary>
		/// 查询物体是否在Camera视野内
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="wordPos"></param>
		/// <returns></returns>
		public static bool IsInCameraView(Camera camera, Vector3 wordPos)
		{
			// 是否在视野内
			bool result = false;
			Vector3 viewportPos = camera.WorldToViewportPoint(wordPos);
			Rect rect = new Rect(0, 0, 1, 1);
			result = rect.Contains(viewportPos);

			if (result)
			{
				// 是否在远近平面内
				if (viewportPos.z >= camera.nearClipPlane && viewportPos.z <= camera.farClipPlane)
				{
					result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// 是否键盘环境
		/// </summary>
		/// <returns></returns>
		public static bool IsKeybordPlatform()
		{
			return Application.platform == RuntimePlatform.WindowsEditor
			|| Application.platform == RuntimePlatform.LinuxEditor
			|| Application.platform == RuntimePlatform.OSXEditor
			|| Application.platform == RuntimePlatform.WindowsPlayer
			|| Application.platform == RuntimePlatform.LinuxPlayer;
		}

		/// <summary>
		/// 获取UGUI对象的矩形区域
		/// </summary>
		/// <param name="rectTransform"></param>
		/// <returns></returns>
		public static Rect ScreenRect(RectTransform rectTransform)
		{
			if (rectTransform == null) return Rect.zero;
			if (Camera.main == null) return Rect.zero;

			Vector3[] fourCornersArray = new Vector3[4];
			rectTransform.GetWorldCorners(fourCornersArray);

			var canvas = rectTransform.GetComponentInParent<Canvas>();
			if (canvas == null) return Rect.zero;
			if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				for (int i = 0; i < fourCornersArray.Length; ++i)
				{
					fourCornersArray[i] = Camera.main.WorldToScreenPoint(fourCornersArray[i]);
				}
			}

			Vector3 position = fourCornersArray[0];
			Vector3 size = fourCornersArray[2] - fourCornersArray[0];
			return new Rect(position.x
				, position.y
				, size.x
				, size.y
				);
		}

		/// <summary>
		/// 获取UGUI对象的中心点坐标
		/// </summary>
		/// <param name="rectTransform"></param>
		/// <returns></returns>
		public static Vector3 ScreenCenterPosition(RectTransform rectTransform)
		{
			Vector3 result = rectTransform.position;

			var canvas = rectTransform.GetComponentInParent<Canvas>();
			if (canvas == null) return result;

			//World position -> screen position
			if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				Vector2 sizeDelta = rectTransform.sizeDelta;
				float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
				float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

				result = canvas.worldCamera.WorldToScreenPoint(rectTransform.position);
				result.x -= rectTransformWidth * rectTransform.pivot.x * 0.5f;
				result.y -= rectTransformHeight * rectTransform.pivot.y * 0.5f;
			}
			else
			{
				Vector3[] fourCornersArray = new Vector3[4];
				rectTransform.GetWorldCorners(fourCornersArray);

				result = (fourCornersArray[2] + fourCornersArray[0]) * 0.5f;
			}

			return result;
		}

		public static void AdaptResolution(Transform selectTran, Vector2 referResolution, Vector2 configResolution)
		{
			int ratio = (int)(referResolution.x / configResolution.x);
			if (ratio == 1) return;

			RectTransform[] allRect = selectTran.GetComponentsInChildren<RectTransform>(true);
			for (int i = 0; i < allRect.Length; i++)
			{
				allRect[i].anchoredPosition = allRect[i].anchoredPosition / ratio;
				allRect[i].sizeDelta = allRect[i].sizeDelta / ratio;
				if (allRect[i].GetComponent<Text>() != null || allRect[i].GetComponent<TextPro>() != null)
				{
					allRect[i].GetComponent<Text>().fontSize = allRect[i].GetComponent<Text>().fontSize / ratio;
					if (allRect[i].GetComponent<Text>().resizeTextForBestFit)
					{
						allRect[i].GetComponent<Text>().resizeTextMaxSize = allRect[i].GetComponent<Text>().resizeTextMaxSize / ratio;
					}
				}
				if (allRect[i].GetComponent<LayoutGroup>() != null)
				{
					RectOffset rectOffset = allRect[i].GetComponent<LayoutGroup>().padding;
					rectOffset.left = rectOffset.left / ratio;
					rectOffset.top = rectOffset.top / ratio;
					rectOffset.right = rectOffset.right / ratio;
					rectOffset.bottom = rectOffset.bottom / ratio;
					allRect[i].GetComponent<LayoutGroup>().padding = rectOffset;

					if (allRect[i].GetComponent<GridLayoutGroup>() != null)
					{
						allRect[i].GetComponent<GridLayoutGroup>().spacing = allRect[i].GetComponent<GridLayoutGroup>().spacing / ratio;
						allRect[i].GetComponent<GridLayoutGroup>().cellSize = allRect[i].GetComponent<GridLayoutGroup>().cellSize / ratio;
					}
					if (allRect[i].GetComponent<HorizontalLayoutGroup>() != null)
					{
						allRect[i].GetComponent<HorizontalLayoutGroup>().spacing = allRect[i].GetComponent<HorizontalLayoutGroup>().spacing / ratio;
					}
					if (allRect[i].GetComponent<VerticalLayoutGroup>() != null)
					{
						allRect[i].GetComponent<VerticalLayoutGroup>().spacing = allRect[i].GetComponent<VerticalLayoutGroup>().spacing / ratio;
					}
				}
			}
		}
	}
}