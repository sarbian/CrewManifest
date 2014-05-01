using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace CrewManifest
{

    
    public class CrewManifestModule : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
        public void DestoryPart()
        {
            if (this.part != null)
                this.part.temperature = 5000;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if(this.part != null && part.name == "crewManifest")
                Events["DestoryPart"].active = true;
            else
                Events["DestoryPart"].active = false;
        }
    }    
      
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ManifestBehaviour : MonoBehaviour
    {
        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager Settings = new SettingsManager();
        private float interval = 30F;
        private float intervalCrewCheck = 0.5f;
        private double crewTransferDelay = 0.25;

        private IButton button;

        public void Awake()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                DontDestroyOnLoad(this);
                Settings.Load();
                InvokeRepeating("RunSave", interval, interval);
                
                button = ToolbarManager.Instance.add("CrewManifest", "CrewManifest");
                button.TexturePath = "CrewManifest/Plugins/IconOff_24";
                button.ToolTip = "Crew Manifest";
                button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                button.OnClick += (e) =>
                {
                    if(!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
                        FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton
                        )
                    {
                        ManifestController manifestController = ManifestController.GetInstance(FlightGlobals.ActiveVessel);

                        button.TexturePath = manifestController.ShowWindow ? "CrewManifest/Plugins/IconOff_24" : "CrewManifest/Plugins/IconOn_24";
                        manifestController.ShowWindow = !manifestController.ShowWindow;
                    }
                };
            }
        }

        public void OnDestroy()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                CancelInvoke("RunSave");

                if (button != null)
                    button.Destroy();
            }
        }

        public void OnGUI()
        {
            Resources.SetupGUI();

            if(Settings.ShowDebugger)
                Settings.DebuggerPosition = GUILayout.Window(398643, Settings.DebuggerPosition, DrawDebugger, "Manifest Debug Console", GUILayout.MinHeight(20));
        }
        
        public void Update()
        {
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    //Instantiate the controller for the active vessel.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton = true;

                    if (crewTransfer != null)
                    {
                        if (Planetarium.GetUniversalTime() - crewTransfer.Initiated >= crewTransferDelay)
                        {
                            if (crewTransfer.Source != null && crewTransfer.Destination != null && crewTransfer.CrewMember != null)
                            {
                                ScreenMessages.PostScreenMessage(string.Format("{0}'s transfer complete.", crewTransfer.CrewMember.name), 2.0f, ScreenMessageStyle.UPPER_CENTER);

                                if(!object.ReferenceEquals(crewTransfer.Source.vessel, crewTransfer.Destination.vessel))
                                {
                                    crewTransfer.Source.vessel.SpawnCrew();
                                }

                                crewTransfer.Destination.vessel.SpawnCrew();

                                FireVesselUpdated();
                            }

                            crewTransfer = null;
                        }
                    }
                }
            }
        }

        public void RunSave()
        {
            Save();
        }

        private void Save()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                ManifestUtilities.LogMessage("Saving Manifest Settings...", "Info");
                Settings.Save();
            }
        }

        // The global vessel update is only required once after each operation or set of operations.
        internal static void FireVesselUpdated()
        {
            // Notify everything that we've made a change to the vessel, TextureReplacer uses this, per shaw:
            // http://forum.kerbalspaceprogram.com/threads/60936-0-23-0-Kerbal-Crew-Manifest-v0-5-6-2?p=1051394&viewfull=1#post1051394

            GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }

        private class CrewTransfer
        {
            public Part Source;
            public Part Destination;
            public ProtoCrewMember CrewMember;
            public double Initiated;

            public CrewTransfer(Part source, Part destination, ProtoCrewMember crewMember)
            {
                this.Source = source;
                this.Destination = destination;
                this.CrewMember = crewMember;
                this.Initiated = Planetarium.GetUniversalTime();
            }
        }

        private static CrewTransfer crewTransfer;

        internal static void BeginDelayedCrewTransfer(Part source, Part destination, ProtoCrewMember crewMember)
        {
            crewTransfer = new CrewTransfer(source, destination, crewMember);
        }
        
        private void DrawDebugger(int windowId)
        {
            GUILayout.BeginVertical();

            ManifestUtilities.DebugScrollPosition = GUILayout.BeginScrollView(ManifestUtilities.DebugScrollPosition, GUILayout.Height(300), GUILayout.Width(500));
            GUILayout.BeginVertical();

            foreach(string error in ManifestUtilities.Errors)
                GUILayout.TextArea(error, GUILayout.Width(460));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }
    }
}
