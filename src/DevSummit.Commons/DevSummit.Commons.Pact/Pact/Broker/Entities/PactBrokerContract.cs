namespace DevSummit.Commons.Pact.Pact.Broker.Entities;
public class PactBrokerContract
{
    public string? ConsumerName { get; set; }
    public string? ProviderName { get; set; }
    public string? Specification { get; set; }
    public string? ContentType { get; set; }
    public string? Content { get; set; }
}