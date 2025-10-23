#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    public static class XRRigTools
    {
        private const string VR_RIG_PREF_KEY = "MyTools_VRRig_InstanceID";
        private const string VR_RIG_STATE_PREF_KEY = "MyTools_VRRig_SavedState";
        private static GameObject cachedVRRig;

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            var vrRig = GetVRRig();
            if (!vrRig) return;

            EnableVRRigImmediately(vrRig);
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var vrRig = GetVRRig();
            if (!vrRig) return;

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    SaveVRRigStateAndEnable(vrRig);
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += RestoreVRRigState;
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += EnsureVRRigEnabled;
                    break;
            }
        }

        private static void SaveVRRigStateAndEnable(GameObject vrRig)
        {
            var wasEnabled = vrRig.activeSelf;
            EditorPrefs.SetBool(VR_RIG_STATE_PREF_KEY, wasEnabled);

            if (!wasEnabled)
                vrRig.SetActive(true);
        }

        private static void RestoreVRRigState()
        {
            var vrRig = GetVRRig();
            if (!vrRig) return;

            bool wasEnabled = EditorPrefs.GetBool(VR_RIG_STATE_PREF_KEY, true);
            vrRig.SetActive(wasEnabled);
            EditorPrefs.DeleteKey(VR_RIG_STATE_PREF_KEY);
        }

        private static void EnableVRRigImmediately(GameObject vrRig)
        {
            if (!vrRig.activeSelf)
                vrRig.SetActive(true);
        }

        private static void EnsureVRRigEnabled()
        {
            var vrRig = GetVRRig();
            if (!vrRig) return;

            if (!vrRig.activeSelf)
                vrRig.SetActive(true);
        }

        private static GameObject GetVRRig()
        {
            if (cachedVRRig && IsValidSceneObject(cachedVRRig))
                return cachedVRRig;

            if (!Application.isPlaying)
            {
                int savedInstanceID = EditorPrefs.GetInt(VR_RIG_PREF_KEY, 0);
                if (savedInstanceID != 0)
                {
                    cachedVRRig = EditorUtility.InstanceIDToObject(savedInstanceID) as GameObject;
                    if (cachedVRRig && IsValidSceneObject(cachedVRRig) && IsVRRig(cachedVRRig))
                        return cachedVRRig;

                    cachedVRRig = null;
                    EditorPrefs.DeleteKey(VR_RIG_PREF_KEY);
                }
            }

            cachedVRRig = FindVRRigInScene();
            if (cachedVRRig && !Application.isPlaying)
                EditorPrefs.SetInt(VR_RIG_PREF_KEY, cachedVRRig.GetInstanceID());

            return cachedVRRig;
        }

        private static bool IsVRRig(GameObject obj)
        {
            if (!obj) return false;
            if (obj.GetComponent("OVRCameraRig")) return true;

            return obj.name is "OVRCameraRig" or "VRRig" or "CameraRig";
        }

        private static GameObject FindVRRigInScene()
        {
            var allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.scene.IsValid())
                .ToArray();

            return allGameObjects.FirstOrDefault(obj =>
                obj.GetComponent("OVRCameraRig") ||
                obj.name is "OVRCameraRig" or "VRRig" or "CameraRig");
        }

        private static bool IsValidSceneObject(GameObject obj)
        {
            return obj && obj.scene.IsValid();
        }

        public static void ToggleVRRig()
        {
            var vrRig = GetVRRig();
            if (!vrRig) return;

            vrRig.SetActive(!vrRig.activeSelf);
        }
    }
}
#endif