using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	public class DevToolMenu : EditorWindow
	{
		private static Process _linkSM = null;

		[MenuItem("Tools/链接模块", false, 1)]
		public static void LinkSM()
		{
			if (_linkSM != null && !_linkSM.HasExited)
			{
				_linkSM.Kill();
				_linkSM.Close();
				_linkSM = null;
			}

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			string projPath = projPathAsserts.Substring(0, projPathAsserts.LastIndexOf('/'));
			string linkExe = $"{projPath}/Tools/LinkSM.exe";
			ProcessStartInfo startInfo = new ProcessStartInfo(linkExe, $"{App.ProductTypeInsideEditor}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			_linkSM = Process.Start(startInfo);
		}

		private static Process _exportTool = null;

		[MenuItem("Tools/导出工具", false, 6)]
		public static void ExportTool()
		{
			if (_exportTool != null && !_exportTool.HasExited)
			{
				_exportTool.Kill();
				_exportTool.Close();
				_exportTool = null;
			}

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			string projPath = projPathAsserts.Substring(0, projPathAsserts.LastIndexOf('/'));
			string linkExe = $"{projPath}/Tools/ExportTool.exe";
			ProcessStartInfo startInfo = new ProcessStartInfo(linkExe, $"{App.ProductTypeInsideEditor}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			_exportTool = Process.Start(startInfo);
		}

		private static Process _integrationTool = null;

		[MenuItem("Tools/部署工具", false, 28)]
		public static void IntegrationTool()
		{
			if (_integrationTool != null && !_integrationTool.HasExited)
			{
				_integrationTool.Kill();
				_integrationTool.Close();
				_integrationTool = null;
			}

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			string projPath = projPathAsserts.Substring(0, projPathAsserts.LastIndexOf('/'));
			string linkExe = $"{projPath}/../Tool/Output/Deploy/FlySoft.Deploy.exe";
			ProcessStartInfo startInfo = new ProcessStartInfo(linkExe, $"{App.ProductTypeInsideEditor}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			_integrationTool = Process.Start(startInfo);
		}

		private static Process _gitTool = null;

		[MenuItem("Tools/GIT工具", false, 28)]
		public static void GitTool()
		{
			if (_integrationTool != null && !_integrationTool.HasExited)
			{
				_integrationTool.Kill();
				_integrationTool.Close();
				_integrationTool = null;
			}

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			string projPath = projPathAsserts.Substring(0, projPathAsserts.LastIndexOf('/'));
			string linkExe = $"{projPath}/Tools/FlySoft.GitTool.exe";
			ProcessStartInfo startInfo = new ProcessStartInfo(linkExe, $"{App.ProductTypeInsideEditor}")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};

			_gitTool = Process.Start(startInfo);
		}
	}
}