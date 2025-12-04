// MIT License - Copyright (c) 2025 BUCK Design LLC - https://github.com/buck-co

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Buck
{
    /// <summary>
    /// ISingleChoiceProvider for desktop display resolutions.
    /// Responsibilities:
    /// - Expose stable IDs ("<width>x<height>") and labels for a presenter (dropdown or toggles).
    /// - Apply resolution size on selection.
    /// - Toggle fullscreen mode independently of size.
    /// - "Auto" sets size to native desktop resolution when fullscreen.
    /// - If m_autoResolutionSetsNativeInWindowedMode is true, "Auto" sets a sensible 16:9 size when windowed.
    /// </summary>
    [AddComponentMenu("BUCK/Display/Resolution Choice Provider")]
    public class ResolutionChoiceProvider : MonoBehaviour, ISingleChoiceProvider
    {
        [Tooltip("If this setting is on and the user has AutoResolution on, windowed mode will use the native resolution. " +
                 "If this setting is off and the user has AutoResolution on, windowed mode will choose a sensible 16:9 size, " +
                 "with a 1280x720 minimum.")]
        [SerializeField] bool m_autoResolutionSetsNativeInWindowedMode = true;
        
        [Tooltip("Show the aspect ratio in the label?")]
        [SerializeField] bool m_showAspectRatio = true;

        readonly List<string> m_ids = new();
        readonly Dictionary<string, Vector2Int> m_idToSize = new(StringComparer.Ordinal);
        string m_currentId;

        public event Action LabelsChanged;

        public void Initialize()
        {
            BuildList();

            // Seed selection from the current physical size; ensure it's present in the list.
            var current = new Vector2Int(Screen.width, Screen.height);
            m_currentId = FindOrAddId(current);
        }

        public IReadOnlyList<string> GetIds() => m_ids;

        public string GetCurrentId()
        {
            if (!string.IsNullOrEmpty(m_currentId) && m_ids.Contains(m_currentId))
                return m_currentId;
            return m_ids.Count > 0 ? m_ids[0] : string.Empty;
        }

        public void SelectById(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!m_idToSize.TryGetValue(id, out var size)) return;

            m_currentId = id;
            SetResolution(size);
            LabelsChanged?.Invoke(); // presenters (e.g., dropdown) reselect immediately
        }

        public string GetLabel(string id)
        {
            if (!m_idToSize.TryGetValue(id, out var sz))
                return id ?? string.Empty;

            if (!m_showAspectRatio)
                return $"{sz.x} × {sz.y}";

            var gcd = ExtensionMethods.GreatestCommonDivisor(sz.x, sz.y);
            return $"{sz.x} × {sz.y}  ({sz.x / gcd}:{sz.y / gcd})";
        }

        public void BindLabelTo(string id, TMP_Text target)
        {
            if (!target) return;
            target.SetText(GetLabel(id) ?? string.Empty);
        }

        /// <summary>
        /// If we're entering full screen OR if m_autoResolutionSetsNativeInWindowedMode is true,
        /// then set the native screen resolution.
        /// 
        /// Otherwise, if we're entering windowed mode and m_autoResolutionSetsNativeInWindowedMode is false,
        /// then choose a window size that is a 16:9 aspect ratio and 2/3rd the height of the native resolution.
        /// </summary>
        public void ApplyAuto()
        {
            EnsureBuilt();
            m_currentId = Screen.fullScreen || m_autoResolutionSetsNativeInWindowedMode ? GetNativeDisplayID() : GetAutoWindowedSizeID();
            SetResolution(m_idToSize[m_currentId]);
            LabelsChanged?.Invoke();
        }

        /// <summary>
        /// Sets fullscreen or windowed mode. Does not change resolution.
        /// </summary>
        public void ApplyFullscreen(bool fullscreen)
        {
            var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.fullScreenMode = mode;
            Screen.fullScreen = fullscreen;
        }

        /// <summary>
        /// Re-applies the current selection's size; useful if the caller wants to enforce the chosen size again.
        /// </summary>
        public void ReapplyCurrentSelectionOrClosest()
        {
            EnsureBuilt();
            
            if (string.IsNullOrEmpty(m_currentId) || !m_idToSize.TryGetValue(m_currentId, out var screenSize))
                return;
            
            m_currentId = FindOrAddId(screenSize);
            SetResolution(screenSize);
            LabelsChanged?.Invoke();
        }

        void EnsureBuilt()
        {
            if (m_ids.Count == 0)
                Initialize();
        }

        void BuildList()
        {
            m_ids.Clear();
            m_idToSize.Clear();

            IEnumerable<Vector2Int> sizes = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height));

            // Always include the current and native sizes so Auto and startup selection are present.
            sizes = sizes
                .Concat(new[] { new Vector2Int(Screen.width, Screen.height), GetNativeDisplaySize() })
                .Where(s => s.x > 0 && s.y > 0)
                .Distinct()
                .OrderByDescending(s => s.x * s.y)
                .ThenByDescending(s => s.x);

            foreach (var s in sizes)
            {
                var id = $"{s.x}x{s.y}";
                if (m_idToSize.ContainsKey(id)) continue;
                m_idToSize[id] = s;
                m_ids.Add(id);
            }
        }

        public static Vector2Int GetNativeDisplaySize()
            => new (Display.main.systemWidth, Display.main.systemHeight);
        
        /// <summary>
        /// Returns the ID for the native resolution.
        /// </summary>
        string GetNativeDisplayID()
            => FindOrAddId(GetNativeDisplaySize());

        /// <summary>
        /// Returns the ID for the auto windowed size.
        /// </summary>
        string GetAutoWindowedSizeID()
        {
            var chosenWindowedSize = Choose16By9WindowedSize();
            return FindOrAddId(chosenWindowedSize);
        }

        /// <summary>
        /// Returns the screen size for a resolution that is 16:9 and approximately 2/3rd the height of the native resolution with a minimum width of 1280x720.
        /// </summary>
        public static Vector2Int Choose16By9WindowedSize()
        {
            var nativeDisplaySize = GetNativeDisplaySize();
            var targetHeight = Mathf.CeilToInt(nativeDisplaySize.y * (2f / 3f));

            return Choose16By9WindowedSize(nativeDisplaySize, targetHeight);
        }
        
        public static Vector2Int Choose16By9WindowedSize(Vector2Int nativeDisplaySize, int targetHeight)
        {
            // Convert Screen.resolutions to a list of Vector2Int so we can filter and sort.
            var resolutionsList = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).ToList();
            
            var candidateResolutions = new List<Vector2Int>(resolutionsList)
                .Where(resolution => resolution.x > 0 && resolution.y > 0)
                .Where(IsExactly16By9)
                .Distinct()
                .OrderBy(resolution => resolution.y)
                .ThenBy(resolution => resolution.x)
                .ToList();

            if (candidateResolutions.Count == 0)
                return Fit16By9Inside(nativeDisplaySize, targetHeight);

            var fittingCandidates = candidateResolutions
                .Where(resolution => resolution.x <= nativeDisplaySize.x && resolution.y <= nativeDisplaySize.y)
                .ToList();

            if (fittingCandidates.Count == 0)
                return Fit16By9Inside(nativeDisplaySize, targetHeight);

            for (var index = fittingCandidates.Count - 1; index >= 0; index--)
            {
                var candidate = fittingCandidates[index];
                if (candidate.y <= targetHeight)
                    return candidate;
            }

            return fittingCandidates[0];
        }

        static bool IsExactly16By9(Vector2Int resolution)
            => resolution.y > 0 && resolution.x * 9 == resolution.y * 16;

        static Vector2Int Fit16By9Inside(Vector2Int nativeDisplaySize, int targetHeight)
        {
            const int aspectWidth = 16;
            const int aspectHeight = 9;

            var maximumScaleFromWidth = nativeDisplaySize.x / aspectWidth;
            var maximumScaleFromHeight = nativeDisplaySize.y / aspectHeight;
            var maximumScaleFromTargetHeight = targetHeight / aspectHeight;

            var maximumScale = Mathf.Min(maximumScaleFromWidth, maximumScaleFromHeight, maximumScaleFromTargetHeight);
            maximumScale = Mathf.Max(1, maximumScale);

            return new Vector2Int(aspectWidth * maximumScale, aspectHeight * maximumScale);
        }
        
        string FindOrAddId(Vector2Int size)
        {
            var id = $"{size.x}x{size.y}";
            if (!m_idToSize.ContainsKey(id))
            {
                // Insert at front so native/current appear at the top if they weren’t in the source list.
                m_idToSize[id] = size;
                m_ids.Insert(0, id);
            }
            return id;
        }

        void SetResolution(Vector2Int size)
        {
            var mode = Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(size.x, size.y, mode);
        }
    }
}
