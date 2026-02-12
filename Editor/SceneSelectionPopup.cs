using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dimzki.Easybuildtool.Editor
{
    public class SceneSelectionPopup : EditorWindow
    {
        private List<string> _availableScenes;
        private HashSet<string> _alreadyAddedPaths;
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private Action<string> _onSceneSelected;

        public static void Show(HashSet<string> alreadyAddedPaths, Action<string> onSceneSelected)
        {
            var window = CreateInstance<SceneSelectionPopup>();
            window.titleContent = new GUIContent("Add Scene");
            window._alreadyAddedPaths = alreadyAddedPaths;
            window._onSceneSelected = onSceneSelected;
            window.minSize = new Vector2(350, 300);
            window.maxSize = new Vector2(500, 500);
            window.RefreshSceneList();
            window.ShowUtility();
        }

        private void RefreshSceneList()
        {
            _availableScenes = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:Scene");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!_alreadyAddedPaths.Contains(path))
                    _availableScenes.Add(path);
            }

            _availableScenes.Sort();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Select a scene to add:", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);
            EditorGUILayout.Space(4);

            if (_availableScenes == null || _availableScenes.Count == 0)
            {
                EditorGUILayout.HelpBox("No additional scenes found in the project.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            string filter = _searchFilter?.ToLowerInvariant() ?? "";

            foreach (string scenePath in _availableScenes)
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    if (!scenePath.ToLowerInvariant().Contains(filter))
                        continue;
                }

                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(sceneName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(scenePath, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Add", GUILayout.Width(50), GUILayout.Height(34)))
                {
                    _onSceneSelected?.Invoke(scenePath);
                    Close();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
