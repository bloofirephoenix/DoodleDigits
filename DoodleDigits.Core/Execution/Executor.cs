﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoodleDigits.Core.Execution.Functions;
using DoodleDigits.Core.Execution.Functions.Binary;
using DoodleDigits.Core.Execution.Results;
using DoodleDigits.Core.Execution.ValueTypes;
using DoodleDigits.Core.Parsing;
using DoodleDigits.Core.Parsing.Ast;
using DoodleDigits.Core.Utilities;
using Rationals;

namespace DoodleDigits.Core.Execution {

    public class ExecutionResult {
        public readonly Result[] Results;
        public ExecutionResult(Result[] results) {
            Results = results;
        }
    }

    public class Executor {
        private readonly ExecutionContext context;
        private readonly List<Result> results;
        private readonly Dictionary<string, FunctionData> functions;

        public Executor(IEnumerable<FunctionData> functions, IEnumerable<Constant> constants) {
            results = new List<Result>();
            this.functions = new Dictionary<string, FunctionData>();
            foreach (FunctionData functionData in functions) {
                foreach (string name in functionData.Names) {
                    this.functions.Add(name, functionData);
                }
            }
            context = new ExecutionContext(constants);
        }

        public ExecutionResult Execute(AstNode root) {
            results.Clear();
            context.Clear();

            if (root is ExpressionList list) {
                foreach (Expression expression in list.Expressions) {
                    results.Add(new ResultValue(Calculate(expression), expression.Position));
                }
            } else if (root is Expression ex) {
                results.Add(new ResultValue(Calculate(ex), ex.Position));
            }

            results.AddRange(context.Results);

            results.Sort((a, b) => a.Position.Start.Value - b.Position.Start.Value);

            return new ExecutionResult(results.ToArray());
        }


        private Value Calculate(Expression expression) {

            switch (expression) {
                case BinaryOperation bo:
                    return Calculate(bo);
                case UnaryOperation uo:
                    return Calculate(uo);
                case NumberLiteral nl:
                    return Calculate(nl);
                case Identifier id:
                    return Calculate(id);
                case Function f:
                    return Calculate(f);
                case EqualsComparison ec:
                    return Calculate(ec);
                case ErrorNode error:
                    return new UndefinedValue();
                default: throw new Exception("Expression not handled for " + expression.GetType());
            }

        }

        private Value Calculate(Function function) {
            if (functions.TryGetValue(function.Identifier, out var functionData)) {

                int minParameters = functionData.ParameterCount.Start.Value;
                int maxParameters = functionData.ParameterCount.End.GetOffset(int.MaxValue);

;                if (function.Arguments.Length < minParameters ||
                    function.Arguments.Length > maxParameters) {

                    results.Add(new ResultError(minParameters == maxParameters ? 
                        $"Function expects {minParameters} parameters" : 
                        $"Function expects between {minParameters} and {maxParameters} parameters", 
                        function.Position));
                    return new UndefinedValue();
                }

                return functionData.Function(function.Arguments.Select(x => Calculate(x)).ToArray(), context.ForNode(function));
            }

            results.Add(new ResultError($"Unknown function: {function.Identifier}", function.Position));
            return new UndefinedValue();
        }

        private Value Calculate(Identifier identifier) {
            if (context.Constants.TryGetValue(identifier.Value, out Value? constantValue)) {
                return constantValue;
            }

            if (context.Variables.TryGetValue(identifier.Value, out Value? variableValue)) {
                return variableValue;
            }

            results.Add(new ResultError("Unknown identifier", identifier.Position));
            return new UndefinedValue();
        }

        private Value Calculate(NumberLiteral numberLiteral) {
            string number = numberLiteral.Number;
            int @base = 10;
            bool trivial = true;
            RealValue.PresentedForm form = RealValue.PresentedForm.Unset;

            if (number.StartsWith("0x")) {
                @base = 16;
                number = number[2..];
                trivial = false;
                form = RealValue.PresentedForm.Hex;
            }

            if (number.StartsWith("0b")) {
                @base = 2;
                number = number[2..];
                trivial = false;
                form = RealValue.PresentedForm.Binary;
            }

            if (RationalUtils.TryParse(number, out Rational result, @base)) {
                return new RealValue(result, trivial, form);
            }

            return new UndefinedValue();
        }

        private Value Calculate(UnaryOperation unaryOperation) {
            
            Value value = Calculate(unaryOperation.Value);

            UnaryOperation.OperationFunction func = UnaryOperation.GetFunctionFromType(unaryOperation.Operation);
            
            return func(value, context.ForNode(unaryOperation));
        }

        private Value Calculate(EqualsComparison equalsComparison) {

            Value? CalculateExpression(Expression expression) {
                if (expression is Identifier identifier) {
                    if (context.Variables.ContainsKey(identifier.Value) == false &&
                        context.Constants.ContainsKey(identifier.Value) == false) {
                        return null;
                    }
                }
                return Calculate(expression);
            }

            Value?[] calculatedResults = equalsComparison.Expressions.Select(x => CalculateExpression(x)).ToArray();

            bool isAssignmentChain = 
                calculatedResults.Count(x => x != null) == 1 && 
                equalsComparison.Signs.Contains(EqualsComparison.EqualsSign.NotEquals) == false;

            if (isAssignmentChain) {
                Value? calculatedResult = calculatedResults.First(x => x != null);
                if (calculatedResult == null) {
                    throw new Exception("This shouldn't be possible");
                }

                for (var i = 0; i < equalsComparison.Expressions.Length; i++) {
                    if (calculatedResults[i] != null) {
                        continue;
                    }
                    Identifier value = (Identifier)equalsComparison.Expressions[i];

                    context.Variables[value.Value] = calculatedResult.Clone(false);
                }

                return calculatedResult;
            }
            else {
                for (int i = 0; i < equalsComparison.Signs.Length; i++) {
                    var type = equalsComparison.Signs[i];
                    Value lhs = calculatedResults[i] ?? Calculate(equalsComparison.Expressions[i]);
                    Value rhs = calculatedResults[i + 1] ?? Calculate(equalsComparison.Expressions[i + 1]);

                    Value result = type == EqualsComparison.EqualsSign.Equals
                        ? BinaryOperations.Equals(lhs, rhs, i, context.ForNode(equalsComparison))
                        : BinaryOperations.NotEquals(lhs, rhs, i, context.ForNode(equalsComparison));

                    if (result is not BooleanValue booleanValue) {
                        return new UndefinedValue();
                    }

                    if (booleanValue.Value == false) {
                        return new BooleanValue(false);
                    }
                }

                return new BooleanValue(true);
            }
        }

        private Value Calculate(BinaryOperation bo) {
            Value lhs = Calculate(bo.Lhs);
            Value rhs = Calculate(bo.Rhs);

            BinaryOperation.OperationFunction func = BinaryOperation.GetOperationFromType(bo.Operation);

            return func(lhs, rhs, context.ForNode(bo));
        }

    }
}
