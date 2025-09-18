using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement SetupAccess domain model - placeholder to fix compilation errors
    // This class should wrap TConTBDSetupAccess and provide domain logic
    public class SetupAccess
    {
        private TConTBDSetupAccess _tconEntity;

        public SetupAccess(TConTBDSetupAccess tconEntity)
        {
            _tconEntity = tconEntity;
        }

        public int No => _tconEntity._No;
        public int MasterAddr => _tconEntity._MasterAddr;

        // TODO: Implement domain logic methods
        public TConTBDSetupAccess GetTConEntity()
        {
            return _tconEntity;
        }
    }
}