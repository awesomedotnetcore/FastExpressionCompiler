﻿using System;
using NUnit.Framework;

#if LIGHT_EXPRESSION
using static FastExpressionCompiler.LightExpression.Expression;
namespace FastExpressionCompiler.LightExpression.UnitTests
#else
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
namespace FastExpressionCompiler.UnitTests
#endif
{
    public class Issue159_NumericComparisonsWithConversions
    {
        [Test]
        public void NumericComparisonsWithConversionsShouldWork()
        {
            var ulongParameter = Parameter(typeof(ValueHolder<ulong>), "ulongValue");
            var ulongValueProperty = Property(ulongParameter, "Value");

            var intVariable = Variable(typeof(ValueHolder<int>), "intValue");
            var intValueProperty = Property(intVariable, "Value");

            var newIntHolder = Assign(intVariable, New(intVariable.Type));

            var ulongGtOrEqualToIntMinValue = GreaterThanOrEqual(
                Convert(ulongValueProperty, intValueProperty.Type),
                Constant(int.MinValue));

            var ulongLtOrEqualToIntMaxValue = LessThanOrEqual(
                ulongValueProperty,
                Convert(Constant(int.MaxValue), ulongValueProperty.Type));

            var ulongIsInIntRange = AndAlso(ulongGtOrEqualToIntMinValue, ulongLtOrEqualToIntMaxValue);

            var ulongAsInt = Convert(ulongValueProperty, typeof(int));
            var defaultInt = Default(intValueProperty.Type);
            var ulongValueOrDefault = Condition(ulongIsInIntRange, ulongAsInt, defaultInt);

            var intValueAssignment = Assign(intValueProperty, ulongValueOrDefault);

            var block = Block(
                new[] { intVariable },
                newIntHolder,
                intValueAssignment,
                intVariable);

            var ulongValueOrDefaultLambda = Lambda<Func<ValueHolder<ulong>, ValueHolder<int>>>(
                block,
                ulongParameter);

            var ulongValueOrDefaultFunc = ulongValueOrDefaultLambda.CompileFast();
            var result = ulongValueOrDefaultFunc.Invoke(new ValueHolder<ulong> { Value = ulong.MaxValue });

            Assert.AreEqual(default(int), result.Value);
        }

        private class ValueHolder<T>
        {
            public T Value { get; set; }
        }
    }
}