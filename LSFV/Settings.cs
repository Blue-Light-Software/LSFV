using Rage;
using System.IO;
using System.Windows.Forms;

namespace LSFV
{
    internal class Settings
    {
        /// <summary>
        /// Gets or sets the postals file name (without extension) to load on startup
        /// </summary>
        internal static string PostalsFileName { get; set; }

        /// <summary>
        /// Gets or sets the desired logging level of the Game.log
        /// </summary>
        internal static LogLevel LogLevel { get; set; }

        /// <summary>
        /// The key to open the callout interaction menu
        /// </summary>
        internal static Keys OpenMenuKey { get; set; } = Keys.F11;

        /// <summary>
        /// The modifier key to open the callout interaction menu
        /// </summary>
        internal static Keys OpenMenuModifierKey { get; set; } = Keys.None;

        /// <summary>
        /// Loads the user settings from the ini file
        /// </summary>
        internal static void Initialize()
        {
            // Log
            Log.Info("Loading LSFV config...");

            // Ensure file exists
            string path = Path.Combine(EntryPoint.FrameworkFolderPath, "LSFV.ini");
            EnsureConfigExists(path);

            // Open ini file
            var ini = new InitializationFile(path);
            ini.Create();

            // Read key bindings
            OpenMenuKey = ini.ReadEnum("KEYBINDINGS", "OpenMenuKey", Keys.F11);
            OpenMenuModifierKey = ini.ReadEnum("KEYBINDINGS", "OpenMenuModifierKey", Keys.None);

            // Read general settings
            LogLevel = ini.ReadEnum("GENERAL", "LogLevel", LogLevel.DEBUG);
            PostalsFileName = ini.ReadString("GENERAL", "PostalsFilename", "old-postals");

            // Log
            Log.Info("Loaded LSFV config successfully!");
        }

        /// <summary>
        /// Ensures the ini file exists. If not, a new ini is created with the default settings.
        /// </summary>
        /// <param name="path"></param>
        private static void EnsureConfigExists(string path)
        {
            if (!File.Exists(path))
            {
                Log.Info("Creating new LSFV config file...");
                Stream resource = typeof(Settings).Assembly.GetManifestResourceStream("IniConfig");
                using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
    }
}
