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

using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;

namespace Common.Application.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse>(
    IUser user,
    IIdentityService identityService) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    // Handles the request by performing authorization checks.
    // If the user is not authorized, it throws an UnauthorizedAccessException or ForbiddenAccessException.
    // If the user is authorized or authorization is not required, it proceeds to the next handler in the pipeline.
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get the AuthorizeAttributes applied to the request type
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            // Check if the user is authenticated
            if (user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (user.Id == "service")
            {
                var validService = await identityService.IsValidServiceAsync(user.Client, cancellationToken);
                //Extend the service validation to check roles and policies
                if (!validService)
                {
                    throw new UnauthorizedAccessException();
                }
            }
            else
            {
                // Iterate through each AuthorizeAttribute
                foreach (var attribute in authorizeAttributes)
                {
                    var resource = attribute.Resource;
                    var action = attribute.Action;

                    // Check if the user is in the required role
                    var roleAuthorized = await identityService.IsInRoleAsync(new Guid(user.Id), resource, action, cancellationToken);

                    // Check if the user is authorized based on policies
                    var policyAuthorized = await identityService.AuthorizeAsync(new Guid(user.Id), resource, action, cancellationToken);

                    // If the user is not authorized, throw ForbiddenAccessException
                    if (!roleAuthorized || !policyAuthorized)
                    {
                        throw new ForbiddenAccessException();
                    }
                }
            }
        }

        // User is authorized / authorization not required, proceed to the next handler
        return await next();
    }

}
