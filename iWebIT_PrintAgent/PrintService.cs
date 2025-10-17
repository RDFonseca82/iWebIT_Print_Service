using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Printing;

namespace iWebIT_PrintAgent
{
    public class PrintService
    {
        private readonly string _tempFolder;
        private readonly string _logPath;
        private readonly string _sumatraPath;
        private readonly string _apiConfirmUrl;

        public PrintService(string tempFolder, string logPath, string sumatraPath, string apiConfirmUrl)
        {
            _tempFolder = tempFolder;
            _logPath = logPath;
            _sumatraPath = sumatraPath;
            _apiConfirmUrl = apiConfirmUrl;
        }

        public void ProcessJobs(string json)
        {
            var root = JObject.Parse(json);

            if (root["status"]?.ToString()?.ToLower() != "ok")
                return;

            var jobs = root["jobs"];
            if (jobs == null) return;

            foreach (var job in jobs)
            {
                string fileUrl = job["file_url"]?.ToString() ?? string.Empty;
                string printerName = job["printer_name"]?.ToString() ?? string.Empty;
                string jobId = job["job_id"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(printerName))
                {
                    WriteLog("Job missing file_url or printer_name.");
                    continue;
                }

                WriteLog($"Received job {jobId}: {fileUrl} -> {printerName}");

                string tempFile = Path.Combine(_tempFolder, Path.GetFileName(new Uri(fileUrl).LocalPath));
                try
                {
                    DownloadFileAsync(fileUrl, tempFile).Wait();
                    PrintFile(tempFile, printerName);
                    WriteLog($"Job {jobId} printed.");
                    ConfirmJobAsync(jobId).Wait();
                }
                catch (Exception ex)
                {
                    WriteLog($"Error processing job {jobId}: {ex.Message}");
                }
                finally
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        private async Task DownloadFileAsync(string url, string dest)
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(dest, bytes);
        }

        private async Task ConfirmJobAsync(string jobId)
        {
            try
            {
                using var http = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("id", jobId),
                    new KeyValuePair<string,string>("status","done")
                });
                var response = await http.PostAsync(_apiConfirmUrl, content);
                WriteLog($"API confirm response for {jobId}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                WriteLog($"Error confirming job {jobId}: {ex.Message}");
            }
        }

        public void PrintFile(string path, string printerName)
        {
            using Image img = Image.FromFile(path);
            using PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            pd.PrintPage += (sender, args) =>
            {
                args.Graphics.DrawImage(img, args.MarginBounds);
            };
            pd.Print();
        }

        private void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(_logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
