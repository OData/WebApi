namespace System.Web.Razor.Parser
{
    [Flags]
    public enum BalancingModes
    {
        None = 0,
        BacktrackOnFailure = 1,
        NoErrorOnFailure = 2,
        AllowCommentsAndTemplates = 4,
        AllowEmbeddedTransitions = 8
    }
}
