﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Convert ticks to minutes number.
    /// </summary>
    public class TicksToMinutes : ExpressionEvaluator
    {
        private const long TicksPerMinute = 60 * 10000000L;

        public TicksToMinutes()
            : base(ExpressionType.TicksToMinutes, Evaluator, ReturnType.Number, FunctionUtils.ValidateUnaryNumber)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0].IsInteger())
                {
                    value = Convert.ToDouble(args[0]) / TicksPerMinute;
                }
                else
                {
                    error = $"{expression} should contain an integer of ticks";
                }
            }

            return (value, error);
        }
    }
}