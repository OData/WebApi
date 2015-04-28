using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model
{
    public class BaseEntity
    {
        public BaseEntity()
        {
        }

        public BaseEntity(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        internal class IdEqualityComparer : IEqualityComparer<BaseEntity>
        {
            public bool Equals(BaseEntity x, BaseEntity y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(BaseEntity obj)
            {
                return obj.Id;
            }
        }
    }
}
