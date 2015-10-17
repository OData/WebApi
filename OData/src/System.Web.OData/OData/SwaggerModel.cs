using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Wrapper of EdmModel.
    /// </summary>
    public class SwaggerModel
    {
        private IEdmModel edmModel;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edmModel"></param>
        public SwaggerModel(IEdmModel edmModel)
        {
            this.edmModel = edmModel;
        }
    }
}
