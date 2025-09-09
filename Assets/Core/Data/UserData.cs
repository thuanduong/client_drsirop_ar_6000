using System;

namespace Core
{
    using Model;
    public class UserData : ModelData
    {
        protected UserData()
        {
        }

        private static UserData _instance;
        public static UserData Instance
        {
            get
            {
                return _instance ?? (_instance = new UserData());
            }
        }
    }
}