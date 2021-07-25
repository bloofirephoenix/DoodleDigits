﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoodleDigits.Core;
using DoodleDigits.Core.Execution;
using DoodleDigits.Core.Execution.Results;
using DoodleDigits.Core.Execution.ValueTypes;
using DoodleDigits.Core.Parsing;
using NUnit.Framework;
using Rationals;

namespace UnitTests.Execution {
    static class ExecutionUtils {

        public static void AssertEqual(Rational expected, string input) {
            AssertEqual(new RealValue(expected), input);
        }

        public static void AssertEqual(bool expected, string input) {
            AssertEqual(new BooleanValue(expected), input);
        }

        public static void AssertEqual(Value expected, string input) {

            Calculator calculator = new(FunctionLibrary.Functions, ConstantLibrary.Constants);

            var results = calculator.Calculate(input);

            foreach (Result result in results.Results) {
                if (result is ResultError error) {
                    Assert.Fail($"Error in {input}.\n{error.Error} at {error.Position} (\"{input[error.Position]}\")");
                }
            }

            ResultValue last = results.Results.OfType<ResultValue>().Last();
            
            Assert.AreEqual(last.Value, expected);
        }
    }
}
