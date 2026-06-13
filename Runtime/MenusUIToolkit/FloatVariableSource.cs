// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using Unity.Properties;

namespace Buck.UIElements
{
    /// <summary>
    /// Exposes a FloatVariable to UI Toolkit runtime data binding. Bind a
    /// Slider's value TwoWay to nameof(Value).
    /// </summary>
    public sealed class FloatVariableSource : VariableSourceBase
    {
        readonly FloatVariable m_variable;

        public FloatVariableSource(FloatVariable variable, bool raiseGameEventOnChange = true)
            : base(variable, raiseGameEventOnChange)
        {
            m_variable = variable;
        }

        [CreateProperty]
        public float Value
        {
            get => m_variable != null ? m_variable.Value : 0f;
            set
            {
                if (m_variable == null || UnityEngine.Mathf.Approximately(m_variable.Value, value)) return;
                m_suppressEcho = true;
                try
                {
                    m_variable.Value = value;
                    if (m_raiseGameEventOnChange) m_variable.Raise();
                }
                finally { m_suppressEcho = false; }
                NotifyValueChanged();
                NotifyUiWrite();
            }
        }
    }
}
#endif
