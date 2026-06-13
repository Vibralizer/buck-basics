// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using System;
using UnityEngine.UIElements;

namespace Buck.UIElements
{
    /// <summary>
    /// Base for adapters that expose a BUCK Variable to the UI Toolkit
    /// runtime data binding system. This is the UI Toolkit counterpart to the
    /// uGUI VariableBinding + UIToggleHelper/UISliderHelper pattern: a UI
    /// control bound TwoWay to the adapter's Value reads and writes the same
    /// Variable asset, and a UI-initiated change optionally raises the
    /// Variable's GameEvent, exactly like the uGUI helpers do.
    ///
    /// Lifecycle: construct, call Attach() (typically in OnEnable), bind with
    /// SetBinding, then ClearBinding and Detach() in OnDisable.
    /// </summary>
    public abstract class VariableSourceBase : INotifyBindablePropertyChanged, IDisposable
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        /// <summary>
        /// Raised only when the Value setter ran because UI wrote to it
        /// (never for external Variable changes). Useful for save-on-change
        /// triggers that must ignore programmatic updates.
        /// </summary>
        public event Action ValueWrittenByUi;

        protected void NotifyUiWrite() => ValueWrittenByUi?.Invoke();

        protected readonly bool m_raiseGameEventOnChange;
        protected bool m_suppressEcho;

        GameEventListenerReference m_listenerReference;
        readonly GameEvent m_gameEvent;

        protected VariableSourceBase(GameEvent variableEvent, bool raiseGameEventOnChange)
        {
            m_gameEvent = variableEvent;
            m_raiseGameEventOnChange = raiseGameEventOnChange;
        }

        /// <summary>
        /// Start listening to the Variable's GameEvent so externally raised
        /// changes notify any bound UI.
        /// </summary>
        public void Attach()
        {
            if (m_gameEvent == null || m_listenerReference != null) return;

            m_listenerReference = new GameEventListenerReference
            {
                Event = m_gameEvent,
                OnEventRaisedDelegate = OnVariableRaised
            };
            m_gameEvent.RegisterListener(m_listenerReference);
        }

        /// <summary>Stop listening. Safe to call repeatedly.</summary>
        public void Detach()
        {
            if (m_gameEvent == null || m_listenerReference == null) return;
            m_gameEvent.UnregisterListener(m_listenerReference);
            m_listenerReference = null;
        }

        public void Dispose() => Detach();

        void OnVariableRaised()
        {
            // A raise that originated from our own setter is already notified
            // by the setter; suppress the echo so bindings update once.
            if (m_suppressEcho) return;
            NotifyValueChanged();
        }

        protected void NotifyValueChanged()
            => propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs("Value"));
    }
}
#endif
