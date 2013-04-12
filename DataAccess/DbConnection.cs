using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using DraftAdmin.Models;
using System.Data;
using DraftAdmin.Output;
using System.IO;
using Oracle.DataAccess.Client;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml.Linq;
using DraftAdmin.Global;
using System.Windows.Forms;
using DraftAdmin.PlayoutCommands;
using System.Windows;

namespace DraftAdmin.DataAccess
{
    public class DbConnection
    {
        public delegate void SetStatusBarMsgEventHandler(string msgText, string msgColor);

        public delegate void SendCommandNoTransitionsEventHandler(PlayerCommand command);

        public static event SetStatusBarMsgEventHandler SetStatusBarMsg;

        public static event SendCommandNoTransitionsEventHandler SendCommandNoTransitionsEvent;

        public static scliveweb.Service WebService;

        private static MySqlConnection createConnectionMySql()
        {
            MySqlConnection cn = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySqlDbConn"].ConnectionString);

            cn.Open();

            return cn;
        }

        private static OracleConnection createConnectionSDR()
        {
            OracleConnection cn = new OracleConnection(ConfigurationManager.ConnectionStrings["SDRDbConn"].ConnectionString);

            cn.Open();

            return cn;
        }

        public static ObservableCollection<Pick> GetDraftOrder(ObservableCollection<Team> teams)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl = null;
            ObservableCollection<Pick> picks = new ObservableCollection<Pick>();

            try
            {   
                cn = createConnectionSDR();

                if (cn != null)
                {
                    string sql = "select * from espnews.draftorder";
                    cmd = new OracleCommand(sql, cn);
                    rdr = cmd.ExecuteReader();
                    tbl = new DataTable();

                    tbl.Load(rdr);
                    rdr.Close();

                    Pick pick;

                    foreach (DataRow row in tbl.Rows)
                    {
                        pick = new Pick();
                        pick.OverallPick = Convert.ToInt16(row["pick"].ToString());
                        pick.Round = Convert.ToInt16(row["round"].ToString());
                        pick.RoundPick = Convert.ToInt16(row["roundpick"].ToString());
                        //pick.Team = GetTeam(Convert.ToInt32(row["teamid"].ToString()));                        
                        pick.Team = (Team)teams.SingleOrDefault(s => s.ID == Convert.ToInt32(row["teamid"]));
                        
                        picks.Add(pick);
                    }
                }
            }            
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return picks;
        }

