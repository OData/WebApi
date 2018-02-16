// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Query.Expressions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

using Address = System.Web.OData.Builder.TestModels.Address;

namespace System.Web.OData.Test.OData.Query
{
    public class ApplyQueryOptionTest
    {
        // Legal apply queries usable against CustomerApplyTestData.
        // Tuple is: apply, expected number
        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestApplies
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "aggregate($count as Count)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Count", 5L} }
                        }
                    },
                    {
                        "aggregate(CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15} }
                        }
                    },
                    {
                        "aggregate(cast(CustomerId, Edm.Int64) with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15L} }
                        }
                    },
                    {
                        "aggregate(SharePrice with sum as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 22.5M} }
                        }
                    },
                    {
                        "aggregate(SharePrice with min as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 2.5M} }
                        }
                    },
                     {
                        "aggregate(SharePrice with max as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 10M} }
                        }
                    },
                    {
                        "aggregate(SharePrice with average as SharePrice)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePrice", 7.5M} }
                        }
                    },
                    {
                        "aggregate(CustomerId with sum as Total, SharePrice with countdistinct as SharePriceDistinctCount)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "SharePriceDistinctCount", 3L}, { "Total", 15} }
                        }
                    },
                    {
                        "groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} },
                            new Dictionary<string, object> { { "Name", "Highest"} },
                            new Dictionary<string, object> { { "Name", "Middle"} }
                        }
                    },
                    {
                        "groupby((Name), aggregate(CustomerId with sum as Total))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Total", 10} },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Total", 2} },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                        }
                    },
                    {
                        "groupby((Name), aggregate($count as Count))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"}, { "Count", 3L} },
                            new Dictionary<string, object> { { "Name", "Highest"}, { "Count", 1L} },
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Count", 1L} }
                        }
                    },
                    {
                        "filter(Name eq 'Lowest')/groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} }
                        }
                    },
                    {
                        "groupby((Name), aggregate(CustomerId with sum as Total))/filter(Total eq 3)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Middle"}, { "Total", 3 } }
                        }
                    },
                    {
                        "groupby((Name))/filter(Name eq 'Lowest')",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Name", "Lowest"} }
                        }
                    },
                    {
                        "groupby((Address/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Address/City", "redmond"} },
                            new Dictionary<string, object> { { "Address/City", "seattle"} },
                            new Dictionary<string, object> { { "Address/City", "hobart"} },
                            new Dictionary<string, object> { { "Address/City", null} },
                        }
                    },
                    {
                        "groupby((Address/City, Address/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Address/City", "redmond"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "Address/City", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "Address/City", "hobart"}, { "Address/State", null} },
                            new Dictionary<string, object> { { "Address/City", null}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/City, Address/State))/groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(startswith(Address/City, 's') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(endswith(Address/City, 't') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", false}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(contains(Address/City, 'o') with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", true}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(length(Address/City) with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", 7}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", 6}, { "Address/State", null} },
                        }
                    },
                    {
                        "aggregate(year(StartDate) with max as MaxYear, year(StartDate) with min as MinYear)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxYear", 2018}, { "MinYear", 2016} },
                        }
                    },
                    {
                        "aggregate(month(StartDate) with max as MaxMonth, month(StartDate) with min as MinMonth)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxMonth", 5}, { "MinMonth", 1} },
                        }
                    },
                    {
                        "aggregate(day(StartDate) with max as MaxDay, day(StartDate) with min as MinDay)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxDay", 7}, { "MinDay", 1} },
                        }
                    },
                    {
                        "aggregate(hour(StartDate) with max as MaxHour, hour(StartDate) with min as MinHour)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxHour", 5 }, { "MinHour", 1} },
                        }
                    },
                    {
                        "aggregate(minute(StartDate) with max as MaxMinute, minute(StartDate) with min as MinMinute)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxMinute", 6}, { "MinMinute", 2} },
                        }
                    },
                    {
                        "aggregate(second(StartDate) with max as MaxSecond, second(StartDate) with min as MinSecond)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxSecond", 7}, { "MinSecond", 3} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(concat(Address/City,Address/State) with max as MaxCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattleWA"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", null}, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity, Address/City with min as MinCity))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "MinCity", "redmond"}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "MinCity", "hobart" }, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(Address/City with max as MaxCity, CustomerId mul CustomerId with sum as CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxCity", "seattle"}, { "CustomerId", 30}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "MaxCity", "hobart"}, { "CustomerId", 25 }, { "Address/State", null} },
                        }
                    },
                    {
                        "groupby((Address/State), aggregate(CustomerId mul CustomerId with sum as CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 30}, { "Address/State", "WA"} },
                            new Dictionary<string, object> { { "CustomerId", 25}, { "Address/State", null} },
                        }
                    },
                    {
                        "aggregate(CustomerId mul CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 55} }
                        }
                    },
                    {
                        // Note SharePrice and CustomerId have different type
                        "aggregate(SharePrice mul CustomerId with sum as Result)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Result", 65.0M} }
                        }
                    },
                    {
                        "groupby((Website))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Website", null} },
                        }
                    },
                    {
                        "aggregate(IntProp with max as MaxIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MaxIntProp", 2} }
                        }
                    },
                    {
                        "aggregate(IntProp with min as MinIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "MinIntProp", 1} }
                        }
                    },
                    {
                        "aggregate(IntProp with countdistinct as DistinctIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "DistinctIntProp", 3L} }
                        }
                    },
                    {
                        "aggregate(IntProp with sum as TotalIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalIntProp", 3M} }
                        }
                    },
                    {
                        "aggregate(IntProp with average as TotalIntProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalIntProp", 1.5M} }
                        }
                    },
                    {
                        "aggregate(MixedProp with sum as TotalMixedProp)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "TotalMixedProp", 1M} }
                        }
                    },
                    {
                        "groupby((StringProp), aggregate(IntProp with min as MinIntProp))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "StringProp", "Test1" }, { "MinIntProp", 1} },
                            new Dictionary<string, object> { { "StringProp", "Test2" }, { "MinIntProp", 2} },
                            new Dictionary<string, object> { { "StringProp", "Test3" }, { "MinIntProp", null} },
                            new Dictionary<string, object> { { "StringProp", null }, { "MinIntProp", null} },
                        }
                    },
                    {
                        "groupby((StringProp), aggregate(IntProp with min as MinIntProp))/groupby((StringProp))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "StringProp", "Test1" } },
                            new Dictionary<string, object> { { "StringProp", "Test2" } },
                            new Dictionary<string, object> { { "StringProp", "Test3" } },
                            new Dictionary<string, object> { { "StringProp", null } },
                        }
                    },
                };
            }
        }

        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesMixedWithOthers
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$filter=Total eq 3",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}}
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total, Total2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$orderby=Name, Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$orderby=Total, Total2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$orderby=Name, Total",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$filter=Address/State eq 'WA'&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$orderby=Address/State desc, Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$filter=Address/City eq 'redmond'",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                        }
                    },
                    //{
                    //    "$apply=groupby((Name))&$top=1",
                    //    new List<Dictionary<string, object>>
                    //    {
                    //        new Dictionary<string, object> {{"Name", "Highest"}},
                    //    }
                    //},
                    //{
                    //    "$apply=groupby((Name))&$skip=1",
                    //    new List<Dictionary<string, object>>
                    //    {
                    //        new Dictionary<string, object> {{"Name", "Lowest"}},
                    //        new Dictionary<string, object> {{"Name", "Middle"}},
                    //    }
                    //},
                };
            }
        }

        public static TheoryDataSet<string, List<Dictionary<string, object>>> CustomerTestAppliesForPaging
        {
            get
            {
                return new TheoryDataSet<string, List<Dictionary<string, object>>>
                {
                    {
                        "$apply=aggregate(CustomerId with sum as CustomerId)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "CustomerId", 15} }
                        }
                    },
                    {
                        "$apply=aggregate(CustomerId with sum as Total)",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> { { "Total", 15} }
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total, CustomerId with sum as Total2))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                        }
                    },
                    {
                        "$apply=groupby((Name, CustomerId))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest" }, { "CustomerId", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 1}},
                        }
                    },
                    {
                        "$apply=groupby((Name, CustomerId))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 4}},
                            new Dictionary<string, object> {{"Name", "Lowest" }, { "CustomerId", 5}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, {"Total", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, {"Total", 10}},
                        }
                    },
                    {
                        "$apply=groupby((Name), aggregate(CustomerId with sum as Total))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}, {"Total", 3}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", "redmond"}, {"Address/State", "WA"}},
                            new Dictionary<string, object> {{"Address/City", "seattle"}, {"Address/State", "WA"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$orderby=Name desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Middle"}},
                            new Dictionary<string, object> {{"Name", "Lowest"}},
                        }
                    },
                    {
                        "$apply=groupby((Name))&$skip=2&$orderby=Name desc",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=CustomerId",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=Name",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Highest"}, { "CustomerId", 2}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 1}},
                        }
                    },
                    {
                        "$apply=groupby((CustomerId, Name))&$orderby=Name&$skip=2",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 4}},
                            new Dictionary<string, object> {{"Name", "Lowest"}, { "CustomerId", 5}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City))&$orderby=Address/City",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}},
                        }
                    },
                    {
                        "$apply=groupby((Address/City, Address/State))&$orderby=Address/State",
                        new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object> {{"Address/City", null}, {"Address/State", null}},
                            new Dictionary<string, object> {{"Address/City", "hobart"}, {"Address/State", null}},
                        }
                    },
                };
            }
        }


        // Legal filter queries usable against CustomerFilterTestData.
        // Tuple is: filter, expected list of customer ID's
        public static TheoryDataSet<string, int[]> CustomerTestFilters
        {
            get
            {
                return new TheoryDataSet<string, int[]>
                {
                    // Primitive properties
                    { "Name eq 'Highest'", new int[] { 2 } },
                    { "endswith(Name, 'est')", new int[] { 1, 2, 4, 5 } },

                    // Complex properties
                    { "Address/City eq 'redmond'", new int[] { 1, 5 } },
                    { "contains(Address/City, 'e')", new int[] { 1, 2, 5 } },

                    // Primitive property collections
                    { "Aliases/any(alias: alias eq 'alias34')", new int[] { 3, 4 } },
                    { "Aliases/any(alias: alias eq 'alias4')", new int[] { 4 } },
                    { "Aliases/all(alias: alias eq 'alias2')", new int[] { 2 } },

                    // Navigational properties
                    { "Orders/any(order: order/OrderId eq 12)", new int[] { 1 } },
                };
            }
        }

        // Test data used by CustomerTestApplies TheoryDataSet
        public static List<Customer> CustomerApplyTestData
        {
            get
            {
                List<Customer> customerList = new List<Customer>();

                Customer c = new Customer
                {
                    CustomerId = 1,
                    Name = "Lowest",
                    SharePrice = 10,
                    Address = new Address { City = "redmond", State = "WA" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test1" }, { "IntProp", 1 }, { "MixedProp", 1 } },
                    StartDate = new DateTimeOffset(new DateTime(2018,02,07, 1, 2, 3))
                };
                c.Orders = new List<Order>
                {
                    new Order { OrderId = 11, Customer = c },
                    new Order { OrderId = 12, Customer = c },
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 2,
                    Name = "Highest",
                    SharePrice = 2.5M,
                    Address = new Address { City = "seattle", State = "WA" },
                    Aliases = new List<string> { "alias2", "alias2" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test2" }, { "IntProp", 2 }, { "MixedProp", "String" } },
                    StartDate = new DateTimeOffset(new DateTime(2017, 03, 07, 5,6,7))
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 3,
                    Name = "Middle",
                    Address = new Address { City = "hobart" },
                    Aliases = new List<string> { "alias2", "alias34", "alias31" },
                    DynamicProperties = new Dictionary<string, object> { { "StringProp", "Test3" } },
                    StartDate = new DateTimeOffset(new DateTime(2018, 01, 01, 2, 3, 4))
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 4,
                    Name = "Lowest",
                    Aliases = new List<string> { "alias34", "alias4" },
                    StartDate = new DateTimeOffset(new DateTime(2016, 05, 07, 2, 3, 4))
                };
                customerList.Add(c);

                c = new Customer
                {
                    CustomerId = 5,
                    Name = "Lowest",
                    SharePrice = 10,
                    Address = new Address { City = "redmond", State = "WA" },
                };
                customerList.Add(c);

                return customerList;
            }
        }

        public static TheoryDataSet<string> AppliesWithReferencesOnGroupedOut
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "$apply=groupby((Name))&$filter=CustomerId eq 1",
                    "$apply=groupby((Name))/filter(CustomerId eq 1)",
                    "$apply=groupby((Name))/filter(Address/City eq 1)",
                    "$apply=groupby((Name))/groupby((CustomerId))",
                };
            }
        }

        [Theory]
        [PropertyData("CustomerTestApplies")]
        public void ApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockContainer() };
            var queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$apply", filter } });
            var applyOption = new ApplyQueryOption(filter, context, queryOptionParser);
            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = applyOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [PropertyData("CustomerTestAppliesMixedWithOthers")]
        public void ClausesAfterApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + filter);
            request.EnableHttpDependencyInjectionSupport();

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;
            // Act
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });


            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [PropertyData("CustomerTestAppliesForPaging")]
        public void StableSortingAndPagingApplyTo_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + filter);
            request.EnableHttpDependencyInjectionSupport();

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;
            // Act
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True, PageSize = 2 });
            
            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [PropertyData("CustomerTestAppliesForPaging")]
        public void StableSortingAndPagingApplyToWithAutoSelect_Returns_Correct_Queryable(string filter, List<Dictionary<string, object>> aggregation)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var customerEdmType = (EdmEntityType)(((IEdmCollectionType)model.EntityContainer.FindEntitySet("Customers").Type).ElementType.Definition);
            model.SetAnnotationValue(customerEdmType, new ModelBoundQuerySettings() { DefaultSelectType = SelectExpandType.Automatic });

            var context = new ODataQueryContext(model, typeof(Customer));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + filter);
            request.EnableHttpDependencyInjectionSupport();

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;
            // Act
            IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True, PageSize = 2 });

            // Assert
            Assert.NotNull(queryable);
            var actualCustomers = Assert.IsAssignableFrom<IEnumerable<DynamicTypeWrapper>>(queryable).ToList();

            Assert.Equal(aggregation.Count(), actualCustomers.Count());

            var aggEnum = actualCustomers.GetEnumerator();

            foreach (var expected in aggregation)
            {
                aggEnum.MoveNext();
                var agg = aggEnum.Current;
                foreach (var key in expected.Keys)
                {
                    object value = GetValue(agg, key);
                    Assert.Equal(expected[key], value);
                }
            }
        }

        [Theory]
        [PropertyData("AppliesWithReferencesOnGroupedOut")]
        public void ClausesWithGroupedOutReferences_Throw_ODataException(string clause)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + clause);
            request.EnableHttpDependencyInjectionSupport();

            var options = new ODataQueryOptions(context, request);

            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act & Assert
            Assert.Throws<ODataException>(() =>
            {
                IQueryable queryable = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });
            });
        }

        [Theory]
        [PropertyData("CustomerTestFilters")]
        public void ApplyTo_Returns_Correct_Queryable_ForFilter(string filter, int[] customerIds)
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            var context = new ODataQueryContext(model, typeof(Customer)) { RequestContainer = new MockContainer() };
            var queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$apply", string.Format("filter({0})", filter) } });
            var filterOption = new ApplyQueryOption(string.Format("filter({0})", filter), context, queryOptionParser);
            IEnumerable<Customer> customers = CustomerApplyTestData;

            // Act
            IQueryable queryable = filterOption.ApplyTo(customers.AsQueryable(), new ODataQuerySettings { HandleNullPropagation = HandleNullPropagationOption.True });

            // Assert
            Assert.NotNull(queryable);
            IEnumerable<Customer> actualCustomers = Assert.IsAssignableFrom<IEnumerable<Customer>>(queryable);
            Assert.Equal(
                customerIds,
                actualCustomers.Select(customer => customer.CustomerId));
        }

        [Fact]
        public void ApplyToSerializationWorks()
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            HttpConfiguration config =
                new[] { typeof(MetadataController), typeof(CustomersController) }.GetHttpConfiguration();

            config.MapODataServiceRoute("odata", "odata", model);
            var client = new HttpClient(new HttpServer(config));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/Customers?$apply=groupby((Name), aggregate(CustomerId with sum as TotalId))");

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response);
            var result = response.Content.ReadAsAsync<JObject>().Result;
            var results = result["value"] as JArray;
            Assert.Equal(3, results.Count);
            Assert.Equal("10", results[0]["TotalId"].ToString());
            Assert.Equal("Lowest", results[0]["Name"].ToString());
            Assert.Equal("2", results[1]["TotalId"].ToString());
            Assert.Equal("Highest", results[1]["Name"].ToString());
            Assert.Equal("3", results[2]["TotalId"].ToString());
            Assert.Equal("Middle", results[2]["Name"].ToString());
        }

        [Fact]
        public void ApplyToSerializationWorksForCompelxTypes()
        {
            // Arrange
            var model = new ODataModelBuilder()
                            .Add_Order_EntityType()
                            .Add_Customer_EntityType_With_Address()
                            .Add_CustomerOrders_Relationship()
                            .Add_Customer_EntityType_With_CollectionProperties()
                            .Add_Customers_EntitySet()
                            .GetEdmModel();
            HttpConfiguration config =
                new[] { typeof(MetadataController), typeof(CustomersController) }.GetHttpConfiguration();

            config.MapODataServiceRoute("odata", "odata", model);
            var client = new HttpClient(new HttpServer(config));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                "http://localhost/odata/Customers?$apply=groupby((Address/City), aggregate(CustomerId with sum as TotalId))");

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response);
            var result = response.Content.ReadAsAsync<JObject>().Result;
            var results = result["value"] as JArray;
            Assert.Equal(4, results.Count);
            Assert.Equal("6", results[0]["TotalId"].ToString());
            var address0 = results[0]["Address"] as JObject;
            Assert.Equal("redmond", address0["City"].ToString());
        }

        private object GetValue(DynamicTypeWrapper wrapper, string path)
        {
            var parts = path.Split('/');
            foreach (var part in parts)
            {
                object value;
                wrapper.TryGetPropertyValue(part, out value);
                wrapper = value as DynamicTypeWrapper;
                if (wrapper == null)
                {
                    return value;
                }
            }

            Assert.False(true, "Property " + path + " not found");
            return null;
        }
    }

    public class CustomersController : ODataController
    {
        private List<Customer> _customers;

        public CustomersController()
        {
            _customers = ApplyQueryOptionTest.CustomerApplyTestData;
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_customers);
        }
    }
}
