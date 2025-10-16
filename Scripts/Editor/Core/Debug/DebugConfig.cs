using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyTools
{
    [CreateAssetMenu(fileName = "DebugConfig", menuName = "MyTools/Debug Config")]
    public sealed class DebugConfig : ScriptableObject
    {
        [Serializable]
        public struct Channel
        {
            [ReadOnly] public string name;
            public bool enabled;
        }

        [SerializeField] private List<Channel> channels = new();

        private Dictionary<string, bool> _lookup;

        public IReadOnlyList<Channel> Channels => channels;

        public bool IsEnabled(string name)
        {
            if (_lookup == null) RebuildLookup();
            return _lookup.TryGetValue(name, out var on) && on;
        }

        public void Set(string name, bool on)
        {
            var idx = channels.FindIndex(c => c.name == name);
            if (idx >= 0) channels[idx] = new Channel { name = name, enabled = on };
            else channels.Add(new Channel { name = name, enabled = on });

            RebuildLookup();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void RebuildLookup()
        {
            _lookup = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (var c in channels)
            {
                if (string.IsNullOrWhiteSpace(c.name)) continue;
                _lookup[c.name] = c.enabled;
            }
        }

        [Button(ButtonSizes.Medium)]
        [GUIColor(1f, 0.4f, 0.4f)]
        private void CleanUpChannels()
        {
            if (channels.Count == 0)
                return;

            channels.Clear();
            RebuildLookup();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() => RebuildLookup();
#endif
    }
}