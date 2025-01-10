using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Data;
using PowrIntegration.Data.Entities;
using PowrIntegration.Dtos;
using PowrIntegration.Extensions;
using PowrIntegration.Options;
using RabbitMQ.Client.Events;
using System.Collections.Immutable;
using System.Text.Json;
using static PowrIntegration.MessageQueue.RabbitMqConsumer;
using static PowrIntegration.Zra.ZraTypes;

namespace PowrIntegration.MessageQueue;

public sealed class PowertillQueueConsumer(
    IOptions<PowertillOptions> options,
    RabbitMqFactory factory,
    IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
    ILogger<PowertillQueueConsumer> logger)
{
    private readonly PowertillOptions _options = options.Value;
    private readonly RabbitMqFactory _factory = factory;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<PowertillQueueConsumer> _logger = logger;
    private RabbitMqConsumer? _consumer;

    public async Task<Result> Start(CancellationToken cancellationToken)
    {
        try
        {
            _consumer = await _factory.CreateConsumer(_options.QueueHost, _options.QueueName, HandleMessage, cancellationToken);

            await _consumer.Start(cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private async Task<MessageAction> HandleMessage(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            Result<QueueMessageType> messageTypeResult = args.GetQueueMessageType();

            if (messageTypeResult.IsFailed)
            {
                messageTypeResult.LogErrors(_logger);

                return MessageAction.Reject;
            }

            var messageType = messageTypeResult.Value;

            var result = messageType switch
            {
                QueueMessageType.StandardCodes => await HandleStandardCodeMessage(args.Body, cancellationToken),
                QueueMessageType.ClassificationCodes => await HandleClassificationCodeMessage(args.Body, cancellationToken),
                QueueMessageType.ZraImportItems => await HandleZraImportItemsMessage(args.Body, cancellationToken),
                QueueMessageType.ItemInsert or QueueMessageType.ItemUpdate => await HandleItemMessage(args.Body, cancellationToken),
                _ => Result.Fail($"Unkown message type: {(int)messageType}.")
            };

            result.LogErrors(_logger);

            return
                result.IsSuccess
                ? MessageAction.Acknowledge
                : MessageAction.Reject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred processing message: {MessageId} from queue: {QueueName}.", args.BasicProperties.MessageId, _options.QueueName);

            return MessageAction.Reject;
        }
    }

    private async Task<Result> HandleStandardCodeMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<StandardCodeClassDto>>(stream, cancellationToken: cancellationToken);

        var entities = dtos.MapToEntities();

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

    private async Task<Result> HandleItemMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var dto = await JsonSerializer.DeserializeAsync<PluItemDto>(stream, cancellationToken: cancellationToken);

        if (dto is null)
        {
            return Result.Fail("Message does not contain a valid PLU item record.");
        }

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingPlu = await dbContext.PluItems.FindAsync([dto.PluNumber], cancellationToken);

        if (existingPlu is null)
        {
            var entity = dto.MapToEntity();

            await dbContext.PluItems.AddAsync(entity, cancellationToken);
        }
        else
        {
            existingPlu.UpdateFrom(dto);
        }

        var outboxRecord = new OutboxItem
        {
            MessageType = existingPlu is null ? QueueMessageType.ItemInsert : QueueMessageType.ItemUpdate,
            MessageBody = body.ToArray()
        };

        await dbContext.OutboxItems.AddAsync(outboxRecord, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandleZraImportItemsMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ImportItemDto>>(stream, cancellationToken: cancellationToken);

        var zraImportItems = dtos.MapToEntities();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.BulkInsertOrUpdateAsync(zraImportItems, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
