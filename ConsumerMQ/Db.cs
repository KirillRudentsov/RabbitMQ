using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Text;

namespace ConsumerMQ
{
    public class Db
    {
        static void InitDb(string connStr)
        {
            _connStr = connStr;
        }

        private static string _connStr;

        public static void start_exec_cmd(int commandId,int robotId)
        {
            InitDb(Config.ConnSTR);
            var result = -1;
            using (var conn = new OracleConnection(_connStr))
            {
                conn.Open();
                var cmd = new OracleCommand();
                cmd.Connection = conn;
                cmd.InitialLONGFetchSize = 1000;
                cmd.CommandText = "start_exec_cmd";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("nCommandID", OracleDbType.Int32).Value = commandId;
                cmd.Parameters.Add("nRobotId", OracleDbType.Int32).Value = robotId;

                cmd.ExecuteNonQuery();
            }
        }


    }
}