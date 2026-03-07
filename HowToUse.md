# CrossHealth – How to Use (Step-by-Step)

Complete guide for building and running CrossHealth on **iPhone** and **Android**.

---

## Part 1: Setup in Unity (Both Platforms)

### Step 1 – Import the Plugin
1. Open Unity (2021 LTS or newer)
2. Copy the entire `Assets/CrossHealth/` folder into your project's `Assets/` directory
3. Wait for Unity to compile – you should see **zero errors** in the Console

### Step 2 – Generate the Demo Scene
1. In Unity's top menu bar, click: **CrossHealth → Create Demo Scene & Prefab**
2. This automatically creates:
   - `Assets/CrossHealth/Prefabs/CrossHealthManager.prefab`
   - `Assets/CrossHealth/Samples/HealthDashboardScene.unity`
3. A dialog will confirm everything was created

### Step 3 – Open the Demo Scene
1. Go to `Assets/CrossHealth/Samples/` in the Project window
2. Double-click **HealthDashboardScene** to open it
3. You'll see a dark dashboard UI with:
   - 🔐 "Request Permissions" button at the top
   - 10 metric buttons (Steps, Distance, Energy, Floors, Heart Rate, etc.)
   - A data display panel
   - A status text bar

### Step 4 – Add Scene to Build Settings
1. Go to **File → Build Settings**
2. Click **Add Open Scenes** to add the Health Dashboard scene
3. Make sure it's checked in the scene list

---

## Part 2: Building for iPhone (iOS)

### Prerequisites
- Mac with **Xcode 14+** installed
- Physical iPhone/iPad (HealthKit has limited simulator support)
- Apple Developer account (free or paid)
- iPhone running **iOS 13** or later

### Step 5 – Configure iOS Build
1. Go to **File → Build Settings**
2. Select **iOS** from the platform list
3. Click **Switch Platform** (wait for Unity to reimport)
4. In **Player Settings** (bottom-left button):
   - **Company Name**: Your name/company
   - **Product Name**: CrossHealth Demo
   - **Bundle Identifier**: `com.yourname.crosshealth`
   - **Target minimum iOS version**: `13.0`

### Step 6 – Build the Xcode Project
1. Click **Build** in Build Settings
2. Choose a folder (e.g., `Builds/iOS`)
3. Wait for Unity to generate the Xcode project
4. **Automatic**: The plugin will automatically:
   - ✅ Add HealthKit capability
   - ✅ Add HealthKit.framework
   - ✅ Add privacy descriptions to Info.plist

### Step 7 – Open in Xcode and Deploy
1. Open the generated `.xcodeproj` file in Xcode
2. Select your **Team** in Signing & Capabilities
3. Connect your iPhone via USB
4. Select your device from the toolbar
5. Click **▶ Run**

### Step 8 – Test on iPhone
1. App opens showing the Health Dashboard
2. Tap **🔐 Request Permissions**
3. iOS will show the HealthKit permission dialog
4. Toggle ON the data types you want to share
5. Tap **Allow**
6. Buttons will become active
7. Tap any button (e.g., **🚶 Steps**) to see your data
8. Tap **📋 Get All Data** to fetch everything at once

### If No Data Appears
- Open the **Health** app on your iPhone
- Make sure it has data (walk around, or add data manually in Health → Browse → Activity → Steps → Add Data)
- Re-open CrossHealth Demo and query again

---

## Part 3: Building for Android

### Prerequisites
- Unity with **Android Build Support** module installed (via Unity Hub)
- Physical Android device with **Android 8+** (API 26+)
- USB cable and **USB Debugging** enabled on the device
- [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver) installed

### Step 9 – Install Health Connect on Device
- **Android 14+**: Health Connect is **built-in**, skip this step
- **Android 8-13**: Install **"Health Connect by Google"** from the Google Play Store on your device

