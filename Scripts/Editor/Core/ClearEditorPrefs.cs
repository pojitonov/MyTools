#if UNITY_EDITOR
using UnityEditor;

namespace MyTools
{
    /// <summary>
    /// Adds Edit > Clear All Editor Prefs (below Clear All Player Prefs).
    /// </summary>
    public static class ClearEditorPrefs
    {
        private const int MenuPriority = 15001;

        [MenuItem("Edit/Clear All EditorPrefs", false, MenuPriority)]
        public static void ClearAllEditorPrefs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Clear All Editor Prefs",
                    "Are you sure you want to clear all EditorPrefs? This action cannot be undone.",
                    "Yes",
                    "No"))
                return;

            UnityEditor.EditorPrefs.DeleteAll();
        }
    }
}
#endif
