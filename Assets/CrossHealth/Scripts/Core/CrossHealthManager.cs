// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// Main entry point for the CrossHealth plugin.
    /// Thread-safe singleton MonoBehaviour that provides a unified API
    /// for reading health data from iOS HealthKit and Android Health Connect.
    ///
    /// Usage:
    ///   1. Add the CrossHealthManager prefab to your scene (or call CrossHealthManager.Instance)
    ///   2. Request permissions: CrossHealthManager.Instance.RequestPermissions(...)
    ///   3. Query data: CrossHealthManager.Instance.GetStepCount(...)
    /// </summary>
    public class CrossHealthManager : MonoBehaviour
    {
        #region Singleton

        private static CrossHealthManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Thread-safe singleton instance. Creates a new GameObject if none exists.
        /// </summary>
        public static CrossHealthManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[CrossHealth] Instance requested after application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<CrossHealthManager>();

                        if (_instance == null)
                        {
                            var go = new GameObject("CrossHealthManager");
                            _instance = go.AddComponent<CrossHealthManager>();
                        }
                    }
                    return _instance;
                }
            }
        }

        #endregion

        #region Services

        private HealthDataService _dataService;
        private HealthPermissionManager _permissionManager;

        /// <summary>Access the underlying data service for advanced queries.</summary>
        public HealthDataService DataService => _dataService;

        /// <summary>Access the permission manager for advanced permission handling.</summary>
        public HealthPermissionManager PermissionManager => _permissionManager;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[CrossHealth] Duplicate CrossHealthManager detected. Destroying this instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _dataService = new HealthDataService(gameObject.name);
            _permissionManager = new HealthPermissionManager(gameObject.name);

            Debug.Log("[CrossHealth] Manager initialized.");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;

#if UNITY_ANDROID && !UNITY_EDITOR
                Platform.AndroidHealthBridge.Dispose();
#endif
            }
        }

        #endregion

        #region Public API - Availability

        /// <summary>
        /// Returns true if health data services are available on the current platform.
        /// iOS: HealthKit available on iOS 8+
        /// Android: Health Connect available (built-in on Android 14+, app on 8-13)
        /// Editor: Always false
        /// </summary>
        public bool IsAvailable()
        {
            return _dataService.IsAvailable();
        }

        #endregion

        #region Public API - Permissions

        /// <summary>
        /// Requests read permissions for the specified health data types.
        /// </summary>
        /// <param name="types">Health data types to request access for.</param>
        /// <param name="callback">Called with true if permissions are granted.</param>
        public void RequestPermissions(HealthDataType[] types, Action<bool> callback)
        {
            _permissionManager.RequestPermissions(types, callback);
        }

        /// <summary>
        /// Requests read permissions for all supported health data types.
        /// </summary>
        public void RequestAllPermissions(Action<bool> callback)
        {
            _permissionManager.RequestAllPermissions(callback);
        }

        #endregion

        #region Public API - Activity Data

        /// <summary>
        /// Gets the total step count for the specified time range.
        /// </summary>
        public void GetStepCount(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.StepCount, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets the total walking/running distance in meters for the specified time range.
        /// </summary>
        public void GetDistance(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.DistanceWalking, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets the total active energy burned in kilocalories for the specified time range.
        /// </summary>
        public void GetActiveEnergy(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.ActiveEnergy, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets the total floors climbed for the specified time range.
        /// </summary>
        public void GetFloorsClimbed(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.FloorsClimbed, startTime, endTime, callback);
        }

        #endregion

        #region Public API - Vital Signs

        /// <summary>
        /// Gets heart rate data (in bpm) for the specified time range.
        /// Returns the average heart rate as the aggregated value.
        /// </summary>
        public void GetHeartRate(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.HeartRate, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets resting heart rate data (in bpm) for the specified time range.
        /// </summary>
        public void GetRestingHeartRate(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.RestingHeartRate, startTime, endTime, callback);
        }

        #endregion

        #region Public API - Body Metrics

        /// <summary>
        /// Gets body mass (in kg) - returns the most recent measurement.
        /// </summary>
        public void GetBodyMass(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.BodyMass, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets height (in meters) - returns the most recent measurement.
        /// </summary>
        public void GetHeight(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.Height, startTime, endTime, callback);
        }

        /// <summary>
        /// Gets BMI (kg/m²) - returns the most recent measurement.
        /// </summary>
        public void GetBMI(DateTime startTime, DateTime endTime, Action<double> callback)
        {
            QuerySimple(HealthDataType.BMI, startTime, endTime, callback);
        }

        #endregion

        #region Public API - Advanced

        /// <summary>
        /// Queries health data with full result details including individual data points.
        /// </summary>
        public void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, Action<HealthQueryResult> callback)
        {
            _dataService.QueryData(type, startTime, endTime, callback);
        }

        #endregion

        #region Native Callbacks (UnitySendMessage Receivers)

        // These methods are called by native code via UnitySendMessage.
        // Method names must match exactly what native code sends.

        /// <summary>
        /// Receives health data query results from native code.
        /// Called via UnitySendMessage from iOS/Android native bridges.
        /// </summary>
        public void OnHealthDataCallback(string jsonPayload)
        {
            _dataService.HandleNativeCallback(jsonPayload);
        }

        /// <summary>
        /// Receives permission request results from native code.
        /// Called via UnitySendMessage from iOS/Android native bridges.
        /// </summary>
        public void OnPermissionCallback(string jsonPayload)
        {
            _permissionManager.HandlePermissionResult(jsonPayload);
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Convenience wrapper that queries data and returns only the aggregated value.
        /// Returns 0 if the query fails or no data is available.
        /// </summary>
        private void QuerySimple(HealthDataType type, DateTime startTime, DateTime endTime, Action<double> callback)
        {
            if (callback == null) return;

            _dataService.QueryData(type, startTime, endTime, (result) =>
            {
                if (result.Success)
                {
                    callback(result.AggregatedValue);
                }
                else
                {
                    Debug.LogWarning($"[CrossHealth] Query failed for {type}: {result.ErrorMessage}");
                    callback(0);
                }
            });
        }

        #endregion
    }
}
