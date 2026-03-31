//
// FILE     : AccountManagerController.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2017-09-30
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using AruaRoseLoginManager.DAL;
using AruaRoseLoginManager.Data;
using AruaRoseLoginManager.Enum;
using AruaRoseLoginManager.Helpers;
using AruaRoseLoginManager.Controls;

namespace AruaRoseLoginManager.Controllers
{
    public class AccountManagerController
    {
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// The view panel for the account manager
        /// </summary>
        private IManagerPanel _viewPanel;

        /// <summary>
        /// The account datastore to access the accounts
        /// </summary>
        private IAccountDatastore _datastore;

        /// <summary>
        /// The list of all the accounts
        /// </summary>
        private Dictionary<string, Account> _accountList;

        /// <summary>
        /// The explicit ordering for account usernames.
        /// </summary>
        private List<string> _accountOrder;

        /// <summary>
        /// The list of all the parties
        /// </summary>
        private Dictionary<string, Party> _partyList;

        /// <summary>
        /// The explicit ordering for party names.
        /// </summary>
        private List<string> _partyOrder;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="panel">The view panel to reference</param>
        public AccountManagerController(IManagerPanel panel)
        {
            _viewPanel = panel;
            _datastore = new XmlAccountDatastore();
            _accountList = new Dictionary<string, Account>();
            _accountOrder = new List<string>();
            _partyOrder = new List<string>();
        }

        /// <summary>
        /// Initializes the event handlers and loads the data
        /// </summary>
        /// <returns>True on success</returns>
        public bool Initialize()
        {
            //Subscribe to the events
            _viewPanel.AccountDisplay.LoginRequest += AccountManagerPanel_LoginRequest;
            _viewPanel.AccountDisplay.PromptedLoginRequest += AccountManagerPanel_PromptedLoginRequest;
            _viewPanel.AccountDisplay.AddRequest += AccountManagerPanel_AddAccountRequest;
            _viewPanel.AccountDisplay.EditRequest += AccountManagerPanel_EditAccountRequest;
            _viewPanel.AccountDisplay.UpdateRequest += AccountManagerPanel_UpdateAccountRequest;
            _viewPanel.AccountDisplay.MoveRequest += AccountManagerPanel_MoveAccountRequest;
            _viewPanel.AccountDisplay.DeleteRequest += AccountManagerPanel_DeleteAccountRequest;
            _viewPanel.PartyDisplay.LoginRequest += AccountManagerPanel_LoginPartyRequest;
            _viewPanel.PartyDisplay.NewRequest += AccountManagerPanel_NewPartyRequest;
            _viewPanel.PartyDisplay.AddRequest += AccountManagerPanel_AddPartyRequest;
            _viewPanel.PartyDisplay.EditRequest += AccountManagerPanel_EditPartyRequest;
            _viewPanel.PartyDisplay.UpdateRequest += AccountManagerPanel_UpdatePartyRequest;
            _viewPanel.PartyDisplay.MoveRequest += AccountManagerPanel_MovePartyRequest;
            _viewPanel.PartyDisplay.DeleteRequest += AccountManagerPanel_DeletePartyRequest;

            // Subscribe to save settings request from the options tab
            ManagerPanel managerPanel = _viewPanel as ManagerPanel;
            if (managerPanel != null)
            {
                managerPanel.SaveSettingsRequested += ManagerPanel_SaveSettingsRequested;
            }

            //Load the existing accounts and preserve current file order
            List<Account> loadedAccounts = _datastore.GetAllAccounts();
            _accountList = loadedAccounts.ToDictionary(account => account.Username);
            _accountOrder = loadedAccounts.Select(account => account.Username).ToList();

            List<Party> loadedParties = _datastore.GetAllParties();
            _partyList = loadedParties.ToDictionary(party => party.Name);
            _partyOrder = loadedParties.Select(party => party.Name).ToList();
            RefreshAccountList();
            RefreshPartyList();

            _viewPanel.RoseFolderPath = _datastore.GetFilePath();
            _viewPanel.RoseFolderPath2 = (_datastore as XmlAccountDatastore)?.GetFilePath2() ?? string.Empty;
            _viewPanel.RoseFolderPath3 = (_datastore as XmlAccountDatastore)?.GetFilePath3() ?? string.Empty;
            _viewPanel.RunAsAdmin = _datastore.GetRunAsAdmin();
            _viewPanel.Size = _datastore.GetWindowSize();

            // Load default game resolution settings for all locations
            WindowSize size = _viewPanel.Size;
            _viewPanel.DefaultGameWidth = size.GameWidth1;
            _viewPanel.DefaultGameHeight = size.GameHeight1;
            _viewPanel.IsFullscreen = size.IsFullscreen1;
            
            _viewPanel.DefaultGameWidth2 = size.GameWidth2;
            _viewPanel.DefaultGameHeight2 = size.GameHeight2;
            _viewPanel.IsFullscreen2 = size.IsFullscreen2;
            
            _viewPanel.DefaultGameWidth3 = size.GameWidth3;
            _viewPanel.DefaultGameHeight3 = size.GameHeight3;
            _viewPanel.IsFullscreen3 = size.IsFullscreen3;

            _viewPanel.AccountDisplay.SetColumnWidths(new double[]
            {
                size.AccountColumn1,
                size.AccountColumn2,
                size.AccountColumn3,
                size.AccountColumn4,
                size.AccountColumn5,
                size.AccountColumn6,
                size.AccountColumn7,
                size.AccountColumn8
            });

            _viewPanel.PartyDisplay.SetColumnWidths(new double[]
            {
                size.PartyColumn1,
                size.PartyColumn2,
                size.PartyColumn3,
                size.PartyColumn4,
                size.PartyColumn5,
                size.PartyColumn6,
                size.PartyColumn7,
                size.PartyColumn8
            });

            // Load stream mode setting
            _viewPanel.StreamMode = size.StreamMode;

            return true;
        }

