using System.Diagnostics;

namespace ScanLocalNetwork
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            await process.StandardInput.WriteLineAsync("arp -a");
            var output = await process.StandardOutput.ReadLineAsync();
            Console.WriteLine(output);
        }
    }
}