﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Collections;
using System.IO;

namespace Crew_Planning
{
    public class CreateNetWork
    {
        private List<Line> lineList;
        public List<Line> LineList
        {
            get { return lineList; }
            set { lineList = value; }
        }
        private List<T_S_Node> t_S_NodeList;
        public List<T_S_Node> T_S_NodeList
        {
            get { return t_S_NodeList; }
            set { t_S_NodeList = value; }
        }
        private List<T_S_S_Node> t_S_S_NodeODTempList;
        public List<T_S_S_Node> T_S_S_NodeODTempList
        {
            get { return t_S_S_NodeODTempList; }
            set { t_S_S_NodeODTempList = value; }
        }
        private Dictionary<int,T_S_S_Node> t_S_S_NodeTempList;
        public Dictionary<int, T_S_S_Node> T_S_S_NodeTempList
        {
            get { return t_S_S_NodeTempList; }
            set { t_S_S_NodeTempList = value; }
        }
        public List<T_S_S_Node> T_S_S_NodeDelList;
        private List<T_S_S_Node> t_S_S_NodeList;
        public List<T_S_S_Node> T_S_S_NodeList
        {
            get { return t_S_S_NodeList; }
            set { t_S_S_NodeList = value; }
        }
        private List<T_S_Arc> t_S_ArcList;
        public List<T_S_Arc> T_S_ArcList
        {
            get { return t_S_ArcList; }
            set { t_S_ArcList = value; }
        }
        private List<CrewBase> crewBaseList;
        public List<CrewBase> CrewBaseList
        {
            get { return crewBaseList; }
            set { crewBaseList = value; }
        }
        private List<Crew> crewList;
        public List<Crew> CrewList
        {
            get { return crewList; }
            set { crewList = value; }
        }
        private List<Station> stationList;
        public List<Station> StationList
        {
            get { return stationList; }
            set { stationList = value; }
        }
        private LinkedList<T_S_S_Node> queueList;//广度优先遍历所需的队列
        public LinkedList<T_S_S_Node> QueueList
        {
            get { return queueList; }
            set { queueList = value; }
        }
        //public ArrayList QueueList;
        public T_S_S_Node t_s_s_nodequeue;
        private List<T_S_S_Arc> t_S_S_ArcList;
        public List<T_S_S_Arc> T_S_S_ArcList
        {
            get { return t_S_S_ArcList; }
            set { t_S_S_ArcList = value; }
        }

        public string path = null;
        public string strConn = null;
        public string sql = null;
        public OleDbConnection OleConn = null;
        public OleDbCommand OleComm = null;
        public OleDbDataAdapter OleDataExcel = null;
        public OleDbCommandBuilder OleBuilder = null;

        public DataSet Ds = null;
        public DataTable Dt = null;
        public DataRow row = null;

        public int STLunch;
        public int ETLunch;
        public int STDinner;
        public int ETDinner;

        public int minMealTime;
        public int maxMealTime;

        public int virtualRoutingCost = 0;

        #region 数据库来取数据，因为读取Excel出错了，目标平台设为32位才行，但是CPLEX是64位的，所以必须设为64位平台
        public SqlConnection sqlConn = null;
        public SqlCommand sqlComm = null;
        public SqlDataAdapter sqlDataAdapter = null;
        public SqlCommandBuilder sqlCommBuilder = null;
        #endregion
        

        public DataSet ConnDataBase()
        {
            //HDR=Yes，表示数据中有标题，从第二行开始读取数据
            //path = System.Environment.CurrentDirectory + "\\郑州城际算例.xlsx";//若以对话框的形式选取文件，可在这里赋值
            //path = System.Environment.CurrentDirectory + "\\铁道学报.xlsx";//若以对话框的形式选取文件，可在这里赋值
            //path = System.Environment.CurrentDirectory + "\\输入.xlsx";
            path = System.Environment.CurrentDirectory + "京津城际原始数据.xlsx";
            //Extended Properties='Excel 8.0，97以上版本全为8.0；HDR=Yes代表第一行是标题，不作为数据处理；IMEX=2，0——只可写入，1——只可读取，2——可写可读；
            strConn = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Persist Security Info = False;Data Source={0};Extended Properties='Excel 12.0;HDR=Yes;IMEX=2'", path);
            OleConn = new OleDbConnection(strConn);
            OleConn.Open();

            Ds = new DataSet("CrewPlaningDataSet");
            Ds.Tables.Add("Timetable");
            Ds.Tables.Add("CrewBase");
            Ds.Tables.Add("Crew");
            Ds.Tables.Add("Show");

            sql = "SELECT * FROM  [Timetable$]";
            OleDataExcel = new OleDbDataAdapter(sql, OleConn);
            OleBuilder = new OleDbCommandBuilder(OleDataExcel); //使DatSet与Excel的更改相协调，只需创建，无需操作 
            OleDataExcel.Fill(Ds, "Timetable");

            sql = "SELECT * FROM  [CrewBase$]";
            OleDataExcel = new OleDbDataAdapter(sql, OleConn);
            OleBuilder = new OleDbCommandBuilder(OleDataExcel); //使DatSet与Excel的更改相协调，只需创建，无需操作 
            OleDataExcel.Fill(Ds, "CrewBase");

            sql = "SELECT * FROM  [Crew$]";
            OleDataExcel = new OleDbDataAdapter(sql, OleConn);
            OleBuilder = new OleDbCommandBuilder(OleDataExcel); //使DatSet与Excel的更改相协调，只需创建，无需操作 
            OleDataExcel.Fill(Ds, "Crew");


            sql = "SELECT * FROM  [Station$]";
            OleDataExcel = new OleDbDataAdapter(sql, OleConn);
            OleBuilder = new OleDbCommandBuilder(OleDataExcel); //使DatSet与Excel的更改相协调，只需创建，无需操作 
            OleDataExcel.Fill(Ds, "Station");
            OleConn.Close();
            return Ds;
        }

        public DataSet ConnSql() 
        {
            path = "Server = PC-201606172102\\SQLExpress;DataBase = 沪杭;Integrated Security = true";
            //path = "Server = PC-201606172102\\SQLExpress;DataBase = 京津算例;Integrated Security = true";
            sqlConn = new SqlConnection(path);
            sqlConn.Open();

            Ds = new DataSet("CrewPlaningDataSet");
            Ds.Tables.Add("Timetable");
            Ds.Tables.Add("CrewBase");
            Ds.Tables.Add("Crew");
            Ds.Tables.Add("Show");

            sql = "SELECT * FROM  [Timetable$]";
            sqlDataAdapter = new SqlDataAdapter(sql, sqlConn);
            sqlCommBuilder = new SqlCommandBuilder(sqlDataAdapter); //使DatSet与sql的更改相协调，只需创建，无需操作
            sqlDataAdapter.Fill(Ds, "Timetable");            

            sql = "SELECT * FROM  [CrewBase$]";
            sqlDataAdapter = new SqlDataAdapter(sql, sqlConn);
            sqlCommBuilder = new SqlCommandBuilder(sqlDataAdapter); //使DatSet与sql的更改相协调，只需创建，无需操作
            sqlDataAdapter.Fill(Ds, "CrewBase");

            sql = "SELECT * FROM  [Crew$]";
            sqlDataAdapter = new SqlDataAdapter(sql, sqlConn);
            sqlCommBuilder = new SqlCommandBuilder(sqlDataAdapter); //使DatSet与sql的更改相协调，只需创建，无需操作
            sqlDataAdapter.Fill(Ds, "Crew");


            sql = "SELECT * FROM  [Station$]";
            sqlDataAdapter = new SqlDataAdapter(sql, sqlConn);
            sqlCommBuilder = new SqlCommandBuilder(sqlDataAdapter); //使DatSet与sql的更改相协调，只需创建，无需操作
            sqlDataAdapter.Fill(Ds, "Station");

            sqlConn.Close();

            return Ds;
        }

