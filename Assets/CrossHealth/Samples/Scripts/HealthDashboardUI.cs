// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Sample Scene V2 - Health Dashboard UI
// Copyright (c) 2025. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CrossHealth;

namespace CrossHealth.Samples
{
    /// <summary>
    /// Sample UI demonstrating all CrossHealth V2 features:
    /// - Permission request flow
    /// - All 15 health data types
    /// - Real-time observer for heart rate
    /// - Historical data display
    /// - Mock data in Editor
    /// </summary>
    public class HealthDashboardUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button requestPermissionsButton;
        [SerializeField] private Button getStepsButton;
        [SerializeField] private Button getHeartRateButton;
        [SerializeField] private Button getDistanceButton;
        [SerializeField] private Button getEnergyButton;
        [SerializeField] private Button getFloorsButton;
        [SerializeField] private Button getRestingHRButton;
        [SerializeField] private Button getBodyMassButton;
        [SerializeField] private Button getHeightButton;
        [SerializeField] private Button getBMIButton;
        [SerializeField] private Button getAllDataButton;

        [Header("V2 Buttons")]
        [SerializeField] private Button getSleepButton;
        [SerializeField] private Button getSpO2Button;
        [SerializeField] private Button getWorkoutButton;
        [SerializeField] private Button getBloodPressureButton;
        [SerializeField] private Button getRespiratoryButton;
        [SerializeField] private Button toggleObserverButton;
        [SerializeField] private Button getHistoryButton;

