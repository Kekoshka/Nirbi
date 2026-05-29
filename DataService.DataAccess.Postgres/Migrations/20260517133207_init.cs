using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataService.DataAccess.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileCollectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoredFiles_FileCollections_FileCollectionId",
                        column: x => x.FileCollectionId,
                        principalTable: "FileCollections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileCollectionOwner",
                columns: table => new
                {
                    FileCollectionsId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileCollectionOwner", x => new { x.FileCollectionsId, x.OwnersId });
                    table.ForeignKey(
                        name: "FK_FileCollectionOwner_FileCollections_FileCollectionsId",
                        column: x => x.FileCollectionsId,
                        principalTable: "FileCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileCollectionOwner_Owners_OwnersId",
                        column: x => x.OwnersId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OwnerStoredFile",
                columns: table => new
                {
                    OwnersId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFilesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerStoredFile", x => new { x.OwnersId, x.StoredFilesId });
                    table.ForeignKey(
                        name: "FK_OwnerStoredFile_Owners_OwnersId",
                        column: x => x.OwnersId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OwnerStoredFile_StoredFiles_StoredFilesId",
                        column: x => x.StoredFilesId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileCollectionOwner_OwnersId",
                table: "FileCollectionOwner",
                column: "OwnersId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerStoredFile_StoredFilesId",
                table: "OwnerStoredFile",
                column: "StoredFilesId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_FileCollectionId",
                table: "StoredFiles",
                column: "FileCollectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileCollectionOwner");

            migrationBuilder.DropTable(
                name: "OwnerStoredFile");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "FileCollections");
        }
    }
}
