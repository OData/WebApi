using System.Collections.Generic;

namespace System.Web.OData.Query.Expressions
{
	internal abstract partial class PropertyContainer
	{
        // Entityframework requires that the two different type initializers for a given type in the same query have the same set of properties in the same order.
        // A $select=Prop1,Prop2,Prop3 where Prop1 and Prop2 are of the same type without this extra NamedPropertyWithNext type results in an select expression that looks like,
        //      c => new NamedProperty<int> { Name = "Prop1", Value = c.Prop1, Next0 = new NamedProperty<int> { Name = "Prop2", Value = c.Prop2 }, Next2 = new NamedProperty<int> { Name = "Prop3", Value = c.Prop3 } };
        // Entityframework cannot translate this expression as the first NamedProperty<int> initialization has Next and the second one doesn't. Also, Entityframework cannot 
        // create null's of NamedProperty<T>. So, you cannot generate an expression like new NamedProperty<int> { Next = null }. The exception that EF throws looks like this,
        // "The type 'NamedProperty`1[SystemInt32...]' appears in two structurally incompatible initializations within a single LINQ to Entities query. 
        // A type can be initialized in two places in the same query, but only if the same properties are set in both places and those properties are set in the same order."

			private class SingleExpandedPropertyWithNext0<T> : SingleExpandedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext1<T> : SingleExpandedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext2<T> : SingleExpandedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext3<T> : SingleExpandedPropertyWithNext2<T>
        {
            public PropertyContainer Next3 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next3.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext4<T> : SingleExpandedPropertyWithNext3<T>
        {
            public PropertyContainer Next4 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next4.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext5<T> : SingleExpandedPropertyWithNext4<T>
        {
            public PropertyContainer Next5 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next5.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext6<T> : SingleExpandedPropertyWithNext5<T>
        {
            public PropertyContainer Next6 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next6.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class SingleExpandedPropertyWithNext7<T> : SingleExpandedPropertyWithNext6<T>
        {
            public PropertyContainer Next7 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next7.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
		private static List<Type> SingleExpandedPropertyTypes = new List<Type> {
		typeof(SingleExpandedProperty<>),
				typeof(SingleExpandedPropertyWithNext0<>),
					typeof(SingleExpandedPropertyWithNext1<>),
					typeof(SingleExpandedPropertyWithNext2<>),
					typeof(SingleExpandedPropertyWithNext3<>),
					typeof(SingleExpandedPropertyWithNext4<>),
					typeof(SingleExpandedPropertyWithNext5<>),
					typeof(SingleExpandedPropertyWithNext6<>),
					typeof(SingleExpandedPropertyWithNext7<>),
			};
			
			
			
			
			
			
			
			
			private class CollectionExpandedPropertyWithNext0<T> : CollectionExpandedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext1<T> : CollectionExpandedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext2<T> : CollectionExpandedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext3<T> : CollectionExpandedPropertyWithNext2<T>
        {
            public PropertyContainer Next3 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next3.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext4<T> : CollectionExpandedPropertyWithNext3<T>
        {
            public PropertyContainer Next4 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next4.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext5<T> : CollectionExpandedPropertyWithNext4<T>
        {
            public PropertyContainer Next5 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next5.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext6<T> : CollectionExpandedPropertyWithNext5<T>
        {
            public PropertyContainer Next6 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next6.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class CollectionExpandedPropertyWithNext7<T> : CollectionExpandedPropertyWithNext6<T>
        {
            public PropertyContainer Next7 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next7.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
		private static List<Type> CollectionExpandedPropertyTypes = new List<Type> {
		typeof(CollectionExpandedProperty<>),
				typeof(CollectionExpandedPropertyWithNext0<>),
					typeof(CollectionExpandedPropertyWithNext1<>),
					typeof(CollectionExpandedPropertyWithNext2<>),
					typeof(CollectionExpandedPropertyWithNext3<>),
					typeof(CollectionExpandedPropertyWithNext4<>),
					typeof(CollectionExpandedPropertyWithNext5<>),
					typeof(CollectionExpandedPropertyWithNext6<>),
					typeof(CollectionExpandedPropertyWithNext7<>),
			};
			
			
			
			
			
			
			
			
			private class AutoSelectedNamedPropertyWithNext0<T> : AutoSelectedNamedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext1<T> : AutoSelectedNamedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext2<T> : AutoSelectedNamedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext3<T> : AutoSelectedNamedPropertyWithNext2<T>
        {
            public PropertyContainer Next3 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next3.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext4<T> : AutoSelectedNamedPropertyWithNext3<T>
        {
            public PropertyContainer Next4 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next4.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext5<T> : AutoSelectedNamedPropertyWithNext4<T>
        {
            public PropertyContainer Next5 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next5.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext6<T> : AutoSelectedNamedPropertyWithNext5<T>
        {
            public PropertyContainer Next6 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next6.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class AutoSelectedNamedPropertyWithNext7<T> : AutoSelectedNamedPropertyWithNext6<T>
        {
            public PropertyContainer Next7 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next7.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
		private static List<Type> AutoSelectedNamedPropertyTypes = new List<Type> {
		typeof(AutoSelectedNamedProperty<>),
				typeof(AutoSelectedNamedPropertyWithNext0<>),
					typeof(AutoSelectedNamedPropertyWithNext1<>),
					typeof(AutoSelectedNamedPropertyWithNext2<>),
					typeof(AutoSelectedNamedPropertyWithNext3<>),
					typeof(AutoSelectedNamedPropertyWithNext4<>),
					typeof(AutoSelectedNamedPropertyWithNext5<>),
					typeof(AutoSelectedNamedPropertyWithNext6<>),
					typeof(AutoSelectedNamedPropertyWithNext7<>),
			};
			
			
			
			
			
			
			
			
			private class NamedPropertyWithNext0<T> : NamedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext1<T> : NamedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext2<T> : NamedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext3<T> : NamedPropertyWithNext2<T>
        {
            public PropertyContainer Next3 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next3.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext4<T> : NamedPropertyWithNext3<T>
        {
            public PropertyContainer Next4 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next4.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext5<T> : NamedPropertyWithNext4<T>
        {
            public PropertyContainer Next5 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next5.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext6<T> : NamedPropertyWithNext5<T>
        {
            public PropertyContainer Next6 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next6.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
			private class NamedPropertyWithNext7<T> : NamedPropertyWithNext6<T>
        {
            public PropertyContainer Next7 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next7.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
		private static List<Type> NamedPropertyTypes = new List<Type> {
		typeof(NamedProperty<>),
				typeof(NamedPropertyWithNext0<>),
					typeof(NamedPropertyWithNext1<>),
					typeof(NamedPropertyWithNext2<>),
					typeof(NamedPropertyWithNext3<>),
					typeof(NamedPropertyWithNext4<>),
					typeof(NamedPropertyWithNext5<>),
					typeof(NamedPropertyWithNext6<>),
					typeof(NamedPropertyWithNext7<>),
			};
			
			
			
			
			
			
			
			
	
	}
}