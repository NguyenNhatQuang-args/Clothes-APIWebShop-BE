using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_Clothes_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 3, 12, 18, 44, 1, 222, DateTimeKind.Utc).AddTicks(2389), new DateTime(2026, 3, 12, 18, 44, 1, 222, DateTimeKind.Utc).AddTicks(2776) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Created_At", "Updated_At" },
                values: new object[] { new DateTime(2026, 3, 12, 18, 44, 1, 221, DateTimeKind.Utc).AddTicks(3146), new DateTime(2026, 3, 12, 18, 44, 1, 221, DateTimeKind.Utc).AddTicks(3521) });
        }
    }
}
