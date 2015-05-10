using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CrewManifest
{
    public static class ManifestUtilities
    {
        public static Vector2 DebugScrollPosition = Vector2.zero;

        private static List<string> _errors = new List<string>();
        public static List<string> Errors
        {
            get { return _errors; }
        }
        
        public static void LogMessage(string error, string type)
        {
            _errors.Add(type + ": " + error);
        }
    }

    public static class Resources
    {
        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonToggledStyle;
        public static GUIStyle ButtonToggledRedStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle ErrorLabelRedStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle LabelStyleRed;
        public static GUIStyle LabelStyleYellow;

        // Part selection colors.
        internal static readonly Color SourceColor = Color.green;
        internal static readonly Color TargetColor = Color.red;
        internal static readonly Color SelectionColor = Color.yellow;

        // Roster error colors.
        private static readonly Color AvailableColor = Color.white;
        private static readonly Color UnavailableColor = Color.yellow;
        private static readonly Color DeadColor = Color.red;

        public static void SetupGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (WindowStyle == null)
            {
                SetStyles();
            }
        }

        public static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Resources.SourceColor;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;

            ButtonToggledRedStyle = new GUIStyle(ButtonToggledStyle);
            ButtonToggledRedStyle.normal.textColor = Resources.TargetColor;

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Resources.AvailableColor;

            ErrorLabelRedStyle = new GUIStyle(GUI.skin.label);
            ErrorLabelRedStyle.normal.textColor = Color.red;
            ErrorLabelRedStyle.fontSize = 10;

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Resources.DeadColor;

            LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Resources.UnavailableColor;
        }
    }

    public class SettingsManager
    {
        public Rect ManifestPosition;
        public Rect TransferPosition;
        public Rect RosterPosition;
        public Rect SettingsPosition;

        public Rect DebuggerPosition;
        public bool ShowDebugger;
        
        public bool AppLauncher = true;

        public void Load()
        {
            ManifestUtilities.LogMessage("Settings load started...", "Info");

            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<CrewManifestModule>();
                configfile.load();

                ManifestPosition = configfile.GetValue<Rect>("ManifestPosition");
                TransferPosition = configfile.GetValue<Rect>("TransferPosition");
                RosterPosition = configfile.GetValue<Rect>("RosterPosition");
                SettingsPosition = configfile.GetValue<Rect>("SettingsPosition");
                DebuggerPosition = configfile.GetValue<Rect>("DebuggerPosition");
                ShowDebugger = configfile.GetValue<bool>("ShowDebugger");
                AppLauncher = configfile.GetValue<bool>("AppLauncher");
                                
                ManifestUtilities.LogMessage(string.Format("ManifestPosition Loaded: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("TransferPosition Loaded: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("RosterPosition Loaded: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("DebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info");
            }
            catch(Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }

        public void ClampWindowsToScreen()
        {
            ClampRectToScreen(ref ManifestPosition);
            ClampRectToScreen(ref TransferPosition);
            ClampRectToScreen(ref RosterPosition);
            ClampRectToScreen(ref SettingsPosition);
            ClampRectToScreen(ref DebuggerPosition);
        }

        private void ClampRectToScreen(ref Rect rect)
        {
            rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
        }

        public void Save()
        {
            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<CrewManifestModule>();

                configfile.SetValue("ManifestPosition", ManifestPosition);
                configfile.SetValue("TransferPosition", TransferPosition);
                configfile.SetValue("RosterPosition", RosterPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("DebuggerPosition", DebuggerPosition);
                configfile.SetValue("ShowDebugger", ShowDebugger);
                configfile.SetValue("AppLauncher", AppLauncher);

                configfile.save();

                ManifestUtilities.LogMessage(string.Format("ManifestPosition Saved: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("TransferPosition Saved: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("RosterPosition Saved: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("DebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info");
                ManifestUtilities.LogMessage(string.Format("AllowRespawn Saved: {0}", ShowDebugger.ToString()), "Info");
            }
            catch (Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }
    }
}
