﻿using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Drawing;

namespace LSFV.NativeUI
{
    internal partial class DeveloperPluginMenu
    {
        /// <summary>
        /// Builds the menu and its buttons
        /// </summary>
        private void BuildRoadsMenu()
        {
            // Create road shoulder ui menu
            RoadUIMenu = new UIMenu(MENU_NAME, "~b~Road Segments Menu")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create road shoulder ui menu
            AddRoadUIMenu = new UIMenu(MENU_NAME, "~b~Add Road Segment")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Road shoulder spawn points menu
            RoadNodesUIMenu = new UIMenu(MENU_NAME, "~b~Road Nodes")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // *************************************************
            // Road UI Menu
            // *************************************************

            // Setup buttons
            RoadShoulderCreateButton = new UIMenuItem("Add New Location", "Creates a new Road Shoulder location where you are currently");
            RoadShoulderLoadBlipsButton = new UIMenuItem("Load Checkpoints", "Loads checkpoints in the world as well as blips on the map to show all saved locations in this zone");
            RoadShoulderClearBlipsButton = new UIMenuItem("Clear Checkpoints", "Clears all checkpoints and blips loaded by the ~y~Load Checkpoints ~w~option");

            // Button Events
            RoadShoulderCreateButton.Activated += RoadShouldersCreateButton_Activated;
            RoadShoulderLoadBlipsButton.Activated += (s, e) => LoadZoneLocations(Locations.Residences.Query(), Color.Red, LocationTypeCode.Residence);
            RoadShoulderClearBlipsButton.Activated += (s, e) => ClearZoneLocations();

            // Add buttons
            RoadUIMenu.AddItem(RoadShoulderCreateButton);
            RoadUIMenu.AddItem(RoadShoulderLoadBlipsButton);
            RoadUIMenu.AddItem(RoadShoulderClearBlipsButton);
        }
    }
}
