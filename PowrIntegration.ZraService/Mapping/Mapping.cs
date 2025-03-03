using PowrIntegration.Shared.Dtos;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.ZraService.Options;
using PowrIntegration.ZraService.Zra.ClassificationCodes;
using PowrIntegration.ZraService.Zra.GetImports;
using PowrIntegration.ZraService.Zra.GetPurchases;
using PowrIntegration.ZraService.Zra.SaveItem;
using PowrIntegration.ZraService.Zra.SavePurchase;
using PowrIntegration.ZraService.Zra.UpdateItem;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using static PowrIntegration.ZraService.Zra.StandardCodes.FetchStandardCodesResponse;

namespace PowrIntegration.ZraService.Mapping;

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

    private static string GetTaxTypeCode(TaxMapping[] taxTypes, int salesGroup)
    {
        return
            taxTypes
                .FirstOrDefault(x => x.SalesGroupId == salesGroup)
                ?.TaxTypeCode
                ?? SaveItemRequest.TaxTypeCode.StandardRated;
    }

    public static ImmutableArray<PurchaseDto> MapToDtos(this GetPurchasesResponse response)
    {
        return
            response
                .data
                ?.saleList
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
                                SequenceNumber = i.itemSeq,
                                SupplierItemCode = i.itemCd,
                                ClassificationCode = i.itemClsCd,
                                Name = i.itemNm,
                                Barcode = i.bcd,
                                PackagingUnitCode = i.pkgUnitCd,
                                Package = i.pkg,
                                QuantityUnitCode = i.qtyUnitCd,
                                Quantity = i.qty,
                                InclusiveUnitPrice = i.prc,
                                SupplyAmount = i.splyAmt,
                                DiscountRate = i.dcRt,
                                DiscountAmount = i.dcAmt,
                                VatCategoryCode = i.vatCatCd,
                                IplCategoryCode = i.iplCatCd,
                                TlCategoryCode = i.tlCatCd,
                                ExciseCategoryCode = i.exciseTxCatC,
                                VatTaxableAmount = i.vatTaxblAmt,
                                IplTaxableAmount = i.iplTaxblAmt,
                                TlTaxableAmount = i.iplTaxblAmt,
                                ExciseTaxableAmount = i.exciseTaxblAmt,
                                VatAmount = i.vatAmt,
                                IplAmount = i.iplAmt,
                                TlAmount = i.tlAmt,
                                ExciseAmount = i.exciseTxAmt,
                                TaxableAmount = i.taxblAmt,
                                TotalAmount = i.totAmt
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
            invcNo = purchase.SupplierInvoiceNumber.ToString(),
            spplrTpin = purchase.SupplierTaxPayerIdentifier,
            spplrBhfId = purchase.SupplierBranchIdentifier,
            spplrNm = purchase.SupplierName,
            spplrInvcNo = purchase.SupplierInvoiceNumber,
            regTyCd = purchase.RecieptTypeCode,
            pchsTyCd = "N",
            rcptTyCd = "P",
            pmtTyCd = "01",
            pchsSttsCd = "02",
            cfmDt = DateTime.Now.ToString("yyyyMMddHHmmss"),
            pchsDt = purchase.SalesDate,
            totItemCnt = purchase.TotalItemCount,
            totTaxblAmt = purchase.TotalTaxableAmount,
            totTaxAmt = purchase.TotalTaxAmount,
            totAmt = purchase.TotalAmount,
            remark = purchase.Remark,
            regrNm = "Admin",
            regrId = "Admin",
            modrNm = "Admin",
            modrId = "Admin",
            itemList =
                    purchase
                        .Items
                        .Select(x => new SavePurchaseRequest.PurchaseItem
                        {
                            itemSeq = x.SequenceNumber,
                            itemCd = x.SupplierItemCode,
                            itemClsCd = x.ClassificationCode,
                            itemNm = x.Name,
                            bcd = x.Barcode,
                            pkgUnitCd = x.PackagingUnitCode,
                            pkg = x.Package,
                            qtyUnitCd = x.QuantityUnitCode ?? "",
                            qty = x.Quantity,
                            prc = x.InclusiveUnitPrice,
                            splyAmt = x.SupplyAmount,
                            dcRt = x.DiscountRate,
                            dcAmt = x.DiscountAmount,
                            taxTyCd = x.VatCategoryCode,
                            iplCatCd = x.IplCategoryCode,
                            tlCatCd = x.TlCategoryCode,
                            exciseCatCd = x.ExciseCategoryCode,
                            taxblAmt = x.VatTaxableAmount,
                            vatCatCd = x.VatCategoryCode,
                            iplTaxblAmt = x.IplTaxableAmount,
                            tlTaxblAmt = x.TlTaxableAmount,
                            exciseTaxblAmt = x.ExciseTaxableAmount,
                            taxAmt = x.TaxAmount,
                            iplAmt = x.IplAmount,
                            tlAmt = x.TlAmount,
                            exciseTxAmt = x.ExciseAmount,
                            totAmt = x.TotalAmount
                        })
                        .ToImmutableArray()
        };
    }
}
