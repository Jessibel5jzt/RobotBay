using System;

namespace WestBay
{
	/// <summary>
	/// 可以不使用构造函数，直接 new DBEntry{rowid = 123;}
	/// 注意：
	/// 所有子类，均以 DBE 开头。如 DBEUser
	/// 类必须是[Serializable]修饰
	/// 字段必须是 Property 就是有 {set; get;}
	/// 字段必须是 DBV 类型
	/// </summary>
	[Serializable]
	public class DBEntry : DBValType
	{
		/// <summary>
		/// 数据库表必须有字段。自增长int64
		/// </summary>
		public INT64 rowid { set; get; }
	}//class

	public enum DBOp
	{
		/// <summary>
		/// Greator than >
		/// </summary>
		G,

		/// <summary>
		/// Greator or Equal >=
		/// </summary>
		GE,

		/// <summary>
		/// Equal  ==
		/// Default Value
		/// </summary>
		E,

		/// <summary>
		/// Less or Equal <=
		/// </summary>
		LE,

		/// <summary>
		/// Less than <
		/// </summary>
		L,

		/// <summary>
		/// Not Equal !=
		/// </summary>
		NE,
	}

	public class BOOL : DBV
	{
		public static implicit operator bool(BOOL v)
		{ return Convert.ToBoolean(v?.V); }

		public static implicit operator BOOL(bool v)
		{ return new BOOL { V = v }; }
	}

	public class INT8 : DBV
	{
		public static implicit operator char(INT8 v)
		{ return Convert.ToChar(v?.V); }

		public static implicit operator INT8(char v)
		{ return new INT8 { V = v }; }
	}

	public class UINT8 : DBV
	{
		public static implicit operator byte(UINT8 v)
		{ return Convert.ToByte(v?.V); }

		public static implicit operator UINT8(byte v)
		{ return new UINT8 { V = v }; }
	}

	public class INT16 : DBV
	{
		public static implicit operator short(INT16 v)
		{ return Convert.ToInt16(v?.V); }

		public static implicit operator INT16(short v)
		{ return new INT16 { V = v }; }
	}

	public class UINT16 : DBV
	{
		public static implicit operator ushort(UINT16 v)
		{ return Convert.ToUInt16(v?.V); }

		public static implicit operator UINT16(ushort v)
		{ return new UINT16 { V = v }; }
	}

	public class INT32 : DBV
	{
		public static implicit operator int(INT32 v)
		{ return Convert.ToInt32(v?.V); }

		public static implicit operator INT32(int v)
		{ return new INT32 { V = v }; }
	}

	public class UINT32 : DBV
	{
		public static implicit operator uint(UINT32 v)
		{ return Convert.ToUInt32(v?.V); }

		public static implicit operator UINT32(uint v)
		{ return new UINT32 { V = v }; }
	}

	public class INT64 : DBV
	{
		public static implicit operator long(INT64 v)
		{ return Convert.ToInt64(v?.V); }

		public static implicit operator INT64(long v)
		{ return new INT64 { V = v }; }
	}

	public class UINT64 : DBV
	{
		public static implicit operator ulong(UINT64 v)
		{ return Convert.ToUInt64(v?.V); }

		public static implicit operator UINT64(ulong v)
		{ return new UINT64 { V = v }; }
	}

	public class SINGLE : DBV
	{
		public static implicit operator float(SINGLE v)
		{ return Convert.ToSingle(v?.V); }

		public static implicit operator SINGLE(float v)
		{ return new SINGLE { V = v }; }
	}

	public class DOUBLE : DBV
	{
		public static implicit operator double(DOUBLE v)
		{ return Convert.ToDouble(v?.V); }

		public static implicit operator DOUBLE(double v)
		{ return new DOUBLE { V = v }; }
	}

	public class TEXT : DBV
	{
		public static implicit operator string(TEXT v)
		{ return Convert.ToString(v?.V); }

		public static implicit operator TEXT(string v)
		{ return new TEXT { V = v }; }
	}

	public class DTIME : DBV
	{
		public static implicit operator DateTime(DTIME v)
		{ return Convert.ToDateTime(v?.V); }

		public static implicit operator DTIME(DateTime v)
		{ return new DTIME { V = v }; }

		public override string ToString()
		{ return (V == null) ? "" : ((DateTime)V).ToString("yyyy-MM-dd HH:mm:ss"); }
	}

	public class UUID : DBV
	{
		public static implicit operator Guid(UUID v)
		{ return (Guid)v?.V; }

		public static implicit operator UUID(Guid v)
		{ return new UUID { V = v }; }
	}

	public class BLOB : DBV
	{
		public static implicit operator byte[](BLOB v)
		{
			return (byte[])v?.V;
		}

		public static implicit operator BLOB(byte[] v)
		{ return new BLOB { V = v }; }

		public override bool IsBLOB()
		{ return true; }
	}

	public class DBV
	{
		/// <summary>
		/// Create DBValue Functions
		/// </summary>
		public static BOOL C(bool d, DBOp p)
		{ return new BOOL { V = d, P = p }; }

		public static INT8 C(char d, DBOp p)
		{ return new INT8 { V = d, P = p }; }

		public static UINT8 C(byte d, DBOp p)
		{ return new UINT8 { V = d, P = p }; }

		public static INT16 C(short d, DBOp p)
		{ return new INT16 { V = d, P = p }; }

		public static UINT16 C(ushort d, DBOp p)
		{ return new UINT16 { V = d, P = p }; }

		public static INT32 C(int d, DBOp p)
		{ return new INT32 { V = d, P = p }; }

		public static UINT32 C(uint d, DBOp p)
		{ return new UINT32 { V = d, P = p }; }

		public static INT64 C(long d, DBOp p)
		{ return new INT64 { V = d, P = p }; }

		public static UINT64 C(ulong d, DBOp p)
		{ return new UINT64 { V = d, P = p }; }

		public static SINGLE C(float d, DBOp p)
		{ return new SINGLE { V = d, P = p }; }

		public static DOUBLE C(double d, DBOp p)
		{ return new DOUBLE { V = d, P = p }; }

		public static TEXT C(string d, DBOp p)
		{ return new TEXT { V = d, P = p }; }

		public static DTIME C(DateTime d, DBOp p)
		{ return new DTIME { V = d, P = p }; }

		public static UUID C(Guid d, DBOp p)
		{ return new UUID { V = d, P = p }; }

		public static BLOB C(byte[] d, DBOp p)
		{ return new BLOB { V = d, P = p }; }

		/// <summary>
		/// Value
		/// </summary>
		public object V;

		/// <summary>
		/// Operation
		/// </summary>
		public DBOp P = DBOp.E;

		/// <summary>
		/// P to string
		/// </summary>
		/// <returns></returns>
		public string GetOpStr()
		{
			switch (P)
			{
				case DBOp.G: return ">";
				case DBOp.GE: return ">=";
				case DBOp.E: return "=";
				case DBOp.LE: return "<=";
				case DBOp.L: return "<";
				case DBOp.NE: return "!=";
			}
			return "=";
		}

		public virtual void SetValue(object v)
		{ V = v; }

		public override string ToString()
		{ return (V == null) ? "" : V.ToString(); }

		public virtual bool IsBLOB()
		{ return false; }
	}
}//ns