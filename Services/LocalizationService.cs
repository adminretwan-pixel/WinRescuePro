using System;
using System.Windows;
using System.Windows.Resources;

namespace WinRescuePro.Services
{
    public static class LocalizationService
    {
        private static bool _isArabic = true;

        public static bool IsArabic => _isArabic;

        public static event Action LanguageChanged;

        public static void ToggleLanguage()
        {
            _isArabic = !_isArabic;
            LoadLanguage(_isArabic ? "ar" : "en");
            LanguageChanged?.Invoke();
        }

        public static void LoadLanguage(string langCode)
        {
            var dict = new ResourceDictionary();
            var uri = new Uri($"Resources/Strings.{langCode}.xaml", UriKind.Relative);
            
            dict.Source = uri;
            
            // استبدال القاموس القديم
            var appResources = Application.Current.Resources.MergedDictionaries;
            
            // إزالة قاموس اللغة القديم
            for (int i = appResources.Count - 1; i >= 0; i--)
            {
                if (appResources[i].Source != null && 
                    appResources[i].Source.OriginalString.Contains("Strings."))
                {
                    appResources.RemoveAt(i);
                }
            }
            
            appResources.Add(dict);
        }

        public static string GetString(string key)
        {
            if (Application.Current.Resources.Contains(key))
                return Application.Current.Resources[key] as string;
            return key;
        }
    }
}
