using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WestBay
{
	[Serializable]
	public struct StringColorBlock : IEquatable<StringColorBlock>
	{
		[FormerlySerializedAs("NormalColor")]
		[SerializeField]
		private string _normalColor;

		public string NormalColor
		{
			get { return _normalColor; }
			set { _normalColor = value; }
		}

		[FormerlySerializedAs("HighlightedColor")]
		[SerializeField]
		private string _highlightedColor;

		public string HighlightedColor
		{
			get { return _highlightedColor; }
			set { _highlightedColor = value; }
		}

		[FormerlySerializedAs("PressedColor")]
		[SerializeField]
		private string _pressedColor;

		public string PressedColor
		{
			get { return _pressedColor; }
			set { _pressedColor = value; }
		}

		[FormerlySerializedAs("SelectedColor")]
		[SerializeField]
		private string _selectedColor;

		public string SelectedColor
		{
			get { return _selectedColor; }
			set { _selectedColor = value; }
		}

		[FormerlySerializedAs("DisabledColor")]
		[SerializeField]
		private string _disabledColor;

		public string DisabledColor
		{
			get { return _disabledColor; }
			set { _disabledColor = value; }
		}

		public static StringColorBlock DefaultStringColorBlock
		{
			get
			{
				var colorBlock = new StringColorBlock
				{
					_normalColor = "Title3",
					_highlightedColor = "Value3",
					_pressedColor = "Value3",
					_selectedColor = "Value3",
					_disabledColor = "Title3"
				};
				return colorBlock;
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is StringColorBlock))
				return false;

			return Equals((StringColorBlock)obj);
		}

		public bool Equals(StringColorBlock other)
		{
			return NormalColor == other.NormalColor &&
				HighlightedColor == other.HighlightedColor &&
				PressedColor == other.PressedColor &&
				SelectedColor == other.SelectedColor &&
				DisabledColor == other.DisabledColor;
		}

		public static bool operator ==(StringColorBlock point1, StringColorBlock point2)
		{
			return point1.Equals(point2);
		}

		public static bool operator !=(StringColorBlock point1, StringColorBlock point2)
		{
			return !point1.Equals(point2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}