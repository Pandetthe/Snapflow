using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_boards_users_created_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_boards_users_deleted_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_boards_users_updated_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_users_created_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_users_deleted_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_users_updated_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_users_created_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_users_deleted_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_users_updated_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_members_users_user_id",
                schema: "public",
                table: "members");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_users_created_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_users_deleted_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_users_updated_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.AddForeignKey(
                name: "fk_boards_asp_net_users_created_by_id",
                schema: "public",
                table: "boards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_asp_net_users_deleted_by_id",
                schema: "public",
                table: "boards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_asp_net_users_updated_by_id",
                schema: "public",
                table: "boards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_asp_net_users_created_by_id",
                schema: "public",
                table: "cards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_asp_net_users_deleted_by_id",
                schema: "public",
                table: "cards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_asp_net_users_updated_by_id",
                schema: "public",
                table: "cards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_asp_net_users_created_by_id",
                schema: "public",
                table: "lists",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_asp_net_users_deleted_by_id",
                schema: "public",
                table: "lists",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_asp_net_users_updated_by_id",
                schema: "public",
                table: "lists",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_members_asp_net_users_user_id",
                schema: "public",
                table: "members",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_asp_net_users_created_by_id",
                schema: "public",
                table: "swimlanes",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_asp_net_users_deleted_by_id",
                schema: "public",
                table: "swimlanes",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_asp_net_users_updated_by_id",
                schema: "public",
                table: "swimlanes",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_boards_asp_net_users_created_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_boards_asp_net_users_deleted_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_boards_asp_net_users_updated_by_id",
                schema: "public",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_asp_net_users_created_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_asp_net_users_deleted_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_cards_asp_net_users_updated_by_id",
                schema: "public",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_asp_net_users_created_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_asp_net_users_deleted_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_lists_asp_net_users_updated_by_id",
                schema: "public",
                table: "lists");

            migrationBuilder.DropForeignKey(
                name: "fk_members_asp_net_users_user_id",
                schema: "public",
                table: "members");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_asp_net_users_created_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_asp_net_users_deleted_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.DropForeignKey(
                name: "fk_swimlanes_asp_net_users_updated_by_id",
                schema: "public",
                table: "swimlanes");

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_created_by_id",
                schema: "public",
                table: "boards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_deleted_by_id",
                schema: "public",
                table: "boards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_updated_by_id",
                schema: "public",
                table: "boards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_created_by_id",
                schema: "public",
                table: "cards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_deleted_by_id",
                schema: "public",
                table: "cards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_updated_by_id",
                schema: "public",
                table: "cards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_created_by_id",
                schema: "public",
                table: "lists",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_deleted_by_id",
                schema: "public",
                table: "lists",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_updated_by_id",
                schema: "public",
                table: "lists",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_members_users_user_id",
                schema: "public",
                table: "members",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_users_created_by_id",
                schema: "public",
                table: "swimlanes",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_users_deleted_by_id",
                schema: "public",
                table: "swimlanes",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_swimlanes_users_updated_by_id",
                schema: "public",
                table: "swimlanes",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
