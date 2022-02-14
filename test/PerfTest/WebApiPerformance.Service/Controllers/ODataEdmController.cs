//-----------------------------------------------------------------------------
// <copyright file="ODataEdmController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace WebApiPerformance.Service
{
    public class ODataEdmController : ODataController
    {
        [HttpGet]
        public EdmEntityObjectCollection Get()
        {
            var props = Request.ODataProperties();

            var model = Request.GetModel();

            var es = (EntitySetSegment)props.Path.Segments[0];

            var entityType = es.EntitySet.EntityType();
            var collectionType = new EdmCollectionType(new EdmEntityTypeReference(entityType, false));

            var queryContext = new ODataQueryContext(model, entityType, props.Path);
            var queryOptions = new ODataQueryOptions(queryContext, Request);

            int n;
            n =
                int.TryParse(
                    Request.GetQueryNameValuePairs().Where(kv => kv.Key == "n").Select(kv => kv.Value).FirstOrDefault(), out n)
                    ? n
                    : 10;

            var @as = TestRepo.GetAs(n);

            if (queryOptions.SelectExpand != null)
            {
                props.SelectExpandClause = queryOptions.SelectExpand.SelectExpandClause;
            }

            return new EdmEntityObjectCollection(new EdmCollectionTypeReference(collectionType), @as.Select(a => Map(model, a)).ToArray());
        }


        private Dictionary<Type, IEdmSchemaType> _typeMap = new Dictionary<Type, IEdmSchemaType>();

        IEdmSchemaType GetType(IEdmModel model, Type t)
        {
            IEdmSchemaType def;
            if (!_typeMap.TryGetValue(t, out def))
            {
                _typeMap.Add(t, def = model.FindType(t.FullName));
            }

            return def;
        }

        private IEdmProperty _polysProp;

        IEdmEntityObject Map(IEdmModel model, ClassA a)
        {
            var type = (IEdmEntityType)GetType(model, a.GetType());

            var e = new EdmEntityObject(type);
            e.TrySetPropertyValue("Name", a.Name);

            e.TrySetPropertyValue("Poly", Map(model, a.Poly));

            _polysProp = _polysProp ?? type.FindProperty("Polys");
            e.TrySetPropertyValue("Polys", new EdmComplexObjectCollection((IEdmCollectionTypeReference)_polysProp.Type,
                a.Polys.Select(p => Map(model, p)).ToArray()));

            var b = a as ClassB;
            if (b != null)
            {
                e.TrySetPropertyValue("Test", b.Test);
            }
            var c = a as ClassC;
            if (c != null)
            {
                e.TrySetPropertyValue("Test2", c.Test2);
            }

            return e;
        }

        IEdmComplexObject Map(IEdmModel model, ComplexPoly p)
        {
            var type = (IEdmComplexType)GetType(model, p.GetType());

            var co = new EdmComplexObject(type);
            co.TrySetPropertyValue("Name", p.Name);

            var b = p as ComplexPolyB;
            if (b != null)
            {
                co.TrySetPropertyValue("Prop1", b.Prop1);
                co.TrySetPropertyValue("Prop2", b.Prop2);
                co.TrySetPropertyValue("Prop3", b.Prop3);
                co.TrySetPropertyValue("Prop4", b.Prop4);
            }

            var c = p as ComplexPolyC;
            if (c != null)
            {
                co.TrySetPropertyValue("Taste", c.Taste);
            }

            return co;
        }
    }
}
