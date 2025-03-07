﻿using FluentResults;
using PowrIntegration.Shared.Extensions;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PowrIntegration.Shared.MessageQueue;

public static class RabbitMqHelpers
{
    public static Result<QueueMessageType> GetQueueMessageType(this BasicDeliverEventArgs args)
    {
        object? typeObject = null;

        if (!args.BasicProperties.Headers?.TryGetValue(MessageQueueHeaderKey.Type, out typeObject) ?? false)
        {
            return Result.Fail($"Invalid message found in the Sync queue. Header '{MessageQueueHeaderKey.Type}' is missing.");
        }

        if (typeObject is not byte[] typeBytes)
        {
            return ProcessMessageTypeError(args);
        }

        string typeString = Encoding.UTF8.GetString(typeBytes);

        return
            !Enum.TryParse(typeString, out QueueMessageType messageType)
            ? ProcessMessageTypeError(args)
            : Result.Ok(messageType);
    }

    private static Result<QueueMessageType> ProcessMessageTypeError(BasicDeliverEventArgs args)
    {
        using var stream = new MemoryStream();

        JsonSerializer.Serialize(stream, args);

        var serializedString = Encoding.UTF8.GetString(stream.ToArray());

        return Result.Fail($"Invalid message found in the Sync queue. Header '{MessageQueueHeaderKey.Type}' is not a valid message type. message: {serializedString}");
    }

    public static string ToLabel(this QueueMessageType messageType)
    {
        var typeAsString = Enum.GetName(messageType);

        if (typeAsString is null)
        {
            return "unkown";
        }

        return typeAsString.ToSnakeCase();
    }
}
