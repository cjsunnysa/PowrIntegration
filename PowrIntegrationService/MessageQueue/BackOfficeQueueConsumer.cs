using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using PowrIntegrationService.Data;
using PowrIntegrationService.Data.Entities;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using RabbitMQ.Client;
using System.Collections.Immutable;
using System.Text.Json;
using static PowrIntegrationService.Zra.ZraTypes;

namespace PowrIntegrationService.MessageQueue;

public sealed class BackOfficeQueueConsumer(
    IChannel channel,
    MessageQueueOptions options,
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
    ILogger<BackOfficeQueueConsumer> logger) : RabbitMqConsumer(channel, options, logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<BackOfficeQueueConsumer> _logger = logger;

    protected async override Task<MessageAction> HandleMessage(QueueMessageType messageType, ReadOnlyMemory<byte> messageBody, CancellationToken cancellationToken)
    {
        var result = messageType switch
        {
            QueueMessageType.ZraStandardCodes => await HandleStandardCodeMessage(messageBody, cancellationToken),
            QueueMessageType.ZraClassificationCodes => await HandleClassificationCodeMessage(messageBody, cancellationToken),
            QueueMessageType.ZraImportItems => await HandleZraImportItemsMessage(messageBody, cancellationToken),
            _ => Result.Fail($"Unkown message type: {(int)messageType}.")
        };

        result.LogErrors(_logger);

        return
            result.IsSuccess
            ? MessageAction.Acknowledge
            : MessageAction.Reject;
    }

    private async Task<Result> HandleStandardCodeMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
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
        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ClassificationCodeDto>>(stream, cancellationToken: cancellationToken);

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var segments =
            dtos
                .Where(x => x.Level == (int)ClassificationLevel.Segment)
                .ToImmutableArray();

        await UpdateSegments(dbContext, segments, cancellationToken);

        var familyCodes =
            dtos
                .Where(x => x.Level == (int)ClassificationLevel.Family)
                .ToImmutableArray();

        await UpdateFamilies(dbContext, familyCodes, cancellationToken);

        var classCodes =
            dtos
                .Where(x => x.Level == (int)ClassificationLevel.Class)
                .ToImmutableArray();

        await UpdateClasses(dbContext, classCodes, cancellationToken);

        var commodityCodes =
            dtos
                .Where(x => x.Level == (int)ClassificationLevel.Commodity)
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
        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ImportItemDto>>(stream, cancellationToken: cancellationToken);

        var zraImportItems = dtos.ToEntities();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.BulkInsertOrUpdateAsync(zraImportItems, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}