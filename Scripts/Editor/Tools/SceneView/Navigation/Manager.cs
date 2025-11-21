#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace MyTools
{
    public static class SceneViewNavigationManager
    {
        public static void SetView(SceneViewType viewType)
        {
            ActiveSceneView.sceneView = SceneView.lastActiveSceneView;
            if (ActiveSceneView.sceneView == null)
                return;

            if (SceneViewNavigationIO.TryConsumeUseLastPoseRequest() &&
                SceneViewNavigationIO.TryGetLastViewState(out var last))
            {
                ApplyNewValues(last, viewType);
                ActiveSceneView.sceneView.Repaint();
                return;
            }

            if (SceneViewNavigationIO.TryGetViewState(viewType, out var savedState))
            {
                ApplyNewValues(savedState, viewType);
            }
            else
            {
                ApplyDefaultValues(viewType);
            }

            ActiveSceneView.sceneView.Repaint();
        }

        public static SceneViewType GetCurrentViewType(SceneView sv)
        {
            if (sv == null) return ActiveSceneView.SceneViewType;

            var rot = sv.rotation;

            (SceneViewType type, Quaternion quat)[] candidates =
            {
                (SceneViewType.Top, DefaultRotation.Top),
                (SceneViewType.Bottom, DefaultRotation.Bottom),
                (SceneViewType.Front, DefaultRotation.Front),
                (SceneViewType.Back, DefaultRotation.Back),
                (SceneViewType.Left, DefaultRotation.Left),
                (SceneViewType.Right, DefaultRotation.Right),
                (SceneViewType.Perspective, DefaultRotation.Perspective),
            };

            float best = float.MaxValue;
            SceneViewType bestType = SceneViewType.Perspective;

            foreach (var c in candidates)
            {
                float angle = Quaternion.Angle(rot, c.quat);
                if (angle < best)
                {
                    best = angle;
                    bestType = c.type;
                }
            }

            const float axisSnapTolerance = 7.5f;

            if (bestType == SceneViewType.Perspective)
            {
                float bestAxis = Mathf.Min(
                    Quaternion.Angle(rot, DefaultRotation.Top),
                    Quaternion.Angle(rot, DefaultRotation.Bottom),
                    Quaternion.Angle(rot, DefaultRotation.Front),
                    Quaternion.Angle(rot, DefaultRotation.Back),
                    Quaternion.Angle(rot, DefaultRotation.Left),
                    Quaternion.Angle(rot, DefaultRotation.Right)
                );

                if (bestAxis <= axisSnapTolerance)
                {
                    best = float.MaxValue;
                    foreach (var c in candidates)
                    {
                        if (c.type == SceneViewType.Perspective) continue;
                        float angle = Quaternion.Angle(rot, c.quat);
                        if (angle < best)
                        {
                            best = angle;
                            bestType = c.type;
                        }
                    }
                }
            }
            else
            {
                if (best > axisSnapTolerance)
                    bestType = SceneViewType.Perspective;
            }

            return bestType;
        }

        private static void ApplyDefaultValues(SceneViewType viewType)
        {
            ActiveSceneView.sceneView.size = DefaultValues.size;
            ActiveSceneView.sceneView.pivot = DefaultValues.pivot;
            ActiveSceneView.sceneView.rotation = GetDefaultRotation(viewType);
            ActiveSceneView.sceneView.orthographic = IsOrthographic(viewType);
        }

        private static void ApplyNewValues(SceneViewNavigationIO.ViewState savedState, SceneViewType viewType)
        {
            ActiveSceneView.sceneView.size = savedState.size;
            ActiveSceneView.sceneView.pivot = savedState.pivot;
            ActiveSceneView.sceneView.rotation = savedState.rotation;
            ActiveSceneView.sceneView.orthographic = IsOrthographic(viewType);
        }

        public static void SaveSceneView(SceneViewType viewTypeAboutToSet)
        {
            if (ActiveSceneView.sceneView != null)
            {
                var sv = ActiveSceneView.sceneView;

                var currentType = GetCurrentViewType(sv);

                SceneViewNavigationIO.SaveViewState(
                    currentType,
                    sv.size, sv.rotation, sv.pivot, sv.orthographic
                );
                SceneViewNavigationIO.SaveLastViewState(sv.size, sv.rotation, sv.pivot, sv.orthographic);
                SceneViewNavigationIO.WriteToEditorPrefs(viewTypeAboutToSet);
            }
        }

        public static void ResetAllSceneViews()
        {
            var sceneViewTypes = Enum.GetValues(typeof(SceneViewType)) as SceneViewType[];
            foreach (var viewType in sceneViewTypes)
            {
                ResetView(viewType);
            }

            RedrawLastSavedSceneView();
        }

        private static void ResetView(SceneViewType viewType)
        {
            float size = DefaultValues.size;
            Vector3 pivot = DefaultValues.pivot;
            Quaternion rotation = GetDefaultRotation(viewType);
            bool orthographic = IsOrthographic(viewType);

            SceneViewNavigationIO.SaveViewState(viewType, size, rotation, pivot, orthographic);
        }

        public static void RedrawLastSavedSceneView()
        {
            var lastSavedViewType = SceneViewNavigationIO.ReadFromEditorPrefs();
            Quaternion rotation = GetDefaultRotation(lastSavedViewType);
            bool orthographic = IsOrthographic(lastSavedViewType);

            ActiveSceneView.sceneView = SceneView.lastActiveSceneView;
            ActiveSceneView.sceneView.size = DefaultValues.size;
            ActiveSceneView.sceneView.pivot = DefaultValues.pivot;
            ActiveSceneView.sceneView.rotation = rotation;
            ActiveSceneView.sceneView.orthographic = orthographic;
            ActiveSceneView.sceneView.Repaint();
        }

        private static Quaternion GetDefaultRotation(SceneViewType viewType)
        {
            return viewType switch
            {
                SceneViewType.Perspective => DefaultRotation.Perspective,
                SceneViewType.Top => DefaultRotation.Top,
                SceneViewType.Bottom => DefaultRotation.Bottom,
                SceneViewType.Front => DefaultRotation.Front,
                SceneViewType.Back => DefaultRotation.Back,
                SceneViewType.Left => DefaultRotation.Left,
                SceneViewType.Right => DefaultRotation.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null)
            };
        }

        private static bool IsOrthographic(SceneViewType viewType)
        {
            return viewType != SceneViewType.Perspective;
        }
    }
}
#endif