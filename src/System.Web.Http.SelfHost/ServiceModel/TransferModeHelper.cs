using System.ServiceModel;

namespace System.Web.Http.SelfHost.ServiceModel
{
    internal static class TransferModeHelper
    {
        public static bool IsDefined(TransferMode transferMode)
        {
            return transferMode == TransferMode.Buffered ||
                   transferMode == TransferMode.Streamed ||
                   transferMode == TransferMode.StreamedRequest ||
                   transferMode == TransferMode.StreamedResponse;
        }

        public static bool IsRequestStreamed(TransferMode transferMode)
        {
            return transferMode == TransferMode.StreamedRequest || transferMode == TransferMode.Streamed;
        }

        public static bool IsResponseStreamed(TransferMode transferMode)
        {
            return transferMode == TransferMode.StreamedResponse || transferMode == TransferMode.Streamed;
        }

        public static void Validate(TransferMode value)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument("value", (int)value, typeof(TransferMode));
            }
        }
    }
}
