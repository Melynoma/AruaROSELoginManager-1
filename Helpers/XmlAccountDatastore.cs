//
// FILE     : XmlAccountDatastore.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2017-05-23
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using AruaRoseLoginManager.Data;

namespace AruaRoseLoginManager.DAL
{
    /// <summary>
    /// Stores/Loads account information with an XML file
    /// </summary>
    public class XmlAccountDatastore : IAccountDatastore
    {
        /// <summary>
        /// The save files name
        /// </summary>
        private const string FILE_NAME = "AccountManager.xml";

        /// <summary>
        /// The parent element name
        /// </summary>
        private const string MANAGER_ELEMENT = "AccountManager";

        /// <summary>
        /// The account element name
        /// </summary>
        private const string ACCOUNT_ELEMENT = "Account";

        /// <summary>
        /// The character element name
        /// </summary>
        private const string CHARACTER_ELEMENT = "Character";

        /// <summary>
        /// The party element name
        /// </summary>
        private const string PARTY_ELEMENT = "Party";

        /// <summary>
        /// The member element name (account that is part of a party)
        /// </summary>
        private const string MEMBER_ELEMENT = "Member";

        /// <summary>
        /// The parent elements attribute to hold the ROSE folder path
        /// </summary>
        private const string FOLDER_ATTRIBUTE = "roseFolder";

        /// <summary>
        /// The parent elements attribute to hold the second ROSE folder path
        /// </summary>
        private const string FOLDER_PATH2_ATTRIBUTE = "roseFolder2";

        /// <summary>
        /// The parent elements attribute to hold the third ROSE folder path
        /// </summary>
        private const string FOLDER_PATH3_ATTRIBUTE = "roseFolder3";

        /// <summary>
        /// The account elements attribute to hold the install location index
        /// </summary>
        private const string INSTALL_LOCATION_ATTRIBUTE = "installLocation";

        /// <summary>
        /// The parent elements attribute to hold the run as admin flag
        /// </summary>
        private const string RUN_AS_ADMIN_ATTRIBUTE = "runAsAdmin";

        /// <summary>
        /// The parent elements attribute to hold the stream mode flag
        /// </summary>
        private const string STREAM_MODE_ATTRIBUTE = "streamMode";

        /// <summary>
        /// The parent elements attribute to hold the window height
        /// </summary>
        private const string WINDOW_HEIGHT_ATTRIBUTE = "windowHeight";

        /// <summary>
        /// The parent elements attribute to hold window width
        /// </summary>
        private const string WINDOW_WIDTH_ATTRIBUTE = "windowWidth";

        /// <summary>
        /// The parent elements attribute to hold default game width
        /// </summary>
        private const string GAME_WIDTH_ATTRIBUTE = "gameWidth";

        /// <summary>
        /// The parent elements attribute to hold default game height
        /// </summary>
        private const string GAME_HEIGHT_ATTRIBUTE = "gameHeight";

        /// <summary>
        /// The parent elements attribute to hold default fullscreen setting
        /// </summary>
        private const string FULLSCREEN_ATTRIBUTE = "isFullscreen";


        /// <summary>
        /// The parent elements attribute to hold install location 1 game width
        /// </summary>
        private const string GAME_WIDTH1_ATTRIBUTE = "gameWidth1";

        /// <summary>
        /// The parent elements attribute to hold install location 1 game height
        /// </summary>
        private const string GAME_HEIGHT1_ATTRIBUTE = "gameHeight1";

        /// <summary>
        /// The parent elements attribute to hold install location 1 fullscreen setting
        /// </summary>
        private const string FULLSCREEN1_ATTRIBUTE = "isFullscreen1";


        /// <summary>
        /// The parent elements attribute to hold install location 2 game width
        /// </summary>
        private const string GAME_WIDTH2_ATTRIBUTE = "gameWidth2";

        /// <summary>
        /// The parent elements attribute to hold install location 2 game height
        /// </summary>
        private const string GAME_HEIGHT2_ATTRIBUTE = "gameHeight2";

        /// <summary>
        /// The parent elements attribute to hold install location 2 fullscreen setting
        /// </summary>
        private const string FULLSCREEN2_ATTRIBUTE = "isFullscreen2";


        /// <summary>
        /// The parent elements attribute to hold install location 3 game width
        /// </summary>
        private const string GAME_WIDTH3_ATTRIBUTE = "gameWidth3";

