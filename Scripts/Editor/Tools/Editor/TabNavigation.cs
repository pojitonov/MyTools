#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MyTools
{
    [InitializeOnLoad]
    public static class TabNavigation
    {
        private const bool ALLOW_WHILE_TEXT_EDITING = false;

        static TabNavigation()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect selectionRect) => TryConsumeAndSwitchFromEvent();
        private static void OnProjectItemGUI(string guid, Rect selectionRect) => TryConsumeAndSwitchFromEvent();

        private static void TryConsumeAndSwitchFromEvent()
        {
            if (State.disabled) return;

            var e = Event.current;
            if (e == null) return;
            if (e.type != EventType.KeyDown) return;

            if (!ALLOW_WHILE_TEXT_EDITING && IsProbablyTypingInTextField())
                return;

            // Ctrl + Alt + Left/Right
            if (e.control && e.alt && !e.shift)
            {
                if (e.keyCode == KeyCode.LeftArrow)
                {
                    e.Use();
                    SwitchTab(-1);
                }
                else if (e.keyCode == KeyCode.RightArrow)
                {
                    e.Use();
                    SwitchTab(1);
                }
            }
        }

        private static bool IsProbablyTypingInTextField()
        {
            return EditorGUIUtility.editingTextField
                   || !string.IsNullOrEmpty(GUI.GetNameOfFocusedControl());
        }

        [MenuItem(Menus.EDITOR_MENU + "Next Tab %&RIGHT", priority = Menus.EDITOR_INDEX + 100)]
        private static void NextTab()
        {
            if (State.disabled) return;
            SwitchTab(1);
        }

        [MenuItem(Menus.EDITOR_MENU + "Next Tab %&RIGHT", validate = true)]
        private static bool ValidateNextTab() => !State.disabled;

        [MenuItem(Menus.EDITOR_MENU + "Previous Tab %&LEFT", priority = Menus.EDITOR_INDEX + 101)]
        private static void PreviousTab()
        {
            if (State.disabled) return;
            SwitchTab(-1);
        }

        [MenuItem(Menus.EDITOR_MENU + "Previous Tab %&LEFT", validate = true)]
        private static bool ValidatePreviousTab() => !State.disabled;

        private static void SwitchTab(int direction)
        {
            Utils.ActivateWindowUnderCursor();

            EditorWindow focusedWindow = EditorWindow.focusedWindow;
            if (!focusedWindow) return;

            var dockArea = GetDockArea(focusedWindow);
            if (!dockArea) return;

            var windowsInTabGroup = GetWindowsInTabGroup(dockArea);
            if (windowsInTabGroup.Count == 0) return;

            int currentIndex = windowsInTabGroup.IndexOf(focusedWindow);
            if (currentIndex < 0) return;

            int nextIndex = (currentIndex + direction + windowsInTabGroup.Count) % windowsInTabGroup.Count;
            windowsInTabGroup[nextIndex].Focus();
        }

        private static Object GetDockArea(EditorWindow window)
        {
            var parent = window?.GetType()
                .GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(window);

            if (parent != null && parent.GetType().Name == "DockArea")
                return parent as Object;

            return null;
        }

        private static List<EditorWindow> GetWindowsInTabGroup(Object dockArea)
        {
            var panes = dockArea?.GetType()
                .GetField("m_Panes", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(dockArea) as List<EditorWindow>;

            return panes ?? new List<EditorWindow>();
        }
    }
}
#endif
