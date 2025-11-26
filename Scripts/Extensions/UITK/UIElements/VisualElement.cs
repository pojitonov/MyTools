using UnityEngine;
using UnityEngine.UIElements;

#if !UNITY_6000_0_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace MyTools.UIElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement(nameof(VisualElement))]
#endif
    public partial class VisualElement : ChildAnnotator
    {
        public const string gapX_name = "gap-X";
        public const string gapY_name = "gap-Y";

        private int _gapX;
        private int _gapY;

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute(gapX_name)]
#endif
        public int gapX
        {
            get => _gapX;
            set
            {
                _gapX = value;
                UpdateChildMargins();
            }
        }
#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute(gapY_name)]
#endif
        public int gapY
        {
            get => _gapY;
            set
            {
                _gapY = value;
                UpdateChildMargins();
            }
        }

        public VisualElement() : base()
        {
            _gapX = 0;
            _gapY = 0;

            this.RegisterCallback<ChangeEvent<FlexDirection>>(OnFlexDirectionChanged);
        }

        private void OnFlexDirectionChanged(ChangeEvent<FlexDirection> evt)
        {
            UpdateChildMargins();
        }

        protected override void OnChildChange(ChildChangeEvent evt)
        {
            base.OnChildChange(evt);
            UpdateChildMargins();
        }

        private void UpdateChildMargins()
        {
            for (int i = 0; i < childCount; i++)
            {
                UnityEngine.UIElements.VisualElement child = this[i];

                if (child == lastChild)
                {
                    child.style.marginRight = 0;
                    child.style.marginBottom = 0;
                }
                else
                {
                    child.style.marginRight = gapX;
                    child.style.marginBottom = gapY;
                }
            }
        }

#if !UNITY_6000_0_OR_NEWER
        [Preserve]
        public new class UxmlFactory : UxmlFactory<VisualElement, UxmlTraits> { }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlIntAttributeDescription gapXAttr = new UxmlIntAttributeDescription
            {
                name = VisualElement.gapX_name,
                defaultValue = 0
            };
            private UxmlIntAttributeDescription gapYAttr = new UxmlIntAttributeDescription
            {
                name = VisualElement.gapY_name,
                defaultValue = 0
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var item = (VisualElement)ve;

                item.gapX = gapXAttr.GetValueFromBag(bag, cc);
                item.gapY = gapYAttr.GetValueFromBag(bag, cc);
            }
        }

#endif
    }
}