        /// <summary>
        /// The parent elements attribute to hold install location 3 game height
        /// </summary>
        private const string GAME_HEIGHT3_ATTRIBUTE = "gameHeight3";

        /// <summary>
        /// The parent elements attribute to hold install location 3 fullscreen setting
        /// </summary>
        private const string FULLSCREEN3_ATTRIBUTE = "isFullscreen3";


        private const string ACCOUNT_COL1_ATTRIBUTE = "accountCol1";
        private const string ACCOUNT_COL2_ATTRIBUTE = "accountCol2";
        private const string ACCOUNT_COL3_ATTRIBUTE = "accountCol3";
        private const string ACCOUNT_COL4_ATTRIBUTE = "accountCol4";
        private const string ACCOUNT_COL5_ATTRIBUTE = "accountCol5";
        private const string ACCOUNT_COL6_ATTRIBUTE = "accountCol6";
        private const string ACCOUNT_COL7_ATTRIBUTE = "accountCol7";
        private const string ACCOUNT_COL8_ATTRIBUTE = "accountCol8";

        private const string PARTY_COL1_ATTRIBUTE = "partyCol1";
        private const string PARTY_COL2_ATTRIBUTE = "partyCol2";
        private const string PARTY_COL3_ATTRIBUTE = "partyCol3";
        private const string PARTY_COL4_ATTRIBUTE = "partyCol4";
        private const string PARTY_COL5_ATTRIBUTE = "partyCol5";
        private const string PARTY_COL6_ATTRIBUTE = "partyCol6";
        private const string PARTY_COL7_ATTRIBUTE = "partyCol7";
        private const string PARTY_COL8_ATTRIBUTE = "partyCol8";

        /// <summary>
        /// The account elements attribute to hold the username
        /// </summary>
        private const string USERNAME_ATTRIBUTE = "username";

        /// <summary>
        /// The account elements attribute to hold the password
        /// </summary>
        private const string PASSWORD_ATTRIBUTE = "password";

        /// <summary>
        /// The account elements attribute to hold the description
        /// </summary>
        private const string DESCRIPTION_ATTRIBUTE = "description";

        /// <summary>
        /// The party element's attribute to hold the name.
        /// </summary>
        private const string NAME_ATTRIBUTE = "name";

        /// <summary>
        /// The document object of the save file
        /// </summary>
        private XDocument _saveFile;

        /// <summary>
        /// The path to the folder to save in
        /// </summary>
        private string _folderPath;

        /// <summary>
        /// The complete path to the save file itself
        /// </summary>
        private string _completeFilePath;

        /// <summary>
        /// Constructor
        /// </summary>
        public XmlAccountDatastore()
        {
            //Get the app data folder
            _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                + @"\AruaROSELoginManager\";

            //Make sure the app data folder exists from the installer
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            _completeFilePath = _folderPath + FILE_NAME;
        }

        /// <summary>
        /// Loads all the account from the XML document
        /// </summary>
        /// <returns>A list of Account's loaded</returns>
        public List<Account> GetAllAccounts()
        {
            List<Account> loadedAccounts = new List<Account>();

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);
                XElement root = _saveFile.Element(MANAGER_ELEMENT);

                // Check if root is null to prevent crashes
                if (root == null)
                {
                    return loadedAccounts;
                }

                //Loop through the Account elements
                foreach (XElement accountElement in root.Elements(ACCOUNT_ELEMENT))
                {
                    string currentUsername = (string)accountElement.Attribute(USERNAME_ATTRIBUTE);
                    string currentPassword = (string)accountElement.Attribute(PASSWORD_ATTRIBUTE);
                    string description = (string)accountElement.Attribute(DESCRIPTION_ATTRIBUTE);
                    
                    List<Character> characters = new List<Character>();
                    foreach (XElement characterElement in accountElement.Elements(CHARACTER_ELEMENT))
                    {
                        string characterName = characterElement.Value;
                        string installLocationValue = (string)characterElement.Attribute(INSTALL_LOCATION_ATTRIBUTE);
                        int installLocation = 1;
                        if (!string.IsNullOrWhiteSpace(installLocationValue) && int.TryParse(installLocationValue, out installLocation))
                        {
                            if (installLocation < 1 || installLocation > 3)
                            {
                                installLocation = 1;
                            }
                        }
                        else
                        {
                            installLocation = 1;
                        }
                        characters.Add(new Character(characterName, installLocation));
                    }
                    
                    Account current = new Account(currentUsername, currentPassword, description, characters);
                    loadedAccounts.Add(current);
                }
            }            

