using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWidthAndHeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "height",
                schema: "public",
                table: "swimlanes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "width",
                schema: "public",
                table: "lists",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "height",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropColumn(
                name: "width",
                schema: "public",
                table: "lists");
        }
    }
}
