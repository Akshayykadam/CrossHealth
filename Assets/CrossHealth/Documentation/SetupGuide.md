# CrossHealth – Setup Guide

> Unity Plugin for Apple HealthKit (iOS) & Android Health Connect

## Quick Start

1. **Import** the `CrossHealth` folder into your Unity project's `Assets/` directory
2. **Add** `CrossHealthManager` to your scene (drag the prefab or it auto-creates)
3. **Request permissions** and **query data** in your scripts:

```csharp
using CrossHealth;

// Request permissions
CrossHealthManager.Instance.RequestAllPermissions((granted) => {
    if (granted) {
        // Query today's steps
        CrossHealthManager.Instance.GetStepCount(
            DateTime.Today, DateTime.Now,
            (steps) => Debug.Log("Steps: " + steps)
        );
    }
});
```

---

## iOS Setup

### Automatic (recommended)
The `CrossHealthPostProcessor.cs` editor script automatically handles:
- Adding HealthKit capability to Xcode project
- Adding HealthKit framework
- Adding `NSHealthShareUsageDescription` to Info.plist
- Adding `NSHealthUpdateUsageDescription` to Info.plist

**No manual Xcode configuration needed.**

### Customizing Privacy Descriptions
Before building, you can override the default descriptions in code:

```csharp
#if UNITY_IOS
CrossHealth.Editor.CrossHealthPostProcessor.HealthShareUsageDescription = 
    "Your custom read-access explanation here.";
#endif
```

### Manual Setup (if needed)
If PostProcessBuild doesn't run:
1. Open Xcode project → Target → **Signing & Capabilities** → **+ Capability** → **HealthKit**
2. Open `Info.plist` and add:
   - `NSHealthShareUsageDescription` – Why you read health data
   - `NSHealthUpdateUsageDescription` – Why you write health data

### Requirements
- iOS 13+
- Xcode 14+
- Device with Health app (simulator has limited support)

---

## Android Setup

### Automatic (recommended)
The plugin includes:
- `AndroidManifest.xml` with Health Connect permissions (auto-merged at build time)
- `CrossHealthAndroidDependencies.xml` for Gradle dependency resolution

### Prerequisites
1. **Install External Dependency Manager for Unity** (EDM4U) from Google
2. Run **Assets → External Dependency Manager → Android Resolver → Resolve**
3. Build and run

### Health Connect Availability
- **Android 14+**: Health Connect is built-in
- **Android 8-13**: Users must install "Health Connect" from Play Store
- The plugin checks availability via `CrossHealthManager.Instance.IsAvailable()`

### Gradle Compatibility
If you encounter Gradle version errors:
1. Enable **Custom Main Gradle Template** in Player Settings
2. Update `com.android.tools.build:gradle` version to `7.4.2+`

### Required Permissions
Declared automatically in the manifest:
| Permission | Data Type |
|---|---|
| `READ_STEPS` | Step Count |
| `READ_DISTANCE` | Distance Walking |
| `READ_TOTAL_CALORIES_BURNED` | Active Energy |
| `READ_FLOORS_CLIMBED` | Floors Climbed |
| `READ_HEART_RATE` | Heart Rate |
| `READ_RESTING_HEART_RATE` | Resting Heart Rate |
| `READ_WEIGHT` | Body Mass |
| `READ_HEIGHT` | Height |

---

## API Reference

### CrossHealthManager (Singleton)

#### Availability
```csharp
bool available = CrossHealthManager.Instance.IsAvailable();
```

#### Permissions
```csharp
// Request specific types
CrossHealthManager.Instance.RequestPermissions(
    new[] { HealthDataType.StepCount, HealthDataType.HeartRate },
    (granted) => { /* ... */ }
);

// Request all types
CrossHealthManager.Instance.RequestAllPermissions((granted) => { /* ... */ });
```

