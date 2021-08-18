//-----------------------------------------------------------------------------
// <copyright file="Types.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApiPerformance.Service
{
    public class ClassA
    {
        [Key]
        public string Name { get; set; }

        public ComplexPoly Poly { get; set; }

        public List<ComplexPolyB> Polys { get; set; }
    }

    public class ClassB : ClassA
    {
        public string Test { get; set; }
    }

    public class ClassC : ClassA
    {
        public string Test2 { get; set; }
    }

    public interface ITest
    {
        string Name { get; }
    }

    public class TestImpl : ITest
    {
        public string Name { get; set; }
    }

    public class ComplexPoly
    {
        public string Name { get; set; }
    }

    public class ComplexPolyB : ComplexPoly
    {
        public string Taste { get; set; }

        public string Prop1 { get; set; }

        public string Prop2 { get; set; }

        public string Prop3 { get; set; }

        public string Prop4 { get; set; }

        public ComplexPolyB()
        {
            Prop1 = "One";
            Prop2 = "Two";
            Prop3 = "Three";
            Prop4 = "Four";
        }
    }

    public class ComplexPolyC : ComplexPolyB
    {
        public int Age { get; set; }
    }
}
