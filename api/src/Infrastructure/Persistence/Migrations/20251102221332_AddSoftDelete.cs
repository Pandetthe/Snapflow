using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "deleted_by_cascade",
                schema: "public",
                table: "swimlanes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "deleted_by_cascade",
                schema: "public",
                table: "lists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "deleted_by_cascade",
                schema: "public",
                table: "cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_by_cascade",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropColumn(
                name: "deleted_by_cascade",
                schema: "public",
                table: "lists");

            migrationBuilder.DropColumn(
                name: "deleted_by_cascade",
                schema: "public",
                table: "cards");
        }
    }
}
