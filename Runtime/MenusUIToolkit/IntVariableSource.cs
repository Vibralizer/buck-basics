// MIT License - Copyright (c) 2026 BUCK Design LLC - https://github.com/buck-co

#if UNITY_6000_0_OR_NEWER
using Unity.Properties;

namespace Buck.UIElements
{
    /// <summary>
    /// Exposes an IntVariable to UI Toolkit runtime data binding.
    /// </summary>
    public sealed class IntVariableSource : VariableSourceBase
    {
        readonly IntVariable m_variable;

        public IntVariableSource(IntVariable variable, bool raiseGameEventOnChange = true)
            : base(variable, raiseGameEventOnChange)
        {
            m_variable = variable;
        }

        [CreateProperty]
        public int Value
        {
            get => m_variable != null ? m_variable.Value : 0;
            set
            {
                if (m_variable == null || m_variable.Value == value) return;
                m_suppressEcho = true;
                try
                {
                    m_variable.Value = value;
                    if (m_raiseGameEventOnChange) m_variable.Raise();
                }
                finally { m_suppressEcho = false; }
                NotifyValueChanged();
            }
        }
    }
}
#endif
