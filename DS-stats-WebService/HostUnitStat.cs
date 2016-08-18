using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DS_stats_WebService
{
    public class HostUnitStat
    {
        private Unit unit;

        public Unit Unit
        {
            get { return unit; }
            set { unit = value; }
        }

        private StorageUnitStat[] storageChildren;

        public StorageUnitStat[] StorageChildren
        {
            get { return storageChildren; }
            set { storageChildren = value; }
        }

        private int cpu,
                    bwUp,
                    bwDown,
                    memory;

        public int Cpu
        {
            get { return cpu; }
            set { cpu = value; }
        }

        public int BwUp
        {
            get { return bwUp; }
            set { bwUp = value; }
        }

        public int BwDown
        {
            get { return bwDown; }
            set { bwDown = value; }
        }

        public int Memory
        {
            get { return memory; }
            set { memory = value; }
        }

        private string timestamp;

        public string Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        public HostUnitStat()
        {
        }

        public HostUnitStat(Unit _unit, StorageUnitStat[] _storageChildren, int _cpu, int _bwUp, int _bwDown, int _memory, string _timestamp)
        {
            unit = _unit;
            storageChildren = _storageChildren;
            cpu = _cpu;
            bwUp = _bwUp;
            bwDown = _bwDown;
            memory = _memory;
            timestamp = _timestamp;
        }
    }
}