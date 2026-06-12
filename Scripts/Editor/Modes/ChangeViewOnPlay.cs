#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    [InitializeOnLoad]
    internal static class ChangeViewOnPlay
    {
        private const int FOCUS_INDEX = Menus.MODES_INDEX + 201;
        private const int MAXIMIZE_INDEX = Menus.MODES_INDEX + 202;
        private const string FOCUS_MENU = Menus.MY_TOOLS_MENU + "Focus Game View on Play";
        private const string MAXIMIZE_MENU = Menus.MY_TOOLS_MENU + "Maximize Active View on Play";

        private static bool _maximizeEnabled;
        private static bool _focusEnabled;
        private static bool _isMaximized;
        private static bool _wasSceneViewActive;
        private static bool _wasGameViewActive;

        static ChangeViewOnPlay()
        {
            _maximizeEnabled = EditorPrefs.GetBool(MAXIMIZE_MENU, true);
            _focusEnabled = EditorPrefs.GetBool(FOCUS_MENU, true);

            EditorApplication.delayCall += () =>
            {
                SetMenuChecked(MAXIMIZE_MENU, _maximizeEnabled);
                SetMenuChecked(FOCUS_MENU, _focusEnabled);
            };

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;
        }

        [MenuItem(MAXIMIZE_MENU, priority = MAXIMIZE_INDEX)]
        private static void ToggleMaximize()
        {
            if (State.disabled) return;

            _maximizeEnabled = !_maximizeEnabled;
            SetMenuChecked(MAXIMIZE_MENU, _maximizeEnabled);
            Debug.Log(Debug.DefaultPrefix, $"Maximize Active View on Play is {(_maximizeEnabled ? "Enabled" : "Disabled")}");
        }

        [MenuItem(MAXIMIZE_MENU, validate = true, priority = MAXIMIZE_INDEX)]
        private static bool ValidateToggleMaximize() => !State.disabled;

        [MenuItem(FOCUS_MENU, priority = FOCUS_INDEX)]
        private static void ToggleFocus()
        {
            if (State.disabled) return;

            _focusEnabled = !_focusEnabled;
            SetMenuChecked(FOCUS_MENU, _focusEnabled);
            Debug.Log(Debug.DefaultPrefix, $"Focus GameView on Play is {(_focusEnabled ? "Enabled" : "Disabled")}");
        }

        [MenuItem(FOCUS_MENU, validate = true, priority = FOCUS_INDEX)]
        private static bool ValidateToggleFocus() => !State.disabled;

        private static void SetMenuChecked(string menuName, bool enabled)
        {
            Menu.SetChecked(menuName, enabled);
            EditorPrefs.SetBool(menuName, enabled);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (State.disabled) return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                _wasSceneViewActive = SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.hasFocus;
                _wasGameViewActive = GetActiveView()?.GetType().Name == "GameView";

                if (_focusEnabled)
                {
                    EditorApplication.update += ExecuteFocusAndMaximize;
                }
                else
                {
                    EditorApplication.update += ExecuteFocusLastActiveView;
                }
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (_focusEnabled)
                {
                    EditorApplication.delayCall += RestoreSceneView;
                }
            }
        }

        private static void ExecuteFocusAndMaximize()
        {
            if (State.disabled) return;

            EditorApplication.update -= ExecuteFocusAndMaximize;

            FocusView("GameView");

            EditorApplication.delayCall += () => EditorApplication.update += ExecuteMaximize;
        }

        private static void ExecuteFocusLastActiveView()
        {
            if (State.disabled) return;

            EditorApplication.update -= ExecuteFocusLastActiveView;

            if (_wasSceneViewActive)
            {
                FocusView("SceneView");
            }
            else if (_wasGameViewActive)
            {
                FocusView("GameView");
            }
            else
            {
                Debug.LogWarning(Debug.DefaultPrefix, "No active SceneView or GameView found to focus.");
            }

            EditorApplication.delayCall += () => EditorApplication.update += ExecuteMaximize;
        }

        private static void ExecuteMaximize()
        {
            if (State.disabled) return;

            EditorApplication.update -= ExecuteMaximize;

            if (_maximizeEnabled)
            {
                MaximizeActiveView();
            }
        }

        private static void OnPauseStateChanged(PauseState state)
        {
            if (State.disabled) return;

            if (_maximizeEnabled)
            {
                MaximizeActiveView(state == PauseState.Unpaused);
            }
        }

        private static void MaximizeActiveView(bool maximize = true)
        {
            if (State.disabled) return;

            var activeView = GetActiveView();
            if (activeView != null)
            {
                if (maximize)
                {
                    _isMaximized = activeView.maximized;
                    activeView.maximized = true;
                }
                else
                {
                    activeView.maximized = _isMaximized;
                }
            }
        }

        private static EditorWindow GetActiveView()
        {
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var window in allWindows)
            {
                if (window.GetType().Name == "GameView" || window.GetType().Name == "SceneView")
                {
                    if (window.hasFocus || window == EditorWindow.focusedWindow)
                    {
                        return window;
                    }
                }
            }

            return null;
        }

        private static void FocusView(string name)
        {
            EditorApplication.delayCall += () =>
            {
                var view = Utils.GetView("UnityEditor." + name);
                if (view != null)
                {
                    view.Focus();
                }
            };
        }

        private static void RestoreSceneView()
        {
            if (State.disabled) return;

            if (_wasSceneViewActive)
            {
                FocusView("SceneView");
            }
        }
    }
}
#endif
