// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

namespace Buck.UIElements
{
    /// <summary>
    /// Convenience base for IMenuScreen implementations that wrap a single
    /// root VisualElement. Show and hide are immediate display toggles
    /// (mirroring the uGUI MenuScreen's CanvasGroup flip); subclasses that
    /// animate can override Show/Hide and call the base when the
    /// animation lands.
    /// </summary>
    public class MenuScreenBase : IMenuScreen
    {
        protected readonly VisualElement m_root;

        public MenuScreenBase(VisualElement root)
        {
            m_root = root;
            m_root.style.display = DisplayStyle.None;
        }

        public VisualElement Root => m_root;

        public virtual void Show(bool focusFirst = true)
        {
            m_root.style.display = DisplayStyle.Flex;
            m_root.pickingMode = PickingMode.Position;

            if (focusFirst)
                GetInitialFocus()?.Focus();

            OnShown();
        }

        public virtual void Hide()
        {
            m_root.style.display = DisplayStyle.None;
            OnHidden();
        }

        /// <summary>Default: do not consume cancel; the stack navigates back.</summary>
        public virtual bool OnCancelPressed() => false;

        /// <summary>
        /// Default initial focus: the first focusable element carrying the
        /// "menu-item" class, falling back to the first focusable descendant.
        /// </summary>
        public virtual Focusable GetInitialFocus()
        {
            var item = m_root.Query<VisualElement>(className: "menu-item")
                             .Where(e => e.focusable && e.canGrabFocus).First();
            if (item != null) return item;

            return m_root.Query<VisualElement>()
                         .Where(e => e.focusable && e.canGrabFocus).First();
        }

        /// <summary>Hook for subclasses; called after the screen becomes visible.</summary>
        protected virtual void OnShown() { }

        /// <summary>Hook for subclasses; called after the screen is hidden.</summary>
        protected virtual void OnHidden() { }
    }
}
#endif
