<p align="center">
  <h1 align="center">CrossHealth</h1>
  <p align="center">
    <strong>Unity Plugin for Apple HealthKit (iOS) & Android Health Connect</strong>
  </p>
  <p align="center">
    A unified C# API to read health & fitness data from native health platforms — no Swift, Kotlin, or native code required.
  </p>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2021%20LTS%2B-000?logo=unity&logoColor=white" alt="Unity Version" />
  <img src="https://img.shields.io/badge/iOS-13%2B-000?logo=apple&logoColor=white" alt="iOS 13+" />
  <img src="https://img.shields.io/badge/Android-8%2B-34A853?logo=android&logoColor=white" alt="Android 8+" />
  <img src="https://img.shields.io/badge/license-Commercial-blue" alt="License" />
  <img src="https://img.shields.io/badge/version-2.0.0-green" alt="Version" />
</p>

---

## ✨ Features

- **Single unified API** — one C# interface for both iOS and Android
- **15 health metrics** — steps, distance, calories, floors, heart rate, resting HR, weight, height, BMI, sleep, SpO2, workout, blood pressure, respiratory rate
- **Real-time observers** — subscribe to live health data updates with configurable intervals
- **Historical data** — query time-bucketed history (hourly, daily, weekly, monthly)
- **Editor mock data** — test in Play Mode without a device, realistic simulated data
- **Events system** — subscribe to `OnDataReceived`, `OnObserverUpdate`, `OnPermissionChanged`
- **Custom editor window** — dashboard with quick testing, observer monitor, and settings
- **Zero native code** — native bridges are included and pre-configured
- **Automated build setup** — HealthKit capabilities and Health Connect permissions added automatically
- **Privacy-first** — no remote storage, local processing only, explicit user consent
- **Asset Store ready** — includes sample scene generator, prefab, and full documentation

---

## 🚀 Quick Start

### 1. Import
Copy the `Assets/CrossHealth/` folder into your Unity project.

### 2. Generate Demo Scene
In Unity: **CrossHealth → Create Demo Scene & Prefab** (one-click setup)

### 3. Use

```csharp
using CrossHealth;
using System;

// Request permissions
CrossHealthManager.Instance.RequestAllPermissions((granted) =>
{
    if (granted)
    {
        // Get today's step count
        CrossHealthManager.Instance.GetStepCount(
            DateTime.Today, DateTime.Now,
            (steps) => Debug.Log($"Steps today: {steps}")
        );
    }
});
```

That's it. No Xcode configuration, no Android manifest editing — it's all automated.

---

## 📊 Supported Health Metrics (15 types)

| Metric | Unit | iOS Source | Android Source |
|---|---|---|---|
| **Step Count** | steps | `HKQuantityTypeIdentifierStepCount` | `StepsRecord` |
| **Distance** | meters | `HKQuantityTypeIdentifierDistanceWalkingRunning` | `DistanceRecord` |
| **Active Energy** | kcal | `HKQuantityTypeIdentifierActiveEnergyBurned` | `ActiveCaloriesBurnedRecord` |
| **Floors Climbed** | floors | `HKQuantityTypeIdentifierFlightsClimbed` | `FloorsClimbedRecord` |
| **Heart Rate** | bpm | `HKQuantityTypeIdentifierHeartRate` | `HeartRateRecord` |
| **Resting Heart Rate** | bpm | `HKQuantityTypeIdentifierRestingHeartRate` | `RestingHeartRateRecord` |
| **Body Mass** | kg | `HKQuantityTypeIdentifierBodyMass` | `WeightRecord` |
| **Height** | meters | `HKQuantityTypeIdentifierHeight` | `HeightRecord` |
| **BMI** | kg/m² | `HKQuantityTypeIdentifierBodyMassIndex` | Calculated |
| **Sleep Analysis** | hours | `HKCategoryTypeIdentifierSleepAnalysis` | `SleepSessionRecord` |
| **Blood Oxygen (SpO2)** | % | `HKQuantityTypeIdentifierOxygenSaturation` | `OxygenSaturationRecord` |
| **Workout Session** | minutes | `HKWorkout` | `ExerciseSessionRecord` |
| **Blood Pressure (Sys)** | mmHg | `HKCorrelationTypeIdentifierBloodPressure` | `BloodPressureRecord` |
| **Blood Pressure (Dia)** | mmHg | ↑ | ↑ |
| **Respiratory Rate** | brpm | `HKQuantityTypeIdentifierRespiratoryRate` | `RespiratoryRateRecord` |

---

