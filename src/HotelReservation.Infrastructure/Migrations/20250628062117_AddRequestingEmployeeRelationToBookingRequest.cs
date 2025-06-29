using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestingEmployeeRelationToBookingRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NationalCode",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_NationalCode",
                table: "Users",
                column: "NationalCode");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_RequestingEmployeeNationalCode",
                table: "BookingRequests",
                column: "RequestingEmployeeNationalCode");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRequests_Users_RequestingEmployeeNationalCode",
                table: "BookingRequests",
                column: "RequestingEmployeeNationalCode",
                principalTable: "Users",
                principalColumn: "NationalCode",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRequests_Users_RequestingEmployeeNationalCode",
                table: "BookingRequests");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_NationalCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_BookingRequests_RequestingEmployeeNationalCode",
                table: "BookingRequests");

            migrationBuilder.AlterColumn<string>(
                name: "NationalCode",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
