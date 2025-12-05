#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    public static class SceneViewToolsMenu
    {
        private static GameObject lastSelectedObject;
        private static bool toggleState;
        private static HashSet<GameObject> hiddenObjects = new();

        [MenuItem(Menus.TOOLS_MENU + "Reset Current View &h",
            priority = Menus.SCENE_VIEW_INDEX + 100)] //  Alt+H
        public static void ResetSceneViewCamera()
        {
            if (State.disabled) return;
            SceneViewNavigationManager.RedrawLastSavedSceneView();
        }

        [MenuItem(Menus.TOOLS_MENU + "Reset Current View &h", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 100)]
        private static bool ValidateResetSceneViewCamera() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Reset All Views %&h",
            priority = Menus.SCENE_VIEW_INDEX + 101)] // Ctrl+Alt+H
        public static void ResetAllViews()
        {
            if (State.disabled) return;
            SceneViewNavigationManager.ResetAllSceneViews();
        }

        [MenuItem(Menus.TOOLS_MENU + "Reset All Views %&h", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 101)]
        private static bool ValidateResetAllViews() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle Projection _o",
            priority = Menus.SCENE_VIEW_INDEX + 200)] // O
        public static void ToggleProjection()
        {
            if (State.disabled) return;

            ActiveSceneView.sceneView = SceneView.lastActiveSceneView;
            if (ActiveSceneView.sceneView == null || ActiveSceneView.sceneView.in2DMode)
            {
                return;
            }

            ActiveSceneView.sceneView.orthographic = !ActiveSceneView.sceneView.orthographic;
            ActiveSceneView.sceneView.Repaint();
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Projection _o", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 200)]
        private static bool ValidateToggleProjection() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle 2D View &o",
            priority = Menus.SCENE_VIEW_INDEX + 201)] // Alt+O
        public static void Toggle2DView()
        {
            if (State.disabled) return;

            ActiveSceneView.sceneView = SceneView.lastActiveSceneView;
            if (ActiveSceneView.sceneView != null)
            {
                ActiveSceneView.sceneView.in2DMode = !ActiveSceneView.sceneView.in2DMode;
                ActiveSceneView.sceneView.Repaint();
            }
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle 2D View &o", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 201)]
        private static bool ValidateToggle2DView() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle Skybox &s",
            priority = Menus.SCENE_VIEW_INDEX + 202)] //  Alt+S
        static void ToggleSkybox()
        {
            if (State.disabled) return;

            ActiveSceneView.sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (ActiveSceneView.sceneView.sceneViewState.skyboxEnabled)
            {
                SceneViewTools.DisableSkybox();
            }
            else
            {
                SceneViewTools.EnableSkybox();
            }
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Skybox &s", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 202)]
        private static bool ValidateToggleSkybox() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle All Gizmos &g",
            priority = Menus.SCENE_VIEW_INDEX + 203)] // Alt+G
        public static void ToggleSceneViewGizmos()
        {
            if (State.disabled) return;

            var currentValue = SceneViewTools.GetSceneViewGizmosEnabled();
            SceneViewTools.SetSceneViewGizmos(!currentValue);
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle All Gizmos &g", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 203)]
        private static bool ValidateToggleSceneViewGizmos() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle Grid Snapping &j",
            priority = Menus.SCENE_VIEW_INDEX + 204)] // Alt+J
        public static void ToggleGridSnapping()
        {
            if (State.disabled) return;

#if UNITY_6000
            EditorSnapSettings.snapEnabled = !EditorSnapSettings.snapEnabled;
#else
            Debug.Log("Snapping shortcut is not supported in this version.");
#endif
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Grid Snapping &j", true)]
        public static bool ValidateToggleGridSnapping()
        {
            if (State.disabled) return false;
#if UNITY_6000
            return true;
#else
            return false;
#endif
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Grid %&#g",
            priority = Menus.SCENE_VIEW_INDEX + 205)] // Ctrl+Alt+Shift+G
        private static void ToggleGridVisibility()
        {
            if (State.disabled) return;

            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView view)
                {
                    view.showGrid = !view.showGrid;
                }
            }
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle VR Rig %&#r",
            priority = Menus.SCENE_VIEW_INDEX + 206)] // Ctrl+Alt+Shift+R
        private static void ToggleVRRig()
        {
            if (State.disabled) return;
            XRRigTools.ToggleVRRig();
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Grid %&g", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 205)]
        private static bool ValidateToggleGridVisibility() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle VR Rig %&#r", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 206)]
        private static bool ValidateToggleVRRig() => !State.disabled;

        [MenuItem(Menus.TOOLS_MENU + "Toggle Isolation on Selection #\\", false,
            Menus.SCENE_VIEW_INDEX + 207)] // Shift+\
        private static void ToggleObjectVisibility()
        {
            if (State.disabled) return;

            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
            {
                RestoreVisibility();
                toggleState = false;
                lastSelectedObject = null;
                return;
            }

            if (selectedObject != lastSelectedObject && toggleState)
            {
                RestoreVisibility();
                toggleState = false;
            }

            if (selectedObject == lastSelectedObject && toggleState)
            {
                RestoreVisibility();
            }
            else
            {
                HideAllExceptSelected(selectedObject);
            }

            toggleState = !toggleState;
            lastSelectedObject = selectedObject;
        }

        [MenuItem(Menus.TOOLS_MENU + "Toggle Isolation on Selection #\\", true,
            Menus.SCENE_VIEW_INDEX + 207)]
        private static bool ValidateToggleObjectVisibility() => !State.disabled;

        private static void HideAllExceptSelected(GameObject selectedObject)
        {
            GameObject[] rootObjects = selectedObject.scene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                if (obj != selectedObject)
                {
                    SetSceneVisibility(obj, false);
                }
            }

            SetSceneVisibility(selectedObject, true);
        }

        private static void SetSceneVisibility(GameObject obj, bool visible)
        {
            if (visible)
            {
                SceneVisibilityManager.instance.Show(obj, true);
                hiddenObjects.Remove(obj);
            }
            else
            {
                SceneVisibilityManager.instance.Hide(obj, true);
                hiddenObjects.Add(obj);
            }

            foreach (Transform child in obj.transform)
            {
                SetSceneVisibility(child.gameObject, visible);
            }
        }

        private static void RestoreVisibility()
        {
            foreach (GameObject obj in hiddenObjects)
            {
                SceneVisibilityManager.instance.Show(obj, true);
            }

            hiddenObjects.Clear();
        }

        [MenuItem(Menus.TOOLS_MENU + "Frame Selected &f",
            priority = Menus.SCENE_VIEW_INDEX + 300)] //  Alt+F
        static void FrameSelected()
        {
            if (State.disabled) return;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem(Menus.TOOLS_MENU + "Frame Selected &f", validate = true,
            priority = Menus.SCENE_VIEW_INDEX + 300)]
        private static bool ValidateFrameSelected() => !State.disabled;
    }
}
#endif