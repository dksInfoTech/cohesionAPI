using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Product.Dal.Migrations
{
    /// <inheritdoc />
    public partial class Release1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ticker",
                table: "Entity",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntityHierarchy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    EntityHierarchyInfo = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ChildIds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityHierarchy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialExtractJob",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    SubmittedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialExtractJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialExtractJob_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinCodeGroup",
                columns: table => new
                {
                    GroupCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GroupTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FinStatementType = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Style = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinCodeGroup", x => x.GroupCode);
                });

            migrationBuilder.CreateTable(
                name: "SourceEntity",
                columns: table => new
                {
                    SourceId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TickerAndExchCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LongCompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    L1Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    L2Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    L3Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    L4Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CountryOfDomicile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NoOfEmployees = table.Column<int>(type: "integer", nullable: true),
                    CurrentMktCap = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    PeRatio = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ParentTicker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ParentLongName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UltParentTicker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UltParentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UltParentLongName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    GlobalCompanyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LeiName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompanyCorpTicker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyLegalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompParentRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LeiUltimateParent = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LatestFiling = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceEntity", x => x.SourceId);
                    table.UniqueConstraint("AK_SourceEntity_Ticker", x => x.Ticker);
                });

            migrationBuilder.CreateTable(
                name: "SourceFinancialCode",
                columns: table => new
                {
                    FinCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFinancialCode", x => x.FinCode);
                });

            migrationBuilder.CreateTable(
                name: "FinancialExtractJobStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExtractJobId = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialExtractJobStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialExtractJobStatus_FinancialExtractJob_ExtractJobId",
                        column: x => x.ExtractJobId,
                        principalTable: "FinancialExtractJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ExtractJobId = table.Column<int>(type: "integer", nullable: false),
                    FinancialYear = table.Column<int>(type: "integer", nullable: false),
                    FinancialType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatement_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialStatement_FinancialExtractJob_ExtractJobId",
                        column: x => x.ExtractJobId,
                        principalTable: "FinancialExtractJob",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinCode",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FinTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GroupCode = table.Column<string>(type: "character varying(100)", nullable: false),
                    FinStatementType = table.Column<string>(type: "text", nullable: false),
                    IsCalc = table.Column<bool>(type: "boolean", nullable: false),
                    Formula = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Style = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinCode", x => x.Code);
                    table.ForeignKey(
                        name: "FK_FinCode_FinCodeGroup_GroupCode",
                        column: x => x.GroupCode,
                        principalTable: "FinCodeGroup",
                        principalColumn: "GroupCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceRating",
                columns: table => new
                {
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BBRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BBRatingDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    BBRatingPrior = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MoodysRating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MoodysRatingDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    MoodysOutlook = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MoodysOutlookDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceRating", x => x.Ticker);
                    table.ForeignKey(
                        name: "FK_SourceRating_SourceEntity_Ticker",
                        column: x => x.Ticker,
                        principalTable: "SourceEntity",
                        principalColumn: "Ticker",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceFinancial",
                columns: table => new
                {
                    Ticker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EqyFundYear = table.Column<int>(type: "integer", nullable: false),
                    FinCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Scaling = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    FinValue = table.Column<decimal>(type: "numeric(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFinancial", x => new { x.Ticker, x.EqyFundYear, x.FinCode });
                    table.ForeignKey(
                        name: "FK_SourceFinancial_SourceEntity_Ticker",
                        column: x => x.Ticker,
                        principalTable: "SourceEntity",
                        principalColumn: "Ticker",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SourceFinancial_SourceFinancialCode_FinCode",
                        column: x => x.FinCode,
                        principalTable: "SourceFinancialCode",
                        principalColumn: "FinCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FinancialStatementId = table.Column<int>(type: "integer", nullable: false),
                    FinCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FinValue = table.Column<decimal>(type: "numeric(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Financial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Financial_FinCode_FinCode",
                        column: x => x.FinCode,
                        principalTable: "FinCode",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Financial_FinancialStatement_FinancialStatementId",
                        column: x => x.FinancialStatementId,
                        principalTable: "FinancialStatement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Financial_FinancialStatementId",
                table: "Financial",
                column: "FinancialStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Financial_FinCode",
                table: "Financial",
                column: "FinCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialExtractJob_EntityId",
                table: "FinancialExtractJob",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialExtractJob_JobId",
                table: "FinancialExtractJob",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialExtractJobStatus_ExtractJobId",
                table: "FinancialExtractJobStatus",
                column: "ExtractJobId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatement_EntityId",
                table: "FinancialStatement",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatement_ExtractJobId",
                table: "FinancialStatement",
                column: "ExtractJobId");

            migrationBuilder.CreateIndex(
                name: "IX_FinCode_GroupCode",
                table: "FinCode",
                column: "GroupCode");

            migrationBuilder.CreateIndex(
                name: "IX_SourceEntity_Ticker",
                table: "SourceEntity",
                column: "Ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceFinancial_FinCode",
                table: "SourceFinancial",
                column: "FinCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityHierarchy");

            migrationBuilder.DropTable(
                name: "Financial");

            migrationBuilder.DropTable(
                name: "FinancialExtractJobStatus");

            migrationBuilder.DropTable(
                name: "SourceFinancial");

            migrationBuilder.DropTable(
                name: "SourceRating");

            migrationBuilder.DropTable(
                name: "FinCode");

            migrationBuilder.DropTable(
                name: "FinancialStatement");

            migrationBuilder.DropTable(
                name: "SourceFinancialCode");

            migrationBuilder.DropTable(
                name: "SourceEntity");

            migrationBuilder.DropTable(
                name: "FinCodeGroup");

            migrationBuilder.DropTable(
                name: "FinancialExtractJob");

            migrationBuilder.DropColumn(
                name: "Ticker",
                table: "Entity");
        }
    }
}
