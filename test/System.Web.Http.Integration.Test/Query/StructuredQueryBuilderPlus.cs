using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;

namespace System.Web.Http.Query
{
    public class StructuredQueryBuilderPlus : DefaultStructuredQueryBuilder
    {
        public override StructuredQuery GetStructuredQuery(Uri uri)
        {
            StructuredQuery query = base.GetStructuredQuery(uri);
            string select = uri.ParseQueryString()["$select"];
            if (select != null)
            {
                query.QueryParts.Add(new SelectStructuredQueryPart { QueryExpression = select });
            }

            return query;
        }

        private class SelectStructuredQueryPart : IStructuredQueryPart
        {
            public string QueryOperator
            {
                get { return "select"; }
            }

            public string QueryExpression { get; set; }

            public IQueryable ApplyTo(IQueryable source)
            {
                string[] fieldsToSelect = QueryExpression.Split(',');
                // Get the selector expression
                var selector = GetSelector(source.ElementType, fieldsToSelect);

                // apply the select on to the queryable and return the result
                return source.Provider.CreateQuery(
                    Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { source.ElementType, selector.Body.Type },
                    source.Expression,
                    selector));
            }

            private static LambdaExpression GetSelector(Type elementType, string[] fields)
            {
                ParameterExpression sourceParameter = Expression.Parameter(elementType);
                var fieldInitExpressions = fields
                                        .Select(field => Expression.Bind(elementType.GetProperty(field), Expression.Property(sourceParameter, field)));

                // new Element { Field1 = sourceElement.Field1, Fields2 = sourceElement.Field2 ... }
                var selector = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(elementType),
                        fieldInitExpressions),
                    sourceParameter);

                return selector;
            }
        }
    }

}
