// CrossHealth - Unity Plugin for HealthKit & Health Connect
// iOS Native Bridge - HealthKit Implementation
// Copyright (c) 2025. All rights reserved.

#import <Foundation/Foundation.h>
#import <HealthKit/HealthKit.h>
#import "CrossHealthKitBridge.h"

// ============================================================================
// MARK: - Helper: HealthKit Type Mapping
// ============================================================================

/// Maps CrossHealth data type integers to HealthKit quantity type identifiers.
static HKQuantityTypeIdentifier _GetQuantityTypeIdentifier(int dataType) {
    switch (dataType) {
        case 0: return HKQuantityTypeIdentifierStepCount;
        case 1: return HKQuantityTypeIdentifierDistanceWalkingRunning;
        case 2: return HKQuantityTypeIdentifierActiveEnergyBurned;
        case 3: return HKQuantityTypeIdentifierFlightsClimbed;
        case 4: return HKQuantityTypeIdentifierHeartRate;
        case 5: return HKQuantityTypeIdentifierRestingHeartRate;
        case 6: return HKQuantityTypeIdentifierBodyMass;
        case 7: return HKQuantityTypeIdentifierHeight;
        case 8: return HKQuantityTypeIdentifierBodyMassIndex;
        default: return nil;
    }
}

/// Returns the appropriate HKUnit for a given data type.
static HKUnit* _GetUnit(int dataType) {
    switch (dataType) {
        case 0: return [HKUnit countUnit];
        case 1: return [HKUnit meterUnit];
        case 2: return [HKUnit kilocalorieUnit];
        case 3: return [HKUnit countUnit];
        case 4: return [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]];
        case 5: return [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]];
        case 6: return [HKUnit gramUnitWithMetricPrefix:HKMetricPrefixKilo];
        case 7: return [HKUnit meterUnit];
        case 8: return [HKUnit countUnit];
        default: return [HKUnit countUnit];
    }
}

/// Returns YES if the data type uses cumulative sum aggregation.
static BOOL _IsCumulativeType(int dataType) {
    return (dataType >= 0 && dataType <= 3);
}

// ============================================================================
// MARK: - HKHealthStore Singleton
// ============================================================================

static HKHealthStore* _healthStore = nil;

static HKHealthStore* _GetHealthStore() {
    if (_healthStore == nil) {
        _healthStore = [[HKHealthStore alloc] init];
    }
    return _healthStore;
}

// ============================================================================
// MARK: - Unity Callback Helper
// ============================================================================

/// Sends a message to a Unity GameObject via UnitySendMessage.
extern void UnitySendMessage(const char* obj, const char* method, const char* msg);

static void _SendCallbackToUnity(const char* objectName, const char* methodName, NSString* jsonPayload) {
    const char* payload = [jsonPayload UTF8String];
    UnitySendMessage(objectName, methodName, payload);
}

/// Creates a JSON error response string.
static NSString* _CreateErrorResponse(NSString* requestId, int dataType, NSString* errorMessage) {
    NSDictionary* response = @{
        @"requestId": requestId ?: @"",
        @"success": @NO,
        @"error": errorMessage ?: @"Unknown error",
        @"dataType": @(dataType),
        @"dataPoints": @[]
    };
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:response options:0 error:nil];
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

/// Creates a JSON success response string with data points.
static NSString* _CreateSuccessResponse(NSString* requestId, int dataType, NSArray* dataPoints) {
    NSDictionary* response = @{
        @"requestId": requestId ?: @"",
        @"success": @YES,
        @"error": @"",
        @"dataType": @(dataType),
        @"dataPoints": dataPoints ?: @[]
    };
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:response options:0 error:nil];
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

// ============================================================================
// MARK: - Extern C Functions
// ============================================================================

