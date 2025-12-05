#if UNITY_6000_0_OR_NEWER && UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MyTools
{
    [Overlay(typeof(SceneView), "My Gizmos")]
    internal class GizmosOverlay : ToolbarOverlay
    {
        private GizmosOverlay() : base(GizmosDropdown.id)
        {
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    internal class GizmosDropdown : EditorToolbarDropdown
    {
        public const string id = "GizmosDropdown";

        public GizmosDropdown()
        {
            text = "Gizmos";
            clicked += ShowDropdown;
        }

        private static void ShowDropdown()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Icons"), ToggleAllIcons.IconsEnabled, () => { ToggleAllIcons.ToggleIcons(); });
            menu.AddItem(new GUIContent("3D Icons"), GizmoUtility.use3dIcons,
                () => { ToggleGizmos.ToggleIcons(!GizmoUtility.use3dIcons); });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Selection Outline"), ToggleGizmos.GetSelectionOutlineState(),
                () => { ToggleGizmos.ToggleSelectionOutline(!ToggleGizmos.GetSelectionOutlineState()); });
            menu.AddItem(new GUIContent("Selection Wire"), ToggleGizmos.GetSelectionWireState(),
                () => { ToggleGizmos.ToggleSelectionWire(!ToggleGizmos.GetSelectionWireState()); });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Camera On"), false,
                () => { ToggleGizmos.ToggleCameraGizmos(true); });
            menu.AddItem(new GUIContent("Camera Off"), false,
                () => { ToggleGizmos.ToggleCameraGizmos(false); });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Canvas On"), false,
                () => { ToggleGizmos.ToggleCanvasGizmos(true); });
            menu.AddItem(new GUIContent("Canvas Off"), false,
                () => { ToggleGizmos.ToggleCanvasGizmos(false); });

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Colliders On"), false,
                () =>
                {
                    ToggleColliders.Toggle3DColliders(true);
                    ToggleColliders.Toggle2DColliders(true);
                });
            menu.AddItem(new GUIContent("Colliders Off"), false,
                () =>
                {
                    ToggleColliders.Toggle3DColliders(false);
                    ToggleColliders.Toggle2DColliders(false);
                });

            menu.ShowAsContext();
        }
    }

    public static class ToggleColliders
    {
        public static bool Colliders3DEnabled { get; private set; } = true;
        public static bool Colliders2DEnabled { get; private set; } = true;

        public static void Toggle3DColliders(bool state)
        {
            Colliders3DEnabled = state;
            var colliderTypes = new[]
            {
                typeof(BoxCollider),
                typeof(CapsuleCollider),
                typeof(MeshCollider),
                typeof(SphereCollider),
                typeof(WheelCollider)
            };

            SetGizmosEnabledForTypes(colliderTypes, state);
            Utils.ClearConsole();
            SceneView.RepaintAll();
        }

        public static void Toggle2DColliders(bool state)
        {
            Colliders2DEnabled = state;
            var collider2DTypes = new[]
            {
                typeof(BoxCollider2D),
                typeof(CapsuleCollider2D),
                typeof(CircleCollider2D),
                typeof(CompositeCollider2D),
                typeof(CustomCollider2D),
                typeof(EdgeCollider2D),
                typeof(PolygonCollider2D),
                typeof(TilemapCollider2D)
            };

            SetGizmosEnabledForTypes(collider2DTypes, state);
            Utils.ClearConsole();
            SceneView.RepaintAll();
        }

        private static void SetGizmosEnabledForTypes(Type[] types, bool state)
        {
            foreach (var type in types)
            {
                GizmoUtility.SetGizmoEnabled(type, state);
            }
        }
    }

    public static class ToggleGizmos
    {
        public static bool IsCameraGizmoEnabled { get; private set; } = true;
        public static bool IsCanvasGizmoEnabled { get; private set; } = true;

        public static void ToggleIcons(bool state)
        {
            GizmoUtility.use3dIcons = state;
            SceneView.RepaintAll();
        }

        public static void ToggleSelectionOutline(bool state)
        {
            SetAnnotationUtilityProperty("showSelectionOutline", state);
            SceneView.RepaintAll();
        }

        public static bool GetSelectionOutlineState() => GetAnnotationUtilityProperty("showSelectionOutline");

        public static void ToggleSelectionWire(bool state)
        {
            SetAnnotationUtilityProperty("showSelectionWire", state);
            SceneView.RepaintAll();
        }

        public static bool GetSelectionWireState() => GetAnnotationUtilityProperty("showSelectionWire");

        public static void ToggleCameraGizmos(bool state)
        {
            IsCameraGizmoEnabled = state;
            GizmoUtility.SetGizmoEnabled(typeof(Camera), state);
            SceneView.RepaintAll();
        }

        public static void ToggleCanvasGizmos(bool state)
        {
            IsCanvasGizmoEnabled = state;
            GizmoUtility.SetGizmoEnabled(typeof(Canvas), state);
            SceneView.RepaintAll();
        }

        private static void SetAnnotationUtilityProperty(string propertyName, bool state)
        {
            var property = GetAnnotationUtilityPropertyInfo(propertyName);
            property?.SetValue(null, state);
        }

        private static bool GetAnnotationUtilityProperty(string propertyName)
        {
            var property = GetAnnotationUtilityPropertyInfo(propertyName);
            return property != null && (bool)property.GetValue(null);
        }

        private static PropertyInfo GetAnnotationUtilityPropertyInfo(string propertyName)
        {
            var asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = asm?.GetType("UnityEditor.AnnotationUtility");
            return type?.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic);
        }
    }

    public static class ToggleAllIcons
    {
        public static bool IconsEnabled { get; private set; } = true;

        public static void ToggleIcons()
        {
            IconsEnabled = !IconsEnabled;
            var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(Component).IsAssignableFrom(type) && !type.IsAbstract);

            foreach (var type in componentTypes)
            {
                GizmoUtility.SetIconEnabled(type, IconsEnabled);
            }

            Utils.ClearConsole();
            SceneView.RepaintAll();
        }
    }
}
#endif