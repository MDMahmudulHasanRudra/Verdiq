using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Verdiq.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CasePhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasePhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CasePhotos_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CasePhotos_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Judgments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Result = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JudgmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextHearingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KeyFindings = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    RecordedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judgments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Judgments_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Judgments_Users_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChamberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_Chambers_ChamberId",
                        column: x => x.ChamberId,
                        principalTable: "Chambers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workflows_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WorkflowDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseWorkflows_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseWorkflows_Users_StartedById",
                        column: x => x.StartedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseWorkflows_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    DueInDays = table.Column<int>(type: "integer", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowSteps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseWorkflowSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseWorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    DueInDays = table.Column<int>(type: "integer", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseWorkflowSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowSteps_CaseWorkflows_CaseWorkflowId",
                        column: x => x.CaseWorkflowId,
                        principalTable: "CaseWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseWorkflowSteps_Users_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CasePhotos_CaseId",
                table: "CasePhotos",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CasePhotos_UploadedById",
                table: "CasePhotos",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflows_CaseId",
                table: "CaseWorkflows",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflows_StartedById",
                table: "CaseWorkflows",
                column: "StartedById");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflows_WorkflowId",
                table: "CaseWorkflows",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowSteps_CaseWorkflowId",
                table: "CaseWorkflowSteps",
                column: "CaseWorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseWorkflowSteps_CompletedById",
                table: "CaseWorkflowSteps",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Judgments_CaseId",
                table: "Judgments",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Judgments_RecordedById",
                table: "Judgments",
                column: "RecordedById");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ChamberId",
                table: "Workflows",
                column: "ChamberId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_CreatedById",
                table: "Workflows",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowSteps_WorkflowId",
                table: "WorkflowSteps",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CasePhotos");

            migrationBuilder.DropTable(
                name: "CaseWorkflowSteps");

            migrationBuilder.DropTable(
                name: "Judgments");

            migrationBuilder.DropTable(
                name: "WorkflowSteps");

            migrationBuilder.DropTable(
                name: "CaseWorkflows");

            migrationBuilder.DropTable(
                name: "Workflows");
        }
    }
}
