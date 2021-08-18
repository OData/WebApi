//-----------------------------------------------------------------------------
// <copyright file="ComplexTypesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class ComplexTypeController : ApiController
    {
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_17 EchoEnumType_17([FromBody] EnumType_17 param_EnumType_17)
        {
            return param_EnumType_17;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public EnumType_35 EchoEnumType_35([FromBody] EnumType_35 param_EnumType_35)
        {
            return param_EnumType_35;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructInt16 EchoStructInt16(StructInt16 input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public StructGuid EchoStructGuid(StructGuid param_StructGuid)
        {
            return param_StructGuid;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_1 EchoDCType_1(DCType_1 param_DCType_1)
        {
            return param_DCType_1;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_3 EchoDCType_3(DCType_3 param_DCType_3)
        {
            return param_DCType_3;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_4 EchoSerType_4(SerType_4 param_SerType_4)
        {
            return param_SerType_4;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_5 EchoSerType_5(SerType_5 param_SerType_5)
        {
            return param_SerType_5;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_7 EchoDCType_7(DCType_7 param_DCType_7)
        {
            return param_DCType_7;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_9 EchoDCType_9(DCType_9 param_DCType_9)
        {
            return param_DCType_9;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_11 EchoSerType_11(SerType_11 param_SerType_11)
        {
            return param_SerType_11;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_15 EchoDCType_15(DCType_15 param_DCType_15)
        {
            return param_DCType_15;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_16 EchoDCType_16(DCType_16 param_DCType_16)
        {
            return param_DCType_16;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_18 EchoDCType_18(DCType_18 param_DCType_18)
        {
            return param_DCType_18;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_19 EchoDCType_19(DCType_19 param_DCType_19)
        {
            return param_DCType_19;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_20 EchoDCType_20(DCType_20 param_DCType_20)
        {
            return param_DCType_20;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_22 EchoSerType_22(SerType_22 param_SerType_22)
        {
            return param_SerType_22;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_25 EchoDCType_25(DCType_25 param_DCType_25)
        {
            return param_DCType_25;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_26 EchoSerType_26(SerType_26 param_SerType_26)
        {
            return param_SerType_26;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_31 EchoDCType_31(DCType_31 param_DCType_31)
        {
            return param_DCType_31;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_32 EchoDCType_32(DCType_32 param_DCType_32)
        {
            return param_DCType_32;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public SerType_33 EchoSerType_33(SerType_33 param_SerType_33)
        {
            return param_SerType_33;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_34 EchoDCType_34(DCType_34 param_DCType_34)
        {
            return param_DCType_34;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_36 EchoDCType_36(DCType_36 param_DCType_36)
        {
            return param_DCType_36;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_38 EchoDCType_38(DCType_38 param_DCType_38)
        {
            return param_DCType_38;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_40 EchoDCType_40(DCType_40 param_DCType_40)
        {
            return param_DCType_40;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_42 EchoDCType_42(DCType_42 param_DCType_42)
        {
            return param_DCType_42;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DCType_65 EchoDCType_65(DCType_65 param_DCType_65)
        {
            return param_DCType_65;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ListType_1 EchoListType_1(ListType_1 param_ListType_1)
        {
            return param_ListType_1;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ListType_2 EchoListType_2(ListType_2 param_ListType_2)
        {
            return param_ListType_2;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public BaseType EchoBaseType(BaseType param_BaseType)
        {
            return param_BaseType;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DerivedType EchoDerivedType(DerivedType param_DerivedType)
        {
            return param_DerivedType;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public PolymorphicMember EchoPolymorphicMember(PolymorphicMember param_PolymorphicMember)
        {
            return param_PolymorphicMember;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public PolymorphicAsInterfaceMember EchoPolymorphicAsInterfaceMember(PolymorphicAsInterfaceMember param_PolymorphicAsInterfaceMember)
        {
            return param_PolymorphicAsInterfaceMember;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public CollectionsWithPolymorphicMember EchoCollectionsWithPolymorphicMember(CollectionsWithPolymorphicMember param_CollectionsWithPolymorphicMember)
        {
            return param_CollectionsWithPolymorphicMember;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Person EchoPerson(Person param_Person)
        {
            return param_Person;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Address EchoAddress(Address param_Address)
        {
            return param_Address;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TestRun EchoTestRun(TestRun testRun)
        {
            return testRun;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Branch EchoBranch(Branch branch)
        {
            return branch;
        }
    }
}