#ifdef __cplusplus
extern "C" {
#endif

bool _CrossHealth_IsAvailable() {
    return [HKHealthStore isHealthDataAvailable];
}

void _CrossHealth_RequestPermissions(const char* dataTypesJson, const char* callbackObjectName) {
    if (![HKHealthStore isHealthDataAvailable]) {
        NSString* json = @"{\"granted\":false,\"error\":\"HealthKit not available\"}";
        _SendCallbackToUnity(callbackObjectName, "OnPermissionCallback", json);
        return;
    }

    // Parse data type array from JSON
    NSString* jsonStr = [NSString stringWithUTF8String:dataTypesJson];
    NSData* jsonData = [jsonStr dataUsingEncoding:NSUTF8StringEncoding];
    NSError* parseError = nil;
    NSDictionary* parsed = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&parseError];

    NSArray* typeValues = parsed[@"values"];
    if (!typeValues || typeValues.count == 0) {
        NSString* json = @"{\"granted\":false,\"error\":\"No data types specified\"}";
        _SendCallbackToUnity(callbackObjectName, "OnPermissionCallback", json);
        return;
    }

    // Build set of HKQuantityTypes to read
    NSMutableSet<HKObjectType*>* readTypes = [NSMutableSet set];
    for (NSNumber* typeNum in typeValues) {
        HKQuantityTypeIdentifier identifier = _GetQuantityTypeIdentifier([typeNum intValue]);
        if (identifier) {
            HKQuantityType* quantityType = [HKQuantityType quantityTypeForIdentifier:identifier];
            if (quantityType) {
                [readTypes addObject:quantityType];
            }
        }
    }

    if (readTypes.count == 0) {
        NSString* json = @"{\"granted\":false,\"error\":\"No valid HealthKit types found\"}";
        _SendCallbackToUnity(callbackObjectName, "OnPermissionCallback", json);
        return;
    }

    // Copy callback object name for use in async block
    NSString* callbackObjName = [NSString stringWithUTF8String:callbackObjectName];

    // Request authorization
    HKHealthStore* store = _GetHealthStore();
    [store requestAuthorizationToShareTypes:nil
                                 readTypes:readTypes
                                completion:^(BOOL success, NSError* _Nullable error) {
        dispatch_async(dispatch_get_main_queue(), ^{
            NSString* json;
            if (success) {
                json = @"{\"granted\":true,\"error\":\"\"}";
            } else {
                NSString* errorMsg = error ? error.localizedDescription : @"Permission denied";
                json = [NSString stringWithFormat:@"{\"granted\":false,\"error\":\"%@\"}", errorMsg];
            }
            _SendCallbackToUnity([callbackObjName UTF8String], "OnPermissionCallback", json);
        });
    }];
}

