using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using ProducerMQ;
using System.Text;

namespace GuiTesterWebServiceManager
{
    public class Db
    {
        static void InitDb(string connStr)
        {
            _connStr = connStr;
        }

        private static string _connStr;

        public static string GetListData(string command)
        {
            InitDb(Config.ConnSTR);
            var resSTRBuleder = new StringBuilder();
            var result = -1;
            using (var conn = new OracleConnection(_connStr))
            {
                conn.Open();
                var cmd = new OracleCommand();
                cmd.CommandText = command;
                cmd.Connection = conn;

                var myDataReader = cmd.ExecuteReader();

                while (myDataReader.Read())
                {
                    for (int i = 0; i < myDataReader.FieldCount; i++)
                    {
                        resSTRBuleder.Append(myDataReader[i] + ",");
                    }
                }

            }

            return resSTRBuleder.ToString().Substring(0,resSTRBuleder.Length-1);
        }


    }
}