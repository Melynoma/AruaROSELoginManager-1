//
// FILE     : AccountLoginEventArgs.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2017-09-30
// 

using AruaRoseLoginManager.Enum;

namespace AruaRoseLoginManager.Data
{
    public class LoginEventArgs : ListEventArgs
    {
        /// <summary>
        /// The server id to login to
        /// </summary>
        public Server ServerId { get; set; }

        /// <summary>
        /// The character name to login with (optional)
        /// </summary>
        public string CharacterName { get; set; }

        /// <summary>
        /// The install location for the character (1, 2, or 3)
        /// </summary>
        public int CharacterInstallLocation { get; set; } = 1;
    }

    public class LoginWithPassEventArgs : LoginEventArgs
    {
        /// <summary>
        /// The unhashed password from the prompt.
        /// </summary>
        public string Password { get; set; }
    }
}
