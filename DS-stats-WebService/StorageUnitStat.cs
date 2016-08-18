using System;
using System.Collections.Generic;
using System.Web;

namespace DS_stats_WebService
{
    public class StorageUnitStat
    {
        private Unit unit;

        public Unit Unit
        {
            get { return unit; }
            set { unit = value; }
        }

        private int used;

        public int Used
        {
            get { return used; }
            set { used = value; }
        }

        private long free;

        public long Free
        {
            get { return free; }
            set { free = value; }
        }

        private string timestamp;

        public string Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        public StorageUnitStat()
        {
        }

        public StorageUnitStat(Unit _unit, int _used, long _free, string _timestamp)
        {
            unit = _unit;
            used = _used;
            free = _free;
            timestamp = _timestamp;
        }
    }
}