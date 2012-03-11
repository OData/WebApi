namespace Microsoft.TestCommon
{
    /// <summary>
    /// MSTest timeout constants for use with the <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.TimeoutAttribute"/>.
    /// </summary>
    public class TimeoutConstant
    {
        private const int seconds = 1000;

        /// <summary>
        /// The default timeout for test methods.
        /// </summary>
        public const int DefaultTimeout = 30 * seconds;

        /// <summary>
        /// An extendedn timeout for longer running test methods.
        /// </summary>
        public const int ExtendedTimeout = 240 * seconds;
    }
}
