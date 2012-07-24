// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public abstract class ModelValidatorProvider
    {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context);
    }
}
