using PowrIntegration.Shared.Dtos;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Options;
using PowrIntegrationService.Zra.ClassificationCodes;
using PowrIntegrationService.Zra.GetImports;
using PowrIntegrationService.Zra.GetPurchases;
using PowrIntegrationService.Zra.SaveItem;
using PowrIntegrationService.Zra.SavePurchase;
using PowrIntegrationService.Zra.UpdateItem;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static PowrIntegrationService.Zra.StandardCodes.FetchStandardCodesResponse;

namespace PowrIntegrationService.Extensions;
public static class Mapping
{
    public static ImmutableArray<StandardCodeClassDto> ToDtos(this ImmutableArray<CodeClass> classes)
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

    public static ImmutableArray<ZraStandardCodeClass> ToEntities(this ImmutableArray<StandardCodeClassDto> dtos)
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

    public static ImmutableArray<ClassificationCodeDto> ToDtos(this ImmutableArray<FetchClassificationCodesResponse.Code> codes)
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

    public static ImmutableArray<ImportItemDto> ToDtos(this ImmutableArray<GetImportsResponse.ImportItem> items)
    {
        return items
            .Select(x => new ImportItemDto
            {
                TaskCode = x.taskCd ?? string.Empty,
                DeclarationNumber = x.dclNo,
                DeclarationReferenceNumber = x.dclRefNum,
                AgentName = x.agntNm,
                ItemSequenceNumber = x.itemSeq,
                ItemName = x.itemNm ?? string.Empty,
                DeclarationDate = x.dclDe is null || x.dclDe == "-1" ? null : DateTime.ParseExact(x.dclDe, "yyyyMMdd", CultureInfo.InvariantCulture),
                HarmonizedSystemCode = x.hsCd,
                SupplierName = x.spplrNm ?? string.Empty,
                ExportCountryCode = x.exptNatCd,
                OriginCountryCode = x.orgnNatCd,
                NetWeight = x.netWt,
                PackageQuantity = x.pkg ?? 0,
                PackageUnitCode = x.pkgUnitCd,
                Quantity = x.qty ?? 0,
                QuantityUnitCode = x.qtyUnitCd,
                TotalWeight = x.totWt,
                InvoiceForeignCurrencyAmount = x.invcFcurAmt ?? 0,
                InvoiceForeignCurrencyCode = x.invcFcurCd ?? string.Empty,
                InvoiceForeignCurrencyExchangeRate = x.invcFcurExcrt
            })
            .ToImmutableArray();
    }

    public static ImmutableArray<ZraImportItem> ToEntities(this ImmutableArray<ImportItemDto> items)
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

    public static SaveItemRequest ToSaveItemRequest(this PluItemDto plu, ZraApiOptions apiOptions)
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

    public static UpdateItemRequest ToUpdateItemRequest(this PluItemDto plu, ZraApiOptions apiOptions)
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

    public static ImmutableArray<PurchaseDto> MapToDtos(this GetPurchasesResponse response)
    {
        return
            response
                .Data
                ?.SaleList
                .Select(x => new PurchaseDto
                {
                    SupplierTaxPayerIdentifier = x.spplrTpin,
                    SupplierName = x.spplrNm,
                    SupplierBranchIdentifier = x.spplrBhfId,
                    SupplierInvoiceNumber = x.spplrInvcNo,
                    RecieptTypeCode = x.rcptTyCd,
                    PaymentTypeCode = x.pmtTyCd,
                    ConfirmedDate = x.cfmDt,
                    SalesDate = x.salesDt,
                    StockReleaseDate = x.stockRlsDt,
                    TotalItemCount = x.totItemCnt,
                    TotalTaxableAmount = x.totTaxblAmt,
                    TotalTaxAmount = x.totTaxAmt,
                    TotalAmount = x.totAmt,
                    Remark = x.remark,
                    Items = 
                        x.itemList
                            .Select(i => new PurchaseDto.PurchaseItemDto 
                            { 
                                SequenceNumber = i.ItemSeq,
                                SupplierItemCode = i.ItemCd,
                                ClassificationCode = i.ItemClsCd,
                                Name = i.ItemNm,
                                Barcode = i.Bcd,
                                PackagingUnitCode = i.PkgUnitCd,
                                Package = i.Pkg,
                                QuantityUnitCode = i.QtyUnitCd,
                                Quantity = i.Qty,
                                InclusiveUnitPrice = i.Prc,
                                SupplyAmount = i.SplyAmt,
                                DiscountRate = i.DcRt,
                                DiscountAmount = i.DcAmt,
                                VatCategoryCode = i.VatCatCd,
                                IplCategoryCode = i.IplCatCd,
                                TlCategoryCode = i.TlCatCd,
                                ExciseCategoryCode = i.ExciseTxCatC,
                                VatTaxableAmount = i.VatTaxblAmt,
                                IplTaxableAmount = i.IplTaxblAmt,
                                TlTaxableAmount = i.IplTaxblAmt,
                                ExciseTaxableAmount = i.ExciseTaxblAmt,
                                VatAmount = i.VatAmt,
                                IplAmount = i.IplAmt,
                                TlAmount = i.TlAmt,
                                ExciseAmount = i.ExciseTxAmt,
                                TaxableAmount = i.TaxblAmt,
                                TotalAmount = i.TotAmt
                            })
                            .ToImmutableArray()
                })
                .ToImmutableArray()
                ?? [];                
    }

