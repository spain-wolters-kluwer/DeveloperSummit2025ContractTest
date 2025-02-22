using Microsoft.Extensions.Configuration;
using PactNet.Verifier;

namespace DevSummit.Commons.Pact.Pact.Extensions;
public static class PactVerifierExtensions
{
    public static IPactVerifierSource WithPactFromConfiguration(this IPactVerifier pactVerifier, string providerName, string consumerName, IConfiguration configuration)
    {
        return pactVerifier.WithPactBrokerSource(new Uri(configuration["PactBrokerUrl"]),
            options =>
            {
                // Authentication
                var value = configuration["PactBrokerUserName"];
                if (!string.IsNullOrEmpty(value))
                    options.BasicAuthentication(configuration["PactBrokerUserName"], configuration["PactBrokerPassword"]);

                if (!string.IsNullOrEmpty(configuration["PactBrokerUserName"]))
                    options.BasicAuthentication(configuration["PactBrokerUserName"], configuration["PactBrokerPassword"]);

                options.PublishResults(configuration[$"{providerName}AssemblyVersion"], (options) =>
                {
                    if (!string.IsNullOrEmpty(configuration[$"{providerName}PipelineUrl"]))
                        options.BuildUri(new Uri(configuration[$"{providerName}PipelineUrl"]));
                    options.ProviderBranch(configuration[$"{providerName}Branch"]);
                    options.ProviderTags([configuration[$"{providerName}EnvironmentTag"]]);
                });

                options.ConsumerVersionSelectors(
                    new ConsumerVersionSelector { Consumer = consumerName, Latest = true },
                    new ConsumerVersionSelector { Consumer = consumerName, Deployed = true }
                );
            }
        );
    }
}