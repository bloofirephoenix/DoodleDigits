using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DoodleDigits.Core.Execution;
using DoodleDigits.Core.Execution.Results;
using DoodleDigits.Core.Parsing;

namespace DoodleDigits.Core; 

public class CalculationResult {
    public readonly Result[] Results;

    public CalculationResult(Result[] results) {
        Results = results;
    }
}

public enum AngleUnit {
    Radians,
    Degrees,
}

public class CalculatorSettings {
    /// <summary>
    /// Unit to use for trigonometric functions.
    /// </summary>
    public AngleUnit AngleUnit { get; init; } = AngleUnit.Radians;

    /// <summary>
    /// Functions to use inside the calculator. By default it's filled with the contents of <see cref="FunctionLibrary"/> 
    /// </summary>
    public List<FunctionData> Functions { get; init; } = FunctionLibrary.Functions.ToList();

    /// <summary>
    /// Constants to use for calculations. By default it's filled with the constants in <see cref="ConstantLibrary"/>
    /// </summary>
    public List<Constant> Constants { get; init; } = ConstantLibrary.Constants.ToList();
}

public class Calculator {

    private readonly Executor executor;
    private readonly Parser parser;

    /// <summary>
    /// Creates a new calculator with the default calculator settings/>
    /// </summary>
    public Calculator() : this(new CalculatorSettings()) {

    }

    /// <summary>
    /// Creates a new calculator with the provided functions and constants
    /// </summary>
    /// <param name="functions">Functions to use</param>
    /// <param name="constants">Constants to use</param>
    public Calculator(CalculatorSettings settings) {
        var functionData = settings.Functions.ToArray();

        executor = new Executor(settings);
        parser = new Parser(functionData);
    }

    public CalculationResult Calculate(string input) {
        ParseResult parseResult = parser.Parse(input);
        ExecutionResult executionResult = executor.Execute(parseResult.Root);

        List<Result> results = new();
        results.AddRange(executionResult.Results);
        results.AddRange(parseResult.Errors.Select(error => new ResultError(error.Message, error.Position)));

        results.Sort((a, b) => a.Position.Start.GetOffset(input.Length) - b.Position.Start.GetOffset(input.Length));

        return new CalculationResult(results.ToArray());
    }
}
