using System;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Exception to be thrown by EditorHttpCaller when the API returns
    /// an error
    /// </summary>
    public class EditorHttpCallerException : Exception
    {
        public string Status;

        public EditorHttpCallerException(string status, string message) : base(message)
        {
            this.Status = status;
        }
    }
}