// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using UnityEngine;

namespace CrossHealth.Platform
{
    /// <summary>
    /// Android-specific bridge to Health Connect via AndroidJavaObject.
    /// Communicates with CrossHealthConnectBridge.java in Plugins/Android.
    /// </summary>
    internal static class AndroidHealthBridge
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        private const string BridgeClassName = "com.crosshealth.bridge.CrossHealthConnectBridge";

        private static AndroidJavaObject _bridgeInstance;

        /// <summary>
        /// Gets or creates the singleton Java bridge instance.
        /// </summary>
        private static AndroidJavaObject GetBridge()
        {
            if (_bridgeInstance == null)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    _bridgeInstance = new AndroidJavaObject(BridgeClassName, activity);
                }
            }
            return _bridgeInstance;
        }

        /// <summary>
        /// Checks if Health Connect is available on this device.
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                using (var bridge = new AndroidJavaObject(BridgeClassName))
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    return bridge.CallStatic<bool>("isAvailable", activity);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[CrossHealth] Android IsAvailable check failed: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Requests read permissions for the specified health data types.
        /// </summary>
        public static void RequestPermissions(HealthDataType[] types, string callbackObjectName)
        {
            try
            {
                int[] typeInts = new int[types.Length];
                for (int i = 0; i < types.Length; i++)
                    typeInts[i] = (int)types[i];

                GetBridge().Call("requestPermissions", typeInts, callbackObjectName);
            }
            catch (Exception e)
            {
                Debug.LogError("[CrossHealth] Android RequestPermissions failed: " + e.Message);
            }
        }

        /// <summary>
        /// Queries health data for a specific type and time range.
        /// </summary>
        public static void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, string requestId, string callbackObjectName)
        {
            try
            {
                GetBridge().Call(
                    "queryHealthData",
                    (int)type,
                    HealthDateUtils.ToUnixTimestamp(startTime),
                    HealthDateUtils.ToUnixTimestamp(endTime),
                    requestId,
                    callbackObjectName
                );
            }
            catch (Exception e)
            {
                Debug.LogError("[CrossHealth] Android QueryHealthData failed: " + e.Message);
            }
        }

        /// <summary>
        /// Clean up the bridge instance.
        /// </summary>
        public static void Dispose()
        {
            if (_bridgeInstance != null)
            {
                _bridgeInstance.Dispose();
                _bridgeInstance = null;
            }
        }

#else
        public static bool IsAvailable() { return false; }
        public static void RequestPermissions(HealthDataType[] types, string callbackObjectName) { }
        public static void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, string requestId, string callbackObjectName) { }
        public static void Dispose() { }
#endif
    }
}
