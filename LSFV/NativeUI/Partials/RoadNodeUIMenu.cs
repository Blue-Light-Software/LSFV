using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSFV.NativeUI
{
    internal partial class DeveloperPluginMenu
    {
        private UIMenu RoadNodeUIMenu;
        private UIMenu AddRoadNodeUIMenu;
        private UIMenu RoadNodeFlagsUIMenu;

        private UIMenuItem RoadNodeCreateButton;
        private UIMenuCheckboxItem RoadNodeRecordButton;
        private UIMenuItem RoadNodeEditButton;
        private UIMenuItem RoadNodeDeleteButton;
        private UIMenuNumericScrollerItem<int> RoadNodeLaneButton;
        private UIMenuItem RoadNodeFlagsButton;
        private UIMenuItem RoadNodeSaveButton;
        private Dictionary<RoadNodeFlags, UIMenuCheckboxItem> RoadNodeFlagsItems;
        private LocationUIStatus SubMenuStatus;

        /// <summary>
        /// Builds the menu and its buttons
        /// </summary>
        private void BuildRoadNodeSubMenu()
        {
            // Create road shoulder ui menu
            RoadNodeUIMenu = new UIMenu(MENU_NAME, "~b~Road Nodes Menu")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create road shoulder ui menu
            AddRoadNodeUIMenu = new UIMenu(MENU_NAME, "~b~Add Road Node")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create Road flags menu
            RoadNodeFlagsUIMenu = new UIMenu(MENU_NAME, "~b~Road Node Flags")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // *************************************************
            // Road Node UI Menu
            // *************************************************

            // Setup buttons
            RoadNodeCreateButton = new UIMenuItem("Add New Road Node", "Creates a single new ~y~RoadNode ~w~location where you are currently");
            RoadNodeEditButton = new UIMenuItem("Edit Raod Node", "Edit a ~y~RoadNode ~w~location that you are currently near.");
            RoadNodeDeleteButton = new UIMenuItem("Delete Road Node", "Delete a ~y~RoadNode ~w~location that you are currently near.");

            // Disable buttons by default
            RoadNodeEditButton.Enabled = false;
            RoadNodeDeleteButton.Enabled = false;

            // Button Events
            RoadNodeCreateButton.Activated += RoadNodeCreateButton_Activated;
            RoadNodeEditButton.Activated += RoadNodeEditButton_Activated;
            RoadNodeDeleteButton.Activated += RoadNodeDeleteButton_Activated;

            // Add buttons
            RoadNodeUIMenu.AddItem(RoadNodeCreateButton);
            RoadNodeUIMenu.AddItem(RoadNodeEditButton);
            RoadNodeUIMenu.AddItem(RoadNodeDeleteButton);

            // Bind Buttons
            RoadNodeUIMenu.BindMenuToItem(AddRoadNodeUIMenu, RoadNodeCreateButton);
            RoadNodeUIMenu.BindMenuToItem(AddRoadNodeUIMenu, RoadNodeEditButton);

            // *************************************************
            // Add/Edit Road Node UI Menu
            // *************************************************

            // Setup ADD NEW Buttons
            RoadNodeRecordButton = new UIMenuCheckboxItem("Record Placement", false, "If checked, you may drive, and every 15 feet a new ~y~RoadNode ~w~ will be placed.");
            RoadNodeLaneButton = new UIMenuNumericScrollerItem<int>("Lane Index", "Sets the lane number for this road node, starting from the left-most lane", 1, 6, 1);
            RoadNodeFlagsButton = new UIMenuItem("Road Node Flags", "Open the ~y~RoadNode ~w~flags menu.");
            RoadNodeSaveButton = new UIMenuItem("Save", "Saves the changes to this ~y~RoadNode.");

            // Set default indexes
            RoadNodeLaneButton.Index = 0;

            // Button events
            RoadNodeSaveButton.Activated += RoadNodeSaveButton_Activated;
            RoadNodeRecordButton.CheckboxEvent += RoadNodeRecordButton_CheckboxEvent;

            // Add Buttons
            AddRoadNodeUIMenu.AddItem(RoadNodeLaneButton);
            AddRoadNodeUIMenu.AddItem(RoadNodeFlagsButton);
            AddRoadNodeUIMenu.AddItem(RoadNodeRecordButton);
            AddRoadNodeUIMenu.AddItem(RoadNodeSaveButton);

            // Bind buttons
            AddRoadNodeUIMenu.BindMenuToItem(RoadNodeFlagsUIMenu, RoadNodeFlagsButton);

            // Add road shoulder flags list
            RoadNodeFlagsItems = new Dictionary<RoadNodeFlags, UIMenuCheckboxItem>();
            foreach (RoadNodeFlags flag in Enum.GetValues(typeof(RoadNodeFlags)))
            {
                var name = Enum.GetName(typeof(RoadNodeFlags), flag);
                var cb = new UIMenuCheckboxItem(name, false, GetRoadNodeFlagDesc(flag));
                RoadNodeFlagsItems.Add(flag, cb);

                // Add button
                RoadNodeFlagsUIMenu.AddItem(cb);
            }

            // Register for events
            AddRoadNodeUIMenu.OnMenuChange += AddRoadNodeUIMenu_OnMenuChange;
        }

        #region Menu Events

        private void AddRoadNodeUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of a menu?
            if (!forward)
            {
                if (oldMenu == AddRoadNodeUIMenu)
                {
                    SubMenuStatus = LocationUIStatus.None;

                    // Stop listening
                    if (ListeningForConnections)
                    {
                        ListeningForConnections = false;
                        ListenFiber?.Abort();
                        ListenFiber = null;
                    }
                }
            }
        }

        #endregion Menu Events

        private void RoadNodeSaveButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        private void RoadNodeCreateButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        private void RoadNodeEditButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        private void RoadNodeRecordButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        private void RoadNodeDeleteButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        /// <summary>
        /// Method called when the <see cref="RoadNodeRecordButton"/> checked state has changed
        /// </summary>
        private void RoadNodeRecordButton_CheckboxEvent(UIMenuCheckboxItem sender, bool Checked)
        {
            if (Checked)
            {
                RoadNodeSaveButton.Text = "Continue";
                RoadNodeSaveButton.Description = "Saves the changes to this ~y~RoadNode~w~, and begins recording your movements.";
            }
            else
            {
                RoadNodeSaveButton.Text = "Save";
                RoadNodeSaveButton.Description = "Saves the changes to this ~y~RoadNode.";
            }
        }
        
        /// <summary>
        /// Gets a description of a <see cref="RoadNodeFlags"/>
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private string GetRoadNodeFlagDesc(RoadNodeFlags flag)
        {
            switch (flag)
            {
                default: return "";
                case RoadNodeFlags.SlipLane: return "";
                case RoadNodeFlags.LeftTurnOnly: return "";
                case RoadNodeFlags.RightTurnOnly: return "";
                case RoadNodeFlags.LeftTurnOrStraight: return "";
                case RoadNodeFlags.RightTurnOrStraight: return "";
            }
        }

    }
}
