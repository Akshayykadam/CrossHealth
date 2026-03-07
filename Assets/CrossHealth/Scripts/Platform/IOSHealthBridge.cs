// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CrossHealth.Platform
{
    /// <summary>
    /// iOS-specific bridge to Apple HealthKit via native Objective-C++ plugin.
    /// Uses [DllImport("__Internal")] for statically linked iOS plugins.
    /// </summary>
    internal static class IOSHealthBridge
    {
#if UNITY_IOS && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern bool _CrossHealth_IsAvailable();

        [DllImport("__Internal")]
        private static extern void _CrossHealth_RequestPermissions(string dataTypesJson, string callbackObjectName);

        [DllImport("__Internal")]
        private static extern void _CrossHealth_QueryHealthData(
            int dataType,
            double startTime,
            double endTime,
            string requestId,
            string callbackObjectName
        );

        /// <summary>
        /// Checks if HealthKit is available on this device.
        /// </summary>
        public static bool IsAvailable()
        {
            return _CrossHealth_IsAvailable();
        }

        /// <summary>
        /// Requests read permissions for the specified health data types.
        /// </summary>
        public static void RequestPermissions(HealthDataType[] types, string callbackObjectName)
        {
            int[] typeInts = new int[types.Length];
            for (int i = 0; i < types.Length; i++)
                typeInts[i] = (int)types[i];

            string json = JsonUtility.ToJson(new IntArrayWrapper { values = typeInts });
            _CrossHealth_RequestPermissions(json, callbackObjectName);
        }

        /// <summary>
        /// Queries health data for a specific type and time range.
        /// </summary>
        public static void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, string requestId, string callbackObjectName)
        {
            _CrossHealth_QueryHealthData(
                (int)type,
                HealthDateUtils.ToUnixTimestamp(startTime),
                HealthDateUtils.ToUnixTimestamp(endTime),
                requestId,
                callbackObjectName
            );
        }

#else
        public static bool IsAvailable() { return false; }
        public static void RequestPermissions(HealthDataType[] types, string callbackObjectName) { }
        public static void QueryHealthData(HealthDataType type, DateTime startTime, DateTime endTime, string requestId, string callbackObjectName) { }
#endif

        [Serializable]
        private class IntArrayWrapper
        {
            public int[] values;
        }
    }
}
