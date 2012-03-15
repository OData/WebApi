using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http.Common;
using System.Web.Http.Filters;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    public class ReflectedHttpActionDescriptor : HttpActionDescriptor
    {
        private readonly Lazy<Collection<HttpParameterDescriptor>> _parameters;

        private ActionExecutor _actionExecutor;
        private MethodInfo _methodInfo;
        private string _actionName;
        private Collection<HttpMethod> _supportedHttpMethods;

        // Getting custom attributes via reflection is slow. 
        // But iterating over a object[] to pick out specific types is fast. 
        // Furthermore, many different services may call to ask for different attributes, so we have multiple callers. 
        // That means there's not a single cache for the callers, which means there's some value caching here.
        // This cache can be a 2x speedup in some benchmarks.
        private object[] _attrCached;

        private static readonly HttpMethod[] _supportedHttpMethodsByConvention = { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Head, HttpMethod.Options, new HttpMethod("PATCH") };

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

        public override Type ReturnType
        {
            get { return _methodInfo == null ? null : _methodInfo.ReturnType; }
        }

        public override Collection<T> GetCustomAttributes<T>()
        {
            Contract.Assert(_methodInfo != null); // can't get attributes without the method set!
            Contract.Assert(_attrCached != null); // setting the method should build the attr cache
            return new Collection<T>(TypeHelper.OfType<T>(_attrCached));
        }

        public override object Execute(HttpControllerContext controllerContext, IDictionary<string, object> arguments)
        {
            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (arguments == null)
            {
                throw Error.ArgumentNull("arguments");
            }

            object[] argumentValues = PrepareParameters(arguments, controllerContext);
            return _actionExecutor.Execute(controllerContext.Controller, argumentValues);
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
            _actionExecutor = new ActionExecutor(methodInfo);
            _attrCached = _methodInfo.GetCustomAttributes(inherit: true);
            CacheAttrsIActionMethodSelector = _attrCached.OfType<IActionMethodSelector>().ToArray();
            _actionName = GetActionName(_methodInfo, _attrCached);
            _supportedHttpMethods = GetSupportedHttpMethods(_actionName, _attrCached);
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

        private static Collection<HttpMethod> GetSupportedHttpMethods(string actionName, object[] actionAttributes)
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
                // Get HttpMethod from action name convention 
                for (int i = 0; i < _supportedHttpMethodsByConvention.Length; i++)
                {
                    if (actionName.StartsWith(_supportedHttpMethodsByConvention[i].Method, StringComparison.OrdinalIgnoreCase))
                    {
                        supportedHttpMethods.Add(_supportedHttpMethodsByConvention[i]);
                        break;
                    }
                }
            }

            return supportedHttpMethods;
        }

        private sealed class ActionExecutor
        {
            private Func<object, object[], object> _executor;

            public ActionExecutor(MethodInfo methodInfo)
            {
                Contract.Assert(methodInfo != null);
                _executor = GetExecutor(methodInfo);
            }

            public object Execute(object instance, object[] arguments)
            {
                return _executor(instance, arguments);
            }

            private static Func<object, object[], object> GetExecutor(MethodInfo methodInfo)
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
                    Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(methodCall, instanceParameter, parametersParameter);
                    Action<object, object[]> voidExecutor = lambda.Compile();
                    return (instance, methodParameters) =>
                    {
                        voidExecutor(instance, methodParameters);
                        return null;
                    };
                }
                else
                {
                    // must coerce methodCall to match Func<object, object[], object> signature
                    UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                    Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(castMethodCall, instanceParameter, parametersParameter);
                    return lambda.Compile();
                }
            }
        }
    }
}
