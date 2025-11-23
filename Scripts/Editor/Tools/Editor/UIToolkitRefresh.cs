using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace MyTools
{
    [InitializeOnLoad]
    public static class UIToolkitRefreshHelper
    {
        private const string AutoRefreshPrefKey = "UIToolkit_AutoRefreshOnCompile";
        private const double FileCheckInterval = 0.5;
        private const double RefreshDebounce = 2.0;

        private const string AutoRefreshMenuPath = Menus.EDITOR_UI_MENU + "Auto-Refresh on Compile";
        private const string ReimportUxmlMenuPath = Menus.EDITOR_UI_MENU + "Reimport UXML Assets &r";

        private static bool _autoRefreshEnabled;
        private static bool _wasCompiling;
        private static double _lastFileCheckTime;
        private static double _lastRefreshTime;
        private static readonly Dictionary<string, DateTime> _scriptFileTimestamps = new();

        static UIToolkitRefreshHelper()
        {
            _autoRefreshEnabled = EditorPrefs.GetBool(AutoRefreshPrefKey, false);
            _wasCompiling = EditorApplication.isCompiling;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            EditorApplication.update += CheckForCompilationComplete;
            EditorApplication.update += CheckForScriptFileChanges;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            RefreshScriptFileTimestamps();
        }

        private static void RefreshScriptFileTimestamps()
        {
            _scriptFileTimestamps.Clear();
            string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });
            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (File.Exists(path))
                {
                    _scriptFileTimestamps[path] = File.GetLastWriteTime(path);
                }
            }
        }

        private static void CheckForScriptFileChanges()
        {
            if (!_autoRefreshEnabled) return;

            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastFileCheckTime < FileCheckInterval)
                return;

            _lastFileCheckTime = currentTime;
            string[] scriptGuids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.Exists(path)) continue;

                DateTime currentWriteTime = File.GetLastWriteTime(path);

                if (_scriptFileTimestamps.TryGetValue(path, out DateTime lastWriteTime))
                {
                    if (currentWriteTime > lastWriteTime && !EditorApplication.isCompiling)
                    {
                        double timeSinceLastRefresh = currentTime - _lastRefreshTime;
                        if (timeSinceLastRefresh < RefreshDebounce)
                            continue;

                        EditorApplication.delayCall += () =>
                        {
                            if (!EditorApplication.isCompiling && _autoRefreshEnabled)
                            {
                                _lastRefreshTime = EditorApplication.timeSinceStartup;
                                RefreshAuto();
                            }
                        };
                        RefreshScriptFileTimestamps();
                        break;
                    }
                }

                _scriptFileTimestamps[path] = currentWriteTime;
            }
        }

        private static void OnCompilationFinished(object obj)
        {
            if (!_autoRefreshEnabled) return;

            EditorApplication.delayCall += () =>
            {
                if (_autoRefreshEnabled && !EditorApplication.isCompiling)
                {
                    RefreshAuto();
                }
            };
        }

        private static void OnAfterAssemblyReload()
        {
            if (!_autoRefreshEnabled) return;

            EditorApplication.delayCall += () =>
            {
                if (_autoRefreshEnabled)
                {
                    RefreshAuto();
                }
            };
        }

        private static void CheckForCompilationComplete()
        {
            if (!_autoRefreshEnabled) return;

            bool isCompiling = EditorApplication.isCompiling;

            if (_wasCompiling && !isCompiling)
            {
                EditorApplication.delayCall += () =>
                {
                    if (!EditorApplication.isCompiling && _autoRefreshEnabled)
                    {
                        RefreshAuto();
                    }
                };
            }

            _wasCompiling = isCompiling;
        }

        [MenuItem(AutoRefreshMenuPath, false, priority: Menus.EDITOR_UI_INDEX + 101)]
        private static void ToggleAutoRefresh()
        {
            _autoRefreshEnabled = !_autoRefreshEnabled;
            EditorPrefs.SetBool(AutoRefreshPrefKey, _autoRefreshEnabled);
            Menu.SetChecked(AutoRefreshMenuPath, _autoRefreshEnabled);
            Debug.Log(Debug.DefaultPrefix, $"Auto-refresh: {(_autoRefreshEnabled ? "Enabled" : "Disabled")}");
        }

        [MenuItem(AutoRefreshMenuPath, true)]
        private static bool ToggleAutoRefreshValidate()
        {
            Menu.SetChecked(AutoRefreshMenuPath, _autoRefreshEnabled);
            return true;
        }

        private static void RefreshAuto()
        {
            ReimportUXMLAssets();
            RefreshUIBuilderWindows();
        }

        [MenuItem(ReimportUxmlMenuPath, false, priority: Menus.EDITOR_UI_INDEX + 102)]
        public static void ReimportUXMLAssets()
        {
            string[] guids = AssetDatabase.FindAssets("t:VisualTreeAsset");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                count++;
            }

            Debug.Log(Debug.DefaultPrefix, $"Reimported {count} UXML asset(s).");
        }

        private static void RefreshUIBuilderWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>()
                .Where(w => w.GetType().Name.Contains("Builder") || w.GetType().Name.Contains("UIBuilder"))
                .ToArray();

            foreach (var window in windows)
            {
                if (window)
                {
                    window.Repaint();

                    var windowType = window.GetType();
                    var refreshMethod = windowType.GetMethod(
                        "Refresh",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                    refreshMethod?.Invoke(window, null);

                    var reloadMethod = windowType.GetMethod(
                        "ReloadDocument",
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                    reloadMethod?.Invoke(window, null);
                }
            }
        }
    }
}