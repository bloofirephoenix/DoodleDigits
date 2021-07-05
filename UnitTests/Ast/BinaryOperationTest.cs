﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoodleDigits.Core;
using DoodleDigits.Core.Ast;
using NUnit.Framework;

namespace UnitTests.Ast {
    class BinaryOperationTest {

        [Test]
        public void TestBasicSingles() {

            AstUtils.AssertEqual(new BinaryOperation(
                new NumberLiteral("5"), 
                BinaryOperation.OperationType.Add,
                new NumberLiteral("5")
                ), "5 + 5");


            AstUtils.AssertEqual(new BinaryOperation(
                new NumberLiteral("5"),
                BinaryOperation.OperationType.Multiply,
                new NumberLiteral("5.125")
            ), "5 * 5.125");

            AstUtils.AssertEqual(new BinaryOperation(
                new NumberLiteral("12345"),
                BinaryOperation.OperationType.Subtract,
                new NumberLiteral("5.125")
            ), "12345-5.125");
        }

        [Test]
        public void TestOrderOfOperations() {

            AstUtils.AssertEqual(
                new BinaryOperation(
                    new BinaryOperation(
                        new NumberLiteral("1"),
                        BinaryOperation.OperationType.Multiply,
                        new NumberLiteral("2")
                        ),
                    BinaryOperation.OperationType.Subtract,
                    new NumberLiteral("3")
                    )
            , "1*2-3");

            AstUtils.AssertEqual(
                new BinaryOperation(
                    new NumberLiteral("4"),
                    BinaryOperation.OperationType.Subtract,
                    new BinaryOperation(
                        new NumberLiteral("5"),
                        BinaryOperation.OperationType.Multiply,
                        new NumberLiteral("6")
                    )
                )
                , "4-5*6");


            AstUtils.AssertEqual(
                new BinaryOperation(
                    new BinaryOperation(
                        new NumberLiteral("10"),
                        BinaryOperation.OperationType.Subtract,
                        new NumberLiteral("1")
                    ),
                    BinaryOperation.OperationType.Subtract,
                    new NumberLiteral("2")
                )
                , "10-1-2");
        }

        [Test]
        public void TestImplicitMultiplication() {
            AstUtils.AssertEqual(
                new BinaryOperation(
                    new BinaryOperation(
                        new NumberLiteral("5"),
                        BinaryOperation.OperationType.Add,
                        new NumberLiteral("5")
                    ), 
                    BinaryOperation.OperationType.Multiply,
                        new NumberLiteral("5")
                    ), "(5 + 5)(5)"
                );

            AstUtils.AssertEqual(
                new BinaryOperation(
                    new BinaryOperation(
                        new NumberLiteral("5"),
                        BinaryOperation.OperationType.Add,
                        new NumberLiteral("5")
                    ),
                    BinaryOperation.OperationType.Multiply,
                    new NumberLiteral("5")
                ), "(5 + 5)5"
            );

            AstUtils.AssertEqual(
                new BinaryOperation(
                    new NumberLiteral("5"),
                    BinaryOperation.OperationType.Multiply,
                    new Identifier("x")
                ), "5x"
            );

            AstUtils.AssertEqual(
                new BinaryOperation(
                    new BinaryOperation(
                        new NumberLiteral("5"),
                        BinaryOperation.OperationType.Multiply,
                        new Identifier("x")
                    ), 
                    BinaryOperation.OperationType.Multiply, 
                    new Identifier("y")
                ), "5x(y)"
            );

        }
    }
}
