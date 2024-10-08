﻿using System;
using System.Reflection;
using UnityEditor;

namespace MyTools
{
    public static class MyTools
    {
        public static EditorWindow GetView(string name)
        {
            Type viewType = typeof(Editor).Assembly.GetType(name);
            return EditorWindow.GetWindow(viewType);
        }
        
        public static void ActivateWindowUnderCursor()
        {
            EditorWindow windowUnderCursor = EditorWindow.mouseOverWindow;

            if (windowUnderCursor != null)
            {
                windowUnderCursor.Focus();
            }
        }
        
        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }
}