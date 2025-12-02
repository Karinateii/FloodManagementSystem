using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WhatsAppMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FromPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateParameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageSid = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisasterIncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisasterAlertId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConversationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InReplyToMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WhatsAppMessages_DisasterAlerts_DisasterAlertId",
                        column: x => x.DisasterAlertId,
                        principalTable: "DisasterAlerts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WhatsAppMessages_DisasterIncidents_DisasterIncidentId",
                        column: x => x.DisasterIncidentId,
                        principalTable: "DisasterIncidents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ConversationId",
                table: "WhatsAppMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_CreatedAt",
                table: "WhatsAppMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_DisasterAlertId",
                table: "WhatsAppMessages",
                column: "DisasterAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_DisasterIncidentId",
                table: "WhatsAppMessages",
                column: "DisasterIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_MessageSid",
                table: "WhatsAppMessages",
                column: "MessageSid",
                unique: true,
                filter: "[MessageSid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_Status",
                table: "WhatsAppMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_ToPhoneNumber",
                table: "WhatsAppMessages",
                column: "ToPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppMessages_UserId",
                table: "WhatsAppMessages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppMessages");
        }
    }
}
