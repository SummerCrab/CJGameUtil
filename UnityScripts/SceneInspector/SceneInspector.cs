using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace QuarioToolbox
{
    [Serializable]
    public class SceneInspectorSettings
    {
        public bool OnlyIncludedScenes = false;
        public bool RestoreAfterPlay = true;
        public string[] scenePaths;
    }

    [InitializeOnLoad]
    public class SceneInspector
    {
        static float Height = 23f;
        static SceneInspectorSettings Settings;
        static HashSet<string> Shortcuts;

        static SceneInspector()
        {
            LoadSettings();
            if (Settings == null)
            {
                Settings = new SceneInspectorSettings();
            }

            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            ToolbarExtender.RightToolbarGUI.Add(OnShortcutsGUI);
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    var path = EditorPrefs.GetString("LastEditScenePath", null);
                    if (!string.IsNullOrEmpty(path) && path != EditorSceneManager.GetActiveScene().path)
                    {
                        EditorPrefs.SetString("LastEditScenePath", null);
                        EditorSceneManager.OpenScene(path);
                    }
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Time.timeScale = 1.0f;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }

        }

        static void SaveSettings()
        {
            EditorPrefs.SetString(GetEditorSettingsKey(), JsonUtility.ToJson(Settings));
        }

        static void LoadSettings()
        {
            if (Settings == null)
            {
                Settings = new SceneInspectorSettings();
            }

            var settingsKeyData = EditorPrefs.GetString(GetEditorSettingsKey());
            if (settingsKeyData == "")
            {
                SaveSettings();
            }

            JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(GetEditorSettingsKey()), Settings);
            Shortcuts = new HashSet<string>(Settings.scenePaths.ToList());
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            EditorGUILayout.BeginHorizontal();
            CreateSettingsButton();
            CreatePlayButton();
            CreateSceneChangeButton();
            EditorGUI.EndDisabledGroup();
            CreateSceneAddButton();

            EditorGUILayout.EndHorizontal();
        }

        static void OnShortcutsGUI()
        {
            if (!EditorApplication.isPlaying && Shortcuts.Count > 0)
            {
                var scenes = Shortcuts.ToArray();
                string[] sceneNames = scenes.ToArray();

                for (int i = 0; i < sceneNames.Length; ++i)
                {
                    sceneNames[i] = GetSceneNameFromPath(sceneNames[i]);
                }

                int selection = GUILayout.Toolbar(-1, sceneNames, GUILayout.Height(Height));
                if (selection != -1)
                {
                    SwitchScene(scenes[selection]);
                }

                GUILayout.FlexibleSpace();
            }
            
            Time.timeScale = EditorGUILayout.Slider("", Time.timeScale, 0, 20, GUILayout.Width(120));

        }

        static void SwitchScene(object scene)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene((string)scene);
            }
        }

        static void AddScene(object scene)
        {
            if (EditorApplication.isPlaying)
            {
                EditorSceneManager.LoadScene((string)scene, LoadSceneMode.Additive);
            }
            else
            {
                EditorSceneManager.OpenScene((string)scene, OpenSceneMode.Additive);
            }
        }
        static void CreatePlayButton()
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            GUIContent playContent = new GUIContent();
            playContent.image = EditorGUIUtility.IconContent("preAudioPlayOff").image;
            playContent.tooltip = "Play game from first scene";

            if (GUILayout.Button(playContent, GUILayout.Height(Height)))
            {
                EditorPrefs.SetString("LastEditScenePath", EditorSceneManager.GetActiveScene().path);
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    AssetDatabase.Refresh();
                    EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
                    EditorApplication.isPlaying = true;
                }
            }

            GUI.backgroundColor = oldColor;
        }

        static void CreateSceneChangeButton()
        {
            GUIContent changeSceneContent = new GUIContent();
            changeSceneContent.text = " " + SceneManager.GetActiveScene().name;
            changeSceneContent.image = EditorGUIUtility.IconContent("BuildSettings.SelectedIcon").image;
            changeSceneContent.tooltip = "Change active scene";

            if (GUILayout.Button(changeSceneContent, GUILayout.Height(Height)) && !EditorApplication.isPlaying)
            {
                GenericMenu menu = new GenericMenu();
                FillScenesMenu(menu, SwitchScene);
                menu.ShowAsContext();
            }
        }

        static void CreateSceneAddButton()
        {
            GUIContent changeSceneContent = new GUIContent();
            changeSceneContent.image = EditorGUIUtility.IconContent("Toolbar Plus More").image;
            changeSceneContent.tooltip = "Open scene in additive mode";

            if (GUILayout.Button(changeSceneContent, GUILayout.Height(Height)))
            {
                GenericMenu menu = new GenericMenu();
                FillScenesMenu(menu, AddScene);
                menu.ShowAsContext();
            }
        }

        static void FillScenesMenu(GenericMenu menu, GenericMenu.MenuFunction2 callback)
        {
            if (Settings.OnlyIncludedScenes)
            {
                if (EditorBuildSettings.scenes.Length == 0)
                {
                    Debug.LogWarning("[Quario:SceneInspector] There is no scenes defined in build settings.");
                }
                else foreach (var scene in EditorBuildSettings.scenes)
                    {
                        menu.AddItem(new GUIContent(GetSceneNameFromPath(scene.path)),
                            scene.path == SceneManager.GetActiveScene().path,
                            callback,
                            scene.path);
                    }
            }
            else
            {
                var scenes = AssetDatabase.FindAssets("t:Scene");
                foreach (var t in scenes)
                {
                    var path = AssetDatabase.GUIDToAssetPath(t);
                    menu.AddItem(new GUIContent(GetSceneNameFromPath(path)),
                        path == SceneManager.GetActiveScene().path,
                        callback,
                        path);
                }
            }
        }

        static void CreateSettingsButton()
        {
            GUIContent settingsContent = new GUIContent();
            settingsContent.image = EditorGUIUtility.IconContent("_Popup").image;
            settingsContent.tooltip = "Scene inspector settings";

            if (GUILayout.Button(settingsContent, GUILayout.Height(Height)))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Show only scenes included in build"), Settings.OnlyIncludedScenes, () =>
                {
                    Settings.OnlyIncludedScenes = !Settings.OnlyIncludedScenes;
                    SaveSettings();
                });

                /*menu.AddItem(new GUIContent("Restore current scene after play"), Settings.RestoreAfterPlay, () =>
                {
                    Settings.RestoreAfterPlay = !Settings.RestoreAfterPlay;
                    SaveSettings();
                });*/

                menu.AddSeparator("/");

                var scenes = AssetDatabase.FindAssets("t:Scene");
                foreach (var t in scenes)
                {
                    var path = AssetDatabase.GUIDToAssetPath(t);
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(path.Split('/').Last());
                    menu.AddItem(new GUIContent("Custom shortcuts/" + sceneName), Shortcuts.Contains(path), () =>
                    {
                        if (!Shortcuts.Add(path))
                        {
                            Shortcuts.Remove(path);
                        }

                        Settings.scenePaths = Shortcuts.ToArray();
                        SaveSettings();
                    });
                }

                menu.AddSeparator("Custom shortcuts/");

                menu.AddItem(new GUIContent("Custom shortcuts/Clear"), false, () =>
                {
                    Shortcuts.Clear();
                    Settings.scenePaths = Shortcuts.ToArray();
                    SaveSettings();
                });

                menu.AddItem(new GUIContent("Clear shortcuts"), false, () =>
                {
                    Shortcuts.Clear();
                    Settings.scenePaths = Shortcuts.ToArray();
                    SaveSettings();
                });
                menu.ShowAsContext();
            }
        }

        static public string GetSceneNameFromPath(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path.Split('/').Last());
        }

        public static string GetEditorSettingsKey()
        {
            return "QuarioToolbox:" + Application.productName + ":Settings";
        }
    }
}
