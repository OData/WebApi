using System;
using System.Collections.Generic;

namespace WebStack.QA.Common.WebHost
{
    /// <summary>
    /// Transform web.config, use constructor to add transform action.
    /// </summary>
    public class WebConfigTransformer
    {
        private Action<WebConfigHelper> _transform;

        public WebConfigTransformer(Action<WebConfigHelper> transform)
        {
            _transform = transform;
        }

        public void Transform(WebConfigHelper config)
        {
            _transform(config);
        }
    }

    /// <summary>
    /// This extension is used to support to run multiple transformers
    /// </summary>
    public static class WebConfigTransformerExtensions
    {
        public static void Transform(this IEnumerable<WebConfigTransformer> transformers, WebConfigHelper webConfig)
        {
            foreach (var transformer in transformers)
            {
                transformer.Transform(webConfig);
            }
        }
    }
}
