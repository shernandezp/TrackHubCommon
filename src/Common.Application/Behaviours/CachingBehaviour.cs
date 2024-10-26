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

using Common.Application.Attributes;
using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Common.Domain.Extensions;

namespace Common.Application.Behaviours;

public class CachingBehaviour<TRequest, TResponse>(IDistributedCache cache, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    // METHOD: Handle
    // DESCRIPTION: Handles the request by checking if there is a cache policy and retrieving the response from cache if available.
    // PARAMETERS:
    // - request: The request object.
    // - next: The delegate representing the next handler in the pipeline.
    // - cancellationToken: The cancellation token.
    // RETURNS: The response object.
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cachePolicy = typeof(TRequest).GetCustomAttribute<CachingAttribute>();
        if (cachePolicy == null)
        {
            // No cache policy found, so just continue through the pipeline
            return await next();
        }
        var fullName = typeof(TRequest).FullName;
        var cacheKey = request.GetCacheKey<TRequest, TResponse>();
        var cachedResponse = await cache.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            logger.LogDebug("Response retrieved {FullName} from cache. CacheKey: {cacheKey}", fullName, cacheKey);
            return cachedResponse;
        }

        var response = await next();
        logger.LogDebug("Caching response for {FullName} with cache key: {cacheKey}", fullName, cacheKey);

        await cache.SetAsync(cacheKey, response, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cachePolicy.AbsoluteExpirationRelativeToNow,
            AbsoluteExpiration = cachePolicy.AbsoluteExpiration,
            SlidingExpiration = cachePolicy.SlidingExpiration
        }, cancellationToken);
        return response;
    }
}
