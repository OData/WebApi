using System.Runtime.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [DataContract]
    public enum DataContractEnum
    {
        [EnumMember]
        First,

        [EnumMember]
        Second,

        Third
    }
}
