// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Custom Editor Window
// Copyright (c) 2025. All rights reserved.

using System;
using UnityEditor;
using UnityEngine;

namespace CrossHealth.Editor
{
    /// <summary>
    /// Custom editor window for CrossHealth plugin.
    /// Provides quick access to:
    /// - Connection status per platform
    /// - Mock data controls and testing
    /// - Quick query buttons for all data types
    /// - Observer monitoring
    ///
    /// Access via: Window → CrossHealth → Dashboard
    /// </summary>
    public class CrossHealthEditorWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private bool _showMockSettings = true;
        private bool _showQuickTest = true;
        private bool _showObservers = true;
        private bool _showStatus = true;

        // Mock test results
        private string _lastResult = "";
        private HealthDataType _selectedType = HealthDataType.StepCount;
        private int _lookBackDays = 1;

        // Observer state
        private HealthDataType _observeType = HealthDataType.HeartRate;
        private float _observeInterval = 5f;
        private string _observerLog = "";

        [MenuItem("Window/CrossHealth/Dashboard", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<CrossHealthEditorWindow>("CrossHealth");
            window.minSize = new Vector2(350, 500);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            EditorGUILayout.Space(5);
            DrawStatusSection();
            EditorGUILayout.Space(5);
            DrawMockSettingsSection();
            EditorGUILayout.Space(5);
            DrawQuickTestSection();
            EditorGUILayout.Space(5);
            DrawObserverSection();
            EditorGUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        // ====================================================================
        // Header
        // ====================================================================

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.FlexibleSpace();

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("♥ CrossHealth Dashboard", titleStyle, GUILayout.Height(35));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ====================================================================
        // Status Section
        // ====================================================================

        private void DrawStatusSection()
        {
            _showStatus = EditorGUILayout.Foldout(_showStatus, "📋 Platform Status", true, EditorStyles.foldoutHeader);
            if (!_showStatus) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Platform info
            DrawStatusRow("Platform", Application.platform.ToString());
            DrawStatusRow("Unity Version", Application.unityVersion);

#if UNITY_IOS
            DrawStatusRow("Target", "iOS (HealthKit)", true);
#elif UNITY_ANDROID
            DrawStatusRow("Target", "Android (Health Connect)", true);
#else
            DrawStatusRow("Target", "Editor (Mock Data)");
#endif

            bool mockEnabled = CrossHealthSettings.Instance.ShouldUseMockData;
            DrawStatusRow("Mock Data", mockEnabled ? "✅ Enabled" : "❌ Disabled");

            // Manager status
            bool managerExists = Application.isPlaying && CrossHealthManager.Instance != null;
            DrawStatusRow("Manager", managerExists ? "✅ Active" : "⚪ Not Running");

            if (managerExists && CrossHealthManager.Instance.Observer != null)
            {
                int observerCount = CrossHealthManager.Instance.Observer.ActiveObserverCount;
                DrawStatusRow("Active Observers", observerCount.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusRow(string label, string value, bool highlight = false)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));

            var style = highlight
                ? new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.3f, 0.8f, 0.3f) }, fontStyle = FontStyle.Bold }
                : EditorStyles.label;

