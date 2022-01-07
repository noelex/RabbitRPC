using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RabbitRPC.States.EntityFrameworkCore.Migrations
{
    [DbContext(typeof(StateDbContext))]
    [Migration("20211230014345_CreateDb")]
    public partial class CreateDb : Migration
    {
        internal static StateTableColumnTypes? CustomColumnTypes { get; set; }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var (keyType, valueType, versionType) = GetDbTypes(migrationBuilder.ActiveProvider);

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

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.13");

            modelBuilder.Entity("RabbitRPC.States.EntityFramework.Models.StateEntry", b =>
            {
                b.Property<string>("Key")
                    .HasColumnType("TEXT");

                b.Property<byte[]>("Value")
                    .IsRequired()
                    .HasColumnType("BLOB");

                b.Property<long>("Version")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnType("INTEGER");

                b.HasKey("Key");

                b.ToTable("StateEntry");
            });
#pragma warning restore 612, 618
        }

        private static (string keyType, string valueType, string versionType) GetDbTypes(string provider)
        {
            if (CustomColumnTypes != null)
            {
                return (CustomColumnTypes.KeyColumnType, CustomColumnTypes.ValueColumnType, CustomColumnTypes.VersionColumnType);
            }
            else if (provider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                return ("TEXT", "BLOB", "INTEGER");
            }
            else if (provider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                return ("NVARCHAR(450)", "VARBINARY(MAX)", "BIGINT");
            }
            else if (provider == "Microsoft.EntityFrameworkCore.PostgreSQL")
            {
                return ("TEXT", "BYTEA", "BIGINT");
            }
            else if (provider == "Microsoft.EntityFrameworkCore.MySql")
            {
                return ("NVARCHAR(450)", "MEDIUMBLOB", "BIGINT");
            }
            else
            {
                throw new NotSupportedException($"Migration for '{provider}' is not supported by RabbitRPC.States.EntityFrameworkCore yet.");
            }
        }
    }
}
