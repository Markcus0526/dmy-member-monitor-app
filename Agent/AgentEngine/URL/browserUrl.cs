using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace AgentEngine
{
    public class URL
    {
        string url;
        string title;
        string browser;
        string accessTime;
        public URL(string url, string title, string browser, string accessTime)
        {
            this.url = url;
            this.title = title;
            this.browser = browser;
            this.accessTime = accessTime;
        }

        public String Url
        {
            get
            {
                return url;
            }
        }

        public String Title
        {
            get
            {
                return title;
            }
        }

        public String VisitTime
        {
            get
            {
                return accessTime;
            }
        }

        public string getData()
        {
            return browser + " - " + title + " - " + url + "-" + accessTime;
        }
    }

    public class GoogleChrome
    {
        public List<URL> URLs = new List<URL>();
        public IEnumerable<URL> GetHistory(string strLocalPath, DateTime lastCheck, int nSessid)
        {
            URLs.Clear();

            // Get Current Users App Data
            string documentsFolder = strLocalPath + "\\Google\\Chrome\\User Data\\Default";

            // Check if directory exists
            if (Directory.Exists(documentsFolder))
            {
                String strGooglePattern = "";
                strGooglePattern = System.Environment.SystemDirectory + "\\" + "GoogleHistory_" + nSessid;
                File.Copy(documentsFolder + "\\History", strGooglePattern, true);
                return ExtractUserHistory(strGooglePattern, lastCheck);

            }
            return null;
        }


        IEnumerable<URL> ExtractUserHistory(string folder, DateTime lastCheck)
        {
            // Get User history info
            DataTable historyDT = ExtractFromTable("urls", folder);

            // Get visit Time/Data info
            //DataTable visitsDT = ExtractFromTable("visits", folder);

            // Loop each history entry
            foreach (DataRow row in historyDT.Rows)
            {

                // Obtain URL and Title strings
                string url = row["url"].ToString();
                string title = row["title"].ToString();
                string time = row["last_visit_time"].ToString();

                double aa = Convert.ToDouble(time); // I am not sure that
                DateTime dtZone = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dtZone = dtZone.AddSeconds(aa / 1000000);//10^7 ,AddSeconds method needs a double
                dtZone = dtZone.ToLocalTime();

                TimeSpan between = DateTime.Now - dtZone;
                if (between.Days <= 60 &&
                    dtZone >= lastCheck)
                {
                    time = dtZone.ToString(ComMisc.strTimeFormat);

                    // Create new Entry
                    URL u = new URL(url.Replace('\'', ' '),
                    title.Replace('\'', ' '),
                    "Google Chrome",
                    time);

                    // Add entry to list
                    URLs.Add(u);
                }
            }

            // Clear URL History

            return URLs;
        }

        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();

            // FireFox database file
            string dbPath = folder;// +"\\History";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +
                ";Version=3;New=False;Compress=True;");

                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }

    public class FireFox
    {
        public List<URL> URLs = new List<URL>();
        public IEnumerable<URL> GetHistory(string strAppPath, DateTime lastCheck)
        {
            URLs.Clear();

            // Get Current Users App Data
            string documentsFolder = strAppPath + "\\Mozilla\\Firefox\\Profiles\\";

            if (!Directory.Exists(documentsFolder))
                return null;

            IEnumerable<String> strEnProfile = Directory.EnumerateDirectories(documentsFolder);
            IEnumerator<String> strProfiles = strEnProfile.GetEnumerator();

            // Check if directory exists
            if (strProfiles.MoveNext())
                return ExtractUserHistory(strProfiles.Current, lastCheck);

            return null;
        }

        IEnumerable<URL> ExtractUserHistory(string folder, DateTime lastCheck)
        {
            // Get User history info
            DataTable placesDT = ExtractFromTable("moz_places", folder);

            int iCnt = 0;
            // Loop each history entry
            foreach (DataRow row in placesDT.Rows)
            {
                // Obtain URL and Title strings
                string url = row["url"].ToString();
                string title = row["title"].ToString();
                //string time = historyDT.Rows[iCnt]["visit_date"].ToString();
                string time = "";
                if (row["last_visit_date"] != null)
                    time = row["last_visit_date"].ToString();

                if (time != "")
                {
                    double aa = Convert.ToDouble(time); // I am not sure that
                    DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    dtZone = dtZone.AddSeconds(aa / 1000000);//10^7 ,AddSeconds method needs a double
                    dtZone = dtZone.ToLocalTime();

                    TimeSpan between = DateTime.Now - dtZone;
                    if (between.Days <= 60 &&
                        dtZone >= lastCheck)
                    {
                        time = dtZone.ToString(ComMisc.strTimeFormat);

                        // Create new Entry
                        URL u = new URL(url.Replace('\'', ' '),
                        title.Replace('\'', ' '),
                        "Mozilla Firefox",
                        time);

                        // Add entry to list
                        URLs.Add(u);
                    }
                }

                iCnt++;
            }

            return URLs;
        }

        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +
                ";Version=3;New=False;Compress=True;");

                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }

    public class SogouExplorer
    {
        public List<URL> URLs = new List<URL>();
        public IEnumerable<URL> GetHistory(string strAppPath, DateTime lastCheck)
        {
            URLs.Clear();
            // Get Current Users App Data
            string documentsFolder = strAppPath + "\\SogouExplorer";

            if (Directory.Exists(documentsFolder))
                return ExtractUserHistory(documentsFolder, lastCheck);

            return null;
        }

        IEnumerable<URL> ExtractUserHistory(string folder, DateTime lastCheck)
        {
            // Get User history info
            DataTable urlhis = ExtractFromTable("tb_urlhistory", folder);
            DataTable urlinfo = ExtractFromTable("tb_urlinfo", folder);

            int iCnt = 0;
            // Loop each history entry
            foreach (DataRow row in urlhis.Rows)
            {
                // Obtain URL and Title strings
                string urlid = row["urlid"].ToString();
                string time = row["time"].ToString();
                string url = "";
                string title = "";

                foreach (DataRow inforow in urlinfo.Rows)
                {
                    string urlInfoId = inforow["urlid"].ToString();
                    if (urlInfoId == urlid)
                    {
                        url = inforow["url"].ToString();
                        title = inforow["title"].ToString();
                        break;
                    }
                }

                DateTime dtZone = DateTime.Parse(time);

                TimeSpan between = DateTime.Now - dtZone;
                if (between.Days <= 60 &&
                    dtZone >= lastCheck)
                {
                    // Create new Entry
                    URL u = new URL(url.Replace('\'', ' '),
                    title.Replace('\'', ' '),
                    "Sogou Explorer",
                    dtZone.ToString(ComMisc.strTimeFormat));

                    // Add entry to list
                    URLs.Add(u);
                }
                iCnt++;
            }

            return URLs;
        }

        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();

            // FireFox database file
            string dbPath = folder + "\\uhistory.db";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +
                ";Version=3;New=False;Compress=True;");

                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }
}
