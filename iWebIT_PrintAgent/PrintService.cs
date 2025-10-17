using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace iWebIT_PrintAgent
{
public static class PrintHelper
{
public static void PrintFile(string filePath, string printerName, string sumatraPath)
{
if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
{
if (!File.Exists(sumatraPath))
throw new FileNotFoundException("SumatraPDF not found at " + sumatraPath);


var psi = new ProcessStartInfo
{
FileName = sumatraPath,
Arguments = $"-print-to \"{printerName}\" -silent \"{filePath}\"",
CreateNoWindow = true,
UseShellExecute = false
};


using (var p = Process.Start(psi))
{
if (p != null)
{
p.WaitForExit(30000); // wait up to 30s
}
}
}
else
{
// Try generic PrintTo verb (works for many document types if associated app supports it)
var psi = new ProcessStartInfo
{
FileName = filePath,
Verb = "PrintTo",
Arguments = $"\"{printerName}\"",
CreateNoWindow = true,
UseShellExecute = true
};


Process proc = Process.Start(psi)!;
// don't block too long
proc.WaitForExit(15000);
}
}
}
}