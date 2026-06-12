// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Buck.UIElements
{
    /// <summary>
    /// Routes a cancel input action to an ordered collection of MenuStacks,
    /// topmost priority first. The first non-empty stack handles the press
    /// (its top screen may consume it, otherwise the stack navigates back).
    /// This replaces the per-controller cancel handling of the uGUI
    /// MenuController for UI Toolkit menus.
    /// </summary>
    [AddComponentMenu("BUCK/UI Toolkit/Menu Cancel Router")]
    public class MenuCancelRouter : MonoBehaviour
    {
        [SerializeField, Tooltip("The UI cancel action (typically the UI action map's Cancel).")]
        InputActionReference m_cancelAction;

        readonly List<MenuStack> m_stacks = new List<MenuStack>();

        /// <summary>
        /// Register the stacks this router serves, ordered topmost priority
        /// first. Replaces any previously registered stacks.
        /// </summary>
        public void SetStacks(IEnumerable<MenuStack> stacksTopmostFirst)
        {
            m_stacks.Clear();
            if (stacksTopmostFirst != null) m_stacks.AddRange(stacksTopmostFirst);
        }

        void OnEnable()
        {
            if (m_cancelAction == null || m_cancelAction.action == null) return;
            m_cancelAction.action.performed += OnCancelPerformed;
            m_cancelAction.action.Enable();
        }

        void OnDisable()
        {
            if (m_cancelAction == null || m_cancelAction.action == null) return;
            m_cancelAction.action.performed -= OnCancelPerformed;
        }

        void OnCancelPerformed(InputAction.CallbackContext context)
        {
            for (int i = 0; i < m_stacks.Count; i++)
                if (m_stacks[i] != null && m_stacks[i].HandleCancel())
                    return;
        }
    }
}
#endif
