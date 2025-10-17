#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace MyTools
{
    public class XRRigBuild : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (!report)
                return;

            Debug.Log("EnableVRRigInBuild", $"Processing scene: {scene.name}");

            GameObject vrRig = FindVRRigInScene();

            if (vrRig)
            {
                if (!vrRig.activeSelf)
                {
                    vrRig.SetActive(true);
                    Debug.Log("EnableVRRigInBuild", $"Enabled VR Rig for build: {vrRig.name}");
                }
                else
                {
                    Debug.Log("EnableVRRigInBuild", $"VR Rig already enabled: {vrRig.name}");
                }
            }
            else
            {
                Debug.LogWarning("EnableVRRigInBuild", $"No VR Rig found in scene: {scene.name}");
            }
        }

        private GameObject FindVRRigInScene()
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
                obj.name is "OVRCameraRig" or "VRRig" or "CameraRig" or "[BuildingBlock] Camera Rig");
        }
    }
}
#endif