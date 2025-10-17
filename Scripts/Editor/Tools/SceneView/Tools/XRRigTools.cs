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
            Debug.Log("OnEnterPlayMode - Enabling VR Rig FIRST");
            EnableVRRigImmediately();
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("ExitingEditMode - Saving VR Rig state and enabling it");
                SaveVRRigStateAndEnable();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("EnteredEditMode - Restoring VR Rig state");
                EditorApplication.delayCall += RestoreVRRigState;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log("EnteredPlayMode - Ensuring VR Rig is enabled");
                EditorApplication.delayCall += EnsureVRRigEnabled;
            }
        }

        private static void SaveVRRigStateAndEnable()
        {
            cachedVRRig = null;
            EditorPrefs.DeleteKey(VR_RIG_PREF_KEY);

            GameObject vrRig = GetVRRig();

            if (vrRig)
            {
                var wasEnabled = vrRig.activeSelf;
                EditorPrefs.SetBool(VR_RIG_STATE_PREF_KEY, wasEnabled);
                Debug.Log($"Saved VR Rig state to EditorPrefs: {(wasEnabled ? "enabled" : "disabled")}");

                if (!vrRig.activeSelf)
                {
                    vrRig.SetActive(true);
                    Debug.Log($"ENABLED VR Rig for play mode: {vrRig.name}");
                }
                else
                {
                    Debug.Log($"VR Rig {vrRig.name} was already enabled");
                }
            }
            else
            {
                Debug.LogWarning("No VR Rig found to save state");
            }
        }

        private static void RestoreVRRigState()
        {
            GameObject vrRig = GetVRRig();

            if (vrRig)
            {
                bool wasEnabled = EditorPrefs.GetBool(VR_RIG_STATE_PREF_KEY, true);
                vrRig.SetActive(wasEnabled);
                Debug.Log($"RESTORED VR Rig to {(wasEnabled ? "enabled" : "disabled")}: {vrRig.name}");

                EditorPrefs.DeleteKey(VR_RIG_STATE_PREF_KEY);
            }
            else
            {
                Debug.LogWarning("No VR Rig found to restore state");
            }
        }

        private static void EnableVRRigImmediately()
        {
            cachedVRRig = null;
            EditorPrefs.DeleteKey(VR_RIG_PREF_KEY);

            GameObject vrRig = GetVRRig();

            if (vrRig)
            {
                if (!vrRig.activeSelf)
                {
                    vrRig.SetActive(true);
                    Debug.Log($"IMMEDIATELY ENABLED VR Rig: {vrRig.name}");
                }
                else
                {
                    Debug.Log($"VR Rig {vrRig.name} was already enabled");
                }
            }
            else
            {
                Debug.LogWarning("No VR Rig found for immediate enable");
            }
        }

        private static void EnsureVRRigEnabled()
        {
            GameObject vrRig = GetVRRig();

            if (vrRig && !vrRig.activeSelf)
            {
                vrRig.SetActive(true);
                Debug.Log($"ENSURE ENABLED VR Rig: {vrRig.name}");
            }
        }

        private static GameObject GetVRRig()
        {
            if (cachedVRRig && IsValidSceneObject(cachedVRRig))
            {
                return cachedVRRig;
            }

            if (!Application.isPlaying)
            {
                int savedInstanceID = EditorPrefs.GetInt(VR_RIG_PREF_KEY, 0);
                if (savedInstanceID != 0)
                {
                    cachedVRRig = EditorUtility.InstanceIDToObject(savedInstanceID) as GameObject;

                    if (cachedVRRig && IsValidSceneObject(cachedVRRig) && IsVRRig(cachedVRRig))
                    {
                        return cachedVRRig;
                    }

                    cachedVRRig = null;
                    EditorPrefs.DeleteKey(VR_RIG_PREF_KEY);
                }
            }

            cachedVRRig = FindVRRigInScene();

            if (cachedVRRig && !Application.isPlaying)
            {
                EditorPrefs.SetInt(VR_RIG_PREF_KEY, cachedVRRig.GetInstanceID());
            }

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

            foreach (GameObject obj in allGameObjects)
            {
                var component = obj.GetComponent("OVRCameraRig");
                if (component)
                {
                    return obj;
                }
            }

            return allGameObjects.FirstOrDefault(obj =>
                obj.name is "OVRCameraRig" or "VRRig" or "CameraRig");
        }

        private static bool IsValidSceneObject(GameObject obj)
        {
            return obj && obj.scene.IsValid();
        }

        public static void ToggleVRRig()
        {
            GameObject vrRig = GetVRRig();

            if (vrRig)
            {
                vrRig.SetActive(!vrRig.activeSelf);
                Debug.Log($"VR Rig {(vrRig.activeSelf ? "enabled" : "disabled")}: {vrRig.name}");
            }
            else
            {
                Debug.LogWarning("No OVRCameraRig found in the scene.");
            }
        }
    }
}
#endif