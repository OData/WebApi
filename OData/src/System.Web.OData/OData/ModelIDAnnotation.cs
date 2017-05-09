using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.OData
{
    /// <summary>
    /// Allows to IEdmModel ID to keep it controlled by the user
    /// </summary>
    public class ModelIDAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModelIDAnnotation"/> class.
        /// </summary>
        /// <param name="modelID"></param>
        public ModelIDAnnotation(Guid modelID)
        {
            this.ModelID = modelID;
        }

        /// <summary>
        /// Gets modelId
        /// </summary>
        public Guid ModelID { get; private set; }
    }
}
