using System;
using UnityEditor;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public class ConfirmBuildPopup : EditorWindow
    {
        private string _platform;
        private string _version;
        private int _sceneCount;
        private bool _zip;
        private bool _devBuild;
        private Action _onConfirm;

        public static void Show(
            string platform,
            string version,
            int sceneCount,
            bool zip,
            bool devBuild,
            Action onConfirm)
        {
            var window = CreateInstance<ConfirmBuildPopup>();
            window.titleContent = new GUIContent("Confirm Build");
            window._platform = platform;
            window._version = version;
            window._sceneCount = sceneCount;
            window._zip = zip;
            window._devBuild = devBuild;
            window._onConfirm = onConfirm;

            window.minSize = new Vector2(300, 200);
            window.maxSize = new Vector2(300, 200);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(12);

            EditorGUILayout.LabelField("Confirm Build", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            DrawRow("Platform:", _platform);
            DrawRow("Version:", _version);
            DrawRow("Scenes:", _sceneCount.ToString());
            DrawRow("Zip:", _zip.ToString());
            DrawRow("Dev Build:", _devBuild.ToString());

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Build", GUILayout.Width(100), GUILayout.Height(28)))
            {
                _onConfirm?.Invoke();
                Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(28)))
            {
                Close();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
        }

        private void DrawRow(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
    }
}
