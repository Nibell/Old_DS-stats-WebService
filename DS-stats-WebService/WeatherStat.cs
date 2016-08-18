using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DS_stats_WebService
{
    public class WeatherStat
    {
        private double geoLat,
                       geoLong,
                       temp,
                       windSpeed;

        public double GeoLat
        {
            get { return geoLat; }
            set { geoLat = value; }
        }

        public double GeoLong
        {
            get { return geoLong; }
            set { geoLong = value; }
        }

        public double Temp
        {
            get { return temp; }
            set { temp = value; }
        }

        public double WindSpeed
        {
            get { return windSpeed; }
            set { windSpeed = value; }
        }

        private string windDirection,
                       weather,
                       starttime,
                       endtime;

        public string WindDirection
        {
            get { return windDirection; }
            set { windDirection = value; }
        }

        public string Weather
        {
            get { return weather; }
            set { weather = value; }
        }

        public string Starttime
        {
            get { return starttime; }
            set { starttime = value; }
        }

        public string Endtime
        {
            get { return endtime; }
            set { endtime = value; }
        }

        private int precipitation;

        public int Precipitation
        {
            get { return precipitation; }
            set { precipitation = value; }
        }

        private int[] hosts;

        public int[] Hosts
        {
            get { return hosts; }
            set { hosts = value; }
        }

        public WeatherStat()
        {
        }

        public WeatherStat(double _geoLat, double _geoLong, double _temp, double _windSpeed, string _windDirection, string _weather, string _starttime, string _endtime, int _precipitation, int[] _hosts)
        {
            geoLat = _geoLat;
            geoLong = _geoLong;
            temp = _temp;
            windSpeed = _windSpeed;
            windDirection = _windDirection;
            weather = _weather;
            starttime = _starttime;
            endtime = _endtime;
            precipitation = _precipitation;
            hosts = _hosts;
        }
    }
}