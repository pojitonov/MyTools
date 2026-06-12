#if UNITY_EDITOR && HAS_ADDRESSABLES
using UnityEditor;
using UnityEngine;

public static class ClearAddressableCache
{
    [MenuItem("Tools/Clear Addressable Cache")]
    private static void ClearCache()
    {
        Caching.ClearCache();
    }
}

#endif