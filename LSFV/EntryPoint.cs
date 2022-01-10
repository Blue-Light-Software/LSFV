using LSFV.NativeUI;
using Rage;
using Rage.Attributes;
using System;
using System.IO;
using System.Reflection;

[assembly: Plugin(
    "Location Services Framework V",
    Author = "Brexin212",
    Description = "Developer's tool to manually collect spawn points.",
    PrefersSingleInstance = true)]

namespace LSFV
{
    public class EntryPoint
    {
        /// <summary>
        /// Gets the current plugin version
        /// </summary>
        public static Version PluginVersion { get; private set; }

        private static DeveloperPluginMenu RageMenu;

        private static bool CanRun;

        private static StaticFinalizer Finalizer;

        /// <summary>
        /// Gets the root folder path to the GTA V installation folder
        /// </summary>
        public static string GTARootPath { get; private set; }

        /// <summary>
        /// Gets the root folder path to the AgencyDispatchFramework plugin folder
        /// </summary>
        public static string FrameworkFolderPath { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static void Main()
        {
            Game.Console.Print("*****GTA V Location Services Framework 1.0 by Brexin212*****");
            //Game.Console.Print("Type \"LSFV\" to display help.");

            // Define our plugin version and root paths
            var assembly = Assembly.GetExecutingAssembly();
            PluginVersion = assembly.GetName().Version;

            // Get GTA 5 root directory path
            GTARootPath = @".\";
            FrameworkFolderPath = @".\Plugins\LSFV\";

            // Initialize log file
            Log.Initialize(Path.Combine(FrameworkFolderPath, "Game.log"), LogLevel.DEBUG);

            // Initialize settings and database
            Settings.Initialize();
            Postal.Initialize();
            Locations.Initialize();

            // Set logging level to config value
            Log.SetLogLevel(Settings.LogLevel);

            // Create internals
            Finalizer = new StaticFinalizer(Finalize);
            CanRun = true;
            RageMenu = new DeveloperPluginMenu();

            // Main loop
            while (CanRun)
            {
                // Be nice and yield
                GameFiber.Yield();

                // Process menu
                RageMenu.Process();
            }
        }

        private static void Finalize()
        {
            // Stop loop
            CanRun = false;
            
            // Close database handle
            Locations.Shutdown();

            // Close menu
            //RageMenu.StopListening();
        }
    }
}
