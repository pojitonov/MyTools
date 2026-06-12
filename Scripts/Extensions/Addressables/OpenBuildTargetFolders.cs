#if UNITY_EDITOR && HAS_ADDRESSABLES
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace MyTools
{
    internal static class OpenBuildTargetFolders
    {
        private const string MENU_PATH = "Tools/Open Build Target Folders";
        private const int MENU_PRIORITY = 1001;

        [MenuItem(MENU_PATH, priority = MENU_PRIORITY)]
        private static void OpenFolders()
        {
            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogError(Debug.DefaultPrefix, "Addressables settings not found. Please ensure Addressables package is installed and configured.");
                    return;
                }

                var profileSettings = settings.profileSettings;
                if (profileSettings == null)
                {
                    Debug.LogError(Debug.DefaultPrefix, "Addressables profile settings not found.");
                    return;
                }

                var activeProfileId = settings.activeProfileId;
                if (string.IsNullOrEmpty(activeProfileId))
                {
                    Debug.LogError(Debug.DefaultPrefix, "No active Addressables profile found.");
                    return;
                }

                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var buildTargetName = buildTarget.ToString();

                var localBuildPath = profileSettings.GetValueByName(activeProfileId, "Local.BuildPath");
                var remoteBuildPath = profileSettings.GetValueByName(activeProfileId, "Remote.BuildPath");

                if (string.IsNullOrEmpty(localBuildPath) && string.IsNullOrEmpty(remoteBuildPath))
                {
                    Debug.LogWarning(Debug.DefaultPrefix, "Both Local.BuildPath and Remote.BuildPath are empty or not found in profile.");
                    return;
                }

                bool anyOpened = false;

                if (!string.IsNullOrEmpty(localBuildPath))
                {
                    var resolvedLocalPath = profileSettings.EvaluateString(activeProfileId, localBuildPath);
                    if (OpenFolder(resolvedLocalPath, "Local"))
                    {
                        anyOpened = true;
                    }
                }

                if (!string.IsNullOrEmpty(remoteBuildPath))
                {
                    var resolvedRemotePath = profileSettings.EvaluateString(activeProfileId, remoteBuildPath);
                    if (OpenFolder(resolvedRemotePath, "Remote"))
                    {
                        anyOpened = true;
                    }
                }

                if (!anyOpened)
                {
                    Debug.LogWarning(Debug.DefaultPrefix, $"No build target folders found or opened for {buildTargetName}.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(Debug.DefaultPrefix, $"Error opening build target folders: {ex.Message}");
            }
        }

        private static bool OpenFolder(string path, string folderType)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                path = Path.GetFullPath(path);

                if (!Directory.Exists(path))
                {
                    var parentPath = Path.GetDirectoryName(path);
                    while (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
                    {
                        parentPath = Path.GetDirectoryName(parentPath);
                    }

                    if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
                    {
                        Debug.LogWarning(Debug.DefaultPrefix, $"{folderType} folder does not exist: {path}\nOpening parent directory instead: {parentPath}");
                        EditorUtility.RevealInFinder(parentPath);
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning(Debug.DefaultPrefix, $"{folderType} folder does not exist and could not find parent directory: {path}");
                        return false;
                    }
                }

                EditorUtility.RevealInFinder(path);
                Debug.Log(Debug.DefaultPrefix, $"Opened {folderType} folder: {path}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(Debug.DefaultPrefix, $"Failed to open {folderType} folder at {path}: {ex.Message}");
                return false;
            }
        }

        [MenuItem(MENU_PATH, validate = true, priority = MENU_PRIORITY)]
        private static bool ValidateOpenFolders()
        {
            try
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                return settings != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif
