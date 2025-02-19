using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;
using PowrIntegrationService.Options;
using PowrIntegrationService.Powertill;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;

namespace PowrIntegrationService.Data.Importers;
public sealed class PluItemsImport(IOptions<IntegrationServiceOptions> options, RabbitMqFactory rabbitMqFactory, ILogger<PluItemsImport> logger)
    : FileImporter<PluItemDto>(options, "PluCreat.csa", logger)
{
    private readonly RabbitMqFactory _rabbitMqFactory = rabbitMqFactory;
    private readonly ILogger<PluItemsImport> _logger = logger;

    private sealed record PluFileItem
    {
        public required long PluNumber { get; init; }
        public string? PluDescription { get; set; }
        public string? SizeDescription { get; set; }
        public decimal SellingPrice1 { get; set; }
        public decimal SellingPrice2 { get; set; }
        public decimal SellingPrice3 { get; set; }
        public decimal SellingPrice4 { get; set; }
        public decimal SellingPrice5 { get; set; }
        public decimal SellingPrice6 { get; set; }
        public decimal SellingPrice7 { get; set; }
        public decimal SellingPrice8 { get; set; }
        public decimal SellingPrice9 { get; set; }
        public decimal GrossCost { get; set; }
        public decimal NettCost { get; set; }
        public decimal TargetMargin1 { get; set; }
        public decimal TargetMargin2 { get; set; }
        public decimal TargetMargin3 { get; set; }
        public decimal TargetMargin4 { get; set; }
        public decimal TargetMargin5 { get; set; }
        public decimal TargetMargin6 { get; set; }
        public decimal TargetMargin7 { get; set; }
        public decimal TargetMargin8 { get; set; }
        public decimal TargetMargin9 { get; set; }
        public int SalesGroup { get; set; }
        public int AccessLevel { get; set; }
        public string Flags { get; set; } = string.Empty;
        public bool IsFlag48 => Flags.Length > 0 && Flags[0] == 'Y';
        public bool IsFlag47 => Flags.Length > 1 && Flags[1] == 'Y';
        public bool IsFlag46 => Flags.Length > 2 && Flags[2] == 'Y';
        public bool IsFlag45 => Flags.Length > 3 && Flags[3] == 'Y';
        public bool IsFlag44 => Flags.Length > 4 && Flags[4] == 'Y';
        public bool IsFlag43 => Flags.Length > 5 && Flags[5] == 'Y';
        public bool IsFlag42 => Flags.Length > 6 && Flags[6] == 'Y';
        public bool IsBottleContentPlu => Flags.Length > 7 && Flags[7] == 'Y';
        public bool IsFridgeStockPlu => Flags.Length > 8 && Flags[8] == 'Y';
        public bool IsScaleComparisonPlu => Flags.Length > 9 && Flags[9] == 'Y';
        public bool IsThirdsPizzaPlu => Flags.Length > 10 && Flags[10] == 'Y';
        public bool IsPriceComparePlu => Flags.Length > 11 && Flags[11] == 'Y';
        public bool IsPrepSteersMainPlu => Flags.Length > 12 && Flags[12] == 'Y';
        public bool IsTrafficType3Plu => Flags.Length > 13 && Flags[13] == 'Y';
        public bool IsTrafficType2Plu => Flags.Length > 14 && Flags[14] == 'Y';
        public bool IsTrafficType1Plu => Flags.Length > 15 && Flags[15] == 'Y';
        public bool IsSelectedIngredientPlu => Flags.Length > 16 && Flags[16] == 'Y';
        public bool IsCheckStockOnHand => Flags.Length > 17 && Flags[17] == 'Y';
        public bool IsQuarterPizzaPlu => Flags.Length > 18 && Flags[18] == 'Y';
        public bool IsNoAddToSlsHistory => Flags.Length > 19 && Flags[19] == 'Y';
        public bool IsCyoPizzaPlu => Flags.Length > 20 && Flags[20] == 'Y';
        public bool IsHalfNHalfPizzaPlu => Flags.Length > 21 && Flags[21] == 'Y';
        public bool IsPrePaidVoucherPlu => Flags.Length > 22 && Flags[22] == 'Y';
        public bool IsReferenceNoComp => Flags.Length > 23 && Flags[23] == 'Y';
        public bool IsBottleContainerPlu => Flags.Length > 24 && Flags[24] == 'Y';
        public bool IsStockCountingPlu => Flags.Length > 25 && Flags[25] == 'Y';
        public bool IsKPrinterMessagePlu => Flags.Length > 26 && Flags[26] == 'Y';
        public bool IsMealAllowancePlu => Flags.Length > 27 && Flags[27] == 'Y';
        public bool IsPreparedItem => Flags.Length > 28 && Flags[28] == 'Y';
        public bool IsToppingPlu => Flags.Length > 29 && Flags[29] == 'Y';
        public bool IsPizzaPlu => Flags.Length > 30 && Flags[30] == 'Y';
        public bool IsAddToQuarterAndHalfHrRpt => Flags.Length > 31 && Flags[31] == 'Y';
        public bool IsPromptForItemCount => Flags.Length > 32 && Flags[32] == 'Y';
        public bool IsScalePresetItem => Flags.Length > 33 && Flags[33] == 'Y';
        public bool IsOutOfStock => Flags.Length > 34 && Flags[34] == 'Y';
        public bool IsOnPromotion => Flags.Length > 35 && Flags[35] == 'Y';
        public bool IsImeiNoCompulsed => Flags.Length > 36 && Flags[36] == 'Y';
        public bool IsSerialNoCompulsed => Flags.Length > 37 && Flags[37] == 'Y';
        public bool IsSpecialDealPlu => Flags.Length > 38 && Flags[38] == 'Y';
        public bool IsChainedPlu => Flags.Length > 39 && Flags[39] == 'Y';
        public bool IsScalePlu => Flags.Length > 40 && Flags[40] == 'Y';
        public bool IsFinishedProduct => Flags.Length > 41 && Flags[41] == 'Y';
        public bool IsStockOnlyPlu => Flags.Length > 42 && Flags[42] == 'Y';
        public bool IsOpenPricePlu => Flags.Length > 43 && Flags[43] == 'Y';
        public bool IsOpenDescriptionPlu => Flags.Length > 44 && Flags[44] == 'Y';
        public bool IsPreparationPlu => Flags.Length > 45 && Flags[45] == 'Y';
        public bool IsCuttingSchedulePlu => Flags.Length > 46 && Flags[46] == 'Y';
        public bool IsPromptForPrice => Flags.Length > 47 && Flags[47] == 'Y';
        public int MixAndMatchGroup { get; set; }
        public int DiscountMatrixGroup { get; set; }
        public string KpFlags { get; set; } = "..............";
        public bool IsKp1Item => KpFlags.Length > 0 && KpFlags[0] == '1';
        public bool IsKp2Item => KpFlags.Length > 1 && KpFlags[1] == '2';
        public bool IsKp3Item => KpFlags.Length > 2 && KpFlags[2] == '3';
        public bool IsKp4Item => KpFlags.Length > 3 && KpFlags[3] == '4';
        public bool IsKp5Item => KpFlags.Length > 4 && KpFlags[4] == '5';
        public bool IsKp6Item => KpFlags.Length > 5 && KpFlags[5] == '6';
        public bool IsKp7Item => KpFlags.Length > 6 && KpFlags[6] == '7';
        public bool IsKp8Item => KpFlags.Length > 7 && KpFlags[7] == '8';
        public bool IsKp9Item => KpFlags.Length > 8 && KpFlags[8] == '9';
        public bool IsKp10Item => KpFlags.Length > 9 && KpFlags[9] == 'A';
        public bool IsKp11Item => KpFlags.Length > 10 && KpFlags[10] == 'B';
        public bool IsKp12Item => KpFlags.Length > 11 && KpFlags[11] == 'C';
        public bool IsKp13Item => KpFlags.Length > 12 && KpFlags[12] == 'D';
        public bool IsKp14Item => KpFlags.Length > 13 && KpFlags[13] == 'E';
        public int? KpPriorityLevel { get; set; }
        public int KitchenPrinterGroup { get; set; }
        public int PieceCount { get; set; }
        public int? SoftKeyboard1 { get; set; }
        public int? SoftKeyboard2 { get; set; }
        public int? SoftKeyboard3 { get; set; }
        public int? SoftKeyboard4 { get; set; }
        public int? SoftKeyboard5 { get; set; }
        public int? SoftKeyboard6 { get; set; }
        public int? SoftKeyboard7 { get; set; }
        public int? SoftKeyboard8 { get; set; }
        public int? SoftKeyboard9 { get; set; }
        public int? SoftKeyboard10 { get; set; }
        public int BillPrintGroup { get; set; }
        public string? LinkedPlu { get; set; }
        public int? LoyaltyPoints { get; set; }
        public int ExpiryPeriod { get; set; }
        public int? DealGroup { get; set; }
        public string InternalPluFlags { get; set; } = "0000000000000000";
        public bool IsInternalPluFlag1 => InternalPluFlags.Length > 0 && InternalPluFlags[0] == '1';
        public bool IsInternalPluFlag2 => InternalPluFlags.Length > 1 && InternalPluFlags[1] == '1';
        public bool IsInternalPluFlag3 => InternalPluFlags.Length > 2 && InternalPluFlags[2] == '1';
        public bool IsInternalPluFlag4 => InternalPluFlags.Length > 3 && InternalPluFlags[3] == '1';
        public bool IsInternalPluFlag5 => InternalPluFlags.Length > 4 && InternalPluFlags[4] == '1';
        public bool IsInternalPluFlag6 => InternalPluFlags.Length > 5 && InternalPluFlags[5] == '1';
        public bool IsInternalPluFlag7 => InternalPluFlags.Length > 6 && InternalPluFlags[6] == '1';
        public bool IsInternalPluFlag8 => InternalPluFlags.Length > 7 && InternalPluFlags[7] == '1';
        public bool IsInternalPluFlag9 => InternalPluFlags.Length > 8 && InternalPluFlags[8] == '1';
        public bool IsInternalPluFlag10 => InternalPluFlags.Length > 9 && InternalPluFlags[9] == '1';
        public bool IsInternalPluFlag11 => InternalPluFlags.Length > 10 && InternalPluFlags[10] == '1';
        public bool IsInternalPluFlag12 => InternalPluFlags.Length > 11 && InternalPluFlags[11] == '1';
        public bool IsInternalPluFlag13 => InternalPluFlags.Length > 12 && InternalPluFlags[12] == '1';
        public bool IsInternalPluFlag14 => InternalPluFlags.Length > 13 && InternalPluFlags[13] == '1';
        public bool IsInternalPluFlag15 => InternalPluFlags.Length > 14 && InternalPluFlags[14] == '1';
        public bool IsInternalPluFlag16 => InternalPluFlags.Length > 15 && InternalPluFlags[15] == '1';
        public int LoyaltyPointsPrice1 { get; set; }
        public int LoyaltyPointsPrice2 { get; set; }
        public int LoyaltyPointsPrice3 { get; set; }
        public int LoyaltyPointsPrice4 { get; set; }
        public int LoyaltyPointsPrice5 { get; set; }
        public int LoyaltyPointsPrice6 { get; set; }
        public int LoyaltyPointsPrice7 { get; set; }
        public int LoyaltyPointsPrice8 { get; set; }
        public int LoyaltyPointsPrice9 { get; set; }
        public int CouponNumber { get; set; }
        public decimal DealMaximumPrice { get; set; }
        public decimal DealPremiumPrice { get; set; }
        public int ToppingKeyboardNo { get; set; }
        public int ToppingGroupNo { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public int FreeToppingsQty { get; set; }
        public int CateringItemType { get; set; }
        public int CateringSize { get; set; }
        public int ClassificationGroup1 { get; set; }
        public int ClassificationGroup2 { get; set; }
        public int ClassificationGroup3 { get; set; }
        public int ClassificationGroup4 { get; set; }
        public int ClassificationGroup5 { get; set; }
        public int ClassificationGroup6 { get; set; }
        public int ClassificationGroup7 { get; set; }
        public int ClassificationGroup8 { get; set; }
        public int ClassificationGroup9 { get; set; }
        public int RedemptionPoints { get; set; }
        public int VariantGroup { get; set; }
        public decimal ToppingPremiumPrice { get; set; }
        public int UseByPeriod { get; set; }
        public decimal CostMaxVariancePercent { get; set; }
        public decimal ContainerContentSize { get; set; }
        public long? ReferredPluNo { get; set; }
        public int? ReferredQuantity { get; set; }
        public int? ReOrderLevel { get; set; }
        public int? ReOrderQuantity { get; set; }
        public int? BinLocationNo { get; set; }
        public int? StocktakingGroup { get; set; }
        public string? Supplier1AccountNo { get; set; }
        public string? Supplier2AccountNo { get; set; }
        public string? Supplier3AccountNo { get; set; }
        public string? Supplier1StockCode { get; set; }
        public string? Supplier2StockCode { get; set; }
        public string? Supplier3StockCode { get; set; }
        public decimal BottleEmptyWeight { get; set; }
        public decimal BottleFullWeight { get; set; }
        public int BottleTotCount { get; set; }
        public int Supplier1LeadTime { get; set; }
        public int Supplier2LeadTime { get; set; }
        public int Supplier3LeadTime { get; set; }
        public decimal QuotedPriceSupplier1 { get; set; }
        public decimal QuotedPriceSupplier2 { get; set; }
        public decimal QuotedPriceSupplier3 { get; set; }
        public DateTime? DateQuotedSupplier1 { get; set; }
        public DateTime? DateQuotedSupplier2 { get; set; }
        public DateTime? DateQuotedSupplier3 { get; set; }
        public string CheckPriceSupplierFlags { get; set; } = "NNN";
        public bool IsCheckPriceSupplier1 => CheckPriceSupplierFlags.Length > 0 && CheckPriceSupplierFlags[0] == 'Y';
        public bool IsCheckPriceSupplier2 => CheckPriceSupplierFlags.Length > 1 && CheckPriceSupplierFlags[1] == 'Y';
        public bool IsCheckPriceSupplier3 => CheckPriceSupplierFlags.Length > 2 && CheckPriceSupplierFlags[2] == 'Y';
        public int? StockPurchaseGroup { get; set; }
        public decimal? PurchasePriceVariancePercent { get; set; }
        public string? WebItemDescription { get; set; }
        public decimal? BoxQuantity { get; set; }
        public decimal? PalletQuantity { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime DateTimeEdited { get; set; }
        public int? ClerkEdited { get; set; }
        public string? Version { get; set; }
    }

    private class PluItemMap : ClassMap<PluFileItem>
    {
        public PluItemMap()
        {
            Map(m => m.PluNumber).Index(0);
            Map(m => m.PluDescription).Index(1);
            Map(m => m.SizeDescription).Index(2);
            Map(m => m.SellingPrice1).Index(3);
            Map(m => m.SellingPrice2).Index(4);
            Map(m => m.SellingPrice3).Index(5);
            Map(m => m.SellingPrice4).Index(6);
            Map(m => m.SellingPrice5).Index(7);
            Map(m => m.SellingPrice6).Index(8);
            Map(m => m.SellingPrice7).Index(9);
            Map(m => m.SellingPrice8).Index(10);
            Map(m => m.SellingPrice9).Index(11);
            Map(m => m.GrossCost).Index(12);
            Map(m => m.NettCost).Index(13);
            Map(m => m.TargetMargin1).Index(14);
            Map(m => m.TargetMargin2).Index(15);
            Map(m => m.TargetMargin3).Index(16);
            Map(m => m.TargetMargin4).Index(17);
            Map(m => m.TargetMargin5).Index(18);
            Map(m => m.TargetMargin6).Index(19);
            Map(m => m.TargetMargin7).Index(20);
            Map(m => m.TargetMargin8).Index(21);
            Map(m => m.TargetMargin9).Index(22);
            Map(m => m.SalesGroup).Index(23);
            Map(m => m.AccessLevel).Index(24);
            Map(m => m.Flags).Index(25);
            Map(m => m.MixAndMatchGroup).Index(26);
            Map(m => m.DiscountMatrixGroup).Index(27);
            Map(m => m.KpFlags).Index(28);
            Map(m => m.KpPriorityLevel).Index(29);
            Map(m => m.KitchenPrinterGroup).Index(30);
            Map(m => m.PieceCount).Index(31);
            Map(m => m.SoftKeyboard1).Index(32);
            Map(m => m.SoftKeyboard2).Index(33);
            Map(m => m.SoftKeyboard3).Index(34);
            Map(m => m.SoftKeyboard4).Index(35);
            Map(m => m.SoftKeyboard5).Index(36);
            Map(m => m.SoftKeyboard6).Index(37);
            Map(m => m.SoftKeyboard7).Index(38);
            Map(m => m.SoftKeyboard8).Index(39);
            Map(m => m.SoftKeyboard9).Index(40);
            Map(m => m.SoftKeyboard10).Index(41);
            Map(m => m.BillPrintGroup).Index(42);
            Map(m => m.LinkedPlu).Index(43);
            Map(m => m.LoyaltyPoints).Index(44);
            Map(m => m.ExpiryPeriod).Index(45);
            Map(m => m.DealGroup).Index(46);
            Map(m => m.InternalPluFlags).Index(47);
            Map(m => m.LoyaltyPointsPrice1).Index(48);
            Map(m => m.LoyaltyPointsPrice2).Index(49);
            Map(m => m.LoyaltyPointsPrice3).Index(50);
            Map(m => m.LoyaltyPointsPrice4).Index(51);
            Map(m => m.LoyaltyPointsPrice5).Index(52);
            Map(m => m.LoyaltyPointsPrice6).Index(53);
            Map(m => m.LoyaltyPointsPrice7).Index(54);
            Map(m => m.LoyaltyPointsPrice8).Index(55);
            Map(m => m.LoyaltyPointsPrice9).Index(56);
            Map(m => m.CouponNumber).Index(57);
            Map(m => m.DealMaximumPrice).Index(58);
            Map(m => m.DealPremiumPrice).Index(59);
            Map(m => m.ToppingKeyboardNo).Index(60);
            Map(m => m.ToppingGroupNo).Index(61);
            Map(m => m.LastPurchaseDate).Index(62).TypeConverter<DelphiDateConverter>();
            Map(m => m.FreeToppingsQty).Index(63);
            Map(m => m.CateringItemType).Index(64);
            Map(m => m.CateringSize).Index(65);
            Map(m => m.ClassificationGroup1).Index(66);
            Map(m => m.ClassificationGroup2).Index(67);
            Map(m => m.ClassificationGroup3).Index(68);
            Map(m => m.ClassificationGroup4).Index(69);
            Map(m => m.ClassificationGroup5).Index(70);
            Map(m => m.ClassificationGroup6).Index(71);
            Map(m => m.ClassificationGroup7).Index(72);
            Map(m => m.ClassificationGroup8).Index(73);
            Map(m => m.ClassificationGroup9).Index(74);
            Map(m => m.RedemptionPoints).Index(75);
            Map(m => m.VariantGroup).Index(76);
            Map(m => m.ToppingPremiumPrice).Index(77);
            Map(m => m.UseByPeriod).Index(78);
            Map(m => m.CostMaxVariancePercent).Index(79);
            Map(m => m.ContainerContentSize).Index(80);
            Map(m => m.ReferredPluNo).Index(81);
            Map(m => m.ReferredQuantity).Index(82);
            Map(m => m.ReOrderLevel).Index(83);
            Map(m => m.ReOrderQuantity).Index(84);
            Map(m => m.BinLocationNo).Index(85);
            Map(m => m.StocktakingGroup).Index(86);
            Map(m => m.Supplier1AccountNo).Index(87);
            Map(m => m.Supplier2AccountNo).Index(88);
            Map(m => m.Supplier3AccountNo).Index(89);
            Map(m => m.Supplier1StockCode).Index(90);
            Map(m => m.Supplier2StockCode).Index(91);
            Map(m => m.Supplier3StockCode).Index(92);
            Map(m => m.BottleEmptyWeight).Index(93);
            Map(m => m.BottleFullWeight).Index(94);
            Map(m => m.BottleTotCount).Index(95);
            Map(m => m.Supplier1LeadTime).Index(96);
            Map(m => m.Supplier2LeadTime).Index(97);
            Map(m => m.Supplier3LeadTime).Index(98);
            Map(m => m.QuotedPriceSupplier1).Index(99);
            Map(m => m.QuotedPriceSupplier2).Index(100);
            Map(m => m.QuotedPriceSupplier3).Index(101);
            Map(m => m.DateQuotedSupplier1).Index(102).TypeConverter<PowertillDateConverter>();
            Map(m => m.DateQuotedSupplier2).Index(103).TypeConverter<PowertillDateConverter>();
            Map(m => m.DateQuotedSupplier3).Index(104).TypeConverter<PowertillDateConverter>();
            Map(m => m.CheckPriceSupplierFlags).Index(105);
            Map(m => m.StockPurchaseGroup).Index(106);
            Map(m => m.PurchasePriceVariancePercent).Index(107);
            Map(m => m.WebItemDescription).Index(108);
            Map(m => m.BoxQuantity).Index(109);
            Map(m => m.PalletQuantity).Index(110);
            Map(m => m.DateTimeCreated).Index(111).TypeConverter<Iso8601DateConverter>();
            Map(m => m.DateTimeEdited).Index(112).TypeConverter<Iso8601DateConverter>();
            Map(m => m.ClerkEdited).Index(113);
            Map(m => m.Version).Index(120);
        }
    }

    protected async override Task<Result<ImmutableArray<PluItemDto>>> ExecuteImport(CancellationToken cancellationToken)
    {
        try
        {
            IncrementTimesImportedMetric();

            var pluItemMap = new PluItemMap();

            var csaFile = new PowertillCsaFile<PluFileItem>(FilePath, null, pluItemMap);

            var pluFileItems = csaFile.ReadRecords().ToImmutableArray();

            var dtos = 
                pluFileItems
                    .Select(x => new PluItemDto
                    { 
                        PluNumber = x.PluNumber,
                        PluDescription = x.PluDescription,
                        SizeDescription = x.SizeDescription,
                        SellingPrice1 = x.SellingPrice1,
                        SalesGroup = x.SalesGroup,
                        Flags = x.Flags,
                        Supplier1StockCode = x.Supplier1StockCode,
                        DateTimeEdited = x.DateTimeEdited,
                        DateTimeCreated = x.DateTimeCreated
                    })
                    .ToImmutableArray();

            var syncQueuePublisher = await _rabbitMqFactory.CreatePublisher<ZraQueuePublisher>(cancellationToken);

            foreach (var item in dtos)
            {
                var publishResult = await syncQueuePublisher.PublishSavePluItem(item, cancellationToken);

                publishResult.LogErrors(_logger);
            }

            return Result.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occured importing PLU items from {FilePath}.", ex));
        }
    }

    private static void IncrementTimesImportedMetric()
    {
        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);

        var counter = meter.CreateCounter<long>("plu_file_import_counter", "times", "Counts number of times PLUs imported from PluCreat file.");

        counter.Add(1);
    }
}
