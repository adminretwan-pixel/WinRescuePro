using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WinRescuePro.Services
{
    public class ReportService
    {
        public async Task<string> GenerateHtmlReportAsync(string logContent)
        {
            return await Task.Run(() =>
            {
                var cpuInfo = GetCpuInfo();
                var osInfo = GetOsInfo();
                var ramInfo = GetRamInfo();
                
                var html = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <title>WinRescue Pro - تقرير الفحص</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Arial; background: #0f172a; color: #e2e8f0; padding: 40px; }}
        .container {{ max-width: 900px; margin: 0 auto; background: #1e293b; border-radius: 12px; padding: 30px; }}
        h1 {{ color: #3b82f6; border-bottom: 3px solid #3b82f6; padding-bottom: 15px; }}
        .info-box {{ background: #0f172a; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .log {{ background: #000; color: #4ade80; padding: 20px; border-radius: 8px; 
               font-family: Consolas, monospace; direction: ltr; text-align: left; 
               max-height: 400px; overflow-y: auto; white-space: pre-wrap; }}
        .footer {{ text-align: center; margin-top: 30px; color: #64748b; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>🛡️ WinRescue Pro - تقرير الفحص</h1>
        
        <div class='info-box'>
            <h3>📋 معلومات النظام</h3>
            <p><strong>المعالج:</strong> {cpuInfo}</p>
            <p><strong>الذاكرة:</strong> {ramInfo}</p>
            <p><strong>نظام التشغيل:</strong> {osInfo}</p>
            <p><strong>تاريخ التقرير:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        </div>
        
        <h3>📜 سجل العمليات</h3>
        <div class='log'>{logContent}</div>
        
        <div class='footer'>
            <p>تم إنشاء هذا التقرير بواسطة WinRescue Pro v1.0</p>
            <p>{LocalizationService.GetString("Slogan")}</p>
        </div>
    </div>
</body>
</html>";

                var fileName = $"WinRescueReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                
                File.WriteAllText(filePath, html, Encoding.UTF8);
                
                // فتح التقرير تلقائياً
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                
                return filePath;
            });
        }

        private string GetCpuInfo()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                        return item["Name"]?.ToString() ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetOsInfo()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get())
                        return $"{item["Caption"]} (Build {item["Version"]})";
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetRamInfo()
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var ramKB = Convert.ToInt64(item["TotalVisibleMemorySize"]);
                        return $"{ramKB / 1024 / 1024} GB";
                    }
                }
            }
            catch { }
            return "Unknown";
        }
    }
}
