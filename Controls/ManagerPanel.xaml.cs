//
// FILE     : ManagerPanel.xaml.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-18
//

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

using AruaRoseLoginManager.Data;
using AruaRoseLoginManager.Enum;
using AruaRoseLoginManager.Helpers;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for ManagerPanel.xaml
    /// </summary>
    public partial class ManagerPanel : UserControl, IManagerPanel
    {
        /// <summary>
        /// Current window size
        /// </summary>
        private WindowSize _windowSize;

        /// <summary>
        /// Store original folder paths for stream mode anonymization
        /// </summary>
        private string _originalFolderPath = string.Empty;
        private string _originalFolderPath2 = string.Empty;
        private string _originalFolderPath3 = string.Empty;

        /// <summary>
        /// Event to raise when save settings is requested
        /// </summary>
        [Browsable(true)]
        public event EventHandler SaveSettingsRequested;

        /// <summary>
        /// Path to the directory with the ROSE client
        /// </summary>
        public string RoseFolderPath
        {
            get { return _originalFolderPath; }
            set 
            { 
                _originalFolderPath = value;
                UpdateFolderDisplay(1);
            }
        }

        /// <summary>
        /// Path to the second install location
        /// </summary>
        public string RoseFolderPath2
        {
            get { return _originalFolderPath2; }
            set 
            { 
                _originalFolderPath2 = value;
                UpdateFolderDisplay(2);
            }
        }

        /// <summary>
        /// Path to the third install location
        /// </summary>
        public string RoseFolderPath3
        {
            get { return _originalFolderPath3; }
            set 
            { 
                _originalFolderPath3 = value;
                UpdateFolderDisplay(3);
            }
        }

        /// <summary>
        /// Default game window width
        /// </summary>
        public int DefaultGameWidth
        {
            get 
            { 
                return int.TryParse(_defaultWidthTextBox?.Text, out int width) ? width : 1024;
            }
            set 
            { 
                if (_defaultWidthTextBox != null) _defaultWidthTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Default game window height
        /// </summary>
        public int DefaultGameHeight
        {
            get 
            { 
                return int.TryParse(_defaultHeightTextBox?.Text, out int height) ? height : 768;
            }
            set 
            { 
                if (_defaultHeightTextBox != null) _defaultHeightTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Whether game should default to fullscreen
        /// </summary>
        public bool IsFullscreen
        {
            get { return _fullscreenCheckbox.IsChecked == null ? false : _fullscreenCheckbox.IsChecked.Value; }
            set { _fullscreenCheckbox.IsChecked = value; }
        }

        /// <summary>
        /// Default game window width for install location 2
        /// </summary>
        public int DefaultGameWidth2
        {
            get 
            { 
                return int.TryParse(_defaultWidthTextBox2?.Text, out int width) ? width : 1024;
            }
            set 
            { 
                if (_defaultWidthTextBox2 != null) _defaultWidthTextBox2.Text = value.ToString();
            }
        }

        /// <summary>
        /// Default game window height for install location 2
        /// </summary>
        public int DefaultGameHeight2
        {
            get 
            { 
                return int.TryParse(_defaultHeightTextBox2?.Text, out int height) ? height : 768;
            }
            set 
            { 
                if (_defaultHeightTextBox2 != null) _defaultHeightTextBox2.Text = value.ToString();
            }
        }

        /// <summary>
        /// Whether game should default to fullscreen for install location 2
        /// </summary>
        public bool IsFullscreen2
        {
            get { return _fullscreenCheckbox2.IsChecked == null ? false : _fullscreenCheckbox2.IsChecked.Value; }
            set { _fullscreenCheckbox2.IsChecked = value; }
        }

        /// <summary>
        /// Default game window width for install location 3
        /// </summary>
        public int DefaultGameWidth3
        {
            get 
            { 
                return int.TryParse(_defaultWidthTextBox3?.Text, out int width) ? width : 1024;
            }
            set 
            { 
                if (_defaultWidthTextBox3 != null) _defaultWidthTextBox3.Text = value.ToString();
            }
        }

        /// <summary>
        /// Default game window height for install location 3
        /// </summary>
        public int DefaultGameHeight3
        {
            get 
            { 
                return int.TryParse(_defaultHeightTextBox3?.Text, out int height) ? height : 768;
            }
            set 
            { 
                if (_defaultHeightTextBox3 != null) _defaultHeightTextBox3.Text = value.ToString();
            }
        }

        /// <summary>
        /// Whether game should default to fullscreen for install location 3
        /// </summary>
        public bool IsFullscreen3
        {
            get { return _fullscreenCheckbox3.IsChecked == null ? false : _fullscreenCheckbox3.IsChecked.Value; }
            set { _fullscreenCheckbox3.IsChecked = value; }
        }

        /// <summary>
        /// Whether stream mode is enabled (hides account and location info)
        /// </summary>
        public bool StreamMode
        {
            get { return _streamModeCheckbox.IsChecked == null ? false : _streamModeCheckbox.IsChecked.Value; }
            set 
            { 
                _streamModeCheckbox.IsChecked = value;
                UpdateStreamMode();
            }
        }

        /// <summary>
        /// Update folder display based on stream mode
        /// </summary>
        private void UpdateFolderDisplay(int location)
        {
            if (StreamMode)
            {
                switch (location)
                {
                    case 1:
                        _folderTextBox.Text = "***********";
                        _patchFolderText1.Visibility = Visibility.Collapsed;
                        break;
                    case 2:
                        _folderTextBox2.Text = "***********";
                        _patchFolderText2.Visibility = Visibility.Collapsed;
                        break;
                    case 3:
                        _folderTextBox3.Text = "***********";
                        _patchFolderText3.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            else
            {
                switch (location)
                {
                    case 1:
                        _folderTextBox.Text = _originalFolderPath;
                        _patchFolderText1.Text = string.IsNullOrEmpty(_originalFolderPath) ? "(not configured)" : _originalFolderPath;
                        _patchFolderText1.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        _folderTextBox2.Text = _originalFolderPath2;
                        _patchFolderText2.Text = string.IsNullOrEmpty(_originalFolderPath2) ? "(not configured)" : _originalFolderPath2;
                        _patchFolderText2.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        _folderTextBox3.Text = _originalFolderPath3;
                        _patchFolderText3.Text = string.IsNullOrEmpty(_originalFolderPath3) ? "(not configured)" : _originalFolderPath3;
                        _patchFolderText3.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        /// <summary>
        /// Update all folder displays when stream mode is toggled
        /// </summary>
        private void UpdateAllFolderDisplays()
        {
            UpdateFolderDisplay(1);
            UpdateFolderDisplay(2);
            UpdateFolderDisplay(3);
        }

        /// <summary>
        /// Whether to run the clients as admin
        /// </summary>
        public bool RunAsAdmin
        {
            get { return _runAsAdminCheckbox.IsChecked == null ? false : _runAsAdminCheckbox.IsChecked.Value; }
            set { _runAsAdminCheckbox.IsChecked = value; }
        }

        /// <summary>
        /// Window size
        /// </summary>
        public WindowSize Size
        {
            get
            {
                return _windowSize;
            }
            set
            {
                ChangeSize(value);
                _windowSize = value;
                InitializeResolutionFields();
            }
        }

        /// <summary>
        /// The account display panel
        /// </summary>
        public IAccountDisplay AccountDisplay { get { return _accountDisplay; } }

        /// <summary>
        /// The party display panel
        /// </summary>
        public IPartyDisplay PartyDisplay { get { return _partyDisplay; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ManagerPanel()
        {
            InitializeComponent();
            DataContext = this;

            ChangeDisplay(DisplayPanel.Accounts);
        }

        /// <summary>
        /// Displays a message box to the user
        /// </summary>
        /// <param name="message">Message to display</param>
        public void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "AruaROSE Login Manager", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        }

        /// <summary>
        /// Launches Update.exe in the selected install location.
        /// </summary>
        public void PatchInstall(int installLocation)
        {
            string rosePath = string.Empty;
            switch (installLocation)
            {
                case 2:
                    rosePath = RoseFolderPath2;
                    break;
                case 3:
                    rosePath = RoseFolderPath3;
                    break;
                default:
                    rosePath = RoseFolderPath;
                    break;
            }

            if (string.IsNullOrWhiteSpace(rosePath) || !Directory.Exists(rosePath))
            {
                ShowMessageBox("Selected ROSE location is not configured or does not exist.");
                return;
            }

            string updaterPath = Path.Combine(rosePath, "Launcher.exe");
            if (!File.Exists(updaterPath))
            {
                ShowMessageBox("Launcher.exe was not found in the selected ROSE location.");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = updaterPath,
                WorkingDirectory = rosePath,
                UseShellExecute = true
            };

            if (RunAsAdmin)
            {
                startInfo.Verb = "runas";
            }

            Process.Start(startInfo);
        }

        /// <summary>
        /// Updates stream mode for all list views
        /// </summary>
        public void UpdateStreamMode()
        {
            _accountDisplay.SetStreamMode(StreamMode);
            _partyDisplay.SetStreamMode(StreamMode);
            UpdateAllFolderDisplays();
        }

        /// <summary>
        /// Changes the size of the window
        /// </summary>
        /// <param name="size">New size</param>
        public void ChangeSize(WindowSize size)
        {
            Application.Current.MainWindow.Height = size.Height;
            Application.Current.MainWindow.Width = size.Width;
        }

        /// <summary>
        /// Change which tab display is active
        /// </summary>
        /// <param name="panel">The new panel to display</param>
        private void ChangeDisplay(DisplayPanel panel)
        {
            _accountsButton.Active = false;
            _partiesButton.Active = false;
            _optionssButton.Active = false;
            _patchButton.Active = false;
            _infoButton.Active = false;
            _accountDisplay.Visibility = Visibility.Hidden;
            _accountDisplay.SwitchPanels(PanelMode.Select);
            _partyDisplay.Visibility = Visibility.Hidden;
            _partyDisplay.SwitchPanels(PanelMode.Select);
            _optionsDisplay.Visibility = Visibility.Hidden;
            _patchDisplay.Visibility = Visibility.Hidden;
            _infoDisplay.Visibility = Visibility.Hidden;

            switch (panel)
            {
                case DisplayPanel.Accounts:
                    _accountsButton.Active = true;
                    _accountDisplay.Visibility = Visibility.Visible;
                    break;
                case DisplayPanel.Parties:
                    _partiesButton.Active = true;
                    _partyDisplay.Visibility = Visibility.Visible;
                    break;
                case DisplayPanel.Options:
                    _optionssButton.Active = true;
                    _optionsDisplay.Visibility = Visibility.Visible;
                    break;
                case DisplayPanel.Patch:
                    _patchButton.Active = true;
                    _patchDisplay.Visibility = Visibility.Visible;
                    break;
                case DisplayPanel.Info:
                    _infoButton.Active = true;
                    _infoDisplay.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Event handler for clicking the browse for folder button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void BrowseFolder1Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                RoseFolderPath = dialog.FileName;
            }
        }

        private void BrowseFolder2Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                RoseFolderPath2 = dialog.FileName;
            }
        }

        private void BrowseFolder3Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                RoseFolderPath3 = dialog.FileName;
            }
        }

        /// <summary>
        /// Event handler for resizing the window to the 'normal' size
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            Size = WindowSize.Default;
        }

        /// <summary>
        /// Event handler for resizing the window to the 'compact' size
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void CompactButton_Click(object sender, RoutedEventArgs e)
        {
            Size = WindowSize.Compact;
        }

        /// <summary>
        /// Event handler for switching to the accounts tab
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDisplay(DisplayPanel.Accounts);
        }

        /// <summary>
        /// Event handler for switching to the parties tab
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartiesButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDisplay(DisplayPanel.Parties);
        }

        /// <summary>
        /// Event handler for switching to the options tab
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDisplay(DisplayPanel.Options);
        }

        /// <summary>
        /// Event handler for switching to the patch tab.
        /// </summary>
        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDisplay(DisplayPanel.Patch);
        }

        /// <summary>
        /// Event handler for switching to the info tab
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeDisplay(DisplayPanel.Info);
        }

        /// <summary>
        /// Event handler for patching install location 1.
        /// </summary>
        private void PatchNow1Button_Click(object sender, RoutedEventArgs e)
        {
            PatchInstall(1);
        }

        /// <summary>
        /// Event handler for patching install location 2.
        /// </summary>
        private void PatchNow2Button_Click(object sender, RoutedEventArgs e)
        {
            PatchInstall(2);
        }

        /// <summary>
        /// Event handler for patching install location 3.
        /// </summary>
        private void PatchNow3Button_Click(object sender, RoutedEventArgs e)
        {
            PatchInstall(3);
        }

        /// <summary>
        /// Event handler for saving the settings/options
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSettingsRequested != null)
            {
                SaveSettingsRequested(this, EventArgs.Empty);
            }
        }

        private void StreamModeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            StreamMode = true;
            UpdateStreamMode();
        }

        private void StreamModeCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            StreamMode = false;
            UpdateStreamMode();
        }

        private void InitializeResolutionFields()
        {
            // Initialize all three resolution field states based on fullscreen checkbox states
            UpdateResolutionFields(1, IsFullscreen);
            UpdateResolutionFields(2, IsFullscreen2);
            UpdateResolutionFields(3, IsFullscreen3);
        }

        private void UpdateResolutionFields(int location, bool isFullscreen)
        {
            switch (location)
            {
                case 1:
                    _defaultWidthTextBox.IsEnabled = !isFullscreen;
                    _defaultHeightTextBox.IsEnabled = !isFullscreen;
                    break;
                case 2:
                    _defaultWidthTextBox2.IsEnabled = !isFullscreen;
                    _defaultHeightTextBox2.IsEnabled = !isFullscreen;
                    break;
                case 3:
                    _defaultWidthTextBox3.IsEnabled = !isFullscreen;
                    _defaultHeightTextBox3.IsEnabled = !isFullscreen;
                    break;
            }
        }

        private void FullscreenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(1, true);
        }

        private void FullscreenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(1, false);
        }

        private void FullscreenCheckbox2_Checked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(2, true);
        }

        private void FullscreenCheckbox2_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(2, false);
        }

        private void FullscreenCheckbox3_Checked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(3, true);
        }

        private void FullscreenCheckbox3_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateResolutionFields(3, false);
        }
    }
}
