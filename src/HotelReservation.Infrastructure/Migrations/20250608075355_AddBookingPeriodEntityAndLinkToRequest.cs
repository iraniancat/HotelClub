using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPeriodEntityAndLinkToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingPeriod",
                table: "BookingRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingPeriodId",
                table: "BookingRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BookingPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingPeriods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_BookingPeriodId",
                table: "BookingRequests",
                column: "BookingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPeriods_Name",
                table: "BookingPeriods",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRequests_BookingPeriods_BookingPeriodId",
                table: "BookingRequests",
                column: "BookingPeriodId",
                principalTable: "BookingPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRequests_BookingPeriods_BookingPeriodId",
                table: "BookingRequests");

            migrationBuilder.DropTable(
                name: "BookingPeriods");

            migrationBuilder.DropIndex(
                name: "IX_BookingRequests_BookingPeriodId",
                table: "BookingRequests");

            migrationBuilder.DropColumn(
                name: "BookingPeriodId",
                table: "BookingRequests");

            migrationBuilder.AddColumn<string>(
                name: "BookingPeriod",
                table: "BookingRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
