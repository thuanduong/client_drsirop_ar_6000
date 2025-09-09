using System;

namespace Core.Model
{
    public class UserDataLocal : ModelData
    {
        private static UserDataLocal _instance;
        public static UserDataLocal Instance
        {
            get
            {
                return _instance ?? (_instance = new UserDataLocal());
            }
        }
    }
}