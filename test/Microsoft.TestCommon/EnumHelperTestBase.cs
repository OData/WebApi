// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.TestCommon
{
    public abstract class EnumHelperTestBase<TEnum> where TEnum : IComparable, IFormattable, IConvertible
    {
        private Func<TEnum, bool> _isDefined;
        private Action<TEnum, string> _validate;
        private TEnum _undefined;

        /// <summary>
        /// Helper to verify that we validate enums correctly when passed as arguments etc.
        /// </summary>
        /// <param name="isDefined">A Func used to validate that a value is defined.</param>
        /// <param name="validate">A Func used to validate that a value is definded of throw an exception.</param>
        /// <param name="undefined">An undefined value.</param>
        protected EnumHelperTestBase(Func<TEnum, bool> isDefined, Action<TEnum, string> validate, TEnum undefined)
        {
            _isDefined = isDefined;
            _validate = validate;
            _undefined = undefined;
        }

        [Fact]
        public void IsDefined_ReturnsTrueForDefinedValues()
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (object value in values)
            {
                Assert.True(_isDefined((TEnum)value));
            }
        }

        [Fact]
        public void IsDefined_ReturnsFalseForUndefinedValues()
        {
            Assert.False(_isDefined(_undefined));
        }

        [Fact]
        public void Validate_DoesNotThrowForDefinedValues()
        {
            Array values = Enum.GetValues(typeof(TEnum));
            foreach (object value in values)
            {
                _validate((TEnum)value, "parameter");
            }
        }

        [Fact]
        public void Validate_ThrowsForUndefinedValues()
        {
            Assert.ThrowsInvalidEnumArgument(
                () => _validate(_undefined, "parameter"),
                "parameter",
                (int)Convert.ChangeType(_undefined, typeof(int)),
                typeof(TEnum),
                allowDerivedExceptions: false);
        }
    }
}
