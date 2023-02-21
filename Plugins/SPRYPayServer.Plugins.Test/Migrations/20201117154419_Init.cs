using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SPRYPayServer.Plugins.Test.Migrations
{
    [DbContext(typeof(TestPluginDbContext))]
    [Migration("20201117154419_Init")]
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SPRYPayServer.Plugins.Test");

            migrationBuilder.CreateTable(
                name: "TestPluginRecords",
                schema: "SPRYPayServer.Plugins.Test",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPluginRecords", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestPluginRecords",
                schema: "SPRYPayServer.Plugins.Test");
        }
    }
}