### Step 10 – Resolve Android Dependencies
1. Install EDM4U in your Unity project (download .unitypackage from GitHub)
2. Go to: **Assets → External Dependency Manager → Android Resolver → Resolve**
3. Wait for it to download `androidx.health.connect:connect-client`
4. You should see "Resolution Succeeded" in the Console

### Step 11 – Configure Android Build
1. Go to **File → Build Settings**
2. Select **Android** from the platform list
3. Click **Switch Platform**
4. In **Player Settings**:
   - **Company Name**: Your name/company
   - **Product Name**: CrossHealth Demo
   - **Package Name**: `com.yourname.crosshealth`
   - **Minimum API Level**: `API Level 26` (Android 8.0)
   - **Target API Level**: `API Level 34` (Android 14) or latest
5. Under **Publishing Settings**:
   - Check **Custom Main Gradle Template** (if Gradle issues occur)

### Step 12 – Build and Run
1. Connect your Android device via USB
2. Enable **USB Debugging** on the device (Settings → Developer Options)
3. In Build Settings, click **Build and Run**
4. Unity will compile, build the APK, and install it on your device

### Step 13 – Test on Android
1. App opens showing the Health Dashboard
2. Tap **🔐 Request Permissions**
3. Android will launch the Health Connect permission screen
4. Toggle ON the data types you want to share
5. Tap **Allow**
6. Return to the app — buttons are now active
7. Tap any button to fetch data
8. Tap **📋 Get All Data** to see all metrics

### If No Data Appears
- Open **Health Connect** app on your device
- Check that other apps (Google Fit, Samsung Health, etc.) are syncing data to Health Connect
- Or manually add records via Health Connect's "Add Entry" feature
- Re-query in CrossHealth Demo

---

## Part 4: Using CrossHealth in Your Own Project

### Basic Integration (3 lines of code)

```csharp
using CrossHealth;
using System;

public class MyHealthScript : MonoBehaviour
{
    void Start()
    {
        // 1. Request permissions
        CrossHealthManager.Instance.RequestAllPermissions((granted) =>
        {
            if (granted)
            {
                // 2. Query data
                CrossHealthManager.Instance.GetStepCount(
                    DateTime.Today, DateTime.Now,
                    (steps) =>
                    {
                        // 3. Use the data
                        Debug.Log($"You walked {steps} steps today!");
                    }
                );
            }
        });
    }
}
```

### Available Methods
| Method | Returns | Unit |
|---|---|---|
| `GetStepCount()` | Total steps | steps |
| `GetDistance()` | Total distance | meters |
| `GetActiveEnergy()` | Calories burned | kcal |
| `GetFloorsClimbed()` | Floors count | floors |
| `GetHeartRate()` | Average HR | bpm |
| `GetRestingHeartRate()` | Resting HR | bpm |
| `GetBodyMass()` | Latest weight | kg |
| `GetHeight()` | Latest height | meters |
| `GetBMI()` | Latest BMI | kg/m² |

### Tips
- Always call `RequestPermissions()` or `RequestAllPermissions()` before querying
- Use `IsAvailable()` to check if the device supports health data
- All callbacks are asynchronous — don't assume data is available immediately
- Test on **physical devices** only — Editor/simulators return empty data
- The `CrossHealthManager` auto-creates itself as a singleton; no prefab needed in your own scenes

---

## Troubleshooting

| Problem | Solution |
|---|---|
| "Health services NOT available" | Enable HealthKit in Xcode (iOS) or install Health Connect app (Android) |
| No data returned | Make sure the device has actual health data recorded |
| Build fails on iOS | Check Xcode signing & team, and that HealthKit capability was added |
| Gradle error on Android | Resolve dependencies via EDM4U; update Gradle plugin if needed |
| Permission dialog doesn't show | Re-install the app; check `Info.plist` (iOS) or `AndroidManifest.xml` (Android) |
| Buttons stay disabled | Permissions were denied — open device Settings and enable health access for the app |
