﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DoodleDigits.Core.Parsing.Ast;

namespace DoodleDigits.Core.Execution.ValueTypes {
    public class UndefinedValue : Value {
        public enum UndefinedType {
            Unset,
            Error,
            Undefined,
        }

        public UndefinedType Type { get; }

        public override string ToString() {
            return "undefined";
        }
        
        public override bool Equals(Value? other) {
            return false;
        }

        public override int GetHashCode() {
            return 0;
        }

        public override Value Clone(bool? triviallyAchieved = null) {
            return new UndefinedValue(Type, this.SourceAstNode);
        }

        public UndefinedValue() : this(UndefinedType.Unset, null) { }

        public UndefinedValue(UndefinedType type, AstNode? sourceAstNode) : base(false, sourceAstNode) {
            Type = type;
        }
    }
}
