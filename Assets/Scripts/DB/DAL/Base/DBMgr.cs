using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WestBay
{
	/// <summary>
	/// 用以CURD DBEntry类
	/// 注意所有子类均已 DBM 开头，如DBMUser
	/// </summary>
	/// <typeparam name="T">子类</typeparam>
	/// <typeparam name="E">子类对应的 DBEntry类</typeparam>
	public class DBMgrT<T, E>
		where T : DBMgrT<T, E>
		where E : DBEntry, new()
	{
		public DBMgrT(E e)
		{
			var type = e.GetType();
			if (type != typeof(DBEntry) && !DBUpdateMgr.Ins.DBEList.Contains(type))
			{
				DBUpdateMgr.Ins.DBEList.Add(type);
			}
		}

		/// <summary>
		/// 根据e.rowid来判断是否 存在。
		/// </summary>
		/// <param name="e">根据e.rowid属性</param>
		/// <returns></returns>
		public bool HasEntry(E e)
		{
			//nothing is has no entry(thing)
			if (e == null) return false;

			if (e.rowid == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(e)}][HasEntry] Must Has rowid");
				return false;
			}
			string sqlString = $"SELECT * FROM [{Sh.GetTableName(e)}] WHERE rowid = {e.rowid}";
			int count = Sh.Execute(sqlString);
			return count > 0;
		}

		/// <summary>
		/// 表中数据量
		/// </summary>
		/// <returns>数量</returns>
		public int GetEntryCount(E e)
		{
			if (e == null) return 0;

			string sqlString = $"SELECT COUNT(*) FROM [{Sh.GetTableName(e)}]";
			int count = Sh.Execute(sqlString);
			return count;
		}

		/// <summary>
		/// 增加一条数据。
		/// </summary>
		/// <param name="e">a DBEntry subclass</param>
		/// <example> AddData ( new E { PropertyA = xxx, PropertyB = xxx } ) </example>
		/// <returns>是否成功</returns>
		public bool AddData(E e)
		{
			//add nothing, OK. nothing Added.
			if (e == null) return true;

			StringBuilder SbKey = new StringBuilder();
			StringBuilder SbValue = new StringBuilder();
			int BlobVarCnt = 0;
			var VV = new SqliteHelp.VarValList();

			foreach (var P in e.GetClassType().GetProperties())
			{
				if (_PropertyNoValue(e, P)) continue;
				if (P.Name != "rowid")
				{
					var Val = P.GetValue(e) as DBV;
					var ValStr = $"'{Val}'";

					bool IsBlob = Val.IsBLOB();
					if (IsBlob)
					{
						BlobVarCnt++;
						ValStr = $":Value{BlobVarCnt}";
						VV.Add(new SqliteHelp.VarVal { Name = $"Value{BlobVarCnt}", Value = Val.V });
					}

					SbKey.Append("[");
					SbKey.Append(P.Name);
					SbKey.Append("], ");

					SbValue.Append(ValStr);
					SbValue.Append(", ");
				}
			}

			SbKey.RemoveLast(2);
			SbValue.RemoveLast(2);

			return _AddData(e, SbKey.ToString(), SbValue.ToString(), VV);
		}

		/// <summary>
		/// Select 1 row only
		/// </summary>
		/// <param name="e"></param>
		/// <example> SelectData1 ( new E { PropertyA = xxx, PropertyB = xxx } ) </example>
		/// <returns></returns>
		public E SelectData1(E e)
		{
			var List = SelectData(e);
			if (List.Count == 0) return null;
			return List[0];
		}

		/// <summary>
		/// Select  1 row only with where and condition
		/// </summary>
		/// <param name="e"></param>
		/// <param name="cond"></param>
		/// <example> SelectData1 ( new E { PropertyA = xxx, PropertyB = xxx }, new DBCondition{...} ) </example>
		public E SelectData1(E e, DBCondition cond)
		{
			var List = SelectData(e, cond);
			if (List.Count == 0) return null;
			return List[0];
		}

		/// <summary>
		/// select 1 row only with condition
		/// 注意，子模块中的DBV类，不能使用这个函数，
		/// 改用 SelectData(E e, DBCondition cond)，
		/// 第一个参数填 空类
		/// </summary>
		public E SelectData1(DBCondition cond)
		{
			var List = SelectData(cond);
			if (List.Count == 0) return null;
			return List[0];
		}

		/// <summary>
		/// select all with condition
		/// 注意，子模块中的DBV类，不能使用这个函数，
		/// 改用 SelectData(E e, DBCondition cond)，
		/// 第一个参数填 空类
		/// </summary>
		/// <param name="cond">
		/// 如果为null则选择全部记录
		/// </param>
		/// <returns>list不会为null。没有记录时，list长度为0</returns>
		public List<E> SelectData(DBCondition cond)
		{
			return SelectData(new E(), cond);
		}

		/// <summary>
		/// select all with where
		/// </summary>
		/// <param name="e">
		/// 如果是null，则选择全部记录。
		/// 如果e有rowid，则_只_根据rowid查找
		/// </param>
		/// <example> SelectData ( new E { PropertyA = xxx, PropertyB = xxx } ) </example>
		/// <returns>list不会为null。没有记录时，list长度为0</returns>
		public List<E> SelectData(E e)
		{
			return SelectData(e, null);
		}

		/// <summary>
		/// 根据带入e的属性选择数据。
		/// DLL DBVs passes empty for *
		/// </summary>
		/// <param name="ent">
		/// 如果是null，则选择全部记录
		/// 如果e有rowid，则_只_根据rowid查找
		/// </param>
		/// <param name="cond">
		/// 如果为null则选择全部记录
		/// </param>
		/// <returns>list不会为null。没有记录时，list长度为0</returns>
		public List<E> SelectData(E ent, DBCondition cond)
		{
			if (ent == null) return new List<E>();

			string WhereCond = _CombineProperties(false, null, ent, CombRule.PreferRowId, Delima.WhereCond);
			return _SelectData(ent, WhereCond, cond);
		}

		public List<E> SelectDataBySql(E ent, string sqlString)
		{
			if (ent == null || string.IsNullOrEmpty(sqlString)) return new List<E>();

			return _SelectData(ent, sqlString);
		}

		public ArrayList SelectData(string sqlStr)
		{
			return _SelectData(sqlStr);
		}

		public void CrateTable(string tableName, List<string> infoList)
		{
			Sh.CreateTable(tableName, infoList);
		}

		public void AddData(string sqlStr, SqliteHelp.VarValList vv)
		{
			Sh.ExecuteUpdate(sqlStr, vv);
		}

		/// <summary>
		/// 根据condition 把所有符合的条目修改为 newValue
		/// </summary>
		/// <param name="condition">如果有rowid则其他条件忽略</param>
		/// <param name="newValue">rowid被忽略</param>
		/// <returns></returns>
		public bool ModifyData(E condition, E newValue)
		{
			//can't change * to *, failed.
			if (condition == null) return false;
			if (newValue == null) return false;

			SqliteHelp.VarValList VV = new SqliteHelp.VarValList();
			string SetStr = _CombineProperties(true, VV, newValue, CombRule.ExceptRowId, Delima.SetValue);
			if (SetStr == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(condition)}][ModifyData] No newValue");
				return false;
			}
			string CondStr = _CombineProperties(false, null, condition, CombRule.PreferRowId, Delima.WhereCond);
			if (CondStr == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(condition)}][ModifyData] No condition");
				return false;
			}
			return _ModifyData(newValue, SetStr, CondStr, VV);
		}

		/// <summary>
		/// 根据e.rowid 来 Update一个条目
		/// </summary>
		/// <param name="e">必须要有rowid属性</param>
		/// <returns></returns>
		public bool ModifyData(E ent)
		{
			//modify nothing, DONE, 'cause nothing changed.
			if (ent == null) return true;

			string CondStr = _CombineProperties(false, null, ent, CombRule.OnlyRowId);
			if (CondStr == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][ModifyData] Must Has rowid");
				return false;
			}

			SqliteHelp.VarValList VV = new SqliteHelp.VarValList();
			string SetStr = _CombineProperties(true, VV, ent, CombRule.ExceptRowId, Delima.SetValue);
			if (SetStr == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][ModifyData] No Value Change.");
				return false;
			}

			return _ModifyData(ent, SetStr, CondStr, VV);
		}

		/// <summary>
		/// 从数据删除一个数据。条件根据e的属性来判断
		/// </summary>
		/// <param name="e">
		/// 不可是null。如果有rowid，则_只_根据rowid查找。
		/// 否则匹配所有其他属性。通过“和”逻辑判断。PropertyA = value AND PropertyB = value
		/// </param>
		/// <returns></returns>
		public bool DeleteData(E ent)
		{
			if (ent == null) return true; //delete nothing, OK then.

			string WhereCond = _CombineProperties(false, null, ent, CombRule.PreferRowId, Delima.WhereCond);
			if (WhereCond == null)
			{
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][DeleteData] No Condition.");
				return false;
			}
			return _DeleteData(ent, WhereCond);
		}

		/// <summary>
		/// 删除表中所有数据
		/// </summary>
		/// <returns>数量</returns>
		public bool DeleteTable(string e)
		{
			if (e == null) return true;

			string sqlString = $"Delete FROM {e}";
			bool count = Sh.ExecuteUpdate(sqlString, null);
			DeleteSequenceData(e);
			return count;
		}

		/// <summary>
		/// 删除递增表中所有数据
		/// </summary>
		/// <returns>数量</returns>
		public bool DeleteSequenceData(string e)
		{
			if (e == null) return true;

			string sqlString = $"Delete FROM sqlite_sequence where  name ='{e}'";
			bool count = Sh.ExecuteUpdate(sqlString, null);
			return count;
		}

		public void DoSqlString(string str)
		{
			if (str == null) return;
			Sh.ExecuteUpdate(str, null);
		}

		public virtual void SetSH(string dbPath)
		{
			Sh = new SqliteHelp(dbPath);
		}

		#region Protected

		protected List<E> _SelectData(DBValType ent, string whereCond, DBCondition cond)
		{
			StringBuilder Sb = new StringBuilder();
			Sb.Append("SELECT rowid, * FROM [");
			Sb.Append(Sh.GetTableName(ent));
			Sb.Append("]");

			if (!string.IsNullOrEmpty(whereCond))
			{
				Sb.Append(" WHERE ");
				Sb.Append(whereCond);
			}

			if (cond != null)
			{
				Sb.Append(cond.ToString());
			}

			var List = Sh.ExecuteQuery<E>(ent, Sb.ToString());
			return List;
		}

		protected List<E> _SelectData(DBValType ent, string sqlString)
		{
			var List = Sh.ExecuteQuery<E>(ent, sqlString);
			return List;
		}

		protected ArrayList _SelectData(string sqlStr)
		{
			var List = Sh.ExecuteQuery(sqlStr);
			return List;
		}

		protected bool _ModifyData(DBValType ent, string keyVal, string whereCond, SqliteHelp.VarValList vv)
		{
			if (string.IsNullOrEmpty(keyVal))
			{
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][_ModifyData] No New Values");
				return false;
			}

			if (string.IsNullOrEmpty(whereCond))
			{
				//UPDATE没有条件，太过恐怖，直接返回。
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][_ModifyData]Must Has Condition");
				return false;
			}

			var sqliteStr = $"UPDATE [{Sh.GetTableName(ent)}] SET {keyVal} WHERE {whereCond}";
			return Sh.ExecuteUpdate(sqliteStr, vv);
		}

		protected bool _AddData(DBValType ent, string keys, string values, SqliteHelp.VarValList vv)
		{
			if (string.IsNullOrEmpty(keys) || string.IsNullOrEmpty(values))
			{
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][_AddData] Must Has Keys & Values");
				return false;
			}
			var sqliteStr = $"INSERT INTO [{Sh.GetTableName(ent)}] ({keys}) VALUES ({values})";
			return Sh.ExecuteUpdate(sqliteStr, vv);
		}

		protected bool _DeleteData(DBValType ent, string whereCond)
		{
			if (string.IsNullOrEmpty(whereCond))
			{
				//delete 没有条件，太过恐怖，直接返回
				Debug.Log($"[DBM][{Sh.GetTableName(ent)}][_DeleteData] Must Has Condition");
				return false;
			}

			var sqliteStr = $"DELETE FROM [{Sh.GetTableName(ent)}] WHERE {whereCond}";
			return Sh.ExecuteUpdate(sqliteStr, null);
		}

		private bool _PropertyNoValue(DBValType ent, PropertyInfo pi)
		{
			var Val = pi.GetValue(ent);
			if (Val == null) return true;
			if (!pi.PropertyType.IsSubclassOf(typeof(DBV))) return true;
			return false;
		}

		public void CloseSQL()
		{
			Sh.CloseSQL();
		}

		public SqliteHelp Sh;

		#endregion Protected

		#region Combine Key Value

		private enum CombRule
		{
			//都要
			ContainRowId,

			//不要rowid
			ExceptRowId,

			//只要rowid
			OnlyRowId,

			//如果有rowid，则其他都不要了
			PreferRowId,
		}

		private enum Delima
		{
			SetValue, // SET Key=Value, Key=Value
			WhereCond, //WHERE Key=Value AND Key=Value
		}

		private string _CombineProperties(bool useBlob, SqliteHelp.VarValList vv, DBEntry ent, CombRule cr, Delima delima = Delima.SetValue)
		{
			StringBuilder Sb = new StringBuilder();
			if (cr == CombRule.OnlyRowId && ent.rowid == null) return null;

			if ((cr == CombRule.OnlyRowId || cr == CombRule.PreferRowId) && ent.rowid != null)
			{
				Sb.Append("[rowid]='");
				Sb.Append(ent.rowid);
				Sb.Append("'");
				return Sb.ToString();
			}

			int BolbVarCnt = 0;
			foreach (var P in ent.GetClassType().GetProperties())
			{
				if (_PropertyNoValue(ent, P)) continue;

				bool Add = true;
				if (cr == CombRule.ExceptRowId && P.Name == "rowid") Add = false;

				if (!Add) continue;

				var Val = P.GetValue(ent) as DBV;
				var ValStr = $"'{Val.ToString()}'";

				bool IsBlob = Val.IsBLOB();
				if (IsBlob)
				{
					if (!useBlob) continue;
					BolbVarCnt++;
					ValStr = $":Value{BolbVarCnt}";
					vv.Add(new SqliteHelp.VarVal { Name = $"Value{BolbVarCnt}", Value = Val.V });
				}

				Sb.Append("[");
				Sb.Append(P.Name);
				Sb.Append("]");
				Sb.Append(Val.GetOpStr());
				Sb.Append(ValStr);

				switch (delima)
				{
					case Delima.SetValue:
						Sb.Append(", ");
						break;

					case Delima.WhereCond:
						Sb.Append(" AND ");
						break;
				}
			}//foreach

			if (Sb.Length <= 3) return null;

			switch (delima)
			{
				case Delima.SetValue:
					Sb.RemoveLast(2);
					break;

				case Delima.WhereCond:
					Sb.RemoveLast(5);
					break;
			}

			return Sb.ToString();
		}

		#endregion Combine Key Value
	}//class DBMgr

	public class DBCondition
	{
		public string ORDER_BY = null;
		public bool DESC = false;
		public int LIMIT = 0;

		public override string ToString()
		{
			bool HasOB = !string.IsNullOrEmpty(ORDER_BY);
			string OBStr = HasOB ? $"ORDER BY {ORDER_BY}" : "";
			string DEStr = DESC ? "DESC" : "";
			string LIMITStr = LIMIT > 0 ? $"LIMIT {LIMIT}" : "";
			return $" {OBStr} {DEStr} {LIMITStr}";
		}
	}

	public class DBMgr : Singleton<DBMgr>
	{
		public DBMgr()
		{ }

		public DB InterimDB { get; private set; }

		public DB GenuineDB { get; private set; }

		public const string GenuineDBName = "combine.db";
		public const string InterimDBName = "combine_interim.db";
		public const string BackupDBName = "combine_old.db";

		public void InitInterimDB()
		{
			InterimDB = new DB();
			InterimDB.InitDBM(InterimDBName);
		}

		/// <summary>
		/// 初始化本地数据库
		/// </summary>
		public void InitGenuineDB()
		{
			string dbPath = PathUtil.GetPersistPath(App.SharedModule);
			if (FileHelper.IsDirectoryExist(dbPath))
			{
				GenuineDB = new DB();
				GenuineDB.InitDBM(GenuineDBName);
			}
		}
	}

	public class DB : DBMgrT<DB, DBEntry>
	{
		public DB() : base(new DBEntry())
		{
		}

		internal DBMConfig Config;

		public void InitDBM(string dbPath)
		{
			SetSH(dbPath);
			Config = new DBMConfig(dbPath);
		}
	}
}//namespace