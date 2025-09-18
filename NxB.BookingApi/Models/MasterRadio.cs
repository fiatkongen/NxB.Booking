using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class MasterRadio : ITConEntityProvider
    {
        private readonly TConMasterRadio _tconMasterRadio;

        public int MasterAddress() => _tconMasterRadio.MasterAddr;
        public string SystemName() => _tconMasterRadio.SystemName;
        public string IpAddress() => _tconMasterRadio.IPaddress;
        public bool IsOnline() => _tconMasterRadio.OnLine;
        public DateTime LastOnline => _tconMasterRadio.LastOnLine;
        public double ScanTime => _tconMasterRadio.ScanTime;
        public int LockState
        {
            get => _tconMasterRadio._LockState;
            set => _tconMasterRadio._LockState = (byte)value;
        }
        public int Quality => _tconMasterRadio.Quality;
        public int RadiosOnlineCount => _tconMasterRadio.RadioOnline;
        public int Type => _tconMasterRadio.Type;
        public string Version => _tconMasterRadio.Version;
        public int FwUpdateSize => _tconMasterRadio.FWupdateSize;
        public int Tbd1UpdateSetup
        {
            get => _tconMasterRadio._TBD1updateSetup;
            set => _tconMasterRadio._TBD1updateSetup = (byte)value;
        }

        public int LogLevel => _tconMasterRadio.LogLevel;
        public int Gsm_RSSI => _tconMasterRadio.GSM_RSSI;
        public bool IsGsmRoaming => _tconMasterRadio.GSM_Roaming;

        public void PublishUpdate()
        {
            this.Tbd1UpdateSetup = 1;
        }

        public MasterRadio(TConMasterRadio tconMasterRadio)
        {
            _tconMasterRadio = tconMasterRadio;
        }

        public object GetTConEntity()
        {
            return _tconMasterRadio;
        }
    }
}
