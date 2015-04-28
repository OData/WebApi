namespace Nuwa.WebStack.Host
{
    public interface IPortArranger
    {
        string Reserve();
        void Return(string port);
    }
}
