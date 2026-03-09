// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Settings ScriptableObject
// Copyright (c) 2025. All rights reserved.

using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// CrossHealth plugin settings. Create via Assets → Create → CrossHealth → Settings.
    /// Controls mock data behavior, observer intervals, and logging.
    /// </summary>
    [CreateAssetMenu(fileName = "CrossHealthSettings", menuName = "CrossHealth/Settings", order = 1)]
    public class CrossHealthSettings : ScriptableObject
    {
        [Header("Mock Data (Editor Only)")]
        [Tooltip("Enable mock data in the Unity Editor for testing without a device")]
        public bool UseMockDataInEditor = true;

        [Tooltip("Seed for mock data randomization (0 = random each run)")]
        public int MockDataSeed = 0;

        [Tooltip("Simulate a delay (seconds) before returning mock data")]
        [Range(0f, 3f)]
        public float MockResponseDelay = 0.2f;

        [Header("Observer Settings")]
        [Tooltip("Default interval (seconds) between observer updates")]
        [Range(1f, 300f)]
        public float DefaultObserverInterval = 5f;

        [Tooltip("Use mock observer updates in Editor")]
        public bool MockObserversInEditor = true;

        [Header("Logging")]
        [Tooltip("Enable detailed debug logging")]
        public bool VerboseLogging = false;

        [Tooltip("Log all native bridge calls")]
        public bool LogNativeCalls = false;

        // ====================================================================
        // Singleton Access
        // ====================================================================

        private static CrossHealthSettings _instance;

        /// <summary>
        /// Gets the settings instance. Loads from Resources or creates defaults.
        /// </summary>
        public static CrossHealthSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CrossHealthSettings>("CrossHealthSettings");

                    if (_instance == null)
                    {
                        // Create default settings at runtime
                        _instance = CreateInstance<CrossHealthSettings>();
                        Debug.Log("[CrossHealth] Using default settings. Create a CrossHealthSettings asset via Assets → Create → CrossHealth → Settings.");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Whether mock data should be used based on current platform and settings.
        /// </summary>
        public bool ShouldUseMockData
        {
            get
            {
#if UNITY_EDITOR
                return UseMockDataInEditor;
#else
                return false;
#endif
            }
        }
    }
}
