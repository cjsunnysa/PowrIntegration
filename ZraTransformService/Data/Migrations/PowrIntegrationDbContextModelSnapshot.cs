﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PowrIntegration.Data;

#nullable disable

namespace PowrIntegration.Data.Migrations
{
    [DbContext(typeof(PowrIntegrationDbContext))]
    partial class PowrIntegrationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("PowrIntegration.Data.Entities.OutboxItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte>("FailureCount")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("MessageBody")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<int>("MessageType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("OutboxItems");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.PluItem", b =>
                {
                    b.Property<long>("PluNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("AccessLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BillPrintGroup")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BinLocationNo")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BottleEmptyWeight")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("BottleFullWeight")
                        .HasColumnType("TEXT");

                    b.Property<int>("BottleTotCount")
                        .HasColumnType("INTEGER");

                    b.Property<decimal?>("BoxQuantity")
                        .HasColumnType("TEXT");

                    b.Property<int>("CateringItemType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CateringSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CheckPriceSupplierFlags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ClassificationGroup1")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup2")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup3")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup4")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup5")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup6")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup7")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup8")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassificationGroup9")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ClerkEdited")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("ContainerContentSize")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("CostMaxVariancePercent")
                        .HasColumnType("TEXT");

                    b.Property<int>("CouponNumber")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("DateQuotedSupplier1")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateQuotedSupplier2")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DateQuotedSupplier3")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTimeCreated")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTimeEdited")
                        .HasColumnType("TEXT");

                    b.Property<int?>("DealGroup")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("DealMaximumPrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("DealPremiumPrice")
                        .HasColumnType("TEXT");

                    b.Property<int>("DiscountMatrixGroup")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExpiryPeriod")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Flags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("FreeToppingsQty")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("GrossCost")
                        .HasColumnType("TEXT");

                    b.Property<string>("InternalPluFlags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("KitchenPrinterGroup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("KpFlags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("KpPriorityLevel")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastPurchaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LinkedPlu")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LoyaltyPoints")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice1")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice2")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice3")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice4")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice5")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice6")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice7")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice8")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LoyaltyPointsPrice9")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MixAndMatchGroup")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("NettCost")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("PalletQuantity")
                        .HasColumnType("TEXT");

                    b.Property<int>("PieceCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PluDescription")
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("PurchasePriceVariancePercent")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("QuotedPriceSupplier1")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("QuotedPriceSupplier2")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("QuotedPriceSupplier3")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ReOrderLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ReOrderQuantity")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RedemptionPoints")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("ReferredPluNo")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ReferredQuantity")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SalesGroup")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("SellingPrice1")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice2")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice3")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice4")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice5")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice6")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice7")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice8")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("SellingPrice9")
                        .HasColumnType("TEXT");

                    b.Property<string>("SizeDescription")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SoftKeyboard1")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard10")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard2")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard3")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard4")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard5")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard6")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard7")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard8")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SoftKeyboard9")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StockPurchaseGroup")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StocktakingGroup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Supplier1AccountNo")
                        .HasColumnType("TEXT");

                    b.Property<int>("Supplier1LeadTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Supplier1StockCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Supplier2AccountNo")
                        .HasColumnType("TEXT");

                    b.Property<int>("Supplier2LeadTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Supplier2StockCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Supplier3AccountNo")
                        .HasColumnType("TEXT");

                    b.Property<int>("Supplier3LeadTime")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Supplier3StockCode")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin1")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin2")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin3")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin4")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin5")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin6")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin7")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin8")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("TargetMargin9")
                        .HasColumnType("TEXT");

                    b.Property<int>("ToppingGroupNo")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ToppingKeyboardNo")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("ToppingPremiumPrice")
                        .HasColumnType("TEXT");

                    b.Property<int>("UseByPeriod")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VariantGroup")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Version")
                        .HasColumnType("TEXT");

                    b.Property<string>("WebItemDescription")
                        .HasColumnType("TEXT");

                    b.HasKey("PluNumber");

                    b.ToTable("PluItems");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationClass", b =>
                {
                    b.Property<long>("Code")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<long>("FamilyCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.HasIndex("FamilyCode");

                    b.ToTable("ZraClassificationClasses");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationCode", b =>
                {
                    b.Property<long>("Code")
                        .HasColumnType("INTEGER");

                    b.Property<long?>("ClassCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsMajorTarget")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("ShouldUse")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TaxTypeCode")
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.HasIndex("ClassCode");

                    b.ToTable("ZraClassificationCodes");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationFamily", b =>
                {
                    b.Property<long>("Code")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("SegmentCode")
                        .HasColumnType("INTEGER");

                    b.HasKey("Code");

                    b.HasIndex("SegmentCode");

                    b.ToTable("ZraClassificationFamilies");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationSegment", b =>
                {
                    b.Property<long>("Code")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.ToTable("ZraClassificationSegments");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraImportItem", b =>
                {
                    b.Property<string>("DeclarationNumber")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemSequenceNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AgentName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeclarationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("DeclarationReferenceNumber")
                        .HasColumnType("TEXT");

                    b.Property<string>("ExportCountryCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("HarmonizedSystemCode")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("InvoiceForeignCurrencyAmount")
                        .HasColumnType("TEXT");

                    b.Property<string>("InvoiceForeignCurrencyCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("InvoiceForeignCurrencyExchangeRate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("NetWeight")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginCountryCode")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("PackageQuantity")
                        .HasColumnType("TEXT");

                    b.Property<string>("PackageUnitCode")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("TEXT");

                    b.Property<string>("QuantityUnitCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("SupplierName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TaskCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("TotalWeight")
                        .HasColumnType("TEXT");

                    b.HasKey("DeclarationNumber", "ItemSequenceNumber");

                    b.ToTable("ZraImportItems");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraStandardCode", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClassCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UserDefinedName")
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.HasIndex("ClassCode");

                    b.ToTable("ZraStandardCodes");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraStandardCodeClass", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Code");

                    b.ToTable("ZraStandardCodeClasses");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationClass", b =>
                {
                    b.HasOne("PowrIntegration.Data.Entities.ZraClassificationFamily", "Family")
                        .WithMany("Classes")
                        .HasForeignKey("FamilyCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Family");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationCode", b =>
                {
                    b.HasOne("PowrIntegration.Data.Entities.ZraClassificationClass", "Class")
                        .WithMany("Codes")
                        .HasForeignKey("ClassCode");

                    b.Navigation("Class");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationFamily", b =>
                {
                    b.HasOne("PowrIntegration.Data.Entities.ZraClassificationSegment", "Segment")
                        .WithMany("Families")
                        .HasForeignKey("SegmentCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Segment");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraStandardCode", b =>
                {
                    b.HasOne("PowrIntegration.Data.Entities.ZraStandardCodeClass", "Class")
                        .WithMany("Codes")
                        .HasForeignKey("ClassCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationClass", b =>
                {
                    b.Navigation("Codes");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationFamily", b =>
                {
                    b.Navigation("Classes");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraClassificationSegment", b =>
                {
                    b.Navigation("Families");
                });

            modelBuilder.Entity("PowrIntegration.Data.Entities.ZraStandardCodeClass", b =>
                {
                    b.Navigation("Codes");
                });
#pragma warning restore 612, 618
        }
    }
}
