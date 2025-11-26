/* Original code[1] Copyright (c) 2022 Shane Celis[1]
   Licensed under the MIT License[1]

   [1]: https://gist.github.com/shanecelis/1ab175c46313da401138ccacceeb0c90
   [1]: https://twitter.com/shanecelis
   [1]: https://opensource.org/licenses/MIT
*/

using UnityEngine.UIElements;

#if !UNITY_6000_0_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace MyTools.UIElements
{
    /** This event represents a change in the children of a VisualElement. */
    public class ChildChangeEvent : EventBase<ChildChangeEvent>, IChangeEvent
    {
        public int previousValue { get; protected set; }

        public int newValue { get; protected set; }

        protected override void Init()
        {
            base.Init();
            this.LocalInit();
        }

        private void LocalInit()
        {
            this.bubbles = false;
            this.tricklesDown = false;
        }

        public static ChildChangeEvent GetPooled(int previousValue, int newValue)
        {
            ChildChangeEvent pooled = EventBase<ChildChangeEvent>.GetPooled();
            pooled.previousValue = previousValue;
            pooled.newValue = newValue;
            return pooled;
        }

        public ChildChangeEvent() => this.LocalInit();
    }

    /** Make `ChildChangeEvent` part of the events a VisualElement receives. There
        are two principle ways to trigger the ChildChangeEvent:

        a) Call CheckChildChange() which emits an event if the child count has
        changed. A manual poll.

        b) Set `checkInterval` to some milliseconds and `CheckChildChange()` will be
        called on that interval.

        Note: It would be nice to not have to do this with a poll. Internally
        `VisualElement` has a version and hierarchy change event, but we have no
        access to it that I know of. In the future, hopefully polling will not be
        required. If that happens, I'll try to add a note about it here and mark
        this class obsolete.
     */
    public class ChildChangeManipulator : IManipulator
    {
        private int lastChildCount;

        private IVisualElementScheduledItem task;
        private int _checkInterval;
        /** Set up a poll to check the child counts every so many milliseconds. */
        public int checkInterval
        {
            get => _checkInterval;
            set
            {
                task?.Pause();
                if (_checkInterval == value)
                    return;
                _checkInterval = value;
                if (_checkInterval > 0)
                    task = target?.schedule.Execute(CheckChildChange)
                               .Every(_checkInterval);
            }
        }

        private UnityEngine.UIElements.VisualElement _target;
        public UnityEngine.UIElements.VisualElement target
        {
            get => _target;
            set => _target = value;
        }

        /** Check whether the child count differs from the last time an event was
            sent. If the counts differ, send a `ChildChangeEvent`.

            This may have false negatives meaning that if one adds and removes from
            the elements in equal amounts, they won't be caught. In that case you might
            want to compute a hash of the children to check for differences.
         */
        public void CheckChildChange()
        {
            if (target?.childCount != lastChildCount)
                SendChildChange();
        }

        public void SendChildChange()
        {
            if (target == null)
                return;
            var e = ChildChangeEvent.GetPooled(lastChildCount, target.childCount);
            e.target = target;
            lastChildCount = target.childCount;
            target.SendEvent(e);
        }
    }

    /** Fake CSS pseudo classes :first-child and :last-child as USS regular classes
        .first-child and .last-child, i.e., the first child will have a .first-child
        USS class, and the last child will have a .last-child USS class. Knowing the
        first and last child helps with styling elements.

        If the children are fixed, no further setup ought to be required.

        If the children are changing, one must setup the `childChanger`; either
        calling `childChanger.CheckChildChange()` or `childChanger.checkInterval =
        1000` to check children every second for instance. Finally one can set that
        interval as a UXML attribute `check-interval`.

        Note: Hopefully Unity will add pseudo class support so one doesn't need to
        resort to this.

        Or hopefully Unity exposes some event for detecting the addition or
        removal of elements. They have one internally but nothing is exposed.
     */
#if UNITY_6000_0_OR_NEWER
    [UxmlElement(nameof(ChildAnnotator))]
#endif
    public partial class ChildAnnotator : UnityEngine.UIElements.VisualElement
    {
        private UnityEngine.UIElements.VisualElement _firstChild;
        public readonly ChildChangeManipulator childChanger;

        protected UnityEngine.UIElements.VisualElement firstChild
        {
            get => _firstChild;
            set
            {
                if (_firstChild != value)
                {
                    _firstChild?.RemoveFromClassList("first-child");
                    _firstChild = value;
                    _firstChild?.AddToClassList("first-child");
                }
            }
        }
        private UnityEngine.UIElements.VisualElement _lastChild;
        protected UnityEngine.UIElements.VisualElement lastChild
        {
            get => _lastChild;
            set
            {
                if (_lastChild != value)
                {
                    _lastChild?.RemoveFromClassList("last-child");
                    _lastChild = value;
                    _lastChild?.AddToClassList("last-child");
                }
            }
        }

        public ChildAnnotator()
        {
            this.AddManipulator(childChanger = new ChildChangeManipulator());
            RegisterCallback<ChildChangeEvent>(OnChildChange);
            schedule.Execute(() => childChanger.CheckChildChange());
        }

        protected virtual void OnChildChange(ChildChangeEvent evt)
        {
            if (childCount == 0)
            {
                firstChild = null;
                lastChild = null;
                return;
            }

            if (childCount > 0)
            {
                firstChild = this[0];
                lastChild = this[childCount - 1];
            }
        }
#if !UNITY_6000_0_OR_NEWER
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ChildAnnotator, UxmlTraits> { }

        [Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlIntAttributeDescription checkInterval = new UxmlIntAttributeDescription { name = "check-interval", defaultValue = 0 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var item = (ChildAnnotator)ve;

                item.childChanger.checkInterval = checkInterval.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}