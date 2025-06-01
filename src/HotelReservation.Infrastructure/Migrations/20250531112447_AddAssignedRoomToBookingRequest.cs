using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedRoomToBookingRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedRoomId",
                table: "BookingRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_AssignedRoomId",
                table: "BookingRequests",
                column: "AssignedRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRequests_Rooms_AssignedRoomId",
                table: "BookingRequests",
                column: "AssignedRoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRequests_Rooms_AssignedRoomId",
                table: "BookingRequests");

            migrationBuilder.DropIndex(
                name: "IX_BookingRequests_AssignedRoomId",
                table: "BookingRequests");

            migrationBuilder.DropColumn(
                name: "AssignedRoomId",
                table: "BookingRequests");
        }
    }
}
