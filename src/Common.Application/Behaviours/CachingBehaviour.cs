﻿// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using Common.Application.Attributes;
using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using Common.Domain.Extensions;

namespace Common.Application.Behaviours;

public class CachingBehaviour<TRequest, TResponse>(IDistributedCache cache)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{

    /// <summary>
    /// Handles the request by checking if there is a cache policy and retrieving the response from cache if available.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The delegate representing the next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response object.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cachePolicy = typeof(TRequest).GetCustomAttribute<CachingAttribute>();
        if (cachePolicy == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next(cancellationToken);
        }

        var enableCachingProperty = request.GetType().GetProperty("EnableCaching");
        var enableCaching = true;
        if (enableCachingProperty != null)
        {
            enableCaching = (bool?)enableCachingProperty.GetValue(request) ?? false;
        }

        if (!enableCaching)
        {
            // Caching is disabled, so just continue through the pipeline
            return await next(cancellationToken);
        }

        var cacheKey = request.GetCacheKey<TRequest, TResponse>();
        var cachedResponse = await cache.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var response = await next(cancellationToken);

        await cache.SetAsync(cacheKey, response, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cachePolicy.AbsoluteExpirationRelativeToNow,
            AbsoluteExpiration = cachePolicy.AbsoluteExpiration,
            SlidingExpiration = cachePolicy.SlidingExpiration
        }, cancellationToken);
        return response;
    }
}
