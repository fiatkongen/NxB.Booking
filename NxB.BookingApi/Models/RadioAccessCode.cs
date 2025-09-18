using System;

namespace NxB.BookingApi.Models
{
    // TODO: Implement RadioAccessCode domain model - placeholder to fix compilation errors
    // This class should wrap TConRadioAccessCode and provide domain logic
    public class RadioAccessCode
    {
        public TConRadioAccessCode _tconRadioAccessCode { get; }

        public RadioAccessCode(TConRadioAccessCode tconRadioAccessCode)
        {
            _tconRadioAccessCode = tconRadioAccessCode;
        }

        public int Id => _tconRadioAccessCode.Idx;
        public int RadioAddress => _tconRadioAccessCode._RadioAddr;
        public int Code => _tconRadioAccessCode._Code;
        public bool IsKeyCode => _tconRadioAccessCode._KeyCode;
        public bool Active => _tconRadioAccessCode._Active;
        public int Option => _tconRadioAccessCode._Option;
    }
}