using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartialIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lists_board_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id",
                schema: "public",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "ix_swimlanes_board_id",
                schema: "public",
                table: "swimlanes",
                column: "board_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_lists_board_id",
                schema: "public",
                table: "lists",
                column: "board_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_lists_swimlane_id",
                schema: "public",
                table: "lists",
                column: "swimlane_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id",
                schema: "public",
                table: "cards",
                column: "board_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id",
                schema: "public",
                table: "cards",
                column: "list_id",
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_swimlanes_board_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropIndex(
                name: "ix_lists_board_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_lists_swimlane_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_cards_board_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_list_id",
                schema: "public",
                table: "cards");

            migrationBuilder.CreateIndex(
                name: "ix_lists_board_id",
                schema: "public",
                table: "lists",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id",
                schema: "public",
                table: "cards",
                column: "board_id");
        }
    }
}
