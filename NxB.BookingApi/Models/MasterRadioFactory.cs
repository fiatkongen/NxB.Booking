using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement MasterRadioFactory - placeholder to fix compilation errors
    // This factory should create MasterRadio domain objects from TConMasterRadio entities
    public class MasterRadioFactory
    {
        // TODO: Implement factory methods
        public MasterRadio Create(TConMasterRadio tconEntity)
        {
            return new MasterRadio(tconEntity);
        }

        public MasterRadio CreateDefault(int masterAddress, string name)
        {
            // TODO: Implement default master radio creation logic
            throw new NotImplementedException("MasterRadioFactory.CreateDefault needs implementation");
        }
    }
}