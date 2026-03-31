//
// FILE     : Character.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-18
// 

namespace AruaRoseLoginManager.Data
{
    /// <summary>
    /// Holds the information for a character
    /// </summary>
    public class Character
    {
        /// <summary>
        /// Character name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Install location for this character (1, 2 or 3)
        /// </summary>
        public int InstallLocation { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The character name</param>
        /// <param name="installLocation">The install location (1-3)</param>
        public Character(string name, int installLocation = 1)
        {
            Name = name;
            InstallLocation = installLocation >= 1 && installLocation <= 3 ? installLocation : 1;
        }
    }
}
