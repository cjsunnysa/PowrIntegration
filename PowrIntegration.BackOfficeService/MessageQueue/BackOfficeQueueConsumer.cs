using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PowrIntegration.BackOfficeService.Data;
using PowrIntegration.BackOfficeService.Data.Entities;
using PowrIntegration.BackOfficeService.Mapping;
using PowrIntegration.BackOfficeService.Powertill;
using PowrIntegration.Shared.Dtos;
using PowrIntegration.Shared.Extensions;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegration.Shared.Observability;
using PowrIntegration.Shared.Options;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace PowrIntegration.BackOfficeService.MessageQueue;

internal sealed class BackOfficeQueueConsumer(
    IChannel channel,
    MessageQueueOptions options,
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
    PurchaseFileExport purchaseExport,
    IMetrics metrics,
    ILogger<BackOfficeQueueConsumer> logger) : RabbitMqConsumer(channel, options, metrics.MetricsMeterName, logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly PurchaseFileExport _purchaseExport = purchaseExport;
    private readonly ILogger<BackOfficeQueueConsumer> _logger = logger;
    private readonly JsonSerializerOptions _pluSerializerOptions = new JsonSerializerOptions
    {
        Converters = { new PluItemDto.DateTimePowertillConverter() }
    };

    protected async override Task<MessageAction> HandleMessage(QueueMessageType messageType, ReadOnlyMemory<byte> messageBody, CancellationToken cancellationToken)
    {
        var result = messageType switch
        {
            QueueMessageType.ZraStandardCodes => await HandleStandardCodeMessage(messageBody, cancellationToken),
            QueueMessageType.ZraClassificationCodes => await HandleClassificationCodeMessage(messageBody, cancellationToken),
            QueueMessageType.ZraImportItems => await HandleZraImportItemsMessage(messageBody, cancellationToken),
            QueueMessageType.Purchase => await HandlePurchaseMessage(messageBody, cancellationToken),
            QueueMessageType.ItemInsert => await HandleItemInsertMessage(messageBody, cancellationToken),
            QueueMessageType.ItemUpdate => await HandleItemUpdateMessage(messageBody, cancellationToken),
            _ => Result.Fail($"Unkown message type: {messageType}.")
        };

        result.LogErrors(_logger);

        return
            result.IsSuccess
            ? MessageAction.Acknowledge
            : MessageAction.Reject;
    }

    private async Task<Result> HandleStandardCodeMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received import standard codes message.");

        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<StandardCodeClassDto>>(stream, cancellationToken: cancellationToken);

        var entities = dtos.ToEntities();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.BulkInsertOrUpdateAsync(entities, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandleClassificationCodeMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received import classification codes message.");

        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ClassificationCodeDto>>(stream, cancellationToken: cancellationToken);

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var segments =
            dtos
                .Where(x => x.Level == (int) ClassificationCodeDto.ClassificationLevel.Segment)
                .ToImmutableArray();

        await UpdateSegments(dbContext, segments, cancellationToken);

        var familyCodes =
            dtos
                .Where(x => x.Level == (int) ClassificationCodeDto.ClassificationLevel.Family)
                .ToImmutableArray();

        await UpdateFamilies(dbContext, familyCodes, cancellationToken);

        var classCodes =
            dtos
                .Where(x => x.Level == (int) ClassificationCodeDto.ClassificationLevel.Class)
                .ToImmutableArray();

        await UpdateClasses(dbContext, classCodes, cancellationToken);

        var commodityCodes =
            dtos
                .Where(x => x.Level == (int) ClassificationCodeDto.ClassificationLevel.Commodity)
                .ToImmutableArray();

        await UpdateClassificationCodes(dbContext, commodityCodes, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task UpdateSegments(PowrIntegrationDbContext dbContext, ImmutableArray<ClassificationCodeDto> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            var existingRecord = await dbContext.ZraClassificationSegments.FindAsync([long.Parse(record.Code!)], cancellationToken);

            if (existingRecord is null)
            {
                _logger.LogWarning("Existing classification segment not found for code: {SegmentCode}.", record.Code);

                continue;
            }

            if (record.Name is not null)
            {
                existingRecord.Name = record.Name;
            }
        }
    }

    private async Task UpdateFamilies(PowrIntegrationDbContext dbContext, ImmutableArray<ClassificationCodeDto> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            var existingRecord = await dbContext.ZraClassificationFamilies.FindAsync([long.Parse(record.Code!)], cancellationToken);

            if (existingRecord is null)
            {
                _logger.LogWarning("Existing classification family not found for code: {FamilyCode}.", record.Code);

                continue;
            }

            if (record.Name is not null)
            {
                existingRecord.Name = record.Name;
            }
        }
    }

    private async Task UpdateClasses(PowrIntegrationDbContext dbContext, ImmutableArray<ClassificationCodeDto> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            var existingRecord = await dbContext.ZraClassificationClasses.FindAsync([long.Parse(record.Code!)], cancellationToken);

            if (existingRecord is null)
            {
                _logger.LogWarning("Existing classification class not found for code: {ClassCode}.", record.Code);

                continue;
            }

            if (record.Name is not null)
            {
                existingRecord.Name = record.Name;
            }
        }
    }

    private async Task UpdateClassificationCodes(PowrIntegrationDbContext dbContext, ImmutableArray<ClassificationCodeDto> records, CancellationToken cancellationToken)
    {
        foreach (var record in records)
        {
            var existingRecord = await dbContext.ZraClassificationCodes.FindAsync([long.Parse(record.Code!)], cancellationToken);

            if (existingRecord is null)
            {
                _logger.LogWarning("Existing classification code not found for code: {ClassificationCode}.", record.Code);

                continue;
            }

            if (record.Name is not null)
            {
                existingRecord.Name = record.Name;
            }

            if (record.TaxTypeCode is not null)
            {
                existingRecord.TaxTypeCode = record.TaxTypeCode;
            }

            if (record.IsMajorTarget.HasValue)
            {
                existingRecord.IsMajorTarget = record.IsMajorTarget.Value;
            }

            if (record.ShouldUse.HasValue)
            {
                existingRecord.ShouldUse = record.ShouldUse.Value;
            }
        }
    }

    private async Task<Result> HandleZraImportItemsMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received import ZRA Import Items message.");

        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ImportItemDto>>(stream, cancellationToken: cancellationToken);

        var zraImportItems = dtos.ToEntities();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.BulkInsertOrUpdateAsync(zraImportItems, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandlePurchaseMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        PurchaseDto? dto = await JsonSerializer.DeserializeAsync<PurchaseDto>(stream, cancellationToken: cancellationToken);

        if (dto is null)
        {
            var bodyString = Encoding.UTF8.GetString(stream.ToArray());

            return Result.Fail($"Cannot deserialize message. Message body is not a valid purchase. Body: {bodyString}");
        }

        _logger.LogInformation("Received purchase message for SupplierTaxPayerIdentifier: {SupplierTaxPayerIdentifier} SupplierInvoiceNumber: {SupplierInvoiceNumber}.", dto.SupplierTaxPayerIdentifier, dto.SupplierInvoiceNumber);

        return await _purchaseExport.Execute(dto, cancellationToken);
    }

    private async Task<Result> HandleItemInsertMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var desrializeStream = new MemoryStream(body.ToArray());

        var dto = await JsonSerializer.DeserializeAsync<PluItemDto>(desrializeStream, options: _pluSerializerOptions, cancellationToken: cancellationToken);

        if (dto is null)
        {
            var bodyString = Encoding.UTF8.GetString(desrializeStream.ToArray());

            return Result.Fail($"Cannot deserialize message. Message body is not a valid item. Body: {bodyString}");
        }

        _logger.LogInformation("Received item insert message for PluNumber: {PluNumber}.", dto.PluNumber);

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = dto.ToEntity();

        await dbContext.PluItems.AddAsync(entity, cancellationToken);

        using var serializeStream = new MemoryStream();

        await JsonSerializer.SerializeAsync(serializeStream, dto, cancellationToken: cancellationToken);

        var outboxRecord = new OutboxItem
        {
            MessageType = QueueMessageType.ItemInsert,
            MessageBody = serializeStream.ToArray()
        };

        await dbContext.OutboxItems.AddAsync(outboxRecord, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandleItemUpdateMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var deserialzeStream = new MemoryStream(body.ToArray());

        var dto = await JsonSerializer.DeserializeAsync<PluItemDto>(deserialzeStream, options: _pluSerializerOptions, cancellationToken: cancellationToken);

        if (dto is null)
        {
            var bodyString = Encoding.UTF8.GetString(deserialzeStream.ToArray());

            return Result.Fail($"Cannot deserialize message. Message body is not a valid item. Body: {bodyString}");
        }

        _logger.LogInformation("Received item update message for PluNumber: {PluNumber}.", dto.PluNumber);

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        PluItem? plu = await dbContext.PluItems.FindAsync([dto.PluNumber], cancellationToken);

        if (plu is null)
        {
            var bodyString = Encoding.UTF8.GetString(deserialzeStream.ToArray());

            return Result.Fail($"Cannot find item for PluNumber: {dto.PluNumber}. Body: {bodyString}");
        }

        plu.PluDescription = dto.PluDescription;
        plu.SizeDescription = dto.SizeDescription;
        plu.SalesGroup = dto.SalesGroup;
        plu.SellingPrice1 = dto.SellingPrice1;
        plu.Flags = dto.Flags;
        plu.Supplier1StockCode = dto.Supplier1StockCode;
        plu.Supplier2StockCode = dto.Supplier2StockCode;
        plu.DateTimeCreated = dto.DateTimeCreated;
        plu.DateTimeEdited = dto.DateTimeEdited;

        using var serializeStram = new MemoryStream();

        await JsonSerializer.SerializeAsync(serializeStram, dto, cancellationToken: cancellationToken);

        var outboxRecord = new OutboxItem
        {
            MessageType = QueueMessageType.ItemUpdate,
            MessageBody = serializeStram.ToArray()
        };

        await dbContext.OutboxItems.AddAsync(outboxRecord, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}