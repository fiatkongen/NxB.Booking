using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Enums
{
    public enum PdfGenerationPriority
    {
        Default,
        SelectFirstThenRocket,
        RocketFirstThenSelect,
        SelectOnly,
        RocketOnly
    }
}
