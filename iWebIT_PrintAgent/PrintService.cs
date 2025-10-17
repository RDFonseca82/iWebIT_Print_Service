using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

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

            if (!Directory.Exists(_tempFolder))
                Directory.CreateDirectory(_tempFolder);
        }

        public void ProcessJobs(string json)
        {
            try
            {
                var root = JObject.Parse(json);
                if (root["status"]?.ToString().ToLower() != "ok") return;

                var jobs = root["jobs"];
                if (jobs == null) return;

                using (var wc = new WebClient())
                {
                    foreach (var job in jobs)
                    {
                        string fileUrl = job["file_url"]?.ToString() ?? "";
                        string printerName = job["printer_name"]?.ToString() ?? "";
                        string jobId = job["job_id"]?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(printerName))
                        {
                            WriteLog($"Job {jobId} inválido.");
                            continue;
                        }

                        WriteLog($"Recebido job {jobId}: {fileUrl} -> {printerName}");

                        string tempFile = Path.Combine(_tempFolder, Path.GetFileName(new Uri(fileUrl).LocalPath));
                        try
                        {
                            wc.DownloadFile(fileUrl, tempFile);
                            PrintHelper.PrintFile(tempFile, printerName, _sumatraPath);
                            WriteLog($"Job {jobId} impresso.");

                            try
                            {
                                var response = wc.UploadString(_apiConfirmUrl, "POST", $"id={jobId}&status=done");
                                WriteLog($"Confirmação API {jobId}: {response}");
                            }
                            catch (Exception ex)
                            {
                                WriteLog($"Erro confirmar job {jobId}: {ex.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog($"Erro processar job {jobId}: {ex.Message}");
                        }
                        finally
                        {
                            try { File.Delete(tempFile); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog("Erro geral: " + ex.Message);
            }
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
