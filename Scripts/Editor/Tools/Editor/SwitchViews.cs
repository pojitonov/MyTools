#if UNITY_EDITOR
using UnityEditor;

namespace MyTools
{
    public static class SwitchViews
    {
        [MenuItem(Menus.EDITOR_MENU + "Switch SceneView _TAB", priority = Menus.EDITOR_INDEX + 200)]
        private static void SwitchView()
        {
            if (State.disabled) return;

            EditorWindow focusedWindow = EditorWindow.focusedWindow;

            if (focusedWindow)
            {
                string windowType = focusedWindow.GetType().Name;

                if (windowType == "SceneView")
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                }
                else if (windowType == "GameView")
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Scene");
                }
            }
        }
        
        [MenuItem(Menus.EDITOR_MENU + "Switch SceneView _TAB", validate = true)]
        static bool ValidateSwitchView()
        {
            return !State.disabled;
        }
    }
}
#endif