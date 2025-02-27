// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Common.Application.Interfaces;
using GraphQL;
using Common.Domain.Constants;

namespace Common.Infrastructure;

/// <summary>
/// Represents the implementation of the identity service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IdentityService"/> class.
/// </remarks>
/// <param name="graphQLClient">The GraphQL client factory.</param>
public sealed class IdentityService(IGraphQLClientFactory graphQLClient) 
    : GraphQLService(graphQLClient.CreateClient(Clients.Identity)), IIdentityService
{

    /// <summary>
    /// Retrieves the username associated with the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The username.</returns>
    public Task<string> GetUserNameAsync(Guid userId, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                    query($userId: UUID!) {
                        userName(query: { userId: $userId })
                    }",
            Variables = new { userId }
        };
        return QueryAsync<string>(request, token);
    }

    /// <summary>
    /// Checks if the specified user is authorized to perform the specified action on the specified resource.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="action">The action.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the user is authorized, otherwise false.</returns>
    public Task<bool> AuthorizeAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                    query($action: String!, $resource: String!, $userId: UUID!) {
                        authorize(query: { action: $action, resource: $resource, userId: $userId })
                    }",
            Variables = new { userId, resource, action }
        };
        return QueryAsync<bool>(request, token);
    }

    /// <summary>
    /// Checks if the specified user is in the specified role for the specified resource and action.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="action">The action.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the user is in the role, otherwise false.</returns>
    public Task<bool> IsInRoleAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                    query($action: String!, $resource: String!, $userId: UUID!) {
                        isInRole(query: { action: $action, resource: $resource, userId: $userId })
                    }",
            Variables = new { userId, resource, action }
        };
        return QueryAsync<bool>(request, token);
    }

    public Task<bool> IsValidServiceAsync(string? client, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = @"
                    query($client: String!) {
                        isValidService(query: { client: $client })
                    }",
            Variables = new { client }
        };
        return QueryAsync<bool>(request, token);
    }
}
