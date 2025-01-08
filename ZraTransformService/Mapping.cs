using PowrIntegration.Data.Entities;
using PowrIntegration.Dtos;
using PowrIntegration.MessageQueue;
using PowrIntegration.Options;
using PowrIntegration.Zra.ClassificationCodes;
using PowrIntegration.Zra.GetImports;
using PowrIntegration.Zra.SaveItem;
using PowrIntegration.Zra.UpdateItem;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static PowrIntegration.Zra.StandardCodes.FetchStandardCodesResponse;

namespace PowrIntegration;
public static class Mapping
{
    public static ImmutableArray<StandardCodeClassDto> MapToDtos(this ImmutableArray<CodeClass> classes)
    {
        return classes
            .Select(x => new StandardCodeClassDto
            {
                Code = x.cdCls,
                Name = x.cdClsNm,
                StandardCodes = x.dtlList
                    .Select(c => new StandardCodeDto
                    {
                        Code = c.cd,
                        Name = c.cdNm,
                        UserDefinedName = c.userDfnNm1
                    })
                    .ToImmutableArray()
            })
            .ToImmutableArray();            
    }

    public static ImmutableArray<ZraStandardCodeClass> MapToEntities(this ImmutableArray<StandardCodeClassDto> dtos)
    {
        return dtos
            .Select(x => new ZraStandardCodeClass
            {
                Code = x.Code,
                Name = x.Name,
                Codes = x.StandardCodes
                    .Select(c => new ZraStandardCode
                    {
                        Code = c.Code,
                        Name = c.Name,
                        UserDefinedName = c.UserDefinedName,
                        ClassCode = x.Code,
                    })
                    .ToList()
            })
            .ToImmutableArray();
    }

    public static ImmutableArray<ClassificationCodeDto> MapToDtos(this ImmutableArray<FetchClassificationCodesResponse.Code> codes)
    {
        return codes
            .Select(x => new ClassificationCodeDto
            {
                Code = x.itemClsCd,
                Name = x.itemClsNm,
                Level = x.itemClsLvl,
                TaxTypeCode = x.taxTyCd,
                IsMajorTarget = x.mjrTgYn is null ? null : x.mjrTgYn == "Y",
                ShouldUse = x.useYn is null ? null : x.useYn == "Y"
            })
            .ToImmutableArray();
    }

    public static ImmutableArray<ImportItemDto> MapToDtos(this ImmutableArray<GetImportsResponse.ImportItem> items)
    {
        return items
            .Select(x => new ImportItemDto
            {
                TaskCode = x.taskCd,
                DeclarationNumber = x.dclNo,
                DeclarationReferenceNumber = x.dclRefNum,
                AgentName = x.agntNm,
                ItemSequenceNumber = x.itemSeq,
                ItemName = x.itemNm,
                DeclarationDate = x.dclDe == "-1" ? null : DateTime.ParseExact(x.dclDe, "yyyyMMdd", CultureInfo.InvariantCulture),
                HarmonizedSystemCode = x.hsCd,
                SupplierName = x.spplrNm,
                ExportCountryCode = x.exptNatCd,
                OriginCountryCode = x.orgnNatCd,
                NetWeight = x.netWt,
                PackageQuantity = x.pkg,
                PackageUnitCode = x.pkgUnitCd,
                Quantity = x.qty,
                QuantityUnitCode = x.qtyUnitCd,
                TotalWeight = x.totWt,
                InvoiceForeignCurrencyAmount = x.invcFcurAmt,
                InvoiceForeignCurrencyCode = x.invcFcurCd,
                InvoiceForeignCurrencyExchangeRate = x.invcFcurExcrt
            })
            .ToImmutableArray();
    }

    public static ImmutableArray<ZraImportItem> MapToEntities(this ImmutableArray<ImportItemDto> items)
    {
        return items
            .Select(x => new ZraImportItem
            {
                TaskCode = x.TaskCode,
                DeclarationNumber = x.DeclarationNumber,
                DeclarationReferenceNumber = x.DeclarationReferenceNumber,
                AgentName = x.AgentName,
                ItemSequenceNumber = x.ItemSequenceNumber,
                ItemName = x.ItemName,
                DeclarationDate = x.DeclarationDate,
                HarmonizedSystemCode = x.HarmonizedSystemCode,
                SupplierName = x.SupplierName,
                ExportCountryCode = x.ExportCountryCode,
                OriginCountryCode = x.OriginCountryCode,
                NetWeight = x.NetWeight,
                PackageQuantity = x.PackageQuantity,
                PackageUnitCode = x.PackageUnitCode,
                Quantity = x.Quantity,
                QuantityUnitCode = x.QuantityUnitCode,
                TotalWeight = x.TotalWeight,
                InvoiceForeignCurrencyAmount = x.InvoiceForeignCurrencyAmount,
                InvoiceForeignCurrencyCode = x.InvoiceForeignCurrencyCode,
                InvoiceForeignCurrencyExchangeRate = x.InvoiceForeignCurrencyExchangeRate
            })
            .ToImmutableArray();
    }

