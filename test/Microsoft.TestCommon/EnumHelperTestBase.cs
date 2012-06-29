// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class EnumHelperTestBase<TEnum> where TEnum : IComparable, IFormattable, IConvertible
    {
        protected void Check_IsDefined_ReturnsTrueForDefinedValues(Func<TEnum, bool> isDefined)
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (object value in values)
            {
                Assert.True(isDefined((TEnum)value));
            }
        }

        protected void Check_IsDefined_ReturnsFalseForUndefinedValues(Func<TEnum, bool> isDefined, TEnum undefined)
        {
            Assert.False(isDefined(undefined));
        }

        protected void Check_Validate_DoesNotThrowForDefinedValues(Action<TEnum, string> validate)
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (object value in values)
            {
                validate((TEnum)value, "parameter");
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
