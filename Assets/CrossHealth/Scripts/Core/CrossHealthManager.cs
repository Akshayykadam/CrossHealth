// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// Main entry point for the CrossHealth plugin (V2).
    /// Thread-safe singleton MonoBehaviour that provides a unified API
    /// for reading health data from iOS HealthKit and Android Health Connect.
    ///
    /// V2 features:
    /// - Mock data in Editor (no device needed)
    /// - Real-time observers
    /// - Historical data with time bucketing
    /// - Events system
    /// - 15 health data types
    ///
    /// Usage:
    ///   CrossHealthManager.Instance.RequestPermissions(...)
    ///   CrossHealthManager.Instance.GetStepCount(...)
    ///   CrossHealthManager.Instance.StartObserving(HealthDataType.HeartRate, (v) => { })
    /// </summary>
    public class CrossHealthManager : MonoBehaviour
    {
        #region Singleton

        private static CrossHealthManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

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
        private HealthObserver _observer;
        private HealthHistoryService _historyService;
        private MockHealthDataProvider _mockProvider;

        /// <summary>Access the underlying data service.</summary>
        public HealthDataService DataService => _dataService;
        /// <summary>Access the permission manager.</summary>
        public HealthPermissionManager PermissionManager => _permissionManager;
        /// <summary>Access the real-time observer.</summary>
        public HealthObserver Observer => _observer;
        /// <summary>Access the history service.</summary>
        public HealthHistoryService HistoryService => _historyService;

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
            _historyService = new HealthHistoryService(gameObject.name);
            _observer = gameObject.AddComponent<HealthObserver>();

            // Initialize mock provider if in editor
            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                _mockProvider = new MockHealthDataProvider(CrossHealthSettings.Instance.MockDataSeed);
                Debug.Log("[CrossHealth] Manager initialized (Editor mode with mock data).");
            }
            else
            {
                Debug.Log("[CrossHealth] Manager initialized.");
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                if (_observer != null)
                    _observer.StopAllObservers();

                HealthEvents.ClearAll();
                _applicationIsQuitting = true;

#if UNITY_ANDROID && !UNITY_EDITOR
                Platform.AndroidHealthBridge.Dispose();
#endif
            }
        }

        #endregion

        #region Public API - Availability

        /// <summary>
        /// Returns true if health data services are available.
        /// In Editor with mock data enabled: returns true.
        /// </summary>
        public bool IsAvailable()
        {
            if (CrossHealthSettings.Instance.ShouldUseMockData)
                return true;
            return _dataService.IsAvailable();
        }

        #endregion

        #region Public API - Permissions

        /// <summary>
        /// Requests read permissions for the specified health data types.
        /// </summary>
        public void RequestPermissions(HealthDataType[] types, Action<bool> callback)
        {
            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                _mockProvider.SimulatePermissionRequest((granted) =>
                {
                    HealthEvents.RaiseAllPermissionsResolved(granted);
                    callback?.Invoke(granted);
                });
                return;
            }
            _permissionManager.RequestPermissions(types, (granted) =>
            {
                HealthEvents.RaiseAllPermissionsResolved(granted);
                callback?.Invoke(granted);
            });
        }

        /// <summary>
        /// Requests read permissions for all supported health data types.
        /// </summary>
        public void RequestAllPermissions(Action<bool> callback)
        {
            var allTypes = (HealthDataType[])Enum.GetValues(typeof(HealthDataType));
            RequestPermissions(allTypes, callback);
        }

        #endregion

        #region Public API - Activity Data

        public void GetStepCount(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.StepCount, startTime, endTime, callback);

        public void GetDistance(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.DistanceWalking, startTime, endTime, callback);

        public void GetActiveEnergy(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.ActiveEnergy, startTime, endTime, callback);

        public void GetFloorsClimbed(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.FloorsClimbed, startTime, endTime, callback);

        #endregion

        #region Public API - Vital Signs

        public void GetHeartRate(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.HeartRate, startTime, endTime, callback);

        public void GetRestingHeartRate(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.RestingHeartRate, startTime, endTime, callback);

        public void GetBloodOxygen(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.BloodOxygen, startTime, endTime, callback);

        public void GetBloodPressure(DateTime startTime, DateTime endTime, Action<double, double> callback)
        {
            double systolic = 0, diastolic = 0;
            int remaining = 2;

            QuerySimple(HealthDataType.BloodPressureSystolic, startTime, endTime, (v) =>
            {
                systolic = v;
                if (--remaining <= 0) callback?.Invoke(systolic, diastolic);
            });
            QuerySimple(HealthDataType.BloodPressureDiastolic, startTime, endTime, (v) =>
            {
                diastolic = v;
                if (--remaining <= 0) callback?.Invoke(systolic, diastolic);
            });
        }

        public void GetRespiratoryRate(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.RespiratoryRate, startTime, endTime, callback);

        #endregion

        #region Public API - Body Metrics

        public void GetBodyMass(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.BodyMass, startTime, endTime, callback);

        public void GetHeight(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.Height, startTime, endTime, callback);

        public void GetBMI(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.BMI, startTime, endTime, callback);

        #endregion

        #region Public API - Sleep & Workout (V2)

        public void GetSleepAnalysis(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.SleepAnalysis, startTime, endTime, callback);

        public void GetWorkoutDuration(DateTime startTime, DateTime endTime, Action<double> callback)
            => QuerySimple(HealthDataType.WorkoutSession, startTime, endTime, callback);

        #endregion

        #region Public API - Advanced Query

        /// <summary>
        /// Queries health data with full result details.
        /// </summary>
        public void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, Action<HealthQueryResult> callback)
        {
            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                var result = _mockProvider.GenerateData(type, startTime, endTime);
                HealthEvents.RaiseDataReceived(result);
                callback?.Invoke(result);
                return;
            }
            _dataService.QueryData(type, startTime, endTime, (result) =>
            {
                HealthEvents.RaiseDataReceived(result);
                callback?.Invoke(result);
            });
        }

        #endregion

        #region Public API - Observers (V2)

        /// <summary>
        /// Starts real-time observation of a health data type.
        /// Callback fires at each update interval with the latest value.
        /// </summary>
        public void StartObserving(HealthDataType type, Action<double> callback, float intervalSeconds = 0f)
        {
            _observer.StartObserving(type, callback, intervalSeconds);
        }

        /// <summary>
        /// Stops observing a specific health data type.
        /// </summary>
        public void StopObserving(HealthDataType type)
        {
            _observer.StopObserving(type);
        }

        /// <summary>
        /// Stops all active observers.
        /// </summary>
        public void StopAllObservers()
        {
            _observer.StopAllObservers();
        }

        #endregion

        #region Public API - History (V2)

        /// <summary>
        /// Queries historical data broken into time buckets.
        /// </summary>
        public void GetHistory(HealthDataType type, DateTime startTime, DateTime endTime,
            HealthInterval interval, Action<List<HealthDataPoint>> callback)
        {
            _historyService.QueryHistory(type, startTime, endTime, interval, callback);
        }

        /// <summary>
        /// Gets daily step history for the specified range.
        /// </summary>
        public void GetStepHistory(DateTime startTime, DateTime endTime,
            HealthInterval interval, Action<List<HealthDataPoint>> callback)
        {
            GetHistory(HealthDataType.StepCount, startTime, endTime, interval, callback);
        }

        /// <summary>
        /// Gets daily heart rate history for the specified range.
        /// </summary>
        public void GetHeartRateHistory(DateTime startTime, DateTime endTime,
            HealthInterval interval, Action<List<HealthDataPoint>> callback)
        {
            GetHistory(HealthDataType.HeartRate, startTime, endTime, interval, callback);
        }

        #endregion

        #region Native Callbacks (UnitySendMessage Receivers)

        public void OnHealthDataCallback(string jsonPayload)
        {
            _dataService.HandleNativeCallback(jsonPayload);
        }

        public void OnPermissionCallback(string jsonPayload)
        {
            _permissionManager.HandlePermissionResult(jsonPayload);
        }

        #endregion

        #region Private Helpers

        private void QuerySimple(HealthDataType type, DateTime startTime, DateTime endTime, Action<double> callback)
        {
            if (callback == null) return;

            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                var result = _mockProvider.GenerateData(type, startTime, endTime);
                HealthEvents.RaiseDataReceived(result);
                callback(result.AggregatedValue);
                return;
            }

            _dataService.QueryData(type, startTime, endTime, (result) =>
            {
                HealthEvents.RaiseDataReceived(result);
                if (result.Success)
                {
                    callback(result.AggregatedValue);
                }
                else
                {
                    Debug.LogWarning($"[CrossHealth] Query failed for {type}: {result.ErrorMessage}");
                    HealthEvents.RaiseQueryError(type, result.ErrorMessage);
                    callback(0);
                }
            });
        }

        #endregion
    }
}
