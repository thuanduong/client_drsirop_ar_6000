using System;

namespace Core
{
    using Model;

    public class MasterData : ModelData
    {
        protected MasterData()
        {
        }

        private static MasterData _instance;
        public static MasterData Instance
        {
            get
            {
                return _instance ?? (_instance = new MasterData());
            }
        }
    }
}
