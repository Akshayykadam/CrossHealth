// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Historical Data with Time Bucketing
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using CrossHealth.Platform;

namespace CrossHealth
{
    /// <summary>
    /// Provides historical health data queries with time bucketing.
    /// Returns arrays of data points broken into hourly, daily, weekly, or monthly intervals.
    ///
    /// Usage:
    ///   CrossHealthManager.Instance.GetStepHistory(
    ///       DateTime.Today.AddDays(-7), DateTime.Now,
    ///       HealthInterval.Daily,
    ///       (history) => {
    ///           foreach (var day in history)
    ///               Debug.Log($"{day.StartTime:d} = {day.Value} steps");
    ///       }
    ///   );
    /// </summary>
    public class HealthHistoryService
    {
        private readonly string _callbackObjectName;

        public HealthHistoryService(string callbackObjectName)
        {
            _callbackObjectName = callbackObjectName;
        }

        /// <summary>
        /// Queries historical health data broken into time buckets.
        /// </summary>
        /// <param name="type">Health data type to query.</param>
        /// <param name="startTime">Start of the historical range.</param>
        /// <param name="endTime">End of the historical range.</param>
        /// <param name="interval">Time bucket size (hourly, daily, weekly, monthly).</param>
        /// <param name="callback">Callback with the list of data points per bucket.</param>
        public void QueryHistory(HealthDataType type, DateTime startTime, DateTime endTime,
            HealthInterval interval, Action<List<HealthDataPoint>> callback)
        {
            if (callback == null) return;

#if UNITY_EDITOR
            if (CrossHealthSettings.Instance.ShouldUseMockData)
            {
                var mock = new MockHealthDataProvider(CrossHealthSettings.Instance.MockDataSeed);
                var points = mock.GenerateHistory(type, startTime, endTime, interval);
                callback(points);
                return;
            }
#endif

            // On device: query each bucket sequentially
            QueryBucketsOnDevice(type, startTime, endTime, interval, callback);
        }

        /// <summary>
        /// Queries historical data by making individual queries per time bucket.
        /// This works on both iOS and Android using the existing single-query API.
        /// </summary>
        private void QueryBucketsOnDevice(HealthDataType type, DateTime startTime,
            DateTime endTime, HealthInterval interval, Action<List<HealthDataPoint>> callback)
        {
            var buckets = GenerateTimeBuckets(startTime, endTime, interval);
            var results = new List<HealthDataPoint>();
            int remaining = buckets.Count;

            if (remaining == 0)
            {
                callback(results);
                return;
            }

            foreach (var bucket in buckets)
            {
                var bucketStart = bucket.Item1;
                var bucketEnd = bucket.Item2;

                CrossHealthManager.Instance.QueryHealthData(type, bucketStart, bucketEnd, (result) =>
                {
                    if (result.Success)
                    {
                        results.Add(new HealthDataPoint(type, result.AggregatedValue,
                            bucketStart, bucketEnd, "History"));
                    }
                    else
                    {
                        // Add zero for failed buckets
                        results.Add(new HealthDataPoint(type, 0, bucketStart, bucketEnd, "History"));
                    }

                    remaining--;
                    if (remaining <= 0)
                    {
                        // Sort by start time
                        results.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                        callback(results);
                    }
                });
            }
        }

        /// <summary>
        /// Generates time bucket boundaries for the given range and interval.
        /// </summary>
        private List<Tuple<DateTime, DateTime>> GenerateTimeBuckets(DateTime start,
            DateTime end, HealthInterval interval)
        {
            var buckets = new List<Tuple<DateTime, DateTime>>();
            DateTime current = start;

            while (current < end)
            {
                DateTime bucketEnd;
                switch (interval)
                {
                    case HealthInterval.Hourly:
                        bucketEnd = current.AddHours(1);
                        break;
                    case HealthInterval.Daily:
                        bucketEnd = current.AddDays(1);
                        break;
                    case HealthInterval.Weekly:
                        bucketEnd = current.AddDays(7);
                        break;
                    case HealthInterval.Monthly:
                        bucketEnd = current.AddMonths(1);
                        break;
                    default:
                        bucketEnd = current.AddDays(1);
                        break;
                }

                if (bucketEnd > end) bucketEnd = end;
                buckets.Add(new Tuple<DateTime, DateTime>(current, bucketEnd));
                current = bucketEnd;
            }

            return buckets;
        }
    }
}
