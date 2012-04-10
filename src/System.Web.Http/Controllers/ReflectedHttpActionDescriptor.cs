// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// An action descriptor representing a reflected synchronous or asynchronous action method.
    /// </summary>
    public class ReflectedHttpActionDescriptor : HttpActionDescriptor
    {
        private static readonly object[] _empty = new object[0];

        private readonly Lazy<Collection<HttpParameterDescriptor>> _parameters;

        private Lazy<ActionExecutor> _actionExecutor;
        private MethodInfo _methodInfo;
        private Type _returnType;
        private string _actionName;
        private Collection<HttpMethod> _supportedHttpMethods;

        // Getting custom attributes via reflection is slow. 
        // But iterating over a object[] to pick out specific types is fast. 
        // Furthermore, many different services may call to ask for different attributes, so we have multiple callers. 
        // That means there's not a single cache for the callers, which means there's some value caching here.
        // This cache can be a 2x speedup in some benchmarks.
        private object[] _attrCached;

        private static readonly HttpMethod[] _supportedHttpMethodsByConvention = 
        { 
            HttpMethod.Get, 
            HttpMethod.Post, 
            HttpMethod.Put, 
            HttpMethod.Delete, 
            HttpMethod.Head, 
            HttpMethod.Options, 
            new HttpMethod("PATCH") 
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedHttpActionDescriptor"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public ReflectedHttpActionDescriptor()
        {
            _parameters = new Lazy<Collection<HttpParameterDescriptor>>(() => InitializeParameterDescriptors());
            _supportedHttpMethods = new Collection<HttpMethod>();
        }

        public ReflectedHttpActionDescriptor(HttpControllerDescriptor controllerDescriptor, MethodInfo methodInfo)
            : base(controllerDescriptor)
        {
            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }

            InitializeProperties(methodInfo);
            _parameters = new Lazy<Collection<HttpParameterDescriptor>>(() => InitializeParameterDescriptors());
        }

        /// <summary>
        /// Caches that the ActionSelector use.
        /// </summary>
        internal IActionMethodSelector[] CacheAttrsIActionMethodSelector { get; private set; }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public override Collection<HttpMethod> SupportedHttpMethods
        {
            get { return _supportedHttpMethods; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                InitializeProperties(value);
            }
        }

        /// <summary>
        /// The return type of the method or <c>null</c> if the method does not return a value (e.g. a method returning
        /// <c>void</c>).
        /// </summary>
        /// <remarks>
        /// This implementation returns the exact value of <see cref="System.Reflection.MethodInfo.ReturnType"/> for 
        /// synchronous methods and an unwrapped value for asynchronous methods (e.g. the <c>T</c> of <see cref="Task{T}"/>.
        /// This returns <c>null</c> for methods returning <c>void</c> or <see cref="Task"/>.
        /// </remarks>
        public override Type ReturnType
        {
            get { return _returnType; }
        }

        public override Collection<T> GetCustomAttributes<T>()
        {
            Contract.Assert(_methodInfo != null); // can't get attributes without the method set!
            Contract.Assert(_attrCached != null); // setting the method should build the attribute cache
            return new Collection<T>(TypeHelper.OfType<T>(_attrCached));
        }

        /// <summary>
        /// Executes the described action and returns a <see cref="Task{T}"/> that once completed will
        /// contain the return value of the action.
        /// </summary>
        /// <param name="controllerContext">The context.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>A <see cref="Task{T}"/> that once completed will contain the return value of the action.</returns>
        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (arguments == null)
            {
                throw Error.ArgumentNull("arguments");
            }

            return TaskHelpers.RunSynchronously(() =>
            {
                object[] argumentValues = PrepareParameters(arguments, controllerContext);
                return _actionExecutor.Value.Execute(controllerContext.Controller, argumentValues);
            });
        }

        public override Collection<IFilter> GetFilters()
        {
            return new Collection<IFilter>(GetCustomAttributes<IFilter>().Concat(base.GetFilters()).ToList());
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            return _parameters.Value;
        }

        private void InitializeProperties(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            _returnType = GetReturnType(methodInfo);
            _actionExecutor = new Lazy<ActionExecutor>(() => InitializeActionExecutor(_methodInfo));
            _attrCached = _methodInfo.GetCustomAttributes(inherit: true);
            CacheAttrsIActionMethodSelector = _attrCached.OfType<IActionMethodSelector>().ToArray();
            _actionName = GetActionName(_methodInfo, _attrCached);
            _supportedHttpMethods = GetSupportedHttpMethods(_methodInfo, _attrCached);
        }

        internal static Type GetReturnType(MethodInfo methodInfo)
        {
            Type result = methodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(result))
            {
                result = TypeHelper.GetTaskInnerTypeOrNull(methodInfo.ReturnType);
            }
            if (result == typeof(void))
            {
                result = null;
            }
            return result;
        }

        private Collection<HttpParameterDescriptor> InitializeParameterDescriptors()
        {
            Contract.Assert(_methodInfo != null);

            List<HttpParameterDescriptor> parameterInfos = _methodInfo.GetParameters().Select(
                (item) => new ReflectedHttpParameterDescriptor(this, item)).ToList<HttpParameterDescriptor>();
            return new Collection<HttpParameterDescriptor>(parameterInfos);
        }

        private object[] PrepareParameters(IDictionary<string, object> parameters, HttpControllerContext controllerContext)
        {
            // This is on a hotpath, so a quick check to avoid the allocation if we have no parameters. 
            if (_parameters.Value.Count == 0)
            {
                return _empty;
            }

            ParameterInfo[] parameterInfos = MethodInfo.GetParameters();
            var rawParameterValues = from parameterInfo in parameterInfos
                                     select ExtractParameterFromDictionary(parameterInfo, parameters, controllerContext);
            object[] parametersArray = rawParameterValues.ToArray();
            return parametersArray;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        private object ExtractParameterFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters, HttpControllerContext controllerContext)
        {
            object value;

            if (!parameters.TryGetValue(parameterInfo.Name, out value))
            {
                // the key should always be present, even if the parameter value is null
                throw new HttpResponseException(controllerContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterNotInDictionary,
                                 parameterInfo.Name, parameterInfo.ParameterType, MethodInfo, MethodInfo.DeclaringType)));
            }

            if (value == null && !TypeHelper.TypeAllowsNullValue(parameterInfo.ParameterType))
            {
                // tried to pass a null value for a non-nullable parameter type
                throw new HttpResponseException(controllerContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterCannotBeNull,
                                    parameterInfo.Name, parameterInfo.ParameterType, MethodInfo, MethodInfo.DeclaringType)));
            }

            if (value != null && !parameterInfo.ParameterType.IsInstanceOfType(value))
            {
                // value was supplied but is not of the proper type
                throw new HttpResponseException(controllerContext.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    Error.Format(SRResources.ReflectedActionDescriptor_ParameterValueHasWrongType,
                                    parameterInfo.Name, MethodInfo, MethodInfo.DeclaringType, value.GetType(), parameterInfo.ParameterType)));
            }

            return value;
        }

        private static string GetActionName(MethodInfo methodInfo, object[] actionAttributes)
        {
            ActionNameAttribute nameAttribute = TypeHelper.OfType<ActionNameAttribute>(actionAttributes).FirstOrDefault();
            return nameAttribute != null
                       ? nameAttribute.Name
                       : methodInfo.Name;
        }

        private static Collection<HttpMethod> GetSupportedHttpMethods(MethodInfo methodInfo, object[] actionAttributes)
        {
            Collection<HttpMethod> supportedHttpMethods = new Collection<HttpMethod>();
            ICollection<IActionHttpMethodProvider> httpMethodProviders = TypeHelper.OfType<IActionHttpMethodProvider>(actionAttributes);
            if (httpMethodProviders.Count > 0)
            {
                // Get HttpMethod from attributes
                foreach (IActionHttpMethodProvider httpMethodSelector in httpMethodProviders)
                {
                    foreach (HttpMethod httpMethod in httpMethodSelector.HttpMethods)
                    {
                        supportedHttpMethods.Add(httpMethod);
                    }
                }
            }
            else
            {
                // Get HttpMethod from method name convention 
                for (int i = 0; i < _supportedHttpMethodsByConvention.Length; i++)
                {
                    if (methodInfo.Name.StartsWith(_supportedHttpMethodsByConvention[i].Method, StringComparison.OrdinalIgnoreCase))
                    {
                        supportedHttpMethods.Add(_supportedHttpMethodsByConvention[i]);
                        break;
                    }
                }
            }

            if (supportedHttpMethods.Count == 0)
            {
                // Use POST as the default HttpMethod
                supportedHttpMethods.Add(HttpMethod.Post);
            }

            return supportedHttpMethods;
        }

        private static ActionExecutor InitializeActionExecutor(MethodInfo methodInfo)
        {
            if (methodInfo.ContainsGenericParameters)
            {
                throw Error.InvalidOperation(SRResources.ReflectedHttpActionDescriptor_CannotCallOpenGenericMethods,
                                     methodInfo, methodInfo.ReflectedType.FullName);
            }

            return new ActionExecutor(methodInfo);
        }

        private sealed class ActionExecutor
        {
            private static readonly Task<object> _completedTaskReturningNull = TaskHelpers.FromResult<object>(null);
            private readonly Func<object, object[], Task<object>> _executor;
            private static MethodInfo _convertOfTMethod = typeof(ActionExecutor).GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic);

            public ActionExecutor(MethodInfo methodInfo)
            {
                Contract.Assert(methodInfo != null);
                _executor = GetExecutor(methodInfo);
            }

            public Task<object> Execute(object instance, object[] arguments)
            {
                return _executor(instance, arguments);
            }

            // Method called via reflection.
            private static Task<object> Convert<T>(object taskAsObject)
            {
                Task<T> task = (Task<T>)taskAsObject;
                return task.Then(r => (object)r);
            }

            // Do not inline or optimize this method to avoid stack-related reflection demand issues when
            // running from the GAC in medium trust
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private static Func<object, Task<object>> CompileGenericTaskConversionDelegate(Type taskValueType)
            {
                Contract.Assert(taskValueType != null);

                return (Func<object, Task<object>>)Delegate.CreateDelegate(typeof(Func<object, Task<object>>), _convertOfTMethod.MakeGenericMethod(taskValueType));
            }

            private static Func<object, object[], Task<object>> GetExecutor(MethodInfo methodInfo)
            {
                // Parameters to executor
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

                // Build parameter list
                List<Expression> parameters = new List<Expression>();
                ParameterInfo[] paramInfos = methodInfo.GetParameters();
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    ParameterInfo paramInfo = paramInfos[i];
                    BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                    UnaryExpression valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                    // valueCast is "(Ti) parameters[i]"
                    parameters.Add(valueCast);
                }

                // Call method
                UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(instanceParameter, methodInfo.ReflectedType) : null;
                MethodCallExpression methodCall = methodCall = Expression.Call(instanceCast, methodInfo, parameters);

                // methodCall is "((MethodInstanceType) instance).method((T0) parameters[0], (T1) parameters[1], ...)"
                // Create function
                if (methodCall.Type == typeof(void))
                {
                    // for: public void Action()
                    Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);
                    Action<object, object[]> voidExecutor = lambda.Compile();
                    return (instance, methodParameters) =>
                    {
                        voidExecutor(instance, methodParameters);
                        return _completedTaskReturningNull;
                    };
                }
                else
                {
                    // must coerce methodCall to match Func<object, object[], object> signature
                    UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                    Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);
                    Func<object, object[], object> compiled = lambda.Compile();
                    if (methodCall.Type == typeof(Task))
                    {
                        // for: public Task Action()
                        return (instance, methodParameters) =>
                        {
                            Task r = (Task)compiled(instance, methodParameters);
                            ThrowIfWrappedTaskInstance(methodInfo, r.GetType());
                            return r.Then(() => (object)null);
                        };
                    }
                    else if (typeof(Task).IsAssignableFrom(methodCall.Type))
                    {
                        // for: public Task<T> Action()
                        // constructs: return (Task<object>)Convert<T>(((Task<T>)instance).method((T0) param[0], ...))
                        Type taskValueType = TypeHelper.GetTaskInnerTypeOrNull(methodCall.Type);
                        var compiledConversion = CompileGenericTaskConversionDelegate(taskValueType);

                        return (instance, methodParameters) =>
                        {
                            object callResult = compiled(instance, methodParameters);
                            Task<object> convertedResult = compiledConversion(callResult);
                            return convertedResult;
                        };
                    }
                    else
                    {
                        // for: public T Action()
                        return (instance, methodParameters) =>
                        {
                            var result = compiled(instance, methodParameters);
                            // Throw when the result of a method is Task. Asynchronous methods need to declare that they
                            // return a Task.
                            Task resultAsTask = result as Task;
                            if (resultAsTask != null)
                            {
                                throw Error.InvalidOperation(SRResources.ActionExecutor_UnexpectedTaskInstance,
                                    methodInfo.Name, methodInfo.DeclaringType.Name);
                            }
                            return TaskHelpers.FromResult(result);
                        };
                    }
                }
            }

            private static void ThrowIfWrappedTaskInstance(MethodInfo method, Type type)
            {
                // Throw if a method declares a return type of Task and returns an instance of Task<Task> or Task<Task<T>>
                // This most likely indicates that the developer forgot to call Unwrap() somewhere.
                Contract.Assert(method.ReturnType == typeof(Task));
                // Fast path: check if type is exactly Task first.
                if (type != typeof(Task))
                {
                    Type innerTaskType = TypeHelper.GetTaskInnerTypeOrNull(type);
                    if (innerTaskType != null && typeof(Task).IsAssignableFrom(innerTaskType))
                    {
                        throw Error.InvalidOperation(SRResources.ActionExecutor_WrappedTaskInstance,
                            method.Name, method.DeclaringType.Name, type.FullName);
                    }
                }
            }
        }
    }
}
