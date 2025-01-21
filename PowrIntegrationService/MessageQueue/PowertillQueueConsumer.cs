using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Data.Entities;
using PowrIntegrationService.Data;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Dtos;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.Options;
using RabbitMQ.Client.Events;
using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using System.Text.Json;
using static PowrIntegrationService.MessageQueue.RabbitMqConsumer;
using static PowrIntegrationService.Zra.ZraTypes;

namespace PowrIntegrationService.MessageQueue;

public sealed class PowertillQueueConsumer
{
    private readonly PowertillOptions _options;
    private readonly RabbitMqFactory _factory;
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory;
    private readonly ILogger<PowertillQueueConsumer> _logger;
    private readonly Counter<long> _messagesConsumerCounter;
    private RabbitMqConsumer? _consumer;

    public PowertillQueueConsumer(
        IOptions<PowertillOptions> options,
        RabbitMqFactory factory,
        IDbContextFactory<PowrIntegrationDbContext> dbContextFactory,
        ILogger<PowertillQueueConsumer> logger)
    {
        _options = options.Value;
        _factory = factory;
        _dbContextFactory = dbContextFactory;
        _logger = logger;

        var meter = new Meter(PowrIntegrationValues.MetricsMeterName);
        _messagesConsumerCounter = meter.CreateCounter<long>("powertill_queue_messages_consumed", "messages", "Number of messages consumed.");
    }

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
                QueueMessageType.IngredientInsert or QueueMessageType.IngredientUpdate => await HandleIngredientMessage(args.Body, cancellationToken),
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
        _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "standard_codes"));

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
        _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "classification_codes"));

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

            _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "add_plu_item"));
        }
        else
        {
            existingPlu.UpdateFrom(dto);

            _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "update_plu_item"));
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
        _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "zra_imported_items"));

        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<ImportItemDto>>(stream, cancellationToken: cancellationToken);

        var zraImportItems = dtos.MapToEntities();

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.BulkInsertOrUpdateAsync(zraImportItems, cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task<Result> HandleIngredientMessage(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(body.ToArray());

        var dtos = await JsonSerializer.DeserializeAsync<ImmutableArray<IngredientDto>>(stream, cancellationToken: cancellationToken);

        var header = dtos.FirstOrDefault(x => x.IsHeader);

        if (header is null)
        {
            return Result.Fail("Ingredient records are missing a header.");
        }

        var recipe = header.ToRecipe();

        var ingredients = dtos.Where(x => !x.IsHeader).ToIngredient(header);

        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken: cancellationToken);

        var existingRecipe = await dbContext.Recipes.FindAsync([recipe.PluNumber], cancellationToken: cancellationToken);

        if (existingRecipe is null)
        {
            recipe.Ingredients = [.. ingredients];

            dbContext.Recipes.Add(recipe);

            _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "add_ingredient_item"));
        }
        else
        {
            existingRecipe.Portions = recipe.Portions;
            existingRecipe.Ingredients = [.. ingredients];

            _messagesConsumerCounter.Add(1, KeyValuePair.Create<string, object?>("type", "update_ingredient_item"));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
