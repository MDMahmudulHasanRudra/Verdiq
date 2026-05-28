using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowTemplateId",
                table: "Cases",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkflowTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChamberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplates_Chambers_ChamberId",
                        column: x => x.ChamberId,
                        principalTable: "Chambers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTemplateSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalSectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplateSections_LegalSections_LegalSectionId",
                        column: x => x.LegalSectionId,
                        principalTable: "LegalSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTemplateSections_WorkflowTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WorkflowTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt" },
                values: new object[] { new Guid("f0000000-0000-0000-0000-000000000030"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manage workflow templates", false, "Workflow", "workflow.manage", null });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_WorkflowTemplateId",
                table: "Cases",
                column: "WorkflowTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplates_ChamberId",
                table: "WorkflowTemplates",
                column: "ChamberId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplateSections_LegalSectionId",
                table: "WorkflowTemplateSections",
                column: "LegalSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTemplateSections_TemplateId_LegalSectionId",
                table: "WorkflowTemplateSections",
                columns: new[] { "TemplateId", "LegalSectionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_WorkflowTemplates_WorkflowTemplateId",
                table: "Cases",
                column: "WorkflowTemplateId",
                principalTable: "WorkflowTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_WorkflowTemplates_WorkflowTemplateId",
                table: "Cases");

            migrationBuilder.DropTable(
                name: "WorkflowTemplateSections");

            migrationBuilder.DropTable(
                name: "WorkflowTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Cases_WorkflowTemplateId",
                table: "Cases");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f0000000-0000-0000-0000-000000000030"));

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateId",
                table: "Cases");
        }
    }
}
