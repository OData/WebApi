// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using Newtonsoft.Json;

namespace Microsoft.Web.Http.Data
{
    public class DataControllerActionValueBinder : DefaultActionValueBinder
    {
        private static ConcurrentDictionary<Type, IEnumerable<SerializerInfo>> _serializerCache = new ConcurrentDictionary<Type, IEnumerable<SerializerInfo>>();

        private MediaTypeFormatter[] _formatters;

        protected override IEnumerable<MediaTypeFormatter> GetFormatters(HttpActionDescriptor actionDescriptor)
        {
            if (_formatters == null)
            {
                HttpControllerDescriptor descr = actionDescriptor.ControllerDescriptor;
                HttpConfiguration config = actionDescriptor.Configuration;
                DataControllerDescription dataDesc = DataControllerDescription.GetDescription(descr);

                List<MediaTypeFormatter> list = new List<MediaTypeFormatter>();
                AddFormattersFromConfig(list, config);
                AddDataControllerFormatters(list, dataDesc);
                _formatters = list.ToArray();
            }

            return _formatters;
        }

        protected override IBodyModelValidator GetBodyModelValidator(HttpActionDescriptor actionDescriptor)
        {
            return null;
        }

        private static void AddDataControllerFormatters(List<MediaTypeFormatter> formatters, DataControllerDescription description)
        {
            var cachedSerializers = _serializerCache.GetOrAdd(description.ControllerType, controllerType =>
            {
                // for the specified controller type, set the serializers for the built
                // in framework types
                List<SerializerInfo> serializers = new List<SerializerInfo>();

                Type[] exposedTypes = description.EntityTypes.ToArray();
                serializers.Add(GetSerializerInfo(typeof(ChangeSetEntry[]), exposedTypes));
                serializers.Add(GetSerializerInfo(typeof(QueryResult), exposedTypes));

                return serializers;
            });

            JsonMediaTypeFormatter formatterJson = new JsonMediaTypeFormatter();
            formatterJson.SerializerSettings = new JsonSerializerSettings() { PreserveReferencesHandling = PreserveReferencesHandling.Objects, TypeNameHandling = TypeNameHandling.All };

            XmlMediaTypeFormatter formatterXml = new XmlMediaTypeFormatter();

            // apply the serializers to configuration
            foreach (var serializerInfo in cachedSerializers)
            {
                formatterXml.SetSerializer(serializerInfo.ObjectType, serializerInfo.XmlSerializer);
            }
            
            formatters.Add(formatterJson);
            formatters.Add(formatterXml);            
        }

        // Get existing formatters from config, excluding Json/Xml formatters. 
        private static void AddFormattersFromConfig(List<MediaTypeFormatter> formatters, HttpConfiguration config)
        {
            foreach (var formatter in config.Formatters)
            {
                if (formatter.GetType() == typeof(JsonMediaTypeFormatter) ||
                    formatter.GetType() == typeof(XmlMediaTypeFormatter))
                {
                    // skip copying the json/xml formatters since we're configuring those
                    // specifically per controller type and can't share instances between
                    // controllers
                    continue;
                }
                formatters.Add(formatter);
            }
        }        

        private static SerializerInfo GetSerializerInfo(Type type, IEnumerable<Type> knownTypes)
        {
            SerializerInfo info = new SerializerInfo();
            info.ObjectType = type;

            info.XmlSerializer = new DataContractSerializer(type, knownTypes);
            return info;
        }

        private class SerializerInfo
        {
            public Type ObjectType { get; set; }
            public DataContractSerializer XmlSerializer { get; set; }
        }
    }
}
