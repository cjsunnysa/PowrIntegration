using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowrIntegration.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageType = table.Column<int>(type: "INTEGER", nullable: false),
                    MessageBody = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FailureCount = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluItems",
                columns: table => new
                {
                    PluNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    PluDescription = table.Column<string>(type: "TEXT", nullable: true),
                    SizeDescription = table.Column<string>(type: "TEXT", nullable: true),
                    SellingPrice1 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice2 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice3 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice4 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice5 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice6 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice7 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice8 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SellingPrice9 = table.Column<decimal>(type: "TEXT", nullable: false),
                    GrossCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    NettCost = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin1 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin2 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin3 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin4 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin5 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin6 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin7 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin8 = table.Column<decimal>(type: "TEXT", nullable: false),
                    TargetMargin9 = table.Column<decimal>(type: "TEXT", nullable: false),
                    SalesGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Flags = table.Column<string>(type: "TEXT", nullable: false),
                    MixAndMatchGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscountMatrixGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    KpFlags = table.Column<string>(type: "TEXT", nullable: false),
                    KpPriorityLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    KitchenPrinterGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    PieceCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftKeyboard1 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard2 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard3 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard4 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard5 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard6 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard7 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard8 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard9 = table.Column<int>(type: "INTEGER", nullable: true),
                    SoftKeyboard10 = table.Column<int>(type: "INTEGER", nullable: true),
                    BillPrintGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedPlu = table.Column<string>(type: "TEXT", nullable: true),
                    LoyaltyPoints = table.Column<int>(type: "INTEGER", nullable: true),
                    ExpiryPeriod = table.Column<int>(type: "INTEGER", nullable: false),
                    DealGroup = table.Column<int>(type: "INTEGER", nullable: true),
                    InternalPluFlags = table.Column<string>(type: "TEXT", nullable: false),
                    LoyaltyPointsPrice1 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice2 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice3 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice4 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice5 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice6 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice7 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice8 = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyPointsPrice9 = table.Column<int>(type: "INTEGER", nullable: false),
                    CouponNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DealMaximumPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    DealPremiumPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    ToppingKeyboardNo = table.Column<int>(type: "INTEGER", nullable: false),
                    ToppingGroupNo = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FreeToppingsQty = table.Column<int>(type: "INTEGER", nullable: false),
                    CateringItemType = table.Column<int>(type: "INTEGER", nullable: false),
                    CateringSize = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup1 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup2 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup3 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup4 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup5 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup6 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup7 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup8 = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassificationGroup9 = table.Column<int>(type: "INTEGER", nullable: false),
                    RedemptionPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    VariantGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    ToppingPremiumPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    UseByPeriod = table.Column<int>(type: "INTEGER", nullable: false),
                    CostMaxVariancePercent = table.Column<decimal>(type: "TEXT", nullable: false),
                    ContainerContentSize = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReferredPluNo = table.Column<long>(type: "INTEGER", nullable: true),
                    ReferredQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    ReOrderLevel = table.Column<int>(type: "INTEGER", nullable: true),
                    ReOrderQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    BinLocationNo = table.Column<int>(type: "INTEGER", nullable: true),
                    StocktakingGroup = table.Column<int>(type: "INTEGER", nullable: true),
                    Supplier1AccountNo = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier2AccountNo = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier3AccountNo = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier1StockCode = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier2StockCode = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier3StockCode = table.Column<string>(type: "TEXT", nullable: true),
                    BottleEmptyWeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    BottleFullWeight = table.Column<decimal>(type: "TEXT", nullable: false),
                    BottleTotCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplier1LeadTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplier2LeadTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Supplier3LeadTime = table.Column<int>(type: "INTEGER", nullable: false),
                    QuotedPriceSupplier1 = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuotedPriceSupplier2 = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuotedPriceSupplier3 = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateQuotedSupplier1 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateQuotedSupplier2 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateQuotedSupplier3 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CheckPriceSupplierFlags = table.Column<string>(type: "TEXT", nullable: false),
                    StockPurchaseGroup = table.Column<int>(type: "INTEGER", nullable: true),
                    PurchasePriceVariancePercent = table.Column<decimal>(type: "TEXT", nullable: true),
                    WebItemDescription = table.Column<string>(type: "TEXT", nullable: true),
                    BoxQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    PalletQuantity = table.Column<decimal>(type: "TEXT", nullable: true),
                    DateTimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateTimeEdited = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClerkEdited = table.Column<int>(type: "INTEGER", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluItems", x => x.PluNumber);
                });

            migrationBuilder.CreateTable(
                name: "ZraClassificationSegments",
                columns: table => new
                {
                    Code = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraClassificationSegments", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ZraImportItems",
                columns: table => new
                {
                    DeclarationNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ItemSequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskCode = table.Column<string>(type: "TEXT", nullable: false),
                    DeclarationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HarmonizedSystemCode = table.Column<string>(type: "TEXT", nullable: true),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    OriginCountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    ExportCountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    PackageQuantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    PackageUnitCode = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantityUnitCode = table.Column<string>(type: "TEXT", nullable: true),
                    TotalWeight = table.Column<decimal>(type: "TEXT", nullable: true),
                    NetWeight = table.Column<decimal>(type: "TEXT", nullable: true),
                    SupplierName = table.Column<string>(type: "TEXT", nullable: false),
                    AgentName = table.Column<string>(type: "TEXT", nullable: true),
                    InvoiceForeignCurrencyAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    InvoiceForeignCurrencyCode = table.Column<string>(type: "TEXT", nullable: false),
                    InvoiceForeignCurrencyExchangeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    DeclarationReferenceNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraImportItems", x => new { x.DeclarationNumber, x.ItemSequenceNumber });
                });

            migrationBuilder.CreateTable(
                name: "ZraStandardCodeClasses",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraStandardCodeClasses", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "ZraClassificationFamilies",
                columns: table => new
                {
                    Code = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SegmentCode = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraClassificationFamilies", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ZraClassificationFamilies_ZraClassificationSegments_SegmentCode",
                        column: x => x.SegmentCode,
                        principalTable: "ZraClassificationSegments",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZraStandardCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    UserDefinedName = table.Column<string>(type: "TEXT", nullable: true),
                    ClassCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraStandardCodes", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ZraStandardCodes_ZraStandardCodeClasses_ClassCode",
                        column: x => x.ClassCode,
                        principalTable: "ZraStandardCodeClasses",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZraClassificationClasses",
                columns: table => new
                {
                    Code = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FamilyCode = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraClassificationClasses", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ZraClassificationClasses_ZraClassificationFamilies_FamilyCode",
                        column: x => x.FamilyCode,
                        principalTable: "ZraClassificationFamilies",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ZraClassificationCodes",
                columns: table => new
                {
                    Code = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TaxTypeCode = table.Column<string>(type: "TEXT", nullable: true),
                    IsMajorTarget = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShouldUse = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClassCode = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZraClassificationCodes", x => x.Code);
                    table.ForeignKey(
                        name: "FK_ZraClassificationCodes_ZraClassificationClasses_ClassCode",
                        column: x => x.ClassCode,
                        principalTable: "ZraClassificationClasses",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZraClassificationClasses_FamilyCode",
                table: "ZraClassificationClasses",
                column: "FamilyCode");

            migrationBuilder.CreateIndex(
                name: "IX_ZraClassificationCodes_ClassCode",
                table: "ZraClassificationCodes",
                column: "ClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_ZraClassificationFamilies_SegmentCode",
                table: "ZraClassificationFamilies",
                column: "SegmentCode");

            migrationBuilder.CreateIndex(
                name: "IX_ZraStandardCodes_ClassCode",
                table: "ZraStandardCodes",
                column: "ClassCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxItems");

            migrationBuilder.DropTable(
                name: "PluItems");

            migrationBuilder.DropTable(
                name: "ZraClassificationCodes");

            migrationBuilder.DropTable(
                name: "ZraImportItems");

            migrationBuilder.DropTable(
                name: "ZraStandardCodes");

            migrationBuilder.DropTable(
                name: "ZraClassificationClasses");

            migrationBuilder.DropTable(
                name: "ZraStandardCodeClasses");

            migrationBuilder.DropTable(
                name: "ZraClassificationFamilies");

            migrationBuilder.DropTable(
                name: "ZraClassificationSegments");
        }
    }
}
