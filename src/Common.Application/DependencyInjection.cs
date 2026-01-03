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

using System.Reflection;
using Ardalis.GuardClauses;
using Common.Application.Behaviors;
using Common.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, Assembly? assembly, bool isGraphQL = true)
    {
        services.AddValidatorsFromAssembly(assembly);
        Guard.Against.Null(assembly, message: $"Application assemblies not loaded.");

        // Register Mediator as ISender and IPublisher
        services.AddScoped<ISender, MediatorDispatcher>();
        services.AddScoped<IPublisher, MediatorDispatcher>();

        // Register pipeline behaviors as open generics
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RateLimitingBehavior<,>));
        if (isGraphQL)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GraphQLValidationBehavior<,>));
        }
        else
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }

        // Register all IRequestHandler<,> and INotificationHandler<> implementations using reflection
        if (assembly != null)
        {
            var types = assembly.GetTypes();

            // Register IRequestHandler<,>
            var requestHandlerInterface = typeof(IRequestHandler<,>);
            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestHandlerInterface);
                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, type);
                }
            }

            // Register INotificationHandler<>
            var notificationHandlerInterface = typeof(INotificationHandler<>);
            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerInterface);
                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, type);
                }
            }
        }

        return services;
    }

    public static IServiceCollection AddBasicApplicationServices(this IServiceCollection services, Assembly? assembly)
    {
        services.AddValidatorsFromAssembly(assembly);
        Guard.Against.Null(assembly, message: $"Application assemblies not loaded.");

        // Register Mediator as ISender and IPublisher
        services.AddScoped<ISender, MediatorDispatcher>();
        services.AddScoped<IPublisher, MediatorDispatcher>();

        // Register pipeline behaviors as open generics
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register all IRequestHandler<,> and INotificationHandler<> implementations using reflection
        if (assembly != null)
        {
            var types = assembly.GetTypes();

            // Register IRequestHandler<,>
            var requestHandlerInterface = typeof(IRequestHandler<,>);
            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestHandlerInterface);
                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, type);
                }
            }

            // Register INotificationHandler<>
            var notificationHandlerInterface = typeof(INotificationHandler<>);
            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerInterface);
                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, type);
                }
            }
        }

        return services;
    }
}
