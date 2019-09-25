namespace AspNetCoreODataSample.DynamicModels.Web.Models
{
    public interface IUserDefinedPropertyBag
    {
        string StringProperty1 { get; set; }
        string StringProperty2 { get; set; }
        string StringProperty3 { get; set; }
        int IntProperty1 { get; set; }
        int IntProperty2 { get; set; }
        int IntProperty3 { get; set; }
        double DoubleProperty1 { get; set; }
        double DoubleProperty2 { get; set; }
        double DoubleProperty3 { get; set; }

    }
}