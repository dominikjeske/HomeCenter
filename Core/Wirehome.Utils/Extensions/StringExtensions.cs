﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Core.Extensions
{
    public static class StringExtensions
    {
        public static int Compare(this string orginalText, string comparedText) => string.Compare(orginalText, comparedText, StringComparison.OrdinalIgnoreCase);
    }
}
