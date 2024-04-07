namespace WestBay
{
	/// <summary>
	/// 测试用表
	/// </summary>
	[DBHistory("DBEAAA", "DBEVLLLLV", "DBETestAAA", "DBEVVVV")]
	public class DBETest : DBEntry
	{
		[DBHistory("Col1", "ColFirst")]
		public TEXT ColA { set; get; }

		[DBHistory("ColS")]
		[DBFieldProperties("PrimaryKey:1", "NotNull:1")]
		public INT64 ColB { set; get; }

		public DTIME Date { get; set; }

		public BLOB Image { get; set; }
	}

	public class DBMTest : DBMgrT<DBMTest, DBETest>
	{
		public DBMTest(string path) : base(new DBETest())
		{
			SetSH(path);
		}
	}

	/// <summary>
	/// 测试用表A
	/// </summary>
	public class DBETestAAA : DBEntry
	{
		[DBHistory("Col1", "ColFirst")]
		public TEXT ColA { set; get; }

		[DBHistory("ColS")]
		public INT64 ColB { set; get; }

		public DTIME Date { get; set; }

		public BLOB Image { get; set; }
	}

	public class DBMTestAAA : DBMgrT<DBMTestAAA, DBETestAAA>
	{
		public DBMTestAAA(string path) : base(new DBETestAAA())
		{
			SetSH(path);
		}
	}

	/// <summary>
	/// 测试用表B
	/// </summary>
	[DBHistory("DBETestAAA", "DBEVLLLLV", "DBEAAA", "DBEVVVV")]
	public class DBETestBBB : DBEntry
	{
		[DBHistory("Col1", "ColFirst")]
		public TEXT ColA { set; get; }

		[DBHistory("ColS")]
		public INT64 ColB { set; get; }

		public DTIME Date { get; set; }

		public BLOB Image { get; set; }
	}

	public class DBMTestBBB : DBMgrT<DBMTestBBB, DBETestBBB>
	{
		public DBMTestBBB(string path) : base(new DBETestBBB())
		{
			SetSH(path);
		}
	}
}