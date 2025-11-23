#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    internal static class AudioPreview
    {
        private static AudioSource audioSource;
        private static GameObject audioSourceObject;
        private static AudioClip lastSelectedClip;

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject is AudioClip selectedClip)
            {
                if (audioSource && audioSource.isPlaying && lastSelectedClip != selectedClip)
                {
                    audioSource.Stop();
                    DestroyAudioSource();
                }

                lastSelectedClip = selectedClip;
            }
        }

        [MenuItem(Menus.EDITOR_MENU + "Audio Preview _SPACE", priority = Menus.EDITOR_INDEX + 300)]
        private static void PlaySelectedAudioClip()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject is AudioClip selectedClip)
            {
                ToggleAudioClip(selectedClip);
            }
        }

        private static void ToggleAudioClip(AudioClip clip)
        {
            if (audioSource && clip)
            {
                if (audioSource.isPlaying && audioSource.clip == clip)
                {
                    audioSource.Stop();
                    DestroyAudioSource();
                }
                else
                {
                    PlayAudioClip(clip);
                }
            }
            else
            {
                PlayAudioClip(clip);
            }
        }

        private static void PlayAudioClip(AudioClip clip)
        {
            if (!audioSourceObject)
            {
                audioSourceObject = new GameObject("EditorAudioSource")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                audioSource = audioSourceObject.AddComponent<AudioSource>();
            }

            audioSource.clip = clip;
            audioSource.Play();
        }

        private static void DestroyAudioSource()
        {
            if (audioSourceObject)
            {
                Object.DestroyImmediate(audioSourceObject);
                audioSourceObject = null;
                audioSource = null;
            }
        }
    }
}
#endif