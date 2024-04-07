using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace WestBay
{
	public class DBValType
	{
		public virtual Type GetClassType()
		{ return GetType(); }
	}

	public class SqliteHelp
	{
		public class VarVal
		{
			public string Name { get; set; }
			public object Value { get; set; }
		}

		public class VarValList : List<VarVal>
		{ }

		protected SqliteConnection DbSqliteConnection;
		private readonly string _database;

		public SqliteHelp(string dbFileName)
		{
			_database = dbFileName;
		}

		private void OpenSQL()
		{
			if (DbSqliteConnection != null)
			{
				DbSqliteConnection.Open();
				return;
			}

			try
			{
				string Src = $"data source ={PathUtil.GetPersistPath(App.SharedModule, _database)}";
				DbSqliteConnection = new SqliteConnection(Src);
				DbSqliteConnection.Open();
			}
			catch (Exception e)
			{
				Log(e.ToString());
			}
		}

		/// <summary>
		/// 关闭数据库
		/// </summary>
		public void CloseSQL()
		{
			if (DbSqliteConnection != null)
			{
				try
				{
					DbSqliteConnection.Close();
				}
				catch (Exception e)
				{
					Log(e.ToString());
				}
			}
		}

		private static HashSet<string> TNs = new HashSet<string>();

		public string GetTableName(DBValType ent)
		{
			string TN = ent.GetClassType().Name;
			if (TNs.Contains(TN)) return TN;

			string sqlString = $"SELECT COUNT(*) FROM [sqlite_master] where type='table' AND name= '{TN}'";
			int Cnt = Execute(sqlString);
			if (Cnt == 0)
			{
				Log($"[DBM][{TN}][GetTableName] No Such Table");
				TN = $"NoTable->{TN}";
			}
			else
			{
				TNs.Add(TN);
			}
			return TN;
		}

		public enum DataType
		{ }

		public class ColumnType : DBValType
		{
			public INT64 cid { set; get; }
			public TEXT name { set; get; }
			public TEXT type { set; get; }
			public INT64 not_null { get; set; }
			public TEXT def_value { get; set; }
			public INT64 primary_key { get; set; }

			public PropertyInfo GetProperty(string n)
			{ return GetType().GetProperty(n); }
		}

		public bool CreateTable(string tbn, List<string> fnl)
		{
			try
			{
				if (fnl == null || fnl.Count <= 0) return false;
				OpenSQL();
				string fn = "";
				foreach (var item in fnl)
				{
					fn += item + ",";
				}
				fn = fn.Substring(0, fn.Length - 1);
				string sqlString = $"CREATE TABLE IF NOT EXISTS {tbn}({fn})";

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception e)
			{
				Log(e.Message);
				return false;
			}
			finally
			{
				CloseSQL();
			}
		}

		public List<string> GetTables()
		{
			try
			{
				OpenSQL();
				string Sql = $"SELECT name FROM sqlite_master where type='table' order by name";
				using (SqliteCommand cmd = new SqliteCommand(Sql, DbSqliteConnection))
				{
					SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();
					if (reader.HasRows)
					{
						List<string> tl = new List<string>();
						while (reader.Read())
						{
							tl.Add(reader.GetString(0));
						}
						return tl;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				CloseSQL();
			}
			return new List<string>();
		}

		/// <summary>
		/// 数据库列名称及类型
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public List<ColumnType> GetColumns(string tableName)
		{
			try
			{
				OpenSQL();
				string Sql = $"PRAGMA TABLE_INFO ({tableName})";

				using (SqliteCommand cmd = new SqliteCommand(Sql, DbSqliteConnection))
				{
					SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();

					var Ret = ToList<ColumnType>(new ColumnType(), reader);
					return Ret;
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				CloseSQL();
			}
			return new List<ColumnType>();
		}

		public bool MigrateRowData(string oFN, string nFN, string tableName)
		{
			try
			{
				OpenSQL();
				string sqlString = $"UPDATE {tableName} Set {nFN} = {oFN}";

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception e)
			{
				Log(e.Message);
				return false;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 修改表名
		/// </summary>
		/// <param name="sqlString"></param>
		/// <param name="args"></param>
		public bool ModifyTableName(string tableName, string newName)
		{
			try
			{
				OpenSQL();
				string sqlString = $"ALTER TABLE {tableName} RENAME TO {newName}";
				////Log(sqlString);

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception e)
			{
				Log(e.Message);
				return false;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 添加字段
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public bool AddField(string tableName, string fieldName, string type)
		{
			try
			{
				OpenSQL();
				string sqlString = $"ALTER TABLE {tableName} ADD COLUMN {fieldName}";

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception e)
			{
				Log(e.Message);
				return false;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 查询数据库表行数或者执行删除操作
		/// </summary>
		/// <param name="sqlString"></param>
		/// <returns></returns>
		public int Execute(string sqlString)
		{
			int count = 0;
			try
			{
				OpenSQL();

				SqliteCommand sqlitecmd = new SqliteCommand(sqlString, DbSqliteConnection);
				count = Convert.ToInt32(sqlitecmd.ExecuteScalar());
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			finally
			{
				CloseSQL();
			}
			return count;
		}

		/// <summary>
		/// 执行插入和更新语句，无返回结果集
		/// </summary>
		/// <param name="sqlString"></param>
		/// <param name="args"></param>
		public bool ExecuteUpdate(string sqlString, VarValList vv)
		{
			try
			{
				OpenSQL();

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					if (vv != null)
					{
						foreach (var V in vv)
						{
							cmd.Parameters.Add(V.Name, DbType.Binary).Value = V.Value;
						}
					}
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception e)
			{
				Log(e.Message);
				return false;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 执行查询语句，返回结果集
		/// </summary>
		/// <param name="sqlString"></param>
		/// <param name="args"></param>
		/// <returns>List 不会是 null。没有时 Count == 0 </returns>
		public List<T> ExecuteQuery<T>(DBValType ent, string sqlString) where T : DBValType, new()
		{
			try
			{
				OpenSQL();
				////Log(sqlString);

				using (SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection))
				{
					SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					cmd.Parameters.Clear();

					var Ret = ToList<T>(ent, reader);
					reader.Close();
					return Ret;
				}
			}
			catch (Exception e)
			{
				Log(e.ToString());
				throw e;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 将DataReader转换成ArrayList
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		private List<T> ToList<T>(DBValType t, SqliteDataReader reader) where T : DBValType, new()
		{
			List<T> ListRet = new List<T>();

			while (reader.Read())
			{
				Type Typ = t.GetType();
				DBValType newT = Activator.CreateInstance(Typ) as T;

				for (int i = 0; i < reader.FieldCount; i++)
				{
					object Val = reader.GetValue(i);
					if (Val.GetType() == typeof(DBNull))
					{
						continue;
					}

					string PropName = reader.GetName(i);
					var P = Typ.GetProperty(PropName);
					if (P == null)
					{
						//这里之前LogError，现改为continue并LogWarning，忽略本地数据表多余字段
						//Debug.LogWarning($"Cant find the property : {PropName}");
						continue;
					}
					else
					{
						DBV PropDBType = Activator.CreateInstance(P.PropertyType) as DBV;
						PropDBType.SetValue(Val);
						P.SetValue(newT, PropDBType);
					}
				}

				ListRet.Add((T)newT);
			}

			return ListRet;
		}

		/// <summary>
		///  执行查询语句，返回结果集
		/// </summary>
		/// <param name="sqlString"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public ArrayList ExecuteQuery(string sqlString, params SqliteParameter[] args)
		{
			try
			{
				OpenSQL();
				SqliteCommand cmd = new SqliteCommand(sqlString, DbSqliteConnection);
				if (args != null)
					cmd.Parameters.AddRange(args);

				SqliteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);//关闭datareader的时候 ，把相关的connection也一同关闭
				cmd.Parameters.Clear();
				return DataReaderToArrayList(reader);
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
				throw e;
			}
			finally
			{
				CloseSQL();
			}
		}

		/// <summary>
		/// 将DataReader转换成ArrayList
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		private ArrayList DataReaderToArrayList(SqliteDataReader reader)
		{
			ArrayList array = new ArrayList();
			try
			{
				while (reader.Read())
				{
					Hashtable record = new Hashtable();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						object cellValue = reader[i];
						if (cellValue.GetType() == typeof(DBNull))
						{
							cellValue = null;
						}
						record[reader.GetName(i)] = cellValue;
					}
					array.Add(record);
				}
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
			}
			finally
			{
				reader.Close();
			}
			return array;
		}

		private void Log(string log)
		{
			//App.Log(Subsystem.Database, log);
			Debug.Log(log);
		}
	}//class
}//namespace