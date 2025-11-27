#if UNITY_EDITOR
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
        private const double AssemblyReloadDelay = 1.0;

        private const string AutoRefreshMenuPath = Menus.EDITOR_UI_MENU + "Auto-Refresh on Compile";
        private const string ReimportUxmlMenuPath = Menus.EDITOR_UI_MENU + "Reimport UXML Assets &r";

        private static bool _autoRefreshEnabled;
        private static bool _wasCompiling;
        private static double _lastFileCheckTime;
        private static double _lastRefreshTime;
        private static bool _isRefreshing;
        private static bool _pendingRefresh;
        private static double _lastAssemblyReloadTime = -1.0;
        private static readonly Dictionary<string, DateTime> _scriptFileTimestamps = new();

        static UIToolkitRefreshHelper()
        {
            _autoRefreshEnabled = EditorPrefs.GetBool(AutoRefreshPrefKey, false);
            _wasCompiling = EditorApplication.isCompiling;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            EditorApplication.update += CheckForCompilationComplete;
            EditorApplication.update += CheckForScriptFileChanges;
            EditorApplication.update += CheckPendingRefresh;
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
                        RefreshScriptFileTimestamps();
                        ScheduleRefresh(0.2);
                        break;
                    }
                }

                _scriptFileTimestamps[path] = currentWriteTime;
            }
        }

        private static void CheckPendingRefresh()
        {
            if (!_autoRefreshEnabled || _isRefreshing) return;
            if (!_pendingRefresh) return;
            if (EditorApplication.isCompiling) return;

            double currentTime = EditorApplication.timeSinceStartup;
            double timeSinceLastRefresh = currentTime - _lastRefreshTime;
            
            bool assemblyReloadCheck = _lastAssemblyReloadTime < 0 || 
                                       (currentTime - _lastAssemblyReloadTime) >= AssemblyReloadDelay;
            
            if (timeSinceLastRefresh >= RefreshDebounce && assemblyReloadCheck)
            {
                _pendingRefresh = false;
                ExecuteRefresh();
            }
        }

        private static void OnCompilationFinished(object obj)
        {
            if (!_autoRefreshEnabled) return;
            ScheduleRefresh(0.1);
        }

        private static void OnAfterAssemblyReload()
        {
            if (!_autoRefreshEnabled) return;
            _lastAssemblyReloadTime = EditorApplication.timeSinceStartup;
            ScheduleRefresh(AssemblyReloadDelay);
        }

        private static void CheckForCompilationComplete()
        {
            if (!_autoRefreshEnabled) return;

            bool isCompiling = EditorApplication.isCompiling;

            if (_wasCompiling && !isCompiling)
            {
                if (_lastAssemblyReloadTime < 0 || 
                    (EditorApplication.timeSinceStartup - _lastAssemblyReloadTime) > AssemblyReloadDelay + 0.5)
                {
                    ScheduleRefresh(0.1);
                }
            }

            _wasCompiling = isCompiling;
        }

        private static void ScheduleRefresh(double delay)
        {
            if (_isRefreshing)
            {
                _pendingRefresh = true;
                return;
            }

            double currentTime = EditorApplication.timeSinceStartup;
            double timeSinceLastRefresh = currentTime - _lastRefreshTime;
            
            if (timeSinceLastRefresh < RefreshDebounce)
            {
                _pendingRefresh = true;
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (!_autoRefreshEnabled || EditorApplication.isCompiling)
                {
                    _pendingRefresh = true;
                    return;
                }

                if (EditorApplication.isCompiling)
                {
                    _pendingRefresh = true;
                    return;
                }

                ExecuteRefresh();
            };
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

        private static void ExecuteRefresh()
        {
            if (_isRefreshing) return;
            
            _isRefreshing = true;
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            
            try
            {
                ReimportUXMLAssets();
                RefreshUIBuilderWindows();
            }
            finally
            {
                _isRefreshing = false;
                
                if (_pendingRefresh)
                {
                    _pendingRefresh = false;
                    double timeSinceLastRefresh = EditorApplication.timeSinceStartup - _lastRefreshTime;
                    if (timeSinceLastRefresh >= RefreshDebounce)
                    {
                        ScheduleRefresh(0.1);
                    }
                }
            }
        }

        private static void RefreshAuto()
        {
            ExecuteRefresh();
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
#endif