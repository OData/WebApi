// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    // TODO: Bug 467610: This class is duplicated from Microsoft.TestCommon as the current build is dependant on RTM branch and this
    // class exists only in the VNext branch. Remove once that changes.
    public class EnumHelperTestBase<TEnum> where TEnum : IComparable, IFormattable, IConvertible
    {
        private static TEnum[] _emptyEnumArray = new TEnum[0];

        protected void Check_IsDefined_ReturnsTrueForDefinedValues(Func<TEnum, bool> isDefined)
        {
            Check_IsDefined_ReturnsTrueForDefinedValues(isDefined, _emptyEnumArray);
        }

        protected void Check_IsDefined_ReturnsTrueForDefinedValues(Func<TEnum, bool> isDefined, params TEnum[] ignoreValues)
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (TEnum value in values)
            {
                if (!ignoreValues.Contains(value))
                {
                    Assert.True(isDefined(value));
                }
            }
        }

        protected void Check_IsDefined_ReturnsFalseForUndefinedValues(Func<TEnum, bool> isDefined, TEnum undefined)
        {
            Assert.False(isDefined(undefined));
        }

        protected void Check_Validate_DoesNotThrowForDefinedValues(Action<TEnum, string> validate)
        {
            Check_Validate_DoesNotThrowForDefinedValues(validate, _emptyEnumArray);
        }

        protected void Check_Validate_DoesNotThrowForDefinedValues(Action<TEnum, string> validate, params TEnum[] ignoreValues)
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (TEnum value in values)
            {
                if (!ignoreValues.Contains(value))
                {
                    validate((TEnum)value, "parameter");
                }
            }
        }

        protected void Check_Validate_ThrowsForUndefinedValues(Action<TEnum, string> validate, TEnum undefined)
        {
            Assert.ThrowsInvalidEnumArgument(
                () => validate(undefined, "parameter"),
                "parameter",
                (int)Convert.ChangeType(undefined, typeof(int)),
                typeof(TEnum),
                allowDerivedExceptions: false);
        }
    }
}
