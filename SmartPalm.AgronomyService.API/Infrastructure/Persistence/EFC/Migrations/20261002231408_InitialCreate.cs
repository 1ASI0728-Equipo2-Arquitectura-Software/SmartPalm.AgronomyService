using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.AgronomyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agronomy");

            migrationBuilder.CreateTable(
                name: "agronomist_affiliation_projections",
                schema: "agronomy",
                columns: table => new
                {
                    affiliation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agronomist_id = table.Column<long>(type: "bigint", nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agronomist_affiliation_projections", x => x.affiliation_id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "agronomy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "agronomy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plantation_projections",
                schema: "agronomy",
                columns: table => new
                {
                    plantation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    palm_grower_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plantation_projections", x => x.plantation_id);
                });

            migrationBuilder.CreateTable(
                name: "sector_projections",
                schema: "agronomy",
                columns: table => new
                {
                    sector_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sector_projections", x => x.sector_id);
                });

            migrationBuilder.CreateTable(
                name: "technical_reports",
                schema: "agronomy",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    agronomist_id = table.Column<long>(type: "bigint", nullable: false),
                    sector_id = table.Column<long>(type: "bigint", nullable: false),
                    intervention_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    findings = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_projections",
                schema: "agronomy",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    has_active_subscription = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_projections", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "recommendations",
                schema: "agronomy",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sector_id = table.Column<long>(type: "bigint", nullable: true),
                    report_id = table.Column<long>(type: "bigint", nullable: true),
                    agronomist_id = table.Column<long>(type: "bigint", nullable: false),
                    content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recommendations", x => x.id);
                    table.ForeignKey(
                        name: "fk_recommendations_technical_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "agronomy",
                        principalTable: "technical_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agronomist_affiliation_projections_agronomist_id_plantation",
                schema: "agronomy",
                table: "agronomist_affiliation_projections",
                columns: new[] { "agronomist_id", "plantation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "agronomy",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_recommendations_agronomist_id_created_at_utc",
                schema: "agronomy",
                table: "recommendations",
                columns: new[] { "agronomist_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_recommendations_report_id",
                schema: "agronomy",
                table: "recommendations",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "ix_recommendations_sector_id_status",
                schema: "agronomy",
                table: "recommendations",
                columns: new[] { "sector_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_sector_projections_plantation_id",
                schema: "agronomy",
                table: "sector_projections",
                column: "plantation_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_reports_intervention_id",
                schema: "agronomy",
                table: "technical_reports",
                column: "intervention_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_reports_sector_id_created_at_utc",
                schema: "agronomy",
                table: "technical_reports",
                columns: new[] { "sector_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agronomist_affiliation_projections",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "plantation_projections",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "recommendations",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "sector_projections",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "user_projections",
                schema: "agronomy");

            migrationBuilder.DropTable(
                name: "technical_reports",
                schema: "agronomy");
        }
    }
}
