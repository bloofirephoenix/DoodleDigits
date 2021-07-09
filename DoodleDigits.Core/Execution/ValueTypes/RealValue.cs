﻿using System;
using DoodleDigits.Core.Execution.Results;
using Rationals;

namespace DoodleDigits.Core.Execution.ValueTypes {
    public class RealValue : Value, IConvertibleToReal, IConvertibleToBool {
        public readonly Rational Value;

        public RealValue(Rational value) {
            Value = value;
        }

        public override string ToString() {
            if (HasDecimal) {
                return ((double)Value).ToString();
            }
            return Value.ToString();
        }
        
        public override bool Equals(Value? other) {
            if (other is not RealValue rOther) {
                return false;
            }

            return rOther.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public BooleanValue ConvertToBool() {
            return new BooleanValue(Value > new Rational(1, 2));
        }

        public BooleanValue ConvertToBool(ExecutionContext context, Range position) {
            BooleanValue newValue = ConvertToBool();
            context.AddResult(new ResultConversion(this, newValue, position));
            return newValue;
        }

        public bool HasDecimal => Value.FractionPart != 0;
        public RealValue ConvertToReal() {
            return this;
        }

        public RealValue ConvertToReal(ExecutionContext context, Range position) {
            return this;
        }
    }
}
