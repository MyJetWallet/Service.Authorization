using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Authorization.Postgres.Migrations
{
    public partial class PinRecordIssue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "authorization",
                table: "pinrecord",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "HasPinIssue",
                schema: "authorization",
                table: "pinrecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "pinrecord_session_issue",
                schema: "authorization",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RootSessionId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TotalFailAttempts = table.Column<int>(type: "integer", nullable: false),
                    CurrentFailAttempts = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pinrecord_session_issue", x => new { x.ClientId, x.RootSessionId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_pinrecord_session_issue_ClientId_IsActive",
                schema: "authorization",
                table: "pinrecord_session_issue",
                columns: new[] { "ClientId", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pinrecord_session_issue",
                schema: "authorization");

            migrationBuilder.DropColumn(
                name: "HasPinIssue",
                schema: "authorization",
                table: "pinrecord");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                schema: "authorization",
                table: "pinrecord",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);
        }
    }
}
