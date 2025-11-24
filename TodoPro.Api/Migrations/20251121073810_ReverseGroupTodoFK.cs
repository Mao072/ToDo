using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoPro.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReverseGroupTodoFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Groups_GroupId",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_GroupId",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Todos");

            migrationBuilder.AddColumn<int>(
                name: "TodoItemId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TodoItemId",
                table: "Groups",
                column: "TodoItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Todos_TodoItemId",
                table: "Groups",
                column: "TodoItemId",
                principalTable: "Todos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Todos_TodoItemId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_TodoItemId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "TodoItemId",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Todos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Todos_GroupId",
                table: "Todos",
                column: "GroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Groups_GroupId",
                table: "Todos",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
