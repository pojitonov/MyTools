#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    static class Assets
    {
        [MenuItem(Menus.ASSETS_MENU + "Create Prefab from Selection", priority = Menus.ASSETS_INDEX + 100)]
        private static void CreatePrefabFromSelectedFBX()
        {
            if (State.disabled) return;

            System.Object[] selectedObjects = Selection.objects;

            foreach (var selectedObject in selectedObjects)
            {
                string path = AssetDatabase.GetAssetPath((Object)selectedObject);
                if (Path.GetExtension(path).ToLower() == ".fbx")
                {
                    GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!fbxModel)
                    {
                        Debug.LogError(Debug.DefaultPrefix, $"Could not load FBX model at path: {path}");
                        return;
                    }

                    string prefabPath = Path.ChangeExtension(path, ".prefab");
                    PrefabUtility.SaveAsPrefabAsset(fbxModel, prefabPath);

                    Debug.Log(Debug.DefaultPrefix, $"Prefab created at: {prefabPath}");
                }
                else
                {
                     Debug.LogWarning(Debug.DefaultPrefix, $"Selected object is not an FBX file: {path}");
                }
            }
        }

        [MenuItem(Menus.ASSETS_MENU + "Create Prefab from Selection", validate = true)]
        private static bool ValidateCreatePrefabFromSelectedFBX() => !State.disabled;

        [MenuItem(Menus.ASSETS_MENU + "Apply Prefab Overrides %&a", priority = Menus.ASSETS_INDEX + 101)] // Ctl+Alt+A
        public static void ApplySelectedPrefabOverrides()
        {
            if (State.disabled) return;

            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning(Debug.DefaultPrefix, "No GameObjects selected.");
                return;
            }

            foreach (var obj in selectedObjects)
            {
                GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                if (prefabRoot != null)
                {
                    PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.UserAction);
                    Debug.Log(Debug.DefaultPrefix, $"Applied overrides to {prefabRoot.name}");
                }
                else
                {
                    Debug.LogWarning(Debug.DefaultPrefix, $"No prefab found for {obj.name}");
                }
            }

            Selection.activeGameObject = null;
            EditorApplication.delayCall += () => Selection.objects = selectedObjects;
        }

        [MenuItem(Menus.ASSETS_MENU + "Apply Prefab Overrides %&a", validate = true)]
        private static bool ValidateApplySelectedPrefabOverrides() => !State.disabled;

        [MenuItem(Menus.ASSETS_MENU + "Force Refresh Assets", priority = Menus.ASSETS_INDEX + 102)]
        private static void ForceRefreshSelectedAsset()
        {
            if (State.disabled) return;

            var selectedObjects = Selection.objects;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                AssetDatabase.Refresh();
                Debug.Log(Debug.DefaultPrefix, "All assets have been refreshed.");
            }
            else
            {
                foreach (var obj in selectedObjects)
                {
                    string assetPath = AssetDatabase.GetAssetPath(obj);

                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        Debug.Log(Debug.DefaultPrefix, $"{assetPath} has been refreshed.");
                    }
                }
            }
        }

        [MenuItem(Menus.ASSETS_MENU + "Force Refresh Assets", validate = true)]
        private static bool ValidateForceRefreshSelectedAsset() => !State.disabled;

        [MenuItem(Menus.ASSETS_MENU + "Create Children LOD Groups", priority = Menus.ASSETS_INDEX + 103)]
        static void CopyLODGroupToFirstLevelChildren()
        {
            if (State.disabled) return;

            GameObject selected = Selection.activeGameObject;

            if (selected == null)
            {
                Debug.LogError(Debug.DefaultPrefix, "No GameObject selected.");
                return;
            }

            LODGroup rootLODGroup = selected.GetComponent<LODGroup>();

            if (rootLODGroup == null)
            {
                Debug.LogError(Debug.DefaultPrefix, "Selected GameObject does not have an LODGroup component.");
                return;
            }

            foreach (Transform child in selected.transform)
            {
                if (child == null) continue;

                LODGroup childLODGroup = child.GetComponent<LODGroup>();

                if (childLODGroup != null)
                {
                    Undo.DestroyObjectImmediate(childLODGroup);
                }

                LODGroup newLODGroup = child.gameObject.AddComponent<LODGroup>();

                CopyLODGroupSettings(rootLODGroup, newLODGroup);
                RemoveAllRenderersFromLODGroup(newLODGroup);
                AssignRenderersToLODGroup(newLODGroup, child);
                SetTransitionScreenSize(newLODGroup);
            }

            Undo.DestroyObjectImmediate(rootLODGroup);

            Debug.Log(Debug.DefaultPrefix, "LOD Group successfully copied to first-level children, renderers assigned, transition sizes set, and LOD Group removed from parent.");
        }

        [MenuItem(Menus.ASSETS_MENU + "Create Children LOD Groups", validate = true)]
        private static bool ValidateCopyLODGroupToFirstLevelChildren() => !State.disabled;

        static void CopyLODGroupSettings(LODGroup source, LODGroup destination)
        {
            if (source == null || destination == null)
            {
                Debug.LogError(Debug.DefaultPrefix, "Source or destination LODGroup is null.");
                return;
            }

            LOD[] lods = source.GetLODs();
            destination.SetLODs(lods);
            destination.fadeMode = source.fadeMode;
            destination.animateCrossFading = source.animateCrossFading;
        }

        static void RemoveAllRenderersFromLODGroup(LODGroup lodGroup)
        {
            LOD[] lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                lods[i].renderers = new Renderer[0];
            }

            lodGroup.SetLODs(lods);
        }

        static void AssignRenderersToLODGroup(LODGroup lodGroup, Transform child)
        {
            List<Renderer> renderersLOD0 = new List<Renderer>();
            List<Renderer> renderersLOD1 = new List<Renderer>();
            List<Renderer> renderersLOD2 = new List<Renderer>();

            foreach (Transform lodChild in child)
            {
                string lodName = lodChild.gameObject.name;

                if (lodName.EndsWith("_LOD0"))
                {
                    renderersLOD0.Add(lodChild.GetComponent<Renderer>());
                }
                else if (lodName.EndsWith("_LOD1"))
                {
                    renderersLOD1.Add(lodChild.GetComponent<Renderer>());
                }
                else if (lodName.EndsWith("_LOD2"))
                {
                    renderersLOD2.Add(lodChild.GetComponent<Renderer>());
                }
            }

            List<LOD> lods = new List<LOD>();

            if (renderersLOD0.Count > 0)
            {
                lods.Add(new LOD(1f, renderersLOD0.ToArray())); // Full priority
            }

            if (renderersLOD1.Count > 0)
            {
                lods.Add(new LOD(0.5f, renderersLOD1.ToArray())); // Half priority
            }

            if (renderersLOD2.Count > 0)
            {
                lods.Add(new LOD(0.25f, renderersLOD2.ToArray())); // Quarter priority
            }

            lodGroup.SetLODs(lods.ToArray());
        }

        static void SetTransitionScreenSize(LODGroup lodGroup)
        {
            LOD[] lods = lodGroup.GetLODs();

            for (int i = 0; i < lods.Length; i++)
            {
                if (lods.Length == 2)
                {
                    switch (i)
                    {
                        case 0: lods[i].screenRelativeTransitionHeight = 0.25f; break;
                        case 1: lods[i].screenRelativeTransitionHeight = 0.01f; break;
                    }
                }
                else if (lods.Length == 3)
                {
                    switch (i)
                    {
                        case 0: lods[i].screenRelativeTransitionHeight = 0.25f; break;
                        case 1: lods[i].screenRelativeTransitionHeight = 0.13f; break;
                        case 2: lods[i].screenRelativeTransitionHeight = 0.01f; break;
                    }
                }
            }

            lodGroup.SetLODs(lods);
        }
    }
}
#endif
