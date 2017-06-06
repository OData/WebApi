using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;

namespace WebStack.QA.Test.OData.Formatter.JsonLight
{
    public class JsonLightConfigurator
    {
        private readonly Dictionary<Uri, IEdmModel> models = new Dictionary<Uri, IEdmModel>();
        private DataServiceContext _ctx;
        private string _acceptHeader;

        public JsonLightConfigurator(DataServiceContext ctx, string acceptHeader)
        {
            _ctx = ctx;
            _acceptHeader = acceptHeader;
        }

        public void Configure()
        {
            if (_ctx.ResolveName == null)
            {
                _ctx.ResolveName = t => t.FullName;
            }
            _ctx.Format.LoadServiceModel = () =>
            {
                Uri metadataUri = _ctx.GetMetadataUri();
                if (!models.ContainsKey(metadataUri))
                {
                    var xmlTextReader = new XmlTextReader(metadataUri.ToString());
                    IEdmModel edmModel = null;
                    IEnumerable<EdmError> errors = null;
                    if (EdmxReader.TryParse(xmlTextReader, out edmModel, out errors))
                    {
                        models[metadataUri] = edmModel;
                    }
                    else
                    {
                        var errorMessageBuilder = new StringBuilder("Model creation failed; please resolve the following errors in the metadata document:");
                        foreach (EdmError error in errors)
                        {
                            errorMessageBuilder.AppendLine(String.Format("t{0}", error.ErrorMessage));
                        }
                        throw new Exception(errorMessageBuilder.ToString());
                    }
                }
                return models[metadataUri];
            };
            _ctx.Format.UseJson();
            _ctx.SendingRequest2 += ChangeAcceptHeader;
        }

        private void ChangeAcceptHeader(object sender, SendingRequest2EventArgs e)
        {
            if (e.RequestMessage.GetHeader("content-type") != null)
            {
                e.RequestMessage.SetHeader("content-type", _acceptHeader);
            }
            e.RequestMessage.SetHeader("accept", _acceptHeader);
        }
    }
}
