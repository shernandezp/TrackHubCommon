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

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            if (user.Id == null)
            {
                throw new UnauthorizedAccessException();
            }

            foreach (var attribute in authorizeAttributes)
            {
                var resource = attribute.Resource;
                var action = attribute.Action;
                var roleAuthorized = await identityService.IsInRoleAsync(new Guid(user.Id), resource, action, cancellationToken);
                var policyAuthorized = await identityService.AuthorizeAsync(new Guid(user.Id), resource, action, cancellationToken);

                if (!roleAuthorized || !policyAuthorized)
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}
