//
// FILE     : PartyDisplay.xaml.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-18
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using AruaRoseLoginManager.Data;
using AruaRoseLoginManager.Enum;
using AruaRoseLoginManager.Helpers;

namespace AruaRoseLoginManager.Controls
{
    /// <summary>
    /// Interaction logic for PartyDisplay.xaml
    /// </summary>
    public partial class PartyDisplay : UserControl, IPartyDisplay
    {
        /// <summary>
        /// The current panel being displayed
        /// </summary>
        private PanelMode _partyMode = PanelMode.Select;

        /// <summary>
        /// Tracks whether streamer mode is currently enabled.
        /// </summary>
        private bool _streamMode;

        /// <summary>
        /// Current widths for the 8 visible table columns.
        /// </summary>
        private double[] _columnWidths = new double[8];

        /// <summary>
        /// Minimum widths for each column (at least header width).
        /// </summary>
        private double[] _minColumnWidths = new double[8];

        /// <summary>
        /// Constructor
        /// </summary>
        public PartyDisplay()
        {
            InitializeComponent();
            InitializeColumnWidths();
        }

        /// <summary>
        /// Event raised when the New Party button is clicked
        /// </summary>
        public event EventHandler<EventArgs> NewRequest;

        /// <summary>
        /// Event raised when one of the login buttons are clicked
        /// </summary>
        public event EventHandler<LoginEventArgs> LoginRequest;

        /// <summary>
        /// Event raised when saving a new party
        /// </summary>
        public event EventHandler<DataEventArgs<Party>> AddRequest;

        /// <summary>
        /// Event raised when deleting a party
        /// </summary>
        public event EventHandler<ListEventArgs> DeleteRequest;

        /// <summary>
        /// Event raised when selecting a party to edit
        /// </summary>
        public event EventHandler<ListEventArgs> EditRequest;

        /// <summary>
        /// Event raised when trying to save the editted party
        /// </summary>
        public event EventHandler<DataEventArgs<Party>> UpdateRequest;

        /// <summary>
        /// Event raised when trying to move a party within the list
        /// </summary>
        public event EventHandler<MoveListItemEventArgs> MoveRequest;

        /// <summary>
        /// Adds a new party to the list
        /// </summary>
        /// <param name="newItem">The new party</param>
        public void AddToDisplay(Party newItem)
        {
            GrowColumnsForParty(newItem);

            ListDisplay newDisplay = new ListDisplay(
                _partyStackPanel.Children.Count,
                newItem
            );
            newDisplay.SetStreamMode(_streamMode);
            newDisplay.SetColumnWidths(_columnWidths);
            newDisplay.Login += PartyDisplay_LoginRequest;
            newDisplay.EditListItem += PartyDisplay_EditRequest;
            newDisplay.MoveListItem += PartyDisplay_MoveRequest;
            newDisplay.DeleteListItem += PartyDisplay_DeleteRequest;
            _partyStackPanel.Children.Add(newDisplay);
            for (int i = 0; i < _partyStackPanel.Children.Count; i++)
            {
                ListDisplay display = (ListDisplay)_partyStackPanel.Children[i];
                display.UpdateDisplay(i, _partyStackPanel.Children.Count);
            }
        }

        /// <summary>
        /// Clears the party list
        /// </summary>
        public void ClearDisplay()
        {
            _partyStackPanel.Children.Clear();
            InitializeColumnWidths();
        }

        /// <summary>
        /// Applies externally provided widths for the 8 display columns.
        /// </summary>
        /// <param name="widths">Column widths</param>
        public void SetColumnWidths(double[] widths)
        {
            if (widths == null || widths.Length < 8)
            {
                return;
            }

            // Prevent stale saved widths from expanding Accounts beyond current visible data.
            double maxAccountsDataWidth = Math.Max(_columnWidths[2], _minColumnWidths[2]);

            for (int i = 0; i < 8; i++)
            {
                double requestedWidth = Math.Max(widths[i], _minColumnWidths[i]);
                if (i == 2)
                {
                    requestedWidth = Math.Min(requestedWidth, maxAccountsDataWidth);
                }
                if (i == 5)
                {
                    requestedWidth = 0; // No AruaGuard column in party rows
                }

                _columnWidths[i] = requestedWidth;
            }

            ApplyColumnWidthsToHeader();
            ApplyColumnWidthsToRows();
        }

