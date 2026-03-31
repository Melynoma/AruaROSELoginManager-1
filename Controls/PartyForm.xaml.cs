//
// FILE     : PartyForm.xaml.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-18
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using AruaRoseLoginManager.Data;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for PartyForm.xaml
    /// </summary>
    public partial class PartyForm : UserControl
    {
        /// <summary>
        /// The list of currently selected party members.
        /// </summary>
        private List<string> _selectedMembers;

        /// <summary>
        /// The list of all currently available accounts
        /// </summary>
        private ObservableCollection<string> _availableAccounts;

        /// <summary>
        /// Dictionary of available accounts with their characters (for new method)
        /// </summary>
        private Dictionary<string, Account> _availableAccountObjects;

        /// <summary>
        /// Current options defaults used when adding/updating party member launch settings.
        /// </summary>
        private WindowSize _defaultWindowSize;

        /// <summary>
        /// Event to raise when the cancel button is clicked
        /// </summary>
        [Browsable(true)]
        public event EventHandler Cancel;

        /// <summary>
        /// Event to raise when the save button is clicked
        /// </summary>
        [Browsable(true)]
        public event EventHandler<DataEventArgs<Party>> SaveParty;

        /// <summary>
        /// Constructor
        /// </summary>
        public PartyForm()
        {
            InitializeComponent();
            _selectedMembers = new List<string>();
            _availableAccountObjects = new Dictionary<string, Account>();
            _defaultWindowSize = WindowSize.Default;
        }

        /// <summary>
        /// Sets the option defaults used for new party member launch settings.
        /// </summary>
        /// <param name="sizeDefaults">Current option defaults</param>
        public void SetDefaultWindowSize(WindowSize sizeDefaults)
        {
            _defaultWindowSize = sizeDefaults ?? WindowSize.Default;
        }

        /// <summary>
        /// Populate the form with the party info
        /// </summary>
        /// <param name="party">Party info</param>
        public void PopulateFields(Party party)
        {
            _partyNameTextBox.Text = party.Name;
            _partyNameTextBox.IsEnabled = false;
            _descriptionTextBox.Text = party.Description;
            foreach(string member in party.Accounts)
            {
                // Normalize format to ensure full "CharacterName|Location|Width|Height" format with defaults
                string normalizedMember = NormalizeMemberFormat(member);
                _selectedMembers.Add(normalizedMember);
                AddMemberToList(normalizedMember);
            }
        }

        /// <summary>
        /// Ensures member format is "CharacterName|Location|Width|Height|Fullscreen|PosX|PosY|Monitor" with defaults for missing parts
        /// </summary>
        /// <param name="member">Member in any format</param>
        /// <returns>Normalized member format</returns>
        private string NormalizeMemberFormat(string member)
        {
            if (string.IsNullOrWhiteSpace(member))
                return member;

            string[] parts = member.Split('|');
            string characterName = parts[0];
            int location = 1;
            int width;
            int height;
            bool isFullscreen;
            int posX = 0;
            int posY = 0;
            int monitor = 0;

            if (parts.Length >= 2 && int.TryParse(parts[1], out int loc))
            {
                location = loc;
            }

            GetDefaultsForLocation(location, out width, out height, out isFullscreen);

            if (parts.Length >= 4)
            {
                if (int.TryParse(parts[2], out int parsedWidth))
                {
                    width = parsedWidth;
                }

                if (int.TryParse(parts[3], out int parsedHeight))
                {
                    height = parsedHeight;
                }
            }
            if (parts.Length >= 5)
            {
                if (bool.TryParse(parts[4], out bool parsedFullscreen))
                {
                    isFullscreen = parsedFullscreen;
                }
            }

            if (parts.Length >= 6)
            {
                if (int.TryParse(parts[5], out int parsedPosX))
                {
                    posX = parsedPosX;
                }
            }

            if (parts.Length >= 7)
            {
                if (int.TryParse(parts[6], out int parsedPosY))
                {
                    posY = parsedPosY;
                }
            }

            if (parts.Length >= 8)
            {
                if (int.TryParse(parts[7], out int parsedMonitor))
                {
                    monitor = parsedMonitor;
                }
            }

            return $"{characterName}|{location}|{width}|{height}|{isFullscreen}|{posX}|{posY}|{monitor}";
        }

        /// <summary>
        /// Gets default width/height/fullscreen settings for a specific install location.
        /// </summary>
        private void GetDefaultsForLocation(int location, out int width, out int height, out bool isFullscreen)
        {
            switch (location)
            {
                case 2:
                    width = _defaultWindowSize.GameWidth2;
                    height = _defaultWindowSize.GameHeight2;
                    isFullscreen = _defaultWindowSize.IsFullscreen2;
                    break;
                case 3:
                    width = _defaultWindowSize.GameWidth3;
                    height = _defaultWindowSize.GameHeight3;
                    isFullscreen = _defaultWindowSize.IsFullscreen3;
                    break;
                default:
                    width = _defaultWindowSize.GameWidth1;
                    height = _defaultWindowSize.GameHeight1;
                    isFullscreen = _defaultWindowSize.IsFullscreen1;
                    break;
            }
        }

        /// <summary>
        /// Updates a selected member entry by character name.
        /// </summary>
        private void UpdateSelectedMember(string characterName, int location, int width, int height, bool isFullscreen, int posX, int posY, int monitor)
        {
            string newMemberInfo = $"{characterName}|{location}|{width}|{height}|{isFullscreen}|{posX}|{posY}|{monitor}";
            for (int i = 0; i < _selectedMembers.Count; i++)
            {
                string member = _selectedMembers[i];
                string memberCharName = member.Contains("|") ? member.Split('|')[0] : member;
                if (memberCharName == characterName)
                {
                    _selectedMembers[i] = newMemberInfo;
                    break;
                }
            }
        }

        /// <summary>
        /// Populate the list of eligible accounts for the party (legacy method with account names)
        /// </summary>
        /// <param name="availableAccounts">The available account names for the party</param>
        public void PopulateAccounts(IEnumerable<string> availableAccounts)
        {
            _availableAccounts = new ObservableCollection<string>(availableAccounts.Where(x => !_selectedMembers.Any(member => member.StartsWith(x + ":"))));
            _accountComboBox.ItemsSource = _availableAccounts;
        }

        /// <summary>
        /// Populate the list of eligible accounts with their characters (new method)
        /// </summary>
        /// <param name="availableAccounts">The available accounts with their characters</param>
        public void PopulateAccountsWithCharacters(Dictionary<string, Account> availableAccounts)
        {
            _availableAccountObjects = availableAccounts;
            List<string> accountNames = new List<string>();
            foreach(var account in availableAccounts.Values)
            {
                if (account != null && account.Characters.Count > 0)
                {
                    accountNames.Add(account.Username);
                }
            }
            _availableAccounts = new ObservableCollection<string>(accountNames);
            _accountComboBox.ItemsSource = _availableAccounts;
        }

        /// <summary>
        /// Clears the party form fields
        /// </summary>
        public void ClearFields()
        {
            _partyNameTextBox.IsEnabled = true;
            _partyNameTextBox.Clear();
            _descriptionTextBox.Clear();
            _selectedMembers.Clear();
            _partyListStackPanel.Children.Clear();
            _noneLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Focuses on the primary input of the form (party name)
        /// </summary>
        public void FocusPrimary()
        {
            _partyNameTextBox.Focus();
        }

        /// <summary>
        /// Event handler for clicking the cancel button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (sender != null && Cancel != null)
            {
                Cancel(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for clicking the save button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void SavePartyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPartyNameValid())
            {
                return;
            }

            if (sender != null && SaveParty != null)
            {
                DataEventArgs<Party> args = new DataEventArgs<Party>()
                {
                    Data = new Party(
                        _partyNameTextBox.Text,
                        new List<string>(_selectedMembers),
                        _descriptionTextBox.Text
                    )
                };
                ClearFields();
                SaveParty(this, args);
            }
        }

        /// <summary>
        /// Event handler for validating the party name on input
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void _partyNameError_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            IsPartyNameValid();
        }

        /// <summary>
        /// Validates a party name to not be empty or have any whitespace.
        /// </summary>
        /// <returns>True if valid, false if not</returns>
        private bool IsPartyNameValid()
        {
            if (string.IsNullOrWhiteSpace(_partyNameTextBox.Text))
            {
                _partyNameError.Visibility = Visibility.Visible;
                return false;
            }
            else if (_selectedMembers.Count < 2)
            {
                _partyNameError.Visibility = Visibility.Hidden;
                _partyMembersError.Visibility = Visibility.Visible;
                return false;
            }

            _partyMembersError.Visibility = Visibility.Hidden;
            _partyNameError.Visibility = Visibility.Hidden;
            return true;
        }

        /// <summary>
        /// Adds a party member to the display with location, resolution, fullscreen and position settings
        /// </summary>
        /// <param name="memberInfo">Member info in format "CharacterName|InstallLocation|GameWidth|GameHeight|Fullscreen|PosX|PosY|Monitor"</param>
        private void AddMemberToList(string memberInfo)
        {
            // Parse the member info
            string characterName = memberInfo;
            int installLocation = 1;
            int gameWidth;
            int gameHeight;
            bool isFullscreen;
            int gamePosX = 0;
            int gamePosY = 0;
            int monitor = 0;

            GetDefaultsForLocation(installLocation, out gameWidth, out gameHeight, out isFullscreen);
            
            if (memberInfo.Contains("|"))
            {
                string[] parts = memberInfo.Split('|');
                characterName = parts[0];
                if (parts.Length >= 2 && int.TryParse(parts[1], out int location))
                {
                    installLocation = location;
                }
                if (parts.Length >= 4)
                {
                    if (int.TryParse(parts[2], out int parsedWidth))
                    {
                        gameWidth = parsedWidth;
                    }

                    if (int.TryParse(parts[3], out int parsedHeight))
                    {
                        gameHeight = parsedHeight;
                    }
                }
                if (parts.Length >= 5)
                {
                    if (bool.TryParse(parts[4], out bool parsedFullscreen))
                    {
                        isFullscreen = parsedFullscreen;
                    }
                }
                if (parts.Length >= 6)
                {
                    if (int.TryParse(parts[5], out int parsedPosX))
                    {
                        gamePosX = parsedPosX;
                    }
                }
                if (parts.Length >= 7)
                {
                    if (int.TryParse(parts[6], out int parsedPosY))
                    {
                        gamePosY = parsedPosY;
                    }
                }
                if (parts.Length >= 8)
                {
                    if (int.TryParse(parts[7], out int parsedMonitor))
                    {
                        monitor = parsedMonitor;
                    }
                }
            }

            // Create a grid for this member with location, resolution, fullscreen, X/Y and delete controls.
            Grid memberGrid = new Grid();
            memberGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
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

            // Character name label
            Label nameLabel = new Label()
            {
                Content = characterName,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameLabel, 0);
            memberGrid.Children.Add(nameLabel);

            // Location dropdown
            ComboBox locationComboBox = new ComboBox()
            {
                SelectedIndex = installLocation - 1,
                Margin = new Thickness(2)
            };
            locationComboBox.Items.Add("Location 1");
            locationComboBox.Items.Add("Location 2");
            locationComboBox.Items.Add("Location 3");
            Grid.SetColumn(locationComboBox, 1);
            memberGrid.Children.Add(locationComboBox);

            // Width textbox
            TextBox widthTextBox = new TextBox()
            {
                Text = gameWidth.ToString(),
                Margin = new Thickness(2),
                Width = 55
            };
            Grid.SetColumn(widthTextBox, 2);
            memberGrid.Children.Add(widthTextBox);

            // Height textbox
            TextBox heightTextBox = new TextBox()
            {
                Text = gameHeight.ToString(),
                Margin = new Thickness(2),
                Width = 55
            };
            Grid.SetColumn(heightTextBox, 4);
            memberGrid.Children.Add(heightTextBox);

            // Fullscreen checkbox
            CheckBox fullscreenCheckbox = new CheckBox()
            {
                Content = "Fullscreen",
                Margin = new Thickness(2),
                IsChecked = isFullscreen,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(fullscreenCheckbox, 5);
            memberGrid.Children.Add(fullscreenCheckbox);

            // X position editor
            StackPanel posXPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };
            posXPanel.Children.Add(new Label() { Content = "X", Padding = new Thickness(0), Width = 10, VerticalContentAlignment = VerticalAlignment.Center });
            TextBox posXTextBox = new TextBox()
            {
                Text = gamePosX.ToString(),
                Width = 38,
                ToolTip = "Window X"
            };
            posXPanel.Children.Add(posXTextBox);
            Grid.SetColumn(posXPanel, 6);
            memberGrid.Children.Add(posXPanel);

            // Y position editor
            StackPanel posYPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2),
                VerticalAlignment = VerticalAlignment.Center
            };
            posYPanel.Children.Add(new Label() { Content = "Y", Padding = new Thickness(0), Width = 10, VerticalContentAlignment = VerticalAlignment.Center });
            TextBox posYTextBox = new TextBox()
            {
                Text = gamePosY.ToString(),
                Width = 38,
                ToolTip = "Window Y"
            };
            posYPanel.Children.Add(posYTextBox);
            Grid.SetColumn(posYPanel, 7);
            memberGrid.Children.Add(posYPanel);

            Button setCursorButton = new Button()
            {
                Content = "Set",
                Width = 38,
                Height = 22,
                Margin = new Thickness(2),
                ToolTip = "Click Set, then click target screen location"
            };
            Grid.SetColumn(setCursorButton, 8);
            memberGrid.Children.Add(setCursorButton);

            ComboBox monitorComboBox = new ComboBox()
            {
                Margin = new Thickness(2),
                Width = 64
            };
            monitorComboBox.Items.Add("0");
            monitorComboBox.Items.Add("1");
            monitorComboBox.Items.Add("2");
            monitorComboBox.SelectedIndex = (monitor >= 0 && monitor <= 2) ? monitor : 0;
            Grid.SetColumn(monitorComboBox, 9);
            memberGrid.Children.Add(monitorComboBox);

            // Delete button
            Button deleteButton = new Button()
            {
                Content = "X",
                Width = 25,
                Height = 25,
                Margin = new Thickness(2)
            };
            Grid.SetColumn(deleteButton, 10);
            memberGrid.Children.Add(deleteButton);

            // Handle location dropdown changes
            locationComboBox.SelectionChanged += (locationSender, locationArgs) =>
            {
                if (locationComboBox.SelectedIndex >= 0)
                {
                    int newLocation = locationComboBox.SelectedIndex + 1;

                    // Location change picks defaults from Options for that location.
                    GetDefaultsForLocation(newLocation, out int locationWidth, out int locationHeight, out bool locationFullscreen);
                    widthTextBox.Text = locationWidth.ToString();
                    heightTextBox.Text = locationHeight.ToString();
                    fullscreenCheckbox.IsChecked = locationFullscreen;

                    int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                    int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                    int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                    UpdateSelectedMember(characterName, newLocation, locationWidth, locationHeight, locationFullscreen, currentPosX, currentPosY, currentMonitor);
                }
            };

            // Handle width textbox changes
            widthTextBox.TextChanged += (widthSender, widthArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, currentPosX, currentPosY, currentMonitor);
            };

            // Handle height textbox changes
            heightTextBox.TextChanged += (heightSender, heightArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, currentPosX, currentPosY, currentMonitor);
            };

            posXTextBox.TextChanged += (posXSender, posXArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, currentPosX, currentPosY, currentMonitor);
            };

            posYTextBox.TextChanged += (posYSender, posYArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, currentPosX, currentPosY, currentMonitor);
            };

            setCursorButton.Click += (xySender, xyArgs) =>
            {
                setCursorButton.IsEnabled = false;
                setCursorButton.Content = "...";

                CaptureNextScreenClick(
                    (screenX, screenY) =>
                    {
                        posXTextBox.Text = screenX.ToString();
                        posYTextBox.Text = screenY.ToString();

                        int currentLocation = locationComboBox.SelectedIndex + 1;
                        int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                        int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                        bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                        int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                        UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, screenX, screenY, currentMonitor);
                    },
                    () =>
                    {
                        setCursorButton.IsEnabled = true;
                        setCursorButton.Content = "Set";
                    }
                );
            };

            monitorComboBox.SelectionChanged += (monitorSender, monitorArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                bool currentFullscreen = fullscreenCheckbox.IsChecked == true;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, currentFullscreen, currentPosX, currentPosY, currentMonitor);
            };

            // Handle fullscreen changes
            fullscreenCheckbox.Checked += (fullscreenSender, fullscreenArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, true, currentPosX, currentPosY, currentMonitor);
            };
            fullscreenCheckbox.Unchecked += (fullscreenSender, fullscreenArgs) =>
            {
                int currentLocation = locationComboBox.SelectedIndex + 1;
                int currentWidth = int.TryParse(widthTextBox.Text, out int w) ? w : 1024;
                int currentHeight = int.TryParse(heightTextBox.Text, out int h) ? h : 768;
                int currentPosX = int.TryParse(posXTextBox.Text, out int x) ? x : 0;
                int currentPosY = int.TryParse(posYTextBox.Text, out int y) ? y : 0;
                int currentMonitor = int.TryParse(monitorComboBox.SelectedItem?.ToString(), out int m) ? m : 0;
                UpdateSelectedMember(characterName, currentLocation, currentWidth, currentHeight, false, currentPosX, currentPosY, currentMonitor);
            };

            // Handle delete button click
            deleteButton.Click += (sender, e) =>
            {
                // Find and remove the member by character name
                for (int i = _selectedMembers.Count - 1; i >= 0; i--)
                {
                    string member = _selectedMembers[i];
                    string memberCharName = member.Contains("|") ? member.Split('|')[0] : member;
                    if (memberCharName == characterName)
                    {
                        _selectedMembers.RemoveAt(i);
                        break;
                    }
                }
                
                _partyListStackPanel.Children.Clear();
                foreach(string member in _selectedMembers)
                {
                    AddMemberToList(member);
                }
                
                // Show "None" label if no members left
                if (_selectedMembers.Count == 0)
                {
                    _noneLabel.Visibility = Visibility.Visible;
                }
            };

            // Hide "None" label when first member is added
            if (_partyListStackPanel.Children.Count == 0)
            {
                _noneLabel.Visibility = Visibility.Hidden;
            }

            _partyListStackPanel.Children.Add(memberGrid);
        }

        /// <summary>
        /// Event handler for clicking the add account button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (_accountComboBox.SelectedItem != null)
            {
                string selectedAccountName = _accountComboBox.SelectedItem.ToString();
                
                // Add each character from the selected account
                if (_availableAccountObjects.ContainsKey(selectedAccountName))
                {
                    Account selectedAccount = _availableAccountObjects[selectedAccountName];
                    if (selectedAccount != null && selectedAccount.Characters != null)
                    {
                        foreach (Character character in selectedAccount.Characters)
                        {
                            if (character != null && !string.IsNullOrWhiteSpace(character.Name))
                            {
                                GetDefaultsForLocation(character.InstallLocation, out int defaultWidth, out int defaultHeight, out bool defaultFullscreen);
                                string memberInfo = $"{character.Name}|{character.InstallLocation}|{defaultWidth}|{defaultHeight}|{defaultFullscreen}|0|0|0";
                                if (!_selectedMembers.Contains(memberInfo))
                                {
                                    _selectedMembers.Add(memberInfo);
                                    AddMemberToList(memberInfo);
                                }
                            }
                        }
                        _availableAccounts.Remove(selectedAccountName);
                        _accountComboBox.SelectedIndex = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Opens a temporary full-screen transparent overlay and captures the next mouse click in screen coordinates.
        /// </summary>
        private void CaptureNextScreenClick(Action<int, int> onCaptured, Action onFinished)
        {
            bool captureArmed = false;
            Grid overlayRoot = new Grid();

            Border hintBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 24, 24, 24)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(14)
            };

            TextBlock hintText = new TextBlock
            {
                Text = "Select position: left-click target point. Right-click or Esc to cancel.",
                Foreground = Brushes.White,
                FontSize = 13
            };
            hintBorder.Child = hintText;
            overlayRoot.Children.Add(hintBorder);

            Window overlay = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Topmost = true,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0)),
                Cursor = Cursors.Cross,
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = overlayRoot
            };

            DispatcherTimer armTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(175)
            };
            armTimer.Tick += (sender, e) =>
            {
                captureArmed = true;
                armTimer.Stop();
            };

            overlay.PreviewMouseLeftButtonDown += (sender, e) =>
            {
                // Ignore the original XY-button click sequence; only capture on the next intentional click.
                if (!captureArmed)
                {
                    e.Handled = true;
                }
            };

            overlay.PreviewMouseLeftButtonUp += (sender, e) =>
            {
                if (!captureArmed)
                {
                    e.Handled = true;
                    return;
                }

                Point clicked = overlay.PointToScreen(e.GetPosition(overlay));
                overlay.Close();
                onCaptured?.Invoke((int)clicked.X, (int)clicked.Y);
                e.Handled = true;
            };

            overlay.PreviewMouseRightButtonDown += (sender, e) =>
            {
                overlay.Close();
                e.Handled = true;
            };

            overlay.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    overlay.Close();
                    e.Handled = true;
                }
            };

            overlay.Closed += (sender, e) =>
            {
                armTimer.Stop();
                onFinished?.Invoke();
            };

            overlay.Show();
            overlay.Focus();
            Keyboard.Focus(overlay);
            armTimer.Start();
        }
    }
}
