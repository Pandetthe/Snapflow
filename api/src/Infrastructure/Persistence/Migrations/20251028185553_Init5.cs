using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "fk_role_claims_asp_net_roles_role_id",
                schema: "public",
                table: "role_claims");

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

            migrationBuilder.DropForeignKey(
                name: "fk_tags_boards_board_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_cards_card_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_asp_net_users_user_id",
                schema: "public",
                table: "user_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_asp_net_roles_role_id",
                schema: "public",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "card_id",
                schema: "public",
                table: "tags",
                newName: "updated_by_id");

            migrationBuilder.RenameIndex(
                name: "ix_tags_card_id",
                schema: "public",
                table: "tags",
                newName: "ix_tags_updated_by_id");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "tags",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "board_id",
                schema: "public",
                table: "tags",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "color",
                schema: "public",
                table: "tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "created_by_id",
                schema: "public",
                table: "tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                schema: "public",
                table: "tags",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deleted_by_id",
                schema: "public",
                table: "tags",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "tags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "public",
                table: "tags",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "swimlanes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "normalized_name",
                schema: "public",
                table: "roles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "public",
                table: "roles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "lists",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "boards",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "public",
                table: "boards",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "card_tag",
                schema: "public",
                columns: table => new
                {
                    cards_id = table.Column<int>(type: "integer", nullable: false),
                    tags_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_tag", x => new { x.cards_id, x.tags_id });
                    table.ForeignKey(
                        name: "fk_card_tag_cards_cards_id",
                        column: x => x.cards_id,
                        principalSchema: "public",
                        principalTable: "cards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_card_tag_tags_tags_id",
                        column: x => x.tags_id,
                        principalSchema: "public",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tags_created_by_id",
                schema: "public",
                table: "tags",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_deleted_by_id",
                schema: "public",
                table: "tags",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_card_tag_tags_id",
                schema: "public",
                table: "card_tag",
                column: "tags_id");

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_created_by_id",
                schema: "public",
                table: "boards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_deleted_by_id",
                schema: "public",
                table: "boards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_boards_users_updated_by_id",
                schema: "public",
                table: "boards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_created_by_id",
                schema: "public",
                table: "cards",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_deleted_by_id",
                schema: "public",
                table: "cards",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_cards_users_updated_by_id",
                schema: "public",
                table: "cards",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_created_by_id",
                schema: "public",
                table: "lists",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_deleted_by_id",
                schema: "public",
                table: "lists",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lists_users_updated_by_id",
                schema: "public",
                table: "lists",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_members_users_user_id",
                schema: "public",
                table: "members",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_role_claims_roles_role_id",
                schema: "public",
                table: "role_claims",
                column: "role_id",
                principalSchema: "public",
                principalTable: "roles",
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

            migrationBuilder.AddForeignKey(
                name: "fk_tags_boards_board_id",
                schema: "public",
                table: "tags",
                column: "board_id",
                principalSchema: "public",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_users_created_by_id",
                schema: "public",
                table: "tags",
                column: "created_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_users_deleted_by_id",
                schema: "public",
                table: "tags",
                column: "deleted_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_tags_users_updated_by_id",
                schema: "public",
                table: "tags",
                column: "updated_by_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "public",
                table: "user_claims",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "public",
                table: "user_roles",
                column: "role_id",
                principalSchema: "public",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "fk_role_claims_roles_role_id",
                schema: "public",
                table: "role_claims");

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

            migrationBuilder.DropForeignKey(
                name: "fk_tags_boards_board_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_users_created_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_users_deleted_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_tags_users_updated_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_users_user_id",
                schema: "public",
                table: "user_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                schema: "public",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "card_tag",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_tags_created_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "ix_tags_deleted_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "tags");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "public",
                table: "tags");

            migrationBuilder.RenameColumn(
                name: "updated_by_id",
                schema: "public",
                table: "tags",
                newName: "card_id");

            migrationBuilder.RenameIndex(
                name: "ix_tags_updated_by_id",
                schema: "public",
                table: "tags",
                newName: "ix_tags_card_id");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "tags",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "board_id",
                schema: "public",
                table: "tags",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "swimlanes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "normalized_name",
                schema: "public",
                table: "roles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                schema: "public",
                table: "roles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "lists",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "public",
                table: "boards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "public",
                table: "boards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldDefaultValue: "");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "public",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

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
                name: "fk_role_claims_asp_net_roles_role_id",
                schema: "public",
                table: "role_claims",
                column: "role_id",
                principalSchema: "public",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "fk_tags_boards_board_id",
                schema: "public",
                table: "tags",
                column: "board_id",
                principalSchema: "public",
                principalTable: "boards",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_tags_cards_card_id",
                schema: "public",
                table: "tags",
                column: "card_id",
                principalSchema: "public",
                principalTable: "cards",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_asp_net_users_user_id",
                schema: "public",
                table: "user_claims",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_asp_net_roles_role_id",
                schema: "public",
                table: "user_roles",
                column: "role_id",
                principalSchema: "public",
                principalTable: "AspNetRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
