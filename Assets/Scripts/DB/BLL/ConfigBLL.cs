using System.Collections.Generic;

namespace WestBay
{
	public class ConfigBLL
	{
		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public bool Exists(string key)
		{
			var dbEntity = DBMgr.Ins.GenuineDB.Config.SelectData1(new DBEConfig { Key = key });
			return dbEntity == null;
		}

		/// <summary>
		/// 增加一条数据
		/// </summary>
		public bool Add(ConfigModel model)
		{
			if (DBMgr.Ins == null || DBMgr.Ins.GenuineDB == null) return false;

			return DBMgr.Ins.GenuineDB.Config.AddData(model.EncodeDB());
		}

		/// <summary>
		/// 更新一条数据
		/// </summary>
		public bool Update(ConfigModel model)
		{
			if (DBMgr.Ins == null || DBMgr.Ins.GenuineDB == null) return false;

			return DBMgr.Ins.GenuineDB.Config.ModifyData(new DBEConfig() { Key = model.Key }, model.EncodeDB());
		}

		/// <summary>
		/// 删除一条数据
		/// </summary>
		public bool Delete(ConfigModel model)
		{
			return DBMgr.Ins.GenuineDB.Config.DeleteData(new DBEConfig() { Key = model.Key });
		}

		/// <summary>
		/// 删除一条数据
		/// </summary>
		public bool DeleteList(string idlist)
		{
			return false;
		}

		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public ConfigModel GetModel(string key)
		{
			if (DBMgr.Ins.GenuineDB == null) return null;

			ConfigModel result = null;

			var dbEntity = DBMgr.Ins.GenuineDB.Config.SelectData1(new DBEConfig { Key = key });
			if (dbEntity != null)
			{
				result = new ConfigModel(dbEntity);
			}

			return result;
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public List<ConfigModel> GetModelList()
		{
			List<ConfigModel> result = new List<ConfigModel>();
			if (DBMgr.Ins == null || DBMgr.Ins.GenuineDB == null) return result;

			var dbEntity = new DBEConfig();
			List<DBEConfig> list = DBMgr.Ins.GenuineDB.Config.SelectData(dbEntity);
			for (int i = 0; i < list.Count; ++i)
			{
				result.Add(new ConfigModel(list[i]));
			}

			return result;
		}
	}
}