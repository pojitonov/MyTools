#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;

namespace MyTools
{
    public static class TabUtilities
    {
        [MenuItem(Menus.EDITOR_MENU + "Maximize Tab %b", priority = Menus.EDITOR_INDEX + 102)]
        private static void Maximize()
        {
            if (State.disabled) return;

            Utils.ActivateWindowUnderCursor();
            EditorWindow window = EditorWindow.focusedWindow;
            if (window) window.maximized = !window.maximized;
        }

        [MenuItem(Menus.EDITOR_MENU + "Maximize Tab %b", validate = true)]
        private static bool ValidateMaximize() => !State.disabled;

        [MenuItem(Menus.EDITOR_MENU + "Close Tab %w", priority = Menus.EDITOR_INDEX + 103)]
        private static void CloseTab()
        {
            if (State.disabled) return;

            Utils.ActivateWindowUnderCursor();
            EditorWindow window = EditorWindow.focusedWindow;
            if (window) window.Close();
        }

        [MenuItem(Menus.EDITOR_MENU + "Close Tab %w", validate = true)]
        private static bool ValidateCloseTab() => !State.disabled;

        [MenuItem(Menus.EDITOR_MENU + "Lock Tab %&l", priority = Menus.EDITOR_INDEX + 104)]
        private static void ToggleWindowLock()
        {
            if (State.disabled) return;

            EditorWindow windowToBeLocked = EditorWindow.mouseOverWindow;
            if (!windowToBeLocked) return;

            if (windowToBeLocked.GetType().Name == "InspectorWindow")
            {
                Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
                PropertyInfo propertyInfo = type.GetProperty("isLocked");
                bool value = (bool)propertyInfo.GetValue(windowToBeLocked, null);
                propertyInfo.SetValue(windowToBeLocked, !value, null);
                windowToBeLocked.Repaint();
            }
            else if (windowToBeLocked.GetType().Name == "ProjectBrowser")
            {
                Type type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.ProjectBrowser");
                PropertyInfo propertyInfo = type.GetProperty(
                    "isLocked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                bool value = (bool)propertyInfo.GetValue(windowToBeLocked, null);
                propertyInfo.SetValue(windowToBeLocked, !value, null);
                windowToBeLocked.Repaint();
            }
            else if (windowToBeLocked.GetType().Name == "SceneHierarchyWindow")
            {
                Type type = Assembly.GetAssembly(typeof(Editor))
                    .GetType("UnityEditor.SceneHierarchyWindow");

                FieldInfo fieldInfo = type.GetField(
                    "m_SceneHierarchy", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo == null) return;

                PropertyInfo propertyInfo = fieldInfo.FieldType.GetProperty(
                    "isLocked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null) return;

                object value = fieldInfo.GetValue(windowToBeLocked);
                bool value2 = (bool)propertyInfo.GetValue(value);
                propertyInfo.SetValue(value, !value2, null);
                windowToBeLocked.Repaint();
            }
        }

        [MenuItem(Menus.EDITOR_MENU + "Lock Tab %&l", validate = true)]
        private static bool ValidateToggleWindowLock() => !State.disabled;
    }
}
#endif
