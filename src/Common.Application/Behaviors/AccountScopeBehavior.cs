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

using System.Collections.Concurrent;
using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Central, fail-closed tenant-scope guard. Runs immediately AFTER
/// <see cref="AuthorizationBehavior{TRequest, TResponse}"/> (the principal must already be
/// established and its permission grant verified) and before every other behavior and the handler.
/// <para>
/// Authorization answers "may this caller perform this action?"; it says nothing about WHICH
/// tenant's data the caller named. Requests take an <c>AccountId</c> straight off the wire, so
/// without this guard tenant scope depends on each individual handler remembering to check — and
/// any handler that forgets is a cross-tenant read or write.
/// </para>
/// <para>The rules, in order:</para>
/// <list type="number">
/// <item>The request names no account (no <c>AccountId</c> property, or an absent/empty value) —
/// there is no tenant dimension to police; pass through untouched.</item>
/// <item>The request type is marked <see cref="AllowCrossAccountAttribute"/> — a deliberate,
/// justified cross-tenant surface; pass, logging the permitted crossing at Debug.</item>
/// <item>Otherwise the named account MUST equal the principal's own account. A mismatch, or a
/// principal carrying no account at all (e.g. a global service identity addressing a tenant it
/// was never scoped to), throws <see cref="ForbiddenAccessException"/>.</item>
/// </list>
/// </summary>
public sealed class AccountScopeBehavior<TRequest, TResponse>(
    ICurrentPrincipal principal,
    ILogger<AccountScopeBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, AllowCrossAccountAttribute?> CrossAccountCache = new();

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var requestAccountId = RequestAccountResolver.GetRequestAccountId(request);
        if (requestAccountId is null)
        {
            return await next();
        }

        var crossAccount = CrossAccountCache.GetOrAdd(typeof(TRequest), static t =>
            t.GetCustomAttribute<AllowCrossAccountAttribute>(inherit: true));

        if (crossAccount is not null)
        {
            logger.LogDebug(
                "Cross-account call permitted for {RequestType} targeting account {AccountId} by principal {PrincipalType} (account {PrincipalAccountId}). Justification: {Justification}",
                typeof(TRequest).FullName,
                requestAccountId.Value,
                principal.PrincipalType,
                principal.AccountId,
                crossAccount.Justification);
            return await next();
        }

        if (principal.AccountId is not { } principalAccountId || principalAccountId == Guid.Empty)
        {
            logger.LogWarning(
                "Tenant scope denied for {RequestType}: the request targets account {AccountId} but principal {PrincipalType} ('{SubjectId}') carries no account scope.",
                typeof(TRequest).FullName,
                requestAccountId.Value,
                principal.PrincipalType,
                principal.ClientId ?? principal.SubjectId);
            throw new ForbiddenAccessException(
                "Insufficient permissions. The caller has no account scope and may not act on a specific account.");
        }

        if (principalAccountId != requestAccountId.Value)
        {
            logger.LogWarning(
                "Tenant scope denied for {RequestType}: principal {PrincipalType} ('{SubjectId}') of account {PrincipalAccountId} targeted account {AccountId}.",
                typeof(TRequest).FullName,
                principal.PrincipalType,
                principal.ClientId ?? principal.SubjectId,
                principalAccountId,
                requestAccountId.Value);
            throw new ForbiddenAccessException(
                "Insufficient permissions. The requested account is outside the caller's account scope.");
        }

        return await next();
    }
}
