using System.Diagnostics;
using System.Text;

namespace WestBay
{
	public class CommandRunner
	{
		private readonly string _executableFile;
		private readonly Process _process;

		public CommandRunner(string executableFile)
		{
			_executableFile = executableFile;
			_process = new Process();
		}

		public bool Run(string workingDirectory, string arguments)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(_executableFile, arguments)
			{
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WorkingDirectory = workingDirectory,
				StandardErrorEncoding = Encoding.UTF8,
				StandardOutputEncoding = Encoding.UTF8
			};

			_process.StartInfo = startInfo;
			_process.Start();

			LastStandardOutput = _process.StandardOutput.ReadToEnd().TrimEnd('\n');
			LastStandardError = _process.StandardError.ReadToEnd().TrimEnd('\n');
			bool result = string.IsNullOrWhiteSpace(LastStandardError);
			_process.WaitForExit(10);

			return result;
		}

		public string LastStandardOutput { get; private set; }
		public string LastStandardError { get; private set; }
	}
}