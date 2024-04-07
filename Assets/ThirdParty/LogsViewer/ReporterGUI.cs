using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ReporterGUI : MonoBehaviour
{
	Reporter reporter;
	void Awake()
	{
		reporter = gameObject.GetComponent<Reporter>();
	}

	void OnGUI()
	{
		reporter.OnGUIDraw();

	}

	bool IsTouchedUI()
	{
		bool touchedUI = false;
		if (Application.isMobilePlatform)
		{
			if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
			{
				touchedUI = true;
			}
		}
		else if (EventSystem.current.IsPointerOverGameObject())
		{
			touchedUI = true;
		}
		return touchedUI;
	}
}
