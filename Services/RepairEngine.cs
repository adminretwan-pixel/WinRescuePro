using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace WinRescuePro.Services
{
    public class RepairEngine
    {
        public event Action<string> OnLog;
        public event Action<int> OnProgress;

        public async Task RepairNetworkAsync()
        {
            Log("🌐 بدء إصلاح الشبكة...");
            
            await RunCommandAsync("ipconfig", "/flushdns", 10);
            Log("✅ تم تنظيف DNS");
            OnProgress?.Invoke(20);
            
            await RunCommandAsync("netsh", "winsock reset", 30);
            Log("✅ تم إعادة تعيين Winsock");
            OnProgress?.Invoke(50);
            
            await RunCommandAsync("netsh", "int ip reset", 30);
            Log("✅ تم إعادة تعيين IP Stack");
            OnProgress?.Invoke(80);
            
            await RunCommandAsync("ipconfig", "/release", 10);
            await RunCommandAsync("ipconfig", "/renew", 20);
            Log("✅ تم تجديد عنوان IP");
            OnProgress?.Invoke(100);
            
            Log("🎉 اكتمل إصلاح الشبكة!");
        }

        public async Task RepairWindowsUpdateAsync()
        {
            Log("🔄 بدء إصلاح Windows Update...");
            
            await RunCommandAsync("net", "stop wuauserv", 15);
            await RunCommandAsync("net", "stop cryptSvc", 15);
            await RunCommandAsync("net", "stop bits", 15);
            await RunCommandAsync("net", "stop msiserver", 15);
            Log("✅ تم إيقاف خدمات التحديث");
            OnProgress?.Invoke(30);
            
            await RunCommandAsync("cmd", "/c ren C:\\Windows\\SoftwareDistribution SoftwareDistribution.old", 10);
            await RunCommandAsync("cmd", "/c ren C:\\Windows\\System32\\catroot2 catroot2.old", 10);
            Log("✅ تم إعادة تسمية مجلدات التحديث");
            OnProgress?.Invoke(60);
            
            await RunCommandAsync("net", "start wuauserv", 15);
            await RunCommandAsync("net", "start cryptSvc", 15);
            await RunCommandAsync("net", "start bits", 15);
            await RunCommandAsync("net", "start msiserver", 15);
            Log("✅ تم إعادة تشغيل الخدمات");
            OnProgress?.Invoke(100);
            
            Log("🎉 اكتمل إصلاح Windows Update!");
        }

        public async Task RepairSoundAndPrinterAsync()
        {
            Log("🔊 بدء إصلاح الصوت والطابعة...");
            
            await RunCommandAsync("net", "stop audiosrv", 10);
            await RunCommandAsync("net", "start audiosrv", 10);
            Log("✅ تم إعادة تشغيل خدمة الصوت");
            OnProgress?.Invoke(40);
            
            await RunCommandAsync("net", "stop spooler", 10);
            await RunCommandAsync("cmd", "/c del /Q /F /S \"%systemroot%\\System32\\spool\\PRINTERS\\*.*\"", 15);
            await RunCommandAsync("net", "start spooler", 10);
            Log("✅ تم تنظيف مهام الطباعة العالقة");
            OnProgress?.Invoke(100);
            
            Log("🎉 اكتمل إصلاح الصوت والطابعة!");
        }

        public async Task RepairMicrosoftStoreAsync()
        {
            Log("🛍️ بدء إصلاح Microsoft Store...");
            
            await RunCommandAsync("wsreset.exe", "", 30);
            Log("✅ تم تنظيف كاش المتجر");
            OnProgress?.Invoke(50);
            
            var psCommand = "Get-AppxPackage -AllUsers Microsoft.WindowsStore | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppXManifest.xml\"}";
            await RunPowerShellAsync(psCommand, 60);
            Log("✅ تم إعادة تسجيل المتجر");
            OnProgress?.Invoke(100);
            
            Log("🎉 اكتمل إصلاح المتجر!");
        }

        public async Task DeepScanAndRepairAsync()
        {
            Log("🩺 بدء الفحص الشامل...");
            
            await RunCommandAsync("DISM", "/Online /Cleanup-Image /RestoreHealth", 300);
            Log("✅ اكتمل فحص DISM");
            OnProgress?.Invoke(50);
            
            await RunCommandAsync("sfc", "/scannow", 300);
            Log("✅ اكتمل فحص SFC");
            OnProgress?.Invoke(100);
            
            Log("🎉 اكتمل الفحص الشامل!");
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private async Task RunCommandAsync(string exe, string args, int timeoutSeconds)
        {
            await Task.Run(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit(timeoutSeconds * 1000);
                }
            });
        }

        private async Task RunPowerShellAsync(string command, int timeoutSeconds)
        {
            await RunCommandAsync("powershell", $"-Command \"{command}\"", timeoutSeconds);
        }
    }
}
