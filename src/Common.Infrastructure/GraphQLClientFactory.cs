// Copyright (c) 2024 Sergio Hernandez. All rights reserved.
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

using Ardalis.GuardClauses;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Common.Application.Interfaces;

namespace Common.Infrastructure;
public sealed class GraphQLClientFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IGraphQLClientFactory
{
    public IGraphQLClient CreateClient(string name)
    {
        var httpClient = httpClientFactory.CreateClient(name);
        var url = configuration.GetValue<string>($"AppSettings:GraphQL{name}Service");
        Guard.Against.Null(url, message: $"Setting 'GraphQL{name}Service' not found.");

        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(url)
        };
        var jsonSerializer = new SystemTextJsonSerializer();
        return new GraphQLHttpClient(options, jsonSerializer, httpClient);
    }
}
