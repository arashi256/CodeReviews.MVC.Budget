using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCSA_Budget.Arashi256.Migrations
{
    /// <inheritdoc />
    public partial class TransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "Transactions",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Transactions",
                newName: "TransactionDate");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Transactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
