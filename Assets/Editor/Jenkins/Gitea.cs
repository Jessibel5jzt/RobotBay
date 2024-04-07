using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace WestBay
{
	public static class Gitea
	{
		public static readonly string AccessToken = "4d99c0cddf3d209eee07cc12a99c0ef4e7dcfd87";
		public static readonly string ServerUrl = "http://192.168.8.250:3000";
		public static readonly string PlatformRepoFullName = "Rups/Platform";
		public static readonly string CommonRepoFullName = "Rups/Common";
		public static readonly string GameRepoFullNamePrefix = "Games";
		public static readonly string ProductRepoFullNamePrefix = "Rups-prod";

		public static async Task<string> RetieveRepoCommitInfo(string fullRepoName, string branch)
		{
			string result = string.Empty;
			string url = $"{ServerUrl}/api/v1/repos/{fullRepoName}/branches/{System.Uri.EscapeDataString(branch)}?access_token={AccessToken}";
			var data = await WebReqHelper.GetRequest(url);
			if (data != null)
			{
				result = System.Text.Encoding.Default.GetString(data);
			}

			return result;
		}

		public static string RetieveRepoCommitInfoSync(string fullRepoName, string branch)
		{
			string result = string.Empty;
			string url = $"{ServerUrl}/api/v1/repos/{fullRepoName}/branches/{System.Uri.EscapeDataString(branch)}?access_token={AccessToken}";
			var data = WebReqHelper.GetRequestSync(url);
			if (data != null)
			{
				result = System.Text.Encoding.Default.GetString(data);
			}

			return result;
		}
	}
}