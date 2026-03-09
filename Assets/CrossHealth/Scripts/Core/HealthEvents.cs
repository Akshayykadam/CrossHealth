// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Events System
// Copyright (c) 2025. All rights reserved.

using System;

namespace CrossHealth
{
    /// <summary>
    /// Central event system for CrossHealth.
    /// Subscribe to these events to receive health data updates, permission changes,
    /// and observer notifications without polling.
    /// </summary>
    public static class HealthEvents
    {
        // ====================================================================
        // Permission Events
        // ====================================================================

        /// <summary>
        /// Fired when permission status changes for any data type.
        /// Parameters: (HealthDataType type, bool granted)
        /// </summary>
        public static event Action<HealthDataType, bool> OnPermissionChanged;

        /// <summary>
        /// Fired when all permissions have been resolved (granted or denied).
        /// Parameter: (bool allGranted)
        /// </summary>
        public static event Action<bool> OnAllPermissionsResolved;

        // ====================================================================
        // Data Events
        // ====================================================================

        /// <summary>
        /// Fired when any health data query completes.
        /// Parameter: (HealthQueryResult result)
        /// </summary>
        public static event Action<HealthQueryResult> OnDataReceived;

        /// <summary>
        /// Fired when a query fails with an error.
        /// Parameters: (HealthDataType type, string errorMessage)
        /// </summary>
        public static event Action<HealthDataType, string> OnQueryError;

        // ====================================================================
        // Observer Events
        // ====================================================================

        /// <summary>
        /// Fired when an observed health data type receives a new sample.
        /// Parameters: (HealthDataType type, double newValue)
        /// </summary>
        public static event Action<HealthDataType, double> OnObserverUpdate;

        /// <summary>
        /// Fired when an observer starts monitoring a data type.
        /// </summary>
        public static event Action<HealthDataType> OnObserverStarted;

        /// <summary>
        /// Fired when an observer stops monitoring a data type.
        /// </summary>
        public static event Action<HealthDataType> OnObserverStopped;

        // ====================================================================
        // Availability Events
        // ====================================================================

        /// <summary>
        /// Fired when health service availability changes (e.g., Health Connect installed/removed).
        /// </summary>
        public static event Action<bool> OnAvailabilityChanged;

        // ====================================================================
        // Internal Invokers (called by CrossHealth internals)
        // ====================================================================

        internal static void RaisePermissionChanged(HealthDataType type, bool granted)
        {
            OnPermissionChanged?.Invoke(type, granted);
        }

        internal static void RaiseAllPermissionsResolved(bool allGranted)
        {
            OnAllPermissionsResolved?.Invoke(allGranted);
        }

        internal static void RaiseDataReceived(HealthQueryResult result)
        {
            OnDataReceived?.Invoke(result);
        }

        internal static void RaiseQueryError(HealthDataType type, string error)
        {
            OnQueryError?.Invoke(type, error);
        }

        internal static void RaiseObserverUpdate(HealthDataType type, double value)
        {
            OnObserverUpdate?.Invoke(type, value);
        }

        internal static void RaiseObserverStarted(HealthDataType type)
        {
            OnObserverStarted?.Invoke(type);
        }

        internal static void RaiseObserverStopped(HealthDataType type)
        {
            OnObserverStopped?.Invoke(type);
        }

        internal static void RaiseAvailabilityChanged(bool available)
        {
            OnAvailabilityChanged?.Invoke(available);
        }

        /// <summary>
        /// Removes all event subscribers. Called during cleanup.
        /// </summary>
        internal static void ClearAll()
        {
            OnPermissionChanged = null;
            OnAllPermissionsResolved = null;
            OnDataReceived = null;
            OnQueryError = null;
            OnObserverUpdate = null;
            OnObserverStarted = null;
            OnObserverStopped = null;
            OnAvailabilityChanged = null;
        }
    }
}
