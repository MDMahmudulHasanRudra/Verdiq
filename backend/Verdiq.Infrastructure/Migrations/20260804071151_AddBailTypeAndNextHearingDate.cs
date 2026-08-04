using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBailTypeAndNextHearingDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BailType",
                table: "Bails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextHearingDate",
                table: "Bails",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BailType",
                table: "Bails");

            migrationBuilder.DropColumn(
                name: "NextHearingDate",
                table: "Bails");
        }
    }
}
