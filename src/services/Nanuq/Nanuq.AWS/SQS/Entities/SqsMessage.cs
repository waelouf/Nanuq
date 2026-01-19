namespace Nanuq.AWS.SQS.Entities;

/// <summary>
/// Represents an SQS message
/// </summary>
/// <param name="MessageId">Unique message identifier</param>
/// <param name="Body">Message body content</param>
/// <param name="ReceiptHandle">Receipt handle for deleting the message</param>
/// <param name="Attributes">Message system attributes</param>
/// <param name="MessageAttributes">Custom message attributes</param>
public record SqsMessage(
    string MessageId,
    string Body,
    string ReceiptHandle,
    Dictionary<string, string> Attributes,
    Dictionary<string, Amazon.SQS.Model.MessageAttributeValue> MessageAttributes);
