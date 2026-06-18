using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICMarkets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_events_EventId_Version",
                table: "events");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_events_EventId_OccurredAt",
                table: "events",
                columns: new[] { "EventId", "OccurredAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_events_EventId_OccurredAt",
                table: "events");

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "events",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_events_EventId_Version",
                table: "events",
                columns: new[] { "EventId", "Version" },
                unique: true);
        }
    }
}
