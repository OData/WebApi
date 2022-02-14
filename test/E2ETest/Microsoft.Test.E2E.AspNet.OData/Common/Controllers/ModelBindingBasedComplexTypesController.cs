//-----------------------------------------------------------------------------
// <copyright file="ModelBindingBasedComplexTypesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary.ModelBindingBasedTypes;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class ModelBindingBasedComplexTypeController : ApiController
    {
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_17 EchoEnumType_17([FromBody] EnumType_17 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_35 EchoEnumType_35([FromBody] EnumType_35 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructInt16 EchoStructInt16(StructInt16 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructGuid EchoStructGuid(StructGuid input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_1 EchoDCType_1(DCType_1 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_3 EchoDCType_3(DCType_3 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_4 EchoSerType_4(SerType_4 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_5 EchoSerType_5(SerType_5 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_7 EchoDCType_7(DCType_7 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_9 EchoDCType_9(DCType_9 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_11 EchoSerType_11(SerType_11 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_15 EchoDCType_15(DCType_15 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_16 EchoDCType_16(DCType_16 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_18 EchoDCType_18(DCType_18 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_19 EchoDCType_19(DCType_19 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_20 EchoDCType_20(DCType_20 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_22 EchoSerType_22(SerType_22 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_25 EchoDCType_25(DCType_25 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_26 EchoSerType_26(SerType_26 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_31 EchoDCType_31(DCType_31 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_32 EchoDCType_32(DCType_32 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_33 EchoSerType_33(SerType_33 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_34 EchoDCType_34(DCType_34 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_36 EchoDCType_36(DCType_36 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_38 EchoDCType_38(DCType_38 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_40 EchoDCType_40(DCType_40 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_42 EchoDCType_42(DCType_42 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_65 EchoDCType_65(DCType_65 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DerivedType EchoDerivedType(DerivedType input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Address EchoAddress(Address input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TestRun EchoTestRun(TestRun input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Branch EchoBranch(Branch input)
        {
            return input;
        }
    }

    public class ModelBindingBasedComplexTypeCollectionsController : ApiController
    {
        #region Echo*Array(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_17[] EchoEnumType_17Array(EnumType_17[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_35[] EchoEnumType_35Array(EnumType_35[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructInt16[] EchoStructInt16Array(StructInt16[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructGuid[] EchoStructGuidArray(StructGuid[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_1[] EchoDCType_1Array(DCType_1[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_3[] EchoDCType_3Array(DCType_3[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_4[] EchoSerType_4Array(SerType_4[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_5[] EchoSerType_5Array(SerType_5[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_7[] EchoDCType_7Array(DCType_7[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_9[] EchoDCType_9Array(DCType_9[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_11[] EchoSerType_11Array(SerType_11[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_15[] EchoDCType_15Array(DCType_15[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_16[] EchoDCType_16Array(DCType_16[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_18[] EchoDCType_18Array(DCType_18[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_19[] EchoDCType_19Array(DCType_19[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_20[] EchoDCType_20Array(DCType_20[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_22[] EchoSerType_22Array(SerType_22[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_25[] EchoDCType_25Array(DCType_25[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_26[] EchoSerType_26Array(SerType_26[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_31[] EchoDCType_31Array(DCType_31[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_32[] EchoDCType_32Array(DCType_32[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_33[] EchoSerType_33Array(SerType_33[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_34[] EchoDCType_34Array(DCType_34[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_36[] EchoDCType_36Array(DCType_36[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_38[] EchoDCType_38Array(DCType_38[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_40[] EchoDCType_40Array(DCType_40[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_42[] EchoDCType_42Array(DCType_42[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_65[] EchoDCType_65Array(DCType_65[] input)
        {
            return input;
        }

        //[AcceptVerbs("PUT", "POST", "DELETE")]
        //public BaseType[] EchoBaseTypeArray(BaseType[] input)
        //{
        //    return input;
        //}

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DerivedType[] EchoDerivedTypeArray(DerivedType[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Address[] EchoAddressArray(Address[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TestRun[] EchoTestRunArray(TestRun[] input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Branch[] EchoBranchArray(Branch[] input)
        {
            return input;
        }
        #endregion

        #region EchoListOf*(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<EnumType_17> EchoListOfEnumType_17(List<EnumType_17> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<EnumType_35> EchoListOfEnumType_35(List<EnumType_35> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<StructInt16> EchoListOfStructInt16(List<StructInt16> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<StructGuid> EchoListOfStructGuid(List<StructGuid> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_1> EchoListOfDCType_1(List<DCType_1> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_3> EchoListOfDCType_3(List<DCType_3> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_7> EchoListOfDCType_7(List<DCType_7> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_9> EchoListOfDCType_9(List<DCType_9> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_15> EchoListOfDCType_15(List<DCType_15> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_16> EchoListOfDCType_16(List<DCType_16> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_18> EchoListOfDCType_18(List<DCType_18> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_19> EchoListOfDCType_19(List<DCType_19> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_20> EchoListOfDCType_20(List<DCType_20> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_25> EchoListOfDCType_25(List<DCType_25> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_31> EchoListOfDCType_31(List<DCType_31> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_32> EchoListOfDCType_32(List<DCType_32> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_34> EchoListOfDCType_34(List<DCType_34> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_36> EchoListOfDCType_36(List<DCType_36> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_38> EchoListOfDCType_38(List<DCType_38> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_40> EchoListOfDCType_40(List<DCType_40> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_42> EchoListOfDCType_42(List<DCType_42> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DCType_65> EchoListOfDCType_65(List<DCType_65> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_4> EchoListOfSerType_4(List<SerType_4> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_5> EchoListOfSerType_5(List<SerType_5> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_11> EchoListOfSerType_11(List<SerType_11> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_22> EchoListOfSerType_22(List<SerType_22> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_26> EchoListOfSerType_26(List<SerType_26> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<SerType_33> EchoListOfSerType_33(List<SerType_33> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Address> EchoListOfAddress(List<Address> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<TestRun> EchoListOfTestRun(List<TestRun> input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Branch> EchoListOfBranch(List<Branch> input)
        {
            return input;
        }
        #endregion
    }

    public class ModelBindingBasedFromUriComplexTypeController : ApiController
    {
        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public EnumType_17 EchoEnumType_17(EnumType_17 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public EnumType_35 EchoEnumType_35(EnumType_35 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public StructInt16 EchoStructInt16([FromUri] StructInt16 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public StructGuid EchoStructGuid([FromUri] StructGuid input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_1 EchoDCType_1([FromUri] DCType_1 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_3 EchoDCType_3([FromUri] DCType_3 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_4 EchoSerType_4([FromUri] SerType_4 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_5 EchoSerType_5([FromUri] SerType_5 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_7 EchoDCType_7([FromUri] DCType_7 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_9 EchoDCType_9([FromUri] DCType_9 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_11 EchoSerType_11([FromUri] SerType_11 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_15 EchoDCType_15([FromUri] DCType_15 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_16 EchoDCType_16([FromUri] DCType_16 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_18 EchoDCType_18([FromUri] DCType_18 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_19 EchoDCType_19([FromUri] DCType_19 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_20 EchoDCType_20([FromUri] DCType_20 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_22 EchoSerType_22([FromUri] SerType_22 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_25 EchoDCType_25([FromUri] DCType_25 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_26 EchoSerType_26([FromUri] SerType_26 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_31 EchoDCType_31([FromUri] DCType_31 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_32 EchoDCType_32([FromUri] DCType_32 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public SerType_33 EchoSerType_33([FromUri] SerType_33 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_34 EchoDCType_34([FromUri] DCType_34 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_36 EchoDCType_36([FromUri] DCType_36 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_38 EchoDCType_38([FromUri] DCType_38 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_40 EchoDCType_40([FromUri] DCType_40 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_42 EchoDCType_42([FromUri] DCType_42 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DCType_65 EchoDCType_65([FromUri] DCType_65 input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public DerivedType EchoDerivedType([FromUri] DerivedType input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Address EchoAddress([FromUri] Address input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public TestRun EchoTestRun([FromUri] TestRun input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Branch EchoBranch([FromUri] Branch input)
        {
            return input;
        }
    }
}
