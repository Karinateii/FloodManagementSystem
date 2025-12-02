using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIoTSensorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TopicSubscriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetPlatform = table.Column<int>(type: "int", nullable: true),
                    DeviceTokens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    DisasterAlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisasterIncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FcmResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotifications_DisasterAlerts_DisasterAlertId",
                        column: x => x.DisasterAlertId,
                        principalTable: "DisasterAlerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PushNotifications_DisasterIncidents_DisasterIncidentId",
                        column: x => x.DisasterIncidentId,
                        principalTable: "DisasterIncidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RainfallSensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LightRainThreshold = table.Column<double>(type: "float", nullable: false),
                    ModerateRainThreshold = table.Column<double>(type: "float", nullable: false),
                    HeavyRainThreshold = table.Column<double>(type: "float", nullable: false),
                    VeryHeavyRainThreshold = table.Column<double>(type: "float", nullable: false),
                    CollectionArea = table.Column<double>(type: "float", nullable: false),
                    Resolution = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeasurementIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    CurrentRainfall = table.Column<double>(type: "float", nullable: true),
                    HourlyRainfall = table.Column<double>(type: "float", nullable: true),
                    DailyRainfall = table.Column<double>(type: "float", nullable: true),
                    CurrentReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentIntensity = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SensorType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCommunicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastDataReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatteryLevel = table.Column<int>(type: "int", nullable: true),
                    PowerSource = table.Column<int>(type: "int", nullable: false),
                    AlertsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AlertRecipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RainfallSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RainfallSensors_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RainfallSensors_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "WaterLevelSensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NormalLevel = table.Column<double>(type: "float", nullable: false),
                    WarningLevel = table.Column<double>(type: "float", nullable: false),
                    DangerLevel = table.Column<double>(type: "float", nullable: false),
                    CriticalLevel = table.Column<double>(type: "float", nullable: false),
                    MinMeasurableLevel = table.Column<double>(type: "float", nullable: false),
                    MaxMeasurableLevel = table.Column<double>(type: "float", nullable: false),
                    Accuracy = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WaterBodyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WaterBodyType = table.Column<int>(type: "int", nullable: true),
                    CurrentLevel = table.Column<double>(type: "float", nullable: true),
                    CurrentReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentStatus = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SensorType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCommunicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastDataReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatteryLevel = table.Column<int>(type: "int", nullable: true),
                    PowerSource = table.Column<int>(type: "int", nullable: false),
                    AlertsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AlertRecipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterLevelSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterLevelSensors_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WaterLevelSensors_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "WeatherSensors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasTemperatureSensor = table.Column<bool>(type: "bit", nullable: false),
                    HasHumiditySensor = table.Column<bool>(type: "bit", nullable: false),
                    HasPressureSensor = table.Column<bool>(type: "bit", nullable: false),
                    HasWindSensor = table.Column<bool>(type: "bit", nullable: false),
                    HasRainGauge = table.Column<bool>(type: "bit", nullable: false),
                    CurrentTemperature = table.Column<double>(type: "float", nullable: true),
                    CurrentHumidity = table.Column<double>(type: "float", nullable: true),
                    CurrentPressure = table.Column<double>(type: "float", nullable: true),
                    CurrentWindSpeed = table.Column<double>(type: "float", nullable: true),
                    CurrentWindDirection = table.Column<double>(type: "float", nullable: true),
                    CurrentReadingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SensorType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCommunicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastDataReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BatteryLevel = table.Column<int>(type: "int", nullable: true),
                    PowerSource = table.Column<int>(type: "int", nullable: false),
                    AlertsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AlertRecipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherSensors_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WeatherSensors_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "RainfallReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rainfall = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Intensity = table.Column<int>(type: "int", nullable: false),
                    HourlyCumulative = table.Column<double>(type: "float", nullable: true),
                    DailyCumulative = table.Column<double>(type: "float", nullable: true),
                    MonthlyCumulative = table.Column<double>(type: "float", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AlertTriggered = table.Column<bool>(type: "bit", nullable: false),
                    AlertTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RainfallReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RainfallReadings_RainfallSensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "RainfallSensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WaterLevelReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RateOfChange = table.Column<double>(type: "float", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    IsCalibrated = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AlertTriggered = table.Column<bool>(type: "bit", nullable: false),
                    AlertTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterLevelReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterLevelReadings_WaterLevelSensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "WaterLevelSensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: true),
                    FeelsLike = table.Column<double>(type: "float", nullable: true),
                    DewPoint = table.Column<double>(type: "float", nullable: true),
                    Humidity = table.Column<double>(type: "float", nullable: true),
                    Pressure = table.Column<double>(type: "float", nullable: true),
                    SeaLevelPressure = table.Column<double>(type: "float", nullable: true),
                    WindSpeed = table.Column<double>(type: "float", nullable: true),
                    WindGust = table.Column<double>(type: "float", nullable: true),
                    WindDirection = table.Column<double>(type: "float", nullable: true),
                    WindDirectionCardinal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Rainfall = table.Column<double>(type: "float", nullable: true),
                    Visibility = table.Column<double>(type: "float", nullable: true),
                    CloudCover = table.Column<int>(type: "int", nullable: true),
                    UVIndex = table.Column<double>(type: "float", nullable: true),
                    SolarRadiation = table.Column<double>(type: "float", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Condition = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherReadings_WeatherSensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "WeatherSensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_IsActive",
                table: "DeviceTokens",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_LastUsedAt",
                table: "DeviceTokens",
                column: "LastUsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_Platform",
                table: "DeviceTokens",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_Token",
                table: "DeviceTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId",
                table: "DeviceTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_CreatedAt",
                table: "PushNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_DisasterAlertId",
                table: "PushNotifications",
                column: "DisasterAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_DisasterIncidentId",
                table: "PushNotifications",
                column: "DisasterIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_Status",
                table: "PushNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_Topic",
                table: "PushNotifications",
                column: "Topic");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallReadings_SensorId",
                table: "RainfallReadings",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallReadings_SensorId_Timestamp",
                table: "RainfallReadings",
                columns: new[] { "SensorId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RainfallReadings_Timestamp",
                table: "RainfallReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallSensors_CityId_Status",
                table: "RainfallSensors",
                columns: new[] { "CityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RainfallSensors_DeviceId",
                table: "RainfallSensors",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RainfallSensors_LGAId",
                table: "RainfallSensors",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_RainfallSensors_Status",
                table: "RainfallSensors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelReadings_SensorId",
                table: "WaterLevelReadings",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelReadings_SensorId_Timestamp",
                table: "WaterLevelReadings",
                columns: new[] { "SensorId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelReadings_Timestamp",
                table: "WaterLevelReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelSensors_CityId_Status",
                table: "WaterLevelSensors",
                columns: new[] { "CityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelSensors_DeviceId",
                table: "WaterLevelSensors",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelSensors_LGAId",
                table: "WaterLevelSensors",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterLevelSensors_Status",
                table: "WaterLevelSensors",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_SensorId",
                table: "WeatherReadings",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_SensorId_Timestamp",
                table: "WeatherReadings",
                columns: new[] { "SensorId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherReadings_Timestamp",
                table: "WeatherReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSensors_CityId_Status",
                table: "WeatherSensors",
                columns: new[] { "CityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSensors_DeviceId",
                table: "WeatherSensors",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSensors_LGAId",
                table: "WeatherSensors",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherSensors_Status",
                table: "WeatherSensors",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTokens");

            migrationBuilder.DropTable(
                name: "PushNotifications");

            migrationBuilder.DropTable(
                name: "RainfallReadings");

            migrationBuilder.DropTable(
                name: "WaterLevelReadings");

            migrationBuilder.DropTable(
                name: "WeatherReadings");

            migrationBuilder.DropTable(
                name: "RainfallSensors");

            migrationBuilder.DropTable(
                name: "WaterLevelSensors");

            migrationBuilder.DropTable(
                name: "WeatherSensors");
        }
    }
}