    public static SaveItemRequest MapToSaveItemRequest(this PluItemDto plu, ApiOptions apiOptions)
    {
        return new SaveItemRequest
        {
            tpin = apiOptions.TaxpayerIdentificationNumber,
            bhfId = apiOptions.TaxpayerBranchIdentifier,
            itemCd = plu.PluNumber.ToString(),
            itemClsCd = plu.Supplier1StockCode!,
            itemTyCd =
                plu.IsStockOnlyPlu
                ? SaveItemRequest.ProductTypes.RawMaterial
                : SaveItemRequest.ProductTypes.FinishedProduct,
            itemNm = plu.PluDescription!,
            orgnNatCd = SaveItemRequest.NationalityCodes.Zambia,
            pkgUnitCd = SaveItemRequest.PackagingUnitCodes.Each,
            qtyUnitCd = SaveItemRequest.QuantityUnitCodes.Each,
            vatCatCd = GetTaxTypeCode(apiOptions.TaxTypeMappings, plu.SalesGroup),
            dftPrc = plu.SellingPrice1,
            useYn = "Y",
            regrNm = "ADMIN",
            regrId = "000",
            modrNm = "ADMIN",
            modrId = "000"
        };
    }

    public static UpdateItemRequest MapToUpdateItemRequest(this PluItemDto plu, ApiOptions apiOptions)
    {
        return new UpdateItemRequest
        {
            tpin = apiOptions.TaxpayerIdentificationNumber,
            bhfId = apiOptions.TaxpayerBranchIdentifier,
            itemCd = plu.PluNumber.ToString(),
            itemClsCd = plu.Supplier1StockCode!,
            itemTyCd =
                plu.IsStockOnlyPlu
                ? SaveItemRequest.ProductTypes.RawMaterial
                : SaveItemRequest.ProductTypes.FinishedProduct,
            itemNm = plu.PluDescription!,
            orgnNatCd = SaveItemRequest.NationalityCodes.Zambia,
            pkgUnitCd = SaveItemRequest.PackagingUnitCodes.Each,
            qtyUnitCd = SaveItemRequest.QuantityUnitCodes.Each,
            vatCatCd = GetTaxTypeCode(apiOptions.TaxTypeMappings, plu.SalesGroup),
            dftPrc = plu.SellingPrice1,
            useYn = "Y",
            regrNm = "ADMIN",
            regrId = "000",
            modrNm = "ADMIN",
            modrId = "000"
        };
    }

    private static string GetTaxTypeCode(TaxTypeMapping[] taxTypes, int salesGroup)
    {
        return
            taxTypes
                .FirstOrDefault(x => x.SalesGroupId == salesGroup)
                ?.TaxTypeCode
                ?? SaveItemRequest.TaxTypeCode.StandardRated;
    }

