using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TwilioMessageSid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisasterIncidentId = table.Column<int>(type: "int", nullable: true),
                    DisasterIncidentId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisasterAlertId = table.Column<int>(type: "int", nullable: true),
                    DisasterAlertId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsNotifications_DisasterAlerts_DisasterAlertId1",
                        column: x => x.DisasterAlertId1,
                        principalTable: "DisasterAlerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SmsNotifications_DisasterIncidents_DisasterIncidentId1",
                        column: x => x.DisasterIncidentId1,
                        principalTable: "DisasterIncidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TemplateText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterAlertId1",
                table: "SmsNotifications",
                column: "DisasterAlertId1");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterIncidentId1",
                table: "SmsNotifications",
                column: "DisasterIncidentId1");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_PhoneNumber",
                table: "SmsNotifications",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_SentAt",
                table: "SmsNotifications",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_Status",
                table: "SmsNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_UserId",
                table: "SmsNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Language_Type",
                table: "SmsTemplates",
                columns: new[] { "LanguageCode", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Type",
                table: "SmsTemplates",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsNotifications");

            migrationBuilder.DropTable(
                name: "SmsTemplates");
        }
    }
}
