using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http.Formatting
{
    // Contract resolver to handle types that DCJS supports, but Json.NET doesn't support out of the box (like [Serializable])
    internal class JsonContractResolver : DefaultContractResolver
    {
        private const BindingFlags AllInstanceMemberFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected override JsonObjectContract CreateObjectContract(Type type)
        {
            JsonObjectContract contract = base.CreateObjectContract(type);

            // Handling [Serializable] types
            if (type.IsSerializable && !IsTypeNullable(type) && !IsTypeDataContract(type) && !IsTypeJsonObject(type))
            {
                contract.Properties.Clear();
                foreach (JsonProperty property in CreateSerializableJsonProperties(type))
                {
                    contract.Properties.Add(property);
                }
            }
            return contract;
        }

        private static IEnumerable<JsonProperty> CreateSerializableJsonProperties(Type type)
        {
            return type.GetFields(AllInstanceMemberFlag)
                .Where(field => !field.IsNotSerialized)
                .Select(field => PrivateMemberContractResolver.Instance.CreatePrivateProperty(field, MemberSerialization.OptOut));
        }

        private static bool IsTypeNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        private static bool IsTypeDataContract(Type type)
        {
            return type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;
        }

        private static bool IsTypeJsonObject(Type type)
        {
            return type.GetCustomAttributes(typeof(JsonObjectAttribute), false).Length > 0;
        }

        private class PrivateMemberContractResolver : DefaultContractResolver
        {
            internal static PrivateMemberContractResolver Instance = new PrivateMemberContractResolver();

            internal PrivateMemberContractResolver()
            {
                DefaultMembersSearchFlags = JsonContractResolver.AllInstanceMemberFlag;
            }

            internal JsonProperty CreatePrivateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                return CreateProperty(member, memberSerialization);
            }
        }
    }
}
