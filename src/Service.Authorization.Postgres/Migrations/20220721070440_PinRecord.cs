using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Authorization.Postgres.Migrations
{
    public partial class PinRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "authorization",
                table: "authlogs",
                type: "timestamp with time zone",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldMaxLength: 256);

            migrationBuilder.CreateTable(
                name: "pinrecord",
                schema: "authorization",
                columns: table => new
                {
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Salt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pinrecord", x => x.ClientId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pinrecord",
                schema: "authorization");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "authorization",
                table: "authlogs",
                type: "timestamp without time zone",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldMaxLength: 256);
        }
    }
}
