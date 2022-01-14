using BlueLightSoftware.Common.Game;
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
        private UIMenuItem IntersectionStreetButton;
        private UIMenuItem IntersectionHintButton;
        private UIMenuListItem IntersectionZoneButton;
        private UIMenuItem IntersectionFlagsButton;
        private UIMenuItem IntersectionConnectButton;
        private UIMenuItem IntersectionSaveButton;
        private Dictionary<IntersectionFlags, UIMenuCheckboxItem> IntersectionFlagsItems;
        private SpawnPoint IntersectionLocation;
        private UIMenuCheckboxItem IntersectionEnteringButton;
        private UIMenuItem DoConnectIntersectionButton;

        private List<Checkpoint> RoadCheckpoints;
        private Checkpoint ClosestRoadCheckpoint;
        private bool ListeningForConnections = false;

        /// <summary>
        /// Builds the menu and its buttons
        /// </summary>
        private void BuildIntersectionMenu()
        {
            // Create road shoulder ui menu
            IntersectionUIMenu = new UIMenu(MENU_NAME, "~b~Intersections Menu")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create road shoulder ui menu
            AddIntersectionUIMenu = new UIMenu(MENU_NAME, "~b~Add Intersection")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create Intersection flags menu
            IntersectionFlagsUIMenu = new UIMenu(MENU_NAME, "~b~Intersection Flags")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // Create road shoulder ui menu
            IntersectionConnectUIMenu = new UIMenu(MENU_NAME, "~b~Conenct Segment Menu")
            {
                MouseControlsEnabled = false,
                AllowCameraMovement = true,
                WidthOffset = 12
            };

            // *************************************************
            // Intersection UI Menu
            // *************************************************

            // Setup buttons
            IntersectionCreateButton = new UIMenuItem("Add New Location", "Creates a new Intersection location where you are currently");
            IntersectionEditButton = new UIMenuItem("Edit Location", "Edit an Intersection location that you are currently near. ~y~Blips must be loaded");
            IntersectionDeleteButton = new UIMenuItem("Delete Location", "Delete an Intersection location that you are currently near. ~y~Blips must be loaded");
            IntersectionLoadBlipsButton = new UIMenuItem("Load Checkpoints", "Loads checkpoints in the world as well as blips on the map to show all saved locations in this zone");
            IntersectionClearBlipsButton = new UIMenuItem("Clear Checkpoints", "Clears all checkpoints and blips loaded by the ~y~Load Checkpoints ~w~option");

            // Disable buttons by default
            IntersectionEditButton.Enabled = false;
            IntersectionDeleteButton.Enabled = false;
            IntersectionClearBlipsButton.Enabled = false;

            // Button Events
            IntersectionCreateButton.Activated += IntersectionCreateButton_Activated;
            IntersectionEditButton.Activated += IntersectionEditButton_Activated;
            IntersectionDeleteButton.Activated += IntersectionDeleteButton_Activated;
            IntersectionLoadBlipsButton.Activated += (s, e) =>
            {
                if (LoadZoneLocations(Locations.Intersections.Query(), Color.Red, LocationTypeCode.Intersection))
                {
                    IntersectionLoadBlipsButton.Enabled = false;
                    IntersectionClearBlipsButton.Enabled = true;
                }
            };
            IntersectionClearBlipsButton.Activated += (s, e) =>
            {
                IntersectionLoadBlipsButton.Enabled = true;
                IntersectionClearBlipsButton.Enabled = false;
                ClearZoneLocations();
            };

            // Add buttons
            IntersectionUIMenu.AddItem(IntersectionCreateButton);
            IntersectionUIMenu.AddItem(IntersectionEditButton);
            IntersectionUIMenu.AddItem(IntersectionDeleteButton);
            IntersectionUIMenu.AddItem(IntersectionLoadBlipsButton);
            IntersectionUIMenu.AddItem(IntersectionClearBlipsButton);

            // Bind Buttons
            IntersectionUIMenu.BindMenuToItem(AddIntersectionUIMenu, IntersectionCreateButton);
            IntersectionUIMenu.BindMenuToItem(AddIntersectionUIMenu, IntersectionEditButton);

            // *************************************************
            // Add/Edit Intersection UI Menu
            // *************************************************

            // Setup Buttons
            IntersectionStreetButton = new UIMenuItem("Intersection Name", "");
            IntersectionHintButton = new UIMenuItem("Location Hint", "");
            IntersectionZoneButton = new UIMenuListItem("Zone", "Selects the zone for this location.");
            IntersectionFlagsButton = new UIMenuItem("Intersection Flags", "Open the Intersection flags menu.");
            IntersectionConnectButton = new UIMenuItem("Make Connection", "Connects this intersection to a road segment");
            IntersectionSaveButton = new UIMenuItem("Save", "Saves the current location to the database.");

            // Button events
            IntersectionStreetButton.Activated += DispayKeyboard_SetDescription;
            IntersectionHintButton.Activated += DispayKeyboard_SetDescription;
            IntersectionSaveButton.Activated += IntersectionSaveButton_Activated;
            IntersectionConnectButton.Activated += IntersectionConnectionButton_Activated;

            // Add Buttons
            AddIntersectionUIMenu.AddItem(IntersectionStreetButton);
            AddIntersectionUIMenu.AddItem(IntersectionHintButton);
            AddIntersectionUIMenu.AddItem(IntersectionZoneButton);
            AddIntersectionUIMenu.AddItem(IntersectionFlagsButton);
            AddIntersectionUIMenu.AddItem(IntersectionConnectButton);
            AddIntersectionUIMenu.AddItem(IntersectionSaveButton);

            // Bind buttons
            AddIntersectionUIMenu.BindMenuToItem(IntersectionFlagsUIMenu, IntersectionFlagsButton);
            AddIntersectionUIMenu.BindMenuToItem(IntersectionConnectUIMenu, IntersectionConnectButton);

            // Add road shoulder flags list
            IntersectionFlagsItems = new Dictionary<IntersectionFlags, UIMenuCheckboxItem>();
            foreach (IntersectionFlags flag in Enum.GetValues(typeof(IntersectionFlags)))
            {
                var name = Enum.GetName(typeof(IntersectionFlags), flag);
                var cb = new UIMenuCheckboxItem(name, false, GetIntersectionFlagDesc(flag));
                IntersectionFlagsItems.Add(flag, cb);

                // Add button
                IntersectionFlagsUIMenu.AddItem(cb);
            }

            // *************************************************
            // Connect Intersection UI Menu
            // *************************************************

            // Setup Buttons
            IntersectionEnteringButton = new UIMenuCheckboxItem("Entering", false, "Check this box if the Road Segment is ~y~entering ~w~the intersection");
            DoConnectIntersectionButton = new UIMenuItem("Connect", "Connects the nearest Road Segment to this Intersection");

            // Button events
            DoConnectIntersectionButton.Activated += DoConnectIntersectionButton_Activated;

            // Add Buttons
            IntersectionConnectUIMenu.AddItem(IntersectionEnteringButton);
            IntersectionConnectUIMenu.AddItem(DoConnectIntersectionButton);

            // Register for events
            AddIntersectionUIMenu.OnMenuChange += AddIntersectionUIMenu_OnMenuChange;
            IntersectionFlagsUIMenu.OnMenuChange += IntersectionFlagsUIMenu_OnMenuChange;
            IntersectionConnectUIMenu.OnMenuChange += IntersectionConnectUIMenu_OnMenuChange;

            // Init fields
            RoadCheckpoints = new List<Checkpoint>();
        }

        #region Menu Events

        private void IntersectionFlagsUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of this menu?
            if (!forward && oldMenu == IntersectionFlagsUIMenu)
            {
                // We must have at least 1 item checked
                if (IntersectionFlagsItems.Any(x => x.Value.Checked))
                {
                    IntersectionFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
                }
                else
                {
                    IntersectionFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
                }
            }
        }

        private void AddIntersectionUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of a menu?
            if (!forward)
            {
                if (oldMenu == AddIntersectionUIMenu)
                {
                    ResetCheckPoints();
                    Status = LocationUIStatus.None;

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

        private void IntersectionConnectUIMenu_OnMenuChange(UIMenu oldMenu, UIMenu newMenu, bool forward)
        {
            // Are we backing out of this menu?
            if (!forward)
            {
                if (oldMenu == IntersectionConnectUIMenu)
                {
                    // Stop listening
                    if (ListeningForConnections)
                    {
                        ListeningForConnections = false;
                        ListenFiber?.Abort();
                        ListenFiber = null;
                    }

                    ClearRoadCheckpoints();
                }
            }
        }

        #endregion Menu Events

        #region Control Events

        /// <summary>
        /// Method called when the "Create New Intersection" button is clicked.
        /// Clears all prior data.
        /// </summary>
        private void IntersectionCreateButton_Activated(UIMenu sender, UIMenuItem selectedItem)
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
            AddIntersectionUIMenu.SubtitleText = "Add Intersection";

            // Grab player location
            var player = GetPlayer();
            var pos = player.Position;
            var cpPos = new Vector3(pos.X, pos.Y, pos.Z - ZCorrection);
            IntersectionLocation = new SpawnPoint(cpPos, player.Heading);

            // Reset Intersection flags
            foreach (var cb in IntersectionFlagsItems.Values)
            {
                cb.Checked = false;
            }

            // Create checkpoint at the player location
            LocationCheckpoint = Checkpoint.Create(cpPos, Color.Red);

            // Add Zones
            IntersectionZoneButton.Collection.Clear();
            IntersectionZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(pos));
            IntersectionZoneButton.Collection.Add("HIGHWAY");

            // Set street name default
            var streetName = GameWorld.GetStreetNameAtLocation(pos, out string crossingRoad);
            if (!String.IsNullOrEmpty(streetName))
            {
                IntersectionStreetButton.Description = streetName;
                IntersectionStreetButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                IntersectionStreetButton.Description = "";
                IntersectionStreetButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Crossing road
            if (!String.IsNullOrEmpty(crossingRoad))
            {
                IntersectionStreetButton.Description += $" and {crossingRoad}";
            }

            // Reset
            IntersectionFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;

            // Enable/Disable buttons
            IntersectionSaveButton.Enabled = true;
            IntersectionConnectButton.Enabled = false;

            // Flag
            Status = LocationUIStatus.Adding;
        }

        /// <summary>
        /// Method called when the "Edit Intersection" menu item is clicked.
        /// </summary>
        private void IntersectionEditButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as Intersection;
            if (editingItem == null) return;

            // Reset road shoulder flags
            foreach (var item in IntersectionFlagsItems)
            {
                item.Value.Checked = editingItem.Flags.Contains(item.Key);
            }

            // Are flags complete?
            if (IntersectionFlagsItems.Any(x => x.Value.Checked))
            {
                IntersectionFlagsButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                IntersectionFlagsButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Add Zones
            IntersectionZoneButton.Collection.Clear();
            IntersectionZoneButton.Collection.Add(GameWorld.GetZoneNameAtLocation(editingItem.Position));
            IntersectionZoneButton.Collection.Add("HIGHWAY");
            var index = IntersectionZoneButton.Collection.IndexOf(editingItem.Zone.ScriptName);
            if (index >= 0)
                IntersectionZoneButton.Index = index;

            // Set street name default
            if (!String.IsNullOrEmpty(editingItem.StreetName))
            {
                IntersectionStreetButton.Description = editingItem.StreetName;
                IntersectionStreetButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                IntersectionStreetButton.Description = "";
                IntersectionStreetButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Crossing road
            if (!String.IsNullOrEmpty(editingItem.Hint))
            {
                IntersectionHintButton.Description = editingItem.Hint;
                IntersectionHintButton.RightBadge = UIMenuItem.BadgeStyle.Tick;
            }
            else
            {
                // Reset ticks
                IntersectionHintButton.RightBadge = UIMenuItem.BadgeStyle.None;
            }

            // Enable/Disable buttons
            IntersectionSaveButton.Enabled = true;
            IntersectionConnectButton.Enabled = true;

            // Update description
            AddIntersectionUIMenu.SubtitleText = "Editing Intersection";

            // Flag
            IntersectionLocation = new SpawnPoint(editingItem.Position, 0f);
            Status = LocationUIStatus.Editing;
        }

        /// <summary>
        /// Method called when the "Save" button is clicked on the Intersection UI menu
        /// </summary>
        private void IntersectionSaveButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Disable button to prevent spam clicking!
            IntersectionSaveButton.Enabled = false;
            var heading = (Status == LocationUIStatus.Adding) ? "Add" : "Edit";

            // Ensure everything is done
            var requiredItems = new[] { IntersectionStreetButton, IntersectionFlagsButton };
            foreach (var item in requiredItems)
            {
                if (item.RightBadge != UIMenuItem.BadgeStyle.Tick)
                {
                    // Display notification to the player
                    ShowNotification($"{heading} Intersection", $"~o~Location does not have all required parameters set");
                    IntersectionSaveButton.Enabled = true;
                    return;
                }
            }

            // Be nice and prevent locking up
            GameFiber.Yield();

            // Grab zone instance
            string zoneName = IntersectionZoneButton.SelectedValue.ToString();
            var zone = Locations.WorldZones.FindOne(x => x.ScriptName == zoneName);
            if (zone == null)
            {
                // Display notification to the player
                ShowNotification($"{heading} Intersection", $"~r~Save Failed: ~o~Unable to find zone in the locations database!");
                return;
            }

            // Wrap to prevent crashing
            try
            {
                // Create new shoulder object
                var item = new Intersection()
                {
                    Position = IntersectionLocation.Position,
                    StreetName = IntersectionStreetButton.Description,
                    Hint = IntersectionHintButton.Description,
                    Flags = IntersectionFlagsItems.Where(x => x.Value.Checked).Select(x => x.Key).ToList(),
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
                    var editingItem = LocationCheckpoint.Tag as Intersection;
                    if (editingItem == null) return;

                    // Set id for update
                    item.Id = editingItem.Id;

                    // Save location in the database
                    Locations.Intersections.Update(item);
                }
                else
                {
                    // Save location in the database
                    var id = Locations.Intersections.Insert(item);
                    item.Id = id.AsInt32;
                }

                // Editing
                LocationCheckpoint.Tag = item;

                // Display notification to the player
                ShowNotification($"~b~{heading} Intersection.", $"~g~Location saved Successfully.");
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification($"~b~{heading} Road Shoulder.", $"~r~Save Failed: ~o~Please check your Game.log!");
            }

            // Go back
            AddIntersectionUIMenu.GoBack();

            // Are we currently showing checkpoints and blips?
            if (Status == LocationUIStatus.Adding && ShowingZoneLocations)
            {
                var pos = IntersectionLocation.Position;

                // Create checkpint and blips
                var blip = new Blip(pos) { Color = Color.Red };
                LocationCheckpoint = Checkpoint.Create(pos, Color.Red, forceGround: true);
                ZoneCheckpoints.Add(LocationCheckpoint, blip);
            }
        }

        /// <summary>
        /// Method called when the "Delete Intersection" menu item is clicked.
        /// </summary>
        private void IntersectionDeleteButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as Intersection;
            if (editingItem == null) return;

            // Disable buttons
            DisableAllEditButtons();

            // Prevent app crashing
            try
            {
                // Delete location
                if (Locations.Intersections.Delete(editingItem.Id))
                {
                    // Delete checkpoint
                    DeleteBlipAndCheckpoint(LocationCheckpoint);
                    LocationCheckpoint = null;

                    // Notify the user
                    ShowNotification("Delete Intersection", $"~g~Location deleted.");
                }
                else
                {
                    // Display notification to the player
                    ShowNotification("Delete Intersection", $"~o~Unable to delete location from database.");
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Delete Intersection", $"~r~Action Failed: ~o~Please check your Game.log!");
            }
        }

        /// <summary>
        /// Method called when the "Make Connection" button is clicked in the IntersectionConnectUIMenu
        /// </summary>
        private void IntersectionConnectionButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab item
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as Intersection;
            if (editingItem == null) return;

            // Disable the connect button
            DoConnectIntersectionButton.Enabled = false;
            ListeningForConnections = true;

            // Need to create a new fiber
            ListenFiber = GameFiber.StartNew(() => 
            {
                // First, load ALL the closest Road Segments. This is a full database scan
                var segs = Locations.RoadSegments.Include(x => x.Zone).Find(x => x.Zone.Id == editingItem.Zone.Id).ToArray();

                // Be nice and prevent locking up
                GameFiber.Yield();

                // Init variables
                Color color = Color.AliceBlue;
                int type = 47;
                bool hasConnection;

                // Create checkpoints
                foreach (RoadSegment s in segs)
                {
                    var startDist = s.Position.DistanceTo(editingItem.Position);
                    var endDist = s.EndPosition.DistanceTo(editingItem.Position);
                    var dist = Math.Min(startDist, endDist);

                    // Skip any road segments that are far away
                    if (dist > 30f) continue;

                    // Grab the closest position for this road segment to the intersection
                    var pos = (startDist == dist) ? s.Position : s.EndPosition;

                    // Skip if the road connection if on an over/under pass.
                    var zDiff = Math.Abs(editingItem.Position.Z - pos.Z);
                    if (zDiff > 5f) continue;

                    // Check if this road segment has a connection already!
                    var connections = s.GetInterconnections();
                    hasConnection = (s.Position.DistanceTo(editingItem.Position) < 50f) 
                        ? connections.Any(x => !x.IsRoadEntering) 
                        : connections.Any(x => x.IsRoadEntering);

                    // Create the checkpoint
                    color = (hasConnection) ? Color.Green : Color.Red;
                    type = (hasConnection) ? ROAD_CONN : ROAD_BROKEN;
                    var cp = Checkpoint.Create(pos, color, type: type);
                    cp.Tag = s;

                    // Check for a cancellation
                    if (!ListeningForConnections) break;

                    // Add to the list
                    RoadCheckpoints.Add(cp);

                    // Be nice and prevent locking up
                    GameFiber.Yield();
                }

                // Main loop
                while (ListeningForConnections)
                {
                    // Be nice and prevent locking up
                    GameFiber.Yield();

                    // Get player position
                    var playerPos = GetPlayer().Position;

                    // Check if close to a checkpoint
                    var closestPoint = (
                        from x in RoadCheckpoints
                        let distance = x.Position.DistanceTo(playerPos)
                        where distance < 4f
                        orderby distance
                        select x).FirstOrDefault();

                    // Disable the connect button
                    DoConnectIntersectionButton.Enabled = closestPoint != null;
                    ClosestRoadCheckpoint = closestPoint;
                }
            });
        }

        /// <summary>
        /// Method called when the "Connect" button is clicked in the IntersectionConnectUIMenu
        /// </summary>
        private void DoConnectIntersectionButton_Activated(UIMenu sender, UIMenuItem selectedItem)
        {
            // Grab intersection
            if (LocationCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var editingItem = LocationCheckpoint.Tag as Intersection;
            if (editingItem == null) return;

            // Grab the road segment
            if (ClosestRoadCheckpoint?.Tag == null) return;

            // Ensure tag is set properly
            var roadSeg = ClosestRoadCheckpoint.Tag as RoadSegment;
            if (roadSeg == null) return;

            // Check for existing connection
            if (ClosestRoadCheckpoint.Color == Color.Green)
            {
                // Display notification to the player
                ShowNotification("Connect Intersection", $"~o~Action Failed: ~y~Intersection already has a connection!");
                return;
            }

            // Stop listening
            ListeningForConnections = false;
            ListenFiber.Abort();
            ListenFiber = null;

            // Wrap this in a try/catch to catch exceptions
            try
            {
                // Create our connection object
                var item = new Interconnection()
                {
                    IsRoadEntering = IntersectionEnteringButton.Checked,
                    Intersection = editingItem,
                    RoadSegment = roadSeg,
                    Position = ClosestRoadCheckpoint.Position
                };

                // Insert
                // Save location in the database
                Locations.Interconnections.Insert(item);
            }
            catch (Exception e)
            {
                Log.Exception(e);

                // Display notification to the player
                ShowNotification("Connect Intersection", $"~r~Action Failed: ~o~Please check your Game.log!");
            }

            // Finally, go back to the previous menu
            IntersectionConnectUIMenu.GoBack();
        }

        #endregion Control Events

        /// <summary>
        /// Resets all RoadSegment checkpoints
        /// </summary>
        private void ClearRoadCheckpoints()
        {
            foreach (var cp in RoadCheckpoints)
            {
                cp.Delete();
            }

            RoadCheckpoints.Clear();
        }

        /// <summary>
        /// Gets the text description of a <see cref="IntersectionFlags"/>
        /// </summary>
        private string GetIntersectionFlagDesc(IntersectionFlags flag)
        {
            switch (flag)
            {
                default: return "";
                case IntersectionFlags.ThreeWayIntersection: return "Describes a T-intersecion with 3 roads";
                case IntersectionFlags.SplitIntersection: return "Describes a 3 way intersection that splits into a Y shape";
                case IntersectionFlags.CrossIntersection: return "Describes a 4 way intersectiond";
                case IntersectionFlags.MultiDirectionIntersection: return "Describes an intersection with more than 4 roads";
                case IntersectionFlags.Lighted: return "Describes an intersection with stop lights on each road";
                case IntersectionFlags.OneWayStop: return "Describes a T or split intersection where only the ajoining road has a stop sign.";
                case IntersectionFlags.OneWayYield: return "Describes a T or split intersection where only the ajoining road has a yield sign.";
                case IntersectionFlags.TwoWayYield: return "Describes an intersection where 2 intersecting roads have a yield sign";
                case IntersectionFlags.TwoWayStop: return "Describes an intersection where 2 intersecting roads have a stop sign";
                case IntersectionFlags.RightOfWayStop: return "Describes an intersection with Right-of-way rules, such as a 3 or 4 way stop sign intersection";
                case IntersectionFlags.RightOfWayYield: return "Describes an intersection with Right-of-way rules, such as a 3 or 4 way yield intersection";
            }
        }
    }
}
