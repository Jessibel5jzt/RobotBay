using System;
using UnityEngine;
using UnityEditor;

namespace WestBay
{
	[CustomPropertyDrawer(typeof(FieldLabelAttribute))]
	public class FieldLabelDrawer : PropertyDrawer

	{
		private FieldLabelAttribute WBAttribute

		{
			get { return (FieldLabelAttribute)attribute; }
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)

		{
			EditorGUI.PropertyField(position, property, new GUIContent(WBAttribute.label), true);
		}
	}
}