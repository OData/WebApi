using System;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public sealed class ComplexModelDtoResult
    {
        public ComplexModelDtoResult(object model, ModelValidationNode validationNode)
        {
            if (validationNode == null)
            {
                throw new ArgumentNullException("validationNode");
            }

            Model = model;
            ValidationNode = validationNode;
        }

        public object Model { get; private set; }

        public ModelValidationNode ValidationNode { get; private set; }
    }
}
