using UnityEngine;

namespace WestBay
{
	internal class DBEConfig : DBEntry
	{
		[DBFieldProperties("PrimaryKey:1", "NotNull:1")]
		public TEXT Key { get; set; }

		public TEXT Value { get; set; }
	}
}