using System;
using System.Collections.Generic;
using System.Web;

namespace DS_stats_WebService
{
    public class Unit
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int parent;

        public int Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        private int type;

        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        private double geoLat,
                       geoLong;

        public double GeoLong
        {
            get { return geoLong; }
            set { geoLong = value; }
        }

        public double GeoLat
        {
            get { return geoLat; }
            set { geoLat = value; }
        }

        public Unit()
        {
        }

        public Unit(int _id, string _name, int _parent, int _type, double _geoLat, double _geoLong)
        {
            id = _id;
            name = _name;
            parent = _parent;
            type = _type;
            geoLat = _geoLat;
            geoLong = _geoLong;
        }
    }
}