using System;
using System.Data;
using System.Data.Common;
using System.Xml;
using System.Collections.Generic;


/* ===================================================================
 *		this class used to database access and executing queries
 * ===================================================================
*/

namespace MainServer
{
    public class DBAccess
    {
        private SQL objSQL = null;
        private ORA objORA = null;
        private bool bMSSQL;

        public enum eType
        {
            MSSQL = 0,
            Oracle = 1
        };

        public DBAccess(eType type)
        {
            bMSSQL = (type == eType.MSSQL) ? true : false;

            if (bMSSQL)
                objSQL = new SQL();
            else
                objORA = new ORA();
        }

        public eType DbType
        {
            get { return bMSSQL ? eType.MSSQL : eType.Oracle; }
            set
            {
                eType currtype = bMSSQL ? eType.MSSQL : eType.Oracle;
                if (currtype != value)
                    Close();
                bMSSQL = (value == eType.MSSQL ? true : false);
            }
        }

        public string Login2SQL(string sConnectionString, string sUserName, string sPassword)
        {
            return bMSSQL ? objSQL.Login2SQL(sConnectionString, sUserName, sPassword) :
                objORA.Login2SQL(sConnectionString, sUserName, sPassword);
        }

        public static eType GetDbTypeByName(string sDBType)
        {
            return sDBType.ToUpper() == "MSSQL" ? eType.MSSQL : DBAccess.eType.Oracle;
        }

        /*
		public bool OpenConnection(string sConnString)
		{
			return bMSSQL ? objSQL.OpenConnection(sConnString):
				objORA.OpenConnection(sConnString);
		}
        */

        public void Close()
        {
            if (bMSSQL)
                objSQL.Close();
            else
                objORA.Close();
        }

        public bool CheckConnection()
        {
            return bMSSQL ? objSQL.CheckConnection() : objORA.CheckConnection();
        }

        public string Execute2Str(string sSQL)
        {
            return Execute2Str(sSQL, false);
        }

        public string Execute2Str(string sSQL, bool bFunc)
        {
            return bMSSQL ? objSQL.Execute2Str(sSQL, bFunc) : objORA.Execute2Str(sSQL, bFunc);
        }

        public DataTable Execute2DataTable(string sSQL)
        {
            return bMSSQL ? objSQL.Execute2DataTable(sSQL) : objORA.Execute2DataTable(sSQL);
        }

        public DataSet Execute2DataSet(string sSQL, string tableName)
        {
            return bMSSQL ? objSQL.Execute2DataSet(sSQL, tableName) : objORA.Execute2DataSet(sSQL, tableName);
        }

        public XmlDocument Execute2XML(string sSQL, bool bPreserveWhiteSpace)
        {
            return bMSSQL ? objSQL.Execute2XML(sSQL, bPreserveWhiteSpace) :
                objORA.Execute2XML(sSQL, bPreserveWhiteSpace);
        }

        public XmlDocument Execute2XML(string sSQL)
        {
            return bMSSQL ? objSQL.Execute2XML(sSQL) : objORA.Execute2XML(sSQL);
        }

        public int Execute2Int(string sSQL)
        {
            return Execute2Int(sSQL, false);
        }

        public int Execute2Int(string sSQL, bool bFunc)
        {
            return bMSSQL ? objSQL.Execute2Int(sSQL, bFunc) : objORA.Execute2Int(sSQL, bFunc);
        }

        public string[,] Execute2StrArrayWithoutFiledName(string sSQL)
        {
            return bMSSQL ? objSQL.Execute2StrArrayWithoutFiledName(sSQL) : objORA.Execute2StrArrayWithoutFiledName(sSQL);
        }

        public string[,] Execute2StrArrayWithFiledName(string sSQL)
        {
            return bMSSQL ? objSQL.Execute2StrArrayWithFiledName(sSQL) : objORA.Execute2StrArrayWithFiledName(sSQL);
        }

        public void Execute2IntArray(string sSQL, int nRows, out int[] data)
        {
            if (bMSSQL)
                objSQL.Execute2IntArray(sSQL, nRows, out data);
            else
                objORA.Execute2IntArray(sSQL, nRows, out data);
        }

        public void Execute(string sSQL)
        {
            Execute(sSQL, false);
        }

        public void Execute(string sSQL, bool bProc)
        {
            if (bMSSQL)
                objSQL.Execute(sSQL, bProc);
            else
                if (bProc)
                objORA.Execute(sSQL, bProc);
            else
                objORA.Execute(sSQL);
        }

        public int[,] Execute2IntArray(string sSQL)
        {
            return bMSSQL ? objSQL.Execute2IntArray(sSQL) : objORA.Execute2IntArray(sSQL);
        }

        public void FillData(string sSQL, int nColumns, int nRows, out string[,] data)
        {
            if (bMSSQL)
                objSQL.FillData(sSQL, nColumns, nRows, out data);
            else
                objORA.FillData(sSQL, nColumns, nRows, out data);
        }

        public void FillDataAll(string sSQL, int nColumns, int nRows, out string[,] data)
        {
            if (bMSSQL)
                objSQL.FillDataAll(sSQL, nColumns, nRows, out data);
            else
                objORA.FillDataAll(sSQL, nColumns, nRows, out data);
        }
        /*GAM 2015.04.06
		public void Execute2XmlReader(string sSQL, out XmlReader reader)
		{
			if(bMSSQL)
				objSQL.Execute2XmlReader(sSQL, out reader);
			else
				objORA.Execute2XmlReader(sSQL, out reader);
		}
         * */

        //last update from koch
        public void Execute(string sSQL, bool bProc, out string sErrMsg)
        {
            sErrMsg = "";
            if (bMSSQL)
                objSQL.Execute(sSQL, bProc);
            else
                if (bProc)
                objORA.Execute(sSQL, bProc, out sErrMsg);
            else
                objORA.Execute(sSQL, false, out sErrMsg);
        }
        public int Execute2Int(string sSQL, bool bFunc, out string sErrMsg)
        {
            sErrMsg = "";
            return bMSSQL ? objSQL.Execute2Int(sSQL, bFunc) : objORA.Execute2Int(sSQL, bFunc, out sErrMsg);
        }

        public string Execute2Str(string sSQL, bool bFunc, out string sErrMsg)
        {
            sErrMsg = "";
            return bMSSQL ? objSQL.Execute2Str(sSQL, bFunc) : objORA.Execute2Str(sSQL, bFunc, out sErrMsg);
        }

        public string ProcCall(string sProcName, List<ProcedureParameter> listParam)
        {
            return bMSSQL ? objSQL.ProcCall(sProcName, listParam) : objORA.ProcCall(sProcName, listParam);
        }

        public string[,] ProcCallCursor(string sProcName, List<ProcedureParameter> listParam)
        {
            return bMSSQL ? objSQL.ProcCallCursor(sProcName, listParam) : objORA.ProcCallCursor(sProcName, listParam);
        }


    }

    
    public class ProcedureParameter
    {
        public string ParameterName;
        public string ParameterValue;
        public string ParameterType;
        public string ParameterDirection;

        public ProcedureParameter() { }

        public ProcedureParameter(string paramName, string paramVal, string paramType, string paramDir)
        {
            ParameterName = paramName;
            ParameterValue = paramVal;
            ParameterType = paramType;
            ParameterDirection = paramDir;
        }
    }
}
