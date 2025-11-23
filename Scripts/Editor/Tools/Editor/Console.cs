#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace MyTools
{
    internal static class Console
    {
        [MenuItem(Menus.EDITOR_MENU + "Clear Console %l", priority = Menus.EDITOR_INDEX + 400)]
        static void Clear()
        {
            if (State.disabled) return;
            Utils.ClearConsole();
        }

        [MenuItem(Menus.EDITOR_MENU + "Clear Console %l", validate = true)]
        static bool ValidateClear() => !State.disabled;

        [MenuItem(Menus.EDITOR_MENU + "Open Editor Logs", priority = Menus.EDITOR_INDEX + 401)]
        static void OpenLogs()
        {
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Unity", "Editor", "Editor.log"
            );

            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                Debug.LogWarning(Debug.DefaultPrefix, $"Editor.log not found at:\n{Path.GetDirectoryName(filePath)}");
            }
        }
    }
}
#endif