// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using CrossHealth.Platform;

namespace CrossHealth
{
    /// <summary>
    /// Platform-abstracted service layer that delegates health data queries
    /// to the appropriate native bridge (iOS HealthKit or Android Health Connect).
    /// Handles request tracking and callback routing.
    /// </summary>
    public class HealthDataService
    {
        private readonly string _callbackObjectName;
        private readonly Dictionary<string, Action<HealthQueryResult>> _pendingRequests;
        private int _requestCounter;

        public HealthDataService(string callbackObjectName)
        {
            _callbackObjectName = callbackObjectName;
            _pendingRequests = new Dictionary<string, Action<HealthQueryResult>>();
            _requestCounter = 0;
        }

        /// <summary>
        /// Checks if health data services are available on the current platform.
        /// </summary>
        public bool IsAvailable()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return IOSHealthBridge.IsAvailable();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHealthBridge.IsAvailable();
#else
            Debug.LogWarning("[CrossHealth] Health services are only available on iOS and Android devices.");
            return false;
#endif
        }

        /// <summary>
        /// Queries health data for the specified type and time range.
        /// Results are returned asynchronously via the callback.
        /// </summary>
        /// <param name="type">The health data type to query.</param>
        /// <param name="startTime">Start of the time range.</param>
        /// <param name="endTime">End of the time range.</param>
        /// <param name="callback">Callback invoked with the query result.</param>
        public void QueryData(HealthDataType type, DateTime startTime, DateTime endTime, Action<HealthQueryResult> callback)
        {
            if (callback == null)
            {
                Debug.LogError("[CrossHealth] QueryData callback cannot be null.");
                return;
            }

            if (!IsAvailable())
            {
                callback(HealthQueryResult.CreateError(type, "Health services not available on this platform."));
                return;
            }

            string requestId = GenerateRequestId();
            _pendingRequests[requestId] = callback;

#if UNITY_IOS && !UNITY_EDITOR
            IOSHealthBridge.QueryHealthData(type, startTime, endTime, requestId, _callbackObjectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidHealthBridge.QueryHealthData(type, startTime, endTime, requestId, _callbackObjectName);
#else
            // Editor fallback: return empty result
            _pendingRequests.Remove(requestId);
            callback(HealthQueryResult.CreateError(type, "Health queries are not available in the Editor. Test on a device."));
#endif
        }

        /// <summary>
        /// Processes a native callback payload. Called by CrossHealthManager when
        /// receiving UnitySendMessage from native code.
        /// </summary>
        internal void HandleNativeCallback(string jsonPayload)
        {
            try
            {
                var payload = JsonUtility.FromJson<NativeCallbackPayload>(jsonPayload);

                if (payload == null)
                {
                    Debug.LogError("[CrossHealth] Failed to parse native callback payload.");
                    return;
                }

                if (!string.IsNullOrEmpty(payload.requestId) && _pendingRequests.ContainsKey(payload.requestId))
                {
                    var callback = _pendingRequests[payload.requestId];
                    _pendingRequests.Remove(payload.requestId);

                    var result = payload.ToQueryResult();
                    callback?.Invoke(result);
                }
                else
                {
                    Debug.LogWarning($"[CrossHealth] Received callback for unknown request ID: {payload.requestId}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CrossHealth] Error handling native callback: {e.Message}");
            }
        }

        /// <summary>
        /// Processes a native permission callback. Called by CrossHealthManager.
        /// </summary>
        internal void HandlePermissionCallback(string jsonPayload)
        {
            try
            {
                var payload = JsonUtility.FromJson<NativeCallbackPayload>(jsonPayload);

                if (payload == null)
                {
                    Debug.LogError("[CrossHealth] Failed to parse permission callback payload.");
                    return;
                }

                if (!string.IsNullOrEmpty(payload.requestId) && _pendingRequests.ContainsKey(payload.requestId))
                {
                    var callback = _pendingRequests[payload.requestId];
                    _pendingRequests.Remove(payload.requestId);

                    var result = payload.ToQueryResult();
                    callback?.Invoke(result);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CrossHealth] Error handling permission callback: {e.Message}");
            }
        }

        /// <summary>
        /// Registers a pending request with an external request ID.
        /// Used by HealthPermissionManager for permission callbacks.
        /// </summary>
        internal string RegisterRequest(Action<HealthQueryResult> callback)
        {
            string requestId = GenerateRequestId();
            _pendingRequests[requestId] = callback;
            return requestId;
        }

        /// <summary>
        /// Cleans up any pending requests that haven't received callbacks.
        /// </summary>
        public void ClearPendingRequests()
        {
            if (_pendingRequests.Count > 0)
            {
                Debug.LogWarning($"[CrossHealth] Clearing {_pendingRequests.Count} pending request(s).");
                _pendingRequests.Clear();
            }
        }

        private string GenerateRequestId()
        {
            _requestCounter++;
            return $"ch_req_{_requestCounter}_{DateTime.UtcNow.Ticks}";
        }
    }
}
