using System.Collections.Generic;

namespace WestBay
{
	public class FileVersionInfo
	{
		public string File { get; set; }
		public string MD5 { get; set; }
		public long Size { get; set; }
	}

	public class PackageConfig
	{
		public string PackageName { get; set; }
		public long Size { get; set; }

		public Dictionary<string, FileVersionInfo> FileInfoDict = new Dictionary<string, FileVersionInfo>();
	}
}