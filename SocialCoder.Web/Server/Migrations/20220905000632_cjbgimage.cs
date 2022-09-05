using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialCoder.Web.Server.Migrations
{
    public partial class cjbgimage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "CodeJamTopics",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "CodeJamTopics");
        }
    }
}