        public static ObservableCollection<Player> GetPlayers(BackgroundWorker worker = null)
        {
            ObservableCollection<Player> players = new ObservableCollection<Player>();

            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl = null;

            int i = 0;

            try
            {
                cn = createConnectionSDR();

                if (cn != null)
                {
                    String sql = "select a.*, b.*, c.*, (select text from espnews.drafttidbits where referencetype = 1 and referenceid = a.playerid and tidbitorder = 999) as tradetidbit ";
                    sql += "from espnews.draftplayers a left join espnews.news_teams b on a.schoolid = b.team_id left join espnews.draftorder c on a.pick = c.pick order by a.pick asc, lastname asc, firstname asc";

                    cmd = new OracleCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    foreach (DataRow row in tbl.Rows)
                    {
                        Player player = createPlayerModel(row);
                        players.Add(player);

                        i++;

                        int percent = Convert.ToInt32(((double)i / tbl.Rows.Count) * 100);

                        if (worker != null)
                        {
                            worker.ReportProgress(percent);
                        }                        
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return players;
        }

        public static List<Player> GetPlayersBySchool(Int32 schoolId)
        {
            List<Player> players = new List<Player>();

            return players;
        }

        private static List<string> getPlayersByConf(Int32 confId)
        {
            List<string> players = new List<string>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataAdapter adp = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionMySql();

                if (cn != null)
                {
                    String sql = "SELECT * FROM players a JOIN schools b ON a.schoolid = b.schoolid ";
                    sql += "JOIN conferences c ON b.conferenceid = c.conferenceid ";
                    sql += "WHERE c.conferenceid = " + confId + " ";
                    sql += "ORDER BY a.lastname ASC";

                    cmd = new MySqlCommand(sql, cn);
                    adp = new MySqlDataAdapter(cmd);

                    tbl = new DataTable();

                    adp.Fill(tbl);

                    string player = null;

                    foreach (DataRow row in tbl.Rows)
                    {

                        if (row["rankspecial"] != DBNull.Value)
                        {
                            player = "#" + row["rankspecial"].ToString() + " ";
                        }

                        player += row["position"].ToString() + " ";

                        player += row["firstname"].ToString() + " " + row["lastname"].ToString() + " - ";

                        player += row["schoolfullname"].ToString();

                        players.Add(player);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (adp != null) adp.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return players;
        }

        public static Player GetPlayer(Int32 playerId)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl = null;

            Player player = null;

            try
            {
                cn = createConnectionSDR();

                if (cn != null)
                {
                    String sql = "select a.*, b.*, c.*, (select text from espnews.drafttidbits where referencetype = 1 and referenceid = " + playerId + " and tidbitorder = 999) as tradetidbit ";
                    sql += "from espnews.draftplayers a left join espnews.news_teams b on a.schoolid = b.team_id left join espnews.draftorder c on a.pick = c.pick ";
                    sql += "where a.playerid = " + playerId;

                    cmd = new OracleCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    if (tbl.Rows.Count == 1)
                    {
                        player = createPlayerModel(tbl.Rows[0]);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return player;
        }

        public static Player GetPlayerByPick(int pick)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl = null;

            Player player = null;

            try
            {
                cn = createConnectionSDR();

                if (cn != null)
                {
                    String sql = "select a.*, (select text from espnews.drafttidbits where referencetype = 1 and referenceid = a.playerid and tidbitorder = 999) as tradetidbit ";
                    sql += "from espnews.draftplayers a left join espnews.news_teams b on a.schoolid = b.team_id left join espnews.draftorder c on a.pick = c.pick where a.pick = " + pick;

                    cmd = new OracleCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    if (tbl.Rows.Count == 1)
                    {
                        player = createPlayerModel(tbl.Rows[0]);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return player;
        }

        public static Team GetTeam(Int32 teamId)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;
            Team team = null;

            try
            {
                cn = createConnectionMySql();

                if (cn != null)
                {
                    String sql = "select * from teams where id = " + teamId;

                    cmd = new MySqlCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    if (tbl.Rows.Count > 0)
                    {
                        DataRow row = tbl.Rows[0];

                        team = new Team();
                        team.ID = Convert.ToInt32(row["id"]);                    
                        team.FullName = row["name"].ToString();
                        team.Tricode = row["tricode"].ToString();
                        team.City = row["city"].ToString();
                        team.Name = row["name"].ToString();
                        team.OverallRecord = row["overallrecord"].ToString();
                        team.ConferenceRecord = row["conferencerecord"].ToString();

                        if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
                        {
                            team.Tidbits = GetTidbitsMySql(2, Convert.ToInt32(row["id"]));
                        }
                        else
                        {
                            team.Tidbits = GetTidbitsSDR(2, Convert.ToInt32(row["id"]));
                        } 
                    
                        team.LogoTga = new Uri(row["logo"].ToString());
                    }
                }
                 else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return team;
        }

        public static Category GetCategory(Int32 categoryId)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;
            Category category = null;

            try
            {
                cn = createConnectionMySql();

                if (cn != null)
                {
                    String sql = "SELECT * FROM categories WHERE categoryid = " + categoryId;

                    cmd = new MySqlCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    if (tbl.Rows.Count > 0)
                    {
                        DataRow row = tbl.Rows[0];

                        category = new Category();
                        category.ID = Convert.ToInt32(row["categoryid"]);

                        category.FullName = row["categoryname"].ToString();
                        category.Tricode = row["tricode"].ToString();

                        category.Tidbits = GetTidbitsMySql(3, Convert.ToInt32(row["categoryid"]));

                        if (row["logonokey"] != DBNull.Value)
                        {
                            if (row["logonokey"].ToString().Substring(0, 2).ToUpper() != "C:" && row["logonokey"].ToString().Substring(0, 2) != "\\\\")
                            {
                                category.LogoTga = new Uri(ConfigurationManager.AppSettings["TemplateDirectory"].ToString() + "\\" + row["logonokey"].ToString());
                            }
                            else
                            {
                                category.LogoTga = new Uri(row["logonokey"].ToString());
                            }
                        }

                        if (row["swatch"] != DBNull.Value)
                        {
                            if (row["swatch"].ToString().Substring(0, 2).ToUpper() != "C:" && row["swatch"].ToString().Substring(0, 2) != "\\\\")
                            {
                                category.SwatchFile = new Uri(ConfigurationManager.AppSettings["TemplateDirectory"].ToString() + "\\" + row["swatch"].ToString());
                            }
                            else
                            {
                                category.SwatchFile = new Uri(row["swatch"].ToString());
                            }
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return category;
        }

        private static Player createPlayerModel(DataRow row)
        {
            Player player = new Player();
            player.PlayerId = Convert.ToInt32(row["playerid"]);
            player.FirstName = row["firstname"].ToString();
            player.LastName = row["lastname"].ToString();
            player.Height = row["height"].ToString();
            player.Weight = row["weight"].ToString();
            player.Class = row["class"].ToString();
            player.TradeTidbit = row["tradetidbit"].ToString();

            if (row["kiperrank"] != DBNull.Value)
            {
                player.KiperRank = Convert.ToInt16(row["kiperrank"]);
            }

            if (row["mcshayrank"] != DBNull.Value)
            {
                player.McShayRank = Convert.ToInt16(row["mcshayrank"]);
            }

            player.Position = row["position"].ToString();

            player.Tidbits = GetTidbitsSDR(1, player.PlayerId);

            try
            {
                if (row["schoolid"].ToString() != "")
                {
                    player.School = (Team)GlobalCollections.Instance.Schools.SingleOrDefault(s => s.ID == Convert.ToInt32(row["schoolid"]));
                }
            }
            catch (Exception ex)
            {

            }
            
            if (row["pick"] != DBNull.Value)
            {
                if (Convert.ToInt16(row["pick"]) > 0)
                {
                    player.Pick = GlobalCollections.Instance.DraftOrder.FirstOrDefault(p => p.OverallPick == Convert.ToInt16(row["pick"]));
                }
            }

            return player;
        }

        public static ObservableCollection<Tidbit> GetTidbitsSDR(int typeId, Int32 refId)
        {
            ObservableCollection<Tidbit> tidbits = new ObservableCollection<Tidbit>();
                        
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionSDR();

                if (cn != null)
                {
                    String sql = "select * from drafttidbits where referencetype = " + typeId + " and referenceid = " + refId + " and tidbitorder < 999 order by tidbitorder";
                    cmd = new OracleCommand(sql, cn);
                    rdr = cmd.ExecuteReader();

                    tbl = new DataTable();

                    tbl.Load(rdr);

                    rdr.Close();
                    rdr.Dispose();

                    foreach (DataRow row in tbl.Rows)
                    {
                        Tidbit tidbit = new Tidbit();
                        tidbit.ReferenceType = Convert.ToInt16(row["referencetype"]);
                        tidbit.ReferenceID = Convert.ToInt32(row["referenceid"]);
                        tidbit.TidbitOrder = Convert.ToInt16(row["tidbitorder"]);
                        tidbit.TidbitText = row["text"].ToString();

                        if (row["enabled"] != DBNull.Value)
                        {
                            tidbit.Enabled = Convert.ToBoolean(row["enabled"]);
                        }
                        else
                        {
                            tidbit.Enabled = false;
                        }

                        tidbits.Add(tidbit);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return tidbits;
        }

        public static ObservableCollection<Tidbit> GetTidbitsMySql(int typeId, Int32 refId)
        {
            ObservableCollection<Tidbit> tidbits = new ObservableCollection<Tidbit>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from tidbits where referencetype = " + typeId + " and referenceid = " + refId + " and tidbitorder < 999 order by tidbitorder";
                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                foreach (DataRow row in tbl.Rows)
                {
                    Tidbit tidbit = new Tidbit();
                    tidbit.ReferenceType = Convert.ToInt16(row["referencetype"]);
                    tidbit.ReferenceID = Convert.ToInt32(row["referenceid"]);
                    tidbit.TidbitOrder = Convert.ToInt16(row["tidbitorder"]);
                    tidbit.TidbitText = row["text"].ToString();

                    if (row["enabled"] != DBNull.Value)
                    {
                        tidbit.Enabled = Convert.ToBoolean(row["enabled"]);
                    }
                    
                    tidbits.Add(tidbit);
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return tidbits;
        }

        public static ObservableCollection<Team> GetSchools(BackgroundWorker worker = null)
        {
            ObservableCollection<Team> schools = new ObservableCollection<Team>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            int i = 0;

            try
            {
                cn = createConnectionMySql();

                String sql = "";

                switch (ConfigurationManager.AppSettings["DraftType"].ToUpper())
                {
                    case "LHN":
                    case "NFL":
                        sql = "select * from teams where league = 'NCAAF' or league = 'NCF23' order by league asc, name asc";
                        break;
                    case "NBA":
                        sql = "select * from teams where league = 'NCAAB' or league = 'OLYMB' order by league asc, name asc";
                        break;
                }               

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                Team school;

                foreach (DataRow row in tbl.Rows)
                {
                    school = new Team();
                    school.ID = Convert.ToInt32(row["id"]);
                    school.FullName = row["name"].ToString();
                    school.Tricode = row["tricode"].ToString();
                    school.City = row["city"].ToString();
                    school.Name = row["name"].ToString();
                    school.League = row["league"].ToString();

                    if (row["logo"].ToString() != "")
                    {
                        school.LogoTga = new Uri(row["logo"].ToString());
                    }
                    
                    if (row["swatch"].ToString() != "")
                    {
                        school.SwatchTga = new Uri(row["swatch"].ToString());
                    }
                    
                    school.OverallRecord = row["overallrecord"].ToString();
                    school.ConferenceRecord = row["conferencerecord"].ToString();

                    //takes too long to load all of the school's tidbits.  we're not using schoold tidbits for the draft anyway...
                    //school.Tidbits = GetTidbitsSDR(2, Convert.ToInt32(row["id"]));
                    
                    schools.Add(school);

                    i++;

                    int percent = Convert.ToInt32(((double)i / tbl.Rows.Count) * 100);

                    if (worker != null)
                    {
                        worker.ReportProgress(percent);
                    }    
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return schools;
        }

        public static void GetSchoolsFromSDR()
        {
            int teamsToImport = 0;
            int teamsImported = 0;

            SetStatusBarMsg("Importing schools...", "Yellow");

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                OracleConnection cn = null;
                OracleCommand cmd = null;
                OracleDataReader rdr = null;
                DataTable tbl = null;

                try
                {
                    cn = createConnectionSDR();

                    if (cn != null)
                    {
                        String sql = "select * from espnews.news_teams where league_id = 'NCAAB'";
                        cmd = new OracleCommand(sql, cn);
                        rdr = cmd.ExecuteReader();

                        tbl = new DataTable();

                        tbl.Load(rdr);

                        rdr.Close();
                        rdr.Dispose();

                        MySqlConnection myCn = createConnectionMySql();

                        teamsToImport = tbl.Rows.Count;

                        foreach (DataRow row in tbl.Rows)
                        {
                            sql = "select * from teams where id = " + row["team_id"].ToString();

                            MySqlCommand myCmd = new MySqlCommand(sql, myCn);
                            MySqlDataAdapter myAdp = new MySqlDataAdapter(myCmd);
                            MySqlCommandBuilder myBldr = new MySqlCommandBuilder(myAdp);
                            DataTable myTbl = new DataTable();
                            DataRow myRow;

                            try
                            {
                                myAdp.Fill(myTbl);

                                if (myTbl.Rows.Count == 0)
                                {
                                    myRow = myTbl.Rows.Add();
                                }
                                else
                                {
                                    myRow = myTbl.Rows[0];
                                }

                                myRow["id"] = Convert.ToInt32(row["team_id"]);
                                myRow["name"] = row["team_name"].ToString();
                                myRow["tricode"] = row["abbrev_4"].ToString();
                                myRow["city"] = row["city_st_name"].ToString();
                                myRow["league"] = row["league_id"].ToString();

                                DataSet dsLogos = getSchoolLogos(new Int32[] { Convert.ToInt32(row["team_id"].ToString()) });

                                if (dsLogos != null)
                                {
                                    myRow["logo"] = "\\\\HEADSHOT01\\Images\\" + dsLogos.Tables[0].Rows[0]["IMAGEPATH"].ToString().ToUpper().Replace(".TGA", "_256.TGA");
                                    myRow["swatch"] = "\\\\HEADSHOT01\\Images\\" + dsLogos.Tables[0].Rows[0]["SWATCHPATH"].ToString().ToUpper();
                                }

                                myAdp.Update(myTbl.GetChanges());
                                myTbl.AcceptChanges();

                                teamsImported++;

                                worker.ReportProgress(teamsImported / teamsToImport);
                            }
                            finally
                            {
                                if (myCmd != null) myCmd.Dispose();
                                if (myTbl != null) myTbl.Dispose();
                                if (myAdp != null) myAdp.Dispose();
                                if (myBldr != null) myBldr.Dispose();
                            }
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                    }
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (tbl != null) tbl.Dispose();
                    if (cn != null) cn.Close(); cn.Dispose();
                }
            }; //do work

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " schools successfully imported at " + DateTime.Now.ToLongTimeString() + ".", "Green");
                GlobalCollections.Instance.LoadSchools();
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " of " + teamsToImport.ToString() + " schools imported.", "Yellow");
            };

            worker.RunWorkerAsync();
        }

        public static ObservableCollection<Team> GetProTeams(BackgroundWorker worker = null)
        {
            ObservableCollection<Team> teams = new ObservableCollection<Team>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            int i = 0;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from teams where league = '" + ConfigurationManager.AppSettings["DraftType"].ToString() + "' order by city asc, name asc";
                //String sql = "select * from teams where league = 'NFL' order by city asc, name asc";

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                Team team;

                foreach (DataRow row in tbl.Rows)
                {
                    team = new Team();
                    team.ID = Convert.ToInt32(row["id"]);
                    team.FullName = row["city"].ToString() + " " + row["name"].ToString();
                    team.Tricode = row["tricode"].ToString();
                    team.City = row["city"].ToString();
                    team.Name = row["name"].ToString();
                    team.LogoTga = new Uri(row["logo"].ToString());
                    team.SwatchTga = new Uri(row["swatch"].ToString());
                    team.OverallRecord = row["overallrecord"].ToString();
                    team.ConferenceRecord = row["conferencerecord"].ToString();

                    if (row["lotterypctrank"] != DBNull.Value)
                    {
                        team.LotteryPctRank = Convert.ToInt16(row["lotterypctrank"].ToString());
                    }

                    if (row["lotteryorder"] != DBNull.Value)
                    {
                        team.LotteryOrder = Convert.ToInt16(row["lotteryorder"].ToString());
                    }

                    if (ConfigurationManager.AppSettings["DraftType"].ToString().ToUpper() == "NBA")
                    {
                        team.PickPlateTga = new Uri(ConfigurationManager.AppSettings["PickPlateDirectory"].ToString() + "\\NBA.tga");
                    }
                    else
                    {
                        team.PickPlateTga = new Uri(ConfigurationManager.AppSettings["PickPlateDirectory"].ToString() + "\\" + row["name"].ToString().ToUpper() + ".tga");
                    }
                    
                    if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
                    {
                        team.Tidbits = GetTidbitsMySql(2, Convert.ToInt32(row["id"]));
                    }
                    else
                    {
                        team.Tidbits = GetTidbitsSDR(2, Convert.ToInt32(row["id"]));
                    }                    

                    teams.Add(team);

                    i++;

                    int percent = Convert.ToInt32(((double)i / tbl.Rows.Count) * 100);

                    if (worker != null)
                    {
                        worker.ReportProgress(percent);
                    }    
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return teams;
        }

        public static void GetProTeamsFromSDR()
        {
            int teamsToImport = 0;
            int teamsImported = 0;

            SetStatusBarMsg("Importing pro teams...", "Yellow");

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                OracleConnection cn = null;
                OracleCommand cmd = null;
                OracleDataReader rdr = null;
                DataTable tbl = null;

                try
                {
                    cn = createConnectionSDR();

                    if (cn != null)
                    {
                        String sql = "select * from espnews.news_teams where league_id = '" + ConfigurationManager.AppSettings["DraftType"].ToString() + "'";
                        cmd = new OracleCommand(sql, cn);
                        rdr = cmd.ExecuteReader();

                        tbl = new DataTable();

                        tbl.Load(rdr);

                        rdr.Close();
                        rdr.Dispose();

                        MySqlConnection myCn = createConnectionMySql();

                        teamsToImport = tbl.Rows.Count;

                        foreach (DataRow row in tbl.Rows)
                        {
                            sql = "select * from teams where id = " + row["team_id"].ToString();

                            MySqlCommand myCmd = new MySqlCommand(sql, myCn);
                            MySqlDataAdapter myAdp = new MySqlDataAdapter(myCmd);
                            MySqlCommandBuilder myBldr = new MySqlCommandBuilder(myAdp);
                            DataTable myTbl = new DataTable();
                            DataRow myRow;

                            try
                            {
                                myAdp.Fill(myTbl);

                                if (myTbl.Rows.Count == 0)
                                {
                                    myRow = myTbl.Rows.Add();
                                }
                                else
                                {
                                    myRow = myTbl.Rows[0];
                                }

                                myRow["id"] = Convert.ToInt32(row["team_id"]);
                                myRow["name"] = row["team_name"].ToString();
                                myRow["tricode"] = row["abbrev_4"].ToString();
                                myRow["city"] = row["city_st_name"].ToString();
                                myRow["league"] = row["league_id"].ToString();

                                DataSet dsLogos = getSchoolLogos(new Int32[] { Convert.ToInt32(row["team_id"].ToString()) });

                                if (dsLogos != null)
                                {
                                    myRow["logo"] = ConfigurationManager.AppSettings["ImsDirectory"].ToString().ToUpper() + "\\" + dsLogos.Tables[0].Rows[0]["IMAGEPATH"].ToString().ToUpper().Replace(".TGA", "_256.TGA");
                                    myRow["swatch"] = ConfigurationManager.AppSettings["ImsDirectory"].ToString().ToUpper() + "\\" + dsLogos.Tables[0].Rows[0]["SWATCHPATH"].ToString().ToUpper();
                                }

                                myAdp.Update(myTbl.GetChanges());
                                myTbl.AcceptChanges();
                            }
                            finally
                            {
                                if (myCmd != null) myCmd.Dispose();
                                if (myTbl != null) myTbl.Dispose();
                                if (myAdp != null) myAdp.Dispose();
                                if (myBldr != null) myBldr.Dispose();
                            }

                            teamsImported++;

                            worker.ReportProgress(teamsImported / teamsToImport);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("There was a problem connecting to the SDR database");
                    }
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (tbl != null) tbl.Dispose();
                    if (cn != null) cn.Close(); cn.Dispose();
                }
            }; //do work

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " teams successfully imported at " + DateTime.Now.ToLongTimeString() + ".", "Green");
                
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " of " + teamsToImport.ToString() + " teams imported.", "Yellow");
            };

            worker.RunWorkerAsync();
        }

        private static void saveXml(ObservableCollection<Team> schools, string file)
        {
            XElement root = new XElement("schools");
            foreach (var item in schools)
            {
                string logo = "";

                if (item.LogoTga != null)
                {
                    logo = item.LogoTga.LocalPath;
                }

                var xml = new XElement("school",
                           new XAttribute("name", item.Name),
                           //new XAttribute("league", item.League),
                           new XAttribute("logo", logo),
                           new XAttribute("id", item.ID),
                           new XAttribute("tricode", item.Tricode),
                           new XAttribute("city", item.City)
                          );

                root.Add(xml);
            }

            root.Save(file);
        }

        private static DataSet getSchoolLogos(Int32[] schoolsArr)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataAdapter adp = null;
            DataSet ds = null;
            int i = 0;

            try
            {
                cn = createConnectionSDR();

                Int32[] bindSize = new Int32[schoolsArr.Length - 1];                
                OracleParameterStatus[] bindStatus = new OracleParameterStatus[schoolsArr.Length -1];

                for (i = 0; i < schoolsArr.Length - 1; i++)
                {
                    bindSize[i] = 10;
                    bindStatus[i] = OracleParameterStatus.Success;                    
                }

                cmd = cn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "IMS.TV_GETS.getImagePath";
                cmd.BindByName = true;

                cmd.Parameters.Add(new OracleParameter("OCURSOR", OracleDbType.RefCursor, ParameterDirection.Output));
                
                cmd.Parameters.Add(new OracleParameter("INIDARRAY", OracleDbType.Int32));
                cmd.Parameters["INIDARRAY"].CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                cmd.Parameters["INIDARRAY"].Value = schoolsArr;
                cmd.Parameters["INIDARRAY"].Size = schoolsArr.Length;
                cmd.Parameters["INIDARRAY"].ArrayBindSize = bindSize;
                cmd.Parameters["INIDARRAY"].ArrayBindStatus = bindStatus;
                cmd.Parameters["INIDARRAY"].Direction = ParameterDirection.Input;

                cmd.Parameters.Add(new OracleParameter("INIMAGETYPE", OracleDbType.Int32, 2, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("INHIDEFIND", OracleDbType.Varchar2, "N", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("INGETSWATCH", OracleDbType.Varchar2, "Y", ParameterDirection.Input));

                adp = new OracleDataAdapter(cmd);

                ds = new DataSet();

                DateTime start = DateTime.Now;

                adp.Fill(ds);

                TimeSpan elapsed = DateTime.Now - start;
                Debug.Print("Time to get IMS logos: " + elapsed.ToString());
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (ds != null) ds.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return ds;
        }
          
        public static ObservableCollection<Category> GetCategories(int categoryTypeId)
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from categories where categorytypeid = " + categoryTypeId + " order by categoryname asc";

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                Category category;

                foreach (DataRow row in tbl.Rows)
                {
                    category = new Category();
                    category.ID = Convert.ToInt32(row["categoryid"]);

                    category.FullName = row["categoryname"].ToString();
                    category.Tricode = row["tricode"].ToString();

                    category.Tidbits = GetTidbitsMySql(3, Convert.ToInt32(row["categoryid"]));

                    if (row["logo"] != DBNull.Value) 
                    {
                        if (row["logo"].ToString().Substring(0, 2).ToUpper() != "C:" && row["logo"].ToString().Substring(0, 2) != "\\\\")
                        {
                            category.LogoTga = new Uri(ConfigurationManager.AppSettings["TemplateDirectory"].ToString() + "\\" + row["logo"].ToString());
                        }
                        else
                        {
                            category.LogoTga = new Uri(row["logo"].ToString());
                        }
                    }

                    if (row["swatch"] != DBNull.Value)
                    {
                        if (row["swatch"].ToString().Substring(0, 2).ToUpper() != "C:" && row["swatch"].ToString().Substring(0, 2) != "\\\\")
                        {
                            category.SwatchFile = new Uri(ConfigurationManager.AppSettings["TemplateDirectory"].ToString() + "\\" + row["swatch"].ToString());
                        }
                        else
                        {
                            category.SwatchFile = new Uri(row["swatch"].ToString());
                        }
                    }

                    categories.Add(category);
                }

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
                GC.Collect();
            }

            return categories;
        }

        public static ObservableCollection<Playlist> GetPlaylists()
        {
            ObservableCollection<Playlist> playlists = new ObservableCollection<Playlist>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from playlists where playlisttype = '" + ConfigurationManager.AppSettings["DraftType"].ToString() + "' order by playlistname asc";

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                Playlist playlist;

                foreach (DataRow row in tbl.Rows)
                {
                    playlist = new Playlist();

                    playlist.PlaylistID = Convert.ToInt16(row["playlistid"].ToString());
                    playlist.PlaylistName = row["playlistname"].ToString();
                                        
                    playlists.Add(playlist);
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return playlists;
        }
        
        public static ObservableCollection<PlaylistItem> GetPlaylistItems(int playlistId)
        {
            ObservableCollection<PlaylistItem> playlistItems = new ObservableCollection<PlaylistItem>();

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from playlistitems where playlistid = " + playlistId + " order by playlistorder asc";

                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);

                rdr.Close();
                rdr.Dispose();

                PlaylistItem playlistItem;

                foreach (DataRow row in tbl.Rows)
                {
                    playlistItem = new PlaylistItem();

                    playlistItem.Template = row["template"].ToString();
                    playlistItem.PanelType = row["paneltype"].ToString();
                    playlistItem.PageType = row["pagetype"].ToString();
                    playlistItem.Description = row["desc"].ToString();
                    playlistItem.Datasource = row["datasource"].ToString();
                    playlistItem.QueryType = row["querytype"].ToString();
                    playlistItem.QueryParameters = row["queryparameters"].ToString();
                    playlistItem.OutputParameter = row["outputparameter"].ToString();

                    if (row["enabled"] != DBNull.Value)
                    {
                        playlistItem.Enabled = Convert.ToBoolean(row["enabled"]);
                    }
                    else
                    {
                        playlistItem.Enabled = false;
                    }

                    if (row["mergedatanotransitions"] != DBNull.Value)
                    {
                        playlistItem.MergeDataNoTransitions = Convert.ToBoolean(row["mergedatanotransitions"]);
                    }
                    else
                    {
                        playlistItem.MergeDataNoTransitions = false;
                    }

                    if (row["playlistorder"] != DBNull.Value)
                    {
                        playlistItem.PlaylistOrder = Convert.ToInt16(row["playlistorder"]);
                    }
                    else
                    {
                        playlistItem.PlaylistOrder = 9999;
                    }

                    if (row["duration"] != DBNull.Value)
                    {
                        playlistItem.Duration = Convert.ToDecimal(row["duration"]);
                    }
                    else
                    {
                        playlistItem.Duration = 5;
                    }

                    playlistItem.Query = row["query"].ToString();

                    if (row["maxrows"] != DBNull.Value)
                    {
                        playlistItem.MaxRows = Convert.ToInt16(row["maxrows"]);
                    }
                    else
                    {
                        playlistItem.MaxRows = 1;
                    }

                    //if (row["rowindex"] != DBNull.Value)
                    //{
                    //    playlistItem.RowIndex = Convert.ToInt16(row["rowindex"]);
                    //}
                    //else
                    //{
                    //    playlistItem.RowIndex = 0;
                    //}

                    if (row["additionaldatafields"].ToString().Trim() != "")
                    {
                        string[] additionalDataFields = row["additionaldatafields"].ToString().Split('|');

                        if (additionalDataFields.Length > 0)
                        {
                            playlistItem.AdditionalDataFields = new Dictionary<string, string>();

                            for (int i = 0; i < additionalDataFields.Length; i++)
                            {
                                string[] keyVal = additionalDataFields[i].ToString().Split('=');
                                playlistItem.AdditionalDataFields.Add(keyVal[0], keyVal[1].Trim());
                            }
                        }
                    }                    
                    
                    playlistItems.Add(playlistItem);
                }

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return playlistItems;
        }

        public static List<XmlDataRow> GetPlaylistItemData(PlaylistItem playlistItem)
        {
            List<XmlDataRow> xmlDataRows = new List<XmlDataRow>();

            OracleConnection cnO = null;
            OracleCommand cmdO = null;
            OracleDataAdapter adpO = null;
            OracleCommandBuilder bldrO = null;
            OracleDataReader rdrO = null;

            MySqlConnection cnM = null;
            MySqlCommand cmdM = null;
            MySqlDataReader rdrM = null;

            DataTable tbl = null;
            DataSet ds = null;

            string teamLogo = " ";
            string teamSwatch = " ";
            string teamAbbrev = "";

            try
            {
                tbl = new DataTable();

                List<Int32> teamIds = new List<Int32>();

                Int32 teamId = 0;

                FileInfo queryFile = null;
                
                if (playlistItem.Query != null && playlistItem.Query.ToString().Trim() != "")
                {
                    queryFile = new FileInfo(ConfigurationManager.AppSettings["QueryDirectory"].ToString() + "\\" + playlistItem.Query);
                }

                string query = "";

                if (queryFile != null)
                {
                    if (queryFile.Exists)
                    {
                        query = File.ReadAllText(queryFile.FullName);
                    }
                    else
                    {
                        query = playlistItem.Query;
                    }

                    Object[] parms = null;

                    switch (playlistItem.Datasource.ToUpper())
                    {
                        case "SDR":
                            cnO = createConnectionSDR();

                            cmdO = new OracleCommand();
                            cmdO.BindByName = true;

                            //add the parms to the query/stored proc if there are any
                            if (playlistItem.QueryParameters.ToString() != "")
                            {
                                parms = playlistItem.QueryParameters.Split('|');

                                for (int i = 0; i < parms.Length; i++)
                                {
                                    string[] parmval = parms[i].ToString().Split('=');

                                    string parm = parmval[0].ToString();
                                    string val = parmval[1].ToString();

                                    if (val.Substring(0, 1) == "'" && val.Substring(val.Length - 1, 1) == "'")
                                    {
                                        string parmStr = val.Substring(1, val.Length - 2);

                                        if (parmStr.Length > 0 && parmStr.Substring(0, 1) == "#")
                                        {
                                            if (ConfigurationManager.AppSettings[parmStr.Substring(1)] != null) //pull the value from the app.config file
                                            {
                                                parmStr = ConfigurationManager.AppSettings[parmStr.Substring(1)].ToString();
                                            }
                                        }

                                        if (parmStr.IndexOf(",") > -1 || (parm.IndexOf('(') > -1 && parm.IndexOf(')') > -1)) //this is an array
                                        {
                                            cmdO.Parameters.Add(new OracleParameter(parm, OracleDbType.Varchar2));

                                            string[] arrVals = parmStr.Split(',');

                                            Int32[] arrSize = new Int32[arrVals.Length - 1];

                                            for (int p = 0; p < arrVals.Length - 1; p++)
                                            {
                                                arrSize[p] = arrVals[p].Length;
                                            }

                                            cmdO.Parameters[parm].CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                                            cmdO.Parameters[parm].Value = arrVals;
                                            cmdO.Parameters[parm].Size = arrVals.Length;
                                            cmdO.Parameters[parm].ArrayBindSize = arrSize;

                                            OracleParameterStatus[] arrStatus = new OracleParameterStatus[1];
                                            arrStatus[0] = 0;

                                            cmdO.Parameters[parm].ArrayBindStatus = arrStatus;
                                            cmdO.Parameters[parm].Direction = ParameterDirection.Input;
                                        }
                                        else
                                        {
                                            cmdO.Parameters.Add(new OracleParameter(parm, OracleDbType.Varchar2, parmStr, ParameterDirection.Input));
                                        }

                                    }
                                    else if (val.ToUpper() == "NULL")
                                    {
                                        cmdO.Parameters.Add(new OracleParameter(parm, OracleDbType.NVarchar2, DBNull.Value, ParameterDirection.Input));
                                    }
                                    else
                                    {
                                        if (val.ToString().ToUpper() != "OTC")
                                        {
                                            if (val.Substring(0, 1) == "#")
                                            {
                                                if (ConfigurationManager.AppSettings[val.Substring(1)] != null)
                                                {
                                                    val = ConfigurationManager.AppSettings[val.Substring(1)];
                                                }
                                            }

                                            cmdO.Parameters.Add(new OracleParameter(parm, OracleDbType.Int32, val, ParameterDirection.Input));
                                        }
                                    }

                                    if (parm.ToUpper() == "INTEAMID")
                                    {
                                        //adding a special case for the On The Clock (connected) items, to add the team swatch to the dataset
                                        if (val.ToString().ToUpper() == "OTC")
                                        {
                                            teamId = GlobalCollections.Instance.OnTheClock.Team.ID;
                                            cmdO.Parameters.Add(new OracleParameter(parm, OracleDbType.Int32, teamId, ParameterDirection.Input));
                                        }
                                        else
                                        {
                                            if (val.Substring(0, 1) == "#")
                                            {
                                                if (ConfigurationManager.AppSettings[val.Substring(1)] != null)
                                                {
                                                    val = ConfigurationManager.AppSettings[val.Substring(1)];
                                                }
                                            }

                                            Int32.TryParse(val, out teamId);
                                        }

                                        teamIds.Add(teamId);
                                    }

                                }
                            }

                            if (playlistItem.QueryType.ToUpper() == "SP")
                            {
                                cmdO.Connection = cnO;
                                cmdO.CommandText = playlistItem.Query;
                                cmdO.CommandType = System.Data.CommandType.StoredProcedure;
                                cmdO.BindByName = true;

                                cmdO.Parameters.Add(new OracleParameter(playlistItem.OutputParameter, OracleDbType.RefCursor, ParameterDirection.Output));

                                adpO = new OracleDataAdapter(cmdO);

                                ds = new DataSet();

                                adpO.Fill(ds);

                                tbl = ds.Tables[0];
                            }
                            else
                            {
                                if (query != "")
                                {
                                    cmdO.CommandText = query.ToString();
                                    cmdO.Connection = cnO;
                                    rdrO = cmdO.ExecuteReader();
                                    tbl.Load(rdrO);
                                    rdrO.Close();
                                    rdrO.Dispose();
                                }

                            }

                            break;
                        case "MYSQL":
                            {
                                cnM = createConnectionMySql();
                                cmdM = new MySqlCommand(query.ToString(), cnM);

                                if (playlistItem.QueryType.ToUpper() == "SP")
                                {
                                    cmdM.CommandType = System.Data.CommandType.StoredProcedure;

                                    if (playlistItem.QueryParameters.ToString() != "")
                                    {
                                        parms = playlistItem.QueryParameters.Split('|');

                                        for (int i = 0; i < parms.Length; i++)
                                        {
                                            string[] parmval = parms[i].ToString().Split('=');

                                            string parm = parmval[0].ToString();
                                            string val = parmval[1].ToString();

                                            if (parm.ToUpper() == "INTEAMID")
                                            {
                                                //adding a special case for the On The Clock (connected) items, to add the team swatch to the dataset
                                                if (val.ToString().ToUpper() == "OTC")
                                                {
                                                    teamId = GlobalCollections.Instance.OnTheClock.Team.ID;
                                                    val = teamId.ToString();
                                                }
                                                else
                                                {
                                                    if (val.Substring(0, 1) == "#")
                                                    {
                                                        if (ConfigurationManager.AppSettings[val.Substring(1)] != null)
                                                        {
                                                            val = ConfigurationManager.AppSettings[val.Substring(1)];
                                                        }
                                                    }

                                                    Int32.TryParse(val, out teamId);
                                                }

                                                teamIds.Add(teamId);
                                            }

                                            cmdM.Parameters.AddWithValue(parm, val);
                                        }
                                    }
                                }

                                rdrM = cmdM.ExecuteReader();
                                tbl.Load(rdrM);
                                rdrM.Close();
                                rdrM.Dispose();
                            }

                            break;
                        case "WS":
                            parms = playlistItem.QueryParameters.Split('|');

                            Object[] wsParms = new Object[parms.Length];

                            for (int i = 0; i < parms.Length; i++)
                            {
                                string[] parmval = parms[i].ToString().Split('=');

                                string parm = parmval[0].ToString();
                                string val = parmval[1].ToString();

                                val = val.Replace("'", "");

                                if (val.Length > 0 && val.Substring(0, 1) == "#")
                                {
                                    if (ConfigurationManager.AppSettings[val.Substring(1)] != null)
                                    {
                                        val = ConfigurationManager.AppSettings[val.Substring(1)];
                                    }
                                }

                                wsParms[i] = val;
                            }

                            DataSet dsTemp = null;

                            try
                            {
                                dsTemp = WebService.CallFunctionByName(playlistItem.Query, wsParms);
                            }
                            catch
                            {
                                Debug.Print("Web Service call failed");
                                dsTemp = null;
                            }

                            if (dsTemp != null)
                            {
                                tbl = dsTemp.Tables[0];
                            }

                            break;
                    }
                
                }
         
                if (playlistItem.AdditionalDataFields != null)
                {
                    foreach (KeyValuePair<string, string> pair in playlistItem.AdditionalDataFields)
                    {
                        if (pair.Key.ToString().ToUpper().IndexOf("INTEAMID") > -1)
                        {
                            string val = pair.Value;

                            if (val.Substring(0, 1) == "#")
                            {
                                if (ConfigurationManager.AppSettings[val.Substring(1)] != null)
                                {
                                    val = ConfigurationManager.AppSettings[val.Substring(1)];
                                }
                            }

                            Int32.TryParse(val, out teamId);

                            teamIds.Add(teamId);
                        }
                    }
                }

                if (tbl.Rows.Count == 0 && playlistItem.Query != null)
                {
                    Debug.Print("No data returned by " + playlistItem.Query);
                }

                XmlDataRow xmlDataRow;

                switch (playlistItem.MaxRows)
                {
                    case 0:
                        //just add a blank data row
                        xmlDataRow = new XmlDataRow();
                        xmlDataRows.Add(xmlDataRow);
                        break;
                    case 1:
                        foreach (DataRow row in tbl.Rows)
                        {
                            //DataRow row = tbl.Rows[playlistItem.RowIndex];

                            xmlDataRow = new XmlDataRow();

                            foreach (DataColumn col in tbl.Columns)
                            {
                                xmlDataRow.Add(col.ColumnName, row[col.ColumnName].ToString());
                            }

                            if (playlistItem.Description.ToUpper() != "PROMPTER")
                            {
                                xmlDataRow.Add("PANEL_TYPE", playlistItem.PanelType);
                                xmlDataRow.Add("PAGE_TYPE", playlistItem.PageType);
                            }

                            if (playlistItem.AdditionalDataFields != null)
                            {
                                foreach (KeyValuePair<string, string> pair in playlistItem.AdditionalDataFields)
                                {
                                    if (pair.Key.ToString().ToUpper().IndexOf("INTEAMID") == -1) //non-teamid items
                                    {
                                        //possibly put a db field to determine if additional fields should be loaded only on the first item
                                        if (playlistItem.CurrentRow == 0)
                                        {
                                            xmlDataRow.Add(pair.Key, pair.Value);
                                        }
                                    }
                                }
                            }

                            xmlDataRows.Add(xmlDataRow);
                        }            
                        break;
                    default:
                        xmlDataRow = new XmlDataRow();
                    int count = 1;

                    if (tbl.Rows.Count > 0)
                    {
                        foreach (DataRow row in tbl.Rows)
                        {
                            if (count > playlistItem.MaxRows)
                            {
                                xmlDataRows.Add(xmlDataRow);
                                xmlDataRow = new XmlDataRow();
                                count = 1;
                            }

                            foreach (DataColumn col in tbl.Columns)
                            {
                                xmlDataRow.Add(col.ColumnName + "_" + count.ToString(), row[col.ColumnName].ToString());
                            }

                            xmlDataRow.Add("PANEL_TYPE", playlistItem.PanelType);
                            xmlDataRow.Add("PAGE_TYPE", playlistItem.PageType);

                            if (playlistItem.AdditionalDataFields != null)
                            {
                                foreach (KeyValuePair<string, string> pair in playlistItem.AdditionalDataFields)
                                {
                                    //possibly put a db field to determine if additional fields should be loaded only on the first item
                                    if (playlistItem.CurrentRow == 0)
                                    {
                                        xmlDataRow.Add(pair.Key, pair.Value);
                                    }
                                }
                            }

                            count++;
                        }

                        ////blank out any extra fields that aren't filled...
                        for (int i = count; i <= playlistItem.MaxRows; i++)
                        {
                            foreach (DataColumn col in tbl.Columns)
                            {
                                xmlDataRow.Add(col.ColumnName + "_" + i.ToString(), "");
                            }
                        }

                        xmlDataRows.Add(xmlDataRow);  //adds in the last row
                    }
                        break; 
                }

                //add all the team data to each xmlDataRow (so the team info is included with each data row)
                if (teamIds.Count > 0)
                {
                    for (var i = 0; i < teamIds.Count; i++)
                    {
                        Team team = null;

                        if (GlobalCollections.Instance.Teams != null)
                        {
                            team = (Team)GlobalCollections.Instance.Teams.SingleOrDefault(t => t.ID == teamIds[i]);

                            if (team == null)
                            {
                                team = (Team)GlobalCollections.Instance.Schools.SingleOrDefault(t => t.ID == teamIds[i]);
                            }
                        }
                        else
                        {
                            team = (Team)GlobalCollections.Instance.Schools.SingleOrDefault(t => t.ID == teamIds[i]);
                        }

                        if (team != null)
                        {

                            teamLogo = team.LogoTgaNoKey.LocalPath;

                            FileInfo file = new FileInfo(teamLogo);

                            if (file.Exists == false)
                            {
                                teamLogo = " ";
                            }

                            teamSwatch = team.SwatchTga.LocalPath;

                            file = new FileInfo(teamSwatch);

                            if (file.Exists == false)
                            {
                                teamSwatch = " ";
                            }

                            teamAbbrev = team.Tricode;

                            foreach (XmlDataRow xmlRow in xmlDataRows)
                            {
                                xmlRow.Add("LOGO_" + (i + 1).ToString(), teamLogo);
                                xmlRow.Add("ABBREV_4_" + (i + 1).ToString(), teamAbbrev);
                                xmlRow.Add("SWATCH_" + (i + 1).ToString(), teamSwatch);
                                xmlRow.Add("VENT_SWATCH_" + (i + 1).ToString(), teamSwatch);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                SetStatusBarMsg("Error getting playlist item data (item " + playlistItem.PlaylistOrder.ToString() + "): " + ex.Message, "Red");
                xmlDataRows.Clear();
            }
            finally
            {
                if (cmdO != null) cmdO.Dispose();
                if (cmdM != null) cmdM.Dispose();
                if (tbl != null) tbl.Dispose();

                if (cnO != null)
                {
                    cnO.Close();
                    cnO.Dispose();
                }

                if (cnM != null)
                {
                    cnM.Close();
                    cnM.Dispose();
                }
            }
            
            return xmlDataRows;
        }

        public static bool SavePlayer(Player player)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataAdapter adp = null;
            OracleCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;

            bool saved = false;

            try
            {
                cn = createConnectionSDR();

                String sql = "select * from draftplayers where playerid = " + player.PlayerId;

                cmd = new OracleCommand(sql, cn);
                adp = new OracleDataAdapter(cmd);
                bldr = new OracleCommandBuilder(adp);
                tbl = new DataTable();
               
                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                    row["playerid"] = player.PlayerId;
                }
                else
                {
                    row = tbl.Rows[0];
                }
                
                row["firstname"] = player.FirstName;
                row["lastname"] = player.LastName;

                int oldRank = 0;

                if (row["kiperrank"] != DBNull.Value)
                {
                    oldRank = Convert.ToInt16(row["kiperrank"]);
                }

                if (oldRank != player.KiperRank)
                {
                    updateKiperRanks(player, oldRank);
                }
                
                if (player.KiperRank == 0)
                {
                    row["kiperrank"] = DBNull.Value;
                }
                else
                {
                    row["kiperrank"] = player.KiperRank;
                }
                
                if (player.School != null)
                {
                    row["schoolid"] = player.School.ID;
                }
                else
                {
                    row["schoolid"] = DBNull.Value;
                }
                
                row["position"] = player.Position;
                row["height"] = player.Height;
                row["weight"] = player.Weight;
                row["class"] = player.Class;

                //update the trade tidbit (NBA)
                UpdateTidbitSDR(1, player.PlayerId, 999, player.TradeTidbit, null, true);
                
                if (player.Tidbits != null)
                {
                    foreach (Tidbit tidbit in player.Tidbits)
                    {
                        UpdateTidbitSDR(tidbit.ReferenceType, tidbit.ReferenceID, tidbit.TidbitOrder, tidbit.TidbitText, null, tidbit.Enabled);
                    }
                }

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();                
            }

            return saved;
        }

        public static bool SelectPlayer(Player player)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            OracleDataAdapter adp = null;
            OracleCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;
            string sql;
            Pick currentPick = GlobalCollections.Instance.OnTheClock;
            int totalPicks = 0;

            bool saved = false;

            try
            {
                cn = createConnectionSDR();

                sql = "select count(*) from draftorder";
                cmd = new OracleCommand(sql, cn);
                rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {
                    rdr.Read();
                    totalPicks = Convert.ToInt16(rdr[0]);
                }

                rdr.Close();
                rdr.Dispose();

                if (currentPick.OverallPick > 0 && currentPick.OverallPick <= totalPicks)
                {
                    sql = "select * from draftplayers where playerid = " + player.PlayerId;

                    cmd = new OracleCommand(sql, cn);
                    adp = new OracleDataAdapter(cmd);
                    bldr = new OracleCommandBuilder(adp);
                    tbl = new DataTable();

                    adp.Fill(tbl);

                    if (tbl.Rows.Count > 0)
                    {                       
                        row = tbl.Rows[0];

                        if (row["pick"] == DBNull.Value || row["pick"].ToString() == "0")
                        {
                            row["pick"] = currentPick.OverallPick;

                            adp.Update(tbl.GetChanges());
                            tbl.AcceptChanges();

                            saved = true;
                        }
                        
                    }

                    //disable all the team finish/ranks pages for NFL
                    if (ConfigurationManager.AppSettings["DraftType"].ToUpper() == "NFL")
                    {
                        sql = "update drafttidbits set enabled = 0 where referencetype = 2 and referenceid = " + currentPick.Team.ID + " and tidbitorder >= 10 and tidbitorder < 40";
                        cmd = new OracleCommand(sql, cn);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        GlobalCollections.Instance.LoadTeams();
                    }

                }                
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static bool SaveTeam(Team team)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataAdapter adp = null;
            MySqlCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;

            bool saved = false;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from teams where id = " + team.ID;

                cmd = new MySqlCommand(sql, cn);
                adp = new MySqlDataAdapter(cmd);
                bldr = new MySqlCommandBuilder(adp);
                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                }
                else
                {
                    row = tbl.Rows[0];
                }

                row["name"] = team.FullName;
                row["tricode"] = team.Tricode;

                row["overallrecord"] = team.OverallRecord;
                row["conferencerecord"] = team.ConferenceRecord;

                row["lotterypctrank"] = team.LotteryPctRank;
                row["lotteryorder"] = team.LotteryOrder;

                OracleConnection cnO = createConnectionSDR();
                
                try
                {  
                    foreach (Tidbit tidbit in team.Tidbits)
                    {
                        if (ConfigurationManager.AppSettings["TeamTidbitsDatabase"].ToString().ToUpper() == "MYSQL")
                        {
                            updateTidbitMySql(tidbit.ReferenceType, tidbit.ReferenceID, tidbit.TidbitOrder, tidbit.TidbitText, null, tidbit.Enabled);
                        }
                        else
                        {
                            UpdateTidbitSDR(tidbit.ReferenceType, tidbit.ReferenceID, tidbit.TidbitOrder, tidbit.TidbitText, null, tidbit.Enabled);
                        }
                    }
                }
                finally
                {
                    if (cnO != null) cnO.Close(); cnO.Dispose();
                }

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static bool SaveCategory(Category category)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataAdapter adp = null;
            MySqlCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;

            bool saved = false;

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from categories where categoryid = " + category.ID;

                cmd = new MySqlCommand(sql, cn);
                adp = new MySqlDataAdapter(cmd);
                bldr = new MySqlCommandBuilder(adp);
                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                }
                else
                {
                    row = tbl.Rows[0];
                }

                row["categoryname"] = category.FullName;
                row["tricode"] = category.Tricode;

                foreach (Tidbit tidbit in category.Tidbits)
                {
                    updateTidbitMySql(tidbit.ReferenceType, category.ID, tidbit.TidbitOrder, tidbit.TidbitText, tidbit.Timecode, tidbit.Enabled);
                }

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static bool SavePoll(List<string> pollLines)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataAdapter adp = null;
            MySqlCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row;

            bool saved = false;

            List<string[]> lines = new List<string[]>();

            foreach (string line in pollLines)
            {
                lines.Add(line.Split('|'));
            }

            try
            {
                cn = createConnectionMySql();

                String sql = "select * from poll where pollid = 1";

                cmd = new MySqlCommand(sql, cn);
                adp = new MySqlDataAdapter(cmd);
                bldr = new MySqlCommandBuilder(adp);
                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                }
                else
                {
                    row = tbl.Rows[0];
                }

                if (lines.Count > 0)
                {
                    foreach (string[] line in lines)
                    {
                        switch (line[0].ToString().ToUpper())
                        {
                            case "Q":
                                row["question"] = line[1].ToString();
                                break;
                            case "P":
                                row["pollname"] = line[1].ToString();
                                break;
                            case "T":
                                row["totalvotes"] = line[1].ToString();
                                break;
                            default:
                                row["answer" + line[0].ToString()] = line[1].ToString();
                                row["answer" + line[0].ToString() + "pct"] = line[3].ToString();
                                break;
                        }

                    }

                    adp.Update(tbl.GetChanges());
                    tbl.AcceptChanges();
                }

                saved = true;
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static void UpdatePollShowAnswers(bool show)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;

            try
            {
                cn = createConnectionMySql();

                cmd = new MySqlCommand("update poll set showanswers = " + show.ToString(), cn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }
        }

        public static bool GetShowPollAnswers()
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;

            bool result = false;

            try
            {
                cn = createConnectionMySql();

                cmd = new MySqlCommand("select showanswers from poll", cn);
                rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {
                    rdr.Read();

                    if (rdr[0] != DBNull.Value)
                    {
                        result = Convert.ToBoolean(rdr[0].ToString());
                    }
                }

                rdr.Close();
                rdr.Dispose();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static void UpdateNumberOfPollAnswers(int num)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            string sql = "";

            try
            {
                cn = createConnectionMySql();

                switch (num)
                {
                    case 2:
                        sql = "update poll set answer1enabled = true, answer2enabled = true, answer3enabled = false, answer4enabled = false, answer5enabled = false";
                        break;
                    case 3:
                        sql = "update poll set answer1enabled = true, answer2enabled = true, answer3enabled = true, answer4enabled = false, answer5enabled = false";
                        break;
                    case 4:
                        sql = "update poll set answer1enabled = true, answer2enabled = true, answer3enabled = true, answer4enabled = true, answer5enabled = false";
                        break;
                    case 5:
                        sql = "update poll set answer1enabled = true, answer2enabled = true, answer3enabled = true, answer4enabled = true, answer5enabled = true";
                        break;
                }

                cmd = new MySqlCommand(sql, cn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }
        }

        private static void updateTidbitMySql(int tidbitTypeId, Int32 refId, int tidbitOrder, string tidbitText = "", string timecode = "", bool enabled = false)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataAdapter adp = null;
            MySqlCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;

            try
            {
                cn = createConnectionMySql();

                string sql = "select * from tidbits where referencetype = " + tidbitTypeId + " and referenceid = " + refId + " and tidbitorder = " + tidbitOrder;
                cmd = new MySqlCommand(sql, cn);
                adp = new MySqlDataAdapter(cmd);
                bldr = new MySqlCommandBuilder(adp);
                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                }
                else
                {
                    row = tbl.Rows[0];
                }

                row["referencetype"] = tidbitTypeId;
                row["referenceid"] = refId;
                row["tidbitorder"] = tidbitOrder;
                row["text"] = tidbitText;
                row["enabled"] = enabled;

                //row["timecode"] = timecode;

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
            }
        }

        public static bool UpdateTidbitSDR(int tidbitTypeId, Int32 refId, int tidbitOrder, string tidbitText = "", string timecode = "", bool enabled = false, int newTidbitOrder = 0)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataAdapter adp = null;
            OracleCommandBuilder bldr = null;
            DataTable tbl = null;
            DataRow row = null;

            bool updated = false;

            try
            {
                cn = createConnectionSDR();

                string sql = "select * from drafttidbits where referencetype = " + tidbitTypeId + " and referenceid = " + refId + " and tidbitorder = " + tidbitOrder;
                cmd = new OracleCommand(sql, cn);
                adp = new OracleDataAdapter(cmd);
                bldr = new OracleCommandBuilder(adp);
                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                }
                else
                {
                    row = tbl.Rows[0];
                }

                row["referencetype"] = tidbitTypeId;
                row["referenceid"] = refId;

                if (newTidbitOrder > 0)
                {
                    row["tidbitorder"] = newTidbitOrder;
                }
                else
                {
                    row["tidbitorder"] = tidbitOrder;
                }

                row["text"] = tidbitText;
                row["enabled"] = enabled;

                //row["timecode"] = timecode;

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();

                updated = true;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (adp != null) adp.Dispose();
                if (bldr != null) bldr.Dispose();
                if (tbl != null) tbl.Dispose();
            }

            return updated;
        }

        private static void updateKiperRanks(Player player, int rank)
        {
            OracleConnection cn = null;
            OracleCommand cmdMax = null;
            OracleCommand cmdRank = null;
            OracleCommand cmdCount = null;
            OracleDataReader rdrCount = null;

            try
            {

                cn = createConnectionSDR();

                string sql = "select max(kiperrank) FROM draftplayers";
                cmdMax = new OracleCommand(sql, cn);
                
                OracleDataReader rdr = cmdMax.ExecuteReader();
                int maxRank = 0;

                if (rdr.HasRows)
                {
                    rdr.Read();

                    if (rdr[0] != DBNull.Value)
                    {
                        maxRank = Convert.ToInt16(rdr[0]);
                    }
                    else
                    {
                        maxRank = 1;
                    }
                }

                cmdMax.Dispose();
                rdr.Close();
                rdr.Dispose();

                sql = "select count(*) from draftplayers where (kiperrank > 0 and kiperrank is not null) or playerid = " + player.PlayerId;
                cmdCount = new OracleCommand(sql, cn);
                rdrCount = cmdCount.ExecuteReader();
                int count = 0;

                if (rdrCount.HasRows)
                {
                    rdrCount.Read();
                    count = Convert.ToInt16(rdrCount[0]);
                }

                cmdCount.Dispose();
                rdrCount.Close();
                rdrCount.Dispose();

                if (player.KiperRank > 0)
                {
                    if (player.KiperRank > count)
                    {
                        player.KiperRank = count;
                    }
                    else
                    {
                        if (player.KiperRank < count)
                        {
                            if (player.KiperRank >= (maxRank + 1))
                            {
                                player.KiperRank = maxRank;
                            }
                        }
                    }

                    sql = "";

                    if (player.KiperRank <= maxRank)
                    {
                        if (rank > player.KiperRank)
                        {
                            sql = "update draftplayers set kiperrank = (kiperrank + 1) where kiperrank <= " + rank.ToString() + " and kiperrank >= " + player.KiperRank.ToString() + " and playerid <> " + player.PlayerId;
                        }
                        else if (rank < player.KiperRank)
                        {
                            if (rank > 0)
                            {
                                sql = "update draftplayers set kiperrank = (kiperrank - 1) where kiperrank > " + rank.ToString() + " and kiperrank <= " + player.KiperRank.ToString() + " and playerid <> " + player.PlayerId;
                            }
                            else
                            {
                                sql = "update draftplayers set kiperrank = (kiperrank + 1) where kiperrank >= " + player.KiperRank.ToString() + " and playerid <> " + player.PlayerId;
                            }
                        }
                    }
                }
                else
                {
                    if (rank > 0) //old rank was not 0, new rank is 0
                    {
                        sql = "update draftplayers set kiperrank = (kiperrank - 1) where kiperrank > " + rank;
                    }
                }

                if (sql != "")
                {
                    cmdRank = new OracleCommand(sql, cn);
                    cmdRank.ExecuteNonQuery();
                }
            }
            finally
            {
                if (cmdMax != null) cmdMax.Dispose();
                if (cmdCount != null) cmdCount.Dispose();
                if (cmdRank != null) cmdRank.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }
        }

        public static bool DeleteTidbitMySql(int tidbitTypeId, Int32 refId, int tidbitOrder)
        {
            bool result = false;
            MySqlConnection cn = null;
            MySqlCommand cmd = null;

            try
            {
                cn = createConnectionMySql();

                string sql = "CALL sp_DeleteTidbit(" + tidbitTypeId + ", " + refId + ", " + tidbitOrder + ")";
                cmd = new MySqlCommand(sql, cn);

                cmd.ExecuteNonQuery();

                result = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();                
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return result;
        }

        public static bool DeleteTidbitSDR(int tidbitTypeId, Int32 refId, int tidbitOrder)
        {
            bool result = false;
            OracleConnection cn = null;
            OracleCommand cmd = null;

            try
            {
                cn = createConnectionSDR();
                
                string sql = "delete from drafttidbits where referencetype = " + tidbitTypeId + " and referenceid = " + refId + " and tidbitorder = " + tidbitOrder;
                cmd = new OracleCommand(sql, cn);

                cmd.ExecuteNonQuery();

                result = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static bool AddTidbitMySql(int typeId, Int32 refId)
        {
            bool result = false;
            MySqlConnection cn = null;
            MySqlCommand cmd = null;

            try
            {
                cn = createConnectionMySql();

                string sql = "CALL sp_AddTidbit(" + typeId + ", " + refId + ")";

                cmd = new MySqlCommand(sql, cn);

                cmd.ExecuteNonQuery();

                result = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose(); 
            }

            return result;
        }

        public static bool AddTidbitSDR(int typeId, Int32 refId)
        {
            bool result = false;
            OracleConnection cn = null;
            OracleCommand cmd = null;

            try
            {
                cn = createConnectionSDR();

                string sql = "insert into drafttidbits (referenceid, referencetype, tidbitorder, enabled) values (" + refId + ", " + typeId + ", ";
                sql += "(select coalesce(max(tidbitorder) + 1, 1) from drafttidbits where referenceid = " + refId + " and referencetype = " + typeId + "), 0)"; 

                cmd = new OracleCommand(sql, cn);

                cmd.ExecuteNonQuery();

                result = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static Int32 AddPlayer(Player player)
        {
            Int32 playerId = 0;
            Int32 result = 0;
            OracleConnection cn = null;
            OracleCommand cmd = null;

            try
            {
                cn = createConnectionSDR();

                cmd = new OracleCommand();
                cmd.Connection = cn;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "ST_DVDB2.players_pkg.proc_players_insert";
                cmd.BindByName = true;

                OracleParameter outputParam = new OracleParameter("nPLAYER_ID", OracleDbType.Int32, ParameterDirection.Output);

                cmd.Parameters.Add(new OracleParameter("cFIRST_NAME", OracleDbType.Varchar2, player.FirstName, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cMIDDLE_NAME", OracleDbType.Varchar2, "", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cLAST_NAME", OracleDbType.Varchar2, player.LastName, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cMAIDEN_NAME", OracleDbType.Varchar2, "", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cCOMMON_FIRST_NAME", OracleDbType.Varchar2, player.FirstName, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cCOMMON_MIDDLE_NAME", OracleDbType.Varchar2, "", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cCOMMON_LAST_NAME", OracleDbType.Varchar2, player.LastName, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cSTREET_ADDRESS", OracleDbType.Varchar2, "Street Address", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cLOCATION_ID", OracleDbType.Varchar2, "LI", ParameterDirection.Input));

                if (player.Weight != null)
                {
                    cmd.Parameters.Add(new OracleParameter("nWEIGHT", OracleDbType.Int16, Convert.ToInt16(player.Weight), ParameterDirection.Input));
                }
                else
                {
                    cmd.Parameters.Add(new OracleParameter("nWEIGHT", OracleDbType.Int16, 0, ParameterDirection.Input));
                }
               
                cmd.Parameters.Add(new OracleParameter("cHEIGHT", OracleDbType.Varchar2, player.Height, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cOLD_PLAYER_ID", OracleDbType.Varchar2, "", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("DBIRTH_DATE", OracleDbType.Date, new DateTime(), ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cSHORT_NAME", OracleDbType.Varchar2, "", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("cMinor_id", OracleDbType.Varchar2, "", ParameterDirection.Input));

                cmd.Parameters.Add(outputParam);
                
                cmd.ExecuteNonQuery();

                playerId = Convert.ToInt32(outputParam.Value.ToString());

                if (playerId > 0)
                {
                    cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "ST_DVDB2.player_sports_pkg.proc_player_sports_insert";
                    cmd.BindByName = true;
                    
                    cmd.Parameters.Add(new OracleParameter("nPLAYER_ID", OracleDbType.Int32, playerId, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cSPORT_ID", OracleDbType.Varchar2, "FOOTBALL", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cTEAM_ID", OracleDbType.Varchar2, player.School.ID.ToString(), ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cAFFILIATE_TEAM_ID", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cJERSEY_NUMBER", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cPRIMARY_POSITION", OracleDbType.Varchar2, player.Position, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("nSCOUT_ID", OracleDbType.Int32, null, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cSWING", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cTHROW", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cFIRST_DRAFT_TEAM_ID", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("nFIRST_DRAFT_YEAR", OracleDbType.Int32, 2012, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("nFIRST_DRAFT_ROUND", OracleDbType.Int32, 0, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("nFIRST_DRAFT_OVERALL", OracleDbType.Int32, 0, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cPLAYER_STATUS", OracleDbType.Varchar2, "A", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cLEAGUE_ID", OracleDbType.Varchar2, player.School.League, ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cExperience", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cRecruits", OracleDbType.Varchar2, "", ParameterDirection.Input));
                    cmd.Parameters.Add(new OracleParameter("cDraftnote", OracleDbType.Varchar2, "", ParameterDirection.Input));

                    cmd.ExecuteNonQuery();

                    result = playerId;
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static bool DeletePlayer(Player player)
        {
            bool result = false;
            OracleConnection cn = null;
            OracleCommand cmd = null;

            try
            {
                cn = createConnectionSDR();

                string sql = "delete from draftplayers where playerid = " + player.PlayerId.ToString();
                cmd = new OracleCommand(sql, cn);

                cmd.ExecuteNonQuery();

                updateKiperRanks(player, 0);

                result = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static bool TradePick(Pick pick, Team newTeam)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataAdapter adp = null;

            MySqlConnection cnMySql = null;
            MySqlCommand cmdMySql = null;

            DataTable tbl = null;

            string sql = "";
            bool saved = false;

            int fromTeam = 0;

            try
            {
                cn = createConnectionSDR();

                sql = "select teamid from draftorder where pick = " + pick.OverallPick;
                cmd = new OracleCommand(sql, cn);
                adp = new OracleDataAdapter(cmd);

                tbl = new DataTable();

                adp.Fill(tbl);

                if (tbl.Rows.Count > 0)
                {
                    fromTeam = int.Parse(tbl.Rows[0][0].ToString());
                }

                sql = "update draftorder set teamid = " + newTeam.ID + " where pick = " + pick.OverallPick;
                cmd = new OracleCommand(sql, cn);
                cmd.ExecuteNonQuery();

                //update the MySql database's teams table with the new total pick counts
                try
                {
                    cnMySql = createConnectionMySql();

                    sql = "select count(*) from draftorder where teamid = " + fromTeam; 
                    cmd = new OracleCommand(sql, cn);
                    adp = new OracleDataAdapter(cmd);

                    tbl = new DataTable();

                    adp.Fill(tbl);

                    if (tbl.Rows.Count > 0)
                    {
                        sql = "update teams set totalpicks = '" + tbl.Rows[0][0].ToString() + "' where id = " + fromTeam;
                        cmdMySql = new MySqlCommand(sql, cnMySql);
                        cmdMySql.ExecuteNonQuery(); 
                    }

                    sql = "select count(*) from draftorder where teamid = " + newTeam.ID;
                    cmd = new OracleCommand(sql, cn);
                    adp = new OracleDataAdapter(cmd);

                    tbl = new DataTable();

                    adp.Fill(tbl);

                    if (tbl.Rows.Count > 0)
                    {
                        sql = "update teams set totalpicks = '" + tbl.Rows[0][0].ToString() + "' where id = " + newTeam.ID;
                        cmdMySql = new MySqlCommand(sql, cnMySql);
                        cmdMySql.ExecuteNonQuery();
                    }
                }
                finally
                {
                    cmd.Dispose();
                    adp.Dispose();
                    tbl.Dispose();
                }

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static void ImportPlayers(string file)
        {
            int playersImported = 0;
            int importErrors = 0;
            int playersToImport = 0;
            int playersNotFound = 0;

            SetStatusBarMsg("Importing players...", "Yellow");
                     
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
                     
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {   
                OracleConnection cn = null;
                OracleCommand cmd = null;
                OracleDataReader rdr = null;
                OracleDataAdapter adp = null;
                OracleCommandBuilder bldr = null;
                DataTable tblPlayer = null;
                DataTable tbl = null;
                DataRow row;

                //use the log when importing based on player names to find missing/duplicate players

                //FileInfo logFile = new FileInfo(ConfigurationManager.AppSettings["ImportLogFile"]);

                //if (logFile.Exists)
                //{
                //    logFile.Delete();
                //}

                //TextWriter log = new StreamWriter(ConfigurationManager.AppSettings["ImportLogFile"]);
                
                string sql;
                long playerId;                
                
                DataSet dsPlayers = new DataSet();

                try
                {
                    
                    dsPlayers.ReadXml(file);
                                
                    cn = createConnectionSDR();

                    playersToImport = dsPlayers.Tables["player"].Rows.Count;

                    foreach (DataRow xmlRow in dsPlayers.Tables["player"].Rows)
                    {
                        //sql = "select player_id from espnews.news_players where upper(last_name) = :lastname and upper(first_name) = :firstname";
                        
                        //cmd = new OracleCommand(sql, cn);
                        //cmd.Parameters.Add(":lastname", xmlRow["lastname"].ToString().ToUpper());
                        //cmd.Parameters.Add(":firstname", xmlRow["firstname"].ToString().ToUpper());

                        //rdr = cmd.ExecuteReader();

                        //tblPlayer = new DataTable();

                        //tblPlayer.Load(rdr);
                        //rdr.Close();
                        //rdr.Dispose();                       

                        //if (tblPlayer.Rows.Count > 0)
                        //{
                        //    if (tblPlayer.Rows.Count > 1)
                        //    {
                        //        //write to the report file so we can see what player's ids are questionable
                        //        log.WriteLine(DateTime.Now + " --- Found multiple records for " + xmlRow["firstname"].ToString() + " " + xmlRow["lastname"].ToString() + ".  Used PLAYERID " + tblPlayer.Rows[0]["player_id"].ToString());
                        //    }

                        //    playerId = Convert.ToInt32(tblPlayer.Rows[0]["player_id"].ToString());
                        //}
                        //else
                        //{
                        //    playerId = 0;
                        //}

                        //tblPlayer.Dispose();

                        //cmd.Dispose();

                        if (xmlRow["playerid"].ToString().Trim() != "")
                        {
                            sql = "select * from espnews.draftplayers where playerid = " + xmlRow["playerid"];
                            cmd = new OracleCommand(sql, cn);
                            adp = new OracleDataAdapter(cmd);
                            bldr = new OracleCommandBuilder(adp);

                            tbl = new DataTable();

                            adp.Fill(tbl);

                            if (tbl.Rows.Count == 0)
                            {
                                row = tbl.Rows.Add();
                                row["playerid"] = xmlRow["playerid"];
                            }
                            else
                            {
                                row = tbl.Rows[0];

                                if (row["lastname"].ToString() != xmlRow["lastname"].ToString())
                                {
                                    string message = "Duplicate player id for " + xmlRow["firstname"].ToString() + " " + xmlRow["lastname"].ToString();

                                    MessageBoxResult result = System.Windows.MessageBox.Show(message, "Duplicate Player ID", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                    
                                    //if (result == MessageBoxResult.Yes)
                                    //{
                                    //    Application.Current.Shutdown();
                                    //}
                                    
                                }
                                
                            }

                            row["firstname"] = xmlRow["firstname"].ToString();
                            row["lastname"] = xmlRow["lastname"].ToString();

                            Int16 age;

                            if (dsPlayers.Tables["players"].Columns["age"] != null)
                            {
                                if (Int16.TryParse(xmlRow["age"].ToString(), out age))
                                {
                                    row["age"] = age;
                                }
                            }

                            row["position"] = xmlRow["position"].ToString();

                            if (dsPlayers.Tables["players"].Columns["positionfull"] != null)
                            {
                                row["positionfull"] = xmlRow["positionfull"].ToString();
                            }

                            if (xmlRow["school"].ToString() != "")
                            {
                                if (Char.IsNumber(xmlRow["school"].ToString().ToCharArray()[0]))
                                {
                                    row["schoolid"] = xmlRow["school"];
                                }                                
                            }

                            if (xmlRow["kiperrank"].ToString() != "")
                            {
                                row["kiperrank"] = Convert.ToInt16(xmlRow["kiperrank"].ToString());
                            }
                            else
                            {
                                row["kiperrank"] = DBNull.Value;
                            }

                            if (xmlRow["mcshayrank"].ToString() != "")
                            {
                                row["mcshayrank"] = Convert.ToInt16(xmlRow["mcshayrank"].ToString());
                            }
                            else
                            {
                                row["mcshayrank"] = DBNull.Value;
                            }

                            row["height"] = xmlRow["height"].ToString();

                            if (xmlRow["weight"].ToString().Trim() != "")
                            {
                                row["weight"] = xmlRow["weight"].ToString() + " LBS";
                            }
                            else
                            {
                                row["weight"] = "";
                            }
                            
                            row["class"] = xmlRow["class"].ToString();

                            adp.Update(tbl.GetChanges());
                            tbl.AcceptChanges();

                            cmd.Dispose();
                            adp.Dispose();
                            bldr.Dispose();
                            tbl.Dispose();
                            
                            DataTable tblTids = null;

                            try
                            {
                                int noteCount;

                                if (ConfigurationManager.AppSettings["DraftType"].ToString().ToUpper() == "NBA")
                                {
                                    noteCount = 2;
                                }
                                else
                                {
                                    noteCount = 4;
                                }

                                for (int i = 1; i <= noteCount; i++)
                                {
                                    if (xmlRow.Table.Columns["matrixnote" + i.ToString()] != null)
                                    {
                                        if (xmlRow["matrixnote" + i.ToString()].ToString().Trim() != "")
                                        {
                                            sql = "select * from espnews.drafttidbits where referenceid = " + xmlRow["playerid"] + " and referencetype = 1 and tidbitorder = " + i;
                                            cmd = new OracleCommand(sql, cn);
                                            adp = new OracleDataAdapter(cmd);
                                            bldr = new OracleCommandBuilder(adp);

                                            tblTids = new DataTable();
                                            DataRow rowTids = null;

                                            adp.Fill(tblTids);

                                            if (tblTids.Rows.Count == 0)
                                            {
                                                rowTids = tblTids.Rows.Add();
                                                rowTids["referenceid"] = xmlRow["playerid"];
                                                rowTids["referencetype"] = 1;
                                                rowTids["tidbitorder"] = i;
                                                rowTids["enabled"] = 1;
                                            }
                                            else
                                            {
                                                rowTids = tblTids.Rows[0];
                                            }

                                            rowTids["text"] = xmlRow["matrixnote" + i.ToString()].ToString();

                                            adp.Update(tblTids.GetChanges());
                                            tblTids.AcceptChanges();

                                            cmd.Dispose();
                                            adp.Dispose();
                                            bldr.Dispose();
                                            tblTids.Dispose();
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (tblTids != null) tblTids.Dispose();
                            }

                            playersImported++;

                            worker.ReportProgress(playersImported / playersToImport);

                        } //playerid > 0  
                        else
                        {
                            playersNotFound++;
                        //    //write to report file with this player, not found
                        //    log.WriteLine(DateTime.Now + " --- Not found: " + xmlRow["firstname"].ToString() + " " + xmlRow["lastname"].ToString());
                        }
                    } //foreach player                           

                } //try
                catch (Exception ex)
                {
                    importErrors++;
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (adp != null) adp.Dispose();
                    if (bldr != null) bldr.Dispose();
                    if (rdr != null) rdr.Dispose();
                    if (cn != null) cn.Close(); cn.Dispose();
                    //log.Close();
                }

            }; //dowork
            
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            { 
                SetStatusBarMsg(playersImported.ToString() + " of " + playersToImport.ToString() + " players successfully imported at " + DateTime.Now.ToLongTimeString() + ".  Players not found by ID:  " + playersNotFound.ToString() + ".  Errors importing " + importErrors.ToString() + ".", "Green");
                GlobalCollections.Instance.LoadPlayers();
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                SetStatusBarMsg(playersImported.ToString() + " of " + playersToImport.ToString() + " players imported.", "Yellow");
            };

            worker.RunWorkerAsync(file);
        }

        public static void ImportTeams(string file)
        {
            int teamsImported = 0;
            int importErrors = 0;
            int teamsToImport = 0;

            SetStatusBarMsg("Importing teams info...", "Yellow");

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                OracleConnection cn = null;
                OracleCommand cmd = null;
                OracleDataReader rdr = null;
                OracleDataAdapter adp = null;
                OracleCommandBuilder bldr = null;

                MySqlConnection cnMySql = null;
                MySqlCommand cmdMySql = null;
                MySqlDataReader rdrMySql = null;
                MySqlDataAdapter adpMySql = null;
                MySqlCommandBuilder bldrMySql = null;

                //DataTable tblPlayer = null;
                DataTable tbl = null;
                DataRow row;

                string sql;
                //long teamId;
                int i;

                int totalPicks = 0;

                DataSet dsTeams = new DataSet();

                dsTeams.ReadXml(file);

                try
                {
                    cn = createConnectionSDR();

                    teamsToImport = dsTeams.Tables["team"].Rows.Count;

                    foreach (DataRow xmlRow in dsTeams.Tables["team"].Rows)
                    {
                        totalPicks = 0;

                        if (xmlRow["teamid"].ToString().Trim() != "")
                        {

                            if (ConfigurationManager.AppSettings["DraftType"].ToUpper() == "NFL")
                            {

                                #region Picks

                                try
                                {
                                    sql = "select count(*) from draftorder where teamid = " + xmlRow["teamid"];
                                    cmd = new OracleCommand(sql, cn);
                                    adp = new OracleDataAdapter(cmd);

                                    tbl = new DataTable();

                                    adp.Fill(tbl);

                                    if (tbl.Rows.Count > 0)
                                    {
                                        totalPicks = int.Parse(tbl.Rows[0][0].ToString());
                                    }
                                }
                                finally
                                {
                                    cmd.Dispose();
                                    adp.Dispose();
                                    tbl.Dispose();
                                }

                                #endregion

                                #region 4 Matrix Notes

                                //import the 4 matrix notes
                                for (i = 1; i <= 4; i++)
                                {
                                    if (xmlRow["note" + i.ToString()].ToString().Trim() != "")
                                    {
                                        sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = " + i.ToString() + " and referenceid = " + xmlRow["teamid"];
                                        cmd = new OracleCommand(sql, cn);
                                        adp = new OracleDataAdapter(cmd);
                                        bldr = new OracleCommandBuilder(adp);

                                        tbl = new DataTable();

                                        adp.Fill(tbl);

                                        if (tbl.Rows.Count == 0)
                                        {
                                            row = tbl.Rows.Add();
                                            row["referencetype"] = 2;
                                            row["referenceid"] = xmlRow["teamid"];
                                            row["tidbitorder"] = i;
                                            row["enabled"] = 1;
                                        }
                                        else
                                        {
                                            row = tbl.Rows[0];
                                        }

                                        row["text"] = xmlRow["note" + i.ToString()].ToString();

                                        adp.Update(tbl.GetChanges());
                                        tbl.AcceptChanges();

                                        cmd.Dispose();
                                        adp.Dispose();
                                        bldr.Dispose();
                                        tbl.Dispose();
                                    }
                                }

                                #endregion

                                #region MySql team ranks/results

                                cnMySql = createConnectionMySql();

                                sql = "select * from teams where id = " + xmlRow["teamid"];
                                cmdMySql = new MySqlCommand(sql, cnMySql);
                                adpMySql = new MySqlDataAdapter(cmdMySql);
                                bldrMySql = new MySqlCommandBuilder(adpMySql);

                                tbl = new DataTable();

                                adpMySql.Fill(tbl);

                                if (tbl.Rows.Count > 0)
                                {
                                    row = tbl.Rows[0];

                                    row["totalpicks"] = totalPicks;
                                    row["overallrecord"] = xmlRow["record"];
                                    row["divisionresult"] = xmlRow["divresult"];
                                    row["playoffs"] = xmlRow["playoffs"];
                                    row["offrankppg"] = xmlRow["offrankppg"];
                                    row["offrankypg"] = xmlRow["offrankypg"];
                                    row["offrankturns"] = xmlRow["offrankturns"];
                                    row["offrankrush"] = xmlRow["offrankrushyds"];
                                    row["offrankpass"] = xmlRow["offrankpassyds"];
                                    row["defrankppg"] = xmlRow["defrankppg"];
                                    row["defrankypg"] = xmlRow["defrankypg"];
                                    row["defranktakeaways"] = xmlRow["defranktakeaways"];
                                    row["defrankrush"] = xmlRow["defrankrushing"];
                                    row["defrankpass"] = xmlRow["defrankpassing"];
                                    row["teamneeds"] = xmlRow["melsneeds"];

                                    adpMySql.Update(tbl.GetChanges());
                                    tbl.AcceptChanges();

                                    cmdMySql.Dispose();
                                    adpMySql.Dispose();
                                    bldrMySql.Dispose();
                                    tbl.Dispose();
                                }


                                #endregion

                                //#region Off Rank Points Per Game

                                ////off. rank Points Per Game
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 10 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 10;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Points Per Game: " + xmlRow["offrankppg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Off Rank Turnovers

                                ////off. rank Turnovers
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 11 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 11;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Turnovers: " + xmlRow["offrankturns"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Off Rank Rush Yards

                                ////off. rank Rush Yards
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 12 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 12;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Rush yards: " + xmlRow["offrankrushyds"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Off Rank Pass Yards

                                ////off. rank Pass Yards
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 13 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 13;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Pass yards: " + xmlRow["offrankpassyds"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank Points Allowed

                                ////def. rank Points Allowed
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 14 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 14;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Points Allowed: " + xmlRow["defrankppg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank Takeaways

                                ////def. rank Takeaways
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 15 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 15;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Takeaways: " + xmlRow["defranktakeaways"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank Rush Defense

                                ////def. rank Rush Defense
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 16 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 16;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Rush Defense: " + xmlRow["defrankrushing"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank Pass Defense

                                ////def. rank Pass Defense
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 17 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 17;

                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = "Pass Defense: " + xmlRow["defrankpassing"].ToString();
                                //row["enabled"] = 1;

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Off Rank (PPG)

                                ////off. rank (PPG)
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 30 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 30;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["offrankppg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Off Rank (YPG)

                                ////off. rank (YPG)
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 31 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 31;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["offrankypg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank (PPG)

                                ////def. rank (PPG)
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 32 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 32;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["defrankppg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Def Rank (YPG)

                                ////def. rank (YPG)
                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 33 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 33;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["defrankypg"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Record

                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 20 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 20;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["record"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Div Result

                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 21 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 21;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["divresult"].ToString();

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Playoffs

                                //sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 22 and referenceid = " + xmlRow["teamid"];
                                //cmd = new OracleCommand(sql, cn);
                                //adp = new OracleDataAdapter(cmd);
                                //bldr = new OracleCommandBuilder(adp);

                                //tbl = new DataTable();

                                //adp.Fill(tbl);

                                //if (tbl.Rows.Count == 0)
                                //{
                                //    row = tbl.Rows.Add();
                                //    row["referencetype"] = 2;
                                //    row["referenceid"] = xmlRow["teamid"];
                                //    row["tidbitorder"] = 22;
                                //    row["enabled"] = 1;
                                //}
                                //else
                                //{
                                //    row = tbl.Rows[0];
                                //}

                                //row["text"] = xmlRow["playoffs"].ToString();
                                

                                //adp.Update(tbl.GetChanges());
                                //tbl.AcceptChanges();

                                //cmd.Dispose();
                                //adp.Dispose();
                                //bldr.Dispose();
                                //tbl.Dispose();

                                //#endregion

                                //#region Mel's Needs

                                //if (xmlRow["melsneeds"].ToString().Trim() != "")
                                //{
                                //    string[] melNeeds = xmlRow["melsneeds"].ToString().Split(',');

                                //    int tidbitOrder = 40;

                                //    for (i = 0; i < melNeeds.Length; i++)
                                //    {
                                //        sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = " + tidbitOrder + " and referenceid = " + xmlRow["teamid"];
                                //        cmd = new OracleCommand(sql, cn);
                                //        adp = new OracleDataAdapter(cmd);
                                //        bldr = new OracleCommandBuilder(adp);

                                //        tbl = new DataTable();

                                //        adp.Fill(tbl);

                                //        if (tbl.Rows.Count == 0)
                                //        {
                                //            row = tbl.Rows.Add();
                                //            row["referencetype"] = 2;
                                //            row["referenceid"] = xmlRow["teamid"];
                                //            row["tidbitorder"] = tidbitOrder;
                                //            row["enabled"] = 1;
                                //        }
                                //        else
                                //        {
                                //            row = tbl.Rows[0];
                                //        }

                                //        row["text"] = melNeeds[i].ToString().Trim();
                                        
                                //        adp.Update(tbl.GetChanges());
                                //        tbl.AcceptChanges();

                                //        cmd.Dispose();
                                //        adp.Dispose();
                                //        bldr.Dispose();
                                //        tbl.Dispose();

                                //        tidbitOrder++;
                                //    }
                                //}

                                //#endregion
                            }
                            else if (ConfigurationManager.AppSettings["DraftType"].ToUpper() == "NBA")
                            {
                                #region 2 Matrix Notes

                                //import the 4 matrix notes
                                for (i = 1; i <= 2; i++)
                                {
                                    if (xmlRow["note" + i.ToString()].ToString().Trim() != "")
                                    {
                                        sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = " + i.ToString() + " and referenceid = " + xmlRow["teamid"];
                                        cmd = new OracleCommand(sql, cn);
                                        adp = new OracleDataAdapter(cmd);
                                        bldr = new OracleCommandBuilder(adp);

                                        tbl = new DataTable();

                                        adp.Fill(tbl);

                                        if (tbl.Rows.Count == 0)
                                        {
                                            row = tbl.Rows.Add();
                                            row["referencetype"] = 2;
                                            row["referenceid"] = xmlRow["teamid"];
                                            row["tidbitorder"] = i;
                                        }
                                        else
                                        {
                                            row = tbl.Rows[0];
                                        }

                                        row["text"] = xmlRow["note" + i.ToString()].ToString();
                                        row["enabled"] = 1;

                                        adp.Update(tbl.GetChanges());
                                        tbl.AcceptChanges();

                                        cmd.Dispose();
                                        adp.Dispose();
                                        bldr.Dispose();
                                        tbl.Dispose();
                                    }
                                }

                                #endregion

                                #region Finish

                                sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 21 and referenceid = " + xmlRow["teamid"];
                                cmd = new OracleCommand(sql, cn);
                                adp = new OracleDataAdapter(cmd);
                                bldr = new OracleCommandBuilder(adp);

                                tbl = new DataTable();

                                adp.Fill(tbl);

                                if (tbl.Rows.Count == 0)
                                {
                                    row = tbl.Rows.Add();
                                    row["referencetype"] = 2;
                                    row["referenceid"] = xmlRow["teamid"];
                                    row["tidbitorder"] = 21;

                                }
                                else
                                {
                                    row = tbl.Rows[0];
                                }

                                row["text"] = xmlRow["divresult"].ToString();
                                row["enabled"] = 1;

                                adp.Update(tbl.GetChanges());
                                tbl.AcceptChanges();

                                cmd.Dispose();
                                adp.Dispose();
                                bldr.Dispose();
                                tbl.Dispose();

                                #endregion

                                #region Record

                                sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 20 and referenceid = " + xmlRow["teamid"];
                                cmd = new OracleCommand(sql, cn);
                                adp = new OracleDataAdapter(cmd);
                                bldr = new OracleCommandBuilder(adp);

                                tbl = new DataTable();

                                adp.Fill(tbl);

                                if (tbl.Rows.Count == 0)
                                {
                                    row = tbl.Rows.Add();
                                    row["referencetype"] = 2;
                                    row["referenceid"] = xmlRow["teamid"];
                                    row["tidbitorder"] = 20;

                                }
                                else
                                {
                                    row = tbl.Rows[0];
                                }

                                row["text"] = xmlRow["record"].ToString();
                                row["enabled"] = 1;

                                adp.Update(tbl.GetChanges());
                                tbl.AcceptChanges();

                                cmd.Dispose();
                                adp.Dispose();
                                bldr.Dispose();
                                tbl.Dispose();

                                #endregion

                                #region Lineup

                                sql = "select * from espnews.drafttidbits where referencetype = 2 and tidbitorder = 30 and referenceid = " + xmlRow["teamid"];
                                cmd = new OracleCommand(sql, cn);
                                adp = new OracleDataAdapter(cmd);
                                bldr = new OracleCommandBuilder(adp);

                                tbl = new DataTable();

                                adp.Fill(tbl);

                                if (tbl.Rows.Count == 0)
                                {
                                    row = tbl.Rows.Add();
                                    row["referencetype"] = 2;
                                    row["referenceid"] = xmlRow["teamid"];
                                    row["tidbitorder"] = 30;

                                }
                                else
                                {
                                    row = tbl.Rows[0];
                                }

                                row["text"] = xmlRow["lineup"].ToString();
                                row["enabled"] = 1;

                                adp.Update(tbl.GetChanges());
                                tbl.AcceptChanges();

                                cmd.Dispose();
                                adp.Dispose();
                                bldr.Dispose();
                                tbl.Dispose();

                                #endregion
                            }

                            teamsImported++;
                        }

                        worker.ReportProgress(teamsImported / teamsToImport);

                    } //foreach team                           

                } //try
                catch (Exception ex)
                {
                    importErrors++;
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                    if (adp != null) adp.Dispose();
                    if (bldr != null) bldr.Dispose();
                    if (rdr != null) rdr.Dispose();
                    if (cn != null) cn.Close(); cn.Dispose();
                    //log.Close();
                }

            }; //dowork

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " of " + teamsToImport.ToString() + " teams successfully imported at " + DateTime.Now.ToLongTimeString(), "Green");
                GlobalCollections.Instance.LoadTeams();
            };

            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                SetStatusBarMsg(teamsImported.ToString() + " of " + teamsToImport.ToString() + " teams imported.", "Yellow");
            };

            worker.RunWorkerAsync(file);
        }

        public static bool DeleteLastPick(int overallPick)
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            string sql = "";
            bool saved = false;

            try
            {
                cn = createConnectionSDR();

                sql = "update draftplayers set pick = null where pick = " + overallPick;
                cmd = new OracleCommand(sql, cn);
                cmd.ExecuteNonQuery();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static bool DeleteAllPicks()
        {
            OracleConnection cn = null;
            OracleCommand cmd = null;
            string sql = "";
            bool saved = false;

            try
            {
                cn = createConnectionSDR();

                sql = "update draftplayers set pick = null";
                cmd = new OracleCommand(sql, cn);
                cmd.ExecuteNonQuery();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        private static bool checkOraFailOverError(string sError)
        {
            if (sError.Contains("ORA-01012") || sError.Contains("ORA-01033") || sError.Contains("ORA-01034") || sError.Contains("ORA-01089") || sError.Contains("ORA-03113") || sError.Contains("ORA-01041") ||
                sError.Contains("ORA-03114") || sError.Contains("ORA-12203") || sError.Contains("ORA-12500") || sError.Contains("ORA-12537") || sError.Contains("ORA-12571") || sError.Contains("ORA-65535"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool DisablePollItems(int playlistId)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            string sql = "";
            bool saved = false;

            try
            {
                cn = createConnectionMySql();

                sql = "update playlistitems set enabled = false where playlistid = " + playlistId + " and upper(template) = 'POLL'";
                cmd = new MySqlCommand(sql, cn);
                cmd.ExecuteNonQuery();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static bool EnablePollItems(int playlistId)
        {
            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            string sql = "";
            bool saved = false;

            try
            {
                cn = createConnectionMySql();

                sql = "update playlistitems set enabled = true where playlistid = " + playlistId + " and upper(template) = 'POLL'";
                cmd = new MySqlCommand(sql, cn);
                cmd.ExecuteNonQuery();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        public static string[] GetPoll(bool results)
        {
            string[] result = null;

            MySqlConnection cn = null;
            MySqlCommand cmd = null;
            MySqlDataReader rdr = null;
            DataTable tbl = null;

            string sql = "";

            try
            {
                cn = createConnectionMySql();

                sql = "CALL sp_GetPoll(" + results + ")";
                cmd = new MySqlCommand(sql, cn);
                rdr = cmd.ExecuteReader();
                
                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                if (tbl.Rows.Count > 0)
                {
                    result = new string[2];
                    result[0] = tbl.Rows[0]["TITLE_1"].ToString();
                    result[1] = tbl.Rows[0]["TIDBIT_1"].ToString();
                }  
                
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (tbl != null) tbl.Dispose();
                if (rdr != null) rdr.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return result;
        }

        public static bool AddPlayerToDraftPlayers(Int64 playerId, string firstName, string lastName, string position, Int32 schoolId)
        {
            bool saved = false;

            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataAdapter adp = null;
            OracleCommandBuilder bldr = null;
            DataTable tbl = null;
            
            string sql = "";

            try
            {
                cn = createConnectionSDR();

                sql = "select * from draftplayers where playerid = " + playerId;
                cmd = new OracleCommand(sql, cn);

                adp = new OracleDataAdapter(cmd);
                bldr = new OracleCommandBuilder(adp);

                tbl = new DataTable();

                DataRow row;

                adp.Fill(tbl);

                if (tbl.Rows.Count == 0)
                {
                    row = tbl.Rows.Add();
                    row["playerid"] = playerId;
                }
                else
                {
                    row = tbl.Rows[0];
                }

                row["schoolid"] = schoolId;
                row["firstname"] = firstName;
                row["lastname"] = lastName;
                row["position"] = position;

                adp.Update(tbl.GetChanges());
                tbl.AcceptChanges();

                saved = true;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return saved;
        }

        private void OnSendCommandNoTransitions(PlayerCommand command)
        {
            SendCommandNoTransitionsEventHandler handler = SendCommandNoTransitionsEvent;

            if (handler != null)
            {
                handler(command);
            }
        }


    }

   
}
