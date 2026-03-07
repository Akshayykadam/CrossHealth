// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Sample Scene - Health Dashboard UI
// Copyright (c) 2025. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.UI;
using CrossHealth;

namespace CrossHealth.Samples
{
    /// <summary>
    /// Sample UI script demonstrating the CrossHealth plugin.
    /// Attach to a Canvas GameObject with the UI elements referenced below.
    ///
    /// This script demonstrates:
    /// - Requesting health data permissions
    /// - Querying each supported health data type
    /// - Displaying results in a simple UI
    /// - Error handling
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

        [Header("Display")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text dataDisplayText;

        [Header("Settings")]
        [Tooltip("Number of days to look back when querying data")]
        [SerializeField] private int lookBackDays = 1;

        private bool _permissionsGranted = false;

        private void Start()
        {
            // Check availability
            bool available = CrossHealthManager.Instance.IsAvailable();
            SetStatus(available ? "Health services available. Request permissions to begin." : "Health services NOT available on this device.");

            // Setup button listeners
            if (requestPermissionsButton != null)
                requestPermissionsButton.onClick.AddListener(OnRequestPermissions);

            if (getStepsButton != null)
                getStepsButton.onClick.AddListener(OnGetSteps);

            if (getHeartRateButton != null)
                getHeartRateButton.onClick.AddListener(OnGetHeartRate);

            if (getDistanceButton != null)
                getDistanceButton.onClick.AddListener(OnGetDistance);

            if (getEnergyButton != null)
                getEnergyButton.onClick.AddListener(OnGetEnergy);

            if (getFloorsButton != null)
                getFloorsButton.onClick.AddListener(OnGetFloors);

            if (getRestingHRButton != null)
                getRestingHRButton.onClick.AddListener(OnGetRestingHR);

            if (getBodyMassButton != null)
                getBodyMassButton.onClick.AddListener(OnGetBodyMass);

            if (getHeightButton != null)
                getHeightButton.onClick.AddListener(OnGetHeight);

            if (getBMIButton != null)
                getBMIButton.onClick.AddListener(OnGetBMI);

            if (getAllDataButton != null)
                getAllDataButton.onClick.AddListener(OnGetAllData);

            // Initially disable data buttons until permissions are granted
            SetDataButtonsInteractable(false);
        }

        // ====================================================================
        // Button Handlers
        // ====================================================================

        private void OnRequestPermissions()
        {
            SetStatus("Requesting permissions...");

            CrossHealthManager.Instance.RequestAllPermissions((granted) =>
            {
                _permissionsGranted = granted;
                SetDataButtonsInteractable(granted);

                if (granted)
                {
                    SetStatus("Permissions granted! Tap any button to fetch data.");
                }
                else
                {
                    SetStatus("Permissions denied. Please enable in device Settings.");
                }
            });
        }

        private void OnGetSteps()
        {
            SetStatus("Fetching steps...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetStepCount(start, end, (steps) =>
            {
                SetDataDisplay($"Steps ({GetTimeRangeLabel()}): {steps:N0}");
                SetStatus("Step count retrieved.");
            });
        }

        private void OnGetHeartRate()
        {
            SetStatus("Fetching heart rate...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetHeartRate(start, end, (hr) =>
            {
                SetDataDisplay($"Avg Heart Rate ({GetTimeRangeLabel()}): {hr:F0} bpm");
                SetStatus("Heart rate retrieved.");
            });
        }

        private void OnGetDistance()
        {
            SetStatus("Fetching distance...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetDistance(start, end, (meters) =>
            {
                double km = meters / 1000.0;
                SetDataDisplay($"Distance ({GetTimeRangeLabel()}): {km:F2} km");
                SetStatus("Distance retrieved.");
            });
        }

        private void OnGetEnergy()
        {
            SetStatus("Fetching active energy...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetActiveEnergy(start, end, (kcal) =>
            {
                SetDataDisplay($"Active Energy ({GetTimeRangeLabel()}): {kcal:F0} kcal");
                SetStatus("Active energy retrieved.");
            });
        }

        private void OnGetFloors()
        {
            SetStatus("Fetching floors climbed...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetFloorsClimbed(start, end, (floors) =>
            {
                SetDataDisplay($"Floors Climbed ({GetTimeRangeLabel()}): {floors:F0}");
                SetStatus("Floors climbed retrieved.");
            });
        }

        private void OnGetRestingHR()
        {
            SetStatus("Fetching resting heart rate...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetRestingHeartRate(start, end, (rhr) =>
            {
                SetDataDisplay($"Resting Heart Rate ({GetTimeRangeLabel()}): {rhr:F0} bpm");
                SetStatus("Resting heart rate retrieved.");
            });
        }

        private void OnGetBodyMass()
        {
            SetStatus("Fetching body mass...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays * 30);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetBodyMass(start, end, (kg) =>
            {
                SetDataDisplay($"Body Mass (latest): {kg:F1} kg");
                SetStatus("Body mass retrieved.");
            });
        }

        private void OnGetHeight()
        {
            SetStatus("Fetching height...");
            DateTime start = DateTime.Today.AddDays(-365);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetHeight(start, end, (meters) =>
            {
                double cm = meters * 100.0;
                SetDataDisplay($"Height (latest): {cm:F1} cm ({meters:F2} m)");
                SetStatus("Height retrieved.");
            });
        }

        private void OnGetBMI()
        {
            SetStatus("Fetching BMI...");
            DateTime start = DateTime.Today.AddDays(-365);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.GetBMI(start, end, (bmi) =>
            {
                string category = GetBMICategory(bmi);
                SetDataDisplay($"BMI (latest): {bmi:F1} ({category})");
                SetStatus("BMI retrieved.");
            });
        }

        private void OnGetAllData()
        {
            SetStatus("Fetching all health data...");
            DateTime start = DateTime.Today.AddDays(-lookBackDays);
            DateTime end = DateTime.Now;
            string output = "";

            int completed = 0;
            int total = 6; // Number of metrics to fetch

            CrossHealthManager.Instance.GetStepCount(start, end, (v) => {
                output += $"Steps: {v:N0}\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetDistance(start, end, (v) => {
                output += $"Distance: {v / 1000.0:F2} km\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetActiveEnergy(start, end, (v) => {
                output += $"Active Energy: {v:F0} kcal\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetFloorsClimbed(start, end, (v) => {
                output += $"Floors: {v:F0}\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetHeartRate(start, end, (v) => {
                output += $"Heart Rate: {v:F0} bpm\n";
                if (++completed >= total) FinalizeAllData(output);
            });
            CrossHealthManager.Instance.GetRestingHeartRate(start, end, (v) => {
                output += $"Resting HR: {v:F0} bpm\n";
                if (++completed >= total) FinalizeAllData(output);
            });
        }

        private void FinalizeAllData(string output)
        {
            SetDataDisplay($"--- Health Dashboard ({GetTimeRangeLabel()}) ---\n{output}");
            SetStatus("All data retrieved.");
        }

        // ====================================================================
        // UI Helpers
        // ====================================================================

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
            Debug.Log($"[CrossHealth Sample] {message}");
        }

        private void SetDataDisplay(string content)
        {
            if (dataDisplayText != null)
                dataDisplayText.text = content;
        }

        private void SetDataButtonsInteractable(bool interactable)
        {
            Button[] dataButtons = {
                getStepsButton, getHeartRateButton, getDistanceButton,
                getEnergyButton, getFloorsButton, getRestingHRButton,
                getBodyMassButton, getHeightButton, getBMIButton,
                getAllDataButton
            };

            foreach (var btn in dataButtons)
            {
                if (btn != null)
                    btn.interactable = interactable;
            }
        }

        private string GetTimeRangeLabel()
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
