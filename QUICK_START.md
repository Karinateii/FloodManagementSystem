# 🚀 Quick Start Guide - Global Disaster Management System

## Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Twilio Account (for SMS/WhatsApp/Voice - optional but recommended)
- Firebase Account (for push notifications - optional)

## 🔧 Setup Steps

### 1. Configure Database Connection
Open `appsettings.json` and update if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GlobalDisasterManagement;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 2. Configure Email Settings (Optional)
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-password",
  "EnableSsl": true
}
```

**Gmail App Password Setup:**
1. Enable 2-Factor Authentication on your Google Account
2. Go to Google Account → Security → App Passwords
3. Generate a new app password for "Mail"
4. Use the 16-character password in configuration

### 3. Configure Twilio Settings (Optional - for SMS/WhatsApp/Voice)
```json
"TwilioSettings": {
  "AccountSid": "your-twilio-account-sid",
  "AuthToken": "your-twilio-auth-token",
  "PhoneNumber": "+1234567890",
  "WhatsAppNumber": "whatsapp:+1234567890"
}
```

Get your credentials from: https://console.twilio.com

### 4. Restore NuGet Packages
```powershell
dotnet restore
```

### 5. Apply Database Migrations
```powershell
cd FloodManagementSystem
dotnet ef database update
```

This will create the database with all tables including:
- User authentication tables
- Disaster management tables
- IoT sensor tables
- Communication tables (SMS, USSD, WhatsApp, Voice, Push)

### 6. Build the Project
```powershell
dotnet build
```

### 7. Run the Application
```powershell
dotnet run
```

The application will start at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger API: `https://localhost:5001/api/docs`

## 👤 First Time Setup

### Register as Admin
1. Navigate to `/UserAuthentication/Register` or click "Register" on homepage
2. Fill in your details and select an LGA and City
3. Login with your credentials
4. An admin can be created via database or through the admin registration endpoint

### Initial Data Setup

#### Option 1: Through the UI (Recommended)
1. **Create LGAs:** Go to Admin Dashboard → Manage LGAs
   - Examples: Ikeja, Ikorodu, Lagos Island, Lagos Mainland, Surulere
   
2. **Create Cities:** For each LGA, add cities
   - Example cities for each LGA

3. **Upload Training Data:** Admin Panel → Upload CSV
   - Format: City, Month, MaxTemp, MinTemp, MeanTemp, Precipitation, PrecipCover, FloodRisk

#### Option 2: Database Seeding
SMS templates are automatically seeded on first run.

### Test IoT Monitoring (Phase 3 ✅)

1. **Register IoT Sensors via API:**
```bash
# Register Water Level Sensor
curl -X POST https://localhost:5001/api/IoTSensor/water-level/register \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "WL001",
    "name": "Lagos River Station 1",
    "latitude": 6.5244,
    "longitude": 3.3792,
    "address": "Victoria Island, Lagos",
    "waterBodyName": "Lagos Lagoon",
    "waterBodyType": "Lagoon",
    "normalLevel": 2.5,
    "highWaterLevel": 4.0,
    "criticalLevel": 5.5
  }'
```

2. **Send Sensor Readings:**
```bash
curl -X POST https://localhost:5001/api/IoTSensor/WL001/water-level \
  -H "Content-Type: application/json" \
  -d '{ "level": 3.2 }'
```

3. **View Dashboards:**
   - Main IoT Dashboard: `/IoTMonitoring/Index`
   - Water Level Monitoring: `/IoTMonitoring/WaterLevel`
   - Rainfall Monitoring: `/IoTMonitoring/Rainfall`
   - Weather Monitoring: `/IoTMonitoring/Weather`

4. **Real-time Updates:** Open multiple browsers to see SignalR real-time updates!

### Test Multi-Channel Communication (Phase 2 ✅)

#### SMS (Requires Twilio)
```bash
curl -X POST https://localhost:5001/api/Sms/send \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+234XXXXXXXXXX",
    "message": "Test flood alert from GDMS"
  }'
```

