using System;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Hosts website login page and auto-fills username/password fields.
    /// </summary>
    public partial class WebsiteLoginWindow : Window
    {
        private readonly string _username;
        private readonly string _password;
        private bool _isFilled;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Website username</param>
        /// <param name="password">Website password</param>
        public WebsiteLoginWindow(string username, string password)
        {
            InitializeComponent();
            _username = username ?? string.Empty;
            _password = password ?? string.Empty;

            SuppressScriptErrors();
            _browser.Navigate("https://www.aruarose.com/login");
        }

        private void Browser_Navigated(object sender, NavigationEventArgs e)
        {
            SuppressScriptErrors();
        }

        private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (_isFilled)
            {
                return;
            }

            try
            {
                dynamic document = _browser.Document;
                if (document == null)
                {
                    return;
                }

                string jsUsername = EscapeForJs(_username);
                string jsPassword = EscapeForJs(_password);
                string script =
                    "(function(){" +
                    "var u=document.getElementsByName('username');" +
                    "var p=document.getElementsByName('password');" +
                    "if(u&&u.length>0){u[0].value='" + jsUsername + "';}" +
                    "if(p&&p.length>0){p[0].value='" + jsPassword + "';}" +
                    "})();";

                document.parentWindow.execScript(script, "JavaScript");
                _isFilled = true;
            }
            catch
            {
                // Ignore DOM/script errors from third-party page changes.
            }
        }

        private static string EscapeForJs(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private void SuppressScriptErrors()
        {
            try
            {
                FieldInfo activeXField = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                object activeX = activeXField == null ? null : activeXField.GetValue(_browser);
                if (activeX != null)
                {
                    activeX.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, activeX, new object[] { true });
                }
            }
            catch
            {
                // If this fails, the browser still works; only script error dialogs may appear.
            }
        }
    }
}