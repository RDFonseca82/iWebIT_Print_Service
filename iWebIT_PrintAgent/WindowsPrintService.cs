using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace iWebIT_PrintAgent
{
    public class WindowsPrintService : IHostedService, IDisposable
    {
        private System.Threading.Timer? _timer;
        private PrintService _printService;
        private readonly string _tempFolder;
        private readonly string _logPath;
        private readonly string _sumatraPath;
        private readonly string _apiJobsUrl;
        private readonly string _apiConfirmUrl;
        private readonly int _pollingIntervalSeconds;

        public WindowsPrintService(IConfiguration config)
        {
            var settings = config.GetSection("PrintAgent");
            _tempFolder = settings["TempFolder"] ?? Path.GetTempPath();
            _logPath = settings["LogPath"] ?? Path.Combine(_tempFolder, "print.log");
            _sumatraPath = settings["SumatraPath"] ?? string.Empty;
            _apiJobsUrl = settings["JobsUrl"] ?? string.Empty;
            _apiConfirmUrl = settings["ConfirmUrl"] ?? string.Empty;
            _pollingIntervalSeconds = int.Parse(settings["PollingIntervalSeconds"] ?? "15");

            if (!Directory.Exists(_tempFolder))
                Directory.CreateDirectory(_tempFolder);

            _printService = new PrintService(_tempFolder, _logPath, _sumatraPath, _apiConfirmUrl);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            WriteLog("WindowsPrintService iniciado.");
            _timer = new System.Threading.Timer(async _ => await ProcessJobsAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(_pollingIntervalSeconds));
            return Task.CompletedTask;
        }

        private async Task ProcessJobsAsync()
        {
            try
            {
                using var http = new HttpClient();
                string json = await http.GetStringAsync(_apiJobsUrl);
                _printService.ProcessJobs(json);
            }
            catch (Exception ex)
            {
                WriteLog("Erro ao buscar/processar jobs: " + ex.Message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WriteLog("WindowsPrintService parado.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
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