    public static ImmutableArray<OutboxItem> MapToOutboxItems(this ImmutableArray<PluItem> records)
    {
        return
            records
                .Select(x => new OutboxItem
                {
                    MessageType = QueueMessageType.ItemInsert,
                    MessageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x))
                })
                .ToImmutableArray();
    }

    public static PluItem MapToEntity(this PluItemDto dto)
    {
        return new PluItem
        {
            PluNumber = dto.PluNumber,
            PluDescription = dto.PluDescription,
            SizeDescription = dto.SizeDescription,
            SellingPrice1 = dto.SellingPrice1,
            SellingPrice2 = dto.SellingPrice2,
            SellingPrice3 = dto.SellingPrice3,
            SellingPrice4 = dto.SellingPrice4,
            SellingPrice5 = dto.SellingPrice5,
            SellingPrice6 = dto.SellingPrice6,
            SellingPrice7 = dto.SellingPrice7,
            SellingPrice8 = dto.SellingPrice8,
            SellingPrice9 = dto.SellingPrice9,
            GrossCost = dto.GrossCost,
            NettCost = dto.NettCost,
            TargetMargin1 = dto.TargetMargin1,
            TargetMargin2 = dto.TargetMargin2,
            TargetMargin3 = dto.TargetMargin3,
            TargetMargin4 = dto.TargetMargin4,
            TargetMargin5 = dto.TargetMargin5,
            TargetMargin6 = dto.TargetMargin6,
            TargetMargin7 = dto.TargetMargin7,
            TargetMargin8 = dto.TargetMargin8,
            TargetMargin9 = dto.TargetMargin9,
            SalesGroup = dto.SalesGroup,
            AccessLevel = dto.AccessLevel,
            Flags = dto.Flags,
            MixAndMatchGroup = dto.MixAndMatchGroup,
            DiscountMatrixGroup = dto.DiscountMatrixGroup,
            KpFlags = dto.KpFlags,
            KpPriorityLevel = dto.KpPriorityLevel,
            KitchenPrinterGroup = dto.KitchenPrinterGroup,
            PieceCount = dto.PieceCount,
            SoftKeyboard1 = dto.SoftKeyboard1,
            SoftKeyboard2 = dto.SoftKeyboard2,
            SoftKeyboard3 = dto.SoftKeyboard3,
            SoftKeyboard4 = dto.SoftKeyboard4,
            SoftKeyboard5 = dto.SoftKeyboard5,
            SoftKeyboard6 = dto.SoftKeyboard6,
            SoftKeyboard7 = dto.SoftKeyboard7,
            SoftKeyboard8 = dto.SoftKeyboard8,
            SoftKeyboard9 = dto.SoftKeyboard9,
            SoftKeyboard10 = dto.SoftKeyboard10,
            BillPrintGroup = dto.BillPrintGroup,
            LinkedPlu = dto.LinkedPlu,
            LoyaltyPoints = dto.LoyaltyPoints,
            ExpiryPeriod = dto.ExpiryPeriod,
            DealGroup = dto.DealGroup,
            InternalPluFlags = dto.InternalPluFlags,
            LoyaltyPointsPrice1 = dto.LoyaltyPointsPrice1,
            LoyaltyPointsPrice2 = dto.LoyaltyPointsPrice2,
            LoyaltyPointsPrice3 = dto.LoyaltyPointsPrice3,
            LoyaltyPointsPrice4 = dto.LoyaltyPointsPrice4,
            LoyaltyPointsPrice5 = dto.LoyaltyPointsPrice5,
            LoyaltyPointsPrice6 = dto.LoyaltyPointsPrice6,
            LoyaltyPointsPrice7 = dto.LoyaltyPointsPrice7,
            LoyaltyPointsPrice8 = dto.LoyaltyPointsPrice8,
            LoyaltyPointsPrice9 = dto.LoyaltyPointsPrice9,
            CouponNumber = dto.CouponNumber,
            DealMaximumPrice = dto.DealMaximumPrice,
            DealPremiumPrice = dto.DealPremiumPrice,
            ToppingKeyboardNo = dto.ToppingKeyboardNo,
            ToppingGroupNo = dto.ToppingGroupNo,
            LastPurchaseDate = dto.LastPurchaseDate,
            FreeToppingsQty = dto.FreeToppingsQty,
            CateringItemType = dto.CateringItemType,
            CateringSize = dto.CateringSize,
            ClassificationGroup1 = dto.ClassificationGroup1,
            ClassificationGroup2 = dto.ClassificationGroup2,
            ClassificationGroup3 = dto.ClassificationGroup3,
            ClassificationGroup4 = dto.ClassificationGroup4,
            ClassificationGroup5 = dto.ClassificationGroup5,
            ClassificationGroup6 = dto.ClassificationGroup6,
            ClassificationGroup7 = dto.ClassificationGroup7,
            ClassificationGroup8 = dto.ClassificationGroup8,
            ClassificationGroup9 = dto.ClassificationGroup9,
            RedemptionPoints = dto.RedemptionPoints,
            VariantGroup = dto.VariantGroup,
            ToppingPremiumPrice = dto.ToppingPremiumPrice,
            UseByPeriod = dto.UseByPeriod,
            CostMaxVariancePercent = dto.CostMaxVariancePercent,
            ContainerContentSize = dto.ContainerContentSize,
            ReferredPluNo = dto.ReferredPluNo,
            ReferredQuantity = dto.ReferredQuantity,
            ReOrderLevel = dto.ReOrderLevel,
            ReOrderQuantity = dto.ReOrderQuantity,
            BinLocationNo = dto.BinLocationNo,
            StocktakingGroup = dto.StocktakingGroup,
            Supplier1AccountNo = dto.Supplier1AccountNo,
            Supplier2AccountNo = dto.Supplier2AccountNo,
            Supplier3AccountNo = dto.Supplier3AccountNo,
            Supplier1StockCode = dto.Supplier1StockCode,
            Supplier2StockCode = dto.Supplier2StockCode,
            Supplier3StockCode = dto.Supplier3StockCode,
            BottleEmptyWeight = dto.BottleEmptyWeight,
            BottleFullWeight = dto.BottleFullWeight,
            BottleTotCount = dto.BottleTotCount,
            Supplier1LeadTime = dto.Supplier1LeadTime,
            Supplier2LeadTime = dto.Supplier2LeadTime,
            Supplier3LeadTime = dto.Supplier3LeadTime,
            QuotedPriceSupplier1 = dto.QuotedPriceSupplier1,
            QuotedPriceSupplier2 = dto.QuotedPriceSupplier2,
            QuotedPriceSupplier3 = dto.QuotedPriceSupplier3,
            DateQuotedSupplier1 = dto.DateQuotedSupplier1,
            DateQuotedSupplier2 = dto.DateQuotedSupplier2,
            DateQuotedSupplier3 = dto.DateQuotedSupplier3,
            CheckPriceSupplierFlags = dto.CheckPriceSupplierFlags,
            StockPurchaseGroup = dto.StockPurchaseGroup,
            PurchasePriceVariancePercent = dto.PurchasePriceVariancePercent,
            WebItemDescription = dto.WebItemDescription,
            BoxQuantity = dto.BoxQuantity,
            PalletQuantity = dto.PalletQuantity,
            DateTimeCreated = dto.DateTimeCreated,
            DateTimeEdited = dto.DateTimeEdited,
            ClerkEdited = dto.ClerkEdited,
            Version = dto.Version,
        };
    }
}
