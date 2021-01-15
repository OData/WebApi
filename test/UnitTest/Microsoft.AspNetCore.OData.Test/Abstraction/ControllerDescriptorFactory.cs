// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create [Http]ControllerDescriptor.
    /// </summary>
    public class ControllerDescriptorFactory
    {
        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
        public static ControllerActionDescriptor Create()
        {
            return new ControllerActionDescriptor();
        }

        /// <summary>
        /// Initializes a new instance of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new instance of the [Http]ControllerDescriptor  class.</returns>
        public static IEnumerable<ControllerActionDescriptor> Create(IRouteBuilder routeBuilder, string name, Type controllerType)
        {
            // Create descriptors. Search for non-public methods to pick up public methods in nested classes
            // as controllers are usually a nested class for the test class ad by default, this are marked private.
            List<ControllerActionDescriptor> descriptors = new List<ControllerActionDescriptor>();
            IEnumerable<MethodInfo> methods = controllerType.GetTypeInfo().DeclaredMethods;
            foreach (MethodInfo methodInfo in methods)
            {
                ControllerActionDescriptor descriptor = new ControllerActionDescriptor();
                descriptor.ControllerName = name;
                descriptor.ControllerTypeInfo = controllerType.GetTypeInfo();
                descriptor.ActionName = methodInfo.Name;
                descriptor.DisplayName = methodInfo.Name;
                descriptor.MethodInfo = methodInfo;
                descriptor.Parameters = methodInfo
                    .GetParameters()
                    .Select(p => new ParameterDescriptor
                    {
                        Name = p.Name,
                        ParameterType = p.ParameterType
                    })
                    .ToList();
                descriptors.Add(descriptor);

                // For attribute routing tests, stash the root service provider on the descriptor.
                descriptor.Properties["serviceProvider"] = routeBuilder.ServiceProvider;
            }

            // Add these descriptors to the global IActionDescriptorCollectionProvider.
            TestActionDescriptorCollectionProvider actionDescriptorCollectionProvider =
                    routeBuilder.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>()
                    as TestActionDescriptorCollectionProvider;

            actionDescriptorCollectionProvider.TestActionDescriptors.AddRange(descriptors);

            return descriptors;
        }

        /// <summary>
        /// Initializes a new collection of the [Http]ControllerDescriptor class.
        /// </summary>
        /// <returns>A new collection of the [Http]ControllerDescriptor  class.</returns>
        public static IEnumerable<ControllerActionDescriptor> CreateCollection()
        {
            return new ControllerActionDescriptor[0];
        }
    }
}