## 🏗️ Architecture

```
Unity C# Layer
├── CrossHealthManager          (Singleton public API)
├── HealthDataService           (Request tracking & routing)
├── HealthPermissionManager     (Permission flows)
├── HealthObserver              (Real-time data observation)
├── HealthHistoryService        (Time-bucketed history queries)
├── HealthEvents                (Global event system)
├── MockHealthDataProvider      (Editor simulation)
├── CrossHealthSettings         (ScriptableObject config)
├── HealthDataTypes / Models    (Enums, data classes)
│
├── IOSHealthBridge.cs          ──► CrossHealthKitBridge.mm (HealthKit)
└── AndroidHealthBridge.cs      ──► CrossHealthConnectBridge.java (Health Connect)
```

---

## 📁 Project Structure (25 files)

```
Assets/CrossHealth/
├── Scripts/
│   ├── CrossHealth.asmdef               # Runtime assembly definition
│   ├── Core/
│   │   ├── CrossHealthManager.cs        # Main singleton API
│   │   ├── CrossHealthSettings.cs       # ScriptableObject settings
│   │   ├── HealthDataService.cs         # Platform-abstracted query service
│   │   ├── HealthPermissionManager.cs   # Permission handling
│   │   ├── HealthDataTypes.cs           # 15 data type enums + HealthInterval
│   │   ├── HealthDataModels.cs          # Data models & JSON parsing
│   │   ├── HealthEvents.cs              # Global event system
│   │   ├── HealthObserver.cs            # Real-time data observer
│   │   ├── HealthHistoryService.cs      # Time-bucketed history
│   │   └── MockHealthDataProvider.cs    # Mock data for Editor
│   └── Platform/
│       ├── IOSHealthBridge.cs           # iOS DllImport bridge
│       └── AndroidHealthBridge.cs       # Android JNI bridge
├── Runtime/
│   ├── iOS/
│   │   ├── CrossHealthKitBridge.h       # C header
│   │   └── CrossHealthKitBridge.mm      # Objective-C++ HealthKit impl
│   └── Android/
│       └── CrossHealthConnectBridge.java # Java Health Connect impl
├── Plugins/Android/
│   └── AndroidManifest.xml              # Health Connect permissions
├── Editor/
│   ├── CrossHealth.Editor.asmdef        # Editor assembly definition
│   ├── CrossHealthPostProcessor.cs      # iOS build automation
│   ├── CrossHealthSceneBuilder.cs       # One-click demo scene generator
│   ├── CrossHealthEditorWindow.cs       # Custom editor dashboard
│   └── CrossHealthAndroidDependencies.xml
├── Samples/
│   ├── CrossHealth.Samples.asmdef       # Samples assembly definition
│   └── Scripts/
│       └── HealthDashboardUI.cs         # Full sample dashboard
└── Documentation/
    └── SetupGuide.md                    # Setup & API reference
```

---

## 📱 Platform Setup

### iOS
**Fully automated.** The `CrossHealthPostProcessor` handles:
- ✅ HealthKit capability added to Xcode project
- ✅ HealthKit framework linked
- ✅ Privacy descriptions added to Info.plist

**Requirements:** iOS 13+, Xcode 14+, physical device

### Android
**Mostly automated.** The included `AndroidManifest.xml` declares all permissions.

**Additional step:** Install [External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) and resolve:
```
Assets → External Dependency Manager → Android Resolver → Resolve
```

**Requirements:** Android 8+ (API 26+). Health Connect is built into Android 14+; on older versions users must install it from the Play Store.

---

## 🔧 API Reference

### Availability & Permissions
```csharp
bool available = CrossHealthManager.Instance.IsAvailable();

CrossHealthManager.Instance.RequestAllPermissions((granted) => { });

CrossHealthManager.Instance.RequestPermissions(
    new[] { HealthDataType.StepCount, HealthDataType.HeartRate },
    (granted) => { }
);
```

