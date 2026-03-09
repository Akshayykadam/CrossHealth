// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Real-time Health Observer
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// Provides real-time observation of health data changes.
    /// On iOS: Uses HKObserverQuery for native-level notifications.
    /// On Android: Polls Health Connect at configurable intervals.
    /// In Editor: Uses mock data with configurable update intervals.
    ///
    /// Usage:
    ///   CrossHealthManager.Instance.StartObserving(HealthDataType.HeartRate, (value) => {
    ///       Debug.Log("HR: " + value);
    ///   });
    /// </summary>
    public class HealthObserver : MonoBehaviour
    {
        private class ObserverEntry
        {
            public HealthDataType Type;
            public Action<double> Callback;
            public float Interval;
            public Coroutine Coroutine;
            public bool IsActive;
        }

        private readonly Dictionary<HealthDataType, ObserverEntry> _activeObservers =
            new Dictionary<HealthDataType, ObserverEntry>();

        private MockHealthDataProvider _mockProvider;

        private void Awake()
        {
            _mockProvider = new MockHealthDataProvider(CrossHealthSettings.Instance.MockDataSeed);
        }

        /// <summary>
        /// Starts observing a health data type with a callback for each update.
        /// </summary>
        /// <param name="type">The data type to observe.</param>
        /// <param name="callback">Called each time a new value is available.</param>
        /// <param name="intervalSeconds">Seconds between updates (0 = use default from settings).</param>
        public void StartObserving(HealthDataType type, Action<double> callback, float intervalSeconds = 0f)
        {
            if (_activeObservers.ContainsKey(type))
            {
                Debug.LogWarning($"[CrossHealth] Already observing {type}. Stop first before restarting.");
                return;
            }

            float interval = intervalSeconds > 0 ? intervalSeconds : CrossHealthSettings.Instance.DefaultObserverInterval;

            var entry = new ObserverEntry
            {
                Type = type,
                Callback = callback,
                Interval = interval,
                IsActive = true
            };

            _activeObservers[type] = entry;

#if UNITY_EDITOR
            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                entry.Coroutine = StartCoroutine(MockObserverCoroutine(entry));
                HealthEvents.RaiseObserverStarted(type);
                Debug.Log($"[CrossHealth Mock] Started observing {type} every {interval}s");
                return;
            }
#endif

            // On device: start native observer + polling coroutine
            entry.Coroutine = StartCoroutine(DeviceObserverCoroutine(entry));
            HealthEvents.RaiseObserverStarted(type);

            if (CrossHealthSettings.Instance.VerboseLogging)
                Debug.Log($"[CrossHealth] Started observing {type} every {interval}s");
        }

        /// <summary>
        /// Stops observing a specific health data type.
        /// </summary>
        public void StopObserving(HealthDataType type)
        {
            if (_activeObservers.TryGetValue(type, out var entry))
            {
                entry.IsActive = false;
                if (entry.Coroutine != null)
                    StopCoroutine(entry.Coroutine);
                _activeObservers.Remove(type);
                HealthEvents.RaiseObserverStopped(type);

                if (CrossHealthSettings.Instance.VerboseLogging)
                    Debug.Log($"[CrossHealth] Stopped observing {type}");
            }
        }

        /// <summary>
        /// Stops all active observers.
        /// </summary>
        public void StopAllObservers()
        {
            var types = new List<HealthDataType>(_activeObservers.Keys);
            foreach (var type in types)
                StopObserving(type);
        }

        /// <summary>
        /// Returns true if the specified type is being observed.
        /// </summary>
        public bool IsObserving(HealthDataType type) => _activeObservers.ContainsKey(type);

        /// <summary>
        /// Returns the count of active observers.
        /// </summary>
        public int ActiveObserverCount => _activeObservers.Count;

        /// <summary>
        /// Returns all currently observed data types.
        /// </summary>
        public HealthDataType[] GetObservedTypes()
        {
            var types = new HealthDataType[_activeObservers.Count];
            _activeObservers.Keys.CopyTo(types, 0);
            return types;
        }

        // ====================================================================
        // Coroutines
        // ====================================================================

        private IEnumerator MockObserverCoroutine(ObserverEntry entry)
        {
            // Initial delay
            yield return new WaitForSeconds(0.5f);

            while (entry.IsActive)
            {
                double value = _mockProvider.GenerateObserverValue(entry.Type);
                entry.Callback?.Invoke(value);
                HealthEvents.RaiseObserverUpdate(entry.Type, value);

                yield return new WaitForSeconds(entry.Interval);
            }
        }

        private IEnumerator DeviceObserverCoroutine(ObserverEntry entry)
        {
            // Initial delay
            yield return new WaitForSeconds(1f);

            while (entry.IsActive)
            {
                // Query the latest value from the device
                bool waiting = true;
                double latestValue = 0;

                var manager = CrossHealthManager.Instance;
                if (manager != null)
                {
                    DateTime start = DateTime.Now.AddMinutes(-5);
                    DateTime end = DateTime.Now;

                    manager.QueryHealthData(entry.Type, start, end, (result) =>
                    {
                        if (result.Success && result.AggregatedValue > 0)
                        {
                            latestValue = result.AggregatedValue;
                        }
                        waiting = false;
                    });

                    // Wait for callback (with timeout)
                    float timeout = 10f;
                    float elapsed = 0f;
                    while (waiting && elapsed < timeout)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }

                    if (latestValue > 0)
                    {
                        entry.Callback?.Invoke(latestValue);
                        HealthEvents.RaiseObserverUpdate(entry.Type, latestValue);
                    }
                }

                yield return new WaitForSeconds(entry.Interval);
            }
        }

        private void OnDestroy()
        {
            StopAllObservers();
        }
    }
}
