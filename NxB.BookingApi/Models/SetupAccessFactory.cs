using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement SetupAccessFactory - placeholder to fix compilation errors
    // This factory should create SetupAccess domain objects from TConTBDSetupAccess entities
    public class SetupAccessFactory
    {
        // TODO: Implement factory methods
        public SetupAccess Create(TConTBDSetupAccess tconEntity)
        {
            return new SetupAccess(tconEntity);
        }

        public SetupAccess CreateDefault(int no)
        {
            // TODO: Implement default setup access creation logic
            throw new NotImplementedException("SetupAccessFactory.CreateDefault needs implementation");
        }
    }
}