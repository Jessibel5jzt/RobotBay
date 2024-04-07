using System;
using UnityEngine;

namespace WestBay
{
	[AttributeUsage(AttributeTargets.Field)]
	public class FieldLabelAttribute : PropertyAttribute
	{
		public string label;

		public FieldLabelAttribute(string label)
		{
			this.label = label;
		}
	}
}