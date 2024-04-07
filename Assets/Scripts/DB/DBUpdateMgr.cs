using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WestBay
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class DBHistory : Attribute
	{
		public string[] History;

		public DBHistory(params string[] pars)
		{
			History = new string[pars.Length];
			for (int i = 0; i < pars.Length; i++)
			{
				History[i] = pars[i];
			}
		}

		public List<string> HistoryList()
		{
			List<string> tl = new List<string>(History);

			return tl;
		}

		public string HistoryStr()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var h in History)
			{
				sb.Append(h);
				sb.Append(";");
			}
			return sb.ToString();
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class DBFieldProperties : Attribute
	{
		public class PropertiesName
		{
			public const string PrimaryKey = "PrimaryKey";
			public const string NotNull = "NotNull";
			public const string AutoIncrement = "AutoIncrement";
		}

		public Dictionary<string, string> Properties;

		/// <summary>
		/// <para>此特性可定义数据库字段的特殊属性，可以分别添加多个特性，也可以一次性都添加进去。</para>
		/// <para>参数：每项可填可不填，顺序随意，不填默认为否，key与value用冒号分隔，冒号为英文半角。例：需要使Name字段既是主键又非空时，就给字段加上特性 [DBFieldProperties("PrimaryKey:1","NotNull:1")]</para>
		/// <para>冒号后的1/0表示是或否，冒号后直接输入字符串表示其内容</para>
		/// <para>1.主键：PrimaryKey:{1/0} 2.非空：NotNull:{1/0} 3.自动递增：AutoIncrement:{1/0} 4.值唯一：Unique:{1/0} 5.默认值：Default:{默认值}</para>
		/// </summary>
		/// <param name="pars"></param>
		public DBFieldProperties(params string[] pars)
		{
			Properties = new Dictionary<string, string>();
			for (int i = 0; i < pars.Length; i++)
			{
				if (!pars[i].Contains(":")) continue;
				var prop = pars[i].Split(':');
				if (prop.Length != 2) continue;
				Properties.Add(prop[0], prop[1]);
			}
		}

		public Dictionary<string, string> PropertiesList()
		{
			var propl = new Dictionary<string, string>(Properties);
			return propl;
		}

		public string PropertiesStr()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var h in Properties)
			{
				sb.Append(h);
				sb.Append(";");
			}
			return sb.ToString();
		}
	}

	public class DBUpdateMgr : Singleton<DBUpdateMgr>
	{
		public InitialUpdateUI DBUpdateUI;

		public class TableInfo
		{
			public Type Table;
			public List<string> OldNames = new List<string>();
		}

		public class ColumnInfo
		{
			public string CurrentType;
			public string Name;
			public bool NotNull = false;
			public bool PrimaryKey = false;
			public bool AutoIncrement = false;
			public bool Unique = false;
			public int Length = 0;
			public List<string> OldNames = new List<string>();
		}

		public class ColumnChange
		{
			public string OldType;
			public string OldName;
			public string CurrentType;
			public string CurrentName;
		}

		public List<Type> DBEList = new List<Type>();
		private DB _interimDB;
		private DB _genuineDB;
		private SqliteHelp _interimSH;
		private SqliteHelp _genuineSH;

		/// <summary>
		/// 数据库更新判断
		/// </summary>
		/// <returns></returns>
		public bool IsNeedUpdate()
		{
			var interimDBPath = PathUtil.GetPersistPath(LobbyUI.SharedModule, DBMgr.InterimDBName);
			if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
			{
				if (FileHelper.IsExist(interimDBPath))
				{
					try
					{
						System.IO.File.Delete(interimDBPath);
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
			var oldDBPath = PathUtil.GetPersistPath(LobbyUI.SharedModule, DBMgr.BackupDBName);
			var curDBPath = PathUtil.GetPersistPath(LobbyUI.SharedModule, DBMgr.GenuineDBName);
			if (FileHelper.IsExist(oldDBPath))
			{
				if (FileHelper.IsExist(curDBPath)) { System.IO.File.Delete(curDBPath); }
				FileHelper.CopyFile(oldDBPath, PathUtil.GetPersistPath(LobbyUI.SharedModule), DBMgr.InterimDBName);

				var bakPath = PathUtil.GetPersistPath("bak", $"{DBMgr.BackupDBName}");
				if (FileHelper.IsExist(bakPath))
				{
					System.IO.File.Delete(bakPath);
				}
				else
				{
					FileHelper.CreatPath(PathUtil.GetPersistPath("bak/"));
				}
				FileHelper.MoveFile(oldDBPath, bakPath);
				return true;
			}
			else
			{
				if (!FileHelper.IsExist(curDBPath)) return true;
			}
			return false;
		}

		public void StartUpdate(Action<bool> onFinish)
		{
			DBUpdateUI = new InitialUpdateUI();
			DBUpdateUI.Begin("更新中");
			Update(delegate (bool isFinish)
			{
				if (isFinish)
				{
					onFinish.Invoke(isFinish);
					DBUpdateUI.Finish();
				}
				else
				{
					DBUpdateUI.SetMessage("update failed");
				}
			}, delegate (float progress)
			{
				DBUpdateUI.SetProgress(progress);
			}).Forget();
		}

		public async UniTaskVoid Update(Action<bool> onFinish = null, Action<float> onProgress = null, Action<string> OnError = null)
		{
			DBMgr.Ins.InitGenuineDB();
			//code
			var til = CheckOutTableInfo();
			_genuineDB = DBMgr.Ins.GenuineDB;
			_genuineSH = _genuineDB.Sh;

			var curDBPath = PathUtil.GetPersistPath(LobbyUI.SharedModule, DBMgr.GenuineDBName);
			if (!FileHelper.IsExist(curDBPath))
			{
				CreateGenuine(til);
			}
			onProgress?.Invoke(5f);

			var tempDBPath = PathUtil.GetPersistPath(LobbyUI.SharedModule, DBMgr.InterimDBName);
			if (FileHelper.IsExist(tempDBPath))
			{
				DBMgr.Ins.InitInterimDB();
				_interimDB = DBMgr.Ins.InterimDB;
				_interimSH = _interimDB.Sh;
				UpdataTempDB(til);
				onProgress?.Invoke(10f);
				await Migrate(til, delegate (float progress)
				{
					onProgress?.Invoke(10f + 80f * progress);
				});
			}

			onProgress?.Invoke(100f);
			onFinish.Invoke(true);
		}

		#region private

		private async UniTask Migrate(List<TableInfo> til, Action<float> onProgress = null)
		{
			if (til == null || til.Count <= 0) return;
			foreach (var ti in til)
			{
				Debug.Log($"transfer data({til.IndexOf(ti) + 1}/{til.Count})：{ti.Table.Name}");
				await MoveData(ti, delegate (float progress)
				{
					onProgress?.Invoke((til.IndexOf(ti) + progress) / til.Count);
				});
			}
		}

		private async UniTask MoveData(TableInfo ti, Action<float> onProgress = null)
		{
			//todo movedata
			await UniTask.Delay(1);
			//if (ti.Table.Equals(typeof(DBEUser)))
			//{
			//	var user = _interimDB.User.SelectData(new DBEUser());
			//	foreach (var item in user)
			//	{
			//		_genuineDB.User.AddData(item);
			//		onProgress?.Invoke((float)(user.IndexOf(item) + 1) / user.Count);
			//		await UniTask.Delay(1);
			//	}
			//	return;
			//}
		}

		private void CreateGenuine(List<TableInfo> til)
		{
			if (til == null || til.Count <= 0) return;
			foreach (var ti in til)
			{
				CrateTable(_genuineSH, ti);
			}
		}

		private void UpdataTempDB(List<TableInfo> til)
		{
			//local
			var lcts = _interimSH.GetTables();

			if (til == null || til.Count <= 0) return;

			foreach (var ti in til)
			{
				if (ti.OldNames == null || ti.OldNames.Count <= 0)//无曾用名
				{
					if (!lcts.Contains(ti.Table.Name))//本地是否有此表
					{
						//无表新建
						CrateTable(_interimSH, ti);
						continue;
					}

					var tablename = IsOldNameOfTable(ti.Table.Name, til);//其它表曾用名
					if (tablename != null && tablename.Count > 0)
					{
						foreach (var oi in tablename)
						{
							if (!lcts.Contains(oi.Table.Name))//本地是否有此表
							{
								bool dm = true;
								var idx = oi.OldNames.IndexOf(ti.Table.Name);

								for (int i = 0; i < idx; i++)
								{
									if (lcts.Contains(oi.OldNames[i]))//本地是否有此曾用名表
									{
										dm = false;
									}
								}

								if (dm)
								{
									_interimSH.ModifyTableName(ti.Table.Name, oi.Table.Name);//更新旧表名

									CrateTable(_interimSH, ti);//建新表

									lcts = _interimSH.GetTables();//更新lcts

									break;
								}
							}
						}
						continue;
					}

					CheckFields(ti);//有表检查字段
				}
				else//有曾用名
				{
					string oldTableName = null;

					foreach (var oldName in ti.OldNames)//检查本地是否有曾用名表
					{
						if (lcts.Contains(oldName))
						{
							oldTableName = oldName;
							break;
						}
					}

					if (string.IsNullOrEmpty(oldTableName))//无曾用名表
					{
						if (!lcts.Contains(ti.Table.Name))//本地是否有此表
						{
							CrateTable(_interimSH, ti);//无表新建
							continue;
						}
						CheckFields(ti);//有表检查字段
					}
					else//有曾用名表
					{
						if (!lcts.Contains(ti.Table.Name))//本地是否有此表
						{
							_interimSH.ModifyTableName(oldTableName, ti.Table.Name);//更新表名

							lcts = _interimSH.GetTables();//更新lcts

							continue;
						}

						CheckFields(ti);//检查字段
					}
				}
			}
		}

		/// <summary>
		/// 新建表
		/// </summary>
		/// <param name="ti"></param>
		private void CrateTable(SqliteHelp sh, TableInfo ti)
		{
			Debug.Log($"数据库新建表：{ti.Table.Name}");

			var cCol = ColumnHistoryName(ti);//代码字段信息

			List<string> nl = new List<string>();
			foreach (var col in cCol)//编辑字段信息
			{
				if (col.Name == "rowid") continue;//过滤rowid
				var str = col.Name;

				if (col.PrimaryKey)
				{
					if (col.AutoIncrement)
					{
						str += " INTEGER PRIMARY KEY AUTOINCREMENT";
					}
					else
					{
						str += " PRIMARY KEY";
					}
				}

				if (col.NotNull)
				{
					str += " NOT NULL";
				}

				nl.Add(str);
			}

			sh.CreateTable(ti.Table.Name, nl);//新建表
		}

		private void CheckFields(TableInfo ti)
		{
			var lcColumns = _interimSH.GetColumns(ti.Table.Name);//本地表查询字段

			if (lcColumns == null || lcColumns.Count <= 0) Debug.LogWarning($"{ti.Table.Name} is Empty");//空

			var cCol = ColumnHistoryName(ti);//代码字段信息

			foreach (var col in cCol)
			{
				if (col.Name.CompareTo("rowid") == 0) continue;//过滤rowid

				if (IsLocalFieldsExist(lcColumns, col.CurrentType, col.Name)) continue;//本地有此字段

				Debug.Log($"[{ti.Table.Name}]表新建字段：{col.Name}");

				_interimSH.AddField(ti.Table.Name, col.Name, col.CurrentType);//新建字段

				if (col.OldNames == null || col.OldNames.Count <= 0) continue;//代码字段无曾用名

				var usedName = IsLocalFieldsExist(lcColumns, col.CurrentType, col.OldNames);//查找曾用名

				if (string.IsNullOrEmpty(usedName)) continue;//本地无曾用名字段

				_interimSH.MigrateRowData(usedName, col.Name, ti.Table.Name);//迁移数据
			}
		}

		/// <summary>
		/// 查询字段是否存在
		/// </summary>
		/// <param name="columnsSql"></param>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns>返回是否存在</returns>
		private bool IsLocalFieldsExist(List<SqliteHelp.ColumnType> columnsSql, string type, string name)
		{
			if (columnsSql == null || columnsSql.Count <= 0) return false;

			foreach (var sqlItem in columnsSql)
			{
				if (sqlItem.name.ToString().CompareTo(name) == 0) return true;
			}

			return false;
		}

		/// <summary>
		/// 查询曾用名字段是否存在
		/// </summary>
		/// <param name="columnsSql"></param>
		/// <param name="type"></param>
		/// <param name="nameList"></param>
		/// <returns>返回存在的曾用名</returns>
		private string IsLocalFieldsExist(List<SqliteHelp.ColumnType> columnsSql, string type, List<string> nameList)
		{
			if (columnsSql == null || columnsSql.Count <= 0) return null;

			foreach (var name in nameList)
			{
				foreach (var sqlItem in columnsSql)
				{
					if (sqlItem.name.ToString().CompareTo(name) == 0 /*&& sqlItem.type == type*/) return name;
				}
			}

			return null;
		}

		private List<TableInfo> IsOldNameOfTable(string tn, List<TableInfo> til)
		{
			List<TableInfo> nl = new List<TableInfo>();
			foreach (var ti in til)
			{
				if (ti.OldNames == null || ti.OldNames.Count <= 0) continue;
				if (ti.OldNames.Contains(tn))
				{
					Debug.Log($"[{tn}]存在于【{ti.Table.Name}】表曾用名中");
					nl.Add(ti);
				}
			}
			return nl;
		}

		private List<string> TableHistroyName(Type tbl)
		{
			MemberInfo info = tbl;
			object[] attributes = info.GetCustomAttributes(typeof(DBHistory), true);
			if (attributes.Length <= 0) return null;
			DBHistory dbh = attributes[0] as DBHistory;
			List<string> thl = dbh.HistoryList();
			return thl;
		}

		private List<TableInfo> CheckOutTableInfo()
		{
			List<TableInfo> til = new List<TableInfo>();

			if (DBEList == null || DBEList.Count <= 0)
			{
				Debug.LogError("DBEList is NULL");
				return null;
			}

			foreach (var ct in DBEList)
			{
				TableInfo ti = new TableInfo
				{
					Table = ct,
					OldNames = TableHistroyName(ct)
				};

				if (!til.Contains(ti))
				{
					til.Add(ti);
				}
			}
			return til;
		}

		private List<ColumnInfo> ColumnHistoryName(TableInfo tbi)
		{
			List<ColumnInfo> ColHists = new List<ColumnInfo>();
			var Ps = tbi.Table.GetProperties();
			foreach (var P in Ps)
			{
				ColumnInfo ch = new ColumnInfo()
				{
					CurrentType = P.GetType().Name,
					Name = P.Name
				};

				var historyAtribs = P.GetCustomAttributes(typeof(DBHistory), true);
				if (historyAtribs.Length > 0)
				{
					DBHistory dbh = historyAtribs[0] as DBHistory;
					foreach (var Name in dbh.History)
					{
						ch.OldNames.Add(Name);
					}
				}

				var fieldPropsAtribs = P.GetCustomAttributes(typeof(DBFieldProperties), true);
				if (fieldPropsAtribs.Length > 0)
				{
					foreach (var fpas in fieldPropsAtribs)
					{
						DBFieldProperties dbf = fpas as DBFieldProperties;

						ch.PrimaryKey = CheckFieldPropsBoolean(dbf.Properties, DBFieldProperties.PropertiesName.PrimaryKey);

						ch.NotNull = CheckFieldPropsBoolean(dbf.Properties, DBFieldProperties.PropertiesName.NotNull);

						ch.AutoIncrement = CheckFieldPropsBoolean(dbf.Properties, DBFieldProperties.PropertiesName.AutoIncrement);
					}
				}

				ColHists.Add(ch);
			}
			return ColHists;
		}

		private bool CheckFieldPropsBoolean(Dictionary<string, string> dic, string key)
		{
			return dic.ContainsKey(key) && dic[key].Equals("1");
		}

		private List<ColumnChange> AnalysisColumnsHistory(List<ColumnInfo> colHists, List<SqliteHelp.ColumnType> colsOld)
		{
			List<ColumnChange> ColChg = new List<ColumnChange>();

			foreach (var CT in colsOld)
			{
				if (CT.name == null) continue;
				foreach (var CH in colHists)
				{
					if (!CH.OldNames.Contains(CT.name)) continue;
					ColChg.Add(new ColumnChange
					{
						CurrentName = CH.Name,
						CurrentType = CH.CurrentType,
						OldName = CT.name,
						OldType = CT.type
					});

					CT.name = null;
					CH.OldNames.Remove(CT.name);
				}
			}

			return ColChg;
		}

		#endregion private
	}
}