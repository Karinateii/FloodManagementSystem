# Mobile API Testing Guide

## Base URL
```
Development: https://localhost:7001/api/mobile
Production: https://yourdomain.com/api/mobile
```

## Authentication Endpoints

### 1. Register New User
```http
POST /api/mobile/auth/register
Content-Type: application/json

{
  "userName": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "phoneNumber": "+1234567890",
  "cityId": 1,
  "lgaId": 1,
  "deviceToken": "firebase_device_token_here",
  "platform": 1,
  "deviceInfo": "iPhone 14 Pro, iOS 17.1"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64_encoded_refresh_token",
    "expiresAt": "2024-12-28T10:30:00Z",
    "user": {
      "id": "user-guid-here",
      "userName": "johndoe",
      "email": "john@example.com",
      "phoneNumber": "+1234567890",
      "cityName": "Lagos",
      "lgaName": "Ikeja",
      "emailConfirmed": false
    }
  },
  "message": null
}
```

### 2. Login
```http
POST /api/mobile/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!",
  "deviceToken": "firebase_device_token_here",
  "platform": 1,
  "deviceInfo": "iPhone 14 Pro, iOS 17.1"
}
```

**Platform Enum:**
- 0 = iOS
- 1 = Android
- 2 = Web

### 3. Refresh Access Token
```http
POST /api/mobile/auth/refresh
Authorization: Bearer {expired_access_token}
Content-Type: application/json

{
  "refreshToken": "your_refresh_token_here"
}
```

### 4. Logout
```http
POST /api/mobile/auth/logout
Authorization: Bearer {access_token}
```

### 5. Update Device Token
```http
POST /api/mobile/auth/device-token
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "deviceToken": "new_firebase_token",
  "platform": 1,
  "deviceInfo": "Samsung Galaxy S23, Android 14"
}
```

## Protected Endpoints (Require Authorization Header)

### 6. Get Dashboard Summary
```http
GET /api/mobile/dashboard
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "activeAlertsCount": 3,
    "recentIncidentsCount": 12,
    "nearbySheltersCount": 8,
    "recentAlerts": [
      {
        "id": 123456,
        "title": "Flash Flood Warning",
        "message": "Heavy rainfall expected in next 6 hours",
        "severity": 2,
        "disasterType": 0,
        "cityName": null,
        "createdAt": "2024-11-28T08:00:00Z",
        "expiresAt": "2024-11-28T20:00:00Z"
      }
    ],
    "userCityName": "Lagos",
    "userLGAName": "Ikeja"
  }
}
```

**Severity Enum:**
- 0 = Low
- 1 = Medium
- 2 = High
- 3 = Critical

**DisasterType Enum:**
- 0 = Flood
- 1 = Earthquake
- 2 = Fire
- 3 = Hurricane
- 4 = Tsunami
- 5 = Tornado
- 6 = Landslide
- 7 = Drought
- 8 = Epidemic
- 9 = Other