    public static SavePurchaseRequest MapToSavePurchaseRequest(this PurchaseDto purchase, ZraApiOptions apiOptions)
    {
        return new SavePurchaseRequest
        {
            tpin = apiOptions.TaxpayerIdentificationNumber,
            bhfId = apiOptions.TaxpayerBranchIdentifier,
            InvcNo = purchase.SupplierInvoiceNumber.ToString(),
            SpplrTpin = purchase.SupplierTaxPayerIdentifier,
            SpplrBhfId = purchase.SupplierBranchIdentifier,
            SpplrNm = purchase.SupplierName,
            SpplrInvcNo = purchase.SupplierInvoiceNumber,
            RegTyCd = purchase.RecieptTypeCode,
            PchsTyCd = "N",
            RcptTyCd = "P",
            PmtTyCd = "01",
            PchsSttsCd = "02",
            CfmDt = DateTime.Now.ToString("yyyyMMddHHmmss"),
            PchsDt = purchase.SalesDate,
            TotItemCnt = purchase.TotalItemCount,
            TotTaxblAmt = purchase.TotalTaxableAmount,
            TotTaxAmt = purchase.TotalTaxAmount,
            TotAmt = purchase.TotalAmount,
            Remark = purchase.Remark,
            RegrNm = "Admin",
            RegrId = "Admin",
            ModrNm = "Admin",
            ModrId = "Admin",
            ItemList =
                    purchase
                        .Items
                        .Select(x => new SavePurchaseRequest.PurchaseItem
                        {
                            ItemSeq = x.SequenceNumber,
                            ItemCd = x.SupplierItemCode,
                            ItemClsCd = x.ClassificationCode,
                            ItemNm = x.Name,
                            Bcd = x.Barcode,
                            PkgUnitCd = x.PackagingUnitCode,
                            Pkg = x.Package,
                            QtyUnitCd = x.QuantityUnitCode ?? "",
                            Qty = x.Quantity,
                            Prc = x.InclusiveUnitPrice,
                            SplyAmt = x.SupplyAmount,
                            DcRt = x.DiscountRate,
                            DcAmt = x.DiscountAmount,
                            TaxTyCd = x.VatCategoryCode,
                            IplCatCd = x.IplCategoryCode,
                            TlCatCd = x.TlCategoryCode,
                            ExciseCatCd = x.ExciseCategoryCode,
                            TaxblAmt = x.VatTaxableAmount,
                            VatCatCd = x.VatCategoryCode,
                            IplTaxblAmt = x.IplTaxableAmount,
                            TlTaxblAmt = x.TlTaxableAmount,
                            ExciseTaxblAmt = x.ExciseTaxableAmount,
                            TaxAmt = x.TaxAmount,
                            IplAmt = x.IplAmount,
                            TlAmt = x.TlAmount,
                            ExciseTxAmt = x.ExciseAmount,
                            TotAmt = x.TotalAmount
                        })
                        .ToImmutableArray()
        };
    }
}
