// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class ViewTemplateUserControl<TModel> : ViewUserControl<TModel>
    {
        protected string FormattedModelValue
        {
            get { return ViewData.TemplateInfo.FormattedModelValue.ToString(); }
        }
    }
}
