using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAvatarAndHearingTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HearingId",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreHearing",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Clients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_HearingId",
                table: "Tasks",
                column: "HearingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Hearings_HearingId",
                table: "Tasks",
                column: "HearingId",
                principalTable: "Hearings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Hearings_HearingId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_HearingId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HearingId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsPreHearing",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Clients");
        }
    }
}
