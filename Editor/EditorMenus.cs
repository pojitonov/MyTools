using System;
using System.Reflection;
using UnityEditor;

static class EditorMenus
{
    [MenuItem("Tools/My Tools/Clear Console &c")] // Alt+C
    static void ClearConsole()
    {
        // This simply does "LogEntries.Clear()" the long way:
        var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }

    [MenuItem("Tools/My Tools/Toggle Lock %&l")] // Ctrl+Alt+L
    // static void ToggleInspectorLock()
    // {
    //     ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
    //     ActiveEditorTracker.sharedTracker.ForceRebuild();
    // }
    static void ToggleWindowLock() // Inspector must be inspecting something to be locked
    {
        EditorWindow
            windowToBeLocked = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead

        if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "InspectorWindow")
        {
            Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
            PropertyInfo propertyInfo = type.GetProperty("isLocked");
            bool value = (bool)propertyInfo.GetValue(windowToBeLocked, null);
            propertyInfo.SetValue(windowToBeLocked, !value, null);
            windowToBeLocked.Repaint();
        }
        else if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "ProjectBrowser")
        {
            Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.ProjectBrowser");
            PropertyInfo propertyInfo = type.GetProperty("isLocked",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            bool value = (bool)propertyInfo.GetValue(windowToBeLocked, null);
            propertyInfo.SetValue(windowToBeLocked, !value, null);
            windowToBeLocked.Repaint();
        }
        else if (windowToBeLocked != null && windowToBeLocked.GetType().Name == "SceneHierarchyWindow")
        {
            Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.SceneHierarchyWindow");

            FieldInfo fieldInfo = type.GetField("m_SceneHierarchy",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = fieldInfo.FieldType.GetProperty("isLocked",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            object value = fieldInfo.GetValue(windowToBeLocked);
            bool value2 = (bool)propertyInfo.GetValue(value);
            propertyInfo.SetValue(value, !value2, null);
            windowToBeLocked.Repaint();
        }
    }
}