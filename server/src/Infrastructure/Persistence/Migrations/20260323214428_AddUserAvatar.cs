using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snapflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_content_type",
                schema: "public",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "avatar_data",
                schema: "public",
                table: "users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "avatar_type",
                schema: "public",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                schema: "public",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_content_type",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_data",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_type",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                schema: "public",
                table: "users");
        }
    }
}
