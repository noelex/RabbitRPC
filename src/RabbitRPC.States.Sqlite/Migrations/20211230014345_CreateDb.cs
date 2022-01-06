using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RabbitRPC.States.Sqlite.Migrations
{
    public partial class CreateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StateEntry",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateEntry", x => x.Key);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StateEntry");
        }
    }
}
