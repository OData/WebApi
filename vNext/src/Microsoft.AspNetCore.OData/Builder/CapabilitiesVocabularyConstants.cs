// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.OData.Builder
{
    /// <summary>
    /// Constant values for Capabilities Vocabulary
    /// </summary>
    internal static class CapabilitiesVocabularyConstants
    {
        /// <summary>Org.OData.Capabilities.V1.CountRestrictions</summary>
        public const string CountRestrictions = "Org.OData.Capabilities.V1.CountRestrictions";

        /// <summary>Property Countable of Org.OData.Capabilities.V1.CountRestrictions</summary>
        public const string CountRestrictionsCountable = "Countable";

        /// <summary>Property NonCountableProperties of Org.OData.Capabilities.V1.CountRestrictions</summary>
        public const string CountRestrictionsNonCountableProperties = "NonCountableProperties";

        /// <summary>Property NonCountableNavigationProperties of Org.OData.Capabilities.V1.CountRestrictions</summary>
        public const string CountRestrictionsNonCountableNavigationProperties = "NonCountableNavigationProperties";

        /// <summary>Org.OData.Capabilities.V1.NavigationRestrictions</summary>
        public const string NavigationRestrictions = "Org.OData.Capabilities.V1.NavigationRestrictions";

        /// <summary>Property Navigability of Org.OData.Capabilities.V1.NavigationRestrictions</summary>
        public const string NavigationRestrictionsNavigability = "Navigability";

        /// <summary>Property RestrictedProperties of Org.OData.Capabilities.V1.NavigationRestrictions</summary>
        public const string NavigationRestrictionsRestrictedProperties = "RestrictedProperties";

        /// <summary>Property NavigationProperty of Org.OData.Capabilities.V1.NavigationPropertyRestriction</summary>
        public const string NavigationPropertyRestrictionNavigationProperty = "NavigationProperty";

        /// <summary>Org.OData.Capabilities.V1.NavigationType</summary>
        public const string NavigationType = "Org.OData.Capabilities.V1.NavigationType";

        /// <summary>Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string FilterRestrictions = "Org.OData.Capabilities.V1.FilterRestrictions";

        /// <summary>Property Filterable of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string FilterRestrictionsFilterable = "Filterable";

        /// <summary>Property RequiresFilter of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string FilterRestrictionsRequiresFilter = "RequiresFilter";

        /// <summary>Property RequiredProperties of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string FilterRestrictionsRequiredProperties = "RequiredProperties";

        /// <summary>Property NonFilterableProperties of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string FilterRestrictionsNonFilterableProperties = "NonFilterableProperties";

        /// <summary>Org.OData.Capabilities.V1.SortRestrictions</summary>
        public const string SortRestrictions = "Org.OData.Capabilities.V1.SortRestrictions";

        /// <summary>Property Sortable of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string SortRestrictionsSortable = "Sortable";

        /// <summary>Property AscendingOnlyProperties of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string SortRestrictionsAscendingOnlyProperties = "AscendingOnlyProperties";

        /// <summary>Property DescendingOnlyProperties of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string SortRestrictionsDescendingOnlyProperties = "DescendingOnlyProperties";

        /// <summary>Property NonSortableProperties of Org.OData.Capabilities.V1.FilterRestrictions</summary>
        public const string SortRestrictionsNonSortableProperties = "NonSortableProperties";

        /// <summary>Org.OData.Capabilities.V1.ExpandRestrictions</summary>
        public const string ExpandRestrictions = "Org.OData.Capabilities.V1.ExpandRestrictions";

        /// <summary>Property Expandable of Org.OData.Capabilities.V1.ExpandRestrictions</summary>
        public const string ExpandRestrictionsExpandable = "Expandable";

        /// <summary>Property NonExpandableProperties of Org.OData.Capabilities.V1.ExpandRestrictions</summary>
        public const string ExpandRestrictionsNonExpandableProperties = "NonExpandableProperties";
    }
}
