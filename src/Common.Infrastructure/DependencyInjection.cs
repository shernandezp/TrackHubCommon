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

using Common.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Common.Application.Interfaces;
using Common.Infrastructure;
using Ardalis.GuardClauses;
using Common.Domain.Constants;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool identityEndpoint = true)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var validateSigningKey = configuration.GetValue<bool>("AuthorityServer:ValidateIssuerSigningKey");
                options.Authority = configuration.GetValue<string>("AuthorityServer:Authority");
                options.TokenValidationParameters.ValidateAudience = configuration.GetValue<bool>("AuthorityServer:ValidateAudience");
                options.TokenValidationParameters.ValidateIssuer = configuration.GetValue<bool>("AuthorityServer:ValidateIssuer");
                options.TokenValidationParameters.ValidateIssuerSigningKey = validateSigningKey;
                options.TokenValidationParameters.ValidIssuer = configuration.GetValue<string>("AuthorityServer:Authority");
                if (validateSigningKey)
                {
                    var certificatePath = configuration.GetValue<string>("OpenIddict:Path");
                    var certificatePassword = configuration.GetValue<string>("OpenIddict:Password");

                    var bytes = File.ReadAllBytes(certificatePath ?? "");
                    var certificate = new X509Certificate2(
                        bytes,
                        certificatePassword);
                    var signingKey = new X509SecurityKey(certificate);
                    options.TokenValidationParameters.IssuerSigningKey = signingKey;
                }
            });

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IGraphQLClientFactory, GraphQLClientFactory>();

        if (identityEndpoint) 
        {
            services.AddHttpClient(Clients.Identity,
                client =>
                {
                    var url = configuration.GetValue<string>($"AppSettings:REST{Clients.Identity}Service");
                    Guard.Against.Null(url, message: $"Setting 'REST{Clients.Identity}Service' not found.");
                    client.BaseAddress = new Uri(url);
                    client.Timeout = TimeSpan.FromSeconds(10);   //Read from config
                })
                .AddHeaderPropagation();

            services.AddScoped<IIdentityService, IdentityService>();
        }

        return services;
    }
}
