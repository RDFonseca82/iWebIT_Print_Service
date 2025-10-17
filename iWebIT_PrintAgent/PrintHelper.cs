using System;

if (root["status"]?.ToString()?.ToLower() == "ok")
{
    var jobs = root["jobs"];
    if (jobs != null)
    {

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
                wc.DownloadFile(fileUrl, tempFile);
                PrintHelper.PrintFile(tempFile, printerName, _sumatraPath);
                WriteLog($"Job {jobId} printed.");


                // notify API (simple GET/POST depending on your API)
            try
            {
                var response = wc.UploadString(_apiConfirmUrl, "POST", $"id={jobId}&status=done");
                WriteLog($"API confirm response for {jobId}: {response}");
            }
            catch (Exception ex)
            {
                WriteLog($"Error confirming job {jobId}: {ex.Message}");
            }
            }   
            catch (Exception ex)
            {
                WriteLog($"Error processing job {jobId}: {ex.Message}");
            }
            finally
            {
                try { File.Delete(tempFile);} catch {}
            }
        }   
    }
}
else
{
// not ok, nothing to do
}



private void WriteLog(string message)
{
try
{
File.AppendAllText(_logPath, $"[{DateTime.Now}] {message}\\r\\n");
}
catch { }
}
