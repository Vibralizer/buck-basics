// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

namespace Buck.UIElements
{
    /// <summary>
    /// Drives pointer-vs-navigation styling for a UI Toolkit panel, the
    /// UI Toolkit counterpart to the uGUI MenuController's UiInputMode plus
    /// SelectableColorsProfile. A BoolVariable (true while the active device
    /// is pointer-like) toggles the "mode-pointer" and "mode-navigation"
    /// classes on the panel root; all state styling hangs off those classes
    /// in USS. See the module README for the class-name contract.
    /// </summary>
    [AddComponentMenu("BUCK/UI Toolkit/UI Input Mode Class Driver")]
    public class UIInputModeClassDriver : MonoBehaviour
    {
        public const string k_pointerClass = "mode-pointer";
        public const string k_navigationClass = "mode-navigation";

        [SerializeField, Tooltip("The UIDocument whose root receives the mode classes.")]
        UIDocument m_document;

        [SerializeField, Tooltip("True while the active input device is pointer-like (mouse, pen, touch). False for gamepad or keyboard navigation.")]
        BoolVariable m_pointerModeVariable;

        GameEventListenerReference m_listenerReference;

        void OnEnable()
        {
            if (m_document == null) m_document = GetComponent<UIDocument>();

            if (m_pointerModeVariable != null)
            {
                m_listenerReference = new GameEventListenerReference
                {
                    Event = m_pointerModeVariable,
                    EventListener = this,
                    OnEventRaisedDelegate = Apply
                };
                m_pointerModeVariable.RegisterListener(m_listenerReference);
            }
            Apply();
        }

        void OnDisable()
        {
            if (m_pointerModeVariable != null && m_listenerReference != null)
                m_pointerModeVariable.UnregisterListener(m_listenerReference);
            m_listenerReference = null;
        }

        void Apply()
        {
            var root = m_document != null ? m_document.rootVisualElement : null;
            if (root == null) return;

            bool pointer = m_pointerModeVariable == null || m_pointerModeVariable.Value;
            root.EnableInClassList(k_pointerClass, pointer);
            root.EnableInClassList(k_navigationClass, !pointer);
        }
    }
}
#endif
