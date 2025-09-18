using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class RadioBase
    {
        protected readonly TConRadio _tconRadio;

        public int RadioAddress => _tconRadio.RadioAddr;
        public int MasterRadioAddress => _tconRadio.MasterAddr;
        public bool IsOnline => _tconRadio.OnLine;
        public int AccessUserCount => _tconRadio.AccessUserCnt;
        public int AccessSocketCount => _tconRadio.AccessSocketCnt;
        public TConRadioType RadioType => _tconRadio.Type;
        public int HopCount => _tconRadio.NoHop;
        public DateTime LastOnline => _tconRadio.LastOnline;
        public int Quality => _tconRadio.Quality;
        public int OfflineCount => _tconRadio.OffLineCount;
        public int PowerOnCount => _tconRadio.PowerOnCount;
        public string Version => _tconRadio.Version;
        public int RSSI => _tconRadio.RSSI;
        public int Noise => _tconRadio.Noise;
        public RadioAccessState AccessState => (RadioAccessState)_tconRadio._AccessState;
        public RadioAccessUpdate AccessUpdate
        {
            get => (RadioAccessUpdate)_tconRadio._AccessState;
            set => _tconRadio._AccessState = (byte)value;
        }

        public virtual async Task OpenSocket(int socket) { }
        public virtual async Task CloseSocket(int socket) { }

        public RadioBase(TConRadio tConRadio)
        {
            _tconRadio = tConRadio ?? throw new ArgumentNullException(nameof(tConRadio));
        }

        public void ForceUpdateOfAccessCodes()
        {
            AccessUpdate = RadioAccessUpdate.ForceCompleteUpdateOfAccessCodes;
        }
    }

    public abstract class RadioSocketLess : RadioBase
    {
        protected RadioSocketLess(TConRadio tConRadio) : base(tConRadio)
        {
        }
    }

    public abstract class RadioMultipleSocket : RadioBase
    {
        protected RadioMultipleSocket(TConRadio tConRadio) : base(tConRadio)
        {
        }

        public override async Task OpenSocket(int socket)
        {
            if (socket == 0) throw new RadioException($"Cannot open socket: 0 for " + this.RadioType + ": " + this.RadioType + " has multiple sockets");
        }

        public override async Task CloseSocket(int socket)
        {
            if (socket == 0) throw new RadioException($"Cannot close socket: 0 for " + this.RadioType + ": " + this.RadioType + " has multiple sockets");
        }

    }

    public class RadioTBD : RadioSocketLess
    {
        public RadioTBD(TConRadio tConRadio) : base(tConRadio)
        {
            if (tConRadio.Type != TConRadioType.TBD && tConRadio.Type != TConRadioType.TWI) throw new RadioException("Cannot create RadioTBE from TconRadioType " + tConRadio.Type);
        }

        public override async Task OpenSocket(int socket)
        {
            if (socket != 0) throw new RadioException($"Cannot open socket: {socket} for TBD: TBD has no sockets");
        }

        public override async Task CloseSocket(int socket)
        {
            if (socket != 0) throw new RadioException($"Cannot close socket: {socket} for TBD: TBD has no sockets");
        }
    }

    public class RadioTWI : RadioMultipleSocket
    {
        public RadioTWI(TConRadio tConRadio) : base(tConRadio)
        {
            if (tConRadio.Type != TConRadioType.TWI) throw new RadioException("Cannot create RadioTWI from TconRadioType " + tConRadio.Type);
        }

        public override async Task OpenSocket(int socket)
        {
            if (socket != 0) throw new RadioException($"Cannot open socket: {socket} for TWI: TWI has no sockets");
        }

        public override async Task CloseSocket(int socket)
        {
            if (socket != 0) throw new RadioException($"Cannot close socket: {socket} for TWI: TWI has no sockets");
        }
    }

    public class RadioTBB : RadioMultipleSocket
    {
        public RadioTBB(TConRadio tConRadio) : base(tConRadio)
        {
            if (tConRadio.Type != TConRadioType.TBB) throw new RadioException("Cannot create RadioTBE from TconRadioType " + tConRadio.Type);
        }
    }

    public class RadioTBE : RadioMultipleSocket
    {
        public RadioTBE(TConRadio tConRadio) : base(tConRadio)
        {
            if (tConRadio.Type != TConRadioType.TBE) throw new RadioException("Cannot create RadioTBE from TconRadioType " + tConRadio.Type);
        }
    }

    public class RadioTWEV : RadioMultipleSocket
    {
        public RadioTWEV(TConRadio tConRadio) : base(tConRadio)
        {
            if (tConRadio.Type != TConRadioType.TWEV) throw new RadioException("Cannot create RadioTVEW from TconRadioType " + tConRadio.Type);
        }
    }
}
