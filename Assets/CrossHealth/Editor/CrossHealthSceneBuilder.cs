// CrossHealth - Unity Plugin for HealthKit & Health Connect
// Editor Script - Auto-generates the Health Dashboard demo scene + prefab
// Copyright (c) 2025. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CrossHealth.Editor
{
    /// <summary>
    /// Editor utility that creates the Health Dashboard demo scene and CrossHealthManager prefab
    /// with a single menu click. Access via: CrossHealth → Create Demo Scene
    /// </summary>
    public static class CrossHealthSceneBuilder
    {
        private const string PREFAB_PATH = "Assets/CrossHealth/Prefabs/CrossHealthManager.prefab";
        private const string SCENE_PATH = "Assets/CrossHealth/Samples/HealthDashboardScene.unity";

        // ====================================================================
        // Menu Items
        // ====================================================================

        [MenuItem("CrossHealth/Create Demo Scene & Prefab", false, 1)]
        public static void CreateDemoSceneAndPrefab()
        {
            CreatePrefab();
            CreateDemoScene();
            Debug.Log("[CrossHealth] Demo scene and prefab created successfully!");
        }

        [MenuItem("CrossHealth/Create Prefab Only", false, 2)]
        public static void CreatePrefabOnly()
        {
            CreatePrefab();
        }

        [MenuItem("CrossHealth/Create Demo Scene Only", false, 3)]
        public static void CreateDemoSceneOnly()
        {
            CreateDemoScene();
        }

        // ====================================================================
        // Prefab Builder
        // ====================================================================

        private static void CreatePrefab()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/CrossHealth/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/CrossHealth", "Prefabs");
            }

            // Create manager GameObject
            var managerGO = new GameObject("CrossHealthManager");
            managerGO.AddComponent<CrossHealthManager>();

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(managerGO, PREFAB_PATH);
            Object.DestroyImmediate(managerGO);

            AssetDatabase.Refresh();
            Debug.Log($"[CrossHealth] Prefab created at: {PREFAB_PATH}");
        }

        // ====================================================================
        // Scene Builder
        // ====================================================================

        private static void CreateDemoScene()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/CrossHealth/Samples"))
            {
                AssetDatabase.CreateFolder("Assets/CrossHealth", "Samples");
            }

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // --- CrossHealthManager ---
            var managerGO = new GameObject("CrossHealthManager");
            managerGO.AddComponent<CrossHealthManager>();

            // --- Canvas ---
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGO.AddComponent<GraphicRaycaster>();

            // --- EventSystem ---
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // --- Background Panel ---
            var bgPanel = CreatePanel(canvasGO.transform, "BackgroundPanel",
                new Vector2(0, 0), new Vector2(1, 1), new Color(0.12f, 0.12f, 0.15f, 1f));

            // --- Title ---
            var title = CreateText(bgPanel.transform, "TitleText", "CrossHealth Dashboard",
                28, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20), new Vector2(0, -80));

            // --- Status Text ---
            var statusText = CreateText(bgPanel.transform, "StatusText", "Initializing...",
                16, FontStyle.Italic, TextAnchor.MiddleCenter, new Color(0.7f, 0.85f, 1f, 1f),
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -90), new Vector2(0, -130));

            // --- Data Display Panel ---
            var dataPanel = CreatePanel(bgPanel.transform, "DataDisplayPanel",
                new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.88f), new Color(0.15f, 0.18f, 0.22f, 1f));

            var dataText = CreateText(dataPanel.transform, "DataDisplayText",
                "Tap 'Request Permissions' to begin.\n\nThen tap any metric button to fetch data.",
                18, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.9f, 0.95f, 1f, 1f),
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(20, 20), new Vector2(-20, -20));

            // --- Buttons Section ---
            float buttonStartY = 0.42f;
            float buttonHeight = 0.04f;
            float buttonGap = 0.005f;

            var permBtn = CreateButton(bgPanel.transform, "RequestPermissionsButton", "🔐  Request Permissions",
                new Color(0.2f, 0.6f, 1f, 1f), buttonStartY, buttonHeight);

            float y = buttonStartY - buttonHeight - buttonGap * 3;

            var stepsBtn = CreateButton(bgPanel.transform, "GetStepsButton", "🚶  Steps",
                new Color(0.3f, 0.7f, 0.3f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var distBtn = CreateButton(bgPanel.transform, "GetDistanceButton", "📏  Distance",
                new Color(0.3f, 0.65f, 0.4f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var energyBtn = CreateButton(bgPanel.transform, "GetEnergyButton", "🔥  Active Energy",
                new Color(0.85f, 0.45f, 0.2f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var floorsBtn = CreateButton(bgPanel.transform, "GetFloorsButton", "🏢  Floors Climbed",
                new Color(0.5f, 0.55f, 0.7f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var hrBtn = CreateButton(bgPanel.transform, "GetHeartRateButton", "❤️  Heart Rate",
                new Color(0.85f, 0.25f, 0.3f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var rhrBtn = CreateButton(bgPanel.transform, "GetRestingHRButton", "💜  Resting Heart Rate",
                new Color(0.6f, 0.3f, 0.65f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var massBtn = CreateButton(bgPanel.transform, "GetBodyMassButton", "⚖️  Body Mass",
                new Color(0.5f, 0.6f, 0.35f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var heightBtn = CreateButton(bgPanel.transform, "GetHeightButton", "📐  Height",
                new Color(0.4f, 0.55f, 0.6f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap;

            var bmiBtn = CreateButton(bgPanel.transform, "GetBMIButton", "📊  BMI",
                new Color(0.55f, 0.5f, 0.3f, 1f), y, buttonHeight);
            y -= buttonHeight + buttonGap * 3;

            var allBtn = CreateButton(bgPanel.transform, "GetAllDataButton", "📋  Get All Data",
                new Color(0.9f, 0.7f, 0.2f, 1f), y, buttonHeight);

            // --- Attach HealthDashboardUI script ---
            var dashboardUI = canvasGO.AddComponent<Samples.HealthDashboardUI>();

            // Wire up references using SerializedObject
            var so = new SerializedObject(dashboardUI);
            so.FindProperty("requestPermissionsButton").objectReferenceValue = permBtn.GetComponent<Button>();
            so.FindProperty("getStepsButton").objectReferenceValue = stepsBtn.GetComponent<Button>();
            so.FindProperty("getHeartRateButton").objectReferenceValue = hrBtn.GetComponent<Button>();
            so.FindProperty("getDistanceButton").objectReferenceValue = distBtn.GetComponent<Button>();
            so.FindProperty("getEnergyButton").objectReferenceValue = energyBtn.GetComponent<Button>();
            so.FindProperty("getFloorsButton").objectReferenceValue = floorsBtn.GetComponent<Button>();
            so.FindProperty("getRestingHRButton").objectReferenceValue = rhrBtn.GetComponent<Button>();
            so.FindProperty("getBodyMassButton").objectReferenceValue = massBtn.GetComponent<Button>();
            so.FindProperty("getHeightButton").objectReferenceValue = heightBtn.GetComponent<Button>();
            so.FindProperty("getBMIButton").objectReferenceValue = bmiBtn.GetComponent<Button>();
            so.FindProperty("getAllDataButton").objectReferenceValue = allBtn.GetComponent<Button>();
            so.FindProperty("statusText").objectReferenceValue = statusText;
            so.FindProperty("dataDisplayText").objectReferenceValue = dataText;
            so.ApplyModifiedProperties();

            // --- Save Scene ---
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            Debug.Log($"[CrossHealth] Demo scene created at: {SCENE_PATH}");
            EditorUtility.DisplayDialog("CrossHealth",
                "Demo scene and prefab created successfully!\n\n" +
                $"Scene: {SCENE_PATH}\n" +
                $"Prefab: {PREFAB_PATH}\n\n" +
                "Build to a device and tap 'Request Permissions' to begin.",
                "OK");
        }

        // ====================================================================
        // UI Creation Helpers
        // ====================================================================

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = color;

            return go;
        }

        private static Text CreateText(Transform parent, string name, string content,
            int fontSize, FontStyle style, TextAnchor alignment, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;

            return text;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Color color, float yPos, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, yPos - height);
            rect.anchorMax = new Vector2(0.92f, yPos);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = color;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // Button text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 2);
            textRect.offsetMax = new Vector2(-10, -2);

            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;

            return go;
        }
    }
}
