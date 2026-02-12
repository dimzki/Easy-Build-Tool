using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public class BuildToolWindow : EditorWindow
    {
        private BuildSettingsData _settings;
        private VersionManager _versionManager;
        private Vector2 _sceneListScrollPos;

        [MenuItem("Build/Build Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildToolWindow>(true, "Easy Build Tool");
            window.minSize = new Vector2(450, 650);
        }

        private void OnEnable()
        {
            _settings = BuildSettingsData.Load();
            _versionManager = new VersionManager();
            _versionManager.LoadFromConfig();
        }

        private void OnDisable()
        {
            _settings?.Save();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);

            DrawPlatformSection();
            DrawSeparator();
            DrawScriptingBackendSection();
            DrawSeparator();
            DrawDevelopmentBuildSection();
            DrawSeparator();
            DrawSceneListSection();
            DrawSeparator();
            DrawVersionSection();

            GUILayout.FlexibleSpace();
            DrawBuildButtons();
            EditorGUILayout.Space(8);
        }

        private void DrawPlatformSection()
        {
            EditorGUILayout.LabelField("Platform Target", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            foreach (BuildPlatformTarget platform in System.Enum.GetValues(typeof(BuildPlatformTarget)))
            {
                bool isSelected = _settings.SelectedPlatform == platform;
                string displayName = BuildSettingsData.GetDisplayName(platform);
                bool toggled = EditorGUILayout.ToggleLeft(displayName, isSelected, GUILayout.Width(105));

                if (toggled && !isSelected)
                    _settings.SelectedPlatform = platform;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScriptingBackendSection()
        {
            EditorGUILayout.LabelField("Script Backend", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            bool isMono = _settings.ScriptingBackend == ScriptingImplementation.Mono2x;

            bool monoToggled = EditorGUILayout.ToggleLeft("Mono", isMono, GUILayout.Width(90));
            if (monoToggled && !isMono)
                _settings.ScriptingBackend = ScriptingImplementation.Mono2x;

            bool il2cppToggled = EditorGUILayout.ToggleLeft("IL2CPP", !isMono, GUILayout.Width(90));
            if (il2cppToggled && isMono)
                _settings.ScriptingBackend = ScriptingImplementation.IL2CPP;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawDevelopmentBuildSection()
        {
            EditorGUILayout.LabelField("Development Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            bool isFalse = !_settings.DevelopmentBuild;

            bool falseToggled = EditorGUILayout.ToggleLeft("False", isFalse, GUILayout.Width(90));
            if (falseToggled && !isFalse)
                _settings.DevelopmentBuild = false;

            bool trueToggled = EditorGUILayout.ToggleLeft("True", !isFalse, GUILayout.Width(90));
            if (trueToggled && isFalse)
                _settings.DevelopmentBuild = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSceneListSection()
        {
            EditorGUILayout.LabelField("Scene Selection", EditorStyles.boldLabel);

            float listHeight = Mathf.Clamp(_settings.Scenes.Count * 26f + 10f, 60f, 200f);
            _sceneListScrollPos = EditorGUILayout.BeginScrollView(
                _sceneListScrollPos,
                EditorStyles.helpBox,
                GUILayout.Height(listHeight));

            int enabledIndex = 0;
            int removeIndex = -1;
            int moveUpIndex = -1;
            int moveDownIndex = -1;

            for (int i = 0; i < _settings.Scenes.Count; i++)
            {
                var scene = _settings.Scenes[i];
                bool sceneExists = System.IO.File.Exists(scene.ScenePath);

                EditorGUILayout.BeginHorizontal();

                // Enabled checkbox
                EditorGUI.BeginDisabledGroup(!sceneExists);
                scene.Enabled = EditorGUILayout.Toggle(scene.Enabled, GUILayout.Width(18));
                EditorGUI.EndDisabledGroup();

                // Build index
                string indexLabel = scene.Enabled && sceneExists ? enabledIndex.ToString() : "-";
                EditorGUILayout.LabelField(indexLabel, GUILayout.Width(20));
                if (scene.Enabled && sceneExists) enabledIndex++;

                // Scene path display
                var labelStyle = new GUIStyle(EditorStyles.label);
                if (!sceneExists)
                {
                    labelStyle.normal.textColor = Color.red;
                    EditorGUILayout.LabelField(scene.ScenePath + " (missing)", labelStyle);
                }
                else
                {
                    EditorGUILayout.LabelField(scene.ScenePath, labelStyle);
                }

                // Move up
                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUILayout.Button("\u25B2", GUILayout.Width(22)))
                    moveUpIndex = i;
                EditorGUI.EndDisabledGroup();

                // Move down
                EditorGUI.BeginDisabledGroup(i == _settings.Scenes.Count - 1);
                if (GUILayout.Button("\u25BC", GUILayout.Width(22)))
                    moveDownIndex = i;
                EditorGUI.EndDisabledGroup();

                // Remove
                if (GUILayout.Button("X", GUILayout.Width(22)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (_settings.Scenes.Count == 0)
            {
                EditorGUILayout.LabelField("No scenes added. Click 'Add Scene' below.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.EndScrollView();

            // Process deferred operations
            if (removeIndex >= 0)
                _settings.Scenes.RemoveAt(removeIndex);
            if (moveUpIndex > 0)
                SwapScenes(moveUpIndex, moveUpIndex - 1);
            if (moveDownIndex >= 0 && moveDownIndex < _settings.Scenes.Count - 1)
                SwapScenes(moveDownIndex, moveDownIndex + 1);

            // Add Scene button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Scene", GUILayout.Width(120)))
            {
                var existing = new HashSet<string>();
                foreach (var s in _settings.Scenes)
                    existing.Add(s.ScenePath);

                SceneSelectionPopup.Show(existing, AddSceneToList);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVersionSection()
        {
            EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // Last Version
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Last Version", GUILayout.Width(110));
            EditorGUILayout.TextField(_versionManager.LastVersion.ToString());
            EditorGUILayout.EndHorizontal();

            // New Version
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Version", GUILayout.Width(110));

            var versionStyle = new GUIStyle(EditorStyles.textField);
            if (!_versionManager.IsVersionUnchanged)
                versionStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);

            EditorGUILayout.TextField(_versionManager.NewVersion.ToString(), versionStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Version adjustment buttons
            DrawVersionButtons();
        }

        private void DrawVersionButtons()
        {
            float buttonWidth = 30f;
            float groupWidth = buttonWidth * 3 + 8;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Major
            EditorGUILayout.BeginVertical(GUILayout.Width(groupWidth));
            EditorGUILayout.LabelField("Major", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(buttonWidth))) _versionManager.DecrementMajor();
            if (GUILayout.Button("+", GUILayout.Width(buttonWidth))) _versionManager.IncrementMajor();
            if (GUILayout.Button("\u21BA", GUILayout.Width(buttonWidth))) _versionManager.ResetMajor();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(16);

            // Minor
            EditorGUILayout.BeginVertical(GUILayout.Width(groupWidth));
            EditorGUILayout.LabelField("Minor", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(buttonWidth))) _versionManager.DecrementMinor();
            if (GUILayout.Button("+", GUILayout.Width(buttonWidth))) _versionManager.IncrementMinor();
            if (GUILayout.Button("\u21BA", GUILayout.Width(buttonWidth))) _versionManager.ResetMinor();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(16);

            // Hotfix
            EditorGUILayout.BeginVertical(GUILayout.Width(groupWidth));
            EditorGUILayout.LabelField("Hotfix", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", GUILayout.Width(buttonWidth))) _versionManager.DecrementHotfix();
            if (GUILayout.Button("+", GUILayout.Width(buttonWidth))) _versionManager.IncrementHotfix();
            if (GUILayout.Button("\u21BA", GUILayout.Width(buttonWidth))) _versionManager.ResetHotfix();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBuildButtons()
        {
            if (_versionManager.IsVersionUnchanged)
            {
                EditorGUILayout.HelpBox(
                    "Increment the version number to enable building.",
                    MessageType.Info);
            }

            // Build path display
            string projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
            string folderName = BuildExecutor.GenerateBuildFolderName(_versionManager.NewVersion);
            string buildPath = System.IO.Path.Combine(projectRoot, "Builds", _settings.SelectedPlatform.ToString(), folderName);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(buildPath);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(4);
            EditorGUI.BeginDisabledGroup(_versionManager.IsVersionUnchanged);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Build Normally", GUILayout.Width(150), GUILayout.Height(35)))
                ExecuteBuild(false);

            if (GUILayout.Button("Build as ZIP", GUILayout.Width(150), GUILayout.Height(35)))
                ExecuteBuild(true);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void ExecuteBuild(bool zip)
        {
            var enabledScenes = _settings.Scenes
                .Where(s => s.Enabled && System.IO.File.Exists(s.ScenePath))
                .Select(s => s.ScenePath)
                .ToArray();

            if (enabledScenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Build Error", "No scenes enabled for build.", "OK");
                return;
            }

            string platform = _settings.SelectedPlatform.ToString();
            var capturedScenes = enabledScenes;

            ConfirmBuildPopup.Show(
                platform,
                _versionManager.NewVersion.ToString(),
                capturedScenes.Length,
                zip,
                _settings.DevelopmentBuild,
                () => RunBuild(capturedScenes, zip));
        }

        private void RunBuild(string[] scenes, bool zip)
        {
            bool success = BuildExecutor.Build(
                _settings.SelectedPlatform,
                _settings.ScriptingBackend,
                _settings.DevelopmentBuild,
                scenes,
                _versionManager.NewVersion,
                zip);

            if (success)
            {
                _versionManager.CommitVersion();
                _settings.Save();
                Repaint();
            }
        }

        private void AddSceneToList(string scenePath)
        {
            _settings.Scenes.Add(new SceneEntry
            {
                ScenePath = scenePath,
                Enabled = true
            });
            Repaint();
        }

        private void SwapScenes(int indexA, int indexB)
        {
            (_settings.Scenes[indexA], _settings.Scenes[indexB]) =
                (_settings.Scenes[indexB], _settings.Scenes[indexA]);
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space(4);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(4);
        }
    }
}