void _CrossHealth_QueryHealthData(int dataType, double startTime, double endTime,
                                   const char* requestId, const char* callbackObjectName) {
    if (![HKHealthStore isHealthDataAvailable]) {
        NSString* reqId = [NSString stringWithUTF8String:requestId];
        NSString* response = _CreateErrorResponse(reqId, dataType, @"HealthKit not available");
        _SendCallbackToUnity(callbackObjectName, "OnHealthDataCallback", response);
        return;
    }

    HKQuantityTypeIdentifier identifier = _GetQuantityTypeIdentifier(dataType);
    if (!identifier) {
        NSString* reqId = [NSString stringWithUTF8String:requestId];
        NSString* response = _CreateErrorResponse(reqId, dataType, @"Unknown data type");
        _SendCallbackToUnity(callbackObjectName, "OnHealthDataCallback", response);
        return;
    }

    HKQuantityType* quantityType = [HKQuantityType quantityTypeForIdentifier:identifier];
    HKUnit* unit = _GetUnit(dataType);
    NSDate* start = [NSDate dateWithTimeIntervalSince1970:startTime];
    NSDate* end = [NSDate dateWithTimeIntervalSince1970:endTime];
    NSPredicate* predicate = [HKQuery predicateForSamplesWithStartDate:start endDate:end options:HKQueryOptionStrictStartDate];

    // Copy strings for async block
    NSString* reqIdStr = [NSString stringWithUTF8String:requestId];
    NSString* callbackObjName = [NSString stringWithUTF8String:callbackObjectName];
    HKHealthStore* store = _GetHealthStore();

    if (_IsCumulativeType(dataType)) {
        // Use HKStatisticsQuery for cumulative types (steps, distance, energy, floors)
        // This correctly handles overlapping data from multiple sources
        HKStatisticsQuery* query = [[HKStatisticsQuery alloc]
            initWithQuantityType:quantityType
            quantitySamplePredicate:predicate
            options:HKStatisticsOptionCumulativeSum
            completionHandler:^(HKStatisticsQuery* _Nonnull q, HKStatistics* _Nullable stats, NSError* _Nullable error) {
                dispatch_async(dispatch_get_main_queue(), ^{
                    if (error) {
                        NSString* response = _CreateErrorResponse(reqIdStr, dataType, error.localizedDescription);
                        _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                        return;
                    }

                    double value = 0;
                    if (stats && stats.sumQuantity) {
                        value = [stats.sumQuantity doubleValueForUnit:unit];
                    }

                    NSDictionary* dataPoint = @{
                        @"startTime": @(startTime),
                        @"endTime": @(endTime),
                        @"value": @(value),
                        @"source": @"HealthKit"
                    };

                    NSString* response = _CreateSuccessResponse(reqIdStr, dataType, @[dataPoint]);
                    _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                });
            }];
        [store executeQuery:query];
    } else {
        // Use HKStatisticsQuery with discreteAverage for discrete types
        // (heart rate, resting HR, body mass, height, BMI)
        HKStatisticsOptions options;
        if (dataType == 4 || dataType == 5) {
            // Heart rate types: use discrete average
            options = HKStatisticsOptionDiscreteAverage;
        } else {
            // Body metrics: use most recent via sample query
            options = HKStatisticsOptionMostRecent;
        }

        // For body metrics (6, 7, 8), use HKSampleQuery to get most recent
        if (dataType >= 6 && dataType <= 8) {
            NSSortDescriptor* sortDescriptor = [[NSSortDescriptor alloc] initWithKey:HKSampleSortIdentifierEndDate ascending:NO];
            HKSampleQuery* sampleQuery = [[HKSampleQuery alloc]
                initWithSampleType:quantityType
                predicate:predicate
                limit:1
                sortDescriptors:@[sortDescriptor]
                resultsHandler:^(HKSampleQuery* _Nonnull q, NSArray<__kindof HKSample*>* _Nullable results, NSError* _Nullable error) {
                    dispatch_async(dispatch_get_main_queue(), ^{
                        if (error) {
                            NSString* response = _CreateErrorResponse(reqIdStr, dataType, error.localizedDescription);
                            _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                            return;
                        }

                        NSMutableArray* dataPoints = [NSMutableArray array];
                        if (results && results.count > 0) {
                            HKQuantitySample* sample = (HKQuantitySample*)results[0];
                            double value = [sample.quantity doubleValueForUnit:unit];
                            NSDictionary* dp = @{
                                @"startTime": @([sample.startDate timeIntervalSince1970]),
                                @"endTime": @([sample.endDate timeIntervalSince1970]),
                                @"value": @(value),
                                @"source": sample.sourceRevision.source.name ?: @"HealthKit"
                            };
                            [dataPoints addObject:dp];
                        }

                        NSString* response = _CreateSuccessResponse(reqIdStr, dataType, dataPoints);
                        _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                    });
                }];
            [store executeQuery:sampleQuery];
        } else {
            // Heart rate types: use statistics query with discrete average
            HKStatisticsQuery* query = [[HKStatisticsQuery alloc]
                initWithQuantityType:quantityType
                quantitySamplePredicate:predicate
                options:options
                completionHandler:^(HKStatisticsQuery* _Nonnull q, HKStatistics* _Nullable stats, NSError* _Nullable error) {
                    dispatch_async(dispatch_get_main_queue(), ^{
                        if (error) {
                            NSString* response = _CreateErrorResponse(reqIdStr, dataType, error.localizedDescription);
                            _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                            return;
                        }

                        double value = 0;
                        if (stats && stats.averageQuantity) {
                            value = [stats.averageQuantity doubleValueForUnit:unit];
                        }

                        NSDictionary* dataPoint = @{
                            @"startTime": @(startTime),
                            @"endTime": @(endTime),
                            @"value": @(value),
                            @"source": @"HealthKit"
                        };

                        NSString* response = _CreateSuccessResponse(reqIdStr, dataType, @[dataPoint]);
                        _SendCallbackToUnity([callbackObjName UTF8String], "OnHealthDataCallback", response);
                    });
                }];
            [store executeQuery:query];
        }
    }
}

#ifdef __cplusplus
}
#endif
