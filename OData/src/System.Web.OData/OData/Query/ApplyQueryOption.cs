using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a $apply OData query option for querying.
    /// </summary>
    public class ApplyQueryOption
    {
        private ApplyClause _applyClause;
        private ODataQueryOptionParser _queryOptionParser;


        /// <summary>
        /// Initialize a new instance of <see cref="ApplyQueryOption"/> based on the raw $filter value and 
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public ApplyQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            // TODO: Implement and add validator
            //Validator = new FilterQueryValidator();
            _queryOptionParser = queryOptionParser;
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }


        /// <summary>
        /// Gets the parsed <see cref="ApplyClause"/> for this query option.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get
            {
                if (_applyClause == null)
                {
                    _applyClause = _queryOptionParser.ParseApply();
                    //SingleValueNode filterExpression = _applyClause.Expression.Accept(
                    //    new ParameterAliasNodeTranslator(_queryOptionParser.ParameterAliasNodes)) as SingleValueNode;
                    //filterExpression = filterExpression ?? new ConstantNode(null);
                    //_applyClause = new ApplyClause(filterExpression, _applyClause.RangeVariable);
                }

                return _applyClause;
            }
        }


        /// <summary>
        ///  Gets the raw $apply value.
        /// </summary>
        public string RawValue { get; private set; }
    }
}