### Simple Queries
```csharp
DateTime start = DateTime.Today;
DateTime end = DateTime.Now;

// Activity
CrossHealthManager.Instance.GetStepCount(start, end, (steps) => { });
CrossHealthManager.Instance.GetDistance(start, end, (meters) => { });
CrossHealthManager.Instance.GetActiveEnergy(start, end, (kcal) => { });
CrossHealthManager.Instance.GetFloorsClimbed(start, end, (floors) => { });

// Vitals
CrossHealthManager.Instance.GetHeartRate(start, end, (bpm) => { });
CrossHealthManager.Instance.GetRestingHeartRate(start, end, (bpm) => { });
CrossHealthManager.Instance.GetBloodOxygen(start, end, (spo2) => { });
CrossHealthManager.Instance.GetBloodPressure(start, end, (sys, dia) => { });
CrossHealthManager.Instance.GetRespiratoryRate(start, end, (brpm) => { });

// Body
CrossHealthManager.Instance.GetBodyMass(start, end, (kg) => { });
CrossHealthManager.Instance.GetHeight(start, end, (meters) => { });
CrossHealthManager.Instance.GetBMI(start, end, (bmi) => { });

// Sleep & Workout
CrossHealthManager.Instance.GetSleepAnalysis(start, end, (hours) => { });
CrossHealthManager.Instance.GetWorkoutDuration(start, end, (minutes) => { });
```

### Real-time Observers
```csharp
// Start observing (updates every 5 seconds)
CrossHealthManager.Instance.StartObserving(HealthDataType.HeartRate, (bpm) =>
{
    Debug.Log($"Live HR: {bpm} bpm");
}, intervalSeconds: 5f);

// Stop
CrossHealthManager.Instance.StopObserving(HealthDataType.HeartRate);
CrossHealthManager.Instance.StopAllObservers();
```

### Historical Data with Time Bucketing
```csharp
// Get daily step count for the last 7 days
CrossHealthManager.Instance.GetStepHistory(
    DateTime.Today.AddDays(-7), DateTime.Now,
    HealthInterval.Daily,
    (history) =>
    {
        foreach (var day in history)
            Debug.Log($"{day.StartTime:ddd}: {day.Value:N0} steps");
    }
);

// Generic history for any type
CrossHealthManager.Instance.GetHistory(
    HealthDataType.HeartRate,
    DateTime.Today.AddDays(-30), DateTime.Now,
    HealthInterval.Weekly,
    (points) => { }
);
```

### Events System
```csharp
// Subscribe to data events
HealthEvents.OnDataReceived += (result) => { };
HealthEvents.OnQueryError += (type, error) => { };

// Observer events
HealthEvents.OnObserverUpdate += (type, value) => { };
HealthEvents.OnObserverStarted += (type) => { };
HealthEvents.OnObserverStopped += (type) => { };

// Permission events
HealthEvents.OnPermissionChanged += (type, granted) => { };
HealthEvents.OnAllPermissionsResolved += (allGranted) => { };
```

### Advanced Query
```csharp
CrossHealthManager.Instance.QueryHealthData(
    HealthDataType.HeartRate,
    DateTime.Today, DateTime.Now,
    (result) =>
    {
        if (result.Success)
        {
            Debug.Log($"Aggregated: {result.AggregatedValue}");
            foreach (var point in result.DataPoints)
                Debug.Log($"  {point.Value} bpm at {point.StartTime}");
        }
    }
);
```

---

## 🎲 Editor Testing (Mock Data)

Test your integration without a device:

1. **Enable**: Mock data is enabled by default in the Editor
2. **Configure**: **Window → CrossHealth → Dashboard** or create a settings asset via **Assets → Create → CrossHealth → Settings**
3. **Test**: Enter Play Mode — all queries return realistic simulated data
4. **Observer**: Mock observers also work, generating values at configurable intervals

| Setting | Default | Description |
|---|---|---|
| Use Mock Data in Editor | ✅ | Enable/disable mock data |
| Random Seed | 0 | Fixed seed for reproducible data (0 = random) |
| Response Delay | 0.2s | Simulated network delay |
| Observer Interval | 5s | Default observer update frequency |
| Verbose Logging | ❌ | Log all events and native calls |

---

## 🔒 Privacy & Security

| Principle | Implementation |
|---|---|
| **No remote storage** | All data stays on-device |
| **No caching** | Fresh queries from the OS health store |
| **Explicit consent** | Users must grant permission before access |
| **Revocable** | Users can revoke via device Settings at any time |
| **No tracking** | Zero analytics, telemetry, or third-party SDKs |

---

## 🗺️ Roadmap

- [ ] Write health data support
- [ ] Native iOS `HKObserverQuery` integration
- [ ] Native Android data change notifications
- [ ] Native bridge support for V2 data types (Sleep, SpO2, etc.)
- [ ] Wearable device streaming (Apple Watch, Wear OS)
- [ ] Async/Await support via UniTask

---

## 📄 License

Commercial license. See [LICENSE](LICENSE) for details.

---

<p align="center">
  <sub>Built for Unity developers who'd rather ship features than write native bridges.</sub>
</p>