#### USSD
Navigate to `/Ussd` to test the USSD menu simulation

#### WhatsApp (Requires Twilio WhatsApp)
```bash
curl -X POST https://localhost:5001/api/WhatsApp/send \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "+234XXXXXXXXXX",
    "message": "Test WhatsApp alert"
  }'
```

### Test Multi-Language Support ✅
Click the language dropdown in the navbar to switch between:
- 🇬🇧 English
- 🇫🇷 French  
- 🇸🇦 Arabic (with RTL support)
- 🇪🇸 Spanish
- 🇵🇹 Portuguese
- 🇳🇬 Hausa
- 🇳🇬 Yoruba
- 🇳🇬 Igbo

## 📁 Project Structure
```
FloodManagementSystem/
├── Controllers/          # MVC Controllers
├── Models/              # Data models
│   ├── Configuration/   # Configuration classes
│   └── DTO/            # Data transfer objects
├── Views/              # Razor views
├── Data/               # Database context
├── AccountRepository/  # Authentication services
├── wwwroot/           # Static files
│   └── Uploads/       # CSV files and ML models
└── Logs/              # Application logs
```

## 🔑 Default Roles
- **admin**: Full system access, can train models and manage data
- **user**: Can view predictions for their registered city/LGA

## 📊 Using the System

### For Admins:
1. **Upload Training Data**: CSV files with historical weather and flood data
2. **Train Models**: System automatically trains ML.NET models
3. **Generate Predictions**: Upload prediction data for future months
4. **Manage Data**: Add/edit LGAs, cities, and users

### For Users:
1. **Register**: Select your LGA and city
2. **View Predictions**: See flood risk predictions for your area
3. **Receive Alerts**: Get email notifications when flood risk is high

## 🐛 Troubleshooting

### Database Connection Issues
```powershell
# Check if LocalDB is installed
sqllocaldb info

# Create a new instance if needed
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### Migration Errors
```powershell
# Delete existing migration
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### Email Not Sending
- Check SMTP credentials
- For Gmail, ensure you're using an App Password
- Check firewall settings for port 587
- Review logs in `Logs/` folder

### File Upload Issues
- Ensure `wwwroot/Uploads/` folder exists
- Check folder permissions (write access needed)
- Verify file size is under 10MB

## 📝 Logging
Application logs are stored in `Logs/floodmanagement-YYYYMMDD.log`

View recent logs:
```powershell
Get-Content Logs/floodmanagement-*.log -Tail 50
```

## 🧪 Testing the ML Model

### Sample Training Data Format:
```csv
City,Month,MaxTemp,MinTemp,MeanTemp,Precipitation,PrecipCover,FloodRisk
Ikeja,1,32.5,22.1,27.3,15.2,45.0,false
Ikeja,2,33.0,23.5,28.2,25.5,60.0,false
Ikeja,7,28.5,20.5,24.5,250.0,95.0,true
Ikeja,8,28.0,21.0,24.5,280.0,98.0,true
```

**Column Definitions:**
- **City**: Name of the city
- **Month**: Month number (1-12)
- **MaxTemp**: Maximum temperature (°C)
- **MinTemp**: Minimum temperature (°C)
- **MeanTemp**: Mean temperature (°C)
- **Precipitation**: Rainfall amount (mm)
- **PrecipCover**: Precipitation coverage (%)
- **FloodRisk**: true/false (historical flood occurrence)

## 🎯 Next Steps
Once the cleanup is complete and you've verified everything works:

1. **Test all features thoroughly**
2. **Gather real historical data** for Lagos areas
3. **Train model with actual data**
4. **Deploy to a test server**
5. **Get feedback from users**
6. **Plan for new features** (see CLEANUP_SUMMARY.md)

## 📞 Need Help?
- Check `CLEANUP_SUMMARY.md` for detailed information about recent changes
- Review application logs in `Logs/` folder
- Ensure all NuGet packages are restored
- Verify .NET 7.0 SDK is installed

## 🎉 You're All Set!
Your Lagos Flood Detection System is now clean, secure, and ready for production use!
