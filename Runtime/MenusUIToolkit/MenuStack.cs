// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Buck.UIElements
{
    /// <summary>
    /// Manages a stack of IMenuScreen instances inside a UI Toolkit panel.
    /// This is the UI Toolkit counterpart to the uGUI MenuController: only the
    /// top screen is visible and interactive, screens below it remain in the
    /// stack, sibling screens can replace the top without disturbing the rest,
    /// and the cancel action navigates back one screen unless the top screen
    /// consumes it.
    /// </summary>
    public class MenuStack
    {
        readonly List<IMenuScreen> m_stack = new List<IMenuScreen>();
        readonly string m_name;

        /// <summary>Optional container element whose picking mode is toggled with stack emptiness.</summary>
        readonly VisualElement m_layerRoot;

        public MenuStack(string name = null, VisualElement layerRoot = null)
        {
            m_name = name;
            m_layerRoot = layerRoot;
            UpdateLayerPicking();
        }

        public string Name => m_name;
        public IMenuScreen Current => m_stack.Count > 0 ? m_stack[m_stack.Count - 1] : null;
        public int Count => m_stack.Count;

        /// <summary>
        /// When false, the cancel action will not close the last remaining
        /// screen (matches the uGUI MenuController's allow-closing-last-menu
        /// option). Defaults to true.
        /// </summary>
        public bool AllowClosingLastScreen { get; set; } = true;

        /// <summary>Raised after a screen is pushed and shown.</summary>
        public event Action<IMenuScreen> OnOpened;

        /// <summary>Raised after a screen is popped and hidden.</summary>
        public event Action<IMenuScreen> OnClosed;

        /// <summary>Raised after the top screen is replaced by a sibling.</summary>
        public event Action<IMenuScreen> OnSiblingSwapped;

        /// <summary>
        /// Raised when the stack transitions between empty and non-empty.
        /// The bool is true when the stack just became empty.
        /// </summary>
        public event Action<bool> OnStackEmptyChanged;

        /// <summary>Push a screen onto the stack and show it. The previous top is hidden but kept.</summary>
        public void Push(IMenuScreen screen, bool focusFirst = true)
        {
            if (screen == null || m_stack.Contains(screen)) return;

            bool wasEmpty = m_stack.Count == 0;
            Current?.Hide();
            m_stack.Add(screen);
            screen.Show(focusFirst);

            OnOpened?.Invoke(screen);
            if (wasEmpty) RaiseEmptyChanged(false);
        }

        /// <summary>Replace the top screen with a sibling. Screens below are untouched.</summary>
        public void SwapSibling(IMenuScreen screen, bool focusFirst = true)
        {
            if (screen == null) return;
            if (m_stack.Count == 0) { Push(screen, focusFirst); return; }
            if (Current == screen) return;

            var outgoing = Current;
            outgoing.Hide();
            m_stack[m_stack.Count - 1] = screen;
            screen.Show(focusFirst);

            OnSiblingSwapped?.Invoke(screen);
        }

        /// <summary>Pop the top screen. The screen below it (if any) is shown and refocused.</summary>
        public void Pop()
        {
            if (m_stack.Count == 0) return;

            var outgoing = Current;
            m_stack.RemoveAt(m_stack.Count - 1);
            outgoing.Hide();
            OnClosed?.Invoke(outgoing);

            if (m_stack.Count > 0)
                Current.Show(focusFirst: true);
            else
                RaiseEmptyChanged(true);
        }

        /// <summary>Pop every screen, bottom to top, hiding each.</summary>
        public void CloseAll()
        {
            if (m_stack.Count == 0) return;

            for (int i = m_stack.Count - 1; i >= 0; i--)
            {
                var screen = m_stack[i];
                m_stack.RemoveAt(i);
                screen.Hide();
                OnClosed?.Invoke(screen);
            }
            RaiseEmptyChanged(true);
        }

        /// <summary>
        /// Route a cancel press to this stack. Returns true if the press was
        /// handled (consumed by the top screen, used to navigate back, or
        /// intentionally ignored on a protected last screen); false if the
        /// stack is empty and the press should go elsewhere.
        /// </summary>
        public bool HandleCancel()
        {
            if (m_stack.Count == 0) return false;
            if (Current.OnCancelPressed()) return true;
            if (m_stack.Count == 1 && !AllowClosingLastScreen) return true;
            Pop();
            return true;
        }

        void RaiseEmptyChanged(bool isEmpty)
        {
            UpdateLayerPicking();
            OnStackEmptyChanged?.Invoke(isEmpty);
        }

        void UpdateLayerPicking()
        {
            if (m_layerRoot == null) return;
            // An empty stack's layer should never block pointer events.
            m_layerRoot.pickingMode = m_stack.Count > 0 ? PickingMode.Position : PickingMode.Ignore;
        }
    }
}
#endif
