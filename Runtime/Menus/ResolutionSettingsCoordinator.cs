// MIT License Copyright (c) 2025 BUCK Design LLC - https://github.com/buck-co

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buck
{
    [AddComponentMenu("BUCK/Display/Resolution Settings Coordinator")]
    public class ResolutionSettingsCoordinator : MonoBehaviour
    {
        [Header("Provider")]
        [SerializeField] ResolutionChoiceProvider m_provider;

        [Header("Toggles")]
        [SerializeField] Toggle m_autoResolutionToggle;
        [SerializeField] Toggle m_fullScreenToggle;

        [Header("Dropdown (optional)")]
        [SerializeField] TMP_Dropdown m_resolutionDropdown;
        [SerializeField] bool m_disableDropdownWhenAuto = true;

        // Optional authoritative sources (resolved from VariableBinding if left null)
        [SerializeField] BoolVariable m_autoVariable;
        [SerializeField] BoolVariable m_fullScreenVariable;

        bool AutoResolutionOn
            => m_autoVariable ? m_autoVariable.Value : (m_autoResolutionToggle && m_autoResolutionToggle.isOn);

        bool FullscreenOn
            => m_fullScreenVariable ? m_fullScreenVariable.Value : (m_fullScreenToggle && m_fullScreenToggle.isOn);
        
        void OnEnable()
        {
            if (!m_provider) m_provider = GetComponent<ResolutionChoiceProvider>();
            TryResolveVariablesFromBindings();

            if (m_autoResolutionToggle)
                m_autoResolutionToggle.onValueChanged.AddListener(OnAutoChanged);

            if (m_fullScreenToggle)
                m_fullScreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            // Ensure the provider has built its list before we drive it.
            m_provider.Initialize();

            SyncDropdownInteractable(AutoResolutionOn);

            // Order: set mode (doesn't change size), then optionally set size if Auto is on.
            m_provider.ApplyFullscreen(FullscreenOn);
            
            // If auto is on, apply it; otherwise reapply current selection or the closest selection.
            if (AutoResolutionOn)
                m_provider.ApplyAuto();
            else
                m_provider.ReapplyCurrentSelectionOrClosest();
        }

        void OnDisable()
        {
            if (m_autoResolutionToggle)
                m_autoResolutionToggle.onValueChanged.RemoveListener(OnAutoChanged);

            if (m_fullScreenToggle)
                m_fullScreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        }

        void OnAutoChanged(bool on)
        {
            SyncDropdownInteractable(on);
            if (on) m_provider.ApplyAuto();
        }

        void OnFullscreenChanged(bool full)
            => _ = ApplyFullscreenAndAuto(full);

        async Awaitable ApplyFullscreenAndAuto(bool full)
        {
            try
            {
                m_provider.ApplyFullscreen(full);

                // Wait for the next frame so that fullscreen can take effect. Then apply the resolution.
                await Awaitable.NextFrameAsync();
                
                if (AutoResolutionOn)
                    m_provider.ApplyAuto();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        void SyncDropdownInteractable(bool autoOn)
        {
            if (!m_resolutionDropdown || !m_disableDropdownWhenAuto) return;
            m_resolutionDropdown.interactable = !autoOn;
        }

        // Resolve BoolVariables from VariableBinding on the toggles if not explicitly wired.
        void TryResolveVariablesFromBindings()
        {
            if (!m_autoVariable && m_autoResolutionToggle)
            {
                var vb = m_autoResolutionToggle.GetComponent<VariableBinding>();
                if (vb && vb.Variable is BoolVariable bv) m_autoVariable = bv;
            }

            if (!m_fullScreenVariable && m_fullScreenToggle)
            {
                var vb = m_fullScreenToggle.GetComponent<VariableBinding>();
                if (vb && vb.Variable is BoolVariable bv) m_fullScreenVariable = bv;
            }
        }
    }
}
