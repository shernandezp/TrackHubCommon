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

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Domain.Extensions;

public static class CachingExtensions
{
    public static string GetCacheKey<TRequest, TResponse>(this TRequest request)
    {
        var r = new { request };
        var props = r.request?.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{string.Join(",", props ?? [])}}}";
    }

    public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
    {
        var serializedValue = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        await distributedCache.SetAsync(key, serializedValue, options, cancellationToken);
    }

    public static async Task<T?> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken cancellationToken) 
    {
        var serializedValue = await distributedCache.GetAsync(key, cancellationToken);
        return serializedValue == null ? default : JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(serializedValue));
    }
}
