using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StackerBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "youtube_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    channel_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    added_by = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    added_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_youtube_subscriptions", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "youtube_subscriptions");
        }
    }
}
