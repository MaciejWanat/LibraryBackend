using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace libraryBackend.Migrations
{
    public partial class Books_AddEbookPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EbookPath",
                table: "Books",
                defaultValue: @"/ebooks/default.mobi",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EbookPath",
                table: "Books");
        }
    }
}