#### Activity Data
```csharp
CrossHealthManager.Instance.GetStepCount(start, end, (value) => { });
CrossHealthManager.Instance.GetDistance(start, end, (meters) => { });
CrossHealthManager.Instance.GetActiveEnergy(start, end, (kcal) => { });
CrossHealthManager.Instance.GetFloorsClimbed(start, end, (floors) => { });
```

#### Vital Signs
```csharp
CrossHealthManager.Instance.GetHeartRate(start, end, (bpm) => { });
CrossHealthManager.Instance.GetRestingHeartRate(start, end, (bpm) => { });
```

#### Body Metrics
```csharp
CrossHealthManager.Instance.GetBodyMass(start, end, (kg) => { });
CrossHealthManager.Instance.GetHeight(start, end, (meters) => { });
CrossHealthManager.Instance.GetBMI(start, end, (bmi) => { });
```

#### Advanced Query
```csharp
CrossHealthManager.Instance.QueryHealthData(
    HealthDataType.HeartRate,
    DateTime.Today, DateTime.Now,
    (result) => {
        if (result.Success) {
            foreach (var point in result.DataPoints) {
                Debug.Log($"{point.Value} {point.Type} at {point.StartTime}");
            }
        }
    }
);
```

---

## Supported Data Types

| Type | Unit | iOS (HealthKit) | Android (Health Connect) |
|---|---|---|---|
| StepCount | steps | HKQuantityTypeIdentifierStepCount | StepsRecord |
| DistanceWalking | meters | HKQuantityTypeIdentifierDistanceWalkingRunning | DistanceRecord |
| ActiveEnergy | kcal | HKQuantityTypeIdentifierActiveEnergyBurned | ActiveCaloriesBurnedRecord |
| FloorsClimbed | floors | HKQuantityTypeIdentifierFlightsClimbed | FloorsClimbedRecord |
| HeartRate | bpm | HKQuantityTypeIdentifierHeartRate | HeartRateRecord |
| RestingHeartRate | bpm | HKQuantityTypeIdentifierRestingHeartRate | RestingHeartRateRecord |
| BodyMass | kg | HKQuantityTypeIdentifierBodyMass | WeightRecord |
| Height | meters | HKQuantityTypeIdentifierHeight | HeightRecord |
| BMI | kg/m² | HKQuantityTypeIdentifierBodyMassIndex | Calculated from Weight+Height |

---

## Troubleshooting

### iOS
| Issue | Solution |
|---|---|
| HealthKit not available | Ensure device supports HealthKit (not all iPads do) |
| Permission dialog doesn't appear | Check Info.plist has NSHealthShareUsageDescription |
| No data returned | Verify Health app has data; check date range |
| Build fails at HealthKit | Ensure HealthKit capability is added in Xcode |

### Android
| Issue | Solution |
|---|---|
| Health Connect not available | Install "Health Connect" from Play Store (Android < 14) |
| NoClassDefFoundError | Update Gradle plugin version (see Gradle Compatibility section) |
| Permissions not requested | Check AndroidManifest.xml has the permission declarations |
| No data returned | Open Health Connect app and verify data exists |

### General
| Issue | Solution |
|---|---|
| "Not available" in Editor | Health queries only work on devices; use Editor as design mode |
| Callback not received | Ensure `CrossHealthManager` GameObject exists and isn't destroyed |
| Multiple manager instances | Use `CrossHealthManager.Instance` — don't add multiple components |

---

## Privacy & Security

- **No remote storage**: All data stays on-device
- **No caching**: Data is queried fresh from the OS health store
- **Explicit consent**: Users must grant permission before any data access
- **Revocable**: Users can revoke permissions anytime via device Settings
- **Compliant**: Follows Apple HealthKit and Google Health Connect guidelines

---

## Version History

### 1.0.0
- Initial release
- Read support for 9 health data types
- iOS HealthKit and Android Health Connect bridges
- Automated build configuration
- Sample dashboard scene
