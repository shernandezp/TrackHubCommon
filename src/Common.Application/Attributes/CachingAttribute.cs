﻿// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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


namespace Common.Application.Attributes;

[AttributeUsage(AttributeTargets.All)]
public sealed class CachingAttribute : Attribute
{
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromSeconds(60);
    public DateTimeOffset? AbsoluteExpiration { get; set; } = null;
    public TimeSpan AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromSeconds(60);
}
