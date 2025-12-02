# Firebase Push Notification Setup Guide

## Prerequisites
- Firebase account (https://console.firebase.google.com)
- Firebase project created
- Firebase Admin SDK credentials

## Step 1: Create Firebase Project

1. Go to https://console.firebase.google.com
2. Click "Add Project"
3. Enter project name: "GlobalDisasterManagement" (or your preferred name)
4. Enable Google Analytics (optional)
5. Click "Create Project"

## Step 2: Generate Service Account Credentials

1. In Firebase Console, click the gear icon ⚙️ > Project Settings
2. Navigate to "Service Accounts" tab
3. Click "Generate new private key"
4. Save the JSON file as `firebase-adminsdk-credentials.json`
5. Place the file in your project root directory:
   ```
   FloodManagementSystem/
   ├── firebase-adminsdk-credentials.json  ← Here
   ├── FloodManagementSystem/
   │   ├── Program.cs
   │   └── ...
   ```

## Step 3: Configure Application Settings

The `appsettings.json` already has Firebase configuration:

```json
{
  "Firebase": {
    "CredentialPath": "firebase-adminsdk-credentials.json"
  }
}
```

For production, use environment-specific path:

```json
{
  "Firebase": {
    "CredentialPath": "/etc/secrets/firebase-adminsdk-credentials.json"
  }
}
```

## Step 4: Security Best Practices

### Development:
- Keep `firebase-adminsdk-credentials.json` in `.gitignore`
- Never commit credentials to version control

### Production:
- Store credentials in secure storage:
  - **Azure**: Azure Key Vault
  - **AWS**: AWS Secrets Manager
  - **Docker**: Mount as secret volume
- Use environment variables:
  ```bash
  export FIREBASE_CREDENTIAL_PATH="/secure/path/firebase-creds.json"
  ```

## Mobile App Setup

### Android (React Native / Flutter / Native)

1. **Add Firebase to Android App**:
   ```bash
   # In Firebase Console
   Project Settings → Add App → Android
   ```

2. **Download `google-services.json`**:
   - Place in `android/app/google-services.json`

3. **Add Firebase Messaging SDK**:

   **React Native:**
   ```bash
   npm install @react-native-firebase/app @react-native-firebase/messaging
   ```

   **Flutter:**
   ```yaml
   dependencies:
     firebase_core: ^2.24.0
     firebase_messaging: ^14.7.0
   ```

   **Native Android:**
   ```gradle
   implementation 'com.google.firebase:firebase-messaging:23.3.1'
   ```

4. **Request Permission & Get Token**:

   **React Native:**
   ```javascript
   import messaging from '@react-native-firebase/messaging';

   async function requestUserPermission() {
     const authStatus = await messaging().requestPermission();
     const enabled =
       authStatus === messaging.AuthorizationStatus.AUTHORIZED ||
       authStatus === messaging.AuthorizationStatus.PROVISIONAL;

     if (enabled) {
       const fcmToken = await messaging().getToken();
       console.log('FCM Token:', fcmToken);
       // Send token to your API
       await registerDeviceToken(fcmToken);
     }
   }

   async function registerDeviceToken(token) {
     await fetch('https://yourapi.com/api/mobile/auth/device-token', {
       method: 'POST',
       headers: {
         'Authorization': `Bearer ${accessToken}`,
         'Content-Type': 'application/json',
       },
       body: JSON.stringify({
         deviceToken: token,
         platform: 1, // Android
         deviceInfo: 'Samsung Galaxy S23'
       })
     });
   }
   ```

   **Flutter:**
   ```dart
   import 'package:firebase_messaging/firebase_messaging.dart';

   Future<void> setupFirebase() async {
     FirebaseMessaging messaging = FirebaseMessaging.instance;
     
     NotificationSettings settings = await messaging.requestPermission(
       alert: true,
       badge: true,
       sound: true,
     );

     if (settings.authorizationStatus == AuthorizationStatus.authorized) {
       String? token = await messaging.getToken();
       print('FCM Token: $token');
       await registerDeviceToken(token!);
     }
   }
   ```

### iOS (React Native / Flutter / Native)

1. **Add Firebase to iOS App**:
   ```bash
   # In Firebase Console
   Project Settings → Add App → iOS
   ```

2. **Download `GoogleService-Info.plist`**:
   - Place in iOS project root in Xcode

3. **Enable Push Notifications**:
   - Xcode → Capabilities → Push Notifications (ON)
   - Xcode → Capabilities → Background Modes → Remote notifications (checked)

4. **Upload APNs Certificate**:
   - Generate APNs certificate in Apple Developer Portal
   - Upload to Firebase Console → Project Settings → Cloud Messaging → APNs

5. **Request Permission & Get Token** (same as Android above)

## Testing Push Notifications

### Method 1: Using Mobile API Endpoint

Send test notification via API:

```http
POST /api/mobile/notifications/test
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "title": "Test Notification",
  "body": "This is a test push notification",
  "imageUrl": "https://example.com/image.jpg"
}
```

### Method 2: Using Firebase Console

1. Go to Firebase Console → Cloud Messaging
2. Click "Send your first message"
3. Enter notification title and text
4. Click "Send test message"
5. Enter FCM token from your device
6. Click "Test"

### Method 3: Using Postman

```http
POST https://fcm.googleapis.com/fcm/send
Content-Type: application/json
Authorization: key=YOUR_SERVER_KEY

{
  "to": "DEVICE_FCM_TOKEN",
  "notification": {
    "title": "Disaster Alert",
    "body": "Flash flood warning in your area",
    "sound": "default"
  },
  "data": {
    "alertId": "123",
    "severity": "high",
    "type": "flood"
  }
}
```

## API Usage Examples

### 1. Register Device Token
Already covered in mobile authentication.

### 2. Subscribe to Disaster Topics

Users can subscribe to specific disaster types:

```javascript
// Subscribe to flood alerts
await fetch('https://yourapi.com/api/mobile/notifications/subscribe', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    topic: 'disaster-flood',
    deviceToken: fcmToken
  })
});

// Available topics:
// - disaster-flood
// - disaster-earthquake
// - disaster-fire
// - disaster-hurricane
// - city-lagos
// - severity-critical
```

### 3. Handle Incoming Notifications

**React Native:**
```javascript
import messaging from '@react-native-firebase/messaging';

// Foreground messages
messaging().onMessage(async remoteMessage => {
  console.log('Notification received:', remoteMessage);
  // Display local notification or update UI
  showLocalNotification(remoteMessage);
});

// Background/Quit state
messaging().setBackgroundMessageHandler(async remoteMessage => {
  console.log('Background notification:', remoteMessage);
});

// Notification tap handler
messaging().onNotificationOpenedApp(remoteMessage => {
  console.log('Notification tapped:', remoteMessage);
  // Navigate to specific screen
  navigation.navigate('AlertDetails', { 
    alertId: remoteMessage.data.alertId 
  });
});
```

**Flutter:**
```dart
FirebaseMessaging.onMessage.listen((RemoteMessage message) {
  print('Got a message whilst in the foreground!');
  print('Message data: ${message.data}');

  if (message.notification != null) {
    print('Message also contained a notification: ${message.notification}');
    // Show local notification
  }
});

FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
  print('Notification tapped!');
  // Navigate to screen
  Navigator.pushNamed(context, '/alert-details', 
    arguments: {'alertId': message.data['alertId']});
});
```

## Notification Types

### 1. Disaster Alert Notification
Automatically sent when new disaster alert is created:

```json
{
  "notification": {
    "title": "⚠️ Flash Flood Warning",
    "body": "Heavy rainfall expected in Lagos. Stay indoors.",
    "imageUrl": "https://api.com/alerts/images/flood-warning.jpg"
  },
  "data": {
    "type": "disaster_alert",
    "alertId": "12345",
    "disasterType": "flood",
    "severity": "high",
    "latitude": "6.5244",
    "longitude": "3.3792"
  }
}
```

### 2. Incident Confirmation
Sent after user reports an incident:

```json
{
  "notification": {
    "title": "Incident Report Received",
    "body": "Your flood report has been submitted. Ref: #INC-12345"
  },
  "data": {
    "type": "incident_confirmation",
    "incidentId": "12345",
    "status": "reported"
  }
}
```

### 3. Shelter Information
Sent when user requests nearby shelter info:

```json
{
  "notification": {
    "title": "Ikeja Central Shelter Available",
    "body": "380 spaces available. 2.5km from your location."
  },
  "data": {
    "type": "shelter_info",
    "shelterId": "67890",
    "latitude": "6.5244",
    "longitude": "3.3792",
    "distance": "2.5"
  }
}
```

## Notification Payload Structure

All notifications follow this structure:

```typescript
interface NotificationPayload {
  notification: {
    title: string;
    body: string;
    imageUrl?: string;
  };
  data: {
    type: 'disaster_alert' | 'incident_confirmation' | 'shelter_info' | 'custom';
    [key: string]: string; // Additional data
  };
  android: {
    priority: 'high' | 'normal';
    notification: {
      sound: string;
      channelId: string;
    };
  };
  apns: {
    headers: {
      'apns-priority': '5' | '10';
    };
    payload: {
      aps: {
        sound: string;
        badge: number;
      };
    };
  };
}
```

## Notification Channels (Android 8.0+)

Create notification channels in your mobile app:

```javascript
// React Native
import PushNotification from 'react-native-push-notification';

PushNotification.createChannel({
  channelId: "disaster-alerts-critical",
  channelName: "Critical Disaster Alerts",
  channelDescription: "Critical emergency notifications",
  importance: 5, // Max
  vibrate: true,
});

PushNotification.createChannel({
  channelId: "disaster-alerts-normal",
  channelName: "Disaster Alerts",
  channelDescription: "General disaster notifications",
  importance: 4, // High
  vibrate: true,
});
```

## Troubleshooting

### Issue: "Firebase not initialized"
**Solution**: Ensure `firebase-adminsdk-credentials.json` exists and path in `appsettings.json` is correct.

### Issue: "Invalid registration token"
**Solution**: Token may have expired. Request new token from mobile device and update.

### Issue: "Authentication error"
**Solution**: Verify service account JSON has correct permissions. Regenerate if needed.

### Issue: Notifications not received on iOS
**Solution**: 
- Check APNs certificate is uploaded to Firebase
- Ensure push notifications capability is enabled
- Verify app is using production/sandbox APNs correctly

### Issue: Notifications not received on Android
**Solution**:
- Check `google-services.json` is in correct location
- Verify Firebase Cloud Messaging is enabled
- Check device has Google Play Services

## Monitoring & Analytics

### View Notification Statistics

```http
GET /api/mobile/notifications/statistics?startDate=2024-11-01&endDate=2024-11-30
Authorization: Bearer {access_token}
```

Response:
```json
{
  "success": true,
  "data": {
    "totalSent": 15420,
    "totalFailed": 45,
    "totalPending": 12,
    "deliveryRate": 99.7
  }
}
```

### Firebase Console Metrics
- Go to Firebase Console → Cloud Messaging
- View delivery rate, open rate, and conversion metrics
- Set up A/B testing for notification content

## Advanced Features

### Conditional Delivery
Send notifications only to users in affected areas:

```csharp
// In your controller
var affectedUsers = await _context.Users
    .Where(u => u.CityId == alertCityId)
    .Select(u => u.Id)
    .ToListAsync();

await _pushNotificationService.SendBulkDisasterAlertsAsync(
    disasterAlert, 
    affectedUsers
);
```

### Scheduled Notifications
Use background job (Hangfire/Quartz) to schedule:

```csharp
BackgroundJob.Schedule(
    () => _pushNotificationService.SendToTopicAsync(
        "disaster-flood",
        "Flood Risk Increased",
        "Heavy rain expected tomorrow"
    ),
    TimeSpan.FromHours(24)
);
```

### Multi-language Notifications
Already implemented - notifications use user's preferred language:

```csharp
await _pushNotificationService.SendDisasterAlertNotificationAsync(
    alert, 
    languageCode: "fr" // French, English, Hausa, etc.
);
```

## Production Checklist

- [ ] Firebase service account credentials stored securely
- [ ] APNs certificate uploaded (iOS)
- [ ] `google-services.json` configured (Android)
- [ ] Device token registration working
- [ ] Test notifications received on physical devices
- [ ] Notification channels configured (Android)
- [ ] Background notification handler working
- [ ] Notification tap navigation working
- [ ] Topic subscriptions working
- [ ] Monitoring and analytics enabled
- [ ] Error handling and retry logic tested
- [ ] Rate limiting configured
- [ ] Credentials added to `.gitignore`

---

**Last Updated**: November 28, 2024  
**Firebase Admin SDK Version**: 3.0.1  
**Support**: support@disastermanagement.com
