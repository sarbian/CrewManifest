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

        private IButton button;

        public void Awake()
        {
            if(HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                DontDestroyOnLoad(this);
                Settings.Load();
                InvokeRepeating("RunSave", interval, interval);
                //InvokeRepeating("CrewCheck", intervalCrewCheck, intervalCrewCheck);

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
            if(HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                CancelInvoke("RunSave");
                //CancelInvoke("CrewCheck");

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

                    if(crewTransferPending)
                    {
                        if(Planetarium.GetUniversalTime() - crewTransferInitiated >= 0.25)
                        {
                            if(crewTransferSourcePart != null && crewTransferDestinationPart != null && crewTransferCrewMember != null)
                            {
                                ScreenMessages.PostScreenMessage("Crew transfer complete.", 1.0f, ScreenMessageStyle.UPPER_CENTER);

                                if(object.ReferenceEquals(crewTransferSourcePart.vessel, crewTransferDestinationPart.vessel))
                                {
                                    crewTransferDestinationPart.vessel.SpawnCrew();
                                }
                                else
                                {
                                    crewTransferSourcePart.vessel.SpawnCrew();
                                    crewTransferDestinationPart.vessel.SpawnCrew();
                                }
                            }

                            crewTransferInitiated = 0;
                            crewTransferPending = false;
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

        private static volatile bool crewTransferPending = false;
        private static Part crewTransferSourcePart;
        private static Part crewTransferDestinationPart;
        private static ProtoCrewMember crewTransferCrewMember;
        private static double crewTransferInitiated = 0;
        internal static void BeginDelayedCrewTransfer(Part source, Part destination, ProtoCrewMember crewMember)
        {
            crewTransferInitiated = Planetarium.GetUniversalTime();
            crewTransferSourcePart = source;
            crewTransferDestinationPart = destination;
            crewTransferCrewMember = crewMember;
            crewTransferPending = true;
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
