using System;
using System.Windows;
using WinRescuePro.Services;

namespace WinRescuePro
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // طلب صلاحيات المسؤول تلقائياً
            if (!IsRunningAsAdministrator())
            {
                RestartAsAdministrator();
                return;
            }
        }

        private bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdministrator()
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new System.Diagnostics.ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            
            try
            {
                System.Diagnostics.Process.Start(startInfo);
            }
            catch { /* المستخدم رفض الصلاحيات */ }
            
            Current.Shutdown();
        }
    }
}
