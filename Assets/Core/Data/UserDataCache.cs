using System;

namespace Core.Model
{
    public class UserDataCache : ModelData
    {

        private static UserDataCache _instance;
        public static UserDataCache Instance
        {
            get
            {
                return _instance ?? (_instance = new UserDataCache());
            }
        }
    }
}