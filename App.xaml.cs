using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

using Microsoft.Win32;

namespace AruaRoseLoginManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// App startup hook.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureBrowserEmulation();
            base.OnStartup(e);
        }

        /// <summary>
        /// Forces WPF WebBrowser to use IE11 edge mode via per-user feature control.
        /// </summary>
        private static void ConfigureBrowserEmulation()
        {
            try
            {
                string exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                const int ie11EdgeMode = 11001;
                string keyPath = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (key == null || string.IsNullOrWhiteSpace(exeName))
                    {
                        return;
                    }

                    object existing = key.GetValue(exeName);
                    if (!(existing is int) || (int)existing < ie11EdgeMode)
                    {
                        key.SetValue(exeName, ie11EdgeMode, RegistryValueKind.DWord);
                    }

                    // Also cover debug host process names when launched from Visual Studio.
                    if (!exeName.EndsWith(".vshost.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string vshostName = Path.GetFileNameWithoutExtension(exeName) + ".vshost.exe";
                        object existingVshost = key.GetValue(vshostName);
                        if (!(existingVshost is int) || (int)existingVshost < ie11EdgeMode)
                        {
                            key.SetValue(vshostName, ie11EdgeMode, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            catch
            {
                // Non-fatal: app still runs; browser may show more script warnings.
            }
        }
    }
}
