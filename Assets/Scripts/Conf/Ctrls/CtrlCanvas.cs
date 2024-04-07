using UnityEngine;

namespace WestBay
{
	[ExecuteInEditMode]
	public class CtrlCanvas : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Language set only for test in Editor")]
		public string EditorLanguage = "en";

		[SerializeField]
		[Tooltip("Theme set only for test in Editor")]
		public string EditorTheme = "light";

		private void Update()
		{
			if (Application.isPlaying) return;
			Setting();
		}

		private void Setting()
		{
			LocalMgr.CurrentCulture = EditorLanguage.ToLower();

			if (ThemeMgr.IsExist(EditorTheme.ToLower()))
			{
				ThemeMgr.CurrentTheme = EditorTheme.ToLower();
			}
			else
			{
				Debug.LogEDITOR($"当前输入的风格:【{EditorTheme.ToLower()}】不存在,切换为默认风格light");
				ThemeMgr.CurrentTheme = "light";
			}
		}
	}
}