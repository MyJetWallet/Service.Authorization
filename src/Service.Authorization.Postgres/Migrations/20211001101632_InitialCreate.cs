using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Service.Authorization.Postgres.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authorization");

            migrationBuilder.CreateTable(
                name: "authcredentials",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Salt = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Brand = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authcredentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "authlogs",
                schema: "authorization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TraderId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Ip = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DateTime = table.Column<DateTime>(type: "timestamp without time zone", maxLength: 128, nullable: false),
                    Location = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authlogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_authcredentials_Id",
                schema: "authorization",
                table: "authcredentials",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_authlogs_TraderId",
                schema: "authorization",
                table: "authlogs",
                column: "TraderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "authcredentials",
                schema: "authorization");

            migrationBuilder.DropTable(
                name: "authlogs",
                schema: "authorization");
        }
    }
}
