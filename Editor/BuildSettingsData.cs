using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public enum BuildPlatformTarget
    {
        Windows,
        MacOS,
        AndroidAPK,
        AndroidAAB,
        iOS
    }

    [Serializable]
    public class SceneEntry
    {
        public string ScenePath;
        public bool Enabled;

        public string SceneName
        {
            get
            {
                if (string.IsNullOrEmpty(ScenePath)) return "(none)";
                string name = System.IO.Path.GetFileNameWithoutExtension(ScenePath);
                return name;
            }
        }
    }

    [Serializable]
    public class BuildSettingsData
    {
        public BuildPlatformTarget SelectedPlatform = BuildPlatformTarget.Windows;
        public ScriptingImplementation ScriptingBackend = ScriptingImplementation.Mono2x;
        public bool DevelopmentBuild;
        public string ProjectNameSuffix = "";
        public List<SceneEntry> Scenes = new List<SceneEntry>();

        private const string PREFS_KEY = "EasyBuildTool_BuildSettings";

        public static BuildSettingsData Load()
        {
            string json = EditorPrefs.GetString(PREFS_KEY, "");
            if (string.IsNullOrEmpty(json))
                return new BuildSettingsData();

            try
            {
                return JsonUtility.FromJson<BuildSettingsData>(json);
            }
            catch
            {
                return new BuildSettingsData();
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(this);
            EditorPrefs.SetString(PREFS_KEY, json);
        }

        public static BuildTarget ToBuildTarget(BuildPlatformTarget platform)
        {
            return platform switch
            {
                BuildPlatformTarget.Windows => BuildTarget.StandaloneWindows64,
                BuildPlatformTarget.MacOS => BuildTarget.StandaloneOSX,
                BuildPlatformTarget.AndroidAPK => BuildTarget.Android,
                BuildPlatformTarget.AndroidAAB => BuildTarget.Android,
                BuildPlatformTarget.iOS => BuildTarget.iOS,
                _ => BuildTarget.StandaloneWindows64
            };
        }

        public static BuildTargetGroup ToBuildTargetGroup(BuildPlatformTarget platform)
        {
            return platform switch
            {
                BuildPlatformTarget.Windows => BuildTargetGroup.Standalone,
                BuildPlatformTarget.MacOS => BuildTargetGroup.Standalone,
                BuildPlatformTarget.AndroidAPK => BuildTargetGroup.Android,
                BuildPlatformTarget.AndroidAAB => BuildTargetGroup.Android,
                BuildPlatformTarget.iOS => BuildTargetGroup.iOS,
                _ => BuildTargetGroup.Standalone
            };
        }

        public static string GetExecutableExtension(BuildPlatformTarget platform)
        {
            return platform switch
            {
                BuildPlatformTarget.Windows => ".exe",
                BuildPlatformTarget.MacOS => ".app",
                BuildPlatformTarget.AndroidAPK => ".apk",
                BuildPlatformTarget.AndroidAAB => ".aab",
                BuildPlatformTarget.iOS => "",
                _ => ""
            };
        }

        public static string GetDisplayName(BuildPlatformTarget platform)
        {
            return platform switch
            {
                BuildPlatformTarget.Windows => "Windows",
                BuildPlatformTarget.MacOS => "MacOS",
                BuildPlatformTarget.AndroidAPK => "Android APK",
                BuildPlatformTarget.AndroidAAB => "Android AAB",
                BuildPlatformTarget.iOS => "iOS",
                _ => platform.ToString()
            };
        }
    }
}
