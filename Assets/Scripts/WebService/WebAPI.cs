namespace WestBay
{
	/// <summary>
	/// Http web 服务API地址
	/// </summary>
	public class WebAPI
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 服务器域名
		/// </summary>
		public static string Server_Default_Host => "http://api.fftai.com:8080";

		public static string ServerName_DOOP = "doop";
		public static string ServerName_DAS = "das";

		private static string _serverHost = string.Empty;

		public static string Server_Host
		{
			get
			{
				if (!string.IsNullOrEmpty(_serverHost)) return _serverHost;

				var model = DBMgr.Ins.GenuineDB.Config.GetValue("WebRequest_Host");
				if (!string.IsNullOrEmpty(model))
				{
					_serverHost = model;
				}
				else
				{
					if (IniMgr.Config != null) _serverHost = IniMgr.Config.GetValue("WebRequest_Host");
				}

				return _serverHost;
			}
			set
			{
				_serverHost = value;
				WebReqHelper.WebServerRequest().Forget();
				Debug.Log($"[WebRequest]Web Host:{_serverHost}");
			}
		}

		/// <summary>
		/// 服务器列表
		/// </summary>
		public static string Server_List_Url
		{
			get
			{
				if (IniMgr.Config != null) return $"http://{Server_Host}{Server_List_API}";
				else return string.Empty;
			}
		}

		/// <summary>
		/// 获取公网IP
		/// </summary>
		public static string Public_IP_Url => "http://icanhazip.com/";

		//商店地址
		public static string Web_Store_Url = string.Empty;

		//DAS Url
		public static string Web_DAS_Url = string.Empty;

		//DOOP Url
		public static string Web_DOOP_Url = string.Empty;

		private static int _webLogEnable = -1;

		public static bool Web_Log_Enable
		{
			get
			{
				if (_webLogEnable != -1) return _webLogEnable == 1;

				if (IniMgr.Config != null) _webLogEnable = IniMgr.Config.GetValue("WebRequest_Log").Equals("1") ? 1 : 0;
				else _webLogEnable = 0;

				return _webLogEnable == 1;
			}
		}

		public static bool IsTest => true && App.IsDebug;
		public static readonly string DAS_Url_LocalTest = "http://47.103.97.21:8084";
		public static readonly string DOOP_Url_LocalTest = "http://47.103.97.21:8080";
		public static string Public_IP = string.Empty;

		///////////////////////////////////////////////////////////////////////////////////////////////////
		//服务端列表
		public const string Server_List_API = "/sysIps/serverList";

		//设备保存
		public const string Machine_Save_API = "/machineInfo/save";

		//设备日志记录
		public const string MachineLog_Save_API = "/machineLog/save";

		///////////////////////////////////////////////////////////////////////////////////////////////////
		//用户
		public const string User_Login_API = "/sys/login/restful";

		public const string User_Add_API = "/users/add";
		public const string User_Save_API = "/users/sync";
		public const string User_Update_API = "/users/update";
		public const string User_Delete_API = "/users/delete";
		public const string User_List_API = "/users/userList";
		public const string User_Change_Password_API = "/users/changePassword";
		public const string Verification_Code_API = "/api/verificationCode";

		/// <summary>
		/// 用户在线短连接
		/// </summary>
		public const string User_ActiveUrl_API = "/users/getActiveUrl";

		public const string User_ActiveQRImage_API = "/wx/qrImageStr";

		//用户日志
		public const string UserLog_Save_API = "/userLogs/save";

		//用户存档
		public const string UserArchive_Save_API = "/userArchives/save";

		public const string UserArchive_Select_API = "/userArchives/selectUserArchive";

		//用户成就
		public const string UserAchivement_Save_API = "/userAchivements/saveOrUpdate";

		public const string UserAchivement_List_API = "/userAchivements/getUserAchivements";

		//用户训练统计
		public const string UserTrainStat_Save_API = "/userTrainStates/saveOrUpdate";

		public const string UserTrainStat_Select_API = "/userTrainStates/selectTrainStateByUser";

		//用户训练评估
		public const string UserEvaluation_Save_API = "/userTrainEvaluations/save";

		//用户训练日志
		public const string UserTrainLog_Save_API = "/userTrainLogs/save";

		//用户训练报告
		public const string UserTrainReport_Save_API = "/userTrainReports/save";

		//用户训练轨迹
		public const string UserTrajectory_Save_API = "/userTrajectorys/save";

		public const string UserTrajectory_List_API = "/userTrajectorys/list";

		///////////////////////////////////////////////////////////////////////////////////////////////////
		//游戏
		public const string Game_List_API = "/games/list";

		public const string User_Game_List_API = "/games/webList";
		public const string GameVersion_List_API = "/games/selectGameFileList";

		///////////////////////////////////////////////////////////////////////////////////////////////////
		//上传RUPS
		public const string RUPSFile_Upload_API = "/files/uploadModule";

		//上传子模块
		public const string ModuleFile_Upload_API = "/files/uploadModuleFile";

		//版本信息
		public const string Version_List_API = "/files/selectModuleFileList";
	}
}