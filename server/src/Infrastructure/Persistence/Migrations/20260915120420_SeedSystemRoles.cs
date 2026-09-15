using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "roles",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[] { 1, "5b0f4f7e-3c1d-4b8a-9a6e-2f1c7d9e8a01", "Admin", "ADMIN" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "roles",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