            EditorGUILayout.LabelField(value, style);
            EditorGUILayout.EndHorizontal();
        }

        // ====================================================================
        // Mock Settings Section
        // ====================================================================

        private void DrawMockSettingsSection()
        {
            _showMockSettings = EditorGUILayout.Foldout(_showMockSettings, "🎲 Mock Data Settings", true, EditorStyles.foldoutHeader);
            if (!_showMockSettings) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var settings = CrossHealthSettings.Instance;

            EditorGUI.BeginChangeCheck();
            settings.UseMockDataInEditor = EditorGUILayout.Toggle("Use Mock Data in Editor", settings.UseMockDataInEditor);
            settings.MockDataSeed = EditorGUILayout.IntField("Random Seed (0 = random)", settings.MockDataSeed);
            settings.MockResponseDelay = EditorGUILayout.Slider("Response Delay (s)", settings.MockResponseDelay, 0f, 3f);
            settings.VerboseLogging = EditorGUILayout.Toggle("Verbose Logging", settings.VerboseLogging);

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Observer Settings", EditorStyles.boldLabel);
            settings.DefaultObserverInterval = EditorGUILayout.Slider("Observer Interval (s)", settings.DefaultObserverInterval, 1f, 60f);
            settings.MockObserversInEditor = EditorGUILayout.Toggle("Mock Observers in Editor", settings.MockObserversInEditor);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(settings);

            EditorGUILayout.Space(3);
            if (GUILayout.Button("Create Settings Asset"))
            {
                CreateSettingsAsset();
            }

            EditorGUILayout.EndVertical();
        }

        // ====================================================================
        // Quick Test Section
        // ====================================================================

        private void DrawQuickTestSection()
        {
            _showQuickTest = EditorGUILayout.Foldout(_showQuickTest, "🧪 Quick Test", true, EditorStyles.foldoutHeader);
            if (!_showQuickTest) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test health data queries.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _selectedType = (HealthDataType)EditorGUILayout.EnumPopup("Data Type", _selectedType);
            _lookBackDays = EditorGUILayout.IntSlider("Look Back Days", _lookBackDays, 1, 30);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔍 Query", GUILayout.Height(28)))
            {
                RunQuickTest();
            }

            if (GUILayout.Button("📋 Query All", GUILayout.Height(28)))
            {
                RunQueryAll();
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_lastResult))
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Result:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(_lastResult, GUILayout.MinHeight(60));
            }

            EditorGUILayout.EndVertical();
        }

        // ====================================================================
        // Observer Section
        // ====================================================================

        private void DrawObserverSection()
        {
            _showObservers = EditorGUILayout.Foldout(_showObservers, "📡 Observer Monitor", true, EditorStyles.foldoutHeader);
            if (!_showObservers) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to start observers.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _observeType = (HealthDataType)EditorGUILayout.EnumPopup("Observe Type", _observeType);
            _observeInterval = EditorGUILayout.Slider("Interval (s)", _observeInterval, 1f, 30f);

            EditorGUILayout.BeginHorizontal();

            bool isObserving = CrossHealthManager.Instance?.Observer?.IsObserving(_observeType) ?? false;

            GUI.backgroundColor = isObserving ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button(isObserving ? "⏹ Stop" : "▶ Start", GUILayout.Height(28)))
            {
                if (isObserving)
                {
                    CrossHealthManager.Instance.StopObserving(_observeType);
                    _observerLog += $"[{DateTime.Now:HH:mm:ss}] Stopped observing {_observeType}\n";
                }
                else
                {
                    CrossHealthManager.Instance.StartObserving(_observeType, (value) =>
                    {
                        _observerLog += $"[{DateTime.Now:HH:mm:ss}] {_observeType} = {value} {HealthDataTypeInfo.GetUnit(_observeType)}\n";
                        Repaint();
                    }, _observeInterval);
                    _observerLog += $"[{DateTime.Now:HH:mm:ss}] Started observing {_observeType}\n";
                }
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("🗑 Clear Log", GUILayout.Height(28)))
                _observerLog = "";

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_observerLog))
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.TextArea(_observerLog, GUILayout.MinHeight(80));
            }

            EditorGUILayout.EndVertical();
        }

        // ====================================================================
        // Actions
        // ====================================================================

        private void RunQuickTest()
        {
            DateTime start = DateTime.Today.AddDays(-_lookBackDays);
            DateTime end = DateTime.Now;

            CrossHealthManager.Instance.QueryHealthData(_selectedType, start, end, (result) =>
            {
                if (result.Success)
                {
                    _lastResult = $"{_selectedType}: {result.AggregatedValue} {HealthDataTypeInfo.GetUnit(_selectedType)}\n";
                    if (result.DataPoints.Count > 0)
                    {
                        foreach (var dp in result.DataPoints)
                            _lastResult += $"  {dp}\n";
                    }
                }
                else
                {
                    _lastResult = $"Error: {result.ErrorMessage}";
                }
                Repaint();
            });
        }

        private void RunQueryAll()
        {
            DateTime start = DateTime.Today.AddDays(-_lookBackDays);
            DateTime end = DateTime.Now;
            _lastResult = $"--- All Data (Last {_lookBackDays} day{(_lookBackDays > 1 ? "s" : "")}) ---\n";

            foreach (HealthDataType type in Enum.GetValues(typeof(HealthDataType)))
            {
                var t = type;
                CrossHealthManager.Instance.QueryHealthData(t, start, end, (result) =>
                {
                    if (result.Success)
                        _lastResult += $"{t}: {result.AggregatedValue} {HealthDataTypeInfo.GetUnit(t)}\n";
                    else
                        _lastResult += $"{t}: Error\n";
                    Repaint();
                });
            }
        }

        private void CreateSettingsAsset()
        {
            if (!AssetDatabase.IsValidFolder("Assets/CrossHealth/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/CrossHealth", "Resources");
            }

            var settings = CreateInstance<CrossHealthSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/CrossHealth/Resources/CrossHealthSettings.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("[CrossHealth] Settings asset created at Assets/CrossHealth/Resources/CrossHealthSettings.asset");
        }
    }
}
