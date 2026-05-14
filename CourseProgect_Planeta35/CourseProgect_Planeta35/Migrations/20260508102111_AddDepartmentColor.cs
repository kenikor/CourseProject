using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseProgect_Planeta35.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Departments");
        }
    }
}
