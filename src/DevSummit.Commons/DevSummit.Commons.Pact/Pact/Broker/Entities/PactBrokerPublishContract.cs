namespace DevSummit.Commons.Pact.Pact.Broker.Entities;

class PactBrokerPublishContract
{
    public string? PacticipantName { get; set; }
    public string? PacticipantVersionNumber { get; set; }
    public string? Branch { get; set; }
    public string[]? Tags { get; set; }
    public string? BuildUrl { get; set; }
    public List<PactBrokerContract>? Contracts { get; set; }
}