### 7. Get Disaster Alerts (Paginated)
```http
GET /api/mobile/alerts?page=1&pageSize=20&severity=2&disasterType=0
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `page` (optional, default: 1)
- `pageSize` (optional, default: 20, max: 100)
- `severity` (optional, 0-3)
- `disasterType` (optional, 0-9)

**Response:**
```json
{
  "success": true,
  "data": {
    "alerts": [...],
    "totalCount": 45,
    "currentPage": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

### 8. Get Alert Details
```http
GET /api/mobile/alerts/123456
Authorization: Bearer {access_token}
```

### 9. Report Incident
```http
POST /api/mobile/incidents/report
Authorization: Bearer {access_token}
Content-Type: multipart/form-data

Form Data:
- title: "Road flooded near市场"
- description: "Water level approximately 1.5 meters"
- disasterType: 0
- severity: 2
- latitude: 6.5244
- longitude: 3.3792
- location: "Ikeja Market, Lagos"
- photo: [file upload]
```

**Response:**
```json
{
  "success": true,
  "data": 987654,
  "message": "Incident reported successfully"
}
```

### 10. Find Nearby Shelters
```http
GET /api/mobile/shelters/nearby?latitude=6.5244&longitude=3.3792&radius=10
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `latitude` (required)
- `longitude` (required)
- `radius` (optional, default: 50 km)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 456789,
      "name": "Ikeja Central Shelter",
      "address": "123 Allen Avenue, Ikeja, Lagos",
      "latitude": 6.5244,
      "longitude": 3.3792,
      "distance": 2.5,
      "capacity": 500,
      "currentOccupancy": 120,
      "facilities": "Medical, Food, Water, Power, Sanitation, Security",
      "contactPhone": "+234-800-SHELTER"
    }
  ]
}
```

### 11. Get IoT Sensors
```http
GET /api/mobile/sensors?cityId=1&sensorType=WaterLevel
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `cityId` (optional)
- `sensorType` (optional: "WaterLevel", "Rainfall", "Weather")

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 111222,
      "name": "Ikeja Water Sensor #1",
      "sensorType": "WaterLevel",
      "status": "Active",
      "latitude": 6.5244,
      "longitude": 3.3792,
      "lastReading": "2024-11-28T09:45:00Z"
    }
  ]
}
```

### 12. Sync Offline Data
```http
POST /api/mobile/sync
Authorization: Bearer {access_token}
Content-Type: application/json

[
  {
    "title": "Offline Incident Report",
    "description": "Reported while offline",
    "disasterType": 0,
    "severity": 1,
    "latitude": 6.5244,
    "longitude": 3.3792,
    "location": "Local area",
    "timestamp": "2024-11-28T07:00:00Z"
  }
]
```

## Push Notification Endpoints

### 13. Send Test Notification
```http
POST /api/mobile/notifications/test
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "title": "Test Push Notification",
  "body": "This is a test message",
  "imageUrl": "https://example.com/image.jpg"
}
```

### 14. Subscribe to Topic
```http
POST /api/mobile/notifications/subscribe
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "deviceToken": "firebase_device_token",
  "topic": "disaster-flood"
}
```

**Available Topics:**
- `disaster-flood` - Flood alerts
- `disaster-earthquake` - Earthquake alerts
- `disaster-fire` - Fire alerts
- `disaster-hurricane` - Hurricane alerts
- `severity-critical` - Critical severity only
- `city-{cityId}` - City-specific alerts

### 15. Unsubscribe from Topic
```http
POST /api/mobile/notifications/unsubscribe
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "deviceToken": "firebase_device_token",
  "topic": "disaster-flood"
}
```

### 16. Get Notification Statistics
```http
GET /api/mobile/notifications/statistics?startDate=2024-11-01&endDate=2024-11-30
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSent": 15420,
    "totalFailed": 45,
    "totalPending": 12
  }
}
```

### 17. Get User's Registered Devices
```http
GET /api/mobile/notifications/devices
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "platform": "Android",
      "deviceInfo": "Samsung Galaxy S23",
      "isActive": true,
      "lastUsed": "2024-11-28T10:30:00Z",
      "registeredAt": "2024-11-20T08:00:00Z"
    }
  ]
}
```

## Error Responses

### Unauthorized (401)
```json
{
  "success": false,
  "data": null,
  "message": "Unauthorized",
  "errorCode": "AUTH_REQUIRED"
}
```

### Validation Error (400)
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed: Email is required",
  "errorCode": "VALIDATION_ERROR"
}
```

### Server Error (500)
```json
{
  "success": false,
  "data": null,
  "message": "An error occurred while processing your request",
  "errorCode": "SERVER_ERROR"
}
```

## Testing with cURL

### Login Example:
```bash
curl -X POST "https://localhost:7001/api/mobile/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePass123!",
    "deviceToken": "test_token",
    "platform": 1
  }'
```

### Get Dashboard (with token):
```bash
curl -X GET "https://localhost:7001/api/mobile/dashboard" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE"
```

## Testing with Postman

1. **Import Collection**: Create a new Postman collection
2. **Set Base URL**: Add variable `{{baseUrl}}` = `https://localhost:7001/api/mobile`
3. **Set Authorization**: After login, save `accessToken` to environment variable
4. **Use Bearer Token**: In Authorization tab, select "Bearer Token" and use `{{accessToken}}`

## Mobile App Integration Notes

### Token Management:
1. Store `accessToken` and `refreshToken` securely (iOS Keychain, Android Keystore)
2. Include `Authorization: Bearer {accessToken}` in all protected API calls
3. Implement automatic token refresh when receiving 401 responses
4. Clear tokens on logout

### Offline Support:
1. Cache recent alerts, shelters, and sensor data locally
2. Queue incident reports when offline
3. Use `/api/mobile/sync` endpoint to upload queued data when connection restored
4. Implement conflict resolution for data synchronization

### Performance Optimization:
1. Use pagination for alerts (default 20 items per page)
2. Implement pull-to-refresh for real-time updates
3. Cache shelter locations for offline maps
4. Compress photo uploads before sending

### Push Notifications:
1. Register device token on login/app launch
2. Update token on each app start (token can change)
3. Handle notification taps to navigate to relevant screens
4. Implement notification permission requests

## Swagger UI

Access interactive API documentation at:
```
https://localhost:7001/api/docs
```

Use the "Authorize" button to test protected endpoints with your JWT token.

---

**Last Updated**: November 28, 2024  
**API Version**: 1.0  
**Support**: support@disastermanagement.com
