// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Mock Health Data Provider for Editor Testing
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// Provides realistic mock health data for Editor testing.
    /// When enabled, all health queries return simulated data instead of
    /// returning "not available" errors.
    ///
    /// Enable via: CrossHealthSettings asset or CrossHealthManager.UseMockData = true
    /// </summary>
    public class MockHealthDataProvider
    {
        private readonly System.Random _rng;
        private readonly Dictionary<HealthDataType, MockProfile> _profiles;

        public MockHealthDataProvider(int seed = 0)
        {
            _rng = seed > 0 ? new System.Random(seed) : new System.Random();
            _profiles = CreateDefaultProfiles();
        }

        /// <summary>
        /// Simulates a permission request. Always grants after a short delay.
        /// </summary>
        public void SimulatePermissionRequest(Action<bool> callback)
        {
            // Simulate a slight delay, then grant
            Debug.Log("[CrossHealth Mock] Simulating permission grant...");
            callback?.Invoke(true);
        }

        /// <summary>
        /// Returns true to simulate availability.
        /// </summary>
        public bool IsAvailable() => true;

        /// <summary>
        /// Generates mock health data for the given type and time range.
        /// </summary>
        public HealthQueryResult GenerateData(HealthDataType type, DateTime startTime, DateTime endTime)
        {
            if (!_profiles.TryGetValue(type, out var profile))
            {
                return HealthQueryResult.CreateError(type, $"No mock profile for {type}");
            }

            var aggregation = HealthDataTypeInfo.GetDefaultAggregation(type);
            double value;

            switch (aggregation)
            {
                case HealthAggregationType.CumulativeSum:
                    // Scale by time range
                    double hours = (endTime - startTime).TotalHours;
                    double dailyValue = RandomInRange(profile.MinDaily, profile.MaxDaily);
                    value = dailyValue * (hours / 24.0);
                    break;

                case HealthAggregationType.DiscreteAverage:
                    value = RandomInRange(profile.MinValue, profile.MaxValue);
                    break;

                case HealthAggregationType.MostRecent:
                    value = RandomInRange(profile.MinValue, profile.MaxValue);
                    break;

                default:
                    value = RandomInRange(profile.MinValue, profile.MaxValue);
                    break;
            }

            // Round to appropriate precision
            value = Math.Round(value, profile.DecimalPlaces);

            var point = new HealthDataPoint(type, value, startTime, endTime, "Mock Data");
            var points = new List<HealthDataPoint> { point };

            Debug.Log($"[CrossHealth Mock] {type} = {value} {HealthDataTypeInfo.GetUnit(type)}");

            return HealthQueryResult.CreateSuccess(type, points);
        }

        /// <summary>
        /// Generates time-bucketed historical mock data.
        /// </summary>
        public List<HealthDataPoint> GenerateHistory(HealthDataType type, DateTime startTime,
            DateTime endTime, HealthInterval interval)
        {
            if (!_profiles.TryGetValue(type, out var profile))
                return new List<HealthDataPoint>();

            var points = new List<HealthDataPoint>();
            DateTime current = startTime;

            while (current < endTime)
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

                if (bucketEnd > endTime) bucketEnd = endTime;

                double value;
                var aggregation = HealthDataTypeInfo.GetDefaultAggregation(type);

                if (aggregation == HealthAggregationType.CumulativeSum)
                {
                    double hours = (bucketEnd - current).TotalHours;
                    value = RandomInRange(profile.MinDaily, profile.MaxDaily) * (hours / 24.0);
                }
                else
                {
                    value = RandomInRange(profile.MinValue, profile.MaxValue);
                }

                value = Math.Round(value, profile.DecimalPlaces);
                points.Add(new HealthDataPoint(type, value, current, bucketEnd, "Mock Data"));

                current = bucketEnd;
            }

            return points;
        }

        /// <summary>
        /// Generates a single mock observer update value.
        /// </summary>
        public double GenerateObserverValue(HealthDataType type)
        {
            if (!_profiles.TryGetValue(type, out var profile))
                return 0;

            return Math.Round(RandomInRange(profile.MinValue, profile.MaxValue), profile.DecimalPlaces);
        }

        // ====================================================================
        // Mock Profiles
        // ====================================================================

        private class MockProfile
        {
            public double MinValue;    // For discrete/most-recent types
            public double MaxValue;
            public double MinDaily;    // For cumulative types (full day value)
            public double MaxDaily;
            public int DecimalPlaces;
        }

        private double RandomInRange(double min, double max)
        {
            return min + _rng.NextDouble() * (max - min);
        }

        private Dictionary<HealthDataType, MockProfile> CreateDefaultProfiles()
        {
            return new Dictionary<HealthDataType, MockProfile>
            {
                { HealthDataType.StepCount, new MockProfile { MinDaily = 3000, MaxDaily = 12000, MinValue = 0, MaxValue = 500, DecimalPlaces = 0 } },
                { HealthDataType.DistanceWalking, new MockProfile { MinDaily = 2000, MaxDaily = 8000, MinValue = 0, MaxValue = 333, DecimalPlaces = 0 } },
                { HealthDataType.ActiveEnergy, new MockProfile { MinDaily = 200, MaxDaily = 800, MinValue = 0, MaxValue = 50, DecimalPlaces = 0 } },
                { HealthDataType.FloorsClimbed, new MockProfile { MinDaily = 5, MaxDaily = 25, MinValue = 0, MaxValue = 3, DecimalPlaces = 0 } },
                { HealthDataType.HeartRate, new MockProfile { MinValue = 58, MaxValue = 95, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
                { HealthDataType.RestingHeartRate, new MockProfile { MinValue = 55, MaxValue = 72, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
                { HealthDataType.BodyMass, new MockProfile { MinValue = 65, MaxValue = 80, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 1 } },
                { HealthDataType.Height, new MockProfile { MinValue = 1.65, MaxValue = 1.85, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 2 } },
                { HealthDataType.BMI, new MockProfile { MinValue = 20, MaxValue = 26, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 1 } },

                // V2 types
                { HealthDataType.SleepAnalysis, new MockProfile { MinDaily = 5.5, MaxDaily = 9.0, MinValue = 0, MaxValue = 9, DecimalPlaces = 1 } },
                { HealthDataType.BloodOxygen, new MockProfile { MinValue = 94, MaxValue = 100, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
                { HealthDataType.WorkoutSession, new MockProfile { MinDaily = 15, MaxDaily = 90, MinValue = 0, MaxValue = 60, DecimalPlaces = 0 } },
                { HealthDataType.BloodPressureSystolic, new MockProfile { MinValue = 110, MaxValue = 135, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
                { HealthDataType.BloodPressureDiastolic, new MockProfile { MinValue = 70, MaxValue = 90, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
                { HealthDataType.RespiratoryRate, new MockProfile { MinValue = 12, MaxValue = 20, MinDaily = 0, MaxDaily = 0, DecimalPlaces = 0 } },
            };
        }
    }
}
