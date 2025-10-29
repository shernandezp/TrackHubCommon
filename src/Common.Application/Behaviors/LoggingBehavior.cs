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
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging request and user information.
/// Logs the request name, user ID, user name, and the request object itself.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LoggingBehavior<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IIdentityService identityService
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the logging of the request and user information before passing control to the next behavior/handler in the pipeline.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The response from the next handler in the pipeline.</returns>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken = default)
    {
        // Get the name of the request type
        var requestName = typeof(TRequest).Name;
        // Get the user ID from the current user context
        var userId = user.Id ?? string.Empty;
        string? userName = string.Empty;

        // If a user ID is present, retrieve the user name from the identity service
        if (!string.IsNullOrEmpty(userId))
        {
            userName = await identityService.GetUserNameAsync(new Guid(userId), cancellationToken);
        }

        // Log the request details along with user information
        logger.LogInformation("TrackHub Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);

        // Call the next behavior/handler in the pipeline
        return await next();
    }
}
