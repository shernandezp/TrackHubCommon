// Copyright (c) 2024 Sergio Hernandez. All rights reserved.
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

using HotChocolate.Language;
using HotChocolate.Types;

namespace Common.Application.GraphQL.Types;

public class AnyType : ScalarType
{
    public AnyType() : base("Any") { }

    public override Type RuntimeType => typeof(object);

    public override bool IsInstanceOfType(IValueNode valueSyntax)
    {
        return valueSyntax switch
        {
            NullValueNode => true,
            IntValueNode => true,
            FloatValueNode => true,
            BooleanValueNode => true,
            StringValueNode => true,
            _ => false
        };
    }

    public override IValueNode ParseValue(object? value)
    {
        return value switch
        {
            null => NullValueNode.Default,
            int i => new IntValueNode(i),
            long l => new IntValueNode(l),
            float f => new FloatValueNode(f),
            double d => new FloatValueNode(d),
            bool b => new BooleanValueNode(b),
            string s => new StringValueNode(s),
            Guid g => new StringValueNode(g.ToString()),
            _ => throw new NotSupportedException()
        };
    }

    public override object? ParseLiteral(IValueNode valueSyntax)
    {
        return valueSyntax switch
        {
            NullValueNode => null,
            IntValueNode intValue => intValue.ToInt32(),
            FloatValueNode floatValue => floatValue.ToDouble(),
            BooleanValueNode booleanValue => booleanValue.Value,
            StringValueNode stringValue when Guid.TryParse(stringValue.Value, out var guid) => guid,
            _ => throw new NotSupportedException()
        };
    }

    public override IValueNode ParseResult(object? resultValue) => ParseValue(resultValue);

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        resultValue = runtimeValue;
        return true;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        runtimeValue = resultValue;
        return true;
    }
}
