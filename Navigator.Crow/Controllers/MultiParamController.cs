﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Navigator.Core.Attributes;

namespace Navigator.Crow.Controllers
{
    [NavigatorController()]
    public class MultiParamController
    {
        [NavigatorMethod]
        public double Sum(params double[] args)
        {
            return args.Sum();
        }

        [NavigatorMethod]
        public string SumWithCaption(string caption, params double[] args)
        {
            return caption + ": " + args.Sum();
        }

        [NavigatorMethod]
        public DoubleWrapper[] ReturnAsArray(params double[] args)
        {
            return args.Select(d => new DoubleWrapper { Value = d, ValueSquared = d * d, ValueString = d.ToString(CultureInfo.InvariantCulture) }).ToArray();
        }

        [NavigatorMethod]
        public List<DoubleWrapper> ReturnAsList(params double[] args)
        {
            return args.Select(d => new DoubleWrapper { Value = d, ValueSquared = d * d, ValueString = d.ToString(CultureInfo.InvariantCulture) }).ToList();
        }

        [NavigatorMethod]
        public string StringConcat(string a, string b, string separator = "%")
        {
            return a + separator + b;
        }



    }

    public class DoubleWrapper
    {
        public double Value { get; set; }
        public double ValueSquared { get; set; }
        public string ValueString { get; set; }
    }
}

