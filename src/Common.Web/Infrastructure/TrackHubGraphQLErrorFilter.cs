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

using Common.Application.Exceptions;
using HotChocolate;
using HotChocolate.Execution;

namespace Common.Web.Infrastructure;

public sealed class TrackHubGraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ForbiddenAccessException forbidden)
        {
            var builder = ErrorBuilder.FromError(error)
                .SetMessage(forbidden.Message)
                .SetCode("FORBIDDEN");

            if (!string.IsNullOrWhiteSpace(forbidden.Resource))
            {
                builder.SetExtension("requiredResource", forbidden.Resource);
            }

            if (!string.IsNullOrWhiteSpace(forbidden.Action))
            {
                builder.SetExtension("requiredAction", forbidden.Action);
            }

            return builder.Build();
        }

        if (error.Exception is FeatureDisabledException featureDisabled)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(featureDisabled.Message)
                .SetCode("FEATURE_DISABLED")
                .SetExtension("featureKey", featureDisabled.FeatureKey)
                .Build();
        }

        if (error.Exception is AccountSuspendedException accountSuspended)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(accountSuspended.Message)
                .SetCode("ACCOUNT_SUSPENDED")
                .SetExtension("accountStatus", accountSuspended.Status.ToString())
                .Build();
        }

        if (error.Exception is UnauthorizedAccessException)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage("Authentication is required.")
                .SetCode("UNAUTHORIZED")
                .Build();
        }

        if (error.Exception is ConflictException conflict)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(conflict.Message)
                .SetCode("CONFLICT")
                .Build();
        }

        return error;
    }
}
