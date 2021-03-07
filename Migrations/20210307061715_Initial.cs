using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FezBotRedux.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NeoBet",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    msgID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BetName = table.Column<string>(type: "TEXT", nullable: true),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    open = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeoBet", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "NeoHubSettings",
                columns: table => new
                {
                    Identitiy = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MsgId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeoHubSettings", x => x.Identitiy);
                });

            migrationBuilder.CreateTable(
                name: "Playings",
                columns: table => new
                {
                    Identitiy = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playings", x => x.Identitiy);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsBlockled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Cash = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Identity);
                });

            migrationBuilder.CreateTable(
                name: "Bet",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BetName = table.Column<string>(type: "TEXT", nullable: true),
                    BetRate = table.Column<double>(type: "REAL", nullable: false),
                    NeoBetIdentity = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bet", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Bet_NeoBet_NeoBetIdentity",
                        column: x => x.NeoBetIdentity,
                        principalTable: "NeoBet",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Prefix = table.Column<string>(type: "TEXT", nullable: true),
                    SettingsIdentity = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Guilds_Settings_SettingsIdentity",
                        column: x => x.SettingsIdentity,
                        principalTable: "Settings",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Afks",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserIdentity = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Afks", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Afks_Users_UserIdentity",
                        column: x => x.UserIdentity,
                        principalTable: "Users",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserIdentity = table.Column<int>(type: "INTEGER", nullable: true),
                    Creation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Blacklist_Users_UserIdentity",
                        column: x => x.UserIdentity,
                        principalTable: "Users",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NeoBets",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserIdentity = table.Column<int>(type: "INTEGER", nullable: true),
                    BetAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    BetLoc = table.Column<int>(type: "INTEGER", nullable: false),
                    NeoBetIdentity = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeoBets", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_NeoBets_NeoBet_NeoBetIdentity",
                        column: x => x.NeoBetIdentity,
                        principalTable: "NeoBet",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NeoBets_Users_UserIdentity",
                        column: x => x.UserIdentity,
                        principalTable: "Users",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Identity = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerIdentity = table.Column<int>(type: "INTEGER", nullable: true),
                    GuildIdentity = table.Column<int>(type: "INTEGER", nullable: true),
                    Creation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Uses = table.Column<int>(type: "INTEGER", nullable: false),
                    Trigger = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    IfAttachment = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Identity);
                    table.ForeignKey(
                        name: "FK_Tags_Guilds_GuildIdentity",
                        column: x => x.GuildIdentity,
                        principalTable: "Guilds",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tags_Users_OwnerIdentity",
                        column: x => x.OwnerIdentity,
                        principalTable: "Users",
                        principalColumn: "Identity",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Afks_UserIdentity",
                table: "Afks",
                column: "UserIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Bet_NeoBetIdentity",
                table: "Bet",
                column: "NeoBetIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Blacklist_UserIdentity",
                table: "Blacklist",
                column: "UserIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_SettingsIdentity",
                table: "Guilds",
                column: "SettingsIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_NeoBets_NeoBetIdentity",
                table: "NeoBets",
                column: "NeoBetIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_NeoBets_UserIdentity",
                table: "NeoBets",
                column: "UserIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildIdentity",
                table: "Tags",
                column: "GuildIdentity");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerIdentity",
                table: "Tags",
                column: "OwnerIdentity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Afks");

            migrationBuilder.DropTable(
                name: "Bet");

            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "NeoBets");

            migrationBuilder.DropTable(
                name: "NeoHubSettings");

            migrationBuilder.DropTable(
                name: "Playings");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "NeoBet");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
