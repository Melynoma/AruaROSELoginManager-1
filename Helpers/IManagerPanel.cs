//
// FILE     : IManagerPanel.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-19
//

using AruaRoseLoginManager.Data;

namespace AruaRoseLoginManager.Helpers
{
    /// <summary>
    /// An interface for the controller to interact with the manager panel front ends
    /// </summary>
    public interface IManagerPanel
    {
        /// <summary>
        /// Path to the folder that contains the ROSE client
        /// </summary>
        string RoseFolderPath { get; set; }

        /// <summary>
        /// Path to the second install location
        /// </summary>
        string RoseFolderPath2 { get; set; }

        /// <summary>
        /// Path to the third install location
        /// </summary>
        string RoseFolderPath3 { get; set; }

        /// <summary>
        /// Whether to run clients in admin mode
        /// </summary>
        bool RunAsAdmin { get; set; }

        /// <summary>
        /// Whether stream mode is enabled (hides account and location info)
        /// </summary>
        bool StreamMode { get; set; }

        /// <summary>
        /// Default game window width
        /// </summary>
        int DefaultGameWidth { get; set; }

        /// <summary>
        /// Default game window height
        /// </summary>
        int DefaultGameHeight { get; set; }

        /// <summary>
        /// Whether game should default to fullscreen
        /// </summary>
        bool IsFullscreen { get; set; }

        /// <summary>
        /// Default game window width for install location 2
        /// </summary>
        int DefaultGameWidth2 { get; set; }

        /// <summary>
        /// Default game window height for install location 2
        /// </summary>
        int DefaultGameHeight2 { get; set; }

        /// <summary>
        /// Whether game should default to fullscreen for install location 2
        /// </summary>
        bool IsFullscreen2 { get; set; }

        /// <summary>
        /// Default game window width for install location 3
        /// </summary>
        int DefaultGameWidth3 { get; set; }

        /// <summary>
        /// Default game window height for install location 3
        /// </summary>
        int DefaultGameHeight3 { get; set; }

        /// <summary>
        /// Whether game should default to fullscreen for install location 3
        /// </summary>
        bool IsFullscreen3 { get; set; }

        /// <summary>
        /// The current window size
        /// </summary>
        WindowSize Size { get; set; }

        /// <summary>
        /// Account list display
        /// </summary>
        IAccountDisplay AccountDisplay { get; }

        /// <summary>
        /// Party list display
        /// </summary>
        IPartyDisplay PartyDisplay { get; }

        /// <summary>
        /// Displays a message box to the user
        /// </summary>
        /// <param name="message">The message to display</param>
        void ShowMessageBox(string message);

        /// <summary>
        /// Launches Update.exe for a configured install location.
        /// </summary>
        /// <param name="installLocation">Install location index (1, 2, or 3)</param>
        void PatchInstall(int installLocation);
    }
}
