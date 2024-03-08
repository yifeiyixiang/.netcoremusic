using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Data;
using System.Data.SQLite;
using System.IO;

using Microsoft.Extensions.Logging;
namespace diary.Controllers
{
    public class SqlLiteHelper
    {
      
        private readonly ILogger<SqlLiteHelper> _logger;
         

        string path = AppDomain.CurrentDomain.BaseDirectory;
        string dbPath;
        string conStr = "Data Source=E:\\thumbnail.db";
        static SqlLiteHelper _instance;
        public static SqlLiteHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SqlLiteHelper();
                return _instance;
            }
        }
        public  string mydbPath;
        public  string musicPath;
        public  string myconStr;
        public  string dirdbPath;
        string cmdCreateTable = "CREATE TABLE IF NOT EXISTS diary (Id INTEGER PRIMARY KEY AUTOINCREMENT NULL, UserName nvarchar, Password nvarchar);";
        string createIndex = "create index if not exists originIndex on diary (OrginFilePath)";
        public SqlLiteHelper() {
            //_logger.LogInformation("This is a log message");
            dbPath = path + "diary.db";
          conStr = "Data Source=" + dbPath; 
            Console.WriteLine("This is dbPath ||" + dbPath);
            Console.WriteLine("\nThis is conStr|| " + conStr);
            mydbPath = dbPath ;
            myconStr = conStr;
            dirdbPath = path;
            musicPath = path + "music\\";

        }

        //如果需要初始化数据库可调用此方法
        public void Init()
        {
            if (!File.Exists(dbPath))
            {
                //如果数据库文件不存在，则创建
                SQLiteConnection.CreateFile(dbPath);
            }
            List<TransModel> models = new List<TransModel>();
            models.Add(new TransModel { CmdText = cmdCreateTable });
            models.Add(new TransModel { CmdText = createIndex });
            bool res = ExecTransaction(models);//一个事务：如果表不存在则创建，如果索引不存在则创建
        }
        //getGetName
       
        //执行非查询的sql语句，返回受影响的行数
        public int ExecuteNonQuery(string cmdText, params SQLiteParameter[] paramters)
        {
            using (SQLiteConnection con = new SQLiteConnection(conStr))
            {
                try
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        cmd.CommandText = cmdText;
                        if (paramters != null)
                            cmd.Parameters.AddRange(paramters);
                        return cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    //_log.E(ex);
                }
                return -1;
            }
        }
        //执行非查询的sql语句，返回第一行第一列的值
        public object ExecuteScalar(string cmdText, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(conStr))
            {
                try
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        return cmd.ExecuteScalar();
                    }
                }
                catch (SQLiteException ex)
                {
                    //_log.E(ex);
                }
                return null;
            }
        }
        //执行查询语句，返回查询到的结果  DataAdapter：用来执行查询sql，一次性读取数据并加载到内存里，通过fill方法填充到DataTable里，查询量太多时可能会OOM
        public DataTable GetDataTable(string cmdText, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection(conStr))
            {
                try
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        DataTable dt = new DataTable();
                        cmd.CommandText = cmdText;
                        cmd.Parameters.AddRange(parameters);
                        SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                        da.Fill(dt);
                        return dt;
                    }
                }
                catch (SQLiteException ex)
                {
                    //_log.E(ex);

                }
                return null;
            }
        }
        //执行事务，如果出现异常则回滚
        public bool ExecTransaction(List<TransModel> models)
        {
            using (SQLiteConnection con = new SQLiteConnection(conStr))
            {
                try
                {
                    con.Open();
                    using (SQLiteTransaction trans = con.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            cmd.Transaction = trans;
                            try
                            {
                                foreach (var model in models)
                                {
                                    cmd.CommandText = model.CmdText;
                                    if (model.Paras != null)
                                        cmd.Parameters.AddRange(model.Paras);
                                    cmd.ExecuteNonQuery();
                                }
                                trans.Commit();
                                return true;
                            }
                            catch (SQLiteException ex)
                            {
                                trans.Rollback();
                                //_log.E(ex);
                            }

                        }
                    }

                }
                catch (SQLiteException ex)
                {
                    // _log.E(ex);
                }
            }
            return false;
        }
    }
    public class TransModel
    {
        public string CmdText { get; set; }
        public SQLiteParameter[] Paras { get; set; }  
    }
}