        /// <summary>
        /// Saves the data when closing the application
        /// </summary>
        public void Shutdown()
        {
            WindowSize currentSize = _viewPanel.Size;
            
            // Save game resolutions for all install locations
            currentSize.GameWidth1 = _viewPanel.DefaultGameWidth;
            currentSize.GameHeight1 = _viewPanel.DefaultGameHeight;
            currentSize.IsFullscreen1 = _viewPanel.IsFullscreen;
            
            currentSize.GameWidth2 = _viewPanel.DefaultGameWidth2;
            currentSize.GameHeight2 = _viewPanel.DefaultGameHeight2;
            currentSize.IsFullscreen2 = _viewPanel.IsFullscreen2;
            
            currentSize.GameWidth3 = _viewPanel.DefaultGameWidth3;
            currentSize.GameHeight3 = _viewPanel.DefaultGameHeight3;
            currentSize.IsFullscreen3 = _viewPanel.IsFullscreen3;

            double[] accountWidths = _viewPanel.AccountDisplay.GetColumnWidths();
            currentSize.AccountColumn1 = accountWidths[0];
            currentSize.AccountColumn2 = accountWidths[1];
            currentSize.AccountColumn3 = accountWidths[2];
            currentSize.AccountColumn4 = accountWidths[3];
            currentSize.AccountColumn5 = accountWidths[4];
            currentSize.AccountColumn6 = accountWidths[5];
            currentSize.AccountColumn7 = accountWidths[6];
            currentSize.AccountColumn8 = accountWidths[7];

            double[] partyWidths = _viewPanel.PartyDisplay.GetColumnWidths();
            currentSize.PartyColumn1 = partyWidths[0];
            currentSize.PartyColumn2 = partyWidths[1];
            currentSize.PartyColumn3 = partyWidths[2];
            currentSize.PartyColumn4 = partyWidths[3];
            currentSize.PartyColumn5 = partyWidths[4];
            currentSize.PartyColumn6 = partyWidths[5];
            currentSize.PartyColumn7 = partyWidths[6];
            currentSize.PartyColumn8 = partyWidths[7];

            // Save stream mode setting
            currentSize.StreamMode = _viewPanel.StreamMode;

            _datastore.SaveManagerData(
                _viewPanel.RoseFolderPath,
                _viewPanel.RoseFolderPath2,
                _viewPanel.RoseFolderPath3,
                _viewPanel.RunAsAdmin,
                currentSize,
                _accountOrder.Where(_accountList.ContainsKey).Select(id => _accountList[id]).ToList(),
                _partyOrder.Where(_partyList.ContainsKey).Select(id => _partyList[id]).ToList()
            );
        }

        /// <summary>
        /// Gets the configured ROSE path for a specific install location
        /// </summary>
        /// <param name="installLocation">The install location index (1, 2, or 3)</param>
        /// <returns>The selected ROSE folder path</returns>
        private string GetRoseFolderPathForInstallLocation(int installLocation)
        {
            switch (installLocation)
            {
                case 2:
                    return _viewPanel.RoseFolderPath2;
                case 3:
                    return _viewPanel.RoseFolderPath3;
                default:
                    return _viewPanel.RoseFolderPath;
            }
        }

