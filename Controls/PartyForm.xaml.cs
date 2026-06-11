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

using AruaRoseLoginManager.Data;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for PartyForm.xaml
    /// </summary>
    public partial class PartyForm : UserControl
    {
        private List<string> _selectedMembers;
        private ObservableCollection<string> _availableAccounts;
        private Dictionary<string, Account> _availableAccountObjects;
        private WindowSize _defaultWindowSize;
        private string _originalPartyName;

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
                AddMemberToList(normalizedMember);
            }
        }

        private string NormalizeMemberFormat(string member)
        {
            if (string.IsNullOrWhiteSpace(member)) return member;
            string[] parts = member.Split('|');
            string characterName = parts[0];
            int location = 1;
            int width = 1024;
            int height = 768;
            if (parts.Length >= 2 && int.TryParse(parts[1], out int loc)) location = loc;
            if (parts.Length >= 4) int.TryParse(parts[2], out width); int.TryParse(parts[3], out height);
            return $"{characterName}|{location}|{width}|{height}";
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

        public void SetEditMode(bool isEdit)
        {
            _deletePartyButton.Visibility = isEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        public void FocusPrimary() => _partyNameTextBox.Focus();

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

            if (memberInfo.Contains("|"))
            {
                string[] parts = memberInfo.Split('|');
                characterName = parts[0];
                if (parts.Length >= 2 && int.TryParse(parts[1], out int location)) installLocation = location;
                if (parts.Length >= 4) int.TryParse(parts[2], out gameWidth); int.TryParse(parts[3], out gameHeight);
            }

            Grid memberGrid = new Grid();
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(50) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
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

            Button editButton = new Button() { Content = "Edit", Margin = new Thickness(2) };
            Grid.SetColumn(editButton, 1);
            memberGrid.Children.Add(editButton);

            ComboBox locationComboBox = new ComboBox() { SelectedIndex = installLocation - 1, Margin = new Thickness(2) };
            locationComboBox.Items.Add("Location 1");
            locationComboBox.Items.Add("Location 2");
            locationComboBox.Items.Add("Location 3");
            Grid.SetColumn(locationComboBox, 2);
            memberGrid.Children.Add(locationComboBox);

            TextBox widthTextBox = new TextBox() { Text = gameWidth.ToString(), Margin = new Thickness(2), Width = 55 };
            Grid.SetColumn(widthTextBox, 3);
            memberGrid.Children.Add(widthTextBox);

            TextBox heightTextBox = new TextBox() { Text = gameHeight.ToString(), Margin = new Thickness(2), Width = 55 };
            Grid.SetColumn(heightTextBox, 5);
            memberGrid.Children.Add(heightTextBox);

            Button deleteButton = new Button() { Content = "X", Width = 25, Height = 25, Margin = new Thickness(2) };
            Grid.SetColumn(deleteButton, 6);
            memberGrid.Children.Add(deleteButton);

            int memberIndex = _selectedMembers.Count - 1;

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
                    int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                    int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                    string newMemberInfo = $"{_selectedMembers[memberIndex].Split('|')[0]}|{newLocation}|{currentWidth}|{currentHeight}";
                    _selectedMembers[memberIndex] = newMemberInfo;
                }
            };

            widthTextBox.TextChanged += (sender, e) =>
            {
                if (memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    int currentLocation = locationComboBox.SelectedIndex + 1;
                    int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                    int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                    string newMemberInfo = $"{_selectedMembers[memberIndex].Split('|')[0]}|{currentLocation}|{currentWidth}|{currentHeight}";
                    _selectedMembers[memberIndex] = newMemberInfo;
                }
            };

            heightTextBox.TextChanged += (sender, e) =>
            {
                if (memberIndex >= 0 && memberIndex < _selectedMembers.Count)
                {
                    int currentLocation = locationComboBox.SelectedIndex + 1;
                    int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                    int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                    string newMemberInfo = $"{_selectedMembers[memberIndex].Split('|')[0]}|{currentLocation}|{currentWidth}|{currentHeight}";
                    _selectedMembers[memberIndex] = newMemberInfo;
                }
            };

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

            string memberInfo = $"{first.Name}|{first.InstallLocation}|1024|768";
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
