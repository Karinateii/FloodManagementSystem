using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlobalDisasterManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixSmsNotificationForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsNotifications_DisasterAlerts_DisasterAlertId1",
                table: "SmsNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsNotifications_DisasterIncidents_DisasterIncidentId1",
                table: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_SmsNotifications_DisasterAlertId1",
                table: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_SmsNotifications_DisasterIncidentId1",
                table: "SmsNotifications");

            migrationBuilder.DropColumn(
                name: "DisasterAlertId1",
                table: "SmsNotifications");

            migrationBuilder.DropColumn(
                name: "DisasterIncidentId1",
                table: "SmsNotifications");

            // Drop the existing int columns and recreate as Guid
            migrationBuilder.DropColumn(
                name: "DisasterAlertId",
                table: "SmsNotifications");

            migrationBuilder.DropColumn(
                name: "DisasterIncidentId",
                table: "SmsNotifications");

            migrationBuilder.AddColumn<Guid>(
                name: "DisasterAlertId",
                table: "SmsNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisasterIncidentId",
                table: "SmsNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterAlertId",
                table: "SmsNotifications",
                column: "DisasterAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterIncidentId",
                table: "SmsNotifications",
                column: "DisasterIncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsNotifications_DisasterAlerts_DisasterAlertId",
                table: "SmsNotifications",
                column: "DisasterAlertId",
                principalTable: "DisasterAlerts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SmsNotifications_DisasterIncidents_DisasterIncidentId",
                table: "SmsNotifications",
                column: "DisasterIncidentId",
                principalTable: "DisasterIncidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SmsNotifications_DisasterAlerts_DisasterAlertId",
                table: "SmsNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SmsNotifications_DisasterIncidents_DisasterIncidentId",
                table: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_SmsNotifications_DisasterAlertId",
                table: "SmsNotifications");

            migrationBuilder.DropIndex(
                name: "IX_SmsNotifications_DisasterIncidentId",
                table: "SmsNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "DisasterIncidentId",
                table: "SmsNotifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisasterAlertId",
                table: "SmsNotifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisasterAlertId1",
                table: "SmsNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisasterIncidentId1",
                table: "SmsNotifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterAlertId1",
                table: "SmsNotifications",
                column: "DisasterAlertId1");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNotifications_DisasterIncidentId1",
                table: "SmsNotifications",
                column: "DisasterIncidentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsNotifications_DisasterAlerts_DisasterAlertId1",
                table: "SmsNotifications",
                column: "DisasterAlertId1",
                principalTable: "DisasterAlerts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SmsNotifications_DisasterIncidents_DisasterIncidentId1",
                table: "SmsNotifications",
                column: "DisasterIncidentId1",
                principalTable: "DisasterIncidents",
                principalColumn: "Id");
        }
    }
}
