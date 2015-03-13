using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser.Semantic;

namespace System.Web.OData.Query
{
    /// <summary>
    /// Represents ordering on a dynamic property
    /// </summary>
    public class OrderByOpenPropertyNode : OrderByNode
    {
        /// <summary>
        /// Default constructor for a dynamic property order by
        /// </summary>
        /// <param name="orderByClause">The order by clause for this open property</param>
        public OrderByOpenPropertyNode(OrderByClause orderByClause)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull("orderByClause");
            }

            OrderByClause = orderByClause;
            Direction = orderByClause.Direction;


            var openPropertyExpression = orderByClause.Expression as SingleValueOpenPropertyAccessNode;
            if (openPropertyExpression == null)
            {
                throw new ODataException(SRResources.OrderByClauseNotSupported);
            }
            PropertyName = openPropertyExpression.Name;
        }

        /// <summary>
        /// The order by clause
        /// </summary>
        public OrderByClause OrderByClause { get; set; }

        /// <summary>
        /// The name of the dynamic property
        /// </summary>
        public string PropertyName { get; private set; }
    }
}