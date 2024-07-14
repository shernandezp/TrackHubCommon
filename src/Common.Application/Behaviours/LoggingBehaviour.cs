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

using Common.Application.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviours;

// Represents the LoggingBehaviour class.
public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user, IIdentityService identityService) : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    // Represents the Process method responsible for logging information about the incoming request.
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        // Get the name of the request type.
        var requestName = typeof(TRequest).Name;
        // Get the user ID.
        var userId = user.Id ?? string.Empty;
        // Initialize the userName variable.
        string? userName = string.Empty;

        // Check if the user ID is not empty.
        if (!string.IsNullOrEmpty(userId))
        {
            // Get the user name based on the user ID.
            userName = await identityService.GetUserNameAsync(new Guid(userId), cancellationToken);
        }

        // Log the information about the incoming request.
        logger.LogInformation("ReThinkMarket Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
        return;
    }
}
