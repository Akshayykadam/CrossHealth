// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using UnityEngine;
using CrossHealth.Platform;

namespace CrossHealth
{
    /// <summary>
    /// Manages health data permission requests and status checks.
    /// Handles platform-specific permission flows for iOS HealthKit and Android Health Connect.
    /// </summary>
    public class HealthPermissionManager
    {
        private readonly string _callbackObjectName;
        private Action<bool> _permissionCallback;

        public HealthPermissionManager(string callbackObjectName)
        {
            _callbackObjectName = callbackObjectName;
        }

        /// <summary>
        /// Requests read permissions for the specified health data types.
        /// On iOS: Shows the HealthKit permission dialog.
        /// On Android: Launches the Health Connect permission activity.
        /// </summary>
        /// <param name="types">Array of data types to request permission for.</param>
        /// <param name="callback">Callback with true if permissions were granted, false otherwise.</param>
        public void RequestPermissions(HealthDataType[] types, Action<bool> callback)
        {
            if (types == null || types.Length == 0)
            {
                Debug.LogWarning("[CrossHealth] No data types specified for permission request.");
                callback?.Invoke(false);
                return;
            }

            _permissionCallback = callback;

#if UNITY_IOS && !UNITY_EDITOR
            IOSHealthBridge.RequestPermissions(types, _callbackObjectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidHealthBridge.RequestPermissions(types, _callbackObjectName);
#else
            Debug.LogWarning("[CrossHealth] Permissions can only be requested on iOS and Android devices.");
            callback?.Invoke(false);
#endif
        }

        /// <summary>
        /// Requests permissions for all supported health data types.
        /// </summary>
        public void RequestAllPermissions(Action<bool> callback)
        {
            var allTypes = (HealthDataType[])Enum.GetValues(typeof(HealthDataType));
            RequestPermissions(allTypes, callback);
        }

        /// <summary>
        /// Handles the native permission callback.
        /// Called by CrossHealthManager when receiving UnitySendMessage.
        /// </summary>
        internal void HandlePermissionResult(string jsonPayload)
        {
            try
            {
                var result = JsonUtility.FromJson<PermissionResult>(jsonPayload);
                bool granted = result != null && result.granted;

                if (granted)
                    Debug.Log("[CrossHealth] Health permissions granted.");
                else
                    Debug.LogWarning("[CrossHealth] Health permissions denied or partially denied.");

                _permissionCallback?.Invoke(granted);
                _permissionCallback = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CrossHealth] Error parsing permission result: {e.Message}");
                _permissionCallback?.Invoke(false);
                _permissionCallback = null;
            }
        }

        [Serializable]
        private class PermissionResult
        {
            public bool granted;
            public string error;
        }
    }
}
