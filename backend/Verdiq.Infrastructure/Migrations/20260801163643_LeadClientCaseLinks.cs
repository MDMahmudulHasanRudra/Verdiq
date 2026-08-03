using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LeadClientCaseLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CaseId",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Leads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CaseId",
                table: "Leads",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ClientId",
                table: "Leads",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Cases_CaseId",
                table: "Leads",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Clients_ClientId",
                table: "Leads",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Cases_CaseId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Clients_ClientId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CaseId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_ClientId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Leads");
        }
    }
}