        [Header("Display")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text dataDisplayText;

        [Header("Settings")]
        [Tooltip("Number of days to look back when querying data")]
        [SerializeField] private int lookBackDays = 1;

        private bool _permissionsGranted = false;
        private bool _isObservingHR = false;

        private void Start()
        {
            // Subscribe to events
            HealthEvents.OnDataReceived += OnDataEvent;
            HealthEvents.OnObserverUpdate += OnObserverEvent;

            // Check availability
            bool available = CrossHealthManager.Instance.IsAvailable();
            SetStatus(available
                ? "Health services available. Request permissions to begin."
                : "Health services NOT available on this device.");

            // Setup button listeners
            WireButton(requestPermissionsButton, OnRequestPermissions);
            WireButton(getStepsButton, OnGetSteps);
            WireButton(getHeartRateButton, OnGetHeartRate);
            WireButton(getDistanceButton, OnGetDistance);
            WireButton(getEnergyButton, OnGetEnergy);
            WireButton(getFloorsButton, OnGetFloors);
            WireButton(getRestingHRButton, OnGetRestingHR);
            WireButton(getBodyMassButton, OnGetBodyMass);
            WireButton(getHeightButton, OnGetHeight);
            WireButton(getBMIButton, OnGetBMI);
            WireButton(getAllDataButton, OnGetAllData);

            // V2 buttons
            WireButton(getSleepButton, OnGetSleep);
            WireButton(getSpO2Button, OnGetSpO2);
            WireButton(getWorkoutButton, OnGetWorkout);
            WireButton(getBloodPressureButton, OnGetBloodPressure);
            WireButton(getRespiratoryButton, OnGetRespiratory);
            WireButton(toggleObserverButton, OnToggleObserver);
            WireButton(getHistoryButton, OnGetHistory);

            SetDataButtonsInteractable(false);
        }

        private void OnDestroy()
        {
            HealthEvents.OnDataReceived -= OnDataEvent;
            HealthEvents.OnObserverUpdate -= OnObserverEvent;

            if (_isObservingHR)
                CrossHealthManager.Instance?.StopObserving(HealthDataType.HeartRate);
        }

        // ====================================================================
        // Event Handlers
        // ====================================================================

        private void OnDataEvent(HealthQueryResult result)
        {
            if (CrossHealthSettings.Instance.VerboseLogging)
                Debug.Log($"[CrossHealth Event] Data received: {result.DataType} = {result.AggregatedValue}");
        }

        private void OnObserverEvent(HealthDataType type, double value)
        {
            SetDataDisplay($"🔴 LIVE {type}: {value} {HealthDataTypeInfo.GetUnit(type)}\n(Updates every {CrossHealthSettings.Instance.DefaultObserverInterval}s)");
        }

        // ====================================================================
        // Button Handlers - V1
        // ====================================================================

        private void OnRequestPermissions()
        {
            SetStatus("Requesting permissions...");
            CrossHealthManager.Instance.RequestAllPermissions((granted) =>
            {
                _permissionsGranted = granted;
                SetDataButtonsInteractable(granted);
                SetStatus(granted
                    ? "Permissions granted! Tap any button to fetch data."
                    : "Permissions denied. Please enable in device Settings.");
            });
        }

        private void OnGetSteps()
        {
            SetStatus("Fetching steps...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetStepCount(start, end, (v) =>
            {
                SetDataDisplay($"🚶 Steps ({GetLabel()}): {v:N0}");
                SetStatus("Done.");
            });
        }

        private void OnGetHeartRate()
        {
            SetStatus("Fetching heart rate...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetHeartRate(start, end, (v) =>
            {
                SetDataDisplay($"❤️ Avg Heart Rate ({GetLabel()}): {v:F0} bpm");
                SetStatus("Done.");
            });
        }

        private void OnGetDistance()
        {
            SetStatus("Fetching distance...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetDistance(start, end, (v) =>
            {
                SetDataDisplay($"📏 Distance ({GetLabel()}): {v / 1000.0:F2} km");
                SetStatus("Done.");
            });
        }

        private void OnGetEnergy()
        {
            SetStatus("Fetching active energy...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetActiveEnergy(start, end, (v) =>
            {
                SetDataDisplay($"🔥 Active Energy ({GetLabel()}): {v:F0} kcal");
                SetStatus("Done.");
            });
        }

        private void OnGetFloors()
        {
            SetStatus("Fetching floors...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetFloorsClimbed(start, end, (v) =>
            {
                SetDataDisplay($"🏢 Floors Climbed ({GetLabel()}): {v:F0}");
                SetStatus("Done.");
            });
        }

        private void OnGetRestingHR()
        {
            SetStatus("Fetching resting HR...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetRestingHeartRate(start, end, (v) =>
            {
                SetDataDisplay($"💜 Resting Heart Rate ({GetLabel()}): {v:F0} bpm");
                SetStatus("Done.");
            });
        }

        private void OnGetBodyMass()
        {
            SetStatus("Fetching body mass...");
            CrossHealthManager.Instance.GetBodyMass(DateTime.Today.AddDays(-30), DateTime.Now, (v) =>
            {
                SetDataDisplay($"⚖️ Body Mass: {v:F1} kg");
                SetStatus("Done.");
            });
        }

        private void OnGetHeight()
        {
            SetStatus("Fetching height...");
            CrossHealthManager.Instance.GetHeight(DateTime.Today.AddDays(-365), DateTime.Now, (v) =>
            {
                SetDataDisplay($"📐 Height: {v * 100:F1} cm ({v:F2} m)");
                SetStatus("Done.");
            });
        }

        private void OnGetBMI()
        {
            SetStatus("Fetching BMI...");
            CrossHealthManager.Instance.GetBMI(DateTime.Today.AddDays(-365), DateTime.Now, (v) =>
            {
                SetDataDisplay($"📊 BMI: {v:F1} ({GetBMICategory(v)})");
                SetStatus("Done.");
            });
        }

        // ====================================================================
        // Button Handlers - V2
        // ====================================================================

        private void OnGetSleep()
        {
            SetStatus("Fetching sleep data...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetSleepAnalysis(start, end, (v) =>
            {
                SetDataDisplay($"😴 Sleep ({GetLabel()}): {v:F1} hrs");
                SetStatus("Done.");
            });
        }

        private void OnGetSpO2()
        {
            SetStatus("Fetching blood oxygen...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetBloodOxygen(start, end, (v) =>
            {
                SetDataDisplay($"🫁 Blood Oxygen (SpO2): {v:F0}%");
                SetStatus("Done.");
            });
        }

        private void OnGetWorkout()
        {
            SetStatus("Fetching workout data...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetWorkoutDuration(start, end, (v) =>
            {
                SetDataDisplay($"🏋️ Workout ({GetLabel()}): {v:F0} min");
                SetStatus("Done.");
            });
        }

        private void OnGetBloodPressure()
        {
            SetStatus("Fetching blood pressure...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetBloodPressure(start, end, (sys, dia) =>
            {
                SetDataDisplay($"🩺 Blood Pressure: {sys:F0}/{dia:F0} mmHg");
                SetStatus("Done.");
            });
        }

        private void OnGetRespiratory()
        {
            SetStatus("Fetching respiratory rate...");
            var (start, end) = GetTimeRange();
            CrossHealthManager.Instance.GetRespiratoryRate(start, end, (v) =>
            {
                SetDataDisplay($"🌬️ Respiratory Rate: {v:F0} breaths/min");
                SetStatus("Done.");
            });
        }

        private void OnToggleObserver()
        {
            if (_isObservingHR)
            {
                CrossHealthManager.Instance.StopObserving(HealthDataType.HeartRate);
                _isObservingHR = false;
                SetStatus("Heart rate observer stopped.");
                SetDataDisplay("Observer stopped. Tap to restart.");
                UpdateObserverButtonLabel();
            }
            else
            {
                CrossHealthManager.Instance.StartObserving(HealthDataType.HeartRate, (v) =>
                {
                    // Display is handled by OnObserverEvent
                }, 5f);
                _isObservingHR = true;
                SetStatus("Heart rate observer started (updates every 5s).");
                UpdateObserverButtonLabel();
            }
        }

        private void OnGetHistory()
        {
            SetStatus("Fetching 7-day step history...");
            CrossHealthManager.Instance.GetStepHistory(
                DateTime.Today.AddDays(-7), DateTime.Now,
                HealthInterval.Daily,
                (history) =>
                {
                    string output = "📈 7-Day Step History:\n";
                    foreach (var point in history)
                    {
                        string bar = new string('█', (int)(point.Value / 1000));
                        output += $"  {point.StartTime:ddd dd}: {point.Value:N0} {bar}\n";
                    }
                    SetDataDisplay(output);
                    SetStatus("History loaded.");
                });
        }

        private void OnGetAllData()
        {
            SetStatus("Fetching all health data...");
            var (start, end) = GetTimeRange();
            string output = "";
            int completed = 0;
            int total = 9;

            CrossHealthManager.Instance.GetStepCount(start, end, (v) => {
                output += $"🚶 Steps: {v:N0}\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetDistance(start, end, (v) => {
                output += $"📏 Distance: {v / 1000.0:F2} km\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetActiveEnergy(start, end, (v) => {
                output += $"🔥 Energy: {v:F0} kcal\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetFloorsClimbed(start, end, (v) => {
                output += $"🏢 Floors: {v:F0}\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetHeartRate(start, end, (v) => {
                output += $"❤️ Heart Rate: {v:F0} bpm\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetRestingHeartRate(start, end, (v) => {
                output += $"💜 Resting HR: {v:F0} bpm\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetSleepAnalysis(start, end, (v) => {
                output += $"😴 Sleep: {v:F1} hrs\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetBloodOxygen(start, end, (v) => {
                output += $"🫁 SpO2: {v:F0}%\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetRespiratoryRate(start, end, (v) => {
                output += $"🌬️ Resp: {v:F0} brpm\n";
                if (++completed >= total) FinalizeAllData(output);
            });
        }

        private void FinalizeAllData(string output)
        {
            SetDataDisplay($"--- Health Dashboard ({GetLabel()}) ---\n{output}");
            SetStatus("All data retrieved.");
        }

        // ====================================================================
        // UI Helpers
        // ====================================================================

        private void SetStatus(string msg)
        {
            if (statusText != null) statusText.text = msg;
            Debug.Log($"[CrossHealth Sample] {msg}");
        }

        private void SetDataDisplay(string content)
        {
            if (dataDisplayText != null) dataDisplayText.text = content;
        }

        private void WireButton(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn != null) btn.onClick.AddListener(action);
        }

