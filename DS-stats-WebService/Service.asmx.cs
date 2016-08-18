using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DS_stats_WebService
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://localhost:53261/Service.asmx")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]   // .None ifall man skall överlagra metoder annars .BasicProfile1_1
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service : System.Web.Services.WebService
    {
        string myConnectionString = "server=127.0.0.1;uid=root;" + "pwd=Dag123;database=dsstats;";
        MySqlConnection conn = new MySqlConnection();
        string myConnectionString1 = "server=127.0.0.1;uid=root;" + "pwd=Dag123;database=dsstats;";
        MySqlConnection conn1 = new MySqlConnection();
        MySqlDataReader reader;
        MySqlDataReader reader1;
        double GeoLat;                          //Dessa tre variabler används då man convertarar geoLat och GeoLong strängen man får ifrån databasen.
        double GeoLong;                         //Då dom används av en del olika funktioner deklarerade jag dem globalt.
        string[] doubleParts = new string[2];

        [WebMethod(Description = "Retur: En lista med samtliga enheter som övervakas och som inte har några barn.")]
        public List<Unit> getUnitList(String password)
        {
            List<Unit> UnitList = new List<Unit>();
            if(password.Equals("A@b!761Nn"))
            {
                Unit unit;
                conn.ConnectionString = myConnectionString;
                conn.Open();            

                MySqlCommand cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM Unit WHERE Parent = 0;", conn);
                reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());

                    unit = new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong);
                    UnitList.Add(unit);
                }

                conn.Clone();
                reader.Close();
                return UnitList;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<Unit> list = new List<Unit>();
                list.Add(new Unit(-1, "wrong password", -1, -1, geolat, geolong));
                return list;
            }
        }
        
        [WebMethod(Description = "Retur: En lista med de enheter som är barn till enheten med ID i parametern parent.")]
        public List<Unit> getUnitListByParent(string password, int parent)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<Unit> UnitList = new List<Unit>();
                Unit unit;
                conn.ConnectionString = myConnectionString;
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM Unit WHERE Parent =" + @parent + ";", conn);
                MySqlParameter par = cmd.Parameters.AddWithValue("@parent", parent.ToString());
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());

                    unit = new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong);
                    UnitList.Add(unit);
                }

                conn.Clone();
                reader.Close();
                return UnitList;
            }
            else 
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<Unit> list = new List<Unit>();
                list.Add(new Unit(-1, "wrong password", -1, -1, geolat, geolong));
                return list;
            }
        }
        
        //MessageName var man tvungen att ha om man skulle överlagra metoder.
        [WebMethod(MessageName = "getUnitStats", Description = "Retur: Senaste mätvärden från samtliga enheter som övervakas")]
        public List<HostUnitStat> getUnitStats(string password)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0;", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();

                for (int i = 0; i < hostUnitList.Count; i++)
                {
                    //hämta alla hostUnits[i] som har mätdata från variable: from;
                    cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " order by timestamp desc;", conn);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                    }
                    reader.Close();
                }
                for (int i = 0; i < hostunitstatlist.Count; i++)
                {
                    cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0, 16) + "%';", conn);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                        storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                    }
                    reader.Close();
                    //skapa en array å kopiera över storageunitlist
                    storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                    for (int j = 0; j < storageUnitList.Count; j++)
                    {
                        storageUnitArray[j] = storageUnitList[j];
                    }

                    hostunitstatlist[i].StorageChildren = storageUnitArray;
                    returListan.Add(hostunitstatlist[i]);
                    storageUnitList = new List<StorageUnitStat>();
                }

                hostunitstatlist = new List<HostUnitStat>();

                return returListan;
            }
            else 
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong),null, -1, -1, -1, -1,""));
                return returListan;
            }
        }

        [WebMethod(MessageName = "getUnitStatsByDate", Description = "Samtliga värden från samtliga enheter som övervakas, under en viss period")]
        public List<HostUnitStat> getUnitStatsByDate(string password, String from, String to)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];

                DateTime dateStart = DateTime.Parse(from);
                DateTime dateEnd = DateTime.Parse(to);

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0;", conn);
                reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();
        
                while (dateStart <= dateEnd)
                {
                    from = dateStart.ToString().Substring(0, 10);
                    for (int i = 0; i < hostUnitList.Count; i++)
                    {
                        //hämta alla hostUnits[i] som har mätdata från variable: from;
                        cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " AND timestamp LIKE '" + from + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();
                    }
                    for (int i = 0; i < hostunitstatlist.Count; i++)
                    {
                        cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0,16) + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while(reader.Read())
                        {
                            formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                            storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();
                        //skapa en array å kopiera över storageunitlist
                        storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                        for(int j = 0; j<storageUnitList.Count;j++)
                        {
                            storageUnitArray[j] = storageUnitList[j];
                        }

                        hostunitstatlist[i].StorageChildren = storageUnitArray;
                        returListan.Add(hostunitstatlist[i]);
                        storageUnitList = new List<StorageUnitStat>();
                    }

                    hostunitstatlist = new List<HostUnitStat>();             
                    dateStart = dateStart.AddDays(1);
                }
                return returListan;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong), null, -1, -1, -1, -1, ""));
                return returListan;
            }
        }

        [WebMethod(MessageName = "getUnitStatsByDateInterval", Description = "Samtliga värden från samtliga enheter som övervakas, under en viss period och med ett visst interval. Möjliga intervall: 10min,20min,30min osv")]
        public List<HostUnitStat> getUnitStatsByDateInterval(string password, String from, String to, int interval)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];
                string date;

                DateTime dateStart = DateTime.Parse(from);
            
                DateTime dateEnd = DateTime.Parse(to);

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0;", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();

                while (dateStart <= dateEnd)
                {
                    for (int i = 0; i < hostUnitList.Count; i++)
                    {
                        for (int p = 0; p < 1440 / interval;p++ )//loopa datumen ett dygn för varje unit.
                        {
                            date = dateStart.ToString().Substring(0,16); // vill inte ha sekundrarna. så länge minuten är rätt duger det.
                            //hämta alla hostUnits[i] som har mätdata från variable: from;, börjar på tid 00:00:00
                            cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " AND timestamp LIKE '" + date + "%';", conn);
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                            }
                            reader.Close();
                            dateStart = dateStart.AddMinutes(interval);
                        }
                        dateStart = dateStart.AddDays(-1);
                    }
                    for (int i = 0; i < hostunitstatlist.Count; i++)
                    {
                        cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0,16) + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                            storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();
                    
                        //skapa en array å kopiera över storageunitlist
                        storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                        for (int j = 0; j < storageUnitList.Count; j++)
                        {
                            storageUnitArray[j] = storageUnitList[j];
                        }
                        hostunitstatlist[i].StorageChildren = storageUnitArray;
                        returListan.Add(hostunitstatlist[i]);
                        storageUnitList = new List<StorageUnitStat>();
                    }

                    hostunitstatlist = new List<HostUnitStat>();
                    dateStart = dateStart.AddDays(1);
                }
                return returListan;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong), null, -1, -1, -1, -1, ""));
                return returListan;
            }
        }

        [WebMethod(MessageName = "getUnitStatsById", Description = "Senaste mätvärdem från angiven enhet. ID: id-numret på den enhet som efterfrågas.(Fås genom getUnitlist())")]
        public List<HostUnitStat> getUnitStatsById(string password, int ID)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0 AND ID = " + ID + ";", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();

                for (int i = 0; i < hostUnitList.Count; i++)
                {
                    //hämta alla hostUnits[i] som har mätdata från variable: from;
                    cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " order by timestamp desc;", conn);
                    reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                    }
                    reader.Close();
                }
                for (int i = 0; i < hostunitstatlist.Count; i++)
                {
                    cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0, 16) + "%';", conn);
                    reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                        storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                    }
                    reader.Close();
                    //skapa en array å kopiera över storageunitlist
                    storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                    for (int j = 0; j < storageUnitList.Count; j++)
                    {
                        storageUnitArray[j] = storageUnitList[j];
                    }

                    hostunitstatlist[i].StorageChildren = storageUnitArray;
                    returListan.Add(hostunitstatlist[i]);
                    storageUnitList = new List<StorageUnitStat>();
                }

                hostunitstatlist = new List<HostUnitStat>();

                return returListan;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong), null, -1, -1, -1, -1, ""));
                return returListan;
            }
        }

        [WebMethod(MessageName = "getUnitStatsByIdDate", Description = "Samtliga mätvärden från angiven enhet, under viss period.")]
        public List<HostUnitStat> getUnitStatsByIdDate(string password, int ID, String from, String to)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];

                DateTime dateStart = DateTime.Parse(from);
                DateTime dateEnd = DateTime.Parse(to);

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0 AND ID = " + ID + ";", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();

                while (dateStart <= dateEnd)
                {
                    from = dateStart.ToString().Substring(0, 10);
                    for (int i = 0; i < hostUnitList.Count; i++)
                    {
                        //hämta alla hostUnits[i] som har mätdata från variable: from;
                        cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " AND timestamp LIKE '" + from + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();
                    }
                    for (int i = 0; i < hostunitstatlist.Count; i++)
                    {
                        cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0, 16) + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                            storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();
                        //skapa en array å kopiera över storageunitlist
                        storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                        for (int j = 0; j < storageUnitList.Count; j++)
                        {
                            storageUnitArray[j] = storageUnitList[j];
                        }

                        hostunitstatlist[i].StorageChildren = storageUnitArray;
                        returListan.Add(hostunitstatlist[i]);
                        storageUnitList = new List<StorageUnitStat>();
                    }

                    hostunitstatlist = new List<HostUnitStat>();
                    dateStart = dateStart.AddDays(1);
                }
                return returListan;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong), null, -1, -1, -1, -1, ""));
                return returListan;
            }
        }

        [WebMethod(MessageName = "getUnitStatsByIdDateInterval", Description = "Samtliga mätvärden från angiven enhet, under viss period och med ett visst interval.")]
        public List<HostUnitStat> getUnitStatsByIdDateInterval(string password, int ID, String from, String to, int interval)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<HostUnitStat> hostunitstatlist = new List<HostUnitStat>();
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                List<Unit> hostUnitList = new List<Unit>();
                List<Unit> childUnitList = new List<Unit>();
                List<StorageUnitStat> storageUnitList = new List<StorageUnitStat>();
                StorageUnitStat[] storageUnitArray;
                StorageUnitStat[] sus = new StorageUnitStat[10];
                string date;

                DateTime dateStart = DateTime.Parse(from);

                DateTime dateEnd = DateTime.Parse(to);

                conn.ConnectionString = myConnectionString;
                conn.Open();
                MySqlCommand cmd;

                //hämta alla parents
                cmd = new MySqlCommand("SELECT ID, Name,Parent,Type,GeoLat,GeoLong FROM unit WHERE Parent = 0 AND ID = " + ID + ";", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                    hostUnitList.Add(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong));
                }
                reader.Close();

                while (dateStart <= dateEnd)
                {
                    for (int i = 0; i < hostUnitList.Count; i++)
                    {
                        for (int p = 0; p < 1440 / interval; p++)//loopa datumen ett dygn för varje unit.
                        {
                            date = dateStart.ToString().Substring(0, 16); // vill inte ha sekundrarna. så länge minuten är rätt duger det.
                            //hämta alla hostUnits[i] som har mätdata från variable: from;, börjar på tid 00:00:00
                            cmd = new MySqlCommand("SELECT unitID, cpu, BWUp, BWDown, memory, timestamp FROM hostunitstat WHERE unitId = " + hostUnitList[i].Id + " AND timestamp LIKE '" + date + "%';", conn);
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                hostunitstatlist.Add(new HostUnitStat(hostUnitList[i], null, int.Parse(reader["cpu"].ToString()), int.Parse(reader["BWUp"].ToString()), int.Parse(reader["BWDown"].ToString()), int.Parse(reader["memory"].ToString()), reader["timestamp"].ToString()));
                            }
                            reader.Close();
                            dateStart = dateStart.AddMinutes(interval);
                        }
                        dateStart = dateStart.AddDays(-1);
                    }
                    for (int i = 0; i < hostunitstatlist.Count; i++)
                    {
                        cmd = new MySqlCommand("SELECT unitId, used, free, timestamp, ID, Name, Parent, Type, GeoLat, GeoLong FROM storageunitstat, unit WHERE unitId = ID AND Parent = " + hostunitstatlist[i].Unit.Id + " AND timestamp LIKE '" + hostunitstatlist[i].Timestamp.Substring(0, 16) + "%';", conn);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            formatGeografs(reader["GeoLat"].ToString(), reader["GeoLong"].ToString());
                            storageUnitList.Add(new StorageUnitStat(new Unit(int.Parse(reader["ID"].ToString()), reader["Name"].ToString(), int.Parse(reader["Parent"].ToString()), int.Parse(reader["Type"].ToString()), GeoLat, GeoLong), int.Parse(reader["used"].ToString()), long.Parse(reader["free"].ToString()), reader["timestamp"].ToString()));
                        }
                        reader.Close();

                        //skapa en array å kopiera över storageunitlist
                        storageUnitArray = new StorageUnitStat[storageUnitList.Count];
                        for (int j = 0; j < storageUnitList.Count; j++)
                        {
                            storageUnitArray[j] = storageUnitList[j];
                        }
                        hostunitstatlist[i].StorageChildren = storageUnitArray;
                        returListan.Add(hostunitstatlist[i]);
                        storageUnitList = new List<StorageUnitStat>();
                    }

                    hostunitstatlist = new List<HostUnitStat>();
                    dateStart = dateStart.AddDays(1);
                }
                return returListan;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                List<HostUnitStat> returListan = new List<HostUnitStat>();
                returListan.Add(new HostUnitStat(new Unit(-1, "wrong password", -1, -1, geolat, geolong), null, -1, -1, -1, -1, ""));
                return returListan;
            }
        }

        [WebMethod(Description = "Retur: Senaste mätvärden från alla städer som övervakas")]
        public List<WeatherStat> getWeather(string password)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStat weatherStat;
                conn.ConnectionString = myConnectionString;
                conn.Open();
                conn1.ConnectionString = myConnectionString1;
                conn1.Open();
                int[] Host;
                List<int> hostList = new List<int>();
                String geoLat = "";
                String geoLong = "";

                MySqlCommand cmd = new MySqlCommand("SELECT geoLat,geoLong,temp,windSpeed,windDirection,weather,startTime,endTime,precipitation FROM weatherStat WHERE startTime LIKE (SELECT startTime FROM weatherStat ORDER BY startTime DESC LIMIT 1);", conn);
                MySqlCommand cmd1;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    geoLat = reader["geoLat"].ToString();
                    geoLong = reader["geoLong"].ToString();
                    string glat = geoLat.Substring(0,4);
                    string glong = geoLong.Substring(0,4);

                    
                    cmd1 = new MySqlCommand("SELECT ID FROM unit WHERE GeoLat LIKE '" + glat + "%' AND GeoLong LIKE '" + glong + "%';", conn1);            

                    reader1 = cmd1.ExecuteReader();
                    while (reader1.Read())
                    {
                        hostList.Add(int.Parse(reader1["ID"].ToString()));
                    }
                    Host = new int[hostList.Count];
                    for(int j = 0; j < hostList.Count; j++)
                    {
                        Host[j] = hostList[j];
                    }
                    reader1.Close();

                    formatGeografs(reader["geoLat"].ToString(), reader["geoLong"].ToString());

                    weatherStat = new WeatherStat(GeoLat,GeoLong, double.Parse(reader["temp"].ToString()), double.Parse(reader["windSpeed"].ToString()), reader["windDirection"].ToString(), reader["weather"].ToString(), reader["startTime"].ToString(), reader["endTime"].ToString(), Convert.ToInt32(reader["precipitation"].ToString()), Host);
                    WeatherStatList.Add(weatherStat);
                    hostList = new List<int>();
                }

                conn1.Clone();
                conn.Clone();
                reader.Close();
                return WeatherStatList;
            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                double temp = -1.0;
                int[] Host = new int[1];
                Host[0] = -1;
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStatList.Add(new WeatherStat(geolat,geolong,temp, -1,"wrong password","wrong password","wrong password","wrong password",-1, Host));
                return WeatherStatList;
            }
        }

        [WebMethod(Description = "Retur: Senaste mätvärden från samtliga enheter som övervakas, under en viss period")]
        public List<WeatherStat> getWeatherByDate(string password, string from, string to)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStat weatherStat;
                conn.ConnectionString = myConnectionString;
                conn.Open();
                conn1.ConnectionString = myConnectionString1;
                conn1.Open();
                int[] Host;
                List<int> hostList = new List<int>();
                String geoLat = "";
                String geoLong = "";

                MySqlCommand cmd = new MySqlCommand("SELECT geolat,geolong,temp,windSpeed,windDirection,weather,startTime,endTime,precipitation FROM weatherStat WHERE (startTime LIKE '" + @from + "%' OR endTime LIKE '" + @to + "%') OR ((startTime BETWEEN '" + @from + "%' AND '" + @to + "%'AND endTime NOT LIKE '" + @from + "%') AND (endTime BETWEEN '" + @from + "%' AND '" + @to + "%'AND startTime NOT LIKE '" + @to + "%'));", conn);
                MySqlParameter par1 = cmd.Parameters.AddWithValue("@from", from);
                MySqlParameter par2 = cmd.Parameters.AddWithValue("@to", to);

                MySqlCommand cmd1;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    geoLat = reader["geoLat"].ToString();
                    geoLong = reader["geoLong"].ToString();
                    string glat = geoLat.Substring(0, 4);
                    string glong = geoLong.Substring(0, 4);


                    cmd1 = new MySqlCommand("SELECT ID FROM unit WHERE GeoLat LIKE '" + glat + "%' AND GeoLong LIKE '" + glong + "%';", conn1);

                    reader1 = cmd1.ExecuteReader();
                    while (reader1.Read())
                    {
                        hostList.Add(int.Parse(reader1["ID"].ToString()));
                    }
                    Host = new int[hostList.Count];
                    for (int j = 0; j < hostList.Count; j++)
                    {
                        Host[j] = hostList[j];
                    }
                    reader1.Close();

                    formatGeografs(reader["geoLat"].ToString(), reader["geoLong"].ToString());

                    weatherStat = new WeatherStat(GeoLat, GeoLong, double.Parse(reader["temp"].ToString()), double.Parse(reader["windSpeed"].ToString()), reader["windDirection"].ToString(), reader["weather"].ToString(), reader["startTime"].ToString(), reader["endTime"].ToString(), Convert.ToInt32(reader["precipitation"].ToString()), Host);
                    WeatherStatList.Add(weatherStat);
                    hostList = new List<int>();
                }

                conn1.Clone();
                conn.Clone();
                reader.Close();
                return WeatherStatList;

            }
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                double temp = -1.0;
                int[] Host = new int[1];
                Host[0] = -1;
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStatList.Add(new WeatherStat(geolat,geolong,temp, -1,"wrong password","wrong password","wrong password","wrong password",-1, Host));
                return WeatherStatList;
            }
        }

        [WebMethod(Description = "Retur: Senaste mätvärden från samtliga enheter som övervakas,med en viss GeoLat och GeoLong, under en viss period double skills med .=punkt")]
        public List<WeatherStat> getWeatherByDateCoords(string password, Double lat, Double Long, String from, String to)
        {
            if (password.Equals("A@b!761Nn"))
            {
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStat weatherStat;
                conn.ConnectionString = myConnectionString;
                conn.Open();
                conn1.ConnectionString = myConnectionString1;
                conn1.Open();
                int[] Host;
                List<int> hostList = new List<int>();
                String geoLat = "";
                String geoLong = "";
                String iNGeoLat = "";
                String iNGeoLong = "";
                String uTGeoLat = "";
                String uTGeoLong = "";
                iNGeoLat = lat.ToString();
                iNGeoLong = Long.ToString();

                uTGeoLat = iNGeoLat.Substring(0, 4);
                uTGeoLong = iNGeoLong.Substring(0, 4);

                doubleParts = uTGeoLat.Split(',');
                uTGeoLat = (doubleParts[0] + "." + doubleParts[1]);

                doubleParts = uTGeoLong.Split(',');
                uTGeoLong = (doubleParts[0] + "." + doubleParts[1]);

                MySqlCommand cmd = new MySqlCommand("SELECT geoLat,geoLong,temp,windSpeed,windDirection,weather,startTime,endTime,precipitation FROM weatherStat WHERE geoLat LIKE '" + @uTGeoLat + "%' AND geoLong LIKE '" + @uTGeoLong + "%' AND ((startTime LIKE '" + @from + "%' OR endTime LIKE '" + @to + "%') OR ((startTime BETWEEN '" + @from + "%' AND '" + @to + "%'AND endTime NOT LIKE '" + @from + "%') AND (endTime BETWEEN '" + @from + "%' AND '" + @to + "%'AND startTime NOT LIKE '" + @to + "%')));", conn);
                MySqlParameter par1 = cmd.Parameters.AddWithValue("@uTGeoLat", uTGeoLat);
                MySqlParameter par2 = cmd.Parameters.AddWithValue("@uTGeoLong", uTGeoLong);
                MySqlParameter par3 = cmd.Parameters.AddWithValue("@from", from);
                MySqlParameter par4 = cmd.Parameters.AddWithValue("@to", to);
                
                MySqlCommand cmd1;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    geoLat = reader["geoLat"].ToString();
                    geoLong = reader["geoLong"].ToString();
                    string glat = geoLat.Substring(0, 4);
                    string glong = geoLong.Substring(0, 4);


                    cmd1 = new MySqlCommand("SELECT ID FROM unit WHERE GeoLat LIKE '" + glat + "%' AND GeoLong LIKE '" + glong + "%';", conn1);

                    reader1 = cmd1.ExecuteReader();
                    while (reader1.Read())
                    {
                        hostList.Add(int.Parse(reader1["ID"].ToString()));
                    }
                    Host = new int[hostList.Count];
                    for (int j = 0; j < hostList.Count; j++)
                    {
                        Host[j] = hostList[j];
                    }
                    reader1.Close();

                    formatGeografs(reader["geoLat"].ToString(), reader["geoLong"].ToString());

                    weatherStat = new WeatherStat(GeoLat, GeoLong, double.Parse(reader["temp"].ToString()), double.Parse(reader["windSpeed"].ToString()), reader["windDirection"].ToString(), reader["weather"].ToString(), reader["startTime"].ToString(), reader["endTime"].ToString(), Convert.ToInt32(reader["precipitation"].ToString()), Host);
                    WeatherStatList.Add(weatherStat);
                    hostList = new List<int>();
                }

                conn1.Clone();
                conn.Clone();
                reader.Close();
                return WeatherStatList;
            }
            
            else
            {
                double geolat = -1.0;
                double geolong = -1.0;
                double temp = -1.0;
                int[] Host = new int[1];
                Host[0] = -1;
                List<WeatherStat> WeatherStatList = new List<WeatherStat>();
                WeatherStatList.Add(new WeatherStat(geolat,geolong,temp, -1,"wrong password","wrong password","wrong password","wrong password",-1, Host));
                return WeatherStatList;
            }
        }

        public void formatGeografs(string _GeoLat, string _GeoLong) //funktion som används för att formatera geograferna till double
        {
            try //Ifall en enhet har GeoLong utan comma så blir det en exception
            {
                doubleParts = _GeoLat.Split('.');
                GeoLat = Convert.ToDouble(doubleParts[0] + "," + doubleParts[1]);
            }
            catch (Exception) { GeoLat = Convert.ToDouble(reader["GeoLat"].ToString()); }

            try
            {
                doubleParts = _GeoLong.Split('.');
                GeoLong = Convert.ToDouble(doubleParts[0] + "," + doubleParts[1]);
            }
            catch (Exception) { GeoLong = Convert.ToDouble(reader["GeoLong"].ToString()); }
        }
    }
}