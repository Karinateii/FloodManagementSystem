using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUssdSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UssdSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    UserInput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContextData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: true),
                    LGAId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UssdSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UssdSessions_IsActive",
                table: "UssdSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UssdSessions_LastActivityAt",
                table: "UssdSessions",
                column: "LastActivityAt");

            migrationBuilder.CreateIndex(
                name: "IX_UssdSessions_PhoneNumber",
                table: "UssdSessions",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_UssdSessions_SessionId",
                table: "UssdSessions",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UssdSessions");
        }
    }
}
