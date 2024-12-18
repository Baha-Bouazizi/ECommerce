using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommerceElectronique.Migrations
{
    /// <inheritdoc />
    public partial class addIsProcessed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "CartItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "CartItems");
        }
    }
}
