using System;
using Core.Model;

[System.Serializable]
public class UserProfileModel : BaseModel
{
    public string UserName { get; set; }
    public string UserId { get; set; }
    public string AgentId { get; set; }
}
