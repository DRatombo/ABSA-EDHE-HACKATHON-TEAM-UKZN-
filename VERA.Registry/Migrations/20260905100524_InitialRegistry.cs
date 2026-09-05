using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VERA.Registry.Migrations
{
    /// <inheritdoc />
    public partial class InitialRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistryIssuers",
                columns: table => new
                {
                    RegistryIssuerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VeraIssuerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CIPCRegistrationNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TradingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VATNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CIPCVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistryIssuers", x => x.RegistryIssuerId);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredPurchaseOrders",
                columns: table => new
                {
                    RegisteredPurchaseOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VeraPOId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegistryIssuerId = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierCIPCRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CanonicalFingerprint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredPurchaseOrders", x => x.RegisteredPurchaseOrderId);
                    table.ForeignKey(
                        name: "FK_RegisteredPurchaseOrders_RegistryIssuers_RegistryIssuerId",
                        column: x => x.RegistryIssuerId,
                        principalTable: "RegistryIssuers",
                        principalColumn: "RegistryIssuerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancingClaims",
                columns: table => new
                {
                    FinancingClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegisteredPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ClaimReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancingClaims", x => x.FinancingClaimId);
                    table.ForeignKey(
                        name: "FK_FinancingClaims_RegisteredPurchaseOrders_RegisteredPurchaseOrderId",
                        column: x => x.RegisteredPurchaseOrderId,
                        principalTable: "RegisteredPurchaseOrders",
                        principalColumn: "RegisteredPurchaseOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDocuments",
                columns: table => new
                {
                    PurchaseOrderDocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegisteredPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "bit", nullable: false),
                    ExtractedPONumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtractedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PONumberMatchesRegistry = table.Column<bool>(type: "bit", nullable: true),
                    AmountMatchesRegistry = table.Column<bool>(type: "bit", nullable: true),
                    AnalysisResult = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDocuments", x => x.PurchaseOrderDocumentId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDocuments_RegisteredPurchaseOrders_RegisteredPurchaseOrderId",
                        column: x => x.RegisteredPurchaseOrderId,
                        principalTable: "RegisteredPurchaseOrders",
                        principalColumn: "RegisteredPurchaseOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancingClaims_RegisteredPurchaseOrderId",
                table: "FinancingClaims",
                column: "RegisteredPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDocuments_DocumentHash",
                table: "PurchaseOrderDocuments",
                column: "DocumentHash");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDocuments_RegisteredPurchaseOrderId",
                table: "PurchaseOrderDocuments",
                column: "RegisteredPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredPurchaseOrders_RegistryIssuerId_PONumber",
                table: "RegisteredPurchaseOrders",
                columns: new[] { "RegistryIssuerId", "PONumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredPurchaseOrders_VeraPOId",
                table: "RegisteredPurchaseOrders",
                column: "VeraPOId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistryIssuers_CIPCRegistrationNumber",
                table: "RegistryIssuers",
                column: "CIPCRegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistryIssuers_VeraIssuerId",
                table: "RegistryIssuers",
                column: "VeraIssuerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancingClaims");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDocuments");

            migrationBuilder.DropTable(
                name: "RegisteredPurchaseOrders");

            migrationBuilder.DropTable(
                name: "RegistryIssuers");
        }
    }
}
