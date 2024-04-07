using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WestBay
{
	/// <summary>
	/// 为场景类提供基础功能
	/// </summary>
	public class SceneMethod
	{
		/// <summary>
		/// 子类复写，实现子模块名字
		/// </summary>
		public virtual string Name
		{ get { return ""; } }

		/// <summary>
		/// 是否展厅状态
		/// </summary>
		public bool IsPause
		{ get { return Module.IsPaused; } }

		/// <summary>
		/// 场景所属模块
		/// </summary>
		public Module Module { get; private set; }

		#region Register Ctrl Callback

		/// @name  注册控件回调
		/// 子类可以使用这些函数方便的将函数和控件绑定
		/// @{
		protected void RegButton(string name, UnityAction act)
		{
			Button Bt = GetComponent(typeof(Button), name) as Button;
			Bt.onClick.AddListener(act);
		}

		protected void RegButton(string name, UnityAction<Object> act)
		{
			Button Bt = GetComponent(typeof(Button), name) as Button;
			Bt.onClick.AddListener(() => act(Bt));
		}

		protected void RegButton(Button go, UnityAction act)
		{
			go.onClick.AddListener(act);
		}

		protected void RegButton(Button go, UnityAction<Button> act)
		{
			go.onClick.AddListener(() => act(go));
		}

		protected void RegToggle(string name, UnityAction<bool> act)
		{
			Toggle Tog = GetComponent(typeof(Toggle), name) as Toggle;
			Tog.onValueChanged.AddListener(act);
		}

		protected void RegToggle(Toggle go, UnityAction<bool> act)
		{
			go.onValueChanged.AddListener(act);
		}

		protected void RegToggle(Toggle go, UnityAction<bool, Toggle> act)
		{
			go.onValueChanged.AddListener((ison) => act(ison, go));
		}

		protected void RegSliderValueChanged(string name, UnityAction<float> act)
		{
			Slider IF = GetComponent(typeof(Slider), name) as Slider;
			IF.onValueChanged.AddListener(act);
		}

		protected void RegSliderValueChanged(Slider inf, UnityAction<float> act)
		{
			inf.onValueChanged.AddListener(act);
		}

		protected void RegInputFieldOnEndEdit(string name, UnityAction<string> act)
		{
			InputField IF = GetComponent(typeof(InputField), name) as InputField;
			IF.onEndEdit.AddListener(act);
		}

		protected void RegInputFieldOnEndEdit(InputField inf, UnityAction<string> act)
		{
			inf.onEndEdit.AddListener(act);
		}

		protected void RegInputFieldonValueChanged(string name, UnityAction<string> act)
		{
			InputField IF = GetComponent(typeof(InputField), name) as InputField;
			IF.onValueChanged.AddListener(act);
		}

		protected void RegInputFieldonValueChanged(InputField inf, UnityAction<string> act)
		{
			inf.onValueChanged.AddListener(act);
		}

		protected void RegDropDown(string name, UnityAction<int> act)
		{
			Dropdown IF = GetComponent(typeof(Dropdown), name) as Dropdown;
			IF.onValueChanged.AddListener(act);
		}

		protected void AddEventTriggerEvent(Component obj, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback)
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
			entry.callback.AddListener(callback);
			trigger.triggers.Add(entry);
		}

		/// @}

		#endregion Register Ctrl Callback

		#region Unregister Ctrl Callback

		///@name 取消注册
		///@{
		protected void UnregButton(string name, UnityAction act)
		{
			Button Bt = GetComponent(typeof(Button), name) as Button;
			Bt.onClick.RemoveListener(act);
		}

		protected void UnregButton(Button btn)
		{
			btn.onClick.RemoveAllListeners();
		}

		protected void UnregButton(Button btn, UnityAction act)
		{
			btn.onClick.RemoveListener(act);
		}

		protected void UnregToggle(string name, UnityAction<bool> act)
		{
			Toggle Tog = GetComponent(typeof(Toggle), name) as Toggle;
			Tog.onValueChanged.RemoveListener(act);
		}

		protected void UnregToggle(Toggle tog, UnityAction<bool> act)
		{
			tog.onValueChanged.RemoveListener(act);
		}

		protected void UnregToggle(string name)
		{
			Toggle Tog = GetComponent(typeof(Toggle), name) as Toggle;
			Tog.onValueChanged.RemoveAllListeners();
		}

		protected void UnregToggle(Toggle tog)
		{
			tog.onValueChanged.RemoveAllListeners();
		}

		protected void UnregButton(string name)
		{
			Button Bt = GetComponent(typeof(Button), name) as Button;
			Bt.onClick.RemoveAllListeners();
		}

		protected void UnregInputFieldOnEndEdit(string name)
		{
			InputField IF = GetComponent(typeof(InputField), name) as InputField;
			IF.onEndEdit.RemoveAllListeners();
		}

		protected void UnregInputFieldOnEndEdit(string name, UnityAction<string> act)
		{
			InputField IF = GetComponent(typeof(InputField), name) as InputField;
			IF.onEndEdit.RemoveListener(act);
		}

		protected void UnregInputFieldonValueChanged(string name, UnityAction<string> act)
		{
			InputField IF = GetComponent(typeof(InputField), name) as InputField;
			IF.onValueChanged.RemoveListener(act);
		}

		protected void UnregDropDown(string name, UnityAction<int> act)
		{
			Dropdown IF = GetComponent(typeof(Dropdown), name) as Dropdown;
			IF.onValueChanged.RemoveListener(act);
		}

		protected void RemoveEventTriggerEvent(Component obj, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback = null)
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

		protected void ClearAllTriggerEvent(Component obj)
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

		///@}

		#endregion Unregister Ctrl Callback

		/// <summary>
		/// Retrieve component
		/// </summary>
		/// <typeparam name="T">Component type</typeparam>
		/// <param name="name">GameObject name of the component</param>
		/// <returns></returns>
		protected Component GetComponent(System.Type t, string name)
		{
			var GO = GameObject.Find(name);
			if (GO == null)
			{
				Debug.LogWarning($"NO GameObject named \"{name}\"");
				return null;
			}
			var BC = GO.GetComponent(t);
			if (BC == null)
			{
				Debug.LogWarning($"NO Component named \"{name}\"");
				return null;
			}

			return BC;
		}

		#region INNER FUNCTIONS

		/// <summary>
		/// 内部使用
		/// </summary>
		public void _set_Module(Module m)
		{ Module = m; }

		#endregion INNER FUNCTIONS
	}
}