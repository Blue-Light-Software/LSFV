using BlueLightSoftware.Common.Game;
using LiteDB;
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
    internal partial class DeveloperPluginMenu
    {
        private const int ROAD_START = 34;
        private const int ROAD_CONN = 35;
        private const int ROAD_END = 36;
        private const int ROAD_BROKEN = 11;

        private RoadSegment ActiveSegment;
        private RoadJunction LastJunction;
        private RoadSegment ClosestRoadSegment;
        private Checkpoint ClosestCheckPoint;

        private UIMenuItem RoadContinueButton;
        private UIMenuNumericScrollerItem<int> RoadSpeedButton;
        private UIMenuNumericScrollerItem<int> RoadLanesButton;
        private UIMenuListItem RoadZoneButton;
        private UIMenuItem RoadFlagsButton;
        private UIMenuItem RoadNextButton;
        private UIMenuItem RoadNodesButton;
        private UIMenuItem RoadRecordNodesButton;
        private UIMenuItem RoadBreakButton;
        private UIMenuCheckboxItem RoadAutoBreakButton;
        private UIMenuItem RoadEndButton;
        private UIMenuItem RoadConnectButton;
        private UIMenuItem RoadDeleteConnectionsButton;
        private UIMenuItem RoadSaveButton;
        private List<Checkpoint> HiddenCheckPoints;
        private Dictionary<RoadFlags, UIMenuCheckboxItem> RoadFlagsItems;
        private SpawnPoint LastRoadLocation;
        private Road ActiveRoad;
        private WorldZone ActiveZone;
        private bool ForceBreak;

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

            // Create road shoulder ui menu
            RoadRecordUIMenu = new UIMenu(MENU_NAME, "~b~Recording Road Segment")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create Road flags menu
            RoadFlagsUIMenu = new UIMenu(MENU_NAME, "~b~Road Flags")
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
            RoadCreateButton = new UIMenuItem("Begin New", "Creates a new Road Segment location where you are currently");
            RoadContinueButton = new UIMenuItem("Continue Segment", "Continues from an existing RoadSegment by attaching a Junction, and creating a new Road Segment");
            RoadEditButton = new UIMenuItem("Edit Segment", "Edit a Road Segment location that you are currently near. ~y~Blips must be loaded");
            RoadDeleteButton = new UIMenuItem("Delete Segment", "Delete a Road Segment location that you are currently near. ~y~Blips must be loaded");
            RoadLoadBlipsButton = new UIMenuItem("Load Checkpoints", "Loads checkpoints in the world as well as blips on the map to show all saved locations in this zone");
            RoadClearBlipsButton = new UIMenuItem("Clear Checkpoints", "Clears all checkpoints and blips loaded by the ~y~Load Checkpoints ~w~option");

            // Disable buttons by default
            RoadContinueButton.Enabled = false;
            RoadEditButton.Enabled = false;
            RoadDeleteButton.Enabled = false;
            RoadClearBlipsButton.Enabled = false;

            // Button Events
            RoadCreateButton.Activated += RoadCreateButton_Activated;
            RoadEditButton.Activated += RoadEditButton_Activated;
            RoadContinueButton.Activated += RoadContinueButton_Activated;
            RoadDeleteButton.Activated += RoadDeleteButton_Activated;
            RoadLoadBlipsButton.Activated += (s, e) =>
            {
                if (LoadRoadLocations(Locations.RoadSegments.Query()))
                {
                    RoadLoadBlipsButton.Enabled = false;
                    RoadClearBlipsButton.Enabled = true;
                }
            };
            RoadClearBlipsButton.Activated += (s, e) =>
            {
                RoadLoadBlipsButton.Enabled = true;
                RoadClearBlipsButton.Enabled = false;
                ClearZoneLocations();
            };

            // Add buttons
            RoadUIMenu.AddItem(RoadCreateButton);
            RoadUIMenu.AddItem(RoadContinueButton);
            RoadUIMenu.AddItem(RoadEditButton);
            RoadUIMenu.AddItem(RoadDeleteButton);
            RoadUIMenu.AddItem(RoadLoadBlipsButton);
            RoadUIMenu.AddItem(RoadClearBlipsButton);

            // Bind Buttons
            RoadUIMenu.BindMenuToItem(AddRoadUIMenu, RoadCreateButton);
            RoadUIMenu.BindMenuToItem(AddRoadUIMenu, RoadEditButton);

            // *************************************************
            // Add/Edit Road Segment UI Menu
            // *************************************************

            // Setup ADD NEW Buttons
            RoadSpeedButton = new UIMenuNumericScrollerItem<int>("Speed Limit", "Sets the speed limit of this road", 10, 80, 5);
            RoadLanesButton = new UIMenuNumericScrollerItem<int>("Lane Count", "Sets the number of lanes for this road segment", 1, 6, 1);
            RoadZoneButton = new UIMenuListItem("Zone", "Selects the zone for this location.");
            RoadFlagsButton = new UIMenuItem("Road Flags", "Open the Intersection flags menu.");
            RoadNextButton = new UIMenuItem("Continue", "Saves the current data for this Road Segment.");

            // Edit Buttons only
            RoadNodesButton = new UIMenuItem("Road Nodes", "Open the Road Nodes menu.");
            RoadShouldersButton = new UIMenuItem("Road Shoulders", "Open the Road Shoulders menu.");
            RoadDeleteConnectionsButton = new UIMenuItem("Delete Interconnections", "Deletes all intersection connections attached to this Road Segment.");
            RoadSaveButton = new UIMenuItem("Save", "Saves the changes to this RoadSegment.");

            // Set default indexes
            RoadLanesButton.Index = 0;

            // Button events
            RoadNextButton.Activated += RoadNextButton_Activated;
            RoadDeleteConnectionsButton.Activated += RoadDeleteConnectionsButton_Activated;

            // Add Buttons
            AddRoadUIMenu.AddItem(RoadSpeedButton);
            AddRoadUIMenu.AddItem(RoadLanesButton);
            AddRoadUIMenu.AddItem(RoadZoneButton);
            AddRoadUIMenu.AddItem(RoadFlagsButton);
            AddRoadUIMenu.AddItem(RoadNextButton);

            // Bind buttons
            AddRoadUIMenu.BindMenuToItem(RoadFlagsUIMenu, RoadFlagsButton);
            AddRoadUIMenu.BindMenuToItem(RoadRecordUIMenu, RoadNextButton);
            AddRoadUIMenu.BindMenuToItem(RoadNodeUIMenu, RoadNodesButton);

            // Add road shoulder flags list
            RoadFlagsItems = new Dictionary<RoadFlags, UIMenuCheckboxItem>();
            foreach (RoadFlags flag in Enum.GetValues(typeof(RoadFlags)))
            {
                var name = Enum.GetName(typeof(RoadFlags), flag);
                var cb = new UIMenuCheckboxItem(name, false, GetRoadFlagDesc(flag));
                RoadFlagsItems.Add(flag, cb);

                // Add button
                RoadFlagsUIMenu.AddItem(cb);
            }

            // *************************************************
            // Recording Road UI Menu
            // *************************************************

            // Setup buttons
            RoadAutoBreakButton = new UIMenuCheckboxItem("Auto Break", false, "If checked, whenever the zone or streetname is changed, the ~y~Break and Continue~w~ button will auto activate.");
            RoadEndButton = new UIMenuItem("End Segment", "Saves the current location to the database.");
            RoadBreakButton = new UIMenuItem("Break & Continue", "Ends the current Road Segment, and begins a new one connected by a Junction.");
            RoadConnectButton = new UIMenuItem("Connect Segment", "Ends the current Road Segment and connects it to the nearest RoadSegment via a Junction.");

            // Button events
            RoadEndButton.Activated += RoadEndButton_Activated;
            RoadBreakButton.Activated += (s, e) => { RoadBreakButton.Enabled = false; ForceBreak = true; };
            RoadDeleteConnectionsButton.Activated += RoadDeleteConnectionsButton_Activated;
            RoadSaveButton.Activated += RoadSaveButton_Activated;
            RoadConnectButton.Activated += RoadConnectButton_Activated;

            // Add Buttons
            RoadRecordUIMenu.AddItem(RoadAutoBreakButton);
            RoadRecordUIMenu.AddItem(RoadEndButton);
            RoadRecordUIMenu.AddItem(RoadBreakButton);
            RoadRecordUIMenu.AddItem(RoadConnectButton);

            // Set default
            RoadConnectButton.Enabled = false;

            // Register for events
            AddRoadUIMenu.OnMenuChange += AddRoadUIMenu_OnMenuChange;
            RoadFlagsUIMenu.OnMenuChange += RoadFlagsUIMenu_OnMenuChange;
        }

        #region Menu Events

        private void RoadFlagsUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of this menu?
            if (!forward && oldMenu == RoadFlagsUIMenu)
            {
                // We must have at least 1 item checked
                if (RoadFlagsItems.Any(x => x.Value.Checked))
                {
                    RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
                }
                else
                {
                    RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
                }
            }
        }

        private void AddRoadUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of a menu?
            if (!forward)
            {
                if (oldMenu == AddRoadUIMenu)
                {
                    Status = LocationUIStatus.None;

                    // Stop listening
                    if (ListeningForConnections)
                    {
                        ListeningForConnections = false;
                        ListenFiber?.Abort();
                        ListenFiber = null;
                    }

                    // Delete incomplete segments
                    if (ActiveSegment != null && ActiveSegment.EndPosition == Vector3.Zero)
                    {
                        // Delete the incomplete segment
                        Locations.RoadSegments.Delete(ActiveSegment.Id);
                        
                        // Remove junction as well
                        if (LastJunction != null && LastJunction.StartingSegment.Id == ActiveSegment.Id)
                        {
                            Locations.RoadJunctions.Delete(LastJunction.Id);
                        }

                        // Reset
                        ActiveSegment = null;
                        LastJunction = null;
                    }
                }
            }
        }

        #endregion Menu Events

        #region Control Events

        /// <summary>
        /// Method called when the "Create" menu item is clicked on the Road Segment menu.
        /// </summary>
        private void RoadCreateButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            //
            // Reset everything!
            //
            ActiveSegment = null;
            LastJunction = null;

            // Update description
            AddRoadUIMenu.SubtitleText = "Add Road Segment";

            // Grab player location
            var player = GetPlayer();
            var pos = player.Position;
            var cpPos = new Vector3(pos.X, pos.Y, pos.Z - ZCorrection);
            LastRoadLocation = new SpawnPoint(cpPos, player.Heading);

            // Reset road shoulder flags
            RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            foreach (var cb in RoadFlagsItems.Values)
            {
                cb.Checked = false;
            }

            // Add / Remove relevent buttons
            var index = AddRoadUIMenu.MenuItems.IndexOf(RoadSaveButton);
            if (index > 0)
            {
                AddRoadUIMenu.RemoveItemAt(index);
                AddRoadUIMenu.RemoveItem(RoadNodesButton);
                AddRoadUIMenu.RemoveItem(RoadShouldersButton);
                AddRoadUIMenu.RemoveItem(RoadDeleteConnectionsButton);

                AddRoadUIMenu.AddItem(RoadNextButton);
            }

            // Add Zones
            RoadZoneButton.Collection.Clear();
            RoadZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(pos));
            RoadZoneButton.Collection.Add("HIGHWAY");

            // Set street name default
            var streetName = GameWorld.GetStreetNameAtLocation(pos);
            var road = Locations.Roads.FindOne(x => x.Name.Equals(streetName));
            if (road == null)
            {
                road = new Road() { Name = streetName };
                var id = Locations.Roads.Insert(road);
                road.Id = id.AsInt32;
            }

            // Set active road name
            ActiveRoad = road;
            ActiveZone = GameWorld.GetZoneAtLocation(pos);

            // Enable/Disable buttons
            RoadNextButton.Enabled = true;
            RoadDeleteConnectionsButton.Enabled = false;

            // Flag
            Status = LocationUIStatus.Adding;
        }

        /// <summary>
        /// Method called when the "Edit" menu item is clicked on the Road Segment menu.
        /// </summary>
        private void RoadEditButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as RoadSegment;
            if (editingItem == null) return;

            // Reset
            ActiveSegment = null;
            LastJunction = null;

            // Update description
            AddRoadUIMenu.SubtitleText = "Edit Road Segment";

            // Enable/Disable buttons
            RoadNextButton.Enabled = false;
            RoadDeleteConnectionsButton.Enabled = true;

            // Add Zones
            RoadZoneButton.Collection.Clear();
            RoadZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(editingItem.Position));
            RoadZoneButton.Collection.Add("HIGHWAY");
            RoadZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(editingItem.EndPosition));
            var index = RoadZoneButton.Collection.IndexOf(editingItem.Zone.ScriptName);

            // Set values for the menu
            RoadSpeedButton.Value = editingItem.SpeedLimit;
            RoadLanesButton.Value = editingItem.LaneCount;
            RoadZoneButton.Index = Math.Abs(index);

            // Reset road flags
            foreach (var item in RoadFlagsItems)
            {
                item.Value.Checked = editingItem.Flags.Contains(item.Key);
            }

            // Are flags complete?
            if (RoadFlagsItems.Any(x => x.Value.Checked))
            {
                RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Add / Remove relevent buttons
            index = AddRoadUIMenu.MenuItems.IndexOf(RoadNextButton);
            if (index > 0)
            {
                AddRoadUIMenu.RemoveItemAt(index);

                AddRoadUIMenu.AddItem(RoadNodesButton);
                AddRoadUIMenu.AddItem(RoadShouldersButton);
                AddRoadUIMenu.AddItem(RoadDeleteConnectionsButton);
                AddRoadUIMenu.AddItem(RoadSaveButton);
            }

            // Cache all loaded locations
            HiddenCheckPoints = new List<Checkpoint>(ZoneCheckpoints.Count);
            foreach (var cp in ZoneCheckpoints)
            {
                HiddenCheckPoints.Add(cp.Key);
            }

            // Delete all checkpoints
            ClearZoneLocations();

            // Add a new start and end point
            CreateCheckpoint(editingItem.Position, editingItem.EndPosition, Color.Red, ROAD_START, editingItem);
            CreateCheckpoint(editingItem.EndPosition, Color.Red, ROAD_END, editingItem);

            // Add road nodes
            RecordEditSegment();

            // Flag
            Status = LocationUIStatus.Editing;
        }

        private void RoadSaveButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method called when the "Save" menu item is clicked on the Record Road Segment menu.
        /// </summary>
        private void RoadEndButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Ignore 
            if (ActiveSegment == null) return;

            // Create checkpoint at the player location
            var player = GetPlayer();
            CreateCheckpoint(player.Position, Color.Yellow, ROAD_END, ActiveSegment);

            // End the current segment
            EndAndUpdateCurrentSegment();

            // Go back twice
            RoadRecordUIMenu.GoBack();
            AddRoadUIMenu.GoBack();
        }

        /// <summary>
        /// Method called when the "Delete" menu item is clicked on the Road Segment menu.
        /// </summary>
        private void RoadDeleteButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as RoadSegment;
            if (editingItem == null) return;

            // Disable buttons
            DisableAllEditButtons();

            // Prevent app crashing
            try
            {
                // Need to delete all connections
                var junctionsToRemove = editingItem.GetJunctions();
                foreach (var junc in junctionsToRemove)
                {
                    Locations.RoadJunctions.Delete(junc.Id);
                }

                // Need to remove all inter connections
                var interToRemove = editingItem.GetInterconnections();
                foreach (var inter in interToRemove)
                {
                    Locations.Intersections.Delete(inter.Id);
                }

                // Delete location
                if (Locations.RoadSegments.Delete(editingItem.Id))
                {
                    // Delete checkpoint
                    DeleteBlipAndCheckpoint(LocationCheckpoint);
                    LocationCheckpoint = null;

                    // Notify the user
                    ShowNotification("Delete Road Segment", $"~g~Location deleted.");
                }
                else
                {
                    // Display notification to the player
                    ShowNotification("Delete Road Segment", $"~o~Unable to delete location from database.");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Delete Road Segment", $"~r~Action Failed: ~o~Please check your Game.log!");
            }
        }

        /// <summary>
        /// Method called when the "Delete Connections" menu item is clicked on the Edit Road Segment menu.
        /// </summary>
        private void RoadDeleteConnectionsButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            
        }

        /// <summary>
        /// Method called when the "Next" menu item is clicked on the Add Road Segment menu.
        /// </summary>
        private void RoadNextButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            if (!ListeningForConnections)
            {
                // Start watching the players movements
                ListeningForConnections = true;
                RecordRoadSegment();
            }

            if (ActiveSegment == null)
            {
                // Create a new segment
                AddOrUpdateSegment();

                // Create checkpoint at the player location
                var player = GetPlayer();
                CreateCheckpoint(LastRoadLocation, player.FrontPosition, Color.Red, ROAD_START, ActiveSegment);
            }
        }

        /// <summary>
        /// Method called when the "Connect Road Segment" menu item is clicked.
        /// </summary>
        private void RoadConnectButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Ensure we have an object
            if (ClosestRoadSegment == null) return;

            // End the current connection
            var currentSeg = ActiveSegment;
            EndAndUpdateCurrentSegment();

            // Make the connection
            try
            {
                // Create item, and insert it into the database
                var item = new RoadJunction()
                {
                    EndingSegment = currentSeg,
                    StartingSegment = ClosestRoadSegment
                };
                Locations.RoadJunctions.Insert(item);

                // Update the closest checkpoint
                var cp = ClosestCheckPoint;
                if (cp != null)
                {
                    ReplaceCheckPointAtLocation(cp, Color.Green, ROAD_CONN);
                }

                // Delete the old
                if (LocationCheckpoint != null)
                {
                    DeleteBlipAndCheckpoint(LocationCheckpoint);
                }

                // Create a new starting checkpoint
                CreateCheckpoint(currentSeg.Position, Color.Green, ROAD_CONN, currentSeg);

                // Set new
                LocationCheckpoint = null;
                Status = LocationUIStatus.None;

                // Go back two menus
                RoadRecordUIMenu.GoBack();
                AddRoadUIMenu.GoBack();
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Road Segment", $"~r~Junction Failed: ~o~Please check your Game.log!");
            }
        }

        /// <summary>
        /// Method called when the "Continue Road Segment" menu item is clicked.
        /// </summary>
        private void RoadContinueButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as RoadSegment;
            if (editingItem == null) return;

            // Grab player location
            var pos = editingItem.EndPosition;
            LastRoadLocation = new SpawnPoint(pos, editingItem.EndHeading);

            // Set street name default
            var streetName = GameWorld.GetStreetNameAtLocation(pos);
            var road = Locations.Roads.FindOne(x => x.Name.Equals(streetName));
            if (road == null)
            {
                road = new Road() { Name = streetName };
                var id = Locations.Roads.Insert(road);
                road.Id = id.AsInt32;
            }

            // Set active road name
            ActiveRoad = road;
            ActiveZone = GameWorld.GetZoneAtLocation(pos);

            // Start watching the players movements
            ListeningForConnections = true;
            RecordRoadSegment();

            // Create a new segment here, this way the "Next" button doesnt create a checkpoint
            AddOrUpdateSegment();

            // Make the connection
            try
            {
                // Create item, and insert it into the database
                var item = new RoadJunction()
                {
                    EndingSegment = editingItem,
                    StartingSegment = ActiveSegment
                };
                var id = Locations.RoadJunctions.Insert(item);
                item.Id = id.AsInt32;

                // Store this so we can delete if incomplete
                LastJunction = item;
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Road Segment", $"~r~Junction Failed: ~o~Please check your Game.log!");
                return;
            }

            // Update description
            AddRoadUIMenu.SubtitleText = "Add Road Segment";

            // Reset road shoulder flags
            RoadFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            foreach (var cb in RoadFlagsItems.Values)
            {
                cb.Checked = false;
            }

            // Add Zones
            RoadZoneButton.Collection.Clear();
            RoadZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(pos));
            RoadZoneButton.Collection.Add("HIGHWAY");

            // Enable/Disable buttons
            RoadNextButton.Enabled = true;

            // Flag
            Status = LocationUIStatus.Continue;

            // Hide this menu
            RoadUIMenu.Visible = false;
            AddRoadUIMenu.Visible = true;
        }

        #endregion Control Events

        /// <summary>
        /// Creates or updates an active <see cref="RoadSegment"/> with the selected
        /// data provided by the <see cref="AddRoadUIMenu"/>
        /// </summary>
        private void AddOrUpdateSegment()
        {
            // Create a new road segment
            if (ActiveSegment == null)
            {
                var seg = new RoadSegment()
                {
                    EndPosition = Vector3.Zero,
                    Position = LastRoadLocation,
                    Zone = ActiveZone,
                    StartHeading = LastRoadLocation.Heading,
                    LaneCount = RoadLanesButton.Value,
                    Road = ActiveRoad,
                    SpeedLimit = RoadSpeedButton.Value,
                    Flags = RoadFlagsItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList(),
                };

                // Insert the new segment into the database
                var id = Locations.RoadSegments.Insert(seg);
                seg.Id = id.AsInt32;

                // Set as active road segment
                ActiveSegment = seg;
            }
            else
            {
                // Get active zone
                var zone = Locations.WorldZones.FindOne(x => x.ScriptName == RoadZoneButton.SelectedItem.DisplayText);
                
                // Update
                ActiveSegment.Zone = zone;
                ActiveSegment.LaneCount = RoadLanesButton.Value;
                ActiveSegment.SpeedLimit = RoadSpeedButton.Value;
                ActiveSegment.Flags = RoadFlagsItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList();
                ActiveSegment.Road = ActiveRoad;
            }
        }

        /// <summary>
        /// Ends and saves the active <see cref="RoadSegment"/>
        /// </summary>
        private void EndAndUpdateCurrentSegment()
        {
            // Ignore 
            if (ActiveSegment == null) return;

            // Save heading and position
            var player = GetPlayer();
            ActiveSegment.EndPosition = player.Position;
            ActiveSegment.EndHeading = player.Heading;
            ActiveSegment.Length = ActiveSegment.Position.TravelDistanceTo(player.Position);

            // Save
            Locations.RoadSegments.Update(ActiveSegment);

            // Null
            ActiveSegment = null;
        }

        /// <summary>
        /// Ends the current active <see cref="RoadSegment"/> and begins a new one.
        /// A <see cref="RoadJunction"/> will be created to join the two <see cref="RoadSegment"/>s
        /// together.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="streetName"></param>
        /// <param name="zoneName"></param>
        private void AddRoadBreak(Vector3 position, string streetName, string zoneName)
        {
            // Grab player location
            var player = GetPlayer();
            var pos = player.Position;
            var cpPos = new Vector3(pos.X, pos.Y, pos.Z - ZCorrection);
            LastRoadLocation = new SpawnPoint(cpPos, player.Heading);

            // Save the current segment
            var currentSeg = ActiveSegment;
            EndAndUpdateCurrentSegment();

            // Delete old handle
            if (LocationCheckpoint != null)
            {
                LocationCheckpoint.Delete();
                LocationCheckpoint = null;
            }

            // Add Zones
            var index = RoadZoneButton.Index;
            RoadZoneButton.Collection.Clear();
            RoadZoneButton.Collection.Add(zoneName);
            RoadZoneButton.Collection.Add("HIGHWAY");
            RoadZoneButton.Index = index;

            // Ensure road is in the database!
            var road = Locations.Roads.FindOne(x => x.Name.Equals(streetName));
            if (road == null)
            {
                road = new Road() { Name = streetName };
                var id = Locations.Roads.Insert(road);
                road.Id = id.AsInt32;
            }

            // Set active road name
            ActiveRoad = road;
            ActiveZone = Locations.WorldZones.FindOne(x => x.ScriptName == zoneName);

            // Start a new segment
            AddOrUpdateSegment();

            // Create junction
            try
            {
                var item = new RoadJunction()
                {
                    EndingSegment = currentSeg,
                    StartingSegment = ActiveSegment
                };

                var id = Locations.RoadJunctions.Insert(item);
                item.Id = id.AsInt32;
                LastJunction = item;

                // Create checkpoint at the player location
                CreateCheckpoint(position, player.FrontPosition, Color.Green, ROAD_CONN, ActiveSegment);

                // Notify the user
                ShowNotification("Road Segment", $"The road segment has ended and a new one has started.");
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Road Segment", $"~r~Junction Failed: ~o~Please check your Game.log!");
            }
        }

        /// <summary>
        /// This method watches the player while moving from a start point
        /// of a Road Segment to an end point. If the player transitions zones
        /// or a different street name, then this method will end and save the
        /// current <see cref="RoadSegment"/>
        /// </summary>
        private void RecordRoadSegment()
        {
            // Need to create a new fiber
            ListenFiber = GameFiber.StartNew(() =>
            {
                // Main loop
                while (ListeningForConnections)
                {
                    // Be nice and prevent locking up
                    GameFiber.Sleep(250);

                    // Max players top speed
                    var player = GetPlayer();
                    if (player.IsInAnyVehicle(false))
                    {
                        // 
                        player.CurrentVehicle.TopSpeed = 20f;
                    }

                    // Get player position
                    player = GetPlayer();
                    var playerPos = player.Position;
                    var streetName = GameWorld.GetStreetNameAtLocation(playerPos);
                    var zoneName = GameWorld.GetZoneNameAtLocation(playerPos);

                    // Check
                    if (!ListeningForConnections) break;

                    // Player requested a break?
                    if (ForceBreak)
                    {
                        // Disable flag
                        ForceBreak = false;

                        // Break and continue
                        AddRoadBreak(playerPos, streetName, zoneName);

                        // Re enable the button
                        RoadBreakButton.Enabled = true;
                    }

                    // Check for street name changes
                    else if (streetName != ActiveRoad.Name)
                    {
                        // Wait breifly
                        GameFiber.Sleep(250);

                        // Auto break?
                        if (RoadAutoBreakButton.Checked)
                        {
                            // Break and continue
                            AddRoadBreak(playerPos, streetName, zoneName);
                        }
                        else
                        {
                            // Stop here
                            RoadEndButton_Activated(null, null);
                        }
                    }

                    // Check for zone name changes
                    else if (zoneName != ActiveZone.ScriptName)
                    {
                        if (!RoadZoneButton.SelectedItem.DisplayText.Equals("HIGHWAY"))
                        {
                            // Wait breifly
                            GameFiber.Sleep(250);

                            // Auto break?
                            if (RoadAutoBreakButton.Checked)
                            {
                                // Break and continue
                                AddRoadBreak(playerPos, streetName, zoneName);
                            }
                            else
                            {
                                // Stop here
                                RoadEndButton_Activated(null, null);
                            }
                        }
                    }

                    // Check
                    if (!ListeningForConnections) break;

                    // Allow editing of the closest point
                    ClosestCheckPoint = GetClosestCheckPoint(2f);
                    if (ClosestCheckPoint != null)
                    {
                        // Grab location from the checkpoint
                        var location = ClosestCheckPoint.Tag as RoadSegment;
                        if (location == null) continue;

                        if (ClosestRoadSegment == null || ClosestRoadSegment.Id != location.Id)
                        {
                            // Enable buttons
                            if (ClosestCheckPoint.CheckpointType == ROAD_START)
                            {
                                // Get junctions
                                var junctions = location.GetJunctions();
                                var canConnect = !junctions.Any(x => x.StartingSegment.Id == location.Id);

                                ClosestRoadSegment = location;
                                RoadConnectButton.Enabled = canConnect;
                            }
                        }
                    }
                    else
                    {
                        ClosestRoadSegment = null;
                        RoadConnectButton.Enabled = false;
                    }
                }
            });
        }

        private void RecordEditSegment()
        {
            
        }

        /// <summary>
        /// Loads all <see cref="RoadSegment"/> locations on the map and into the world via
        /// <see cref="Blip"/>s and <see cref="Checkpoint"/>s
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        private bool LoadRoadLocations(ILiteQueryable<RoadSegment> queryable, bool clear = true)
        {
            // Clear old shit
            if (clear) ClearZoneLocations();
            LoadedBlipsLocationType = LocationTypeCode.RoadSegment;

            // Get players current zone name
            var pos = Rage.Game.LocalPlayer.Character.Position;
            var zoneName = GameWorld.GetZoneNameAtLocation(pos);
            var enumName = typeof(RoadSegment).Name;
            RoadSegment otherSeg = null;

            // Grab zone
            var zone = Locations.WorldZones.FindOne(x => x.ScriptName.Equals(zoneName));
            if (zone == null)
            {
                // Display notification to the player
                ShowNotification("Load Zone Locations", $"~r~Failed: ~o~Unable to find WorldZone ~y~{zoneName} ~o~in the locations database!");
                return false;
            }

            // Now grab locations
            var items = queryable.Include(x => x.Zone).Where(x => x.Zone.Id == zone.Id).ToArray();
            if (items == null || items.Length == 0)
            {
                // Display notification to the player
                ShowNotification("Load Zone Locations", $"There are no {enumName} locations in the database");
                return false;
            }

            // Add each location as a checkpoint and blip
            foreach (RoadSegment roadSeg in items)
            {
                // === START

                // Get all road nodes attached
                var nodes = roadSeg.GetRoadNodes();

                // Get starting color
                Color color = (nodes.Length == 0) ? Color.Yellow : Color.Green;
                var hasConnection = roadSeg.TryGetInterconnections(out var connections);

                // Is the start of this road connected to anything?
                if (!connections.Any(x => !x.IsRoadEntering) && !roadSeg.TryGetPreviousSegment(out otherSeg))
                {
                    color = Color.Red;
                }

                // Create the checkpoint in the game world
                CreateCheckpoint(roadSeg.Position, roadSeg.EndPosition, color, ROAD_START, roadSeg);

                // === END

                // Is the end of this road connected to anything?
                hasConnection = connections.Any(x => x.IsRoadEntering);
                if (!hasConnection && !roadSeg.TryGetNextSegment(out otherSeg))
                {
                    // Create the checkpoint in the game world
                    CreateCheckpoint(roadSeg.EndPosition, Color.Red, ROAD_END, roadSeg);
                }
                else
                {
                    color = (nodes.Length == 0) ? Color.Yellow : Color.Green;

                    // We have a connected segment
                    if (!hasConnection)
                    {
                        // Did we change zones?
                        if (otherSeg.Zone.Id != roadSeg.Zone.Id)
                        {
                            // Create the checkpoint in the game world
                            CreateCheckpoint(roadSeg.EndPosition, color, ROAD_CONN, roadSeg);
                        }
                    }
                    else
                    {
                        // Did we change zones?
                        var conn = connections.Where(x => x.IsRoadEntering).First();
                        if (conn.Intersection.Zone.Id != roadSeg.Zone.Id)
                        {
                            // Create the checkpoint in the game world
                            CreateCheckpoint(roadSeg.EndPosition, color, ROAD_CONN, roadSeg);
                        }
                    }
                }

                // === RoadNodes
                int lastIndex = nodes.Length - 1;
                for (int i = 0; i < nodes.Length; i++)
                {
                    Vector3 point = roadSeg.EndPosition;
                    var node = nodes[i];
                    if (i != lastIndex)
                    {
                        point = nodes[i + 1].Position;
                    }

                    CreateCheckpoint(node.Position, point, Color.Blue, 32, node);
                }
            }

            // Flag
            ShowingZoneLocations = true;
            return true;
        }
    }
}
