//-----------------------------------------------------------------------------
// <copyright file="TestRepo.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace WebApiPerformance.Service
{
    public class TestRepo
    {
        public static IEnumerable<ClassA> GetAs(int count = 10)
        {
            return Enumerable.Range(1, count).Select(i =>
            {
                var d = i - 1;

                ClassA a;
                if (d % 3 == 0)
                {
                    a = new ClassA();
                }
                else if (d % 3 == 1)
                {
                    a = new ClassB()
                    {
                        Test = "Test #" + i
                    };

                }
                else
                {
                    a = new ClassC()
                    {
                        Test2 = "Test2 #" + i
                    };
                }


                a.Name = "A" + i;

                if (d % 2 == 0)
                {
                    a.Poly = new ComplexPoly() { Name = "Poly" + i };
                }
                else
                {
                    a.Poly = new ComplexPolyB() { Name = "PolyB" + i };
                }


                a.Polys =
                    Enumerable.Range(1, 3)
                    .Select(j => j % 2 == 0 ? new ComplexPolyB() { Name = "Test " + j, Taste = "Kiwi" } : new ComplexPolyC() { Age = 10, Name = "Test" + j, Taste = "Sweet" })
                        .Cast<ComplexPolyB>()
                        .ToList();

                return a;

            });
        }

        public static IEdmModel GetModel()
        {
            var model = new ODataConventionModelBuilder();
            model.EntitySet<ClassA>("ODataClr");
            model.EntitySet<ClassA>("ODataEdm");

            return model.GetEdmModel();
        }
    }
}