        /// <summary>
        /// Refreshes the account list
        /// </summary>
        private void RefreshAccountList()
        {
            _viewPanel.AccountDisplay.ClearDisplay();
            foreach (string username in _accountOrder)
            {
                if (_accountList.ContainsKey(username))
                {
                    _viewPanel.AccountDisplay.AddToDisplay(_accountList[username]);
                }
            }
        }

        /// <summary>
        /// Refreshes the party list
        /// </summary>
        private void RefreshPartyList()
        {
            _viewPanel.PartyDisplay.ClearDisplay();
            foreach(string partyName in _partyOrder)
            {
                if (_partyList.ContainsKey(partyName))
                {
                    _viewPanel.PartyDisplay.AddToDisplay(_partyList[partyName]);
                }
            }
        }

        /// <summary>
        /// Opens a ROSE client with the given credentials to log in
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_LoginRequest(object sender, LoginEventArgs e)
        {
            string roseFolderPath = GetRoseFolderPathForInstallLocation(e.CharacterInstallLocation);
            GetDefaultLaunchSettingsForInstallLocation(e.CharacterInstallLocation, out int gameWidth, out int gameHeight, out bool isFullscreen);
            LoginAccount(_accountList[e.Id], e.ServerId, roseFolderPath, _viewPanel.RunAsAdmin, gameWidth, gameHeight, isFullscreen, 0, 0, false, null);
        }

        /// <summary>
        /// Opens a ROSE client with the credentials given when the user enters the password
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_PromptedLoginRequest(object sender, LoginWithPassEventArgs e)
        {
            string passwordHash = MD5Generator.GetMd5Hash(e.Password);
            Account account = new Account(e.Id, passwordHash, "", new List<Character>());
            string roseFolderPath = GetRoseFolderPathForInstallLocation(e.CharacterInstallLocation);
            GetDefaultLaunchSettingsForInstallLocation(e.CharacterInstallLocation, out int gameWidth, out int gameHeight, out bool isFullscreen);
            LoginAccount(account, e.ServerId, roseFolderPath, _viewPanel.RunAsAdmin, gameWidth, gameHeight, isFullscreen, 0, 0, false, null);
        }

        /// <summary>
        /// Checks if the required information is there before starting a thread to open the ROSE client
        /// </summary>
        /// <param name="account">The account information to log in</param>
        /// <param name="serverId">The server to select</param>
        /// <param name="roseFolderPath">Path to the folder containing the rose client</param>
        /// <param name="runAsAdmin">Whether to run as admin or not</param>
        /// <param name="gameWidth">Game window width</param>
        /// <param name="gameHeight">Game window height</param>
        /// <param name="isFullscreen">Whether to run the client in fullscreen</param>
        /// <param name="gamePosX">Game window X position</param>
        /// <param name="gamePosY">Game window Y position</param>
        private void LoginAccount(Account account, Server serverId, string roseFolderPath, bool runAsAdmin, int gameWidth, int gameHeight, bool isFullscreen, int gamePosX, int gamePosY, bool applyWindowPosition, int? targetAdapter)
        {
            string passwordHash = account.PasswordHash;
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                _viewPanel.AccountDisplay.PromptForPassword(account.Username, serverId);
                return;
            }
            //Start a thread for the new login process
            Thread loginThread = new Thread(() => LoginAccountThread(
                account.Username,
                account.PasswordHash,
                serverId,
                roseFolderPath,
                runAsAdmin,
                gameWidth,
                gameHeight,
                isFullscreen,
                gamePosX,
                gamePosY,
                applyWindowPosition,
                targetAdapter
            ));
            loginThread.Start();
        }

        /// <summary>
        /// Gets default launch settings for a given install location from Options.
        /// </summary>
        private void GetDefaultLaunchSettingsForInstallLocation(int installLocation, out int gameWidth, out int gameHeight, out bool isFullscreen)
        {
            switch (installLocation)
            {
                case 2:
                    gameWidth = _viewPanel.DefaultGameWidth2;
                    gameHeight = _viewPanel.DefaultGameHeight2;
                    isFullscreen = _viewPanel.IsFullscreen2;
                    break;
                case 3:
                    gameWidth = _viewPanel.DefaultGameWidth3;
                    gameHeight = _viewPanel.DefaultGameHeight3;
                    isFullscreen = _viewPanel.IsFullscreen3;
                    break;
                default:
                    gameWidth = _viewPanel.DefaultGameWidth;
                    gameHeight = _viewPanel.DefaultGameHeight;
                    isFullscreen = _viewPanel.IsFullscreen;
                    break;
            }
        }

