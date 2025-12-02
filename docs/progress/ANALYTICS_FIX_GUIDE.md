# Analytics Service - Model Property Fix Guide

## Overview
The AnalyticsService.cs has property mismatches with the actual database models. This guide provides all the necessary corrections.

## Quick Fix Instructions

### Required Model Property Mappings

Based on the actual models in your system, here are the correct property names:

#### DisasterAlert Model
- ❌ `ResolvedAt` → Use `UpdatedAt` or check for `Status == AlertStatus.Cancelled` or `AlertStatus.Expired`
- ❌ `AlertStatus.Resolved` → Use `AlertStatus.Expired` or `AlertStatus.Cancelled`
- ✅ `Status` → Already correct
- ✅ `IssuedAt` → Already correct

#### DisasterIncident Model
- ❌ `RespondedAt` → Use `EmergencyResponseTime`
- ❌ `IncidentStatus.Pending` → Use `IncidentStatus.Reported`
- ❌ `IncidentStatus.UnderInvestigation` → Use `IncidentStatus.InProgress` or similar
- ❌ `AffectedPeopleCount` → Use `AffectedPeople`
- ❌ `EvacuatedPeopleCount` → This property doesn't exist, skip or set to 0
- ✅ `ReportedAt` → Already correct
- ✅ `ResolvedAt` → Already correct

#### City Model
- ❌ `CityName` → Use `Name`

#### EmergencyShelter Model
- ❌ `ShelterId` → Use `Id`
- ❌ `ShelterName` → Use `Name`
- Need to check actual model for correct property names

#### User Model
- ❌ `LastLoginDate` → May not exist, use alternative tracking
- ❌ `CreatedDate` → Check if this exists or use registration date

#### VoiceCall Model
- ❌ `InitiatedAt` → Use `CreatedAt` or `CallTime`

#### IoTSensor DbSet
- ❌ `_context.IoTSensors` → Check DisasterDbContext for correct DbSet name (might be `IoTSensorData` or similar)

#### Notification Models
- ❌ `DeliveryStatus` property on SmsNotification → Use `Status`
- ❌ `DeliveryStatus` property on PushNotification → Use `Status`
- ❌ `DeliveryStatus` property on WhatsAppMessage → Use `Status`
- ❌ `NotificationDeliveryStatus` enum → Check actual enum name in your models
- ❌ `VoiceCall.InitiatedAt` → Use `CreatedAt` or similar

#### DisasterAlert
- ❌ `TriggeredBySensor` → This property doesn't exist, remove or add logic

## Alternative Approach: Simplified Analytics

Since there are many model mismatches, here are two options:

### Option 1: Fix All Properties (Recommended)
You need to check each model file and update the AnalyticsService.cs accordingly. This will take time but provides accurate analytics.

### Option 2: Simplified Version (Quick Fix)
Use only properties that definitely exist and simplify calculations. Here's a simplified version:

```csharp
// Simplified Alert Metrics
kpi.TotalAlerts = alerts.Count;
kpi.ActiveAlerts = alerts.Count(a => a.Status == AlertStatus.Active);
kpi.ResolvedAlerts = alerts.Count(a => a.Status == AlertStatus.Expired || a.Status == AlertStatus.Cancelled);
kpi.AlertEffectivenessRate = kpi.TotalAlerts > 0 ? (double)kpi.ResolvedAlerts / kpi.TotalAlerts * 100 : 0;

// Simplified Incident Metrics  
kpi.TotalIncidents = incidents.Count;
kpi.PendingIncidents = incidents.Count(i => i.Status == IncidentStatus.Reported);
kpi.ResolvedIncidents = incidents.Count(i => i.ResolvedAt.HasValue);

// For response time - use EmergencyResponseTime if available
var respondedIncidents = incidents.Where(i => i.EmergencyResponseTime.HasValue);
if (respondedIncidents.Any())
{
    kpi.AverageResponseTime = respondedIncidents
        .Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes);
}
```

## Specific Fixes Needed

### 1. Line 43-45: Alert Status
```csharp
// OLD:
kpi.ResolvedAlerts = alerts.Count(a => a.Status == AlertStatus.Resolved);
var resolvedAlerts = alerts.Where(a => a.Status == AlertStatus.Resolved && a.ResolvedAt.HasValue);

// NEW:
kpi.ResolvedAlerts = alerts.Count(a => a.Status == AlertStatus.Expired || a.Status == AlertStatus.Cancelled);
var resolvedAlerts = alerts.Where(a => (a.Status == AlertStatus.Expired || a.Status == AlertStatus.Cancelled) && a.UpdatedAt.HasValue);
```

### 2. Line 62-68: Incident Status
```csharp
// OLD:
kpi.PendingIncidents = incidents.Count(i => i.Status == IncidentStatus.Pending || i.Status == IncidentStatus.UnderInvestigation);
var respondedIncidents = incidents.Where(i => i.RespondedAt.HasValue);

// NEW:
kpi.PendingIncidents = incidents.Count(i => i.Status == IncidentStatus.Reported);
var respondedIncidents = incidents.Where(i => i.EmergencyResponseTime.HasValue);
if (respondedIncidents.Any())
{
    kpi.AverageResponseTime = respondedIncidents
        .Average(i => (i.EmergencyResponseTime!.Value - i.ReportedAt).TotalMinutes);
}
```

### 3. Line 91: IoT Sensors DbSet
```csharp
// Check your DisasterDbContext.cs for the correct DbSet name
// It might be one of these:
var sensors = await _context.IoTSensorData.ToListAsync();
// OR
var sensors = await _context.Sensors.ToListAsync();
// OR check the actual DbSet name in DisasterDbContext.cs
```

### 4. Line 161-162: Affected People
```csharp
// OLD:
AffectedPeople = incidents.Sum(i => i.AffectedPeopleCount ?? 0),
EvacuatedPeople = incidents.Sum(i => i.EvacuatedPeopleCount ?? 0),

// NEW:
AffectedPeople = incidents.Sum(i => i.AffectedPeople ?? 0),
EvacuatedPeople = 0, // This property doesn't exist in the model
```

### 5. Line 170: City Name
```csharp
// OLD:
.GroupBy(i => new { i.City?.CityName, i.Latitude, i.Longitude })

// NEW:
.GroupBy(i => new { i.City?.Name, i.Latitude, i.Longitude })
```

### 6. Notification Status Properties
```csharp
// For SMS, WhatsApp, Push, Voice - check each model for the correct Status property
// Replace all DeliveryStatus with Status
// Replace NotificationDeliveryStatus enum with the actual enum in your models
```

## Recommended Approach

1. **Check DisasterDbContext.cs** to see all DbSet property names
2. **Check each model file** to verify property names
3. **Update AnalyticsService.cs** with correct property names
4. **Test each analytics method** individually
5. **Add null checks** where properties might not exist

## Need Help?

If you want me to fix the AnalyticsService.cs, please:
1. Share the complete model files for: DisasterAlert, DisasterIncident, EmergencyShelter, User, VoiceCall, IoTSensor
2. Share the DisasterDbContext.cs to see DbSet names
3. I'll create a corrected version of the AnalyticsService.cs

## Temporary Solution

Comment out methods with errors and focus on the dashboard view first:
- Keep Dashboard KPIs with simplified properties
- Comment out advanced analytics until models are confirmed
- This allows the system to compile and you can gradually add analytics as you verify models

---

**Created**: November 28, 2024  
**Status**: Action Required - Model Property Verification Needed
