using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.MessageQueue;
using PowrIntegrationService.Options;
using PowrIntegrationService.Zra.ClassificationCodes;
using PowrIntegrationService.Zra.GetImports;
using PowrIntegrationService.Zra.SaveItem;
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
}