        /// <summary>
        /// Opens the rose client and logs in an account
        /// </summary>
        /// <param name="username">The user name to use</param>
        /// <param name="password">The password hash to use</param>
        /// <param name="roseFilePath">The path to the rose folder</param>
        /// <param name="runAsAdmin">Whether we should run as admin or not</param>
        /// <param name="gameWidth">Game window width</param>
        /// <param name="gameHeight">Game window height</param>
        /// <param name="isFullscreen">Whether to run in fullscreen mode</param>
        /// <param name="gamePosX">Game window X position</param>
        /// <param name="gamePosY">Game window Y position</param>
        private void LoginAccountThread(string username, string password, Server serverId, string roseFilePath, bool runAsAdmin, int gameWidth, int gameHeight, bool isFullscreen, int gamePosX, int gamePosY, bool applyWindowPosition, int? targetAdapter)
        {
            //Start the process for TRose.exe
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = @"TRose.exe"
            };
            if (roseFilePath == string.Empty)
            {
                _viewPanel.ShowMessageBox("Please select your AruaROSE folder in the Options tab.");
                return;
            }
            startInfo.WorkingDirectory = roseFilePath;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.Arguments = "_account " + username + " _passwordMD5 " + password + " _server " + serverId.ToString("D") + " _channel 1";
            startInfo.Arguments += isFullscreen
                ? " _fullscreen 1"
                : " _fullscreen 0 _width " + gameWidth + " _height " + gameHeight;
            if (targetAdapter.HasValue)
            {
                startInfo.Arguments += " -adapter " + targetAdapter.Value;
            }
            if (runAsAdmin)
            {
                startInfo.Verb = "runas";
            }
            process.StartInfo = startInfo;
            try
            {
                process.Start();

                if (!isFullscreen && applyWindowPosition)
                {
                    PositionProcessWindow(process, gamePosX, gamePosY);
                }
            }
            catch (Exception ex)
            {
                _viewPanel.ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// Attempts to move the spawned game window to the configured X/Y position.
        /// Monitors for up to 60 seconds and re-applies whenever the main window handle
        /// changes, covering the login-screen → game-world window replacement that
        /// TRose.exe performs during startup.
        /// </summary>
        private void PositionProcessWindow(System.Diagnostics.Process process, int gamePosX, int gamePosY)
        {
            try
            {
                process.WaitForInputIdle(5000);
            }
            catch
            {
                // Some process states do not support WaitForInputIdle.
            }

            IntPtr lastPositionedHandle = IntPtr.Zero;

            // Poll for 60 seconds at 200 ms intervals (300 ticks).
            // Every time the main window handle changes (indicating a new window was created,
            // e.g. the game world replacing the login screen), reapply the target position.
            for (int i = 0; i < 300; i++)
            {
                try
                {
                    if (process.HasExited)
                        break;

                    process.Refresh();
                    IntPtr handle = process.MainWindowHandle;

                    if (handle != IntPtr.Zero && handle != lastPositionedHandle)
                    {
                        SetWindowPos(handle, IntPtr.Zero, gamePosX, gamePosY, 0, 0,
                            SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                        lastPositionedHandle = handle;
                    }
                }
                catch
                {
                    break;
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Event handler for adding an account
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_AddAccountRequest(object sender, DataEventArgs<Account> e)
        {
            if (sender != null && e != null && e.Data != null)
            {
                if (!string.IsNullOrWhiteSpace(e.Data.Username))
                {
                    if (_accountList.ContainsKey(e.Data.Username))
                    {
                        _viewPanel.ShowMessageBox("An account with this username already exists.");
                        return;
                    }

                    _accountList.Add(e.Data.Username, e.Data);
                    _accountOrder.Add(e.Data.Username);
                    _viewPanel.AccountDisplay.AddToDisplay(e.Data);
                }
            }
        }

        /// <summary>
        /// Event handler for editting an account
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_EditAccountRequest(object sender, ListEventArgs e)
        {
            if (sender != null && _accountList.ContainsKey(e.Id))
            {
                _viewPanel.AccountDisplay.Prompt(_accountList[e.Id]);
            }
        }

        /// <summary>
        /// Event handler for an account
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_UpdateAccountRequest(object sender, DataEventArgs<Account> e)
        {
            if (
                sender != null
                && !string.IsNullOrWhiteSpace(e.Data.Username)
                && _accountList.ContainsKey(e.Data.Username)
            )
            {
                _accountList[e.Data.Username] = e.Data;
                RefreshAccountList();
            }
        }

        /// <summary>
        /// Event handler for moving an account
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_MoveAccountRequest(object sender, MoveListItemEventArgs e)
        {
            int currentIndex = _accountOrder.IndexOf(e.Id);
            if (sender != null && currentIndex != -1)
            {
                int newIndex = e.Direction == MovementDirection.Up
                    ? currentIndex - 1
                    : currentIndex + 1;

                if (newIndex < 0 || newIndex >= _accountOrder.Count)
                {
                    return;
                }

                string accountId = _accountOrder[currentIndex];
                _accountOrder.RemoveAt(currentIndex);
                _accountOrder.Insert(newIndex, accountId);
                RefreshAccountList();
            }
        }

        /// <summary>
        /// Event handler for deleting an account
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_DeleteAccountRequest(object sender, ListEventArgs e)
        {
            if (sender != null)
            {
                _accountList.Remove(e.Id);
                _accountOrder.Remove(e.Id);
                List<Party> affectedParties = _partyList.Values.Where(x => x.Accounts.Contains(e.Id)).ToList();
                foreach (Party party in affectedParties)
                {
                    _partyList.Remove(party.Name);
                    _partyOrder.Remove(party.Name);
                }
                RefreshAccountList();
                RefreshPartyList();
            }
        }

        /// <summary>
        /// Event handler for logging in a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_LoginPartyRequest(object sender, LoginEventArgs e)
        {
            Party party = _partyList[e.Id];
            foreach(string memberInfo in party.Accounts)
            {
                // Parse member info: "CharacterName|InstallLocation|GameWidth|GameHeight|Fullscreen|PosX|PosY|Monitor"
                string[] parts = memberInfo.Split('|');
                if (parts.Length >= 4)
                {
                    string characterName = parts[0];
                    int installLocation = int.TryParse(parts[1], out int loc) ? loc : 1;
                    int gameWidth = int.TryParse(parts[2], out int w) ? w : 1024;
                    int gameHeight = int.TryParse(parts[3], out int h) ? h : 768;
                    bool isFullscreen = parts.Length >= 5 && bool.TryParse(parts[4], out bool fullscreen) && fullscreen;
                    int gamePosX = parts.Length >= 6 && int.TryParse(parts[5], out int x) ? x : 0;
                    int gamePosY = parts.Length >= 7 && int.TryParse(parts[6], out int y) ? y : 0;
                    int targetAdapter = parts.Length >= 8 && int.TryParse(parts[7], out int adapter) ? adapter : 0;

                    // Find the account that contains this character
                    Account account = null;
                    foreach (var acc in _accountList.Values)
                    {
                        if (acc.Characters.Any(c => c.Name == characterName))
                        {
                            account = acc;
                            break;
                        }
                    }

                    if (account != null)
                    {
                        string roseFolderPath = GetRoseFolderPathForInstallLocation(installLocation);
                        LoginAccount(account, e.ServerId, roseFolderPath, _viewPanel.RunAsAdmin, gameWidth, gameHeight, isFullscreen, gamePosX, gamePosY, true, targetAdapter);
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for prompting for a new party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_NewPartyRequest(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Dictionary<string, Account> eligibleAccounts = _accountList
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value.PasswordHash) && x.Value.Characters.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value);
                _viewPanel.PartyDisplay.PromptWithAccounts(eligibleAccounts, _viewPanel.Size);
            }
        }

        /// <summary>
        /// Event handler for adding a new party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_AddPartyRequest(object sender, DataEventArgs<Party> e)
        {
            if (sender != null)
            {
                if (!string.IsNullOrWhiteSpace(e.Data.Name))
                {
                    _partyList.Add(e.Data.Name, e.Data);
                    _partyOrder.Add(e.Data.Name);
                    _viewPanel.PartyDisplay.AddToDisplay(e.Data);
                }
            }
        }

        /// <summary>
        /// Event handler for editing a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_EditPartyRequest(object sender, ListEventArgs e)
        {
            if (sender != null && _partyList.ContainsKey(e.Id))
            {
                Dictionary<string, Account> eligibleAccounts = _accountList
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value.PasswordHash) && x.Value.Characters.Count > 0)
                    .ToDictionary(x => x.Key, x => x.Value);
                _viewPanel.PartyDisplay.PromptWithAccounts(eligibleAccounts, _partyList[e.Id], _viewPanel.Size);
            }
        }

        /// <summary>
        /// Event handler for updating a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_UpdatePartyRequest(object sender, DataEventArgs<Party> e)
        {
            if (
                sender != null
                && !string.IsNullOrWhiteSpace(e.Data.Name)
                && _partyList.ContainsKey(e.Data.Name)
            )
            {
                _partyList[e.Data.Name] = e.Data;
                RefreshPartyList();
            }
        }

        /// <summary>
        /// Event handler for moving a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_MovePartyRequest(object sender, MoveListItemEventArgs e)
        {
            int currentIndex = _partyOrder.IndexOf(e.Id);
            if (sender != null && currentIndex != -1)
            {
                int newIndex = e.Direction == MovementDirection.Up
                    ? currentIndex - 1
                    : currentIndex + 1;

                if (newIndex < 0 || newIndex >= _partyOrder.Count)
                {
                    return;
                }

                string partyId = _partyOrder[currentIndex];
                _partyOrder.RemoveAt(currentIndex);
                _partyOrder.Insert(newIndex, partyId);
                RefreshPartyList();
            }
        }

        /// <summary>
        /// Event handler for deleting a party
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void AccountManagerPanel_DeletePartyRequest(object sender, ListEventArgs e)
        {
            if (sender != null)
            {
                _partyList.Remove(e.Id);
                _partyOrder.Remove(e.Id);
                RefreshPartyList();
            }
        }

        /// <summary>
        /// Event handler for saving settings/options
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void ManagerPanel_SaveSettingsRequested(object sender, EventArgs e)
        {
            WindowSize currentSize = _viewPanel.Size;
            
            // Save game resolutions for all install locations
            currentSize.GameWidth1 = _viewPanel.DefaultGameWidth;
            currentSize.GameHeight1 = _viewPanel.DefaultGameHeight;
            currentSize.IsFullscreen1 = _viewPanel.IsFullscreen;
            
            currentSize.GameWidth2 = _viewPanel.DefaultGameWidth2;
            currentSize.GameHeight2 = _viewPanel.DefaultGameHeight2;
            currentSize.IsFullscreen2 = _viewPanel.IsFullscreen2;
            
            currentSize.GameWidth3 = _viewPanel.DefaultGameWidth3;
            currentSize.GameHeight3 = _viewPanel.DefaultGameHeight3;
            currentSize.IsFullscreen3 = _viewPanel.IsFullscreen3;

            double[] accountWidths = _viewPanel.AccountDisplay.GetColumnWidths();
            currentSize.AccountColumn1 = accountWidths[0];
            currentSize.AccountColumn2 = accountWidths[1];
            currentSize.AccountColumn3 = accountWidths[2];
            currentSize.AccountColumn4 = accountWidths[3];
            currentSize.AccountColumn5 = accountWidths[4];
            currentSize.AccountColumn6 = accountWidths[5];
            currentSize.AccountColumn7 = accountWidths[6];
            currentSize.AccountColumn8 = accountWidths[7];

            double[] partyWidths = _viewPanel.PartyDisplay.GetColumnWidths();
            currentSize.PartyColumn1 = partyWidths[0];
            currentSize.PartyColumn2 = partyWidths[1];
            currentSize.PartyColumn3 = partyWidths[2];
            currentSize.PartyColumn4 = partyWidths[3];
            currentSize.PartyColumn5 = partyWidths[4];
            currentSize.PartyColumn6 = partyWidths[5];
            currentSize.PartyColumn7 = partyWidths[6];
            currentSize.PartyColumn8 = partyWidths[7];

            // Save stream mode setting
            currentSize.StreamMode = _viewPanel.StreamMode;

            _datastore.SaveManagerData(
                _viewPanel.RoseFolderPath,
                _viewPanel.RoseFolderPath2,
                _viewPanel.RoseFolderPath3,
                _viewPanel.RunAsAdmin,
                currentSize,
                _accountOrder.Where(_accountList.ContainsKey).Select(id => _accountList[id]).ToList(),
                _partyOrder.Where(_partyList.ContainsKey).Select(id => _partyList[id]).ToList()
            );
            _viewPanel.ShowMessageBox("Settings saved successfully!");
        }
    }
}