        private void SetDataButtonsInteractable(bool on)
        {
            Button[] btns = {
                getStepsButton, getHeartRateButton, getDistanceButton,
                getEnergyButton, getFloorsButton, getRestingHRButton,
                getBodyMassButton, getHeightButton, getBMIButton,
                getAllDataButton, getSleepButton, getSpO2Button,
                getWorkoutButton, getBloodPressureButton, getRespiratoryButton,
                toggleObserverButton, getHistoryButton
            };
            foreach (var b in btns)
                if (b != null) b.interactable = on;
        }

        private void UpdateObserverButtonLabel()
        {
            if (toggleObserverButton != null)
            {
                var txt = toggleObserverButton.GetComponentInChildren<Text>();
                if (txt != null) txt.text = _isObservingHR ? "⏹ Stop HR Observer" : "📡 Start HR Observer";
            }
        }

        private (DateTime start, DateTime end) GetTimeRange()
        {
            return (DateTime.Today.AddDays(-lookBackDays), DateTime.Now);
        }

        private string GetLabel()
        {
            return lookBackDays == 1 ? "Today" : $"Last {lookBackDays} days";
        }

        private static string GetBMICategory(double bmi)
        {
            if (bmi <= 0) return "No data";
            if (bmi < 18.5) return "Underweight";
            if (bmi < 25.0) return "Normal";
            if (bmi < 30.0) return "Overweight";
            return "Obese";
        }
    }
}
