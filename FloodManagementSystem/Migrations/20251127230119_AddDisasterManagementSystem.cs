using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddDisasterManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CsvFilesCities_CsvFiles_CsvFileId",
                table: "CsvFilesCities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CsvFilesCities",
                table: "CsvFilesCities");

            migrationBuilder.RenameTable(
                name: "CsvFilesCities",
                newName: "CsvFileCities");

            migrationBuilder.RenameColumn(
                name: "LGA",
                table: "AspNetUsers",
                newName: "LGAName");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "AspNetUsers",
                newName: "CityName");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CityId",
                table: "AspNetUsers",
                newName: "IX_Users_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_CsvFilesCities_CsvFileId",
                table: "CsvFileCities",
                newName: "IX_CsvFileCities_CsvFileId");

            migrationBuilder.AlterColumn<string>(
                name: "LGAName",
                table: "LGAs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "CsvFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CsvFiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "CityPredictions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Prediction",
                table: "CityPredictions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Month",
                table: "CityPredictions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CsvFileCities",
                table: "CsvFileCities",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DisasterIncidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisasterType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    DisasterSpecificData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoadBlocked = table.Column<bool>(type: "bit", nullable: false),
                    PowerOutage = table.Column<bool>(type: "bit", nullable: false),
                    StructuralDamage = table.Column<bool>(type: "bit", nullable: false),
                    PeopleTrapped = table.Column<bool>(type: "bit", nullable: false),
                    CasualtiesReported = table.Column<bool>(type: "bit", nullable: false),
                    EnvironmentalHazard = table.Column<bool>(type: "bit", nullable: false),
                    AffectedPeople = table.Column<int>(type: "int", nullable: true),
                    PhotoUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReporterId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReporterPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyServicesNotified = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyResponseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedResponders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisasterIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisasterIncidents_AspNetUsers_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DisasterIncidents_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DisasterIncidents_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "DisasterResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    MinimumThreshold = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    ManagedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiresRefrigeration = table.Column<bool>(type: "bit", nullable: false),
                    IsPerishable = table.Column<bool>(type: "bit", nullable: false),
                    AllocatedToShelterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllocatedToIncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllocationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastInventoryCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisasterResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisasterResources_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ServiceType = table.Column<int>(type: "int", nullable: false),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondaryPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyHotline = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Is24Hours = table.Column<bool>(type: "bit", nullable: false),
                    OperatingHours = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyShelters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    TotalCapacity = table.Column<int>(type: "int", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false),
                    HasMedicalFacility = table.Column<bool>(type: "bit", nullable: false),
                    HasFood = table.Column<bool>(type: "bit", nullable: false),
                    HasWater = table.Column<bool>(type: "bit", nullable: false),
                    HasPower = table.Column<bool>(type: "bit", nullable: false),
                    HasSanitation = table.Column<bool>(type: "bit", nullable: false),
                    HasSecurity = table.Column<bool>(type: "bit", nullable: false),
                    IsAccessible = table.Column<bool>(type: "bit", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManagerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SpecialNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyShelters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyShelters_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmergencyShelters_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "EvacuationRoutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    RouteCoordinates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartLatitude = table.Column<double>(type: "float", nullable: false),
                    StartLongitude = table.Column<double>(type: "float", nullable: false),
                    EndLatitude = table.Column<double>(type: "float", nullable: false),
                    EndLongitude = table.Column<double>(type: "float", nullable: false),
                    StartAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EndAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DistanceKm = table.Column<double>(type: "float", nullable: false),
                    EstimatedTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    MinimumElevationMeters = table.Column<double>(type: "float", nullable: true),
                    MaximumElevationMeters = table.Column<double>(type: "float", nullable: true),
                    HasLighting = table.Column<bool>(type: "bit", nullable: false),
                    HasShelters = table.Column<bool>(type: "bit", nullable: false),
                    IsAccessible = table.Column<bool>(type: "bit", nullable: false),
                    LinkedShelterIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Warnings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastVerified = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvacuationRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvacuationRoutes_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvacuationRoutes_LGAs_LGAId",
                        column: x => x.LGAId,
                        principalTable: "LGAs",
                        principalColumn: "LGAId");
                });

            migrationBuilder.CreateTable(
                name: "DisasterAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisasterType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AffectedCountries = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedRegions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedCities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedLGAs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinLatitude = table.Column<double>(type: "float", nullable: true),
                    MaxLatitude = table.Column<double>(type: "float", nullable: true),
                    MinLongitude = table.Column<double>(type: "float", nullable: true),
                    MaxLongitude = table.Column<double>(type: "float", nullable: true),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsOfficial = table.Column<bool>(type: "bit", nullable: false),
                    RecommendedActions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvacuationInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SafetyTips = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    ShareCount = table.Column<int>(type: "int", nullable: false),
                    AcknowledgmentCount = table.Column<int>(type: "int", nullable: false),
                    RelatedIncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedAlertIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisasterAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisasterAlerts_DisasterIncidents_RelatedIncidentId",
                        column: x => x.RelatedIncidentId,
                        principalTable: "DisasterIncidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShelterCheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShelterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    FamilyMembers = table.Column<int>(type: "int", nullable: false),
                    HasMedicalNeeds = table.Column<bool>(type: "bit", nullable: false),
                    MedicalConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NeedsSpecialAssistance = table.Column<bool>(type: "bit", nullable: false),
                    SpecialRequirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCheckedOut = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EvacuatedFromAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShelterCheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShelterCheckIns_EmergencyShelters_ShelterId",
                        column: x => x.ShelterId,
                        principalTable: "EmergencyShelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LGAs_Name",
                table: "LGAs",
                column: "LGAName");

            migrationBuilder.CreateIndex(
                name: "IX_CsvFiles_UploadDateTime",
                table: "CsvFiles",
                column: "UploadDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_CityPredictions_CityId_Year_Month",
                table: "CityPredictions",
                columns: new[] { "CityId", "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_CityPredictions_Prediction",
                table: "CityPredictions",
                column: "Prediction");

            migrationBuilder.CreateIndex(
                name: "IX_CityPredictions_Year",
                table: "CityPredictions",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LGAId",
                table: "AspNetUsers",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterAlerts_DisasterType",
                table: "DisasterAlerts",
                column: "DisasterType");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterAlerts_IssuedAt",
                table: "DisasterAlerts",
                column: "IssuedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterAlerts_RelatedIncidentId",
                table: "DisasterAlerts",
                column: "RelatedIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterAlerts_Status",
                table: "DisasterAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_CityId_Status",
                table: "DisasterIncidents",
                columns: new[] { "CityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_DisasterType",
                table: "DisasterIncidents",
                column: "DisasterType");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_DisasterType_Status",
                table: "DisasterIncidents",
                columns: new[] { "DisasterType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_LGAId",
                table: "DisasterIncidents",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_ReportedAt",
                table: "DisasterIncidents",
                column: "ReportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_ReporterId",
                table: "DisasterIncidents",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_Severity",
                table: "DisasterIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterIncidents_Status",
                table: "DisasterIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterResources_CityId",
                table: "DisasterResources",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterResources_Status",
                table: "DisasterResources",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterResources_Type",
                table: "DisasterResources",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DisasterResources_Type_Status",
                table: "DisasterResources",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_IsActive",
                table: "EmergencyContacts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_ServiceType",
                table: "EmergencyContacts",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyShelters_CityId_IsActive",
                table: "EmergencyShelters",
                columns: new[] { "CityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyShelters_IsActive",
                table: "EmergencyShelters",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyShelters_LGAId",
                table: "EmergencyShelters",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRoutes_CityId_Status",
                table: "EvacuationRoutes",
                columns: new[] { "CityId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRoutes_LGAId",
                table: "EvacuationRoutes",
                column: "LGAId");

            migrationBuilder.CreateIndex(
                name: "IX_EvacuationRoutes_Status",
                table: "EvacuationRoutes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterCheckIns_IsCheckedOut",
                table: "ShelterCheckIns",
                column: "IsCheckedOut");

            migrationBuilder.CreateIndex(
                name: "IX_ShelterCheckIns_ShelterId",
                table: "ShelterCheckIns",
                column: "ShelterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CsvFileCities_CsvFiles_CsvFileId",
                table: "CsvFileCities",
                column: "CsvFileId",
                principalTable: "CsvFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CsvFileCities_CsvFiles_CsvFileId",
                table: "CsvFileCities");

            migrationBuilder.DropTable(
                name: "DisasterAlerts");

            migrationBuilder.DropTable(
                name: "DisasterResources");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "EvacuationRoutes");

            migrationBuilder.DropTable(
                name: "ShelterCheckIns");

            migrationBuilder.DropTable(
                name: "DisasterIncidents");

            migrationBuilder.DropTable(
                name: "EmergencyShelters");

            migrationBuilder.DropIndex(
                name: "IX_LGAs_Name",
                table: "LGAs");

            migrationBuilder.DropIndex(
                name: "IX_CsvFiles_UploadDateTime",
                table: "CsvFiles");

            migrationBuilder.DropIndex(
                name: "IX_CityPredictions_CityId_Year_Month",
                table: "CityPredictions");

            migrationBuilder.DropIndex(
                name: "IX_CityPredictions_Prediction",
                table: "CityPredictions");

            migrationBuilder.DropIndex(
                name: "IX_CityPredictions_Year",
                table: "CityPredictions");

            migrationBuilder.DropIndex(
                name: "IX_Cities_Name",
                table: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_Users_LGAId",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CsvFileCities",
                table: "CsvFileCities");

            migrationBuilder.RenameTable(
                name: "CsvFileCities",
                newName: "CsvFilesCities");

            migrationBuilder.RenameColumn(
                name: "LGAName",
                table: "AspNetUsers",
                newName: "LGA");

            migrationBuilder.RenameColumn(
                name: "CityName",
                table: "AspNetUsers",
                newName: "City");

            migrationBuilder.RenameIndex(
                name: "IX_Users_CityId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_CsvFileCities_CsvFileId",
                table: "CsvFilesCities",
                newName: "IX_CsvFilesCities_CsvFileId");

            migrationBuilder.AlterColumn<string>(
                name: "LGAName",
                table: "LGAs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "CsvFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CsvFiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Year",
                table: "CityPredictions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Prediction",
                table: "CityPredictions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Month",
                table: "CityPredictions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CsvFilesCities",
                table: "CsvFilesCities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CsvFilesCities_CsvFiles_CsvFileId",
                table: "CsvFilesCities",
                column: "CsvFileId",
                principalTable: "CsvFiles",
                principalColumn: "Id");
        }
    }
}
