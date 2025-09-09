using System;

namespace Data
{
    [Serializable]
    public class RetellConnectRequest
    {
        public string agent_id;
        public string user_id;
        public string llm_type;
    }

    [Serializable]
    public class RetellConnectResponse
    {
        public string call_type;
        public string call_id;
        public string agent_id;
        public string agent_name;
        public string url;
        public string access_token;
    }

    [Serializable]
    public class RetellErrorResponse
    {
        public string error;
        public string details;
    }

    [Serializable]
    public class RetellSendChatRequest
    {
        public string message;
    }

    [Serializable]
    public class RetellReceiveChatReponse
    {
        public string status;
        public string message;
    }

    [Serializable]
    public class RetellStartChatReponse
    {
        public string status;
        public string chat_id;
        public string initial_message;
    }

    [Serializable]
    public class RetellEndChatReponse
    {
        public string status;
        public string message;
    }

}