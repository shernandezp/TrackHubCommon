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

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Common.Web.Transformers;

/** https://github.com/scalar/scalar/issues/4055 **/

public sealed class BearerSecuritySchemeTransformer(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer" || authScheme.Name.Contains("Bearer")))
        {
            document.Components ??= new OpenApiComponents();

            var securitySchemeId = "Bearer";

            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            };

            if (document.Components.SecuritySchemes != null)
            {
                document.Components.SecuritySchemes[securitySchemeId] = securityScheme;
            }

            // Add "Bearer" scheme as a requirement for the API as a whole
            var securitySchemeReference = new OpenApiSecuritySchemeReference(securitySchemeId, document);

            var requirement = new OpenApiSecurityRequirement
            {
                [securitySchemeReference] = []
            };

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations != null)
                {
                    foreach (var operation in path.Operations.Values)
                    {
                        operation.Security?.Add(requirement);
                    }
                }
            }
        }
    }
}
