using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RabbitRPC.States.EntityFrameworkCore.Migrations
{
    public partial class CreateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string keyType, valueType, versionType;

            if(migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                (keyType, valueType, versionType) = ("TEXT", "BLOB", "INTEGER");
            }
            else if(migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                (keyType, valueType, versionType) = ("NVARCHAR(450)", "VARBINARY(MAX)", "BIGINT");
            }
            else if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.PostgreSQL")
            {
                (keyType, valueType, versionType) = ("TEXT", "BYTEA", "BIGINT");
            }
            else if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.MySql")
            {
                (keyType, valueType, versionType) = ("NVARCHAR(450)", "MEDIUMBLOB", "BIGINT");
            }
            else
            {
                throw new NotSupportedException($"Migration for '{migrationBuilder.ActiveProvider}' is not supported by RabbitRPC.States.EntityFrameworkCore yet.");
            }

            migrationBuilder.CreateTable(
                name: "StateEntry",
                columns: table => new
                {
                    Key = table.Column<string>(type: keyType, nullable: false),
                    Value = table.Column<byte[]>(type: valueType, nullable: false),
                    Version = table.Column<long>(type: versionType, rowVersion: true, nullable: false)
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
