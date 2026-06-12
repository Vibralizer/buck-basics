// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

namespace Buck.UIElements
{
    /// <summary>
    /// A UI Toolkit menu screen that can be managed by a MenuStack. This is
    /// the UI Toolkit counterpart to the uGUI MenuScreen component: a screen
    /// is shown or hidden by the stack, receives initial focus when shown,
    /// and may consume the cancel action before the stack pops it.
    /// </summary>
    public interface IMenuScreen
    {
        /// <summary>The root element of this screen. The stack toggles its display.</summary>
        VisualElement Root { get; }

        /// <summary>Show the screen. When focusFirst is true, move focus to GetInitialFocus().</summary>
        void Show(bool focusFirst = true);

        /// <summary>Hide the screen.</summary>
        void Hide();

        /// <summary>
        /// Called by the stack when the cancel action fires while this screen
        /// is on top. Return true to consume the cancel; return false to let
        /// the stack navigate back one screen.
        /// </summary>
        bool OnCancelPressed();

        /// <summary>
        /// The element that should receive focus when the screen is shown.
        /// Return null to leave focus unchanged.
        /// </summary>
        Focusable GetInitialFocus();
    }
}
#endif
