// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Polly;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Resilience level for an inter-service GraphQL HttpClient. GraphQL always travels as POST,
/// so HTTP-method-based idempotency detection cannot distinguish queries from mutations:
/// clients that carry mutations must not retry.
/// </summary>
public enum GraphQLClientResilience
{
    /// <summary>No resilience pipeline (bare named client with timeout).</summary>
    None,

    /// <summary>Timeout + circuit breaker, retries disabled. Safe default for clients that carry mutations.</summary>
    NoRetry,

    /// <summary>Full standard pipeline including retries. Only for clients that exclusively run queries.</summary>
    WithRetry
}

/// <summary>
/// Single registration point for inter-service GraphQL HttpClients
/// (consumed through <c>IGraphQLClientFactory.CreateClient</c>). Guarantees every client gets
/// the same timeout, Authorization/x-correlation-id propagation, and an explicit resilience choice.
/// </summary>
public static class GraphQLClientServiceCollectionExtensions
{
    // Marker so header-propagation options are configured once per host even when several
    // Infrastructure projects register clients.
    private sealed class TrackHubHeaderPropagationConfigured { }

    /// <summary>
    /// Registers the named HttpClient for a user-token (header-propagating) GraphQL client.
    /// </summary>
    public static IHttpClientBuilder AddGraphQLClient(
        this IServiceCollection services,
        string name,
        bool propagateHeaders = true,
        GraphQLClientResilience resilience = GraphQLClientResilience.NoRetry,
        int timeoutSeconds = 30)
    {
        if (propagateHeaders)
        {
            services.AddTrackHubHeaderPropagation();
        }

        var builder = services.AddHttpClient(name,
            client => client.Timeout = TimeSpan.FromSeconds(timeoutSeconds));

        if (propagateHeaders)
        {
            builder.AddHeaderPropagation();
        }

        switch (resilience)
        {
            case GraphQLClientResilience.NoRetry:
                builder.AddStandardResilienceHandler(options =>
                    options.Retry.ShouldHandle = static _ => PredicateResult.False());
                break;
            case GraphQLClientResilience.WithRetry:
                builder.AddStandardResilienceHandler();
                break;
        }

        return builder;
    }

    /// <summary>
    /// Registers the '{name}AsService' twin used by <c>IGraphQLClientFactory.CreateClient(name, asService: true)</c>:
    /// no user-token propagation; the factory attaches the host's client-credentials identity.
    /// </summary>
    public static IHttpClientBuilder AddGraphQLServiceClient(
        this IServiceCollection services,
        string name,
        GraphQLClientResilience resilience = GraphQLClientResilience.NoRetry,
        int timeoutSeconds = 30)
        => services.AddGraphQLClient($"{name}AsService", propagateHeaders: false, resilience, timeoutSeconds);

    /// <summary>
    /// Configures which inbound headers are propagated to outbound clients (Authorization and
    /// x-correlation-id). Idempotent across the multiple Infrastructure registrations of a host.
    /// </summary>
    public static IServiceCollection AddTrackHubHeaderPropagation(this IServiceCollection services)
    {
        if (!services.Any(d => d.ServiceType == typeof(TrackHubHeaderPropagationConfigured)))
        {
            services.AddSingleton<TrackHubHeaderPropagationConfigured>();
            services.AddHeaderPropagation(options =>
            {
                options.Headers.Add("Authorization");
                options.Headers.Add("x-correlation-id");
            });
        }

        return services;
    }
}
