using BlueLightSoftware.Common.Extensions;
using BlueLightSoftware.Common.Game;
using LSFV.Extensions;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LSFV.NativeUI
{
    /// <summary>
    /// This partial class contains the logic and menu properties
    /// for the <see cref="RoadShoulder"/> location
    /// </summary>
    internal partial class DeveloperPluginMenu
    {
        private UIMenu RoadShoulderUIMenu;
        private UIMenu AddRoadShoulderUIMenu;

        #region Properties

        private SpawnPoint RoadShoulderLocation { get; set; }

        private Dictionary<RoadFlags, UIMenuCheckboxItem> RoadShoulderFlagsItems { get; set; }

        private Dictionary<RoadShoulderPosition, UIMenuItem<SpawnPoint>> RoadShoulderSpawnPointItems { get; set; }

        #endregion

        #region Control Properties

        private UIMenuItem RoadShoulderCreateButton { get; set; }

        private UIMenuItem RoadShoulderEditButton { get; set; }

        private UIMenuItem RoadShoulderDeleteButton { get; set; }

        private UIMenuItem RoadShoulderLoadBlipsButton { get; set; }

        private UIMenuItem RoadShoulderClearBlipsButton { get; set; }

        private UIMenuNumericScrollerItem<int> RoadShoulderSpeedButton { get; set; }

        private UIMenuListItem RoadShoulderZoneButton { get; set; }

        private UIMenuItem RoadShoulderStreetButton { get; set; }

        private UIMenuItem RoadShoulderHintButton { get; set; }

        private UIMenuItem RoadShoulderFlagsButton { get; set; }

        private UIMenuItem RoadShoulderBeforeFlagsButton { get; set; }

        private UIMenuListItem RoadShoulderBeforeListButton { get; set; }

        private Dictionary<IntersectionFlags, UIMenuCheckboxItem> BeforeIntersectionItems { get; set; }

        private UIMenuItem RoadShoulderAfterFlagsButton { get; set; }

        private UIMenuListItem RoadShoulderAfterListButton { get; set; }

        private Dictionary<IntersectionFlags, UIMenuCheckboxItem> AfterIntersectionItems { get; set; }

        private UIMenuItem RoadShoulderSaveButton { get; set; }

        private UIMenuItem RoadShoulderSpawnPointsButton { get; set; }

        #endregion

        /// <summary>
        /// Builds the menu and its buttons
        /// </summary>
        private void BuildRoadShouldersMenu()
        {
            // Create road shoulder ui menu
            RoadShoulderUIMenu = new UIMenu(MENU_NAME, "~b~Road Shoulder Menu")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create road shoulder ui menu
            AddRoadShoulderUIMenu = new UIMenu(MENU_NAME, "~b~Add Road Shoulder")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Road Shoulder flags selection menu
            RoadShoulderFlagsUIMenu = new UIMenu(MENU_NAME, "~b~Road Shoulder Flags")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Road shoulder spawn points menu
            RoadShoulderSpawnPointsUIMenu = new UIMenu(MENU_NAME, "~b~Road Shoulder Spawn Points")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // *************************************************
            // RoadShoulder UI Menu
            // *************************************************

            // Setup buttons
            RoadShoulderCreateButton = new UIMenuItem("Add New Location", "Creates a new Road Shoulder location where you are currently");
            RoadShoulderEditButton = new UIMenuItem("Edit Location", "Edit a Road Shoulder location that you are currently near. ~y~Blips must be loaded");
            RoadShoulderDeleteButton = new UIMenuItem("Delete Location", "Delete a Road Shoulder location that you are currently near. ~y~Blips must be loaded");
            RoadShoulderLoadBlipsButton = new UIMenuItem("Load Checkpoints", "Loads checkpoints in the world as well as blips on the map to show all saved locations in this zone");
            RoadShoulderClearBlipsButton = new UIMenuItem("Clear Checkpoints", "Clears all checkpoints and blips loaded by the ~y~Load Checkpoints ~w~option");

            // Disable buttons by default
            RoadShoulderEditButton.Enabled = false;
            RoadShoulderDeleteButton.Enabled = false;
            RoadShoulderClearBlipsButton.Enabled = false;

            // Button Events
            RoadShoulderCreateButton.Activated += RoadShouldersCreateButton_Activated;
            RoadShoulderEditButton.Activated += RoadShoulderEditButton_Activated;
            RoadShoulderDeleteButton.Activated += RoadShoulderDeleteButton_Activated;
            RoadShoulderLoadBlipsButton.Activated += (s, e) =>
            {
                if (LoadZoneLocations(Locations.RoadShoulders.Query(), Color.Red, LocationTypeCode.RoadShoulder))
                {
                    RoadShoulderLoadBlipsButton.Enabled = false;
                    RoadShoulderClearBlipsButton.Enabled = true;
                }
            };
            RoadShoulderClearBlipsButton.Activated += (s, e) =>
            {
                RoadShoulderLoadBlipsButton.Enabled = true;
                RoadShoulderClearBlipsButton.Enabled = false;
                ClearZoneLocations();
            };

            // Add buttons
            RoadShoulderUIMenu.AddItem(RoadShoulderCreateButton);
            RoadShoulderUIMenu.AddItem(RoadShoulderEditButton);
            RoadShoulderUIMenu.AddItem(RoadShoulderDeleteButton);
            RoadShoulderUIMenu.AddItem(RoadShoulderLoadBlipsButton);
            RoadShoulderUIMenu.AddItem(RoadShoulderClearBlipsButton);

            // Bind Buttons
            RoadShoulderUIMenu.BindMenuToItem(AddRoadShoulderUIMenu, RoadShoulderCreateButton);
            RoadShoulderUIMenu.BindMenuToItem(AddRoadShoulderUIMenu, RoadShoulderEditButton);

            // *************************************************
            // Add RoadShoulder UI Menu
            // *************************************************

            // Setup Buttons
            RoadShoulderStreetButton = new UIMenuItem("Street Name", "");
            RoadShoulderHintButton = new UIMenuItem("Location Hint", "");
            RoadShoulderSpeedButton = new UIMenuNumericScrollerItem<int>("Speed Limit", "Sets the speed limit of this road", 10, 80, 5);
            RoadShoulderZoneButton = new UIMenuListItem("Zone", "Selects the zone");
            RoadShoulderSpawnPointsButton = new UIMenuItem("Spawn Points", "Sets safe spawn points for ped groups");
            RoadShoulderFlagsButton = new UIMenuItem("Road Shoulder Flags", "Open the RoadShoulder flags menu.");
            RoadShoulderSaveButton = new UIMenuItem("Save", "Saves the current location data to the database.");

            // Button events
            RoadShoulderStreetButton.Activated += DispayKeyboard_SetDescription;
            RoadShoulderHintButton.Activated += DispayKeyboard_SetDescription;
            RoadShoulderSaveButton.Activated += RoadShoulderSaveButton_Activated;

            // Add Buttons
            AddRoadShoulderUIMenu.AddItem(RoadShoulderStreetButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderHintButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderSpeedButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderZoneButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderSpawnPointsButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderFlagsButton);
            AddRoadShoulderUIMenu.AddItem(RoadShoulderSaveButton);

            // Bind buttons
            AddRoadShoulderUIMenu.BindMenuToItem(RoadShoulderFlagsUIMenu, RoadShoulderFlagsButton);
            AddRoadShoulderUIMenu.BindMenuToItem(RoadShoulderSpawnPointsUIMenu, RoadShoulderSpawnPointsButton);

            // *************************************************
            // Intersection Flags
            // *************************************************
            RoadShoulderBeforeFlagsUIMenu = new UIMenu(MENU_NAME, "~b~Before Intersection Flags")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            RoadShoulderAfterFlagsUIMenu = new UIMenu(MENU_NAME, "~b~After Intersection Flags")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create Buttons
            RoadShoulderBeforeListButton = new UIMenuListItem("Direction", "The direction of the ajoining road (only applies to ~y~Three Way intersections)");
            RoadShoulderAfterListButton = new UIMenuListItem("Direction", "The direction of the ajoining road (only applies to ~y~Three Way intersections)");

            // Add directions
            foreach (RelativeDirection direction in Enum.GetValues(typeof(RelativeDirection)))
            {
                var name = Enum.GetName(typeof(RelativeDirection), direction);
                RoadShoulderBeforeListButton.Collection.Add(direction, name);
                RoadShoulderAfterListButton.Collection.Add(direction, name);
            }

            // Add buttons to the menu
            RoadShoulderBeforeFlagsUIMenu.AddItem(RoadShoulderBeforeListButton);
            RoadShoulderAfterFlagsUIMenu.AddItem(RoadShoulderAfterListButton);

            // Add road shoulder intersection flags
            BeforeIntersectionItems = new Dictionary<IntersectionFlags, UIMenuCheckboxItem>();
            AfterIntersectionItems = new Dictionary<IntersectionFlags, UIMenuCheckboxItem>();
            foreach (IntersectionFlags flag in Enum.GetValues(typeof(IntersectionFlags)))
            {
                var name = Enum.GetName(typeof(IntersectionFlags), flag);
                var cb = new UIMenuCheckboxItem(name, false);
                BeforeIntersectionItems.Add(flag, cb);
                RoadShoulderBeforeFlagsUIMenu.AddItem(cb);

                cb = new UIMenuCheckboxItem(name, false);
                AfterIntersectionItems.Add(flag, cb);
                RoadShoulderAfterFlagsUIMenu.AddItem(cb);
            }

            // Bind buttons
            RoadShoulderBeforeFlagsButton = new UIMenuItem("Before Intersection Flags");
            RoadShoulderBeforeFlagsButton.LeftBadge = UIMenuItem.BadgeStyle.Car;
            RoadShoulderFlagsUIMenu.AddItem(RoadShoulderBeforeFlagsButton);
            RoadShoulderFlagsUIMenu.BindMenuToItem(RoadShoulderBeforeFlagsUIMenu, RoadShoulderBeforeFlagsButton);

            RoadShoulderAfterFlagsButton = new UIMenuItem("After Intersection Flags");
            RoadShoulderAfterFlagsButton.LeftBadge = UIMenuItem.BadgeStyle.Car;
            RoadShoulderFlagsUIMenu.AddItem(RoadShoulderAfterFlagsButton);
            RoadShoulderFlagsUIMenu.BindMenuToItem(RoadShoulderAfterFlagsUIMenu, RoadShoulderAfterFlagsButton);

            // Add positions
            RoadShoulderSpawnPointItems = new Dictionary<RoadShoulderPosition, UIMenuItem<SpawnPoint>>();
            foreach (RoadShoulderPosition flag in Enum.GetValues(typeof(RoadShoulderPosition)))
            {
                var name = Enum.GetName(typeof(RoadShoulderPosition), flag);
                var item = new UIMenuItem<SpawnPoint>(null, name, "Activate to set position. ~y~Ensure there is proper space to spawn a group of Peds at this location");
                item.Activated += RoadShoulderSpawnPointButton_Activated;
                RoadShoulderSpawnPointItems.Add(flag, item);

                // Add button
                RoadShoulderSpawnPointsUIMenu.AddItem(item);
            }

            // Add road shoulder flags list
            RoadShoulderFlagsItems = new Dictionary<RoadFlags, UIMenuCheckboxItem>();
            foreach (RoadFlags flag in Enum.GetValues(typeof(RoadFlags)))
            {
                var name = Enum.GetName(typeof(RoadFlags), flag);
                var cb = new UIMenuCheckboxItem(name, false, GetRoadFlagDesc(flag));
                RoadShoulderFlagsItems.Add(flag, cb);

                // Add button
                RoadShoulderFlagsUIMenu.AddItem(cb);
            }

            // Register for events
            AddRoadShoulderUIMenu.OnMenuChange += AddRoadShoulderUIMenu_OnMenuChange;
            RoadShoulderFlagsUIMenu.OnMenuChange += RoadShoulderFlagsUIMenu_OnMenuChange;
        }

        #region Menu Events

        private void AddRoadShoulderUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of a menu?
            if (!forward)
            {
                if (oldMenu == AddRoadUIMenu)
                {
                    ResetCheckPoints();
                    Status = LocationUIStatus.None;
                }
            }
        }

        private void RoadShoulderFlagsUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of this menu?
            if (!forward && oldMenu == RoadShoulderFlagsUIMenu)
            {
                // We must have at least 1 item checked
                if (RoadShoulderFlagsItems.Any(x => x.Value.Checked))
                {
                    RoadShoulderFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
                }
                else
                {
                    RoadShoulderFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
                }
            }
        }

        #endregion Menu Events

        #region Control Events

        /// <summary>
        /// Method called when the "Create New Road Shoulder" button is clicked.
        /// Clears all prior data.
        /// </summary>
        private void RoadShouldersCreateButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            //
            // Reset everything!
            //
            // Delete old handle
            if (LocationCheckpoint != null)
            {
                LocationCheckpoint.Delete();
                LocationCheckpoint = null;
            }

            // Update description
            AddRoadShoulderUIMenu.SubtitleText = "Add Road Shoulder";

            // Grab player location
            var player = GetPlayer();
            var pos = player.Position;
            var cpPos = new Vector3(pos.X, pos.Y, pos.Z - ZCorrection);
            RoadShoulderLocation = new SpawnPoint(cpPos, player.Heading);

            // Reset road shoulder flags
            foreach (var cb in RoadShoulderFlagsItems.Values)
            {
                cb.Checked = false;
            }

            // Reset spawn points
            foreach (UIMenuItem<SpawnPoint> item in RoadShoulderSpawnPointItems.Values)
            {
                item.Tag = null;
                item.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Reset intersection flags
            foreach (var cb in BeforeIntersectionItems)
            {
                cb.Value.Checked = false;
                AfterIntersectionItems[cb.Key].Checked = false;
            }
            RoadShoulderBeforeListButton.Index = 0;
            RoadShoulderAfterListButton.Index = 0;

            // Create checkpoint at the player location
            LocationCheckpoint = Checkpoint.Create(cpPos, Color.Red);

            // Add Zones
            RoadShoulderZoneButton.Collection.Clear();
            RoadShoulderZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(pos));
            RoadShoulderZoneButton.Collection.Add("HIGHWAY");

            // Set street name default
            var streetName = GameWorld.GetStreetNameAtLocation(pos, out string crossingRoad);
            if (!String.IsNullOrEmpty(streetName))
            {
                RoadShoulderStreetButton.Description = streetName;
                RoadShoulderStreetButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                RoadShoulderStreetButton.Description = "";
                RoadShoulderStreetButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Crossing road
            if (!String.IsNullOrEmpty(crossingRoad))
            {
                RoadShoulderHintButton.Description = $"near {crossingRoad}";
                RoadShoulderHintButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                // Reset ticks
                RoadShoulderHintButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Reset
            RoadShoulderFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            RoadShoulderSpawnPointsButton.RightBadge = UIMenuItem.BadgeStyle.None;

            // Enable button
            RoadShoulderSaveButton.Enabled = true;

            // Flag
            Status = LocationUIStatus.Adding;
        }

        /// <summary>
        /// Method called when the "Edit Road Shoulder" menu item is clicked.
        /// </summary>
        private void RoadShoulderEditButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as RoadShoulder;
            if (editingItem == null) return;

            // Reset road shoulder flags
            foreach (var item in RoadShoulderFlagsItems)
            {
                item.Value.Checked = editingItem.Flags.Contains(item.Key);
            }

            // Are flags complete?
            if (RoadShoulderFlagsItems.Any(x => x.Value.Checked))
            {
                RoadShoulderFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                RoadShoulderFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Reset spawn points
            bool complete = true;
            foreach (var item in RoadShoulderSpawnPointItems)
            {
                // Checking for incomplete spawn points
                if (editingItem.SpawnPoints.ContainsKey(item.Key))
                {
                    // Assign item as complete
                    var sp = editingItem.SpawnPoints[item.Key];
                    item.Value.Tag = sp;
                    item.Value.RightBadge = UIMenuItem.BadgeStyle.Tick;

                    // Create checkpoint!
                    var position = (int)item.Key;
                    var checkpoint = Checkpoint.Create(sp.Position, Color.Yellow, radius: 5f);
                    SpawnPointHandles.AddOrUpdate(position, checkpoint);
                }
                else
                {
                    item.Value.Tag = null;
                    item.Value.RightBadge = UIMenuItem.BadgeStyle.None;
                    complete = false;
                }
            }

            // Are spawnpoints complete?
            var spBadgeStyle = (!complete) ? UIMenuItem.BadgeStyle.None : UIMenuItem.BadgeStyle.Tick;
            RoadShoulderSpawnPointsButton.RightBadge = spBadgeStyle;

            // Reset Before intersection flags
            RoadShoulderBeforeListButton.Index = RoadShoulderBeforeListButton.Collection.IndexOf(editingItem.BeforeIntersectionDirection);
            foreach (var flag in editingItem.BeforeIntersectionFlags)
            {
                BeforeIntersectionItems[flag].Checked = true;
            }

            // Reset After intersection flags
            RoadShoulderAfterListButton.Index = RoadShoulderAfterListButton.Collection.IndexOf(editingItem.AfterIntersectionDirection);
            foreach (var flag in editingItem.AfterIntersectionFlags)
            {
                AfterIntersectionItems[flag].Checked = true;
            }

            // Add Zones
            RoadShoulderZoneButton.Collection.Clear();
            RoadShoulderZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(editingItem.Position));
            RoadShoulderZoneButton.Collection.Add("HIGHWAY");
            var index = RoadShoulderZoneButton.Collection.IndexOf(editingItem.Zone.ScriptName);
            if (index >= 0)
                RoadShoulderZoneButton.Index = index;

            // Set street name default
            if (!String.IsNullOrEmpty(editingItem.StreetName))
            {
                RoadShoulderStreetButton.Description = editingItem.StreetName;
                RoadShoulderStreetButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                RoadShoulderStreetButton.Description = "";
                RoadShoulderStreetButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Crossing road
            if (!String.IsNullOrEmpty(editingItem.Hint))
            {
                RoadShoulderHintButton.Description = editingItem.Hint;
                RoadShoulderHintButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                // Reset ticks
                RoadShoulderHintButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Set Speed
            RoadShoulderSpeedButton.Value = editingItem.SpeedLimit;

            // Enable button
            RoadShoulderSaveButton.Enabled = true;

            // Update description
            AddRoadUIMenu.SubtitleText = "Editing Road Shoulder";

            // Flag
            RoadShoulderLocation = new SpawnPoint(editingItem.Position, editingItem.Heading);
            Status = LocationUIStatus.Editing;
        }

        /// <summary>
        /// Method called when the "Delete Road Shoulder" menu item is clicked.
        /// </summary>
        private void RoadShoulderDeleteButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as RoadShoulder;
            if (editingItem == null) return;

            // Disable buttons
            DisableAllEditButtons();

            // Prevent app crashing
            try
            {
                // Delete location
                if (Locations.RoadShoulders.Delete(editingItem.Id))
                {
                    // Delete checkpoint
                    DeleteBlipAndCheckpoint(LocationCheckpoint);
                    LocationCheckpoint = null;

                    // Notify the user
                    ShowNotification("Delete Road Shoulder", $"~g~Location deleted.");
                }
                else
                {
                    // Display notification to the player
                    ShowNotification("Delete Road Shoulder", $"~o~Unable to delete location from database.");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Delete Road Shoulder", $"~r~Action Failed: ~o~Please check your Game.log!");
            }
        }

        /// <summary>
        /// Method called when a residece "Spawnpoint" button is clicked on the Road Shoulder Spawn Points UI menu
        /// </summary>
        private void RoadShoulderSpawnPointButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            Checkpoint handle;
            var player = GetPlayer();
            var pos = player.Position;
            var value = (RoadShoulderPosition)Enum.Parse(typeof(RoadShoulderPosition), selectedItem.Text);
            int index = (int)value;
            var menuItem = (UIMenuItem<SpawnPoint>)selectedItem;

            // Check, do we have a check point already for this position?
            if (SpawnPointHandles.ContainsKey(index))
            {
                handle = SpawnPointHandles[index];
                handle.Delete();
            }

            // Create new checkpoint !!important, need to subtract 2 from the Z since checkpoints spawn at waist level
            var cpPos = new Vector3(pos.X, pos.Y, pos.Z - ZCorrection);
            handle = Checkpoint.Create(cpPos, Color.Yellow, radius: 5f);
            SpawnPointHandles.AddOrUpdate(index, handle);

            // Create spawn point
            menuItem.Tag = new SpawnPoint(cpPos, player.Heading);
            menuItem.RightBadge = UIMenuItem.BadgeStyle.Tick;

            // Are we complete
            bool complete = true;
            foreach (UIMenuItem<SpawnPoint> item in RoadShoulderSpawnPointItems.Values)
            {
                if (item.Tag == null)
                {
                    complete = false;
                    break;
                }
            }

            // Signal to the player
            if (complete)
            {
                RoadShoulderSpawnPointsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
        }

        /// <summary>
        /// Method called when the "Save" button is clicked on the Road Shoulder UI menu
        /// </summary>
        private void RoadShoulderSaveButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Disable button to prevent spam clicking!
            RoadShoulderSaveButton.Enabled = false;
            var heading = (Status == LocationUIStatus.Adding) ? "Add" : "Edit";

            // Ensure everything is done
            var requiredItems = new[] { RoadShoulderStreetButton, RoadShoulderSpawnPointsButton, RoadShoulderFlagsButton };
            foreach (var item in requiredItems)
            {
                if (item.RightBadge != UIMenuItem.BadgeStyle.Tick)
                {
                    // Display notification to the player
                    ShowNotification($"{heading} Road Shoulder", $"~o~Location does not have all required parameters set");
                    RoadShoulderSaveButton.Enabled = true;
                    return;
                }
            }

            // Be nice and prevent locking up
            GameFiber.Yield();

            // Grab zone instance
            string zoneName = RoadShoulderZoneButton.SelectedValue.ToString();
            var zone = Locations.WorldZones.FindOne(x => x.ScriptName == zoneName);
            if (zone == null)
            {
                // Display notification to the player
                ShowNotification($"{heading} Road Shoulder", $"~r~Save Failed: ~o~Unable to find zone in the locations database!");
                return;
            }

            // Wrap to prevent crashing
            try
            {
                // Create new shoulder object
                var shoulder = new RoadShoulder()
                {
                    Position = RoadShoulderLocation.Position,
                    Heading = RoadShoulderLocation.Heading,
                    SpeedLimit = RoadShoulderSpeedButton.Value,
                    StreetName = RoadShoulderStreetButton.Description,
                    Hint = RoadShoulderHintButton.Description,
                    Flags = RoadShoulderFlagsItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList(),
                    BeforeIntersectionFlags = BeforeIntersectionItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList(),
                    BeforeIntersectionDirection = (RelativeDirection)RoadShoulderBeforeListButton.SelectedValue,
                    AfterIntersectionFlags = AfterIntersectionItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList(),
                    AfterIntersectionDirection = (RelativeDirection)RoadShoulderAfterListButton.SelectedValue,
                    SpawnPoints = RoadShoulderSpawnPointItems.ToDictionary(k => k.Key, v => v.Value.Tag),
                    Zone = zone
                };

                // Be nice and prevent locking up
                GameFiber.Yield();

                // Insert or update
                if (Status == LocationUIStatus.Editing)
                {
                    // Grab item
                    if (LocationCheckpoint?.Tag == null) return;

                    // Ensure tag is set properly
                    var editingItem = LocationCheckpoint.Tag as RoadShoulder;
                    if (editingItem == null) return;

                    // Set id for update
                    shoulder.Id = editingItem.Id;

                    // Save location in the database
                    Locations.RoadShoulders.Update(shoulder);
                }
                else
                {
                    // Save location in the database
                    var id = Locations.RoadShoulders.Insert(shoulder);
                    shoulder.Id = id.AsInt32;
                }

                // Editing
                LocationCheckpoint.Tag = shoulder;

                // Display notification to the player
                ShowNotification($"~b~{heading} Road Shoulder.", $"~g~Location saved Successfully.");
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification($"~b~{heading} Road Shoulder.", $"~r~Save Failed: ~o~Please check your Game.log!");
            }

            // Go back
            AddRoadShoulderUIMenu.GoBack();

            // Are we currently showing checkpoints and blips?
            if (Status == LocationUIStatus.Adding && ShowingZoneLocations)
            {
                var pos = RoadShoulderLocation.Position;

                // Create checkpint and blips
                var blip = new Blip(pos) { Color = Color.Red };
                LocationCheckpoint = Checkpoint.Create(pos, Color.Red, forceGround: true);
                ZoneCheckpoints.Add(LocationCheckpoint, blip);
            }
        }

        #endregion Control Events

        /// <summary>
        /// Gets the text description of a <see cref="RoadFlags"/>
        /// </summary>
        private string GetRoadFlagDesc(RoadFlags flag)
        {
            switch (flag)
            {
                default: return "";
                case RoadFlags.DirtRoad: return "Describes a Road as being along an unpaved road";
                case RoadFlags.OneWayRoad: return "Describes a Road as being along a one way road";
                case RoadFlags.Alley: return "Describes a Road as being in an alley way";
                case RoadFlags.SingleLaneRoad: return "Describes a Road as being along a single lane road where the direction of traffic moves in both directions";
                case RoadFlags.PassingZone: return "Describes a road with a dotted center line, allowing passing on the on coming traffic lane";
                case RoadFlags.OnInterstate: return "Describes a Road as being on the interstate";
                case RoadFlags.InterstateOnRamp: return "Describes a Road as being on an interstate freeway On ramp";
                case RoadFlags.InterstateOffRamp: return "Describes a Road as being on an interstate freeway Off ramp";
                case RoadFlags.OnBridge: return "Describes a Road as being on a bridge";
                case RoadFlags.InsideTunnel: return "Describes a Road as being inside of a tunnel";
                case RoadFlags.DrivewaysLeft: return "Describes a Road as being along a road with driveways on the left side";
                case RoadFlags.DrivewaysRight: return "Describes a Road as being along a road with driveways on the right side";
                case RoadFlags.BusinessesLeft: return "Describes a Road as being along a road with businesses on the left side";
                case RoadFlags.BusinessesRight: return "Describes a Road as being along a road with businesses on the right side";
                case RoadFlags.HasCenterTurnRoad: return "Describes a Road as having a center left turn lane";
                case RoadFlags.HasRightTurnOnlyLane: return "Describes a Road as having a Right turn only lane";
                case RoadFlags.NoBigVehicles: return "Describes a road that doesn't support large vehicles";
            }
        }
    }
}
