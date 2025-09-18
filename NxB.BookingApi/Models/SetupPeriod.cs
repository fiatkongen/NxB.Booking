using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement SetupPeriod domain model - placeholder to fix compilation errors
    // This class should wrap TConTBDSetupPeriod and provide domain logic
    public class SetupPeriod
    {
        private TConTBDSetupPeriod _tconEntity;

        public SetupPeriod(TConTBDSetupPeriod tconEntity)
        {
            _tconEntity = tconEntity;
        }

        public int No => _tconEntity._No;
        public int MasterAddr => _tconEntity._MasterAddr;
        public byte StartH => _tconEntity._StartH;
        public byte StartM => _tconEntity._StartM;
        public byte EndH => _tconEntity._EndH;
        public byte EndM => _tconEntity._EndM;

        // TODO: Implement domain logic methods
        public TConTBDSetupPeriod GetTConEntity()
        {
            return _tconEntity;
        }
    }
}