namespace Nanuq.AWS.SNS.Requests;

/// <summary>
/// Request to subscribe an endpoint to an SNS topic
/// </summary>
/// <param name="Region">AWS region</param>
/// <param name="TopicArn">Topic ARN</param>
/// <param name="Protocol">Protocol type (email, email-json, sms, http, https, sqs, lambda, application, firehose)</param>
/// <param name="Endpoint">Endpoint address (email, URL, queue ARN, lambda ARN, etc.)</param>
public record SubscribeRequest(
    string Region,
    string TopicArn,
    string Protocol,
    string Endpoint);
