#if UNITY_6000_0_OR_NEWER && UNITY_EDITOR
using UnityEditor;

namespace MyTools
{
    [InitializeOnLoad]
    public static class SceneViewAutoSave
    {
        static SceneViewAutoSave()
        {
            EditorApplication.quitting += SaveCurrentView;
            EditorApplication.playModeStateChanged += _ => SaveCurrentView();
            EditorApplication.focusChanged += _ => SaveCurrentView();
        }

        private static void SaveCurrentView()
        {
            if (State.disabled) return;

            var sv = SceneView.lastActiveSceneView;
            if (sv == null) return;

            SceneViewNavigationIO.SaveLastViewState(sv.size, sv.rotation, sv.pivot, sv.orthographic);

            var currentType = SceneViewNavigationManager.GetCurrentViewType(sv);

            ActiveSceneView.sceneView = sv;
            SceneViewNavigationIO.SaveViewState(
                currentType,
                sv.size, sv.rotation, sv.pivot, sv.orthographic
            );

            SceneViewNavigationIO.WriteToEditorPrefs(currentType);
        }
    }
}
#endif