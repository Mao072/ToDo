using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoPro.Api.Migrations
{
    /// <inheritdoc />
    public partial class _1121 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCount",
                table: "Todos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserCount",
                table: "Todos");
        }
    }
}
