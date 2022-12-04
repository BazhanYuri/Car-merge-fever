namespace HomaGames.HomaBelly.Installer.Utilities
{
    /// <summary>
    /// Result object returned from IHttpCaller
    /// </summary>
    /// <typeparam name="T">The result object type</typeparam>
    public class HttpCallerResult<T>
    {
        public Error ErrorResult;
        public T Result;

        public struct Error
        {
            public System.Net.HttpStatusCode Code;
            public string Message;
        }
    }
}
