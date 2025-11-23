#if ODIN_INSPECTOR || SIRENIX_ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace MyTools
{
    public sealed class DebugController : MonoBehaviour
    {
#if ODIN_INSPECTOR || SIRENIX_ODIN_INSPECTOR
        [InlineEditor(Expanded = true)]
#endif
        [SerializeField] private DebugConfig config;

        private void Awake()
        {
            Debug.SetConfig(config);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (config) config.RebuildLookup();
        }
#endif
    }
}