        public void LoadData(DataSet Ds ,int Days, int numTrain, int numCrew)
        {
            int i = 0, j = 0;
            Dictionary<string, bool> StationMap = new Dictionary<string, bool>();
            Dt = Ds.Tables["Timetable"];
            LineList = new List<Line>();
            T_S_NodeList = new List<T_S_Node>();
            for (i = 0; i < numTrain; i++)//Dt.Rows.Count
            {
                Line line = new Line();
                line.ID = Convert.ToInt32(Dt.Rows[i]["编号"]);
                line.LineID = Convert.ToInt32(Dt.Rows[i]["编号"]);
                line.TrainCode = Convert.ToString(Dt.Rows[i]["车次"]);
                line.RountingID = Convert.ToInt32(Dt.Rows[i]["交路编号"]);
                line.DepStaCode = Convert.ToString(Dt.Rows[i]["出发车站"]);
                line.DepTimeCode = Convert.ToInt32(Dt.Rows[i]["出发时刻"]);
                line.ArrStaCode = Convert.ToString(Dt.Rows[i]["到达车站"]);
                line.ArrTimeCode = Convert.ToInt32(Dt.Rows[i]["到达时刻"]);
                line.LagMultiplier = 0.0;
                line.NumSelected = 0;
                LineList.Add(line);

                //add station
                string depSta = line.DepStaCode;
                if (!StationMap.ContainsKey(depSta))
                {
                    StationMap[depSta] = true;
                }
                string arrSta = line.ArrStaCode;
                if (!StationMap.ContainsKey(arrSta))
                {
                    StationMap[arrSta] = true;
                }

                for (j = 0; j < Days; j++)
                {
                    T_S_Node t_s_note1 = new T_S_Node();
                    t_s_note1.ID = (Convert.ToInt32(Dt.Rows[i]["编号"])*2-1) + 2*Dt.Rows.Count*j;
                    t_s_note1.LineID = Convert.ToInt32(Dt.Rows[i]["编号"]);
                    t_s_note1.RountingID = Convert.ToInt32(Dt.Rows[i]["交路编号"]);
                    t_s_note1.TrainCode = Convert.ToString(Dt.Rows[i]["车次"]);
                    t_s_note1.StaCode = Convert.ToString(Dt.Rows[i]["出发车站"]);
                    t_s_note1.TimeCode = Convert.ToInt32(Dt.Rows[i]["出发时刻"])+1440*j;
                    t_s_note1.PointType = 1;//1——始发点，2——终到点
                    T_S_NodeList.Add(t_s_note1);

                    T_S_Node t_s_note2 = new T_S_Node();
                    t_s_note2.ID = (Convert.ToInt32(Dt.Rows[i]["编号"])*2) + 2*Dt.Rows.Count * j;
                    t_s_note2.LineID = Convert.ToInt32(Dt.Rows[i]["编号"]);
                    t_s_note2.RountingID = Convert.ToInt32(Dt.Rows[i]["交路编号"]);
                    t_s_note2.TrainCode = Convert.ToString(Dt.Rows[i]["车次"]);
                    t_s_note2.StaCode = Convert.ToString(Dt.Rows[i]["到达车站"]);
                    t_s_note2.TimeCode = Convert.ToInt32(Dt.Rows[i]["到达时刻"])+1440*j;//加最小换乘时间
                    t_s_note2.PointType = 2;//1——始发点，2——终到点
                    T_S_NodeList.Add(t_s_note2);
                }
            }

            Dt = Ds.Tables["CrewBase"];
            CrewBaseList = new List<CrewBase>();
            for (i = 0; i < Dt.Rows.Count; i++)//Dt.Rows.Count
            {
                #region 录入虚拟起终点
                T_S_Node t_s_note1 = new T_S_Node();
                t_s_note1.ID = T_S_NodeList.Count + 1;
                t_s_note1.LineID = 0;
                t_s_note1.RountingID = 0;
                t_s_note1.TrainCode = "";
                t_s_note1.StaCode = Convert.ToString(Dt.Rows[i]["乘务基地"]);
                t_s_note1.TimeCode = 0;
                t_s_note1.PointType = 3;//1——始发点，2——终到点，3——虚拟起点
                T_S_NodeList.Add(t_s_note1);

                T_S_Node t_s_note2 = new T_S_Node();
                t_s_note2.ID = T_S_NodeList.Count + 1;
                t_s_note2.LineID = 0;
                t_s_note2.RountingID = 0;
                t_s_note2.TrainCode = "";
                t_s_note2.StaCode = Convert.ToString(Dt.Rows[i]["乘务基地"]);
                t_s_note2.TimeCode = 0;
                t_s_note2.PointType = 4;//1——始发点，2——终到点，4——虚拟终点
                T_S_NodeList.Add(t_s_note2);
                #endregion

                CrewBase crewbase = new CrewBase();
                crewbase.ID = Convert.ToInt32(Dt.Rows[i]["编号"]);
                crewbase.StaCode = Convert.ToString(Dt.Rows[i]["乘务基地"]);
                crewbase.PhyCrewCapacity = Convert.ToInt32(Dt.Rows[i]["实设乘务组数"]);
                crewbase.VirCrewCapacity = Convert.ToInt32(Dt.Rows[i]["虚设乘务组数"]);
                crewbase.VirStartPointID = T_S_NodeList.Count - 1;
                crewbase.VirEndPointID = T_S_NodeList.Count;
                //crewbase.VirStartPointID = Convert.ToInt32(Dt.Rows[i]["时空网起点标号"]);
                //crewbase.VirEndPointID = Convert.ToInt32(Dt.Rows[i]["时空网终点标号"]);
                crewbase.OIDinTSS = Convert.ToInt32(Dt.Rows[i]["时空状态网起点标号"]);
                crewbase.DIDinTSS = Convert.ToInt32(Dt.Rows[i]["时空状态网终点标号"]);
                CrewBaseList.Add(crewbase);

            }
            Dt = Ds.Tables["Crew"];
            CrewList = new List<Crew>();
            for (i = 0; i < numCrew; i++)//Dt.Rows.Count
            {
                Crew crew = new Crew();
                crew.ID = Convert.ToInt32(Dt.Rows[i]["乘务组代号"]);
                crew.OutCost = Convert.ToDouble(Dt.Rows[i]["出动成本"]);
                crew.RemainCost = Convert.ToDouble(Dt.Rows[i]["停驻成本"]);
                CrewList.Add(crew);
            }

            Dt = Ds.Tables["Station"];
            StationList = new List<Station>();
            //for (i = 0; i < Dt.Rows.Count; i++)
            //{
            //    Station station = new Station();
            //    station.ID = Convert.ToInt32(Dt.Rows[i]["编号"]);
            //    station.StaCode = Convert.ToString(Dt.Rows[i]["车站"]);
            //    station.ReMainCrew = Convert.ToInt32(Dt.Rows[i]["是否允许外段驻班"]);
            //    StationList.Add(station);
            //}
            int id = 1;
            foreach (var sta in StationMap)
            {
                Station station = new Station();
                station.ID = id;
                station.StaCode = sta.Key;
                station.ReMainCrew = 0;
                StationList.Add(station);
                id++;
            }
        }
        public void LoadData_csv(int Days,
            string timetableFile,
            string crewbaseFile,
            string crewFile,
            string stationFile)
        {
            //string f1 = System.Environment.CurrentDirectory + "\\DATA\\沪杭\\Timetable.csv";
            //string f2 = System.Environment.CurrentDirectory + "\\DATA\\沪杭\\CrewBase.csv";
            //string f3 = System.Environment.CurrentDirectory + "\\DATA\\沪杭\\Crew.csv";
            //string f4 = System.Environment.CurrentDirectory + "\\DATA\\沪杭\\Station.csv";
            string f1 = timetableFile;
            string f2 = crewbaseFile;
            string f3 = crewFile;
            string f4 = stationFile;

            int j;
            string[] str = new string[7];
            StreamReader csv = new StreamReader(f1,Encoding.Default);
            string row = csv.ReadLine();
            row = csv.ReadLine();
            //Dictionary<string, bool> StationMap = new Dictionary<string, bool>();
            LineList = new List<Line>();
            T_S_NodeList = new List<T_S_Node>();

            while (row != null)
            {
                str = row.Split(',');
                Line line = new Line();

                line.ID = Convert.ToInt32(str[0]);
                line.LineID = Convert.ToInt32(str[0]);
                line.TrainCode = Convert.ToString(str[1]);
                line.RountingID = Convert.ToInt32(str[6]);
                line.DepStaCode = Convert.ToString(str[2]);
                line.DepTimeCode = Convert.ToInt32(str[4]);
                line.ArrStaCode = Convert.ToString(str[3]);
                line.ArrTimeCode = Convert.ToInt32(str[5]);
                line.LagMultiplier = 0.0;
                line.NumSelected = 0;
                LineList.Add(line);
                //station
                //string depSta = line.DepStaCode;
                //if (!StationMap.ContainsKey(depSta))
                //{
                //    StationMap[depSta] = true;
                //}
                //string arrSta = line.ArrStaCode;
                //if (!StationMap.ContainsKey(arrSta))
                //{
                //    StationMap[arrSta] = true;
                //}

                for (j = 0; j < Days; j++)
                {
                    T_S_Node t_s_note1 = new T_S_Node();
                    t_s_note1.ID = (Convert.ToInt32(str[0]) * 2 - 1);//+ 2 * Dt.Rows.Count * j;
                    t_s_note1.LineID = Convert.ToInt32(str[0]);
                    t_s_note1.RountingID = Convert.ToInt32(str[6]);
                    t_s_note1.TrainCode = Convert.ToString(str[1]);
                    t_s_note1.StaCode = Convert.ToString(str[2]);
                    t_s_note1.TimeCode = Convert.ToInt32(str[4]) + 1440 * j;
                    t_s_note1.PointType = 1;//1——始发点，2——终到点
                    T_S_NodeList.Add(t_s_note1);

                    T_S_Node t_s_note2 = new T_S_Node();
                    t_s_note2.ID = (Convert.ToInt32(str[0]) * 2);//+ 2 * Dt.Rows.Count * j;
                    t_s_note2.LineID = Convert.ToInt32(str[0]);
                    t_s_note2.RountingID = Convert.ToInt32(str[6]);
                    t_s_note2.TrainCode = Convert.ToString(str[1]);
                    t_s_note2.StaCode = Convert.ToString(str[3]);
                    t_s_note2.TimeCode = Convert.ToInt32(str[5]) + 1440 * j;//加最小换乘时间
                    t_s_note2.PointType = 2;//1——始发点，2——终到点
                    T_S_NodeList.Add(t_s_note2);

                    row = csv.ReadLine();

                }

            }
            csv.Close();

            //CrewBase            
            CrewBaseList = new List<CrewBase>();

            csv = new StreamReader(f2, Encoding.Default);
            row = csv.ReadLine();
            row = csv.ReadLine();
            while (row != null)
            {
                str = row.Split(',');

                #region 录入虚拟起终点
                T_S_Node t_s_note1 = new T_S_Node();
                t_s_note1.ID = T_S_NodeList.Count + 1;
                t_s_note1.LineID = 0;
                t_s_note1.RountingID = 0;
                t_s_note1.TrainCode = "";
                t_s_note1.StaCode = Convert.ToString(str[1]);
                t_s_note1.TimeCode = 0;
                t_s_note1.PointType = 3;//1——始发点，2——终到点，3——虚拟起点
                T_S_NodeList.Add(t_s_note1);

                T_S_Node t_s_note2 = new T_S_Node();
                t_s_note2.ID = T_S_NodeList.Count + 1;
                t_s_note2.LineID = 0;
                t_s_note2.RountingID = 0;
                t_s_note2.TrainCode = "";
                t_s_note2.StaCode = Convert.ToString(str[1]);
                t_s_note2.TimeCode = 0;
                t_s_note2.PointType = 4;//1——始发点，2——终到点，4——虚拟终点
                T_S_NodeList.Add(t_s_note2);
                #endregion

                CrewBase crewbase = new CrewBase();
                crewbase.ID = Convert.ToInt32(str[0]);
                crewbase.StaCode = Convert.ToString(str[1]);
                crewbase.PhyCrewCapacity = Convert.ToInt32(str[2]);
                crewbase.VirCrewCapacity = Convert.ToInt32(str[3]);
                crewbase.VirStartPointID = T_S_NodeList.Count - 1;
                crewbase.VirEndPointID = T_S_NodeList.Count;
                //crewbase.VirStartPointID = Convert.ToInt32(Dt.Rows[i]["时空网起点标号"]);
                //crewbase.VirEndPointID = Convert.ToInt32(Dt.Rows[i]["时空网终点标号"]);
                crewbase.OIDinTSS = Convert.ToInt32(str[4]);
                crewbase.DIDinTSS = Convert.ToInt32(str[5]);
                CrewBaseList.Add(crewbase);

                row = csv.ReadLine();
            }
            csv.Close();

            //Crew
            csv = new StreamReader(f3, Encoding.Default);
            row = csv.ReadLine();

            CrewList = new List<Crew>();
            while (!csv.EndOfStream) {
                row = csv.ReadLine();
                str = row.Split(',');

                Crew crew = new Crew();
                crew.ID = Convert.ToInt32(str[0]);
                crew.OutCost = Convert.ToDouble(str[1]);
                crew.RemainCost = Convert.ToDouble(str[2]);
                CrewList.Add(crew);
            }


            //station            
            StationList = new List<Station>();
            csv = new StreamReader(f4, Encoding.Default);
            row = csv.ReadLine();
            row = csv.ReadLine();
            while (row!=null)
            {                
                str = row.Split(',');
                Station station = new Station();

                station.ID = Convert.ToInt32(str[0]);
                station.StaCode = str[1];
                station.ReMainCrew = Convert.ToInt32(str[2]);
                StationList.Add(station);
                row = csv.ReadLine();
            }

            //int id = 1;
            //foreach (var sta in StationMap)
            //{
            //    Station station = new Station();
            //    station.ID = id;
            //    station.StaCode = sta.Key;
            //    station.ReMainCrew = 0;
            //    StationList.Add(station);
            //    id++;
            //}

        }

        
        /// <summary>
        /// 时空网络的创建,，无需检查，已调试完成
        /// </summary>
        public void CreateT_S_NetWork()
        {
            int i = 0, j = 0, k = 1;
            T_S_ArcList = new List<T_S_Arc>();
            #region 创建运行弧
            for (i = 0; i < T_S_NodeList.Count; i++)
            {
                for (j = 0; j < T_S_NodeList.Count; j++)
                {
                    //构建列车弧
                    if (T_S_NodeList[i].LineID == T_S_NodeList[j].LineID 
                        && T_S_NodeList[i].PointType == 1 && T_S_NodeList[j].PointType == 2 
                        && ((T_S_NodeList[i].TimeCode >= 1440 && T_S_NodeList[j].TimeCode >= 1440) 
                            || (T_S_NodeList[i].TimeCode < 1440 && T_S_NodeList[j].TimeCode < 1440)))
                    {
                        T_S_Arc t_s_arc = new T_S_Arc();
                        t_s_arc.ID = k;
                        t_s_arc.LineID = T_S_NodeList[i].LineID;
                        t_s_arc.RountingID = T_S_NodeList[i].RountingID;
                        t_s_arc.TrainCode = T_S_NodeList[i].TrainCode;
                        t_s_arc.StartPointID = T_S_NodeList[i].ID;
                        t_s_arc.EndPointID = T_S_NodeList[j].ID;                        
                        t_s_arc.StartStaCode = T_S_NodeList[i].StaCode;
                        t_s_arc.EndStaCode = T_S_NodeList[j].StaCode;
                        t_s_arc.StartTimeCode = T_S_NodeList[i].TimeCode;
                        t_s_arc.EndTimeCode = T_S_NodeList[j].TimeCode;
                        t_s_arc.ArcType = 1;//1——运行弧
                        T_S_ArcList.Add(t_s_arc);
                        k++;

                        t_s_arc.StartPoint = T_S_NodeList[i];
                        t_s_arc.EndPoint = T_S_NodeList[j];
                        t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);
                    }
                }
            }
            #endregion
            #region 创建日间连接弧和夜间连接弧
            List<T_S_Node> StationSortList = new List<T_S_Node>();
            T_S_Node tempnote = new T_S_Node();
            foreach (Station station in StationList)
            {
                StationSortList.Clear();
                for (i = 0; i < T_S_NodeList.Count; i++)
                {
                    if (station.StaCode == T_S_NodeList[i].StaCode && T_S_NodeList[i].PointType != 3 && T_S_NodeList[i].PointType != 4)
                    {
                        StationSortList.Add(T_S_NodeList[i]);
                    }
                }
                //冒泡排序,建立连接弧
                for (i = 0; i < StationSortList.Count; i++)
                {
                    for (j = 0; j < StationSortList.Count - 1 - i; j++)
                    {
                        if (StationSortList[j].TimeCode > StationSortList[j + 1].TimeCode)
                        {
                            tempnote = StationSortList[j];
                            StationSortList[j] = StationSortList[j + 1];
                            StationSortList[j + 1] = tempnote;
                        }
                    }
                }

                for (i = 0; i < StationSortList.Count - 1; i++)
                {
                    if (StationSortList[i].TimeCode <= 1440 && StationSortList[i + 1].TimeCode > 1440 && station.ReMainCrew==1)
                    {
                        T_S_Arc t_s_arc = new T_S_Arc();
                        t_s_arc.ID = k;
                        t_s_arc.LineID = 0;
                        t_s_arc.RountingID = 0;
                        t_s_arc.TrainCode = "";
                        t_s_arc.StartPointID = StationSortList[i].ID;
                        t_s_arc.EndPointID = StationSortList[i + 1].ID;
                        t_s_arc.StartStaCode = StationSortList[i].StaCode;
                        t_s_arc.EndStaCode = StationSortList[i + 1].StaCode;
                        t_s_arc.StartTimeCode = StationSortList[i].TimeCode;
                        t_s_arc.EndTimeCode = StationSortList[i + 1].TimeCode;
                        t_s_arc.ArcType = 22;//22——夜间连接弧
                        T_S_ArcList.Add(t_s_arc);
                        k++;

                        t_s_arc.StartPoint = StationSortList[i];
                        t_s_arc.EndPoint = StationSortList[i+1];
                        t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);

                    }
                    else if ((StationSortList[i].TimeCode < 1440 && StationSortList[i + 1].TimeCode < 1440) || (StationSortList[i].TimeCode >= 1440 && StationSortList[i + 1].TimeCode >= 1440))
                    {
                        T_S_Arc t_s_arc = new T_S_Arc();
                        t_s_arc.ID = k;
                        t_s_arc.LineID = 0;
                        t_s_arc.RountingID = 0;
                        t_s_arc.TrainCode = "";
                        t_s_arc.StartPointID = StationSortList[i].ID;
                        t_s_arc.EndPointID = StationSortList[i + 1].ID;
                        t_s_arc.StartStaCode = StationSortList[i].StaCode;
                        t_s_arc.EndStaCode = StationSortList[i + 1].StaCode;
                        t_s_arc.StartTimeCode = StationSortList[i].TimeCode;
                        t_s_arc.EndTimeCode = StationSortList[i + 1].TimeCode;
                        t_s_arc.ArcType = 21;//21——日间连接弧
                        T_S_ArcList.Add(t_s_arc);
                        k++;

                        t_s_arc.StartPoint = StationSortList[i];
                        t_s_arc.EndPoint = StationSortList[i+1];
                        t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);
                    }
               
                }
            }
            #endregion
            #region 创建出退乘弧
            foreach (CrewBase crewbase in CrewBaseList)
            {
                i = 0;
                for (i = 0; i < T_S_NodeList.Count; i++)
                {
                    //可以建立发车时间窗
                    if (T_S_NodeList[i].StaCode == crewbase.StaCode 
                        && T_S_NodeList[i].PointType == 1
                        && T_S_NodeList[i].TimeCode<1440)
                    {
                        T_S_Arc t_s_arc = new T_S_Arc();
                        t_s_arc.ID = k;
                        t_s_arc.RountingID = T_S_NodeList[i].RountingID;
                        t_s_arc.LineID = 0;
                        t_s_arc.TrainCode = T_S_NodeList[i].TrainCode;
                        t_s_arc.StartPointID = crewbase.VirStartPointID;
                        t_s_arc.EndPointID = T_S_NodeList[i].ID;
                        t_s_arc.StartStaCode = T_S_NodeList[i].StaCode;
                        t_s_arc.EndStaCode = T_S_NodeList[i].StaCode;
                        t_s_arc.StartTimeCode = 0;
                        t_s_arc.EndTimeCode = T_S_NodeList[i].TimeCode;
                        t_s_arc.ArcType = 31;//31——出乘弧
                        T_S_ArcList.Add(t_s_arc);                       
                        k++;

                        t_s_arc.StartPoint = T_S_NodeList[crewbase.VirStartPointID-1];
                        t_s_arc.EndPoint = T_S_NodeList[i];
                        t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);
                    }
                    if (T_S_NodeList[i].StaCode == crewbase.StaCode 
                        && T_S_NodeList[i].PointType == 2)
                    {
                        T_S_Arc t_s_arc = new T_S_Arc();
                        t_s_arc.ID = k;
                        t_s_arc.RountingID = 0;
                        t_s_arc.LineID = 0;
                        t_s_arc.TrainCode = T_S_NodeList[i].TrainCode;
                        t_s_arc.StartPointID = T_S_NodeList[i].ID;
                        t_s_arc.EndPointID = crewbase.VirEndPointID;
                        t_s_arc.StartStaCode = T_S_NodeList[i].StaCode;
                        t_s_arc.EndStaCode = T_S_NodeList[i].StaCode;
                        t_s_arc.StartTimeCode = T_S_NodeList[i].TimeCode;
                        if (t_s_arc.StartTimeCode<=1440)
                        {
                            t_s_arc.EndTimeCode = 1440;
                        }
                        else
                        {
                            t_s_arc.EndTimeCode = 2880;
                        }
                        
                        t_s_arc.ArcType = 32;//32——退乘弧
                        T_S_ArcList.Add(t_s_arc);
                        k++;

                        t_s_arc.StartPoint = T_S_NodeList[i];
                        t_s_arc.EndPoint = T_S_NodeList[crewbase.VirEndPointID-1];
                        t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);
                    }
                }
  
            }
            foreach (CrewBase crewbase in CrewBaseList)
            {
                T_S_Arc t_s_arc = new T_S_Arc();
                t_s_arc.ID = k;
                t_s_arc.RountingID = 0;
                t_s_arc.LineID = 0;
                t_s_arc.TrainCode = "";
                t_s_arc.StartPointID = crewbase.VirStartPointID;
                t_s_arc.EndPointID = crewbase.VirEndPointID;
                t_s_arc.StartStaCode = crewbase .StaCode;
                t_s_arc.EndStaCode = crewbase.StaCode;
                t_s_arc.StartTimeCode = 0;
                t_s_arc.EndTimeCode = 1440;//若是两天会有些变化
                t_s_arc.ArcType = 33;//33——虚拟乘务组出退乘弧
                T_S_ArcList.Add(t_s_arc);
                k++;

                t_s_arc.StartPoint = T_S_NodeList[crewbase.VirStartPointID-1];
                t_s_arc.EndPoint = T_S_NodeList[crewbase.VirEndPointID - 1];
                t_s_arc.StartPoint.Out_T_S_ArcList.Add(t_s_arc);
            }
            #endregion
        }

        public void SetMealWindows(int stLunch, int etLunch, int stDinner, int etDinner)
        {
            this.STLunch = stLunch;
            this.ETLunch = etLunch;
            this.STDinner = stDinner;
            this.ETDinner = etDinner;
        }
        public void SetMealTime(int minMealTime_, int maxMealTime_) {
            minMealTime = minMealTime_;
            maxMealTime = maxMealTime_;
        }
        public void SetVirRoutingCost(int virRoutingCost) {
            virtualRoutingCost = virRoutingCost;
        }

        /// <summary>
        /// 时空状态网络的创建
        /// </summary>
        public List<T_S_S_Arc> CreateT_S_S_NetWork(int mindrivetime,int maxdrivetime,int maxconntime,int mintranslation,
                                                   int minrelaxtime,int maxrelaxtime,int minoutrelaxtime,int maxoutrelaxtime,
                                                   int mindaycrewtime,int maxdaycrewtime)
        {
            //给虚拟起点定义初始状态，由虚拟起点按时间轴前进方向广度优先遍历各点，标出各点状态
            T_S_S_NodeTempList = new Dictionary<int, T_S_S_Node>();
            T_S_S_NodeODTempList = new List<T_S_S_Node>();
            T_S_S_ArcList = new List<T_S_S_Arc>();
            QueueList = new LinkedList<T_S_S_Node>();
            //QueueList = new ArrayList();
            t_s_s_nodequeue = new T_S_S_Node();

            //int STLunch=660,ETLunch=780,STDinner=1020,ETDinner=1140; //2h windows
            //int STLunch = 630, ETLunch = 810, STDinner = 990, ETDinner = 1170; //3h windows
            //int STLunch = 600, ETLunch = 840, STDinner = 960, ETDinner = 1200; //4h windows    

            //int minmealtime = 0, maxmealtime = 0;
            //int STLunch = 4000, ETLunch = 8400, STDinner = 9600, ETDinner = 14400; //不考虑 windows

            int minmealtime = minMealTime;//30;
            int maxmealtime = maxMealTime;//40;

            //added 4-27 说明是不考虑时间窗的
            if (STLunch > 3000) 
            {
                minmealtime = 0;
                maxmealtime = 0;
            }

            int windowofO = 0;
            int k1 = CrewBaseList.Count * 2, i = 0,l=0;//k1的编号从乘务基地虚拟起终点之后的编号起
            int isdrive=1;
            bool isconn = false, isdaycrew=false, iscreate=false ;//3个状态属性，判断其是否满足此状态
            int QueueCount = 0;            
            foreach (T_S_Node t_s_node in T_S_NodeList)
            {
                //给虚拟起点定义初始时空状态坐标
                if (t_s_node.PointType == 3 )
                {
                    T_S_S_Node t_s_s_node = new T_S_S_Node();
                    foreach(CrewBase crewbase in CrewBaseList)
                    {
                        if (t_s_node.StaCode == crewbase.StaCode )
                        {
                            t_s_s_node.ID = crewbase.OIDinTSS;//将起点和终点放在编号前列，便于连续编号
                        }
                    }
                    t_s_s_node.PrevID = -1;
                    t_s_s_node.LineID = -1;//虚拟起点,-1——该点为原点，无线相连；0——该店与前继节点连成连接弧
                    t_s_s_node.RountingID = 0;
                    t_s_s_node.TrainCode = t_s_node.TrainCode;
                    t_s_s_node.StaCode = t_s_node.StaCode;
                    t_s_s_node.TimeCode = t_s_node.TimeCode;
                    t_s_s_node.PrevSuperID = -1;
                    t_s_s_node.SuperPointID = t_s_node.ID;
                    t_s_s_node.PointType = 0;//虚拟起点
                    t_s_s_node.OStation = t_s_s_node.StaCode;

                    t_s_s_node.CumuConnTime = 0;//起点初始化，均为0
                    t_s_s_node.CumuLunch =0;
                    t_s_s_node.CumuDinner=0;
                    t_s_s_node.CumuDriveTime = 0;
                    t_s_s_node.CumuDayCrewTime = 0;

                    t_s_s_node.Price=0;
                    t_s_s_node.Relax = 0;

                    t_s_s_node.PervCumuConnTime = -1;
                    t_s_s_node.PervCumuDriveTime = -1;
                    t_s_s_node.PervCumuDayCrewTime = -1;
                    t_s_s_node.PervCumuLunch = -1;
                    t_s_s_node.PervCumuDinner = -1;

                    t_s_s_node.PervRelax = -1;
                    t_s_s_node.PerPointType = -1;

                    T_S_S_NodeTempList.Add(t_s_s_node.ID,t_s_s_node);

                    QueueList.AddLast(t_s_s_node);

                    t_s_s_node.SuperPoint = T_S_NodeList[t_s_s_node.SuperPointID];
                }
            }
            #region 广度优先遍历各点
            do
            {
                t_s_s_nodequeue = QueueList.First<T_S_S_Node>();
                QueueCount = QueueList.Count;
                for (i = 0; i < T_S_ArcList.Count; i++)
                {
                    if (T_S_ArcList[i].StartPointID == t_s_s_nodequeue.SuperPointID)
                    {
                        // debug 20191020
                        //var arc = T_S_ArcList[i];
                        //if (t_s_s_nodequeue.ID == 5285
                        //    //arc.ArcType == 1 && arc.StartTimeCode == 767
                        //    ) {
                        //    int debug = 0;
                        //}
                        //if (t_s_s_nodequeue.TimeCode == 808 && t_s_s_nodequeue.PassLine.Contains(136) &&
                        //     arc.ArcType == 32) {
                        //    Console.WriteLine("t_s_s_nodequeue.Sta: {0}, t_s_s_nodequeue.Time:{1}, t_s_s_nodequeue.Lunch:{2}", 
                        //        t_s_s_nodequeue.StaCode, t_s_s_nodequeue.TimeCode, t_s_s_nodequeue.CumuLunch);
                        //    Console.WriteLine("arc.endSta: {0}, arc.endTime:{1}",arc.EndStaCode, arc.EndTimeCode);
                        //    Console.Write("passline：[");
                        //    foreach (var line in t_s_s_nodequeue.PassLine) {
                        //        Console.Write(line + "->");
                        //    }
                        //    Console.Write("passline：]\n");

                        //    int debug = 0;
                        //}                        
                        // end debug 20191020

                        var cur_arc = T_S_ArcList[i];
                        //var cur_startpoint = cur_arc.StartPoint;
                        //var cur_endpoint = cur_arc.EndPoint;
                        //Console.Write(Logger.stateNodeInfoToStr(t_s_s_nodequeue, OutputMode.console));
                        //Console.WriteLine("curArcType[{0}]", cur_arc.ArcType);
                        //Console.WriteLine("OID,OSta,OTime[{0},{1},{2}] \nDID,DSta,DTime[{3},{4},{5}]\n",
                        //    cur_startpoint.ID, cur_startpoint.StaCode, cur_startpoint.TimeCode,
                        //    cur_endpoint.ID, cur_endpoint.StaCode, cur_endpoint.TimeCode);


                        #region 根据用餐约束判断是否有必要继续遍历
                        iscreate = false;
                        bool isBase = false;

                        foreach (CrewBase bs in CrewBaseList) {
                            if (T_S_ArcList[i].StartStaCode == bs.StaCode) {
                                isBase = true;
                                break;
                            }
                        }
                        //if (T_S_ArcList[i].StartStaCode == "南京" || T_S_ArcList[i].StartStaCode == "上海"
                        //    || T_S_ArcList[i].StartStaCode == "杭州" || T_S_ArcList[i].StartStaCode == "合肥") {
                        //    isBase = true;
                        //}

                        if (!isBase || T_S_ArcList[i].ArcType == 32 || T_S_ArcList[i].ArcType == 33) {
                            iscreate = true;
                        }
                        else
                        {
                            //始发点在上午,司机来不及吃午饭，直接出乘
                            if (t_s_s_nodequeue.TimeCode - t_s_s_nodequeue.CumuDayCrewTime < STLunch + minmealtime)
                            {
                                windowofO = 1;
                                //当前点在上午，午饭晚饭均没吃才允许创建
                                if (t_s_s_nodequeue.TimeCode < STLunch && 
                                    t_s_s_nodequeue.CumuLunch == 0 
                                    && t_s_s_nodequeue.CumuDinner == 0)
                                { iscreate = true; }
                                //当前点在午饭时间段，允许创建，这样才能创建用餐弧
                                else if (t_s_s_nodequeue.TimeCode >= STLunch 
                                    && t_s_s_nodequeue.TimeCode <= ETLunch)
                                { iscreate = true; }
                                //当前点在下午，吃过午饭允许创建
                                else if (t_s_s_nodequeue.TimeCode > ETLunch 
                                    && t_s_s_nodequeue.TimeCode < STDinner 
                                    && t_s_s_nodequeue.CumuLunch == 2 
                                    && t_s_s_nodequeue.CumuDinner == 0)
                                { iscreate = true; }
                                //当前点在晚饭时间段，允许创建，这样才能创建用餐弧
                                else if (t_s_s_nodequeue.TimeCode >= STDinner 
                                    && t_s_s_nodequeue.TimeCode <= ETDinner)
                                { iscreate = true; }
                                //当前点在晚间，午饭晚饭均吃过才允许创建
                                else if (t_s_s_nodequeue.TimeCode > ETDinner 
                                    && t_s_s_nodequeue.CumuLunch == 2 
                                    && t_s_s_nodequeue.CumuDinner == 2)
                                { iscreate = true; }
                            }
                            //始发点在午餐段或下午,司机吃午饭后再出乘
                            else if (t_s_s_nodequeue.TimeCode - t_s_s_nodequeue.CumuDayCrewTime >= STLunch + minmealtime
                                && t_s_s_nodequeue.TimeCode - t_s_s_nodequeue.CumuDayCrewTime <= STDinner + minmealtime)
                            {
                                windowofO = 2;
                                //当前点在午饭时间段或下午，允许创建，这样才能创建用餐弧
                                if (t_s_s_nodequeue.TimeCode >= STLunch 
                                    && t_s_s_nodequeue.TimeCode <= ETDinner)
                                { iscreate = true; }
                                //当前点在晚间，午饭晚饭均吃过才允许创建
                                else if (t_s_s_nodequeue.TimeCode > ETDinner 
                                    && t_s_s_nodequeue.CumuDinner == 2)
                                { iscreate = true; }
                            }
                            //始发点在晚餐段或晚间,司机吃晚饭后再出乘
                            else if (t_s_s_nodequeue.TimeCode - t_s_s_nodequeue.CumuDayCrewTime >= STDinner + minmealtime)
                            {
                                windowofO = 3;
                                //当前点在晚饭时间段，允许创建，这样才能创建用餐弧
                                if (t_s_s_nodequeue.TimeCode >= STDinner)
                                { iscreate = true; }
                            }
                        }
                        #endregion
                        if (iscreate == true)
                        {
                            int drive_time = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode + t_s_s_nodequeue.CumuDriveTime;
                            int day_crew_time = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode + t_s_s_nodequeue.CumuDayCrewTime;

                            if (drive_time <= mindrivetime 
                                && day_crew_time <= maxdaycrewtime)//这个弧被执行完时，其累计运行时间不超过单次驾驶时长
                            {
                                isdrive = 1;//必须驾驶
                            }
                            else if (drive_time > mindrivetime 
                                && drive_time <= maxdrivetime)
                            {
                                isdrive = 2;//可以驾驶也可以休息
                            }
                            else
                            {
                                isdrive = 3;//不能驾驶
                            }
                            
                            if (T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode + t_s_s_nodequeue.CumuConnTime
                                <= maxconntime)
                            {
                                isconn = true;
                            }
                            else {
                                isconn = false;
                            }

                            if (T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode
                                + t_s_s_nodequeue.CumuDayCrewTime-t_s_s_nodequeue.CumuConnTime 
                                <= maxdaycrewtime)
                            {
                                isdaycrew = true;
                            }
                            else { isdaycrew = false; }

                            //算法核心！！！两天的网去掉  && T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode + t_s_s_notequeue.CumuDayCrewTime<=maxdaycrewtime
                            if (T_S_ArcList[i].ArcType == 21
                                && t_s_s_nodequeue.PointType != 31
                                && T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode 
                                    + t_s_s_nodequeue.CumuDayCrewTime 
                                    <= maxdaycrewtime)//此弧是日间连接弧
                            {
                                //1、需要间休，t_s_s_notequeue.Relax：0-不需要休息；1-需要间休；2-间休可休可不休；3-需要外段驻班休；4-外段驻班可休克不休（在最小休息时间和最大休息时间之间）。2、需要接续
                                //需要间休
                                if (isdaycrew == true 
                                    && (isdrive != 1 
                                        || t_s_s_nodequeue.Relax == 1 
                                        || t_s_s_nodequeue.Relax == 2)
                                    )//三种状态分别为：1、刚好需要休息；2、已经休息，但还未休息够；3、休息够了，但还没达到最大间休时间
                                #region
                                {
                                    //1、休息时间小于最小休息时间要求，必需休息（1、刚好需要休息；2、已经休息，但还未休息够；）
                                    if (t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= minrelaxtime 
                                        && (t_s_s_nodequeue.CumuConnTime <= maxconntime 
                                            && t_s_s_nodequeue.PointType != 22 
                                            || t_s_s_nodequeue.PointType == 22)
                                        )
                                        //括号里的第一项是为了避免连接弧无限接续；第二项区别于第一项，若为间休弧，可向下接续直到最大间休时间
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;//记录此线是由哪条运行线传过来的，为了建立换乘连接弧使用
                                        t_s_s_note.PointType = 22;//与前继节点组成弧为间休连接弧
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;

                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = 0;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        //午餐状态
                                        //若始发点在上午
                                        if (windowofO==1)
                                        {
                                            //1、间休起点在上午，当前点在午餐时间段
                                            if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime < STLunch 
                                                && t_s_s_note.TimeCode >= STLunch 
                                                && t_s_s_note.TimeCode <= ETLunch 
                                                && t_s_s_nodequeue.CumuLunch != 2)
                                            {
                                                if (t_s_s_note.TimeCode - STLunch > minmealtime) { t_s_s_note.CumuLunch = 2; }//吃完了
                                                else { t_s_s_note.CumuLunch = 1; }//正在吃

                                            }
                                            //2、间休起点、当前点均在在午餐时间段
                                            else if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime >= STLunch 
                                                && t_s_s_note.TimeCode >= STLunch 
                                                && t_s_s_note.TimeCode <= ETLunch 
                                                && t_s_s_nodequeue.CumuLunch != 2)
                                            {
                                                if (t_s_s_note.CumuConnTime > minmealtime) { t_s_s_note.CumuLunch = 2; }//吃完了
                                                else { t_s_s_note.CumuLunch = 1; }//正在吃
                                            }
                                            else { t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch; }//和前继节点的状态相同
                                        }
                                        //若始发点在午餐时间段、下午或晚上，默认吃完午餐
                                        else { t_s_s_note.CumuLunch = 2; }
                                        //晚餐状态
                                        //始发时刻在晚餐时间短之前
                                        if (windowofO != 3)
                                        {
                                            //1、间休起点在下午，当前点在晚餐时间段
                                            if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime < STDinner 
                                                && t_s_s_note.TimeCode >= STDinner 
                                                && t_s_s_note.TimeCode <= ETDinner 
                                                && t_s_s_nodequeue.CumuDinner != 2)
                                            {
                                                if (t_s_s_note.TimeCode - STDinner > minmealtime) { t_s_s_note.CumuDinner = 2; }//吃完了
                                                else { t_s_s_note.CumuDinner = 1; }//正在吃
                                            }
                                            //2、间休起点、当前点均在在晚餐时间段
                                            else if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime >= STDinner 
                                                && t_s_s_note.TimeCode >= STDinner 
                                                && t_s_s_note.TimeCode <= ETDinner 
                                                && t_s_s_nodequeue.CumuDinner != 2)
                                            {
                                                if (t_s_s_note.CumuConnTime > minmealtime) { t_s_s_note.CumuDinner = 2; }//吃完了
                                                else { t_s_s_note.CumuDinner = 1; }//正在吃
                                            }
                                            else { t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner; }//和前继节点的状态相同
                                        }
                                        else { t_s_s_note.CumuDinner = 2; }
                                        t_s_s_note.Relax = 1;//0-不需要休息；1-需要休息；2-可休可不休
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                    //休息时间在最大休息时间和最小休息时间之间，可以休息可以不休息（3、休息够了，但还没达到最大间休时间）
                                    else if (t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                            - T_S_ArcList[i].StartTimeCode >= minrelaxtime 
                                        && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                            - T_S_ArcList[i].StartTimeCode <= maxrelaxtime 
                                        && ((t_s_s_nodequeue.CumuConnTime <= maxconntime 
                                            && t_s_s_nodequeue.PointType != 22) 
                                            || t_s_s_nodequeue.PointType == 22)
                                        )//括号内有问题
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 22;//与前继节点组成弧为间休连接弧
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;

                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = 0;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        //午餐状态
                                        //若始发点在上午
                                        if (windowofO == 1)
                                        {
                                            //1、间休起点在上午，当前点在午餐时间段
                                            if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime < STLunch 
                                                && t_s_s_note.TimeCode >= STLunch
                                                && t_s_s_note.TimeCode <= ETLunch 
                                                && t_s_s_nodequeue.CumuLunch != 2)
                                            {
                                                if (t_s_s_note.TimeCode - STLunch > minmealtime) { t_s_s_note.CumuLunch = 2; }//吃完了
                                                else { t_s_s_note.CumuLunch = 1; }//正在吃

                                            }
                                            //2、间休起点、当前点均在在午餐时间段
                                            else if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime >= STLunch 
                                                && t_s_s_note.TimeCode >= STLunch 
                                                && t_s_s_note.TimeCode <= ETLunch 
                                                && t_s_s_nodequeue.CumuLunch != 2)
                                            {
                                                if (t_s_s_note.CumuConnTime > minmealtime) { t_s_s_note.CumuLunch = 2; }//吃完了
                                                else { t_s_s_note.CumuLunch = 1; }//正在吃
                                            }
                                            else { t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch; }//和前继节点的状态相同
                                        }
                                        //若始发点在午餐时间段、下午或晚上，默认吃完午餐
                                        else { t_s_s_note.CumuLunch = 2; }
                                        //晚餐状态
                                        //始发时刻在晚餐时间短之前
                                        if (windowofO != 3)
                                        {
                                            //1、间休起点在下午，当前点在晚餐时间段
                                            if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime < STDinner 
                                                && t_s_s_note.TimeCode >= STDinner 
                                                && t_s_s_note.TimeCode <= ETDinner 
                                                && t_s_s_nodequeue.CumuDinner != 2)
                                            {
                                                if (t_s_s_note.TimeCode - STDinner > minmealtime) { t_s_s_note.CumuDinner = 2; }//吃完了
                                                else { t_s_s_note.CumuDinner = 1; }//正在吃
                                            }
                                            //2、间休起点、当前点均在在晚餐时间段
                                            else if (t_s_s_note.TimeCode - t_s_s_note.CumuConnTime >= STDinner 
                                                && t_s_s_note.TimeCode >= STDinner 
                                                && t_s_s_note.TimeCode <= ETDinner 
                                                && t_s_s_nodequeue.CumuDinner != 2)
                                            {
                                                if (t_s_s_note.CumuConnTime > minmealtime) { t_s_s_note.CumuDinner = 2; }//吃完了
                                                else { t_s_s_note.CumuDinner = 1; }//正在吃
                                            }
                                            else { t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner; }//和前继节点的状态相同
                                        }
                                        else { t_s_s_note.CumuDinner = 2; }
                                        t_s_s_note.Relax = 2;//可休可不休
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                    //超了，休息任务完成
                                    else if (t_s_s_nodequeue.Relax == 2 
                                        && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                            - T_S_ArcList[i].StartTimeCode > maxrelaxtime)
                                    {
                                        //t_s_s_notequeue.Relax = 0;//休息完毕
                                        //QueueList.Insert(1, t_s_s_notequeue);
                                    }

                                }
                                #endregion
                                else if (t_s_s_nodequeue.CumuDayCrewTime-t_s_s_nodequeue.CumuConnTime >= mindaycrewtime 
                                    || t_s_s_nodequeue.Relax == 3 
                                    || t_s_s_nodequeue.Relax == 4)
                                    //满足交路段时长约束，可以休息，也可以接续
                                #region
                                {
                                    //休息不足
                                    if (t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode < minoutrelaxtime)
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 23;//与前继节点组成弧为外段驻班连接弧
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = 0;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = 0;
                                        t_s_s_note.CumuLunch = 0;
                                        t_s_s_note.CumuDinner = 0;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        t_s_s_note.Relax = 3;//外段驻班必须修
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                    //休息完毕
                                    else if (t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode >= minoutrelaxtime && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode <= maxoutrelaxtime)
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 23;//与前继节点组成弧为外段驻班连接弧
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = 0;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = 0;
                                        t_s_s_note.CumuLunch = 0;
                                        t_s_s_note.CumuDinner = 0;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        t_s_s_note.Relax = 4;//外段驻班可休可不休
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                    //超额休息
                                    else if (t_s_s_nodequeue.Relax == 4 && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode >= maxoutrelaxtime)
                                    {
                                        //t_s_s_notequeue.Relax = 0;
                                        //QueueList.Insert(1, t_s_s_notequeue);
                                    }
                                }
                                #endregion

                                if (isdaycrew == true 
                                    && isdrive != 3 
                                    && t_s_s_nodequeue.Relax == 0)
                                #region
                                {
                                    if (t_s_s_nodequeue.PointType != 24 && isconn == true)//若不是用餐弧
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 21;//与前继节点组成弧为接续连接弧
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuLunch=t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.CumuDinner=t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        t_s_s_note.Relax = 0;
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                    if (t_s_s_nodequeue.CumuLunch != 2 
                                        && windowofO == 1 
                                        && (t_s_s_nodequeue.PointType == 1 || t_s_s_nodequeue.PointType == 24) 
                                        && t_s_s_nodequeue.TimeCode >= STLunch 
                                        && t_s_s_nodequeue.TimeCode + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= ETLunch 
                                        && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= maxmealtime)//午餐
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 24;
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        if (t_s_s_note.CumuConnTime < minmealtime)
                                        {t_s_s_note.CumuLunch = 1;}
                                        else if (t_s_s_note.CumuConnTime >= minmealtime)
                                        { t_s_s_note.CumuLunch =2; }
                                        t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        t_s_s_note.Relax = 0;
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);                                        
                                        k1++;
                                    }
                                    else if (t_s_s_nodequeue.CumuDinner != 2 
                                        && windowofO != 3 
                                        && (t_s_s_nodequeue.PointType == 1 || t_s_s_nodequeue.PointType == 24) 
                                        && t_s_s_nodequeue.TimeCode >= STDinner 
                                        && t_s_s_nodequeue.TimeCode + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= ETDinner 
                                        && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= maxmealtime)//晚餐
                                    {
                                        T_S_S_Node t_s_s_note = new T_S_S_Node();
                                        t_s_s_note.ID = k1;
                                        t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                        t_s_s_note.PointType = 24;
                                        t_s_s_note.LineID = 0;
                                        t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                        t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                        t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                        //途径运行线
                                        for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                        {
                                            t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                        }
                                        t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                        t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch;
                                        if (t_s_s_note.CumuConnTime < minmealtime)
                                        { t_s_s_note.CumuDinner = 1; }
                                        else if (t_s_s_note.CumuConnTime >= minmealtime)
                                        { t_s_s_note.CumuDinner = 2; }
                                        t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                        t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                        t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                        t_s_s_note.Relax = 0;
                                        t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                        t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                        t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                        t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                        t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                        t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                        t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                        t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                        T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                        QueueList.AddLast(t_s_s_note);
                                        //QueueList.Add(t_s_s_note);
                                        k1++;
                                    }
                                }
                                #endregion
                            }
                            //夜间弧：只要不是乘务基地所在站,并且该站有能力停驻乘务组，就必须过夜
                            else if (T_S_ArcList[i].ArcType == 22
                                && t_s_s_nodequeue.PointType != 31 
                                && T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode 
                                    + t_s_s_nodequeue.CumuDayCrewTime <= maxdaycrewtime)
                            {
                                #region
                              
                                //乘务组未在乘务基地过夜，外段驻班:不是本站，且时长不超
                                if (t_s_s_nodequeue.StaCode != t_s_s_nodequeue.OStation 
                                    && t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode 
                                        - T_S_ArcList[i].StartTimeCode <= maxoutrelaxtime)
                                {
                                    T_S_S_Node t_s_s_note = new T_S_S_Node();
                                    t_s_s_note.ID = k1;
                                    t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                    t_s_s_note.PointType = 23;//与前继节点组成弧为接续外段驻班弧
                                    t_s_s_note.LineID = 0;
                                    t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                    t_s_s_note.PrevNode = t_s_s_nodequeue;
                                    t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                    t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                    t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                    //途径运行线
                                    for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                    {
                                        t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                    }
                                    t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                    t_s_s_note.CumuDriveTime = 0;
                                    t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                    t_s_s_note.CumuDayCrewTime = 0;
                                    t_s_s_note.CumuLunch = 0;
                                    t_s_s_note.CumuDinner = 0;
                                    t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                    t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                    t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                    if (t_s_s_note.CumuConnTime < minoutrelaxtime)
                                    {
                                        t_s_s_note.Relax = 3;//外段驻班修
                                    }
                                    else
                                    {
                                        t_s_s_note.Relax = 4;//外段驻班可休可不休
                                    }
                                    t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                    t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                    t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                    t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                    t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                    t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                    t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                    t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                    T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                    QueueList.AddLast(t_s_s_note);
                                    //QueueList.Add(t_s_s_note);
                                    k1++;
                                }
                                #endregion
                            }

                             //运行弧：只要不是间修或外段驻班休不足就可以担当任务
                            else if (T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode 
                                    + t_s_s_nodequeue.CumuDayCrewTime <= maxdaycrewtime
                                && t_s_s_nodequeue.CumuLunch != 1
                                && t_s_s_nodequeue.CumuDinner != 1
                                && isdaycrew == true 
                                && T_S_ArcList[i].ArcType == 1 
                                && isdrive != 3 
                                && t_s_s_nodequeue.Relax != 1 && t_s_s_nodequeue.Relax != 3 
                                && (t_s_s_nodequeue.PointType != 23
                                    || t_s_s_nodequeue.PointType == 23 
                                    && t_s_s_nodequeue.Relax == 4)
                                )                          
                            {
                                #region 回溯
                                bool driveornot = true;
                                /*回溯法，查找此条径路已经走过的点是否包含了T_S_ArcList[i]这条运行弧，
                            同一个运行弧不能被执行两次,在这里排除被同一个乘务执行两次，在启发式算法中排除被不同的乘务组执行两次，
                             保证每一个可行解的可行性*/
                                for (l = 0; l < t_s_s_nodequeue.PassLine.Count;l++ )
                                {
                                    if(T_S_ArcList[i].LineID==t_s_s_nodequeue.PassLine[l])
                                    {
                                        driveornot = false;
                                        break;
                                    }
                                }
                                #endregion

                                    if (driveornot == true)
                                    {
                                        //同一交路，不换乘
                                        if (t_s_s_nodequeue.RountingID == T_S_ArcList[i].RountingID)
                                        #region
                                        {

                                            T_S_S_Node t_s_s_note = new T_S_S_Node();
                                            t_s_s_note.ID = k1;
                                            t_s_s_note.RountingID = T_S_ArcList[i].RountingID;
                                            t_s_s_note.PointType = 1;//与前继节点组成弧为运行弧
                                            t_s_s_note.LineID = T_S_ArcList[i].LineID;
                                            t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                            t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                            t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                            //途径运行线
                                            for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                            {
                                                t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                            }
                                            t_s_s_note.PassLine.Add(t_s_s_note.LineID);
                                            t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                            t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.CumuConnTime = 0;//列车终到点初始化，均为0
                                            t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch;
                                            t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner;
                                            t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                            t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                            t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                            t_s_s_note.Relax = 0;
                                            t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                            t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                            t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                            t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                            t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                            t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                            t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                            T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                            QueueList.AddLast(t_s_s_note);
                                            //QueueList.Add(t_s_s_note);
                                            k1++;
                                        }
                                        #endregion
                                        //不同交路，换乘
                                        else if (t_s_s_nodequeue.RountingID != T_S_ArcList[i].RountingID 
                                        && t_s_s_nodequeue.CumuConnTime >= mintranslation)
                                        #region
                                        {
                                            T_S_S_Node t_s_s_note = new T_S_S_Node();
                                            t_s_s_note.ID = k1;
                                            t_s_s_note.RountingID = T_S_ArcList[i].RountingID;//终到点交路代号更新
                                            t_s_s_note.PointType = 1;//与前继节点组成弧为运行弧
                                            t_s_s_note.LineID = T_S_ArcList[i].LineID;
                                            t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                            t_s_s_note.PrevNode = t_s_s_nodequeue;
                                        t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                        t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                            t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                            //途径运行线
                                            for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                            {
                                                t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                            }
                                            t_s_s_note.PassLine.Add(t_s_s_note.LineID);
                                            t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                            t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.CumuConnTime = 0;//列车终到点初始化，均为0
                                            t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime + T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch;
                                            t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner;
                                            t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                            t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                            t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                            t_s_s_note.Relax = 0;
                                            t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                            t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                            t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                            t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                            t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                            t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                            t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                            t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                            T_S_S_NodeTempList.Add(t_s_s_note.ID,t_s_s_note);
                                            QueueList.AddLast(t_s_s_note);
                                            //QueueList.Add(t_s_s_note);
                                            k1++;
                                        }
                                        #endregion
                                    }
                            }

                            else if (T_S_ArcList[i].ArcType == 31)
                            {
                                T_S_S_Node t_s_s_note = new T_S_S_Node();
                                t_s_s_note.ID = k1;
                                t_s_s_note.RountingID = T_S_ArcList[i].RountingID;
                                t_s_s_note.PointType = 31;//与前继节点组成弧为出乘弧
                                t_s_s_note.LineID = 0;
                                t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                t_s_s_note.PrevNode = t_s_s_nodequeue;
                                t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                //途径运行线
                                for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                {
                                    t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                }
                                t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                t_s_s_note.CumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                t_s_s_note.CumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                if (t_s_s_note.TimeCode<STLunch+minmealtime) { t_s_s_note.CumuLunch = 0; }
                                else { t_s_s_note.CumuLunch = 2; }
                                if (t_s_s_note.TimeCode>STDinner+minmealtime) { t_s_s_note.CumuDinner = 2; }
                                else { t_s_s_note.CumuDinner = 0; }
                                t_s_s_note.Relax = 0;
                                //t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                t_s_s_note.Price = 1440; //出乘固定成本 // T_S_ArcList[i].EndTimeCode; //60;
                                t_s_s_note.PervCumuConnTime = t_s_s_nodequeue.CumuConnTime;
                                t_s_s_note.PervCumuDriveTime = t_s_s_nodequeue.CumuDriveTime;
                                t_s_s_note.PervCumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                t_s_s_note.PervCumuLunch = t_s_s_nodequeue.CumuLunch;
                                t_s_s_note.PervCumuDinner = t_s_s_nodequeue.CumuDinner;
                                t_s_s_note.PervRelax = t_s_s_nodequeue.Relax;
                                t_s_s_note.PerPointType = t_s_s_nodequeue.PointType;
                                T_S_S_NodeTempList.Add(t_s_s_note.ID, t_s_s_note);
                                QueueList.AddLast(t_s_s_note);
                                //QueueList.Add(t_s_s_note);
                                k1++;
                            }
                            else if (T_S_ArcList[i].ArcType == 32 
                                && T_S_ArcList[i].EndStaCode == t_s_s_nodequeue.OStation 
                                && t_s_s_nodequeue.PointType == 1 
                                && t_s_s_nodequeue.CumuDayCrewTime >= mindaycrewtime 
                                && t_s_s_nodequeue.CumuDayCrewTime <= maxdaycrewtime)
                            {
                                T_S_S_Node t_s_s_note = new T_S_S_Node();
                                foreach (CrewBase crewbase in CrewBaseList)
                                {
                                    if (T_S_ArcList[i].EndStaCode == crewbase.StaCode)
                                    {
                                        t_s_s_note.ID = crewbase.DIDinTSS;//将起点和终点放在编号前列，便于连续编号
                                    }
                                }
                                t_s_s_note.RountingID = t_s_s_nodequeue.RountingID;
                                t_s_s_note.PointType = 32;
                                t_s_s_note.LineID = 0;
                                t_s_s_note.PrevID = t_s_s_nodequeue.ID;

                                t_s_s_note.PrevNode = t_s_s_nodequeue;
                                t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                t_s_s_note.PrevSuperID = t_s_s_nodequeue.SuperPointID;
                                t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                //途径运行线
                                for (l = 0; l < t_s_s_nodequeue.PassLine.Count; l++)
                                {
                                    t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                }
                                t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                t_s_s_note.CumuDriveTime = 0;
                                t_s_s_note.CumuConnTime = 0;
                                t_s_s_note.CumuDayCrewTime = t_s_s_nodequeue.CumuDayCrewTime;
                                t_s_s_note.CumuLunch = t_s_s_nodequeue.CumuLunch;
                                t_s_s_note.CumuDinner = t_s_s_nodequeue.CumuDinner;
                                t_s_s_note.SuperPointID = T_S_ArcList[i].EndTimeCode;
                                t_s_s_note.TimeCode = T_S_ArcList[i].StartTimeCode;
                                t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                t_s_s_note.Relax = 1;
                                //t_s_s_note.Price = T_S_ArcList[i].EndTimeCode - T_S_ArcList[i].StartTimeCode;
                                t_s_s_note.Price = 0;// 1440 - T_S_ArcList[i].StartTimeCode; //60;
                                //t_s_s_note.PervCumuConnTime =t_s_s_notequeue.CumuConnTime;
                                t_s_s_note.PervCumuDriveTime =t_s_s_nodequeue.CumuDriveTime;
                                t_s_s_note.PervCumuDayCrewTime =t_s_s_nodequeue.CumuDayCrewTime;
                                t_s_s_note.PervCumuLunch =t_s_s_nodequeue.CumuLunch;
                                t_s_s_note.PervCumuDinner =t_s_s_nodequeue.CumuDinner;
                                t_s_s_note.PervRelax =t_s_s_nodequeue.Relax;
                                t_s_s_note.PerPointType =t_s_s_nodequeue.PointType;
                                T_S_S_NodeODTempList.Add(t_s_s_note);
                                QueueList.AddLast(t_s_s_note);
                                //QueueList.Add(t_s_s_note);
                            }
                            else if (T_S_ArcList[i].ArcType == 33)
                            {
                                T_S_S_Node t_s_s_note = new T_S_S_Node();
                                foreach (CrewBase crewbase in CrewBaseList)
                                {
                                    if (T_S_ArcList[i].EndStaCode == crewbase.StaCode)
                                    {
                                        t_s_s_note.ID = crewbase.DIDinTSS;//将起点和终点放在编号前列，便于连续编号
                                    }
                                }
                                t_s_s_note.RountingID =t_s_s_nodequeue.RountingID;
                                t_s_s_note.PointType = 33;//与前面连成虚拟乘务组出乘弧
                                t_s_s_note.LineID = 0;
                                t_s_s_note.PrevID =t_s_s_nodequeue.ID;

                                t_s_s_note.PrevNode = t_s_s_nodequeue;
                                t_s_s_note.SuperPoint = cur_arc.EndPoint;
                                t_s_s_note.PrevSuperID =t_s_s_nodequeue.SuperPointID;
                                t_s_s_note.StaCode = T_S_ArcList[i].EndStaCode;
                                //途径运行线
                                for (l = 0; l <t_s_s_nodequeue.PassLine.Count; l++)
                                {
                                    t_s_s_note.PassLine.Add(t_s_s_nodequeue.PassLine[l]);
                                }
                                t_s_s_note.OStation = t_s_s_nodequeue.OStation;
                                t_s_s_note.CumuDriveTime = 0;
                                t_s_s_note.CumuConnTime = 0;
                                t_s_s_note.CumuDayCrewTime = 0;
                                t_s_s_note.CumuLunch = 0;
                                t_s_s_note.CumuDinner = 0;
                                t_s_s_note.SuperPointID = T_S_ArcList[i].EndPointID;
                                t_s_s_note.TimeCode = T_S_ArcList[i].EndTimeCode;
                                t_s_s_note.TrainCode = T_S_ArcList[i].TrainCode;
                                t_s_s_note.Relax = 1;
                                t_s_s_note.Price = virtualRoutingCost;//虚拟停驻交路的成本 //1000;//1200;//
                                t_s_s_note.PervCumuConnTime =t_s_s_nodequeue.CumuConnTime;
                                t_s_s_note.PervCumuDriveTime =t_s_s_nodequeue.CumuDriveTime;
                                t_s_s_note.PervCumuDayCrewTime =t_s_s_nodequeue.CumuDayCrewTime;
                                t_s_s_note.PervCumuLunch =t_s_s_nodequeue.CumuLunch;
                                t_s_s_note.PervCumuDinner =t_s_s_nodequeue.CumuDinner;
                                t_s_s_note.PervRelax =t_s_s_nodequeue.Relax;
                                t_s_s_note.PerPointType =t_s_s_nodequeue.PointType;
                                T_S_S_NodeODTempList.Add(t_s_s_note);
                                QueueList.AddLast(t_s_s_note);                                
                            }
                        }
                    }
                }
                #region //剪枝规则
                //bool flag = false;
                //foreach (CrewBase crewbase in CrewBaseList)
                //{
                //    if(t_s_s_notequeue.ID == crewbase.DIDinTSS)
                //    {
                //        flag = true;
                //        break;
                //    }
                //}
                //if(QueueList.Count !=QueueCount ||flag ==true)
                //{
                //    isAddNode = true;
                //}
                //else
                //{
                //    isAddNode = false ;
                //}
                //if(isAddNode == false)
                //{
                //    int resent=0,nnext=0;//nnext代表resent点的后继点数，若为1则直接删掉
                //    int deleteindex=0;//哈希表里删除字典的索引
                //    //T_S_S_Note node = new T_S_S_Note();
                //    //T_S_S_NoteTempList.Remove(t_s_s_notequeue.ID);
                //    //k++;
                //    resent = t_s_s_notequeue.ID;
                //    do
                //    {
                //        foreach (KeyValuePair<int, T_S_S_Note> node in T_S_S_NoteTempList)
                //        {
                //            if (node.Value.PervID == resent)
                //            {
                //                nnext++;
                //                break;
                //            }
                //        }
                //        if (nnext == 0)
                //        {
                //            deleteindex = resent;
                //            resent = T_S_S_NoteTempList[deleteindex].PervID;
                //            T_S_S_NoteTempList.Remove(deleteindex);
                //        }
                //    }
                //    while (nnext == 0);
                //}
                #endregion
                QueueList.RemoveFirst();                
            }         
            while (QueueList.Count > 0);            
            #endregion 
            //剪枝
             int d = 0;
            QueueList.Clear();
            List<T_S_S_Node> DPointList = new List<T_S_S_Node>();
            T_S_S_NodeDelList = new List<T_S_S_Node>();
            //将所有的退乘弧加入集合
            foreach (CrewBase crewbase in CrewBaseList)
            {
                for (i = 0; i < T_S_S_NodeODTempList.Count; i++)
                {
                    if (T_S_S_NodeODTempList[i].ID == crewbase.DIDinTSS)
                    {
                        T_S_S_NodeODTempList[i].Isfeasible = 1;
                        DPointList.Add(T_S_S_NodeODTempList[i]);
                    }
                }
            }
            foreach (T_S_S_Node note in DPointList)
            {
                d = note.PrevID;
                do
                {
                    if(T_S_S_NodeTempList.ContainsKey(d))
                    {                       
                        T_S_S_NodeTempList[d].Isfeasible = 1;
                        d = T_S_S_NodeTempList[d].PrevID;
                    }                                  
                }
                while (d != -1);
            }
            //这一步是可以优化的，回溯找到的可行点直接加入到T_S_S_NoteDelList 
            //CrewBaseList.Count+T_S_S_NoteTempList.Count中的CrewBaseList.Count位TSS网络虚拟时空状态起点的个数，剩下的均为连续标号
            for (i = 0; i < CrewBaseList.Count+T_S_S_NodeTempList.Count; i++)
            {
                if (T_S_S_NodeTempList.ContainsKey(i) && T_S_S_NodeTempList[i].Isfeasible == 1)
                {
                    T_S_S_NodeDelList.Add(T_S_S_NodeTempList[i]);
                }
            }
            //加入终点数据的状态
            for (i = 0; i < DPointList.Count; i++)
            {               
                T_S_S_NodeDelList.Add(DPointList[i]);
            }

            //bool isContain = false;
            T_S_S_NodeList = new List<T_S_S_Node>();
            for (i = 0; i < T_S_S_NodeDelList.Count; i++)
            {
                T_S_S_NodeList.Add(T_S_S_NodeDelList[i]);
            }
            //删除重复状态
            #region 合并状态
            //int j = 0;
            //bool isContain = false;
            //T_S_S_NoteList = new List<T_S_S_Note>();
            //T_S_S_NoteList.Add(T_S_S_NoteTempList[0]);

            ////合并标号需要改变ID（key）的值，所以不能用Hash
            ////实际是求一个序列的最长公共子序列，可以动态规划，日后简化
            //for (i = 0; i < T_S_S_NoteDelList.Count; i++)
            //{
            //    isContain = false;
            //    for (j = 0; j < T_S_S_NoteList.Count; j++)
            //    {
            //        if (T_S_S_NoteDelList[i].SuperPointID == T_S_S_NoteList[j].SuperPointID && T_S_S_NoteDelList[i].CumuConnTime == T_S_S_NoteList[j].CumuConnTime && T_S_S_NoteDelList[i].CumuDriveTime == T_S_S_NoteList[j].CumuDriveTime && T_S_S_NoteDelList[i].CumuDayCrewTime == T_S_S_NoteList[j].CumuDayCrewTime && T_S_S_NoteDelList[i].CumuLunch == T_S_S_NoteList[j].CumuLunch && T_S_S_NoteDelList[i].CumuDinner == T_S_S_NoteList[j].CumuDinner && T_S_S_NoteDelList[i].Relax == T_S_S_NoteList[j].Relax && T_S_S_NoteDelList[i].PointType == T_S_S_NoteList[j].PointType && T_S_S_NoteDelList[i].PervSuperID == T_S_S_NoteList[j].PervSuperID && T_S_S_NoteDelList[i].PervCumuConnTime == T_S_S_NoteList[j].PervCumuConnTime && T_S_S_NoteDelList[i].PervCumuDriveTime == T_S_S_NoteList[j].PervCumuDriveTime && T_S_S_NoteDelList[i].PervCumuDayCrewTime == T_S_S_NoteList[j].PervCumuDayCrewTime && T_S_S_NoteDelList[i].PervCumuLunch == T_S_S_NoteList[j].PervCumuLunch && T_S_S_NoteDelList[i].PervCumuDinner == T_S_S_NoteList[j].PervCumuDinner && T_S_S_NoteDelList[i].PervRelax == T_S_S_NoteList[j].PervRelax && T_S_S_NoteDelList[i].PerPointType == T_S_S_NoteList[j].PerPointType)
            //        {
            //            isContain = true;
            //            break;
            //        }
            //    }
            //    if (isContain == false)
            //    {
            //        if (T_S_S_NoteDelList[i].PervID == 5562 && T_S_S_NoteDelList[i].ID == 1)
            //        {

            //            int a = 111;

            //        }
            //        T_S_S_NoteList.Add(T_S_S_NoteDelList[i]);

            //    }
            //}
            ////#region 剪枝
            ////////剪枝
            //////int d = 0;
            //////QueueList.Clear();
            //////List<T_S_S_Note> DPointList = new List<T_S_S_Note>();
            //////T_S_S_NoteList = new List<T_S_S_Note>();
            ////////将所有的退乘弧加入集合
            //////foreach (CrewBase crewbase in CrewBaseList)
            //////{
            //////    for (i = 0; i < T_S_S_NoteTempList.Count; i++)
            //////    {
            //////        if (T_S_S_NoteTempList[i].ID == crewbase.DIDinTSS)
            //////        {
            //////            T_S_S_NoteTempList[i].Isfeasible = 1;
            //////            DPointList.Add(T_S_S_NoteTempList[i]);
            //////        }
            //////    }
            //////}
            //////foreach (T_S_S_Note note in DPointList)
            //////{
            //////    d = note.PervID;
            //////    do
            //////    {
            //////        for (i = T_S_S_NoteTempList.Count - 1; i >= 0; i--)
            //////        {
            //////            if (T_S_S_NoteTempList[i].ID == d)
            //////            {
            //////                d = T_S_S_NoteTempList[i].PervID;
            //////                T_S_S_NoteTempList[i].Isfeasible = 1;
            //////            }

            //////        }

            //////    }
            //////    while (d != -1);
            //////}
            //////for (i = 0; i < T_S_S_NoteTempList.Count; i++)
            //////{
            //////    if (T_S_S_NoteTempList[i].Isfeasible == 1)
            //////    {
            //////        T_S_S_NoteList.Add(T_S_S_NoteTempList[i]);
            //////    }
            //////}
            ////#endregion
            //////重新编号1、算出出虚拟起终点外所有的点
            //////严重问题，重大bug！！！！！！！
            ////for (j = 0; j < T_S_S_NoteList.Count; j++)
            ////{
            ////    for (i = 0; i < T_S_S_NoteList.Count; i++)
            ////    {
            ////        //T_S_S_NoteList[j].PointType != 31&(T_S_S_NoteList[i].PointType != 32 && T_S_S_NoteList[i].PointType != 33) &&
            ////        if (T_S_S_NoteList[i].PointType != 31 & T_S_S_NoteList[j].PointType != 32 && T_S_S_NoteList[j].PointType != 33 && T_S_S_NoteList[j].PervSuperID == T_S_S_NoteList[i].SuperPointID && T_S_S_NoteList[j].PervCumuConnTime == T_S_S_NoteList[i].CumuConnTime && T_S_S_NoteList[j].PervCumuDriveTime == T_S_S_NoteList[i].CumuDriveTime && T_S_S_NoteList[j].PervCumuDayCrewTime == T_S_S_NoteList[i].CumuDayCrewTime && T_S_S_NoteList[j].PervCumuLunch == T_S_S_NoteList[i].CumuLunch && T_S_S_NoteList[j].PervCumuDinner == T_S_S_NoteList[i].CumuDinner && T_S_S_NoteList[j].PervRelax == T_S_S_NoteList[i].Relax && T_S_S_NoteList[j].PerPointType == T_S_S_NoteList[i].PointType)
            ////        {
            ////            if (T_S_S_NoteList[j].PervID == 5562 && T_S_S_NoteList[j].ID == 1)
            ////            {
            ////                int a = 111;
            ////            }

            ////            T_S_S_NoteList[i].ID = T_S_S_NoteList[j].PervID;
            ////        }
            ////    }
            //}
            #endregion
            #region 重新编号数组索引法            
            //用Hash查找快
            Dictionary<int, int> NewID = new Dictionary<int, int>();
            int value = CrewBaseList.Count * 2;//非起终点的标号开始点
            foreach (T_S_S_Node note in T_S_S_NodeList)
            {                
                if (note.ID >= CrewBaseList.Count * 2)
                {
                    if (!NewID.ContainsKey(note.ID))
                    {
                        NewID.Add(note.ID, value);
                        value++;
                    }
                }
            }           
            for (i = 0; i < T_S_S_NodeList.Count; i++)
            {
                if (NewID.ContainsKey(T_S_S_NodeList[i].ID))
                {
                    T_S_S_NodeList[i].ID = NewID[T_S_S_NodeList[i].ID];
                }
                if (NewID.ContainsKey(T_S_S_NodeList[i].PrevID))
                {
                    T_S_S_NodeList[i].PrevID = NewID[T_S_S_NodeList[i].PrevID];
                }               
            }
            #endregion                
            #region 建立时空状态网
            k1 = 1;
            for (i = 0; i < T_S_S_NodeList.Count; i++)
            {
                if (T_S_S_NodeList[i].PointType != 0)
                {
                    T_S_S_Arc t_s_s_arc = new T_S_S_Arc();
                    t_s_s_arc.ID = k1;
                    t_s_s_arc.StartPointID = T_S_S_NodeList[i].PrevID;
                    t_s_s_arc.EndPointID = T_S_S_NodeList[i].ID;
                    if (T_S_S_NodeList[i].PointType == 31)
                    {
                        t_s_s_arc.StartTimeCode = 0;
                        t_s_s_arc.EndTimeCode = T_S_S_NodeList[i].TimeCode;
                    }
                    else if(T_S_S_NodeList[i].PointType == 32)
                    {
                        t_s_s_arc.StartTimeCode = T_S_S_NodeList[i].TimeCode;
                        t_s_s_arc.EndTimeCode = 1440;
                    }
                    else if(T_S_S_NodeList[i].PointType == 33)
                    {
                        t_s_s_arc.StartTimeCode = 0;
                        t_s_s_arc.EndTimeCode = 1440;
                    }
                    else
                    {
                        t_s_s_arc.StartTimeCode = T_S_S_NodeList[i].TimeCode - Convert.ToInt32(T_S_S_NodeList[i].Price);
                        t_s_s_arc.EndTimeCode = T_S_S_NodeList[i].TimeCode;
                    }
                    
                    t_s_s_arc.NumSelected = 0;
                    t_s_s_arc.LineID = T_S_S_NodeList[i].LineID;
                    if (T_S_S_NodeList[i].PointType == 1)
                    {
                        t_s_s_arc.TrainCode = T_S_S_NodeList[i].TrainCode;
                    }
                    else
                    {
                        t_s_s_arc.TrainCode = "";
                    }
                    t_s_s_arc.ArcType = T_S_S_NodeList[i].PointType;

                    //t_s_s_arc.Price = Convert.ToDouble(T_S_S_NodeList[i].Price);
                    t_s_s_arc.SetPenaltyMealViolate(0);
                    t_s_s_arc.SetArcPrice(Convert.ToDouble(T_S_S_NodeList[i].Price), t_s_s_arc.PenaltyMealViolate);

                    t_s_s_arc.LagPrice = Convert.ToDouble(T_S_S_NodeList[i].Price);                   
                    t_s_s_arc.HeurPrice = Convert.ToDouble(T_S_S_NodeList[i].Price);                   
                    t_s_s_arc.EndCumuLunch = T_S_S_NodeList[i].CumuLunch;
                    t_s_s_arc.EndCumuDinner = T_S_S_NodeList[i].CumuDinner;                    
                    if (t_s_s_arc.ArcType == 1)
                    {
                        t_s_s_arc.LagMultiplier = 0;
                    }
                    else if(t_s_s_arc.ArcType == 33)
                    {
                        t_s_s_arc.LagMultiplier = 0;
                    }
                    else { t_s_s_arc.LagMultiplier = -1; }
                    foreach (CrewBase crewbase in CrewBaseList)
                    {
                        if (t_s_s_arc.ArcType == 33 
                            && t_s_s_arc.StartPointID == crewbase.OIDinTSS)
                        {
                            t_s_s_arc.BestNs = crewbase.PhyCrewCapacity;//将起点和终点放在编号前列，便于连续编号
                        }
                    }
                    T_S_S_ArcList.Add(t_s_s_arc);
                    k1++;
                }
            }
         
            #endregion

            return T_S_S_ArcList;

        }
        public bool AllLineContain()
        {
            int i=0,j=0;
            bool isnotcontain = false;
            for (i = 0; i < LineList.Count; i++)
            {
                LineList[i].TSSContain = false;
            }
            for(i=0;i<LineList.Count;i++)
            {
                for(j=0;j<T_S_S_ArcList.Count;j++)
                {
                    if(LineList[i].ID==T_S_S_ArcList[j].LineID)
                    {
                        LineList[i].TSSContain =true;
                    }
                }
            }

            string str = "";
            string savepath = ".\\looseSeg.csv";
            string str1 = "";
            string savepath1 = ".\\looseSeg1.txt";
            string str2 = "";
            string savepath2 = ".\\chosenSeg.txt";
            int numLoose = 0;
            string str3 = "";
            string savepath3 = ".\\dontChosenSeg.txt";
            for (i = 0; i < LineList.Count; i++)
            {
                if (LineList[i].TSSContain == false)
                {
                    isnotcontain = true;
                    str += LineList[i].TrainCode + ",";
                    numLoose++;
                    string strTemp1 = "";
                    strTemp1 += LineList[i].ID + ",";
                    strTemp1 += LineList[i].TrainCode + ",";
                    strTemp1 += LineList[i].DepStaCode + ",";
                    strTemp1 += LineList[i].ArrStaCode + ",";
                    strTemp1 += LineList[i].DepTimeCode + ",";
                    strTemp1 += LineList[i].ArrTimeCode + ",";
                    strTemp1 += LineList[i].RountingID + ",";
                    str3 += strTemp1 + '\n';
                    //break;
                }
                else if (LineList[i].TSSContain)
                {
                    string strTemp = "";
                    strTemp += LineList[i].ID + ",";
                    strTemp += LineList[i].TrainCode + ",";
                    strTemp += LineList[i].DepStaCode + ",";
                    strTemp += LineList[i].ArrStaCode + ",";
                    strTemp += LineList[i].DepTimeCode + ",";
                    strTemp += LineList[i].ArrTimeCode + ",";
                    strTemp += LineList[i].RountingID + ",";
                    str2 += strTemp + '\n';
                }
            }
            str1 += numLoose;
            File.WriteAllText(savepath, str);
            File.WriteAllText(savepath1, str1);
            File.WriteAllText(savepath2, str2);
            File.WriteAllText(savepath3, str3);
            return isnotcontain;
        }
        public DataTable ShowResult()
        {
            Dt = Ds.Tables["Show"];
            Dt.Columns.Add(new DataColumn("编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("运行线代号", typeof(int)));
            Dt.Columns.Add(new DataColumn("起点编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("终点编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("lag", typeof(double)));
            //Dt.Columns.Add(new DataColumn("始发站", typeof(string)));
            //Dt.Columns.Add(new DataColumn("终到站", typeof(string)));
            //Dt.Columns.Add(new DataColumn("始发时间", typeof(string)));
            //Dt.Columns.Add(new DataColumn("终到时间", typeof(string)));
            //Dt.Columns.Add(new DataColumn("起点累计连接时间", typeof(int)));
            //Dt.Columns.Add(new DataColumn("终点累计连接时间", typeof(int)));
            //Dt.Columns.Add(new DataColumn("起点累计驾驶时间", typeof(int)));
            //Dt.Columns.Add(new DataColumn("终点累计驾驶时间", typeof(int)));
            //Dt.Columns.Add(new DataColumn("起点所属超节点", typeof(int)));
            //Dt.Columns.Add(new DataColumn("终点所属超节点", typeof(int)));
            Dt.Columns.Add(new DataColumn("价格", typeof(double)));
            Dt.Columns.Add(new DataColumn("弧类型", typeof(int)));
            Dt.Columns.Add(new DataColumn("被选次数", typeof(int)));

            foreach (T_S_S_Arc arc in T_S_S_ArcList)
            {
                row = Dt.NewRow();
                row["编号"] = arc.ID;
                row["运行线代号"] = arc.LineID;
                row["起点编号"] = arc.StartPointID;
                row["终点编号"] = arc.EndPointID;
                row["lag"] = arc.LagMultiplier;
                //row["始发站"] = arc.StartStaCode;
                //row["终到站"] = arc.EndStaCode;
                //row["始发时间"] = arc.StartTimeCode;
                //row["终到时间"] = arc.EndTimeCode;
                //row["起点累计连接时间"] = arc.StartCumuConnTime;
                //row["起点累计驾驶时间"] = arc.StartCumuDriveTime;
                //row["终点累计连接时间"] = arc.EndCumuConnTime;
                //row["终点累计驾驶时间"] = arc.EndCumuDriveTime;
                //row["起点所属超节点"] = arc.StartSuperPointID;
                //row["终点所属超节点"] = arc.EndSuperPointID;
                row["价格"] = arc.Price;
                row["弧类型"] = arc.ArcType;
                row["被选次数"] = arc.NumSelected;
                Dt.Rows.Add(row);

            }
            return Dt;
        }
        public DataTable ShowResult1()
        {
            Dt = Ds.Tables["Show"];
            Dt.Columns.Add(new DataColumn("前继超节点", typeof(int)));
            Dt.Columns.Add(new DataColumn("所属超节点", typeof(int)));
            Dt.Columns.Add(new DataColumn("累计连接时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("累计驾驶时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("累计乘务时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("午餐状态", typeof(int)));
            Dt.Columns.Add(new DataColumn("晚餐状态", typeof(int)));
            Dt.Columns.Add(new DataColumn("休息状态", typeof(int)));
            Dt.Columns.Add(new DataColumn("点类型", typeof(int)));
            Dt.Columns.Add(new DataColumn("前累计连接时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("前累计驾驶时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("前累计乘务时间", typeof(int)));
            Dt.Columns.Add(new DataColumn("前午餐状态", typeof(int)));
            Dt.Columns.Add(new DataColumn("前晚餐状态", typeof(int))); 
            Dt.Columns.Add(new DataColumn("前休息状态", typeof(int)));
            Dt.Columns.Add(new DataColumn("前点类型", typeof(int)));
            Dt.Columns.Add(new DataColumn("前继节点", typeof(int)));
            Dt.Columns.Add(new DataColumn("编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("运行线代号", typeof(int)));
            Dt.Columns.Add(new DataColumn("车次", typeof(string)));
            Dt.Columns.Add(new DataColumn("价格", typeof(double)));
            Dt.Columns.Add(new DataColumn("车站", typeof(string)));
            Dt.Columns.Add(new DataColumn("时间", typeof(string)));

            foreach (T_S_S_Node arc in T_S_S_NodeList)
            {
                row = Dt.NewRow();
                row["编号"] = arc.ID;
                row["前继节点"] = arc.PrevID;
                row["前继超节点"] = arc.PrevSuperID;
                row["车站"] = arc.StaCode;
                row["运行线代号"] = arc.LineID;
                row["车次"] = arc.TrainCode; 
                row["时间"] = arc.TimeCode;
                row["点类型"] = arc.PointType;
                row["前点类型"] = arc.PerPointType;
                row["累计连接时间"] = arc.CumuConnTime;
                row["累计乘务时间"] = arc.CumuDayCrewTime;
                row["累计驾驶时间"] = arc.CumuDriveTime;
                row["午餐状态"] = arc.CumuLunch;
                row["晚餐状态"] = arc.CumuDinner;
                row["休息状态"] = arc.Relax;
                row["前休息状态"] = arc.PervRelax;
                row["前累计连接时间"] = arc.PervCumuConnTime;
                row["前累计乘务时间"] = arc.PervCumuDayCrewTime;
                row["前累计驾驶时间"] = arc.PervCumuDriveTime;
                row["前午餐状态"] = arc.PervCumuLunch;
                row["前晚餐状态"] = arc.PervCumuDinner;
                row["所属超节点"] = arc.SuperPointID;
                row["价格"] = arc.Price;
                Dt.Rows.Add(row);

            }
            return Dt;
        }
        public DataTable ShowResult2()
        {
            
            Dt = Ds.Tables["Show"];
            Dt.Columns.Add(new DataColumn("编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("运行线代号", typeof(int)));
            Dt.Columns.Add(new DataColumn("起点编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("终点编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("车次", typeof(string)));
            Dt.Columns.Add(new DataColumn("始发站", typeof(string)));
            Dt.Columns.Add(new DataColumn("终到站", typeof(string)));
            Dt.Columns.Add(new DataColumn("始发时间", typeof(string)));
            Dt.Columns.Add(new DataColumn("终到时间", typeof(string)));
            Dt.Columns.Add(new DataColumn("弧类型", typeof(int)));

            foreach (T_S_Arc arc in T_S_ArcList)
            {
                row = Dt.NewRow();
                row["编号"] = arc.ID;
                row["运行线代号"] = arc.LineID;
                row["起点编号"] = arc.StartPointID;
                row["终点编号"] = arc.EndPointID;
                row["车次"] = arc.TrainCode;
                row["始发站"] = arc.StartStaCode;
                row["终到站"] = arc.EndStaCode;
                row["始发时间"] = arc.StartTimeCode;
                row["终到时间"] = arc.EndTimeCode;
                row["弧类型"] = arc.ArcType;
                Dt.Rows.Add(row);

            }
            return Dt;
        }
        public DataTable ShowResult3()
        {
            Dt = Ds.Tables["Show"];
            Dt.Columns.Add(new DataColumn("编号", typeof(int)));
            Dt.Columns.Add(new DataColumn("运行线代号", typeof(int)));
            Dt.Columns.Add(new DataColumn("车次", typeof(string)));
            Dt.Columns.Add(new DataColumn("车站", typeof(string)));
            Dt.Columns.Add(new DataColumn("时间", typeof(string)));
            Dt.Columns.Add(new DataColumn("点类型", typeof(int)));

            foreach (T_S_Node arc in T_S_NodeList)
            {
                row = Dt.NewRow();
                row["编号"] = arc.ID;
                row["运行线代号"] = arc.LineID;
                row["车站"] = arc.StaCode;
                row["车次"] = arc.TrainCode;
                row["时间"] = arc.TimeCode;
                row["点类型"] = arc.PointType;
                Dt.Rows.Add(row);

            }
            return Dt;
        }
    }
   

}