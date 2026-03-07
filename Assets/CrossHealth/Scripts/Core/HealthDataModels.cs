// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrossHealth
{
    /// <summary>
    /// Represents a single health data measurement with time range and value.
    /// </summary>
    [Serializable]
    public class HealthDataPoint
    {
        /// <summary>Start time of the measurement period.</summary>
        public DateTime StartTime;

        /// <summary>End time of the measurement period.</summary>
        public DateTime EndTime;

        /// <summary>The measured value (units depend on HealthDataType).</summary>
        public double Value;

        /// <summary>The type of health data this point represents.</summary>
        public HealthDataType Type;

        /// <summary>Optional source device or app name.</summary>
        public string Source;

        public HealthDataPoint() { }

        public HealthDataPoint(HealthDataType type, double value, DateTime startTime, DateTime endTime, string source = "")
        {
            Type = type;
            Value = value;
            StartTime = startTime;
            EndTime = endTime;
            Source = source;
        }

        public override string ToString()
        {
            return $"[{Type}] {Value} {HealthDataTypeInfo.GetUnit(Type)} ({StartTime:g} - {EndTime:g})";
        }
    }

    /// <summary>
    /// Result wrapper for health data queries. Contains either data or an error.
    /// </summary>
    [Serializable]
    public class HealthQueryResult
    {
        /// <summary>Whether the query completed successfully.</summary>
        public bool Success;

        /// <summary>Error message if the query failed, empty otherwise.</summary>
        public string ErrorMessage;

        /// <summary>The queried health data type.</summary>
        public HealthDataType DataType;

        /// <summary>List of data points returned by the query.</summary>
        public List<HealthDataPoint> DataPoints;

        /// <summary>
        /// Convenience property returning the aggregated value from the first data point,
        /// or 0 if no data is available.
        /// </summary>
        public double AggregatedValue
        {
            get
            {
                if (DataPoints != null && DataPoints.Count > 0)
                    return DataPoints[0].Value;
                return 0;
            }
        }

        public static HealthQueryResult CreateSuccess(HealthDataType type, List<HealthDataPoint> points)
        {
            return new HealthQueryResult
            {
                Success = true,
                ErrorMessage = "",
                DataType = type,
                DataPoints = points ?? new List<HealthDataPoint>()
            };
        }

        public static HealthQueryResult CreateError(HealthDataType type, string error)
        {
            return new HealthQueryResult
            {
                Success = false,
                ErrorMessage = error,
                DataType = type,
                DataPoints = new List<HealthDataPoint>()
            };
        }
    }

    /// <summary>
    /// JSON-serializable payload for native → Unity communication.
    /// Used internally by the native bridges to pass data back to C#.
    /// </summary>
    [Serializable]
    internal class NativeCallbackPayload
    {
        public string requestId;
        public bool success;
        public string error;
        public int dataType;
        public NativeDataPoint[] dataPoints;

        [Serializable]
        internal class NativeDataPoint
        {
            public double startTime; // Unix timestamp (seconds)
            public double endTime;   // Unix timestamp (seconds)
            public double value;
            public string source;
        }

        /// <summary>
        /// Converts the native payload into a HealthQueryResult.
        /// </summary>
        public HealthQueryResult ToQueryResult()
        {
            var type = (HealthDataType)dataType;

            if (!success)
                return HealthQueryResult.CreateError(type, error ?? "Unknown error");

            var points = new List<HealthDataPoint>();
            if (dataPoints != null)
            {
                foreach (var np in dataPoints)
                {
                    points.Add(new HealthDataPoint(
                        type,
                        np.value,
                        DateTimeFromUnix(np.startTime),
                        DateTimeFromUnix(np.endTime),
                        np.source ?? ""
                    ));
                }
            }

            return HealthQueryResult.CreateSuccess(type, points);
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static DateTime DateTimeFromUnix(double unixTimestamp)
        {
            return UnixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
    }

    /// <summary>
    /// Utility class for DateTime ↔ Unix timestamp conversions.
    /// </summary>
    public static class HealthDateUtils
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>Converts a DateTime to a Unix timestamp (seconds since epoch).</summary>
        public static double ToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        /// <summary>Converts a Unix timestamp to a local DateTime.</summary>
        public static DateTime FromUnixTimestamp(double unixTimestamp)
        {
            return UnixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
    }
}
