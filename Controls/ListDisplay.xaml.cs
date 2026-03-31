//
// FILE     : ListDisplay.xaml.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-18
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AruaRoseLoginManager.Data;
using AruaRoseLoginManager.Enum;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for AccountDisplay.xaml
    /// </summary>
    public partial class ListDisplay : UserControl
    {
        /// <summary>
        /// Used to make #EAEAEA/rgb(234, 234, 234)
        /// </summary>
        private const byte ODD_BACKGROUND_COLOUR_VALUE = 234;

        /// <summary>
        /// Used to make #FAFAFA/rgb(250, 250, 250)
        /// </summary>
        private const byte EVEN_BACKGROUD_COLOUR_VALUE = 250;

        /// <summary>
        /// Tracks the position of the display in the stack view
        /// </summary>
        private int _position;

        /// <summary>
        /// Unique identifier for actions carried out on the list
        /// </summary>
        private string _itemIdentifier;

        /// <summary>
        /// Stores the account information for access during login
        /// </summary>
        private Account _accountInfo;

        /// <summary>
        /// The background colour to use for odd positions
        /// </summary>
        private SolidColorBrush _oddBackgroundColour;

        /// <summary>
        /// The background colour to use for even positions
        /// </summary>
        private SolidColorBrush _evenBackgroundColour;

        /// <summary>
        /// Flag to show extras like the emblem and password saved icon
        /// </summary>
        private bool _showExtras = true;

        /// <summary>
        /// Whether stream mode is enabled (for anonymization)
        /// </summary>
        private bool _streamMode = false;

        /// <summary>
        /// Start point for drag detection.
        /// </summary>
        private Point _dragStartPoint;

        /// <summary>
        /// Whether a drag may be initiated from current mouse down.
        /// </summary>
        private bool _canStartDrag;

        /// <summary>
        /// Drag data payload for row reordering.
        /// </summary>
        private class RowDragData
        {
            public string Id { get; set; }

            public int Position { get; set; }
        }

        /// <summary>
        /// Event to raise when the delete button is pressed
        /// </summary>
        public event EventHandler<ListEventArgs> DeleteListItem;

        /// <summary>
        /// Event to raise when the edit button is pressed
        /// </summary>
        public event EventHandler<ListEventArgs> EditListItem;

        /// <summary>
        /// Event to raise when the Arua/Classic login buttons are pressed
        /// </summary>
        public event EventHandler<LoginEventArgs> Login;

        /// <summary>
        /// Event to raise when the move up/down buttons are pressed
        /// </summary>
        public event EventHandler<MoveListItemEventArgs> MoveListItem;

        /// <summary>
        /// Constructor (common)
        /// </summary>
        /// <param name="position">Position in the list</param>
        private ListDisplay(int position)
        {
            InitializeComponent();
            _position = position;
            _evenBackgroundColour = new SolidColorBrush(Color.FromRgb(EVEN_BACKGROUD_COLOUR_VALUE, EVEN_BACKGROUD_COLOUR_VALUE, EVEN_BACKGROUD_COLOUR_VALUE));
            _oddBackgroundColour = new SolidColorBrush(Color.FromRgb(ODD_BACKGROUND_COLOUR_VALUE, ODD_BACKGROUND_COLOUR_VALUE, ODD_BACKGROUND_COLOUR_VALUE));
        }

        /// <summary>
        /// Constructor (for accounts)
        /// </summary>
        /// <param name="position">Position in the list</param>
        /// <param name="info">Account info</param>
        /// <param name="emblem">Emblem to use</param>
        public ListDisplay(int position, Account info, ImageSource emblem) : this(position)
        {
            _itemIdentifier = info.Username;
            _accountInfo = info;
            
            // Fill in account info
            _emblem.Source = emblem;
            _listItemIdentifier.Text = _streamMode ? "***********" : info.Username;
            _allInfoIncluded.Source = GetPasswordSavedIcon(info.PasswordHash);
            _listItemDescription.Text = info.Description;
            
            // Extract character names from Character objects
            List<string> characterNames = new List<string>();
            if (info.Characters != null)
            {
                foreach (Character character in info.Characters)
                {
                    if (character != null && !string.IsNullOrWhiteSpace(character.Name))
                    {
                        characterNames.Add(character.Name);
                    }
                }
            }
            _listItemDetails.Text = string.Join(", ", characterNames.ToArray());
        }

        /// <summary>
        /// Constructor (for parties)
        /// </summary>
        /// <param name="position">Position in the list</param>
        /// <param name="info">Party info</param>
        public ListDisplay(int position, Party info) : this(position)
        {
            _itemIdentifier = info.Name;
            _showExtras = false;
            _aruaGuardButton.Visibility = Visibility.Collapsed;

            // Fill in party info
            _listItemIdentifier.Text = info.Name;
            _allInfoIncluded.Visibility = Visibility.Hidden;
            _listItemDescription.Text = info.Description;
            
            // Parse party members to display with install locations and resolutions
            List<string> displayMembers = new List<string>();
            foreach (string member in info.Accounts)
            {
                if (member.Contains("|"))
                {
                    // Parse format: "CharacterName|InstallLocation|GameWidth|GameHeight|Fullscreen|PosX|PosY|Monitor"
                    string[] parts = member.Split('|');
                    string characterName = parts[0];
                    int location = 1;
                    int width = 1024;
                    int height = 768;
                    bool isFullscreen = false;
                    int posX = 0;
                    int posY = 0;
                    int monitor = 0;
                    
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int loc))
                    {
                        location = loc;
                    }
                    if (parts.Length >= 4)
                    {
                        int.TryParse(parts[2], out width);
                        int.TryParse(parts[3], out height);
                    }
                    if (parts.Length >= 5)
                    {
                        bool.TryParse(parts[4], out isFullscreen);
                    }

                    if (parts.Length >= 6)
                    {
                        int.TryParse(parts[5], out posX);
                    }

                    if (parts.Length >= 7)
                    {
                        int.TryParse(parts[6], out posY);
                    }

                    if (parts.Length >= 8)
                    {
                        int.TryParse(parts[7], out monitor);
                    }

                    string modeText = isFullscreen ? "Fullscreen" : $"{width}x{height}";
                    string positionText = isFullscreen ? "" : $" | Pos {posX},{posY}";
                    string monitorText = $" | Monitor {monitor}";
                    displayMembers.Add($"{characterName} [Location {location} | {modeText}{positionText}{monitorText}]");
                }
                else
                {
                    // Legacy format: just account/character name - add defaults
                    displayMembers.Add($"{member} [Location 1 | 1024x768 | Pos 0,0 | Monitor 0]");
                }
            }

            // Show each character/member on its own line for easier scanning in Parties.
            _listItemDetails.Text = string.Join(Environment.NewLine, displayMembers);
            _listItemDetails.TextWrapping = TextWrapping.Wrap;
            _listItemDetails.TextTrimming = TextTrimming.None;
            _listItemDetails.VerticalAlignment = VerticalAlignment.Top;

            // Grow row height to fit multiple member lines without clipping.
            int lineCount = Math.Max(displayMembers.Count, 1);
            _listDisplayGrid.MinHeight = Math.Max(44, 10 + (lineCount * 18));
        }

        /// <summary>
        /// Updates the display based on a new position and how many total displays there are
        /// </summary>
        /// <param name="newPosition">New position in the list</param>
        /// <param name="totalDisplays">Total number of displays in the list</param>
        public void UpdateDisplay(int newPosition, int totalDisplays)
        {
            _position = newPosition;
            UpdateBackground();
            UpdateButtons(totalDisplays);
        }

        /// <summary>
        /// Applies externally-controlled column widths for account/party table alignment.
        /// </summary>
        /// <param name="widths">Column widths for the first 8 columns</param>
        public void SetColumnWidths(double[] widths)
        {
            if (widths == null || widths.Length < 8)
            {
                return;
            }

            _col1.Width = new GridLength(widths[0], GridUnitType.Pixel);
            _col2.Width = new GridLength(widths[1], GridUnitType.Pixel);
            _col3.Width = new GridLength(widths[2], GridUnitType.Pixel);
            _col4.Width = new GridLength(widths[3], GridUnitType.Pixel);
            _col5.Width = new GridLength(widths[4], GridUnitType.Pixel);
            _col6.Width = new GridLength(widths[5], GridUnitType.Pixel);
            _col7.Width = new GridLength(widths[6], GridUnitType.Pixel);
            _col8.Width = new GridLength(widths[7], GridUnitType.Pixel);
        }

        /// <summary>
        /// Update stream mode and refresh account name display
        /// </summary>
        /// <param name="streamMode">Whether stream mode is enabled</param>
        public void SetStreamMode(bool streamMode)
        {
            _streamMode = streamMode;
            
            // Account usernames are hidden in stream mode; party names are always shown
            if (_accountInfo != null)
            {
                _listItemIdentifier.Text = _streamMode ? "***********" : _accountInfo.Username;
            }
            else if (!string.IsNullOrEmpty(_itemIdentifier) && _itemIdentifier != "Account")
            {
                // Party name - never hidden
                _listItemIdentifier.Text = _itemIdentifier;
            }
        }

        /// <summary>
        /// Determines if the password saved icon should be shown or not
        /// </summary>
        /// <param name="passwordHash">The password hash</param>
        /// <returns>The icon image to display</returns>
        private BitmapImage GetPasswordSavedIcon(string passwordHash)
        {
            string iconPath = string.IsNullOrWhiteSpace(passwordHash)
                ? "pack://application:,,,/Assets/exclamation-icon.png"
                : "pack://application:,,,/Assets/check-icon.png";
            return new BitmapImage(new Uri(iconPath));
        }

        /// <summary>
        /// Updates the background colour to be different for even and odd positions
        /// </summary>
        private void UpdateBackground()
        {
            // Even
            if (_position % 2 == 0)
            {
                _listDisplayGrid.Background = _evenBackgroundColour;
            }
            // Odd
            else
            {
                _listDisplayGrid.Background = _oddBackgroundColour;
            }
        }

        /// <summary>
        /// Updates the move buttons to be enabled/disabled based on position.
        /// </summary>
        /// <param name="totalDisplays">Total displays in the list</param>
        private void UpdateButtons(int totalDisplays)
        {
            // Hide the move up button if it's the first one
            if (_position == 0)
            {
                _moveUpButton.IsEnabled = false;
            }
            else
            {
                _moveUpButton.IsEnabled = true;
            }

            // Hide the move down button if it's the last one
            if (_position == totalDisplays - 1)
            {
                _moveDownButton.IsEnabled = false;
            }
            else
            {
                _moveDownButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Changes the display into its compact state by removing some elements
        /// </summary>
        private void CompactDisplay()
        {
            _allInfoIncluded.Visibility = Visibility.Hidden;
            _aruaGuardButton.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// Changes the display into its normal state by showing all elements
        /// </summary>
        private void NormalDisplay()
        {
            if (_showExtras)
            {
                _allInfoIncluded.Visibility = Visibility.Visible;
                _aruaGuardButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Event handler for when the Arua login button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AruaLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && Login != null)
            {
                // Use the first character's install location, or default to 1 if no characters
                int installLocation = 1;
                string characterName = "";
                if (_accountInfo != null && _accountInfo.Characters != null && _accountInfo.Characters.Count > 0)
                {
                    Character firstChar = _accountInfo.Characters[0];
                    if (firstChar != null)
                    {
                        characterName = firstChar.Name;
                        installLocation = firstChar.InstallLocation;
                    }
                }

                LoginEventArgs args = new LoginEventArgs()
                {
                    Id = _itemIdentifier,
                    ServerId = Server.Arua,
                    CharacterName = characterName,
                    CharacterInstallLocation = installLocation
                };
                Login(this, args);
            }
        }

        /// <summary>
        /// Event handler for when the classic login button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void ClassicLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && Login != null)
            {
                // Use the first character's install location, or default to 1 if no characters
                int installLocation = 1;
                string characterName = "";
                if (_accountInfo != null && _accountInfo.Characters != null && _accountInfo.Characters.Count > 0)
                {
                    Character firstChar = _accountInfo.Characters[0];
                    if (firstChar != null)
                    {
                        characterName = firstChar.Name;
                        installLocation = firstChar.InstallLocation;
                    }
                }

                LoginEventArgs args = new LoginEventArgs()
                {
                    Id = _itemIdentifier,
                    ServerId = Server.Classic,
                    CharacterName = characterName,
                    CharacterInstallLocation = installLocation
                };
                Login(this, args);
            }
        }

        /// <summary>
        /// Opens AruaGuard login page for this account.
        /// </summary>
        private void AruaGuardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_accountInfo == null)
            {
                return;
            }

            Window owner = Application.Current == null ? null : Application.Current.MainWindow;
            WebsitePasswordPrompt prompt = new WebsitePasswordPrompt(_accountInfo.Username)
            {
                Owner = owner
            };

            bool? accepted = prompt.ShowDialog();
            if (accepted != true)
            {
                return;
            }

            WebsiteLoginWindow loginWindow = new WebsiteLoginWindow(_accountInfo.Username, prompt.WebsitePassword);
            if (owner != null)
            {
                loginWindow.Owner = owner;
            }

            loginWindow.Show();
        }

        /// <summary>
        /// Event handler for when the delete button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && DeleteListItem != null)
            {
                ListEventArgs args = new ListEventArgs()
                {
                    Id = _itemIdentifier
                };
                DeleteListItem(this, args);
            }
        }

        /// <summary>
        /// Event handler for when the edit button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && EditListItem != null)
            {
                ListEventArgs args = new ListEventArgs()
                {
                    Id = _itemIdentifier
                };
                EditListItem(this, args);
            }
        }

        /// <summary>
        /// Event handler for when the move up button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && MoveListItem != null)
            {
                MoveListItemEventArgs args = new MoveListItemEventArgs()
                {
                    Id = _itemIdentifier,
                    Direction = MovementDirection.Up
                };
                MoveListItem(this, args);
            }
        }

        /// <summary>
        /// Event handler for when the move down button is clicked
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && MoveListItem != null)
            {
                MoveListItemEventArgs args = new MoveListItemEventArgs()
                {
                    Id = _itemIdentifier,
                    Direction = MovementDirection.Down
                };
                MoveListItem(this, args);
            }
        }

        /// <summary>
        /// Event handler for when the display size changes
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void ListDisplayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 400)
            {
                CompactDisplay();
            } else
            {
                NormalDisplay();
            }
        }

        /// <summary>
        /// Captures start point for row drag operations.
        /// </summary>
        private void ListDisplayGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            _canStartDrag = !IsClickFromButton(e.OriginalSource as DependencyObject);
        }

        /// <summary>
        /// Starts drag operation when mouse movement exceeds system drag threshold.
        /// </summary>
        private void ListDisplayGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_canStartDrag || e.LeftButton != MouseButtonState.Pressed || string.IsNullOrWhiteSpace(_itemIdentifier))
            {
                return;
            }

            Point current = e.GetPosition(this);
            bool movedEnough = Math.Abs(current.X - _dragStartPoint.X) >= SystemParameters.MinimumHorizontalDragDistance
                || Math.Abs(current.Y - _dragStartPoint.Y) >= SystemParameters.MinimumVerticalDragDistance;

            if (!movedEnough)
            {
                return;
            }

            _canStartDrag = false;
            RowDragData data = new RowDragData()
            {
                Id = _itemIdentifier,
                Position = _position
            };
            DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
        }

        /// <summary>
        /// Enables move effect only for valid row drag payloads.
        /// </summary>
        private void ListDisplayGrid_DragOver(object sender, DragEventArgs e)
        {
            RowDragData data = e.Data.GetData(typeof(RowDragData)) as RowDragData;
            e.Effects = data == null ? DragDropEffects.None : DragDropEffects.Move;
            e.Handled = true;
        }

        /// <summary>
        /// Reorders rows by emitting MoveListItem events until source reaches drop index.
        /// </summary>
        private void ListDisplayGrid_Drop(object sender, DragEventArgs e)
        {
            RowDragData data = e.Data.GetData(typeof(RowDragData)) as RowDragData;
            if (data == null || MoveListItem == null || string.IsNullOrWhiteSpace(data.Id))
            {
                return;
            }

            int sourceIndex = data.Position;
            int targetIndex = _position;
            bool dropAfter = e.GetPosition(this).Y > (ActualHeight / 2.0);

            int finalIndex;
            if (sourceIndex < targetIndex)
            {
                finalIndex = dropAfter ? targetIndex : targetIndex - 1;
            }
            else if (sourceIndex > targetIndex)
            {
                finalIndex = dropAfter ? targetIndex + 1 : targetIndex;
            }
            else
            {
                finalIndex = dropAfter ? sourceIndex + 1 : sourceIndex;
            }

            if (finalIndex < 0)
            {
                finalIndex = 0;
            }

            int steps = finalIndex - sourceIndex;
            if (steps == 0)
            {
                return;
            }

            MovementDirection direction = steps > 0 ? MovementDirection.Down : MovementDirection.Up;
            for (int i = 0; i < Math.Abs(steps); i++)
            {
                MoveListItem(this, new MoveListItemEventArgs()
                {
                    Id = data.Id,
                    Direction = direction
                });
            }
        }

        /// <summary>
        /// Checks whether a mouse event originated from within a button.
        /// </summary>
        private bool IsClickFromButton(DependencyObject source)
        {
            DependencyObject current = source;
            while (current != null)
            {
                if (current is Button)
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
