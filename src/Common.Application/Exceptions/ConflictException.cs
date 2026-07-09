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

namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when a create/update would violate a uniqueness constraint (duplicate resource).
/// Maps to HTTP 409 (REST) / the <c>CONFLICT</c> GraphQL error code.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : this("The resource already exists.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}
