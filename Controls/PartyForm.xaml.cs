// FILE     : PartyForm.xaml.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz (modified)
// DATE     : 2021-02-18

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

using AruaRoseLoginManager.Data;
using AruaRoseLoginManager.Helpers;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for PartyForm.xaml
    /// </summary>
    public partial class PartyForm : UserControl
    {
        // P/Invoke declarations for global mouse hook
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private IntPtr _hookHandle = IntPtr.Zero;
        private LowLevelMouseProc _mouseDelegate = null;
        private TextBox _captureXTextBox = null;
        private TextBox _captureYTextBox = null;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private List<string> _selectedMembers;
        private ObservableCollection<string> _availableAccounts;
        private Dictionary<string, Account> _availableAccountObjects;
        private WindowSize _defaultWindowSize;
        private string _originalPartyName;
        private IManagerPanel _managerPanel;

        [Browsable(true)]
        public event EventHandler Cancel;

        [Browsable(true)]
        public event EventHandler<DataEventArgs<Party>> SaveParty;
        [Browsable(true)]
        public event EventHandler<ListEventArgs> DeleteParty;

        public PartyForm()
        {
            InitializeComponent();
            _selectedMembers = new List<string>();
            _availableAccountObjects = new Dictionary<string, Account>();
            _defaultWindowSize = WindowSize.Default;
        }

        public void PopulateFields(Party party)
        {
            _originalPartyName = party.Name;
            _partyNameTextBox.Text = party.Name;
            _partyNameTextBox.IsEnabled = false;
            _descriptionTextBox.Text = party.Description;
            foreach (string member in party.Accounts)
            {
                string normalizedMember = NormalizeMemberFormat(member);
                _selectedMembers.Add(normalizedMember);
                string charName = normalizedMember.Split('|')[0];
                List<string> accountCharacters = FindCharactersForMember(charName);
                AddMemberToList(normalizedMember, accountCharacters.Count > 0 ? accountCharacters : null);
            }
        }

        private List<string> FindCharactersForMember(string characterName)
        {
            foreach (Account account in _availableAccountObjects.Values)
            {
                if (account?.Characters == null) continue;
                foreach (var character in account.Characters)
                {
                    if (character?.Name == characterName)
                    {
                        return account.Characters
                            .Where(c => c != null && !string.IsNullOrWhiteSpace(c.Name))
                            .Select(c => c.Name)
                            .ToList();
                    }
                }
            }
            return new List<string>();
        }

        private string NormalizeMemberFormat(string member)
        {
            if (string.IsNullOrWhiteSpace(member)) return member;
            string[] parts = member.Split('|');
            string characterName = parts[0];
            int location = 1;
            int width = 1024;
            int height = 768;
            bool fullscreen = false;
            int x = 0;
            int y = 0;
            int monitor = 0;
            if (parts.Length >= 2 && int.TryParse(parts[1], out int loc)) location = loc;
            if (parts.Length >= 4) { int.TryParse(parts[2], out width); int.TryParse(parts[3], out height); }
            if (parts.Length >= 8)
            {
                // New 8-part format: name|location|width|height|fullscreen|x|y|monitor
                bool.TryParse(parts[4], out fullscreen);
                int.TryParse(parts[5], out x);
                int.TryParse(parts[6], out y);
                int.TryParse(parts[7], out monitor);
            }
            else if (parts.Length >= 7)
            {
                // Old 7-part format: name|location|width|height|x|y|monitor
                int.TryParse(parts[4], out x);
                int.TryParse(parts[5], out y);
                int.TryParse(parts[6], out monitor);
            }
            return $"{characterName}|{location}|{width}|{height}|{fullscreen}|{x}|{y}|{monitor}";
        }

        public void PopulateAccounts(IEnumerable<string> availableAccounts)
        {
            _availableAccounts = new ObservableCollection<string>(availableAccounts.Where(x => !_selectedMembers.Any(member => member.StartsWith(x + ":"))));
            _accountComboBox.ItemsSource = _availableAccounts;
        }

        public void PopulateAccountsWithCharacters(Dictionary<string, Account> availableAccounts)
        {
            _availableAccountObjects = availableAccounts ?? new Dictionary<string, Account>();
            List<string> accountNames = new List<string>();
            foreach (var account in _availableAccountObjects.Values)
            {
                if (account != null && account.Characters != null && account.Characters.Count > 0)
                    accountNames.Add(account.Username);
            }
            _availableAccounts = new ObservableCollection<string>(accountNames);
            _accountComboBox.ItemsSource = _availableAccounts;
        }

        public void ClearFields()
        {
            _partyNameTextBox.IsEnabled = true;
            _partyNameTextBox.Clear();
            _descriptionTextBox.Clear();
            _selectedMembers.Clear();
            _partyListStackPanel.Children.Clear();
            _noneLabel.Visibility = Visibility.Visible;
        }

        public void SetDefaultWindowSize(WindowSize sizeDefaults)
        {
            _defaultWindowSize = sizeDefaults ?? WindowSize.Default;
        }

        public void SetManagerPanel(IManagerPanel managerPanel)
        {
            _managerPanel = managerPanel;
        }

        public void SetEditMode(bool isEdit)
        {
            _deletePartyButton.Visibility = isEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        public void FocusPrimary() => _partyNameTextBox.Focus();

        private int GetDefaultGameWidth(int location)
        {
            if (_managerPanel != null)
            {
                if (location == 1)
                    return _managerPanel.DefaultGameWidth;
                else if (location == 2)
                    return _managerPanel.DefaultGameWidth2;
                else if (location == 3)
                    return _managerPanel.DefaultGameWidth3;
            }
            else if (_defaultWindowSize != null)
            {
                if (location == 1)
                    return _defaultWindowSize.GameWidth1;
                else if (location == 2)
                    return _defaultWindowSize.GameWidth2;
                else if (location == 3)
                    return _defaultWindowSize.GameWidth3;
            }
            return 1024;
        }

        private int GetDefaultGameHeight(int location)
        {
            if (_managerPanel != null)
            {
                if (location == 1)
                    return _managerPanel.DefaultGameHeight;
                else if (location == 2)
                    return _managerPanel.DefaultGameHeight2;
                else if (location == 3)
                    return _managerPanel.DefaultGameHeight3;
            }
            else if (_defaultWindowSize != null)
            {
                if (location == 1)
                    return _defaultWindowSize.GameHeight1;
                else if (location == 2)
                    return _defaultWindowSize.GameHeight2;
                else if (location == 3)
                    return _defaultWindowSize.GameHeight3;
            }
            return 768;
        }

        private void CaptureScreenCoordinates(TextBox xTextBox, TextBox yTextBox)
        {
            _captureXTextBox = xTextBox;
            _captureYTextBox = yTextBox;

            _mouseDelegate = MouseHookCallback;
            IntPtr moduleHandle = GetModuleHandle(null);
            _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _mouseDelegate, moduleHandle, 0);

            MessageBox.Show("Click on the screen location to capture coordinates.\nPress Escape to cancel.", "Capture Coordinates");

            UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                _captureXTextBox.Text = hookStruct.pt.x.ToString();
                _captureYTextBox.Text = hookStruct.pt.y.ToString();

                // Unhook after capturing one click
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void SavePartyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPartyNameValid()) return;
            if (SaveParty != null)
            {
                DataEventArgs<Party> args = new DataEventArgs<Party>()
                {
                    Data = new Party(_partyNameTextBox.Text, new List<string>(_selectedMembers), _descriptionTextBox.Text)
                };
                ClearFields();
                SaveParty(this, args);
            }
        }

        private void _partyNameError_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) => IsPartyNameValid();

        private bool IsPartyNameValid()
        {
            if (string.IsNullOrWhiteSpace(_partyNameTextBox.Text))
            {
                _partyNameError.Visibility = Visibility.Visible;
                return false;
            }
            else if (_selectedMembers.Count < 1)
            {
                _partyNameError.Visibility = Visibility.Hidden;
                _partyMembersError.Visibility = Visibility.Visible;
                return false;
            }
            _partyMembersError.Visibility = Visibility.Hidden;
            _partyNameError.Visibility = Visibility.Hidden;
            return true;
        }

        // Add member: supports passing accountCharacters to populate a ComboBox so user can choose
        private void AddMemberToList(string memberInfo, List<string> accountCharacters = null)
        {
            string characterName = memberInfo;
            int installLocation = 1;
            int gameWidth = 1024;
            int gameHeight = 768;
            bool isFullscreen = false;
            int gameX = 0;
            int gameY = 0;
            int monitor = 0;

            if (memberInfo.Contains("|"))
            {
                string[] parts = memberInfo.Split('|');
                characterName = parts[0];
                if (parts.Length >= 2 && int.TryParse(parts[1], out int location)) installLocation = location;
                if (parts.Length >= 4) { int.TryParse(parts[2], out gameWidth); int.TryParse(parts[3], out gameHeight); }
                if (parts.Length >= 8)
                {
                    bool.TryParse(parts[4], out isFullscreen);
                    int.TryParse(parts[5], out gameX);
                    int.TryParse(parts[6], out gameY);
                    int.TryParse(parts[7], out monitor);
                }
                else if (parts.Length >= 7)
                {
                    int.TryParse(parts[4], out gameX);
                    int.TryParse(parts[5], out gameY);
                    int.TryParse(parts[6], out monitor);
                }
            }

            Grid memberGrid = new Grid();
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(250) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(90) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(55) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(55) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(45) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30) });

            ComboBox charCombo = new ComboBox() { Margin = new Thickness(2), VerticalAlignment = VerticalAlignment.Center };
            if (accountCharacters != null && accountCharacters.Count > 0)
            {
                foreach (var n in accountCharacters) charCombo.Items.Add(n);
            }
            else
            {
                charCombo.Items.Add(characterName);
            }
            charCombo.SelectedItem = characterName;
            Grid.SetColumn(charCombo, 0);
            memberGrid.Children.Add(charCombo);

            ComboBox locationComboBox = new ComboBox() { SelectedIndex = installLocation - 1, Margin = new Thickness(2) };
            locationComboBox.Items.Add("Location 1");
            locationComboBox.Items.Add("Location 2");
            locationComboBox.Items.Add("Location 3");
            Grid.SetColumn(locationComboBox, 1);
            memberGrid.Children.Add(locationComboBox);

            TextBox widthTextBox = new TextBox() { Text = gameWidth.ToString(), Margin = new Thickness(2), IsEnabled = !isFullscreen };
            Grid.SetColumn(widthTextBox, 2);
            memberGrid.Children.Add(widthTextBox);

            TextBox heightTextBox = new TextBox() { Text = gameHeight.ToString(), Margin = new Thickness(2), IsEnabled = !isFullscreen };
            Grid.SetColumn(heightTextBox, 4);
            memberGrid.Children.Add(heightTextBox);

            CheckBox fullscreenCheckBox = new CheckBox() { Content = "Fullscreen", IsChecked = isFullscreen, Margin = new Thickness(4, 0, 2, 0), VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(fullscreenCheckBox, 5);
            memberGrid.Children.Add(fullscreenCheckBox);

            // Column 6: "x" flag label + X coordinate textbox
            Grid xGrid = new Grid();
            xGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            xGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Label xLabel = new Label() { Content = "x", Padding = new Thickness(0, 2, 1, 2), VerticalContentAlignment = VerticalAlignment.Center, FontSize = 9 };
            Grid.SetColumn(xLabel, 0);
            xGrid.Children.Add(xLabel);
            TextBox xTextBox = new TextBox() { Text = gameX.ToString(), Margin = new Thickness(0, 2, 2, 2), IsEnabled = !isFullscreen };
            Grid.SetColumn(xTextBox, 1);
            xGrid.Children.Add(xTextBox);
            Grid.SetColumn(xGrid, 6);
            memberGrid.Children.Add(xGrid);

            // Column 7: "y" flag label + Y coordinate textbox
            Grid yGrid = new Grid();
            yGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(12) });
            yGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            Label yLabel = new Label() { Content = "y", Padding = new Thickness(0, 2, 1, 2), VerticalContentAlignment = VerticalAlignment.Center, FontSize = 9 };
            Grid.SetColumn(yLabel, 0);
            yGrid.Children.Add(yLabel);
            TextBox yTextBox = new TextBox() { Text = gameY.ToString(), Margin = new Thickness(0, 2, 2, 2), IsEnabled = !isFullscreen };
            Grid.SetColumn(yTextBox, 1);
            yGrid.Children.Add(yTextBox);
            Grid.SetColumn(yGrid, 7);
            memberGrid.Children.Add(yGrid);

            Button setButton = new Button() { Content = "Set", Margin = new Thickness(2), IsEnabled = !isFullscreen };
            Grid.SetColumn(setButton, 8);
            memberGrid.Children.Add(setButton);

            setButton.Click += (sender, e) => CaptureScreenCoordinates(xTextBox, yTextBox);

            ComboBox monitorComboBox = new ComboBox() { SelectedIndex = monitor, Margin = new Thickness(2) };
            monitorComboBox.Items.Add("0");
            monitorComboBox.Items.Add("1");
            Grid.SetColumn(monitorComboBox, 9);
            memberGrid.Children.Add(monitorComboBox);

            Button deleteButton = new Button() { Content = "X", Width = 25, Height = 25, Margin = new Thickness(2) };
            Grid.SetColumn(deleteButton, 10);
            memberGrid.Children.Add(deleteButton);

            int memberIndex = _selectedMembers.Count - 1;

            void RebuildMember()
            {
                if (memberIndex < 0 || memberIndex >= _selectedMembers.Count) return;
                string name = _selectedMembers[memberIndex].Split('|')[0];
                int loc = locationComboBox.SelectedIndex + 1;
                int w = int.TryParse(widthTextBox.Text, out int ww) ? ww : 1024;
                int h = int.TryParse(heightTextBox.Text, out int hh) ? hh : 768;
                bool fs = fullscreenCheckBox.IsChecked == true;
                int cx = int.TryParse(xTextBox.Text, out int xx) ? xx : 0;
                int cy = int.TryParse(yTextBox.Text, out int yy) ? yy : 0;
                int mon = monitorComboBox.SelectedIndex >= 0 ? monitorComboBox.SelectedIndex : 0;
                _selectedMembers[memberIndex] = $"{name}|{loc}|{w}|{h}|{fs}|{cx}|{cy}|{mon}";
            }

            charCombo.SelectionChanged += (s, e) =>
            {
                if (charCombo.SelectedItem != null && memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    string newName = charCombo.SelectedItem.ToString();
                    string existing = _selectedMembers[memberIndex];
                    string[] parts = existing.Split('|');
                    parts[0] = newName;
                    _selectedMembers[memberIndex] = string.Join("|", parts);
                }
            };

            locationComboBox.SelectionChanged += (sender, e) =>
            {
                if (locationComboBox.SelectedIndex >= 0 && memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    int newLocation = locationComboBox.SelectedIndex + 1;
                    int defaultWidth = GetDefaultGameWidth(newLocation);
                    int defaultHeight = GetDefaultGameHeight(newLocation);
                    widthTextBox.Text = defaultWidth.ToString();
                    heightTextBox.Text = defaultHeight.ToString();
                    RebuildMember();
                }
            };

            fullscreenCheckBox.Checked += (sender, e) =>
            {
                widthTextBox.IsEnabled = false;
                heightTextBox.IsEnabled = false;
                xTextBox.IsEnabled = false;
                yTextBox.IsEnabled = false;
                setButton.IsEnabled = false;
                RebuildMember();
            };

            fullscreenCheckBox.Unchecked += (sender, e) =>
            {
                widthTextBox.IsEnabled = true;
                heightTextBox.IsEnabled = true;
                xTextBox.IsEnabled = true;
                yTextBox.IsEnabled = true;
                setButton.IsEnabled = true;
                RebuildMember();
            };

            widthTextBox.TextChanged += (sender, e) => RebuildMember();
            heightTextBox.TextChanged += (sender, e) => RebuildMember();
            xTextBox.TextChanged += (sender, e) => RebuildMember();
            yTextBox.TextChanged += (sender, e) => RebuildMember();
            monitorComboBox.SelectionChanged += (sender, e) => RebuildMember();

            deleteButton.Click += (sender, e) =>
            {
                if (memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    _selectedMembers.RemoveAt(memberIndex);
                }
                _partyListStackPanel.Children.Clear();
                foreach (string member in _selectedMembers) AddMemberToList(member);
                if (_selectedMembers.Count == 0) _noneLabel.Visibility = Visibility.Visible;
            };

            monitorComboBox.SelectionChanged += (sender, e) =>
            {
                if (memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    int currentLocation = locationComboBox.SelectedIndex + 1;
                    int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                    int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                    int currentX = int.TryParse(xTextBox.Text, out int x) ? x : 0;
                    int currentY = int.TryParse(yTextBox.Text, out int y) ? y : 0;
                    int currentMonitor = monitorComboBox.SelectedIndex >= 0 ? monitorComboBox.SelectedIndex : 0;
                    string newMemberInfo = $"{_selectedMembers[memberIndex].Split('|')[0]}|{currentLocation}|{currentWidth}|{currentHeight}|{currentX}|{currentY}|{currentMonitor}";
                    _selectedMembers[memberIndex] = newMemberInfo;
                }
            };

            if (_partyListStackPanel.Children.Count == 0) _noneLabel.Visibility = Visibility.Hidden;
            _partyListStackPanel.Children.Add(memberGrid);
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (_accountComboBox.SelectedItem == null) return;
            string selectedAccountName = _accountComboBox.SelectedItem.ToString();
            if (!_availableAccountObjects.ContainsKey(selectedAccountName)) return;
            Account selectedAccount = _availableAccountObjects[selectedAccountName];
            if (selectedAccount == null || selectedAccount.Characters == null || selectedAccount.Characters.Count == 0) return;

            List<string> charNames = selectedAccount.Characters.Where(c => c != null && !string.IsNullOrWhiteSpace(c.Name)).Select(c => c.Name).ToList();
            Character first = selectedAccount.Characters.FirstOrDefault(c => c != null && !string.IsNullOrWhiteSpace(c.Name));
            if (first == null) return;

            string memberInfo = $"{first.Name}|{first.InstallLocation}|1024|768|False|0|0|0";
            _selectedMembers.Add(memberInfo);
            AddMemberToList(memberInfo, charNames);
            _availableAccounts.Remove(selectedAccountName);
            _accountComboBox.SelectedIndex = -1;
        }

        private void DeletePartyButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteParty != null)
            {
                string partyId = !string.IsNullOrWhiteSpace(_originalPartyName) ? _originalPartyName : _partyNameTextBox.Text;
                if (!string.IsNullOrWhiteSpace(partyId))
                {
                    DeleteParty(this, new ListEventArgs() { Id = partyId });
                }
            }
        }
    }
}
