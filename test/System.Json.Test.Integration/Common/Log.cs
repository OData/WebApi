namespace System.Json
{
    internal static class Log
    {
        public static void Info(string text, params object[] args)
        {
            Console.WriteLine(text, args);
        }
    }
}
