namespace ParseData
{
    [System.Serializable]
    public class AuthenticateContext
    {
        public string user_id;
        public string session_token;
        public string game_id;
    }

    public class AuthenticateWrapper
    {
        public AuthenticateWrapperBody body;
    }

    public class AuthenticateWrapperBody
    {
        public string access_token;
        public string refresh_token;
        public int statusCode;
    }
}