            return loadedAccounts;
        }

        /// <summary>
        /// Loads all the parties from the XML document
        /// </summary>
        /// <returns>The list of parties</returns>
        public List<Party> GetAllParties()
        {
            List<Party> loadedParties = new List<Party>();

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);
                XElement root = _saveFile.Element(MANAGER_ELEMENT);

                //Loop through the Account elements
                foreach (XElement partyElement in root.Elements(PARTY_ELEMENT))
                {
                    string currentPartyName = (string)partyElement.Attribute(NAME_ATTRIBUTE);
                    string description = (string)partyElement.Attribute(DESCRIPTION_ATTRIBUTE);
                    List<string> accounts = new List<string>();
                    foreach (XElement accountElement in partyElement.Elements(MEMBER_ELEMENT))
                    {
                        accounts.Add(accountElement.Value);
                    }
                    Party current = new Party(currentPartyName, accounts, description);
                    loadedParties.Add(current);
                }
            }

            return loadedParties;
        }

        /// <summary>
        /// Gets the file path for the ROSE folder
        /// </summary>
        /// <returns>ROSE folder file path</returns>
        public string GetFilePath()
        {
            string filePath = string.Empty;

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);

                //Loops through the parent element (Should only be one)
                foreach (XElement accountManagerElement in _saveFile.Descendants(MANAGER_ELEMENT))
                {
                    filePath = (string)accountManagerElement.Attribute(FOLDER_ATTRIBUTE);
                }
            }            

            return filePath;
        }

        /// <summary>
        /// Gets the file path for the second ROSE install location
        /// </summary>
        /// <returns>ROSE folder file path</returns>
        public string GetFilePath2()
        {
            string filePath = string.Empty;

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);

                foreach (XElement accountManagerElement in _saveFile.Descendants(MANAGER_ELEMENT))
                {
                    filePath = (string)accountManagerElement.Attribute(FOLDER_PATH2_ATTRIBUTE);
                }
            }

            return filePath;
        }

        /// <summary>
        /// Gets the file path for the third ROSE install location
        /// </summary>
        /// <returns>ROSE folder file path</returns>
        public string GetFilePath3()
        {
            string filePath = string.Empty;

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);

                foreach (XElement accountManagerElement in _saveFile.Descendants(MANAGER_ELEMENT))
                {
                    filePath = (string)accountManagerElement.Attribute(FOLDER_PATH3_ATTRIBUTE);
                }
            }

            return filePath;
        }

        /// <summary>
        /// Gets the status for running as admin
        /// </summary>
        /// <returns>True/False whether to run as admin</returns>
        public bool GetRunAsAdmin()
        {
            bool runAsAdmin = false;

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);

                //Loops through the parent element (Should only be one)
                foreach (XElement accountManagerElement in _saveFile.Descendants(MANAGER_ELEMENT))
                {
                    runAsAdmin = (bool)accountManagerElement.Attribute(RUN_AS_ADMIN_ATTRIBUTE);
                }
            }

            return runAsAdmin;
        }

        /// <summary>
        /// Loads the window size from the XML document
        /// </summary>
        /// <returns>The window size with width and height</returns>
        public WindowSize GetWindowSize()
        {
            WindowSize windowSize = WindowSize.Default;

            if (File.Exists(_completeFilePath))
            {
                _saveFile = XDocument.Load(_completeFilePath);
                XElement accountManagerElement =_saveFile.Descendants(MANAGER_ELEMENT).FirstOrDefault();
                if (
                    accountManagerElement != null
                    && accountManagerElement.Attribute(WINDOW_WIDTH_ATTRIBUTE) != null
                    && accountManagerElement.Attribute(WINDOW_HEIGHT_ATTRIBUTE) != null
                    && int.TryParse(accountManagerElement.Attribute(WINDOW_WIDTH_ATTRIBUTE).Value, out int width)
                    && int.TryParse(accountManagerElement.Attribute(WINDOW_HEIGHT_ATTRIBUTE).Value, out int height)
                )
                {
                    windowSize = new WindowSize() { Height = height, Width = width };
                }

                // Load game resolution settings if available
                if (accountManagerElement != null)
                {
                    // Load legacy game width/height/fullscreen (for backwards compatibility)
                    if (accountManagerElement.Attribute(GAME_WIDTH_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_WIDTH_ATTRIBUTE).Value, out int gameWidth))
                    {
                        windowSize.GameWidth = gameWidth;
                    }

                    if (accountManagerElement.Attribute(GAME_HEIGHT_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_HEIGHT_ATTRIBUTE).Value, out int gameHeight))
                    {
                        windowSize.GameHeight = gameHeight;
                    }

                    if (accountManagerElement.Attribute(FULLSCREEN_ATTRIBUTE) != null
                        && bool.TryParse(accountManagerElement.Attribute(FULLSCREEN_ATTRIBUTE).Value, out bool isFullscreen))
                    {
                        windowSize.IsFullscreen = isFullscreen;
                    }

                    // Load install location 1 game resolution
                    if (accountManagerElement.Attribute(GAME_WIDTH1_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_WIDTH1_ATTRIBUTE).Value, out int gameWidth1))
                    {
                        windowSize.GameWidth1 = gameWidth1;
                    }

                    if (accountManagerElement.Attribute(GAME_HEIGHT1_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_HEIGHT1_ATTRIBUTE).Value, out int gameHeight1))
                    {
                        windowSize.GameHeight1 = gameHeight1;
                    }

                    if (accountManagerElement.Attribute(FULLSCREEN1_ATTRIBUTE) != null
                        && bool.TryParse(accountManagerElement.Attribute(FULLSCREEN1_ATTRIBUTE).Value, out bool isFullscreen1))
                    {
                        windowSize.IsFullscreen1 = isFullscreen1;
                    }

                    // Load install location 2 game resolution
                    if (accountManagerElement.Attribute(GAME_WIDTH2_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_WIDTH2_ATTRIBUTE).Value, out int gameWidth2))
                    {
                        windowSize.GameWidth2 = gameWidth2;
                    }

                    if (accountManagerElement.Attribute(GAME_HEIGHT2_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_HEIGHT2_ATTRIBUTE).Value, out int gameHeight2))
                    {
                        windowSize.GameHeight2 = gameHeight2;
                    }

                    if (accountManagerElement.Attribute(FULLSCREEN2_ATTRIBUTE) != null
                        && bool.TryParse(accountManagerElement.Attribute(FULLSCREEN2_ATTRIBUTE).Value, out bool isFullscreen2))
                    {
                        windowSize.IsFullscreen2 = isFullscreen2;
                    }

                    // Load install location 3 game resolution
                    if (accountManagerElement.Attribute(GAME_WIDTH3_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_WIDTH3_ATTRIBUTE).Value, out int gameWidth3))
                    {
                        windowSize.GameWidth3 = gameWidth3;
                    }

                    if (accountManagerElement.Attribute(GAME_HEIGHT3_ATTRIBUTE) != null
                        && int.TryParse(accountManagerElement.Attribute(GAME_HEIGHT3_ATTRIBUTE).Value, out int gameHeight3))
                    {
                        windowSize.GameHeight3 = gameHeight3;
                    }

                    if (accountManagerElement.Attribute(FULLSCREEN3_ATTRIBUTE) != null
                        && bool.TryParse(accountManagerElement.Attribute(FULLSCREEN3_ATTRIBUTE).Value, out bool isFullscreen3))
                    {
                        windowSize.IsFullscreen3 = isFullscreen3;
                    }

                    // Load stream mode setting
                    if (accountManagerElement.Attribute(STREAM_MODE_ATTRIBUTE) != null
                        && bool.TryParse(accountManagerElement.Attribute(STREAM_MODE_ATTRIBUTE).Value, out bool streamMode))
                    {
                        windowSize.StreamMode = streamMode;
                    }

                    if (accountManagerElement.Attribute(ACCOUNT_COL1_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL1_ATTRIBUTE).Value, out double accountCol1))
                    {
                        windowSize.AccountColumn1 = accountCol1;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL2_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL2_ATTRIBUTE).Value, out double accountCol2))
                    {
                        windowSize.AccountColumn2 = accountCol2;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL3_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL3_ATTRIBUTE).Value, out double accountCol3))
                    {
                        windowSize.AccountColumn3 = accountCol3;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL4_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL4_ATTRIBUTE).Value, out double accountCol4))
                    {
                        windowSize.AccountColumn4 = accountCol4;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL5_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL5_ATTRIBUTE).Value, out double accountCol5))
                    {
                        windowSize.AccountColumn5 = accountCol5;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL6_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL6_ATTRIBUTE).Value, out double accountCol6))
                    {
                        windowSize.AccountColumn6 = accountCol6;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL7_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL7_ATTRIBUTE).Value, out double accountCol7))
                    {
                        windowSize.AccountColumn7 = accountCol7;
                    }
                    if (accountManagerElement.Attribute(ACCOUNT_COL8_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(ACCOUNT_COL8_ATTRIBUTE).Value, out double accountCol8))
                    {
                        windowSize.AccountColumn8 = accountCol8;
                    }

                    if (accountManagerElement.Attribute(PARTY_COL1_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL1_ATTRIBUTE).Value, out double partyCol1))
                    {
                        windowSize.PartyColumn1 = partyCol1;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL2_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL2_ATTRIBUTE).Value, out double partyCol2))
                    {
                        windowSize.PartyColumn2 = partyCol2;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL3_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL3_ATTRIBUTE).Value, out double partyCol3))
                    {
                        windowSize.PartyColumn3 = partyCol3;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL4_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL4_ATTRIBUTE).Value, out double partyCol4))
                    {
                        windowSize.PartyColumn4 = partyCol4;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL5_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL5_ATTRIBUTE).Value, out double partyCol5))
                    {
                        windowSize.PartyColumn5 = partyCol5;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL6_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL6_ATTRIBUTE).Value, out double partyCol6))
                    {
                        windowSize.PartyColumn6 = partyCol6;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL7_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL7_ATTRIBUTE).Value, out double partyCol7))
                    {
                        windowSize.PartyColumn7 = partyCol7;
                    }
                    if (accountManagerElement.Attribute(PARTY_COL8_ATTRIBUTE) != null
                        && double.TryParse(accountManagerElement.Attribute(PARTY_COL8_ATTRIBUTE).Value, out double partyCol8))
                    {
                        windowSize.PartyColumn8 = partyCol8;
                    }
                }
            }

            return windowSize;
        }

        /// <summary>
        /// Saves all the login manager data to the XML file
        /// </summary>
        /// <param name="filePath1">The path to the ROSE folder for location 1</param>
        /// <param name="filePath2">The path to the ROSE folder for location 2</param>
        /// <param name="filePath3">The path to the ROSE folder for location 3</param>
        /// <param name="allAccounts">The list of accounts to save</param>
        public void SaveManagerData(string filePath1, string filePath2, string filePath3, bool runAdAdmin, WindowSize size, List<Account> allAccounts, List<Party> allParties)
        {
            _saveFile = new XDocument();

            //Create the manager element with the file path
            XElement managerElement = new XElement(MANAGER_ELEMENT);
            managerElement.Add(new XAttribute(FOLDER_ATTRIBUTE, filePath1));
            managerElement.Add(new XAttribute(FOLDER_PATH2_ATTRIBUTE, filePath2));
            managerElement.Add(new XAttribute(FOLDER_PATH3_ATTRIBUTE, filePath3));
            managerElement.Add(new XAttribute(RUN_AS_ADMIN_ATTRIBUTE, runAdAdmin));
            managerElement.Add(new XAttribute(STREAM_MODE_ATTRIBUTE, size.StreamMode));
            managerElement.Add(new XAttribute(WINDOW_HEIGHT_ATTRIBUTE, size.Height));
            managerElement.Add(new XAttribute(WINDOW_WIDTH_ATTRIBUTE, size.Width));
            
            // Save legacy game resolution attributes for backwards compatibility
            managerElement.Add(new XAttribute(GAME_WIDTH_ATTRIBUTE, size.GameWidth));
            managerElement.Add(new XAttribute(GAME_HEIGHT_ATTRIBUTE, size.GameHeight));
            managerElement.Add(new XAttribute(FULLSCREEN_ATTRIBUTE, size.IsFullscreen));
            
            // Save install location specific game resolutions
            managerElement.Add(new XAttribute(GAME_WIDTH1_ATTRIBUTE, size.GameWidth1));
            managerElement.Add(new XAttribute(GAME_HEIGHT1_ATTRIBUTE, size.GameHeight1));
            managerElement.Add(new XAttribute(FULLSCREEN1_ATTRIBUTE, size.IsFullscreen1));
            
            managerElement.Add(new XAttribute(GAME_WIDTH2_ATTRIBUTE, size.GameWidth2));
            managerElement.Add(new XAttribute(GAME_HEIGHT2_ATTRIBUTE, size.GameHeight2));
            managerElement.Add(new XAttribute(FULLSCREEN2_ATTRIBUTE, size.IsFullscreen2));
            
            managerElement.Add(new XAttribute(GAME_WIDTH3_ATTRIBUTE, size.GameWidth3));
            managerElement.Add(new XAttribute(GAME_HEIGHT3_ATTRIBUTE, size.GameHeight3));
            managerElement.Add(new XAttribute(FULLSCREEN3_ATTRIBUTE, size.IsFullscreen3));
            managerElement.Add(new XAttribute(ACCOUNT_COL1_ATTRIBUTE, size.AccountColumn1));
            managerElement.Add(new XAttribute(ACCOUNT_COL2_ATTRIBUTE, size.AccountColumn2));
            managerElement.Add(new XAttribute(ACCOUNT_COL3_ATTRIBUTE, size.AccountColumn3));
            managerElement.Add(new XAttribute(ACCOUNT_COL4_ATTRIBUTE, size.AccountColumn4));
            managerElement.Add(new XAttribute(ACCOUNT_COL5_ATTRIBUTE, size.AccountColumn5));
            managerElement.Add(new XAttribute(ACCOUNT_COL6_ATTRIBUTE, size.AccountColumn6));
            managerElement.Add(new XAttribute(ACCOUNT_COL7_ATTRIBUTE, size.AccountColumn7));
            managerElement.Add(new XAttribute(ACCOUNT_COL8_ATTRIBUTE, size.AccountColumn8));
            managerElement.Add(new XAttribute(PARTY_COL1_ATTRIBUTE, size.PartyColumn1));
            managerElement.Add(new XAttribute(PARTY_COL2_ATTRIBUTE, size.PartyColumn2));
            managerElement.Add(new XAttribute(PARTY_COL3_ATTRIBUTE, size.PartyColumn3));
            managerElement.Add(new XAttribute(PARTY_COL4_ATTRIBUTE, size.PartyColumn4));
            managerElement.Add(new XAttribute(PARTY_COL5_ATTRIBUTE, size.PartyColumn5));
            managerElement.Add(new XAttribute(PARTY_COL6_ATTRIBUTE, size.PartyColumn6));
            managerElement.Add(new XAttribute(PARTY_COL7_ATTRIBUTE, size.PartyColumn7));
            managerElement.Add(new XAttribute(PARTY_COL8_ATTRIBUTE, size.PartyColumn8));

            //Create the account elements
            foreach (Account account in allAccounts)
            {
                XElement accountElement = new XElement(ACCOUNT_ELEMENT);
                accountElement.Add(new XAttribute(USERNAME_ATTRIBUTE, account.Username));
                accountElement.Add(new XAttribute(PASSWORD_ATTRIBUTE, account.PasswordHash));
                if (!string.IsNullOrWhiteSpace(account.Description))
                {
                    accountElement.Add(new XAttribute(DESCRIPTION_ATTRIBUTE, account.Description));
                }
                foreach(Character character in account.Characters)
                {
                    XElement characterElement = new XElement(CHARACTER_ELEMENT, character.Name);
                    characterElement.Add(new XAttribute(INSTALL_LOCATION_ATTRIBUTE, character.InstallLocation));
                    accountElement.Add(characterElement);
                }

                managerElement.Add(accountElement);
            }

            // Create the party elements
            foreach(Party party in allParties)
            {
                XElement partyElement = new XElement(PARTY_ELEMENT);
                partyElement.Add(new XAttribute(NAME_ATTRIBUTE, party.Name));
                if (!string.IsNullOrWhiteSpace(party.Description))
                {
                    partyElement.Add(new XAttribute(DESCRIPTION_ATTRIBUTE, party.Description));
                }
                foreach (string accountName in party.Accounts)
                {
                    partyElement.Add(new XElement(MEMBER_ELEMENT, accountName));
                }

                managerElement.Add(partyElement);
            }

            _saveFile.Add(managerElement);
            _saveFile.Save(_completeFilePath);  
        }
    }
}
