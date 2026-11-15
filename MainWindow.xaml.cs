using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinRescuePro.Services;

namespace WinRescuePro
{
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        private readonly RepairEngine _repairEngine;
        private readonly RestorePointService _restorePointService;
        private readonly ReportService _reportService;
        private string _currentLog = "";

        public MainWindow()
        {
            InitializeComponent();
            
            _repairEngine = new RepairEngine();
            _restorePointService = new RestorePointService();
            _reportService = new ReportService();
            
            // ربط الأحداث
            _repairEngine.OnLog += message => 
            {
                Dispatcher.Invoke(() => 
                {
                    _currentLog += message + "\n";
                    LogTextBox.Text = _currentLog;
                    LogScrollViewer.ScrollToBottom();
                });
            };
            
            _repairEngine.OnProgress += progress =>
            {
                Dispatcher.Invoke(() => ProgressBar.Value = progress);
            };
            
            LocalizationService.LanguageChanged += () =>
            {
                // تحديث النصوص تلقائياً
                this.InvalidateVisual();
            };
        }

        private async void BtnNetwork_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRepairAsync("Network Repair", () => _repairEngine.RepairNetworkAsync());
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRepairAsync("Windows Update Repair", () => _repairEngine.RepairWindowsUpdateAsync());
        }

        private async void BtnSound_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRepairAsync("Sound & Printer Repair", () => _repairEngine.RepairSoundAndPrinterAsync());
        }

        private async void BtnStore_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRepairAsync("Microsoft Store Repair", () => _repairEngine.RepairMicrosoftStoreAsync());
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            await ExecuteRepairAsync("Deep System Scan", () => _repairEngine.DeepScanAndRepairAsync());
        }

        private async Task ExecuteRepairAsync(string operationName, Func<Task> repairAction)
        {
            try
            {
                // تعطيل الأزرار
                SetButtonsEnabled(false);
                
                // إنشاء نقطة استعادة
                LogTextBox.Text += $"\n🛡️ {LocalizationService.GetString("RestorePointCreating")}\n";
                var restoreCreated = await _restorePointService.CreateRestorePointAsync($"Before {operationName}");
                
                if (restoreCreated)
                    LogTextBox.Text += $"{LocalizationService.GetString("RestorePointSuccess")}\n\n";
                
                // تنفيذ الإصلاح
                await repairAction();
                
                // رسالة النجاح
                LogTextBox.Text += $"\n✅ {LocalizationService.GetString("SuccessMessage")}\n";
            }
            catch (Exception ex)
            {
                LogTextBox.Text += $"\n❌ خطأ: {ex.Message}\n";
            }
            finally
            {
                SetButtonsEnabled(true);
                ProgressBar.Value = 0;
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            // يمكن إضافة منطق تعطيل الأزرار هنا
        }

        private void BtnToggleLang_Click(object sender, RoutedEventArgs e)
        {
            LocalizationService.ToggleLanguage();
        }

        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePath = await _reportService.GenerateHtmlReportAsync(_currentLog);
                MessageBox.Show($"تم إنشاء التقرير:\n{filePath}", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء التقرير: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
