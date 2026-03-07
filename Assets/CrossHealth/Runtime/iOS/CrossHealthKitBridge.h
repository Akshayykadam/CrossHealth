// CrossHealth - Unity Plugin for HealthKit & Health Connect
// iOS Native Bridge - HealthKit Implementation
// Copyright (c) 2025. All rights reserved.

#ifndef CrossHealthKitBridge_h
#define CrossHealthKitBridge_h

#ifdef __cplusplus
extern "C" {
#endif

/// Check if HealthKit is available on this device
bool _CrossHealth_IsAvailable(void);

/// Request read permissions for the specified health data types
/// @param dataTypesJson JSON string containing an array of data type integers
/// @param callbackObjectName Name of the Unity GameObject to send callbacks to
void _CrossHealth_RequestPermissions(const char* dataTypesJson, const char* callbackObjectName);

/// Query health data for a specific type and time range
/// @param dataType Integer identifier for the health data type
/// @param startTime Unix timestamp (seconds) for start of range
/// @param endTime Unix timestamp (seconds) for end of range
/// @param requestId Unique identifier for this request
/// @param callbackObjectName Name of the Unity GameObject to send callbacks to
void _CrossHealth_QueryHealthData(int dataType, double startTime, double endTime,
                                   const char* requestId, const char* callbackObjectName);

#ifdef __cplusplus
}
#endif

#endif /* CrossHealthKitBridge_h */
