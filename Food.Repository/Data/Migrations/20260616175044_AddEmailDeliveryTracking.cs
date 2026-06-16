using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Food.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDeliveryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Emails",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "Emails");
        }
    }
}
