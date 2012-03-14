using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;

namespace System.Web.Http.Filters
{
    /// <summary>
    /// An action filter that eagerly evaluates the results of any actions methods that return <see cref="IEnumerable{T}"/> or <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>
    /// This filter is required to run so that any lazily evaluated results are evaluated within the <see cref="ApiController"/> filter pipeline.
    /// This ensures that any exceptions thrown during the evaluation will be processes by any registered exception filters and
    /// that any context objects backing an <see cref="IQueryable{T}"/> result are safe to be disposed in the controller.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class EnumerableEvaluatorFilter : ActionFilterAttribute
    {
        private static object _conversionDelegateCacheKey = new object();
        private static MethodInfo _convertMethod = typeof(EnumerableEvaluatorFilter).GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic);
        private static EnumerableEvaluatorFilter _instance = new EnumerableEvaluatorFilter();

        internal static EnumerableEvaluatorFilter Instance
        {
            get { return _instance; }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            HttpResponseMessage response = actionExecutedContext.Result;
            IEnumerable valueAsEnumerable;
            if (response == null || !response.TryGetContentValue(out valueAsEnumerable))
            {
                return;
            }

            HttpActionDescriptor actionDescriptor = actionExecutedContext.ActionContext.ActionDescriptor;
            Type declaredContentType = TypeHelper.GetUnderlyingContentInnerType(actionDescriptor.ReturnType);

            if (!IsSupportedDeclaredContentType(declaredContentType))
            {
                return;
            }

            if (!declaredContentType.IsAssignableFrom(valueAsEnumerable.GetType()))
            {
                // If the current value in the response message is no longer of a type that's compatible with
                // the action method's declared content type then do nothing. This could happen if some other filter
                // decided to short-circuit the response with a different content.
                return;
            }

            Func<object, object> conversionDelegate = GetConversionDelegate(actionDescriptor, declaredContentType);

            if (conversionDelegate != null)
            {
                object valueAsList = conversionDelegate(valueAsEnumerable);
                ((ObjectContent)response.Content).Value = valueAsList;
            }
        }

        private static Func<object, object> GetConversionDelegate(HttpActionDescriptor actionDescriptor, Type contentType)
        {
            Contract.Assert(actionDescriptor != null);
            Contract.Assert(contentType != null);
            Contract.Assert(contentType.IsGenericType);
            Contract.Assert((contentType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || contentType.GetGenericTypeDefinition() == typeof(IQueryable<>)));

            Func<object, object> conversionDelegate = actionDescriptor.Properties.GetOrAdd<Func<object, object>>(_conversionDelegateCacheKey, _ =>
            {
                Type genericEnumerableType = TypeHelper.ExtractGenericInterface(contentType, typeof(IEnumerable<>));
                Type enumerableParameterType = TypeHelper.GetTypeArgumentsIfMatch(genericEnumerableType, typeof(IEnumerable<>))[0];
                return CompileConversionDelegate(enumerableParameterType);
            });
            return conversionDelegate;
        }

        internal static bool IsSupportedDeclaredContentType(Type contentType)
        {
            Contract.Assert(contentType != null);
            // Only action methods that declare their returned content type as exactly IEnumerable<T> or
            // IQueryable<T> are supported by this filter. Derived types (i.e. List<T>, etc) will not be
            // processed by this filter.
            if (!contentType.IsGenericType)
            {
                return false;
            }
            Type genericTypeDefinition = contentType.GetGenericTypeDefinition();
            return genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(IQueryable<>);
        }

        // Do not inline or optimize this method to avoid stack-related reflection demand issues when
        // running from the GAC in medium trust
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static Func<object, object> CompileConversionDelegate(Type itemType)
        {
            Contract.Assert(itemType != null);

            return (Func<object, object>)Delegate.CreateDelegate(typeof(Func<object, object>), _convertMethod.MakeGenericMethod(itemType));
        }

        private static object Convert<T>(object input)
        {
            // This method is called from the delegate constructed in CompileConversionDelegate()
            IEnumerable<T> result = new List<T>((IEnumerable<T>)input);
            if (input is IQueryable<T>)
            {
                // If the input is actually an IQueryable<T> return the result also typed as IQueryable<T>
                result = result.AsQueryable<T>();
            }
            return result;
        }
    }
}
