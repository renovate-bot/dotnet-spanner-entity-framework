// Copyright 2021 Google LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     https://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using System.Reflection;

namespace Google.Cloud.EntityFrameworkCore.Spanner.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SpannerStringMethodTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _containsMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        private static readonly MethodInfo _startsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) });

        private static readonly MethodInfo _endsWithMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

        private static readonly MethodInfo _lengthMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Length), new[] { typeof(int) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SpannerStringMethodTranslator([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (_containsMethodInfo.Equals(method))
            {
                var pos = TranslateSingleArgFunction("STRPOS", instance, arguments[0], typeof(long));
                return _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.GreaterThan(pos, _sqlExpressionFactory.Constant(0L)));
            }

            if (_startsWithMethodInfo.Equals(method))
            {
                return TranslateSingleArgFunction("STARTS_WITH", instance, arguments[0], typeof(bool));
            }

            if (_endsWithMethodInfo.Equals(method))
            {
                return TranslateSingleArgFunction("ENDS_WITH", instance, arguments[0], typeof(bool));
            }

            return null;
        }

        private SqlExpression TranslateSingleArgFunction(string function, SqlExpression instance, SqlExpression arg, System.Type returnType)
        {
            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                function,
                new[] { instance, arg },
                returnType));
        }
    }
}
