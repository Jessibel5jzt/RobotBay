using UnityEngine.EventSystems;
using UnityEngine;

namespace WestBay
{
	public class PointClickHandler : MonoBehaviour, IPointerClickHandler
	{
		public void OnPointerClick(PointerEventData eventData)
		{
		}
	}

	public class PointEnterHandler : MonoBehaviour, IPointerEnterHandler
	{
		public void OnPointerEnter(PointerEventData eventData)
		{
		}
	}

	public class PointExitHandler : MonoBehaviour, IPointerExitHandler
	{
		public void OnPointerExit(PointerEventData eventData)
		{
		}
	}
}