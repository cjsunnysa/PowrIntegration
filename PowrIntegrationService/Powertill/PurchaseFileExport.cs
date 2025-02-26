using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegrationService.Data;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.MessageQueue;
using PowrIntegrationService.Options;
using System;
using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace PowrIntegrationService.Powertill;

public sealed class PurchaseFileExport(IOptions<PowertillOptions> options, IOptions<ZraApiOptions> zraApiOptions, IDbContextFactory<PowrIntegrationDbContext> dbContextFactory)
{
    private readonly PowertillOptions _options = options.Value;
    private readonly ZraApiOptions _zraApiOptions = zraApiOptions.Value;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;

    private static class RecordType
    {
        public const string FileName = "XfrName";
        public const string Header = "XfrH";
        public const string Item = "XfrI";
    }

    public async Task<Result> Execute(PurchaseDto purchase, CancellationToken cancellationToken)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        using var serializedPurchaseStream = new MemoryStream();

        await JsonSerializer.SerializeAsync(serializedPurchaseStream, purchase, cancellationToken: cancellationToken);

        var stockCodes = purchase.Items.Select(x => x.SupplierItemCode).ToImmutableList();

        var plus =
            dbContext
                .PluItems
                .AsNoTracking()
                .Where(x => stockCodes.Contains(x.Supplier2StockCode ?? ""))
                .ToImmutableArray();

        if (plus.Length < purchase.Items.Length)
        {
            string purchaseString = Encoding.UTF8.GetString(serializedPurchaseStream.ToArray());

            return Result.Fail(new Error($"Error exporting purchase. Not all PLUs exist for all purchase items. Purchase: {purchaseString}"));
        }

        string fileName = $"{purchase.SupplierName}-{purchase.SupplierInvoiceNumber}.csa";

        string windowsFilePath = $"{_options.FileOutputDirectoryWindowsPath}\\{fileName}";

        string linuxFilePath = Path.Combine(_options.FileOutputDirectory, fileName);

        StringBuilder sb = BuildFileContents(windowsFilePath, purchase, plus);

        await System.IO.File.WriteAllTextAsync(linuxFilePath, sb.ToString(), Encoding.UTF8, cancellationToken);

        var outboxItem = new OutboxItem
        {
            MessageType = QueueMessageType.SavePurchase,
            MessageBody = serializedPurchaseStream.ToArray()
        };

        dbContext.OutboxItems.Add(outboxItem);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private StringBuilder BuildFileContents(string filePath, PurchaseDto purchase, ImmutableArray<PluItem> plus)
    {
        string supplierIdentifier = $"{purchase.SupplierTaxPayerIdentifier}{purchase.SupplierBranchIdentifier}";

        var sb = new StringBuilder();

        sb.AppendLine($"{RecordType.FileName},{filePath},XfrSrc,2,XfrDest,1,XfrRef,{purchase.SupplierInvoiceNumber},XfrNo,1");

        sb.AppendLine($"{RecordType.Header},1,{supplierIdentifier},{purchase.SupplierName},{purchase.SupplierInvoiceNumber},{purchase.SalesDate.ToPowertillDate()},{purchase.SalesDate.ToPowertillDate()},{purchase.TotalTaxableAmount},{purchase.TotalTaxAmount},0.00,0.00,0.00,0.00,{purchase.TotalAmount},0.00,,Ver8.0");

        foreach (var line in purchase.Items)
        {
            sb.AppendLine(CreatePurchaseLine(line, plus));
        }

        return sb;
    }

    private string CreatePurchaseLine(PurchaseDto.PurchaseItemDto line, ImmutableArray<PluItem> plus)
    {
        PluItem plu = plus.First(x => x.Supplier2StockCode == line.SupplierItemCode);

        int taxGroupId = GetTaxGroupFor(plu.SalesGroup);

        return $"{RecordType.Item},1,{plu.PluNumber},{line.SupplierItemCode},{plu.PluDescription},{plu.SizeDescription},0,0,0,{line.Quantity},{line.ExclusiveUnitPrice},{line.DiscountAmount},{line.TaxableAmount}, {taxGroupId}, {line.TaxAmount},{line.InclusiveUnitPrice},{line.TotalAmount},N,Ver8.0";
    }

    private int GetTaxGroupFor(int salesGroupId)
    {
        return 
            _zraApiOptions.TaxTypeMappings.FirstOrDefault(x => x.SalesGroupId == salesGroupId)?.TaxGroupId 
            ?? 1;
    }
}

file static class StringDateExtensions
{
    // return yyyyMMdd as ddMMyyyy
    public static string ToPowertillDate(this string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return dateString;
        }

        ReadOnlySpan<char> dateSpan = dateString.AsSpan();
        Span<char> result = stackalloc char[dateString.Length];

        dateSpan[^2..].CopyTo(result);
        dateSpan[4..^2].CopyTo(result);
        dateSpan[..4].CopyTo(result);

        return new string(result);
    }
}
