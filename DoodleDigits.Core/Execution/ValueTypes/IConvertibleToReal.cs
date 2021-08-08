﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoodleDigits.Core.Execution.ValueTypes {
    public interface IConvertibleToReal {

        public RealValue ConvertToReal();

        public RealValue ConvertToReal(ExecutionContext context);
    }
}
