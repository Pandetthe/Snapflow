using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_swimlanes_board_id_rank",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropIndex(
                name: "ix_lists_swimlane_id_rank",
                schema: "public",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_cards_list_id_rank",
                schema: "public",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "ix_swimlanes_board_id",
                schema: "public",
                table: "swimlanes",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_lists_swimlane_id",
                schema: "public",
                table: "lists",
                column: "swimlane_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id",
                schema: "public",
                table: "cards",
                column: "list_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_swimlanes_board_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropIndex(
                name: "ix_lists_swimlane_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_cards_list_id",
                schema: "public",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "ix_swimlanes_board_id_rank",
                schema: "public",
                table: "swimlanes",
                columns: new[] { "board_id", "rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lists_swimlane_id_rank",
                schema: "public",
                table: "lists",
                columns: new[] { "swimlane_id", "rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id_rank",
                schema: "public",
                table: "cards",
                columns: new[] { "list_id", "rank" },
                unique: true);
        }
    }
}
