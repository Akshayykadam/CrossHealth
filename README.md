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
  <img src="https://img.shields.io/badge/version-1.0.0-green" alt="Version" />
</p>

---

## ✨ Features

- **Single unified API** — one C# interface for both iOS and Android
- **9 health metrics** — steps, distance, calories, floors, heart rate, resting HR, weight, height, BMI
- **Zero native code** — native bridges are included and pre-configured
- **Automated build setup** — HealthKit capabilities and Health Connect permissions added automatically
- **Privacy-first** — no remote storage, local processing only, explicit user consent
- **Asset Store ready** — includes sample scene, prefab, and full documentation

---

## 🚀 Quick Start

### 1. Import
Copy the `Assets/CrossHealth/` folder into your Unity project.

### 2. Use

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

## 📊 Supported Health Metrics

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
| **BMI** | kg/m² | `HKQuantityTypeIdentifierBodyMassIndex` | Calculated from Weight + Height |

---

## 🏗️ Architecture

```
Unity C# Layer
├── CrossHealthManager          (Singleton public API)
├── HealthDataService           (Request tracking & routing)
├── HealthPermissionManager     (Permission flows)
├── HealthDataTypes / Models    (Enums, data classes)
│
├── IOSHealthBridge.cs          ──► CrossHealthKitBridge.mm (HealthKit)
└── AndroidHealthBridge.cs      ──► CrossHealthConnectBridge.java (Health Connect)
```

All native callbacks are serialized as JSON and routed through `UnitySendMessage` to the `CrossHealthManager` singleton.

---

## 📁 Project Structure

```
Assets/CrossHealth/
├── Scripts/
│   ├── Core/
│   │   ├── CrossHealthManager.cs        # Main singleton API
│   │   ├── HealthDataService.cs         # Platform-abstracted query service
│   │   ├── HealthPermissionManager.cs   # Permission handling
│   │   ├── HealthDataTypes.cs           # Enums & type metadata
│   │   └── HealthDataModels.cs          # Data models & JSON parsing
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
│   ├── CrossHealthPostProcessor.cs      # iOS build automation
│   └── CrossHealthAndroidDependencies.xml
├── Samples/Scripts/
│   └── HealthDashboardUI.cs             # Sample dashboard scene
└── Documentation/
    └── SetupGuide.md                    # Full setup & API reference
```

---

## 📱 Platform Setup

### iOS
**Fully automated.** The `CrossHealthPostProcessor` handles:
- ✅ HealthKit capability added to Xcode project
- ✅ HealthKit framework linked
- ✅ `NSHealthShareUsageDescription` added to Info.plist
- ✅ `NSHealthUpdateUsageDescription` added to Info.plist

**Requirements:** iOS 13+, Xcode 14+, physical device (simulators have limited HealthKit support)

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
// Check availability
bool available = CrossHealthManager.Instance.IsAvailable();

// Request specific permissions
CrossHealthManager.Instance.RequestPermissions(
    new[] { HealthDataType.StepCount, HealthDataType.HeartRate },
    (granted) => { }
);

// Request all permissions
CrossHealthManager.Instance.RequestAllPermissions((granted) => { });
```

### Query Data (Simple)
```csharp
DateTime start = DateTime.Today;
DateTime end = DateTime.Now;

CrossHealthManager.Instance.GetStepCount(start, end, (steps) => { });
CrossHealthManager.Instance.GetDistance(start, end, (meters) => { });
CrossHealthManager.Instance.GetActiveEnergy(start, end, (kcal) => { });
CrossHealthManager.Instance.GetFloorsClimbed(start, end, (floors) => { });
CrossHealthManager.Instance.GetHeartRate(start, end, (bpm) => { });
CrossHealthManager.Instance.GetRestingHeartRate(start, end, (bpm) => { });
CrossHealthManager.Instance.GetBodyMass(start, end, (kg) => { });
CrossHealthManager.Instance.GetHeight(start, end, (meters) => { });
CrossHealthManager.Instance.GetBMI(start, end, (bmi) => { });
```

### Query Data (Advanced)
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
        else
        {
            Debug.LogError(result.ErrorMessage);
        }
    }
);
```

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
- [ ] Sleep analysis
- [ ] Blood oxygen (SpO2)
- [ ] Workout sessions
- [ ] Real-time heart rate streaming
- [ ] Wearable device integration

---

## 📄 License

Commercial license. See [LICENSE](LICENSE) for details.

---

<p align="center">
  <sub>Built for Unity developers who'd rather ship features than write native bridges.</sub>
</p>
