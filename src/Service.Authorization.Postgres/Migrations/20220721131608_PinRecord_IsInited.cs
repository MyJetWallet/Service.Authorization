using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.Authorization.Postgres.Migrations
{
    public partial class PinRecord_IsInited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInited",
                schema: "authorization",
                table: "pinrecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInited",
                schema: "authorization",
                table: "pinrecord");
        }
    }
}
