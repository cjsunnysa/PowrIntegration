using PowrIntegrationService.Dtos;

namespace PowrIntegrationService.Data.Entities;

public sealed record PluItem
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

    public Recipe? Recipe { get; set; }
    public List<Ingredient> IngredientIn { get; init; } = [];

    public void UpdateFrom(PluItemDto plu)
    {
        PluDescription = plu.PluDescription;
        SizeDescription = plu.SizeDescription;
        SellingPrice1 = plu.SellingPrice1;
        SellingPrice2 = plu.SellingPrice2;
        SellingPrice3 = plu.SellingPrice3;
        SellingPrice4 = plu.SellingPrice4;
        SellingPrice5 = plu.SellingPrice5;
        SellingPrice6 = plu.SellingPrice6;
        SellingPrice7 = plu.SellingPrice7;
        SellingPrice8 = plu.SellingPrice8;
        SellingPrice9 = plu.SellingPrice9;
        GrossCost = plu.GrossCost;
        NettCost = plu.NettCost;
        TargetMargin1 = plu.TargetMargin1;
        TargetMargin2 = plu.TargetMargin2;
        TargetMargin3 = plu.TargetMargin3;
        TargetMargin4 = plu.TargetMargin4;
        TargetMargin5 = plu.TargetMargin5;
        TargetMargin6 = plu.TargetMargin6;
        TargetMargin7 = plu.TargetMargin7;
        TargetMargin8 = plu.TargetMargin8;
        TargetMargin9 = plu.TargetMargin9;
        SalesGroup = plu.SalesGroup;
        AccessLevel = plu.AccessLevel;
        Flags = plu.Flags;
        MixAndMatchGroup = plu.MixAndMatchGroup;
        DiscountMatrixGroup = plu.DiscountMatrixGroup;
        KpFlags = plu.KpFlags;
        KpPriorityLevel = plu.KpPriorityLevel;
        KitchenPrinterGroup = plu.KitchenPrinterGroup;
        PieceCount = plu.PieceCount;
        SoftKeyboard1 = plu.SoftKeyboard1;
        SoftKeyboard2 = plu.SoftKeyboard2;
        SoftKeyboard3 = plu.SoftKeyboard3;
        SoftKeyboard4 = plu.SoftKeyboard4;
        SoftKeyboard5 = plu.SoftKeyboard5;
        SoftKeyboard6 = plu.SoftKeyboard6;
        SoftKeyboard7 = plu.SoftKeyboard7;
        SoftKeyboard8 = plu.SoftKeyboard8;
        SoftKeyboard9 = plu.SoftKeyboard9;
        SoftKeyboard10 = plu.SoftKeyboard10;
        BillPrintGroup = plu.BillPrintGroup;
        LinkedPlu = plu.LinkedPlu;
        LoyaltyPoints = plu.LoyaltyPoints;
        ExpiryPeriod = plu.ExpiryPeriod;
        DealGroup = plu.DealGroup;
        InternalPluFlags = plu.InternalPluFlags;
        LoyaltyPointsPrice1 = plu.LoyaltyPointsPrice1;
        LoyaltyPointsPrice2 = plu.LoyaltyPointsPrice2;
        LoyaltyPointsPrice3 = plu.LoyaltyPointsPrice3;
        LoyaltyPointsPrice4 = plu.LoyaltyPointsPrice4;
        LoyaltyPointsPrice5 = plu.LoyaltyPointsPrice5;
        LoyaltyPointsPrice6 = plu.LoyaltyPointsPrice6;
        LoyaltyPointsPrice7 = plu.LoyaltyPointsPrice7;
        LoyaltyPointsPrice8 = plu.LoyaltyPointsPrice8;
        LoyaltyPointsPrice9 = plu.LoyaltyPointsPrice9;
        CouponNumber = plu.CouponNumber;
        DealMaximumPrice = plu.DealMaximumPrice;
        DealPremiumPrice = plu.DealPremiumPrice;
        ToppingKeyboardNo = plu.ToppingKeyboardNo;
        ToppingGroupNo = plu.ToppingGroupNo;
        LastPurchaseDate = plu.LastPurchaseDate;
        FreeToppingsQty = plu.FreeToppingsQty;
        CateringItemType = plu.CateringItemType;
        CateringSize = plu.CateringSize;
        ClassificationGroup1 = plu.ClassificationGroup1;
        ClassificationGroup2 = plu.ClassificationGroup2;
        ClassificationGroup3 = plu.ClassificationGroup3;
        ClassificationGroup4 = plu.ClassificationGroup4;
        ClassificationGroup5 = plu.ClassificationGroup5;
        ClassificationGroup6 = plu.ClassificationGroup6;
        ClassificationGroup7 = plu.ClassificationGroup7;
        ClassificationGroup8 = plu.ClassificationGroup8;
        ClassificationGroup9 = plu.ClassificationGroup9;
        RedemptionPoints = plu.RedemptionPoints;
        VariantGroup = plu.VariantGroup;
        ToppingPremiumPrice = plu.ToppingPremiumPrice;
        UseByPeriod = plu.UseByPeriod;
        CostMaxVariancePercent = plu.CostMaxVariancePercent;
        ContainerContentSize = plu.ContainerContentSize;
        ReferredPluNo = plu.ReferredPluNo;
        ReferredQuantity = plu.ReferredQuantity;
        ReOrderLevel = plu.ReOrderLevel;
        ReOrderQuantity = plu.ReOrderQuantity;
        BinLocationNo = plu.BinLocationNo;
        StocktakingGroup = plu.StocktakingGroup;
        Supplier1AccountNo = plu.Supplier1AccountNo;
        Supplier2AccountNo = plu.Supplier2AccountNo;
        Supplier3AccountNo = plu.Supplier3AccountNo;
        Supplier1StockCode = plu.Supplier1StockCode;
        Supplier2StockCode = plu.Supplier2StockCode;
        Supplier3StockCode = plu.Supplier3StockCode;
        BottleEmptyWeight = plu.BottleEmptyWeight;
        BottleFullWeight = plu.BottleFullWeight;
        BottleTotCount = plu.BottleTotCount;
        Supplier1LeadTime = plu.Supplier1LeadTime;
        Supplier2LeadTime = plu.Supplier2LeadTime;
        Supplier3LeadTime = plu.Supplier3LeadTime;
        QuotedPriceSupplier1 = plu.QuotedPriceSupplier1;
        QuotedPriceSupplier2 = plu.QuotedPriceSupplier2;
        QuotedPriceSupplier3 = plu.QuotedPriceSupplier3;
        DateQuotedSupplier1 = plu.DateQuotedSupplier1;
        DateQuotedSupplier2 = plu.DateQuotedSupplier2;
        DateQuotedSupplier3 = plu.DateQuotedSupplier3;
        CheckPriceSupplierFlags = plu.CheckPriceSupplierFlags;
        StockPurchaseGroup = plu.StockPurchaseGroup;
        PurchasePriceVariancePercent = plu.PurchasePriceVariancePercent;
        WebItemDescription = plu.WebItemDescription;
        BoxQuantity = plu.BoxQuantity;
        PalletQuantity = plu.PalletQuantity;
        DateTimeCreated = plu.DateTimeCreated;
        DateTimeEdited = plu.DateTimeEdited;
        ClerkEdited = plu.ClerkEdited;
        Version = plu.Version;
    }
}
