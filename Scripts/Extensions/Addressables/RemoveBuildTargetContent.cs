#if UNITY_EDITOR && HAS_ADDRESSABLES
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace MyTools
{
    internal static class RemoveBuildTargetContent
    {
        private const string MENU_PATH = "Tools/Remove Content for Build Target";
        private const int MENU_PRIORITY = 1000;

        [MenuItem(MENU_PATH, priority = MENU_PRIORITY)]
        private static void RemoveContent()
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

                var pathsToClean = new System.Collections.Generic.List<string>();

                if (!string.IsNullOrEmpty(localBuildPath))
                {
                    var resolvedLocalPath = profileSettings.EvaluateString(activeProfileId, localBuildPath);
                    if (Directory.Exists(resolvedLocalPath))
                    {
                        pathsToClean.Add(resolvedLocalPath);
                    }
                }

                if (!string.IsNullOrEmpty(remoteBuildPath))
                {
                    var resolvedRemotePath = profileSettings.EvaluateString(activeProfileId, remoteBuildPath);
                    if (Directory.Exists(resolvedRemotePath))
                    {
                        pathsToClean.Add(resolvedRemotePath);
                    }
                }

                if (pathsToClean.Count == 0)
                {
                    Debug.Log(Debug.DefaultPrefix, $"No build target folders found for {buildTargetName}. Nothing to remove.");
                    return;
                }

                var message = $"This will delete all files in the following folders for {buildTargetName}:\n\n";
                foreach (var path in pathsToClean)
                {
                    message += $"• {path}\n";
                }
                message += "\nThis action cannot be undone. Continue?";

                if (!EditorUtility.DisplayDialog("Remove Content for Build Target", message, "Yes", "No"))
                {
                    return;
                }

                int totalFilesRemoved = 0;
                int totalDirectoriesRemoved = 0;
                bool anyRemoved = false;

                foreach (var path in pathsToClean)
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            var directory = new DirectoryInfo(path);
                            var fileCount = directory.GetFiles().Length;
                            var dirCount = directory.GetDirectories().Length;
                            
                            if (fileCount == 0 && dirCount == 0)
                            {
                                continue;
                            }

                            var (filesRemoved, dirsRemoved) = DeleteDirectoryContents(path);
                            totalFilesRemoved += filesRemoved;
                            totalDirectoriesRemoved += dirsRemoved;
                            anyRemoved = true;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError(Debug.DefaultPrefix, $"Failed to remove contents from {path}: {ex.Message}");
                    }
                }

                if (!anyRemoved)
                {
                    Debug.Log(Debug.DefaultPrefix, $"No content removed - all folders were empty for {buildTargetName}.");
                }
                else
                {
                    Debug.Log(Debug.DefaultPrefix, $"Successfully removed {totalFilesRemoved} file(s) and {totalDirectoriesRemoved} folder(s) total for {buildTargetName}.");
                }

                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(Debug.DefaultPrefix, $"Error removing build target content: {ex.Message}");
            }
        }

        private static (int filesRemoved, int directoriesRemoved) DeleteDirectoryContents(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            int filesRemoved = 0;
            int directoriesRemoved = 0;
            
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
                filesRemoved++;
            }
            
            foreach (var subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
                directoriesRemoved++;
            }

            return (filesRemoved, directoriesRemoved);
        }

        [MenuItem(MENU_PATH, validate = true, priority = MENU_PRIORITY)]
        private static bool ValidateRemoveContent()
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
