namespace Nanuq.AWS.SNS.Entities;

/// <summary>
/// Represents an SNS subscription
/// </summary>
/// <param name="SubscriptionArn">ARN of the subscription</param>
/// <param name="Protocol">Protocol type (email, sms, http, https, sqs, lambda, etc.)</param>
/// <param name="Endpoint">Endpoint address (email, URL, queue ARN, etc.)</param>
/// <param name="Owner">AWS account ID of the subscription owner</param>
/// <param name="Status">Status of the subscription (PendingConfirmation, Confirmed)</param>
public record SnsSubscription(
    string SubscriptionArn,
    string Protocol,
    string Endpoint,
    string Owner,
    string Status);
