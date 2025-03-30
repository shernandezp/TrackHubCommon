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

namespace Common.Domain.Extensions;

public static class DateTimeExtensions
{
    public static string ToIso8601String(this DateTimeOffset date)
        => date.ToString("yyyy-MM-ddTHH:mm:ssZ");

    public static string ToIso8601String(this DateTime date)
        => date.ToString("yyyy-MM-ddTHH:mm:ssZ");

    public static string FormatDateTime(this DateTime? date)
        => date == null ? string.Empty : date.Value.FormatDateTime();

    public static string FormatDateTime(this DateTimeOffset? date)
        => date == null ? string.Empty : date.Value.FormatDateTime();

    public static string FormatDateTime(this DateTime date)
        => date.ToString("yyyy-MM-dd HH:mm:ss");

    public static string FormatDateTime(this DateTimeOffset date)
        => date.ToString("yyyy-MM-dd HH:mm:ss");

    public static string FormatDate(this DateTime? date)
        => date == null ? string.Empty : date.Value.FormatDate();

    public static string FormatDate(this DateTimeOffset? date)
        => date == null ? string.Empty : date.Value.FormatDate();

    public static string FormatDate(this DateTime date)
        => date.ToString("yyyy-MM-dd");

    public static string FormatDate(this DateTimeOffset date)
        => date.ToString("yyyy-MM-dd");

}