        /// <summary>
        /// Gets current widths for the 8 display columns.
        /// </summary>
        /// <returns>Column widths</returns>
        public double[] GetColumnWidths()
        {
            return (double[])_columnWidths.Clone();
        }

        /// <summary>
        /// Updates stream mode for all displayed parties
        /// </summary>
        /// <param name="streamMode">Whether stream mode is enabled</param>
        public void SetStreamMode(bool streamMode)
        {
            _streamMode = streamMode;
            for (int i = 0; i < _partyStackPanel.Children.Count; i++)
            {
                ListDisplay display = (ListDisplay)_partyStackPanel.Children[i];
                if (display != null)
                {
                    display.SetStreamMode(streamMode);
                }
            }
        }

        /// <summary>
        /// Populates the party form for a brand new party
        /// </summary>
        /// <param name="accounts">Accounts eligible to be added to a party</param>
        public void Prompt(IEnumerable<string> accounts)
        {
            _partyForm.ClearFields();
            _partyForm.SetEditMode(false);
            _partyForm.PopulateAccounts(accounts);
            SwitchPanels(PanelMode.New);
        }

        /// <summary>
        /// Populates the party form for editting a party
        /// </summary>
        /// <param name="accounts">Accounts eligible to be added to the party</param>
        /// <param name="info">The current party info</param>
        public void Prompt(IEnumerable<string> accounts, Party info)
        {
            _partyForm.ClearFields();
            _partyForm.PopulateFields(info);
            _partyForm.SetEditMode(true);
            _partyForm.PopulateAccounts(accounts);
            SwitchPanels(PanelMode.Edit);
        }

        /// <summary>
        /// Populates the party form for a brand new party with Account objects
        /// </summary>
        /// <param name="accounts">Account objects eligible to be added to a party</param>
        public void PromptWithAccounts(Dictionary<string, Account> accounts)
        {
            _partyForm.ClearFields();
            _partyForm.SetDefaultWindowSize(WindowSize.Default);
            _partyForm.SetEditMode(false);
            _partyForm.PopulateAccountsWithCharacters(accounts);
            SwitchPanels(PanelMode.New);
        }

        /// <summary>
        /// Populates the party form for a brand new party with Account objects and option defaults.
        /// </summary>
        /// <param name="accounts">Account objects eligible to be added to a party</param>
        /// <param name="sizeDefaults">Current option defaults</param>
        public void PromptWithAccounts(Dictionary<string, Account> accounts, WindowSize sizeDefaults)
        {
            _partyForm.ClearFields();
            _partyForm.SetDefaultWindowSize(sizeDefaults ?? WindowSize.Default);
            _partyForm.SetEditMode(false);
            _partyForm.PopulateAccountsWithCharacters(accounts);
            SwitchPanels(PanelMode.New);
        }

        /// <summary>
        /// Populates the party form for editting a party with Account objects
        /// </summary>
        /// <param name="accounts">Account objects eligible to be added to the party</param>
        /// <param name="info">The current party info</param>
        public void PromptWithAccounts(Dictionary<string, Account> accounts, Party info)
        {
            _partyForm.ClearFields();
            _partyForm.SetDefaultWindowSize(WindowSize.Default);
            _partyForm.PopulateFields(info);
            _partyForm.SetEditMode(true);
            _partyForm.PopulateAccountsWithCharacters(accounts);
            SwitchPanels(PanelMode.Edit);
        }

        /// <summary>
        /// Populates the party form for editing with Account objects and option defaults.
        /// </summary>
        /// <param name="accounts">Account objects eligible to be added to the party</param>
        /// <param name="info">The current party info</param>
        /// <param name="sizeDefaults">Current option defaults</param>
        public void PromptWithAccounts(Dictionary<string, Account> accounts, Party info, WindowSize sizeDefaults)
        {
            _partyForm.ClearFields();
            _partyForm.SetDefaultWindowSize(sizeDefaults ?? WindowSize.Default);
            _partyForm.PopulateFields(info);
            _partyForm.SetEditMode(true);
            _partyForm.PopulateAccountsWithCharacters(accounts);
            SwitchPanels(PanelMode.Edit);
        }

