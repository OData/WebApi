namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public class InteriorDefinitionAnnotation
    {
        public int DefinitionID { get; }

        public InteriorDefinitionAnnotation(int definitionId)
        {
            DefinitionID = definitionId;
        }
    }
}