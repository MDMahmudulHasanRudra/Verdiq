using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientPastAffairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientPastAffairs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CourtName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CaseType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FilingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Opponent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    JudgeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Verdict = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActsAndSections = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LawyerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsCriminal = table.Column<bool>(type: "boolean", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChamberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPastAffairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPastAffairs_Chambers_ChamberId",
                        column: x => x.ChamberId,
                        principalTable: "Chambers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientPastAffairs_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientPastAffairs_ChamberId",
                table: "ClientPastAffairs",
                column: "ChamberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPastAffairs_ClientId",
                table: "ClientPastAffairs",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientPastAffairs");
        }
    }
}
