// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

namespace CrossHealth
{
    /// <summary>
    /// Supported health data types that can be queried across platforms.
    /// </summary>
    public enum HealthDataType
    {
        /// <summary>Number of steps taken.</summary>
        StepCount = 0,

        /// <summary>Distance walked or run in meters.</summary>
        DistanceWalking = 1,

        /// <summary>Active energy burned in kilocalories.</summary>
        ActiveEnergy = 2,

        /// <summary>Number of floors climbed.</summary>
        FloorsClimbed = 3,

        /// <summary>Heart rate in beats per minute.</summary>
        HeartRate = 4,

        /// <summary>Resting heart rate in beats per minute.</summary>
        RestingHeartRate = 5,

        /// <summary>Body mass in kilograms.</summary>
        BodyMass = 6,

        /// <summary>Height in meters.</summary>
        Height = 7,

        /// <summary>Body Mass Index (kg/m²).</summary>
        BMI = 8,

        // --- V2 Data Types ---

        /// <summary>Sleep analysis (hours of sleep).</summary>
        SleepAnalysis = 9,

        /// <summary>Blood oxygen saturation (SpO2) as percentage.</summary>
        BloodOxygen = 10,

        /// <summary>Workout session duration in minutes.</summary>
        WorkoutSession = 11,

        /// <summary>Blood pressure – systolic over diastolic (mmHg). Value encodes systolic.</summary>
        BloodPressureSystolic = 12,

        /// <summary>Blood pressure – diastolic (mmHg).</summary>
        BloodPressureDiastolic = 13,

        /// <summary>Respiratory rate in breaths per minute.</summary>
        RespiratoryRate = 14
    }

    /// <summary>
    /// Time interval for historical data bucketing.
    /// </summary>
    public enum HealthInterval
    {
        /// <summary>One data point per hour.</summary>
        Hourly = 0,

        /// <summary>One data point per day.</summary>
        Daily = 1,

        /// <summary>One data point per week.</summary>
        Weekly = 2,

        /// <summary>One data point per month.</summary>
        Monthly = 3
    }

    /// <summary>
    /// Permission status for health data access.
    /// </summary>
    public enum HealthPermissionStatus
    {
        /// <summary>Permission status has not been determined yet.</summary>
        Unknown = 0,

        /// <summary>User has granted permission to read this data type.</summary>
        Authorized = 1,

        /// <summary>User has denied permission to read this data type.</summary>
        Denied = 2,

        /// <summary>Health data service is not available on this device.</summary>
        NotAvailable = 3
    }

    /// <summary>
    /// Aggregation type used when querying health data.
    /// </summary>
    public enum HealthAggregationType
    {
        /// <summary>Sum of all values in the time range (e.g., total steps).</summary>
        CumulativeSum = 0,

        /// <summary>Average of all values in the time range (e.g., average heart rate).</summary>
        DiscreteAverage = 1,

        /// <summary>Most recent value in the time range (e.g., latest body mass).</summary>
        MostRecent = 2
    }

    /// <summary>
    /// Utility class providing metadata about health data types.
    /// </summary>
    public static class HealthDataTypeInfo
    {
        /// <summary>
        /// Returns the recommended aggregation type for a given health data type.
        /// Cumulative types (steps, distance, energy, floors) use sum.
        /// Discrete types (heart rate, body metrics) use average or most recent.
        /// </summary>
        public static HealthAggregationType GetDefaultAggregation(HealthDataType type)
        {
            switch (type)
            {
                case HealthDataType.StepCount:
                case HealthDataType.DistanceWalking:
                case HealthDataType.ActiveEnergy:
                case HealthDataType.FloorsClimbed:
                    return HealthAggregationType.CumulativeSum;

                case HealthDataType.HeartRate:
                case HealthDataType.RestingHeartRate:
                case HealthDataType.BloodOxygen:
                case HealthDataType.RespiratoryRate:
                    return HealthAggregationType.DiscreteAverage;

                case HealthDataType.BodyMass:
                case HealthDataType.Height:
                case HealthDataType.BMI:
                case HealthDataType.BloodPressureSystolic:
                case HealthDataType.BloodPressureDiastolic:
                    return HealthAggregationType.MostRecent;

                case HealthDataType.SleepAnalysis:
                case HealthDataType.WorkoutSession:
                    return HealthAggregationType.CumulativeSum;

                default:
                    return HealthAggregationType.CumulativeSum;
            }
        }

        /// <summary>
        /// Returns the unit string for a given health data type.
        /// </summary>
        public static string GetUnit(HealthDataType type)
        {
            switch (type)
            {
                case HealthDataType.StepCount:
                    return "steps";
                case HealthDataType.DistanceWalking:
                    return "m";
                case HealthDataType.ActiveEnergy:
                    return "kcal";
                case HealthDataType.FloorsClimbed:
                    return "floors";
                case HealthDataType.HeartRate:
                case HealthDataType.RestingHeartRate:
                    return "bpm";
                case HealthDataType.BodyMass:
                    return "kg";
                case HealthDataType.Height:
                    return "m";
                case HealthDataType.BMI:
                    return "kg/m²";
                case HealthDataType.SleepAnalysis:
                    return "hrs";
                case HealthDataType.BloodOxygen:
                    return "%";
                case HealthDataType.WorkoutSession:
                    return "min";
                case HealthDataType.BloodPressureSystolic:
                case HealthDataType.BloodPressureDiastolic:
                    return "mmHg";
                case HealthDataType.RespiratoryRate:
                    return "brpm";
                default:
                    return "";
            }
        }
    }
}
