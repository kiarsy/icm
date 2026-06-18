using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ICMarkets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blockchain",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BlockchainIdentifier = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Height = table.Column<long>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LatestUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousHash = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PeerCount = table.Column<long>(type: "INTEGER", nullable: false),
                    UnconfirmedCount = table.Column<long>(type: "INTEGER", nullable: false),
                    LastForkHeight = table.Column<long>(type: "INTEGER", nullable: true),
                    LastForkHash = table.Column<string>(type: "TEXT", nullable: true),
                    HighFeePerKb = table.Column<long>(type: "INTEGER", nullable: true),
                    MediumFeePerKb = table.Column<long>(type: "INTEGER", nullable: true),
                    LowFeePerKb = table.Column<long>(type: "INTEGER", nullable: true),
                    HighGasPrice = table.Column<long>(type: "INTEGER", nullable: true),
                    MediumGasPrice = table.Column<long>(type: "INTEGER", nullable: true),
                    LowGasPrice = table.Column<long>(type: "INTEGER", nullable: true),
                    HighPriorityFee = table.Column<long>(type: "INTEGER", nullable: true),
                    MediumPriorityFee = table.Column<long>(type: "INTEGER", nullable: true),
                    LowPriorityFee = table.Column<long>(type: "INTEGER", nullable: true),
                    BaseFee = table.Column<long>(type: "INTEGER", nullable: true),
                    RawJson = table.Column<string>(type: "TEXT", nullable: false),
                    Revision = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blockchain", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blockchain_BlockchainIdentifier",
                table: "blockchain",
                column: "BlockchainIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_EventId_Version",
                table: "events",
                columns: new[] { "EventId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blockchain");

            migrationBuilder.DropTable(
                name: "events");
        }
    }
}
