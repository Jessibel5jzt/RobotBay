using SFB;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WestBay
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class OpenDialogFile
	{
		public int structSize = 0;
		public IntPtr dlgOwner = IntPtr.Zero;
		public IntPtr instance = IntPtr.Zero;
		public String filter = null;
		public String customFilter = null;
		public int maxCustFilter = 0;
		public int filterIndex = 0;
		public String file = null;
		public int maxFile = 0;
		public String fileTitle = null;
		public int maxFileTitle = 0;
		public String initialDir = null;
		public String title = null;
		public int flags = 0;
		public short fileOffset = 0;
		public short fileExtension = 0;
		public String defExt = null;
		public IntPtr custData = IntPtr.Zero;
		public IntPtr hook = IntPtr.Zero;
		public String templateName = null;
		public IntPtr reservedPtr = IntPtr.Zero;
		public int reservedInt = 0;
		public int flagsEx = 0;

		public OpenDialogFile()
		{
			dlgOwner = DllOpenFileDialog.GetActiveWindow();
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class OpenDialogDir
	{
		public IntPtr hwndOwner = IntPtr.Zero;
		public IntPtr pidlRoot = IntPtr.Zero;
		public String pszDisplayName = null;
		public String lpszTitle = null;
		public UInt32 ulFlags = 0;
		public IntPtr lpfn = IntPtr.Zero;
		public IntPtr lParam = IntPtr.Zero;
		public int iImage = 0;
	}

	public class DllOpenFileDialog
	{
		[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
		public static extern bool GetOpenFileName([In, Out] OpenDialogFile ofn);

		[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
		public static extern bool GetSaveFileName([In, Out] OpenDialogFile ofn);

		[DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
		public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

		[DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
		public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

		[DllImport("user32.dll")]
		public static extern IntPtr GetActiveWindow();
	}

	public class KeyboardEvent
	{
		[DllImport("user32.dll", EntryPoint = "keybd_event")]
		public static extern void Keybd_event(

				byte bvk,//虚拟键值

				byte bScan,//0

				int dwFlags,//0按下，1按住，2释放

				int dwExtraInfo//0

				);
	}

	public class OpenFileHelper
	{
		public static string GetSavePath()
		{
			var path = string.Empty;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			var paths = StandaloneFileBrowser.OpenFolderPanel(string.Empty, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), false);
			if (paths.Length > 0)
			{
				path = paths[0];
			}
#else
			path = PathUtil.GetPersistPath();
#endif
			return path;
		}

		public static void OpenDirectory(string path)
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			if (string.IsNullOrEmpty(path)) return;

			path = path.Replace("/", "\\");
			if (!Directory.Exists(path))
			{
				return;
			}
			System.Diagnostics.Process.Start("explorer.exe", path);
#else
		return;
#endif
		}
	}
}