        /// <summary>
        /// Switch to a different screen within the party display
        /// </summary>
        /// <param name="newMode">New screen to display</param>
        public void SwitchPanels(PanelMode newMode)
        {
            _addPartyButton.Visibility = Visibility.Hidden;
            _partyStackPanel.Visibility = Visibility.Hidden;
            _partyDisplayScrollViewer.Visibility = Visibility.Hidden;
            _partyForm.Visibility = Visibility.Hidden;
            _partyMode = newMode;
            switch (newMode)
            {
                case PanelMode.New:
                    _partyForm.Visibility = Visibility.Visible;
                    _partyForm.FocusPrimary();
                    break;
                case PanelMode.Edit:
                    _partyForm.Visibility = Visibility.Visible;
                    break;
                case PanelMode.Login:

                    break;
                case PanelMode.Select:
                default:
                    _addPartyButton.Visibility = Visibility.Visible;
                    _partyStackPanel.Visibility = Visibility.Visible;
                    _partyDisplayScrollViewer.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Event handler for clicking the New Party button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void NewPartyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && NewRequest != null)
            {
                NewRequest(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event handler for clicking either of the login party buttons
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyDisplay_LoginRequest(object sender, LoginEventArgs e)
        {
            if (sender != null && LoginRequest != null)
            {
                LoginRequest(sender, e);
            }
        }

        /// <summary>
        /// Event handler for clicking the edit button for a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyDisplay_EditRequest(object sender, ListEventArgs e)
        {
            if (sender != null && EditRequest != null)
            {
                EditRequest(sender, e);
            }
        }

        /// <summary>
        /// Event handler for clicking one of the move buttons for a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyDisplay_MoveRequest(object sender, MoveListItemEventArgs e)
        {
            if (sender != null && MoveRequest != null)
            {
                MoveRequest(sender, e);
            }
        }

        /// <summary>
        /// Event handler for clicking the delete button for a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyDisplay_DeleteRequest(object sender, ListEventArgs e)
        {
            if (sender != null && DeleteRequest != null)
            {
                DeleteRequest(sender, e);
            }
        }

        /// <summary>
        /// Event handler for clicking the cancel button on the party form
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyForm_Cancel(object sender, EventArgs e)
        {
            SwitchPanels(PanelMode.Select);
        }

        /// <summary>
        /// Event handler for clicking the delete button on the party form
        /// </summary>
        private void PartyForm_DeleteParty(object sender, ListEventArgs e)
        {
            if (sender != null && DeleteRequest != null)
            {
                DeleteRequest(sender, e);
                SwitchPanels(PanelMode.Select);
            }
        }

        /// <summary>
        /// Event handler for clicking the save button on the party form
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void PartyForm_SaveParty(object sender, DataEventArgs<Party> e)
        {
            EventHandler<DataEventArgs<Party>> actionHandler = _partyMode == PanelMode.New
                ? AddRequest
                : UpdateRequest;
            if (sender != null && actionHandler != null)
            {
                actionHandler(sender, e);
                SwitchPanels(PanelMode.Select);
            }
        }

        /// <summary>
        /// Captures header widths after user drags a splitter, then reapplies to all rows.
        /// </summary>
        private void HeaderColumnSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            SyncColumnWidthsFromHeader();
            ApplyColumnWidthsToRows();
        }

        /// <summary>
        /// Initializes minimum and default widths based on header titles.
        /// </summary>
        private void InitializeColumnWidths()
        {
            _minColumnWidths[0] = MeasureTextWidth(_headerTitle1.Text, _headerTitle1) + 16;
            _minColumnWidths[1] = MeasureTextWidth(_headerTitle2.Text, _headerTitle2) + 16;
            _minColumnWidths[2] = MeasureTextWidth(_headerTitle3.Text, _headerTitle3) + 16;
            _minColumnWidths[3] = MeasureTextWidth(_headerTitle4.Text, _headerTitle4) + 20;
            _minColumnWidths[4] = MeasureTextWidth(_headerTitle5.Text, _headerTitle5) + 20;
            _minColumnWidths[5] = 0; // No AruaGuard button in party rows
            _minColumnWidths[6] = MeasureTextWidth(_headerTitle7.Text, _headerTitle7) + 20;
            _minColumnWidths[7] = 0;

            for (int i = 0; i < 8; i++)
            {
                _columnWidths[i] = Math.Max(_columnWidths[i], _minColumnWidths[i]);
            }

            ApplyColumnWidthsToHeader();
            ApplyColumnWidthsToRows();
        }

        /// <summary>
        /// Grows tracked column widths so content stays visible up to current max populated text.
        /// </summary>
        /// <param name="party">Party to measure against current maxima</param>
        private void GrowColumnsForParty(Party party)
        {
            if (party == null)
            {
                return;
            }

            string description = party.Description ?? string.Empty;

            _columnWidths[0] = Math.Max(_columnWidths[0], MeasureTextWidth(party.Name ?? string.Empty, _headerTitle1) + 16);
            _columnWidths[1] = Math.Max(_columnWidths[1], MeasureTextWidth(description, _headerTitle2) + 16);
            _columnWidths[2] = Math.Max(_columnWidths[2], MeasurePartyAccountsColumnWidth(party));

            ApplyColumnWidthsToHeader();
            ApplyColumnWidthsToRows();
        }

        /// <summary>
        /// Applies tracked widths to header columns.
        /// </summary>
        private void ApplyColumnWidthsToHeader()
        {
            _headerCol1.Width = new GridLength(_columnWidths[0], GridUnitType.Pixel);
            _headerCol2.Width = new GridLength(_columnWidths[1], GridUnitType.Pixel);
            _headerCol3.Width = new GridLength(_columnWidths[2], GridUnitType.Pixel);
            _headerCol4.Width = new GridLength(_columnWidths[3], GridUnitType.Pixel);
            _headerCol5.Width = new GridLength(_columnWidths[4], GridUnitType.Pixel);
            _headerCol6.Width = new GridLength(_columnWidths[5], GridUnitType.Pixel);
            _headerCol7.Width = new GridLength(_columnWidths[6], GridUnitType.Pixel);
            _headerCol8.Width = new GridLength(_columnWidths[7], GridUnitType.Pixel);
        }

        /// <summary>
        /// Applies tracked widths to all row controls.
        /// </summary>
        private void ApplyColumnWidthsToRows()
        {
            foreach (object child in _partyStackPanel.Children)
            {
                ListDisplay row = child as ListDisplay;
                if (row != null)
                {
                    row.SetColumnWidths(_columnWidths);
                }
            }
        }

        /// <summary>
        /// Copies current header widths into tracked values, respecting minimum sizes.
        /// </summary>
        private void SyncColumnWidthsFromHeader()
        {
            double[] current = new double[]
            {
                _headerCol1.ActualWidth,
                _headerCol2.ActualWidth,
                _headerCol3.ActualWidth,
                _headerCol4.ActualWidth,
                _headerCol5.ActualWidth,
                _headerCol6.ActualWidth,
                _headerCol7.ActualWidth,
                _headerCol8.ActualWidth
            };

            for (int i = 0; i < 8; i++)
            {
                _columnWidths[i] = Math.Max(current[i], _minColumnWidths[i]);
            }

            ApplyColumnWidthsToHeader();
        }

        /// <summary>
        /// Measures a string in pixels for a given text element's typography.
        /// </summary>
        private double MeasureTextWidth(string text, TextBlock sample)
        {
            string safeText = string.IsNullOrEmpty(text) ? " " : text;
            Typeface typeface = new Typeface(sample.FontFamily, sample.FontStyle, sample.FontWeight, sample.FontStretch);
            FormattedText formatted = new FormattedText(
                safeText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                sample.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip
            );
            return formatted.WidthIncludingTrailingWhitespace;
        }

        /// <summary>
        /// Measures the Accounts column by the widest visible member line.
        /// </summary>
        private double MeasurePartyAccountsColumnWidth(Party party)
        {
            double widest = _minColumnWidths[2];

            if (party == null || party.Accounts == null)
            {
                return widest;
            }

            foreach (string member in party.Accounts)
            {
                double current = MeasureTextWidth(GetPartyMemberDisplayText(member), _headerTitle3) + 16;
                if (current > widest)
                {
                    widest = current;
                }
            }

            return widest;
        }

        /// <summary>
        /// Formats party member text to match the visible list row text.
        /// </summary>
        private string GetPartyMemberDisplayText(string member)
        {
            if (string.IsNullOrWhiteSpace(member))
            {
                return string.Empty;
            }

            if (member.Contains("|"))
            {
                string[] parts = member.Split('|');
                string characterName = parts[0];
                int location = 1;
                int width = 1024;
                int height = 768;
                bool isFullscreen = false;
                int posX = 0;
                int posY = 0;
                int monitor = 0;

                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedLocation))
                {
                    location = parsedLocation;
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
                string positionText = isFullscreen ? string.Empty : $" | Pos {posX},{posY}";
                string monitorText = $" | Monitor {monitor}";

                return $"{characterName} [Location {location} | {modeText}{positionText}{monitorText}]";
            }

            return $"{member} [Location 1 | 1024x768 | Pos 0,0 | Monitor 0]";
        }

    }
}
