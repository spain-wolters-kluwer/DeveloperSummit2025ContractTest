using DevSummit.Blog.Consumer.Tests.Pact.Broker;
using DevSummit.Commons.Pact.Pact.Broker.Entities;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DevSummit.Commons.Pact.Pact.Broker;

internal class PactBrokerContractBuilder
{
    private readonly IConfiguration configuration;
    private string? provider;
    private string? consumer;
    private string[] tags = Array.Empty<string>();

    public PactBrokerContractBuilder(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public PactBrokerPublishContract Build()
    {
        return new PactBrokerPublishContract()
        {
            PacticipantName = consumer,
            Contracts = [CreatePactContract(provider)],
            Tags = tags.Length == 0 ? null : tags,
            Branch = configuration[$"{consumer}Branch"],
            PacticipantVersionNumber = configuration[$"{consumer}AssemblyVersion"],
            BuildUrl = configuration[$"{consumer}PipelineUrl"]
        };
    }

    private PactBrokerContract CreatePactContract(string provider)
    {
        return new PactBrokerContract()
        {
            ConsumerName = consumer,
            ProviderName = provider,
            ContentType = "application/json",
            Specification = "pact",
            Content = GetContentInBase64(provider)
        };
    }

    private string GetContentInBase64(string provider)
    {
        var pactContent = PactFileHandler.GetContentFromPactFile(configuration["PactDir"], provider, consumer);
        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(pactContent);
        return Convert.ToBase64String(toEncodeAsBytes);
    }

    public PactBrokerContractBuilder WithConsumer(string? consumer)
    {
        this.consumer = consumer;
        return this;
    }

    public PactBrokerContractBuilder WithTags(string[] tags)
    {
        this.tags = tags;
        return this;
    }

    public PactBrokerContractBuilder WithProvider(string? provider)
    {
        this.provider = provider;
        return this;
    }
}
