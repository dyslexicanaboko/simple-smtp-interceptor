using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSmtpInterceptor.Data.Migrations
{
    public partial class AddedLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                schema: "dbo",
                columns: table => new
                {
                    LogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MachineName = table.Column<string>(maxLength: 200, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "getutcdate()"),
                    Level = table.Column<string>(maxLength: 5, nullable: false),
                    Message = table.Column<string>(nullable: false),
                    Logger = table.Column<string>(maxLength: 300, nullable: true),
                    Properties = table.Column<string>(nullable: true),
                    Callsite = table.Column<string>(maxLength: 300, nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Log_LogId", x => x.LogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log",
                schema: "dbo");
        }
    }
}
