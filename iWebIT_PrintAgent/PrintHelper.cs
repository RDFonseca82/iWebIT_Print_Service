using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Diagnostics;

namespace iWebIT_PrintAgent
{
    public static class PrintHelper
    {
        // Imprime uma imagem diretamente
        public static void PrintImage(string filePath, string printerName)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Ficheiro não encontrado: " + filePath);
        
            using (Image image = Image.FromFile(filePath))
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrinterSettings.PrinterName = printerName;
                printDoc.DocumentName = Path.GetFileName(filePath);
        
                printDoc.PrintPage += (sender, e) =>
                {
                    // Usa PageBounds para ocupar toda a página
                    var pageWidth = e.PageBounds.Width;
                    var pageHeight = e.PageBounds.Height;
        
                    // Mantém proporção da imagem
                    float scale = Math.Min((float)pageWidth / image.Width, (float)pageHeight / image.Height);
                    int drawWidth = (int)(image.Width * scale);
                    int drawHeight = (int)(image.Height * scale);
        
                    int posX = (pageWidth - drawWidth) / 2;
                    int posY = (pageHeight - drawHeight) / 2;
        
                    e.Graphics.DrawImage(image, posX, posY, drawWidth, drawHeight);
                };
        
                printDoc.Print();
            }
        }


        // Imprime PDFs via SumatraPDF (opcional)
        public static void PrintPdf(string filePath, string printerName, string sumatraPath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            if (!File.Exists(sumatraPath)) throw new FileNotFoundException(sumatraPath);

            var psi = new ProcessStartInfo
            {
                FileName = sumatraPath,
                Arguments = $"-print-to \"{printerName}\" -silent \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process?.WaitForExit(30000);
            }
        }

        // Detecta o tipo de ficheiro e imprime automaticamente
        public static void PrintFile(string filePath, string printerName, string sumatraPath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".pdf")
                PrintPdf(filePath, printerName, sumatraPath);
            else
                PrintImage(filePath, printerName);
        }
    }
}
