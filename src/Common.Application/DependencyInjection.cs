﻿// Copyright (c) 2024 Sergio Hernandez. All rights reserved.
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
using Common.Application.Behaviours;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly? assembly)
    {
        services.AddValidatorsFromAssembly(assembly);

        services.AddMediatR(cfg => {
#pragma warning disable CS8604 // Possible null reference argument.
            cfg.RegisterServicesFromAssembly(assembly);
#pragma warning restore CS8604 // Possible null reference argument.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        });

        return services;
    }
}
