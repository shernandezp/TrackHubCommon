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

using Microsoft.Extensions.Hosting;
using Serilog;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Wires Serilog into the host using configuration only. All sinks (including the
/// PostgreSQL database sink), minimum levels, overrides and enrichers are read from the
/// <c>Serilog</c> section of appsettings, so APIs and background services opt in to
/// database logging by editing configuration only.
/// </summary>
public static class SerilogServiceCollectionExtensions
{
    public static TBuilder AddTrackHubSerilog<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName));

        return builder;
    }
}
