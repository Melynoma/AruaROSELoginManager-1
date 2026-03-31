using System.Windows;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Prompts for the website password so login form can be auto-filled.
    /// </summary>
    public partial class WebsitePasswordPrompt : Window
    {
        /// <summary>
        /// Entered website password.
        /// </summary>
        public string WebsitePassword { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Account username to display</param>
        public WebsitePasswordPrompt(string username)
        {
            InitializeComponent();
            _usernameTextBox.Text = username ?? string.Empty;
            _passwordBox.Focus();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_passwordBox.Password))
            {
                MessageBox.Show("Please enter the website password.", "AruaROSE Login Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WebsitePassword = _passwordBox.Password;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}