using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceCalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoiceCalls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FromPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CallType = table.Column<int>(type: "int", nullable: false),
                    CallSid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    RecordingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordingDuration = table.Column<int>(type: "int", nullable: true),
                    MenuState = table.Column<int>(type: "int", nullable: true),
                    UserInput = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RingingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisasterAlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisasterIncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceCalls_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoiceCalls_DisasterAlerts_DisasterAlertId",
                        column: x => x.DisasterAlertId,
                        principalTable: "DisasterAlerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoiceCalls_DisasterIncidents_DisasterIncidentId",
                        column: x => x.DisasterIncidentId,
                        principalTable: "DisasterIncidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_CallSid",
                table: "VoiceCalls",
                column: "CallSid",
                unique: true,
                filter: "[CallSid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_CreatedAt",
                table: "VoiceCalls",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_DisasterAlertId",
                table: "VoiceCalls",
                column: "DisasterAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_DisasterIncidentId",
                table: "VoiceCalls",
                column: "DisasterIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_Status",
                table: "VoiceCalls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_ToPhoneNumber",
                table: "VoiceCalls",
                column: "ToPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCalls_UserId",
                table: "VoiceCalls",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoiceCalls");
        }
    }
}
