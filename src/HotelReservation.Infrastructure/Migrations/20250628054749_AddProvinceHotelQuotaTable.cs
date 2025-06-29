using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceHotelQuotaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvinceHotelQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomLimit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceHotelQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProvinceHotelQuotas_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProvinceHotelQuotas_Provinces_ProvinceCode",
                        column: x => x.ProvinceCode,
                        principalTable: "Provinces",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvinceHotelQuotas_HotelId",
                table: "ProvinceHotelQuotas",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProvinceHotelQuotas_ProvinceCode_HotelId",
                table: "ProvinceHotelQuotas",
                columns: new[] { "ProvinceCode", "HotelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvinceHotelQuotas");
        }
    }
}
