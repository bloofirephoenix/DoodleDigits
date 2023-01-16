using DoodleDigits.Core;
using NUnit.Framework;

namespace UnitTests.Execution;
class TrigonometryTest {

    [Test]
    public void TestDegreeAngles() {
        var settings = new CalculatorSettings {
            AngleUnit = AngleUnit.Degrees
        };

        ExecutionTestUtils.AssertEqual(0, "sin(360)", settings);
        ExecutionTestUtils.AssertEqual(1, "cos(360)", settings);


        ExecutionTestUtils.AssertEqual(1, "sin(-270)", settings);
        ExecutionTestUtils.AssertEqual(0, "cos(-270)", settings);

        ExecutionTestUtils.AssertEqual(1, "sin(90)", settings);
        ExecutionTestUtils.AssertEqual(0, "cos(90)", settings);

        ExecutionTestUtils.AssertEqual(0, "acos(1)", settings);
        ExecutionTestUtils.AssertEqual(90, "acos(0)", settings);

        ExecutionTestUtils.AssertEqual(0, "asin(0)", settings);
        ExecutionTestUtils.AssertEqual(90, "asin(1)", settings);

    }
}
