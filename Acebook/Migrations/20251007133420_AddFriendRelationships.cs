using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace acebook.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_UserId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_UserId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Friends");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_AccepterId",
                table: "Friends",
                column: "AccepterId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_RequesterId",
                table: "Friends",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_AccepterId",
                table: "Friends",
                column: "AccepterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_RequesterId",
                table: "Friends",
                column: "RequesterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_AccepterId",
                table: "Friends");

            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_RequesterId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_AccepterId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_RequesterId",
                table: "Friends");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Friends",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId",
                table: "Friends",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_UserId",
                table: "Friends",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
