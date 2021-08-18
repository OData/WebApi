//-----------------------------------------------------------------------------
// <copyright file="ComplexTypesCollectionsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class ComplexTypeCollectionsController : ApiController
    {
        #region Reverse*Array(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_17[] ReverseEnumType_17Array(EnumType_17[] param_EnumType_17)
        {
            if (param_EnumType_17 == null)
            {
                return null;
            }
            return param_EnumType_17.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_35[] ReverseEnumType_35Array(EnumType_35[] param_EnumType_35)
        {
            if (param_EnumType_35 == null)
            {
                return null;
            }
            return param_EnumType_35.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructInt16[] ReverseStructInt16Array(StructInt16[] param)
        {
            if (param == null)
            {
                return null;
            }
            return param.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructGuid[] ReverseStructGuidArray(StructGuid[] param_StructGuid)
        {
            if (param_StructGuid == null)
            {
                return null;
            }
            return param_StructGuid.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_1[] ReverseDCType_1Array(DCType_1[] param_DCType_1)
        {
            if (param_DCType_1 == null)
            {
                return null;
            }
            return param_DCType_1.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_3[] ReverseDCType_3Array(DCType_3[] param_DCType_3)
        {
            if (param_DCType_3 == null)
            {
                return null;
            }
            return param_DCType_3.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_4[] ReverseSerType_4Array(SerType_4[] param_SerType_4)
        {
            if (param_SerType_4 == null)
            {
                return null;
            }
            return param_SerType_4.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_5[] ReverseSerType_5Array(SerType_5[] param_SerType_5)
        {
            if (param_SerType_5 == null)
            {
                return null;
            }
            return param_SerType_5.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_7[] ReverseDCType_7Array(DCType_7[] param_DCType_7)
        {
            if (param_DCType_7 == null)
            {
                return null;
            }
            return param_DCType_7.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_9[] ReverseDCType_9Array(DCType_9[] param_DCType_9)
        {
            if (param_DCType_9 == null)
            {
                return null;
            }
            return param_DCType_9.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_11[] ReverseSerType_11Array(SerType_11[] param_SerType_11)
        {
            if (param_SerType_11 == null)
            {
                return null;
            }
            return param_SerType_11.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_15[] ReverseDCType_15Array(DCType_15[] param_DCType_15)
        {
            if (param_DCType_15 == null)
            {
                return null;
            }
            return param_DCType_15.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_16[] ReverseDCType_16Array(DCType_16[] param_DCType_16)
        {
            if (param_DCType_16 == null)
            {
                return null;
            }
            return param_DCType_16.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_18[] ReverseDCType_18Array(DCType_18[] param_DCType_18)
        {
            if (param_DCType_18 == null)
            {
                return null;
            }
            return param_DCType_18.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_19[] ReverseDCType_19Array(DCType_19[] param_DCType_19)
        {
            if (param_DCType_19 == null)
            {
                return null;
            }
            return param_DCType_19.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_20[] ReverseDCType_20Array(DCType_20[] param_DCType_20)
        {
            if (param_DCType_20 == null)
            {
                return null;
            }
            return param_DCType_20.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_22[] ReverseSerType_22Array(SerType_22[] param_SerType_22)
        {
            if (param_SerType_22 == null)
            {
                return null;
            }
            return param_SerType_22.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_25[] ReverseDCType_25Array(DCType_25[] param_DCType_25)
        {
            if (param_DCType_25 == null)
            {
                return null;
            }
            return param_DCType_25.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_26[] ReverseSerType_26Array(SerType_26[] param_SerType_26)
        {
            if (param_SerType_26 == null)
            {
                return null;
            }
            return param_SerType_26.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_31[] ReverseDCType_31Array(DCType_31[] param_DCType_31)
        {
            if (param_DCType_31 == null)
            {
                return null;
            }
            return param_DCType_31.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_32[] ReverseDCType_32Array(DCType_32[] param_DCType_32)
        {
            if (param_DCType_32 == null)
            {
                return null;
            }
            return param_DCType_32.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_33[] ReverseSerType_33Array(SerType_33[] param_SerType_33)
        {
            if (param_SerType_33 == null)
            {
                return null;
            }
            return param_SerType_33.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_34[] ReverseDCType_34Array(DCType_34[] param_DCType_34)
        {
            if (param_DCType_34 == null)
            {
                return null;
            }
            return param_DCType_34.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_36[] ReverseDCType_36Array(DCType_36[] param_DCType_36)
        {
            if (param_DCType_36 == null)
            {
                return null;
            }
            return param_DCType_36.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_38[] ReverseDCType_38Array(DCType_38[] param_DCType_38)
        {
            if (param_DCType_38 == null)
            {
                return null;
            }
            return param_DCType_38.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_40[] ReverseDCType_40Array(DCType_40[] param_DCType_40)
        {
            if (param_DCType_40 == null)
            {
                return null;
            }
            return param_DCType_40.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_42[] ReverseDCType_42Array(DCType_42[] param_DCType_42)
        {
            if (param_DCType_42 == null)
            {
                return null;
            }
            return param_DCType_42.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_65[] ReverseDCType_65Array(DCType_65[] param_DCType_65)
        {
            if (param_DCType_65 == null)
            {
                return null;
            }
            return param_DCType_65.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ListType_1[] ReverseListType_1Array(ListType_1[] param_ListType_1)
        {
            if (param_ListType_1 == null)
            {
                return null;
            }
            return param_ListType_1.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ListType_2[] ReverseListType_2Array(ListType_2[] param_ListType_2)
        {
            if (param_ListType_2 == null)
            {
                return null;
            }
            return param_ListType_2.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public BaseType[] ReverseBaseTypeArray(BaseType[] param_BaseType)
        {
            if (param_BaseType == null)
            {
                return null;
            }
            return param_BaseType.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DerivedType[] ReverseDerivedTypeArray(DerivedType[] param_DerivedType)
        {
            if (param_DerivedType == null)
            {
                return null;
            }
            return param_DerivedType.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public PolymorphicMember[] ReversePolymorphicMemberArray(PolymorphicMember[] param_PolymorphicMember)
        {
            if (param_PolymorphicMember == null)
            {
                return null;
            }
            return param_PolymorphicMember.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public PolymorphicAsInterfaceMember[] ReversePolymorphicAsInterfaceMemberArray(PolymorphicAsInterfaceMember[] param_PolymorphicAsInterfaceMember)
        {
            if (param_PolymorphicAsInterfaceMember == null)
            {
                return null;
            }
            return param_PolymorphicAsInterfaceMember.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public CollectionsWithPolymorphicMember[] ReverseCollectionsWithPolymorphicMemberArray(CollectionsWithPolymorphicMember[] param_CollectionsWithPolymorphicMember)
        {
            if (param_CollectionsWithPolymorphicMember == null)
            {
                return null;
            }
            return param_CollectionsWithPolymorphicMember.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Person[] ReversePersonArray(Person[] param_Person)
        {
            if (param_Person == null)
            {
                return null;
            }
            return param_Person.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Address[] ReverseAddressArray(Address[] param_Address)
        {
            if (param_Address == null)
            {
                return null;
            }
            return param_Address.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TestRun[] ReverseTestRunArray(TestRun[] testRun)
        {
            if (testRun == null)
            {
                return null;
            }
            return testRun.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Branch[] ReverseBranchArray(Branch[] branch)
        {
            if (branch == null)
            {
                return null;
            }
            return branch.Reverse().ToArray();
        }
        #endregion

        #region ReverseListOf*(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<EnumType_17> ReverseListOfEnumType_17(List<EnumType_17> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<EnumType_35> ReverseListOfEnumType_35(List<EnumType_35> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<StructInt16> ReverseListOfStructInt16(List<StructInt16> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<StructGuid> ReverseListOfStructGuid(List<StructGuid> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_1> ReverseListOfDCType_1(List<DCType_1> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_3> ReverseListOfDCType_3(List<DCType_3> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_7> ReverseListOfDCType_7(List<DCType_7> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_9> ReverseListOfDCType_9(List<DCType_9> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_15> ReverseListOfDCType_15(List<DCType_15> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_16> ReverseListOfDCType_16(List<DCType_16> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_18> ReverseListOfDCType_18(List<DCType_18> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_19> ReverseListOfDCType_19(List<DCType_19> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_20> ReverseListOfDCType_20(List<DCType_20> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_25> ReverseListOfDCType_25(List<DCType_25> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_31> ReverseListOfDCType_31(List<DCType_31> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_32> ReverseListOfDCType_32(List<DCType_32> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_34> ReverseListOfDCType_34(List<DCType_34> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_36> ReverseListOfDCType_36(List<DCType_36> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_38> ReverseListOfDCType_38(List<DCType_38> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_40> ReverseListOfDCType_40(List<DCType_40> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_42> ReverseListOfDCType_42(List<DCType_42> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_65> ReverseListOfDCType_65(List<DCType_65> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_4> ReverseListOfSerType_4(List<SerType_4> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_5> ReverseListOfSerType_5(List<SerType_5> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_11> ReverseListOfSerType_11(List<SerType_11> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_22> ReverseListOfSerType_22(List<SerType_22> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_26> ReverseListOfSerType_26(List<SerType_26> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_33> ReverseListOfSerType_33(List<SerType_33> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Person> ReverseListOfPerson(List<Person> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Address> ReverseListOfAddress(List<Address> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<TestRun> ReverseListOfTestRun(List<TestRun> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Branch> ReverseListOfBranch(List<Branch> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }
        #endregion

        #region EchoDictionaryOf*And*(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, int> EchoDictionaryOfInt32AndInt32(Dictionary<int, int> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, string> EchoDictionaryOfStringAndString(Dictionary<string, string> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, int> EchoDictionaryOfStringAndInt32(Dictionary<string, int> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, string> EchoDictionaryOfInt32AndString(Dictionary<int, string> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, double> EchoDictionaryOfGuidAndDouble(Dictionary<Guid, double> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, DateTime> EchoDictionaryOfstringAndDateTime(Dictionary<string, DateTime> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, TimeSpan> EchoDictionaryOfintAndTimeSpan(Dictionary<int, TimeSpan> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, EnumType_17> EchoDictionaryOfGuidAndEnumType_17(Dictionary<Guid, EnumType_17> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, EnumType_35> EchoDictionaryOfstringAndEnumType_35(Dictionary<string, EnumType_35> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, StructInt16> EchoDictionaryOfintAndStructInt16(Dictionary<int, StructInt16> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, StructGuid> EchoDictionaryOfGuidAndStructGuid(Dictionary<Guid, StructGuid> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, DCType_1> EchoDictionaryOfstringAndDCType_1(Dictionary<string, DCType_1> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, DCType_3> EchoDictionaryOfintAndDCType_3(Dictionary<int, DCType_3> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, DCType_7> EchoDictionaryOfGuidAndDCType_7(Dictionary<Guid, DCType_7> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, DCType_9> EchoDictionaryOfstringAndDCType_9(Dictionary<string, DCType_9> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, SerType_4> EchoDictionaryOfintAndSerType_4(Dictionary<int, SerType_4> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, SerType_5> EchoDictionaryOfGuidAndSerType_5(Dictionary<Guid, SerType_5> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, Person> EchoDictionaryOfstringAndPerson(Dictionary<string, Person> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<int, Address> EchoDictionaryOfintAndAddress(Dictionary<int, Address> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<Guid, TestRun> EchoDictionaryOfGuidAndTestRun(Dictionary<Guid, TestRun> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Dictionary<string, Branch> EchoDictionaryOfstringAndBranch(Dictionary<string, Branch> input)
        {
            return input;
        }
        #endregion
    }
}
