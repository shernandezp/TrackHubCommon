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

using FluentValidation.Results;
using GraphQL;
using HotChocolate;

namespace Common.Application.Exceptions;
public static class ExceptionConverter
{
    public static IError[] ConvertToIError(this IEnumerable<GraphQLError> graphQLError) 
        => graphQLError.Select(error => error.ConvertToIError()).ToArray();

    public static IError[] ConvertToIError(this IEnumerable<ValidationFailure> graphQLError)
        => graphQLError.Select(error => error.ConvertToIError()).ToArray();

    public static IError ConvertToIError(this GraphQLError graphQLError)
    {
        IReadOnlyList<Location> locations = [];
        if (graphQLError.Locations != null)
        {
            locations = graphQLError.Locations
            .Select(location => new Location((int)location.Line, (int)location.Column))
            .ToList();
        }

        IError error = new Error(
            graphQLError.Message,
            string.Empty,
            new ExceptionPath(graphQLError.Path),
            locations
        );

        return error;
    }

    public static IError ConvertToIError(this ValidationFailure validationFailure)
        => new Error(
            validationFailure.ErrorMessage
            //,validationFailure.ErrorCode
            //,new ExceptionPath(validationFailure.PropertyName)
        );

    public class ExceptionPath : HotChocolate.Path
    {
        public ExceptionPath(ErrorPath? errorPath)
        {
            if (errorPath != null)
            {
                foreach (var segment in errorPath.ToList())
                {
                    Append(segment?.ToString() ?? string.Empty);
                }
            }
        }

        public ExceptionPath(string property)
        {
            if (!string.IsNullOrEmpty(property))
            {
                Append(property);
            }
        }
    }

}