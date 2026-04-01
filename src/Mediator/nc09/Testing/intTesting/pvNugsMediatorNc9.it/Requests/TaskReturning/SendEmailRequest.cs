using pvNugsMediatorNc9Abstractions.Mediator;

namespace pvNugsMediatorNc9.it.Requests.TaskReturning;

/// <summary>
/// Request to send an email - using MediatR-style Task-returning handler
/// </summary>
public class SendEmailRequest : IRequest
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}

