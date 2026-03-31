//
// FILE     : WindowSize.cs
// PROJECT  : AruaROSE Login Manager
// AUTHOR   : xHergz
// DATE     : 2021-02-19
// 

namespace AruaRoseLoginManager.Data
{
    /// <summary>
    /// Holds window size information for the login manager window (defaults + current)
    /// </summary>
    public class WindowSize
    {
        /// <summary>
        /// Default size width
        /// </summary>
        private const int DEFAULT_WIDTH = 1250;

        /// <summary>
        /// Default size height
        /// </summary>
        private const int DEFAULT_HEIGHT = 1050;

        /// <summary>
        /// Compact size width
        /// </summary>
        private const int COMPACT_WIDTH = DEFAULT_WIDTH / 2;

        /// <summary>
        /// Compact size height
        /// </summary>
        private const int COMPACT_HEIGHT = DEFAULT_HEIGHT / 2;

        /// <summary>
        /// Gets the default window size
        /// </summary>
        public static WindowSize Default {
            get
            {
                return new WindowSize() { Height = DEFAULT_HEIGHT, Width = DEFAULT_WIDTH };
            }
        }

        /// <summary>
        /// Gets the compact window size
        /// </summary>
        public static WindowSize Compact
        {
            get
            {
                return new WindowSize() { Height = COMPACT_HEIGHT, Width = COMPACT_WIDTH };
            }
        }

        /// <summary>
        /// Window width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Window height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Game window width for install location 1 (for TRose.exe _width parameter)
        /// </summary>
        public int GameWidth1 { get; set; } = 1024;

        /// <summary>
        /// Game window height for install location 1 (for TRose.exe _height parameter)
        /// </summary>
        public int GameHeight1 { get; set; } = 768;

        /// <summary>
        /// Default fullscreen setting for game at install location 1 (false = windowed, true = fullscreen)
        /// </summary>
        public bool IsFullscreen1 { get; set; } = false;

        /// <summary>
        /// Game window width for install location 2 (for TRose.exe _width parameter)
        /// </summary>
        public int GameWidth2 { get; set; } = 1024;

        /// <summary>
        /// Game window height for install location 2 (for TRose.exe _height parameter)
        /// </summary>
        public int GameHeight2 { get; set; } = 768;

        /// <summary>
        /// Default fullscreen setting for game at install location 2 (false = windowed, true = fullscreen)
        /// </summary>
        public bool IsFullscreen2 { get; set; } = false;

        /// <summary>
        /// Game window width for install location 3 (for TRose.exe _width parameter)
        /// </summary>
        public int GameWidth3 { get; set; } = 1024;

        /// <summary>
        /// Game window height for install location 3 (for TRose.exe _height parameter)
        /// </summary>
        public int GameHeight3 { get; set; } = 768;

        /// <summary>
        /// Default fullscreen setting for game at install location 3 (false = windowed, true = fullscreen)
        /// </summary>
        public bool IsFullscreen3 { get; set; } = false;

        // Legacy properties for backwards compatibility - default to location 1
        /// <summary>
        /// Game window width (for TRose.exe _width parameter) - defaults to location 1
        /// </summary>
        public int GameWidth { get => GameWidth1; set => GameWidth1 = value; }

        /// <summary>
        /// Game window height (for TRose.exe _height parameter) - defaults to location 1
        /// </summary>
        public int GameHeight { get => GameHeight1; set => GameHeight1 = value; }

        /// <summary>
        /// Default fullscreen setting for game (false = windowed, true = fullscreen) - defaults to location 1
        /// </summary>
        public bool IsFullscreen { get => IsFullscreen1; set => IsFullscreen1 = value; }

        /// <summary>
        /// Whether stream mode is enabled (hides account and location info)
        /// </summary>
        public bool StreamMode { get; set; } = false;

        // Persisted account table widths
        public double AccountColumn1 { get; set; } = 0;
        public double AccountColumn2 { get; set; } = 0;
        public double AccountColumn3 { get; set; } = 0;
        public double AccountColumn4 { get; set; } = 0;
        public double AccountColumn5 { get; set; } = 0;
        public double AccountColumn6 { get; set; } = 0;
        public double AccountColumn7 { get; set; } = 0;
        public double AccountColumn8 { get; set; } = 0;

        // Persisted party table widths
        public double PartyColumn1 { get; set; } = 0;
        public double PartyColumn2 { get; set; } = 0;
        public double PartyColumn3 { get; set; } = 0;
        public double PartyColumn4 { get; set; } = 0;
        public double PartyColumn5 { get; set; } = 0;
        public double PartyColumn6 { get; set; } = 0;
        public double PartyColumn7 { get; set; } = 0;
        public double PartyColumn8 { get; set; } = 0;
    }
}
