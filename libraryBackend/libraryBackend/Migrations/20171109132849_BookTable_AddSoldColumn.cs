using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace libraryBackend.Migrations
{
    public partial class BookTable_AddSoldColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sold",
                table: "Book",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sold",
                table: "Book");
        }
    }
}
