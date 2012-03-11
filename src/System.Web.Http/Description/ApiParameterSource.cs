namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes where the parameter come from.
    /// </summary>
    public enum ApiParameterSource
    {
        FromUri = 0,
        FromBody,
        Unknown
    }
}
