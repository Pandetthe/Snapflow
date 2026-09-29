using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTagTitlePerBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_board_id",
                schema: "public",
                table: "tags");

            migrationBuilder.Sql("""
                UPDATE public.tags AS t
                SET title = left(t.title, 20 - length(t.id::text) - 1) || '-' || t.id
                FROM (
                    SELECT id, row_number() OVER (PARTITION BY board_id, title ORDER BY id) AS position
                    FROM public.tags
                    WHERE is_deleted = false
                ) AS duplicates
                WHERE t.id = duplicates.id
                  AND duplicates.position > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_tags_board_id_title",
                schema: "public",
                table: "tags",
                columns: new[] { "board_id", "title" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tags_board_id_title",
                schema: "public",
                table: "tags");

            migrationBuilder.CreateIndex(
                name: "ix_tags_board_id",
                schema: "public",
                table: "tags",
                column: "board_id");
        }
    }
}
