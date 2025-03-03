using PowrIntegration.BackOfficeService.Data.Entities;
using PowrIntegration.Shared.Dtos;
using PowrIntegration.Shared.MessageQueue;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace PowrIntegration.BackOfficeService.Mapping;
public static class Mapping
{
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

    public static ImmutableArray<OutboxItem> ToOutboxItems(this ImmutableArray<PluItem> records)
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

    public static PluItem ToEntity(this PluItemDto dto)
    {
        return new PluItem
        {
            PluNumber = dto.PluNumber,
            PluDescription = dto.PluDescription,
            SizeDescription = dto.SizeDescription,
            SalesGroup = dto.SalesGroup,
            Flags = dto.Flags,
            SellingPrice1 = dto.SellingPrice1,
            Supplier1StockCode = dto.Supplier1StockCode,
            Supplier2StockCode = dto.Supplier2StockCode,
            DateTimeCreated = dto.DateTimeCreated,
            DateTimeEdited = dto.DateTimeEdited
        };
    }
}