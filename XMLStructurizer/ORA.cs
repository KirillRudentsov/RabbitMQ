using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Xml;
using System.IO;
//using System.Data.OracleClient;
//using Oracle.DataAccess.Client;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections;
using System.Collections.Generic;


using System.Linq;
using System.Text;

/* ===================================================================
 *		this class used to SQL access and executing queries
 * ===================================================================
*/

namespace MainServer
{
	/// <summary>
	/// Summary description for SQL.
	/// </summary>
	public class ORA
	{
		private string sOracleConnectionString;
		private string sSQLUserName;
		private string sSQLPassword;
		private string sSQLQueryTop;
		private string sSQLQueryAlwaysMask;
		private string sSQLQueryMask;
        private OracleConnection objConn;
		private const string cStateClosed = "closed";
		
		public ORA()
		{
			sOracleConnectionString = "";
			sSQLUserName = "";
			sSQLPassword = "";
			sSQLQueryTop = "0";
			sSQLQueryAlwaysMask = "0";
			sSQLQueryMask = "";
		}

		public string SQLQueryTop
		{
			get {return sSQLQueryTop;}
			set {sSQLQueryTop = value;}
		}

		public string SQLQueryAlwaysMask
		{
			get {return sSQLQueryAlwaysMask;}
			set {sSQLQueryAlwaysMask = value;}
		}

		public string SQLQueryMask
		{
			get {return sSQLQueryMask;}
			set {sSQLQueryMask = value;}
		}

		public string Login2SQL(string sConnectionString,string sUserName,string sPassword)
		{
            objConn = new OracleConnection();
			sSQLUserName = sUserName;
			sSQLPassword = sPassword;
			sOracleConnectionString = sConnectionString;
			objConn.ConnectionString = sConnectionString + "User id= " + sUserName + "; Password=" + sPassword + ";";
			try
			{
				objConn.Open();
				return "OK";
			}
			catch (Exception err)
			{
				objConn.Close();
				return err.Message;
			}
		}

        //public bool OpenConnection(string sConnString)
        //{
        //	string sErrMsg="";
        //	return OpenConnection(sConnString, out sErrMsg);
        //}

        //public bool OpenConnection(string sConnString, out string sErrMsg)
        //{
        //	sErrMsg="";
        //	objConn = new OracleConnection();
        //	sSQLUserName = "";
        //	sSQLPassword = "";
        //	sOracleConnectionString = sConnString;
        //	objConn.ConnectionString = sOracleConnectionString;
        //	try
        //	{
        //		objConn.Open();
        //		return true;
        //	}
        //	catch (Exception err)
        //	{
        //		objConn.Close();
        //		sErrMsg = err.Message;
        //		return false;
        //	}
        //}

        //      public bool OpenConnection(OracleConnection objConnection)
        //{
        //	//return true if success
        //	objConn = objConnection;
        //	sSQLUserName = "";
        //	sSQLPassword = "";
        //	sOracleConnectionString = "";
        //	return true;
        //}

        public void Dispose()
        {
            objConn.Dispose();
        }

		public void Close()
		{
			//close connection....
			objConn.Close();
		}

		public bool CheckConnection()
		{
			return objConn.State.ToString()!=cStateClosed;
		}

		public string Execute2Str(string sSQL)
		{
			return Execute2Str(sSQL, false);
		}

        public string Execute2Str(string sSQL, bool bFunc)
		{
			string sResult;
			if(bFunc)
				sSQL=string.Format("BEGIN {0}; END;", sSQL);

//GAM 2015.04.06
//			try
//			{
				OracleCommand myCMD = new OracleCommand();
				myCMD.Connection = objConn;
				myCMD.CommandText = sSQL;
				OracleDataReader myDataReader;
				myDataReader = myCMD.ExecuteReader();
				sResult = "";
            while (myDataReader.Read())
            {
                for (int i = 0; i < myDataReader.FieldCount; i++)
                {
                    sResult += myDataReader[i];
                }
            }
            

                myDataReader.Close();
/*			}
			catch(Exception err) {
				string s = err.Message;
				sResult="";
			}
 * */
			return sResult;
		}

        public string Execute2StrScalar(string sSQL)
        {
            string sResult = "";
            OracleCommand myCMD = new OracleCommand();
            myCMD.Connection = objConn;
            myCMD.CommandText = sSQL;
            OracleDataReader myDataReader;
            myDataReader = myCMD.ExecuteReader();

            sResult = myDataReader.GetString(0);

            myDataReader.Close();
            /*			}
                        catch(Exception err) {
                            string s = err.Message;
                            sResult="";
                        }
             * */
            return sResult;
        }

        public DataTable Execute2DataTable(string sSQL)
		{
			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader;
			myDataReader = myCMD.ExecuteReader();

			return myDataReader.GetSchemaTable();
		}



        public DataSet Execute2DataSet(string sSQL,string tableName)
		{

			OracleCommand myCMD = new OracleCommand();

			myCMD.Connection = objConn;

            myCMD.CommandText = sSQL;

            OracleDataAdapter myDataAdapter = new OracleDataAdapter(myCMD);


            DataSet myDataSet = new DataSet();

            myDataAdapter.Fill(myDataSet, tableName);

            return myDataSet;
		}

        /*public XmlDocument Execute2XMLSimple(string sSQL)
		{
			return Execute2XMLSimple(sSQL, false);
		}*/

        /*public XmlDocument Execute2XMLSimple(string sSQL, bool bPreserveWhiteSpace)
		{
			try 
			{
				OracleCommand myCMD = new OracleCommand();
				myCMD.Connection = objConn;
				myCMD.CommandText = sSQL;
				XmlReader myReader = myCMD.ExecuteXmlReader();
				XmlDocument xmlResult = new XmlDocument();
				if(bPreserveWhiteSpace)
					xmlResult.PreserveWhitespace = true;
				xmlResult.LoadXml(SaveXml2Str(myReader));
				myReader.Close();

				return xmlResult;
			}
			catch (Exception err)
			{
				string s = err.Message;
				return null;
			}
		}*/

        public XmlDocument Execute2XML(string sSQL)
		{
			return Execute2XML(sSQL, false);
		}

		public XmlDocument Execute2XML(string sQuery, bool bPreserveWhiteSpace)
		{
			XmlDocument xml = null;
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				OracleDataReader dr = new OracleCommand(sQuery, objConn).ExecuteReader();
				sb.Append("<ROOT>");
				do
				{
				while (dr.Read())
				{
					sb.Append("<row");
					for (int i = 0; i < dr.FieldCount; i++)
					{
						sb.Append(string.Format(" {0}='{1}'", dr.GetName(i).ToUpper(), dr[i].ToString().Replace("&","&amp;").Replace("'","&apos;").Replace("<","&lt;").Replace(">","&gt;")));
					}
					sb.Append("/>");
				}
				} 
				while (dr.NextResult());
				
				dr.Close();
				sb.Append("</ROOT>");
				
				xml = new XmlDocument();
				if(bPreserveWhiteSpace)
					xml.PreserveWhitespace = true;
				xml.LoadXml(sb.ToString());
			}
			catch
			{
				xml = null;
			}

			return xml;
		}	

		/*public bool Execute2XML_Async_Init(string sSQL, ref XmlReader myReader)
		{
			try 
			{
				OracleCommand myCMD = new OracleCommand();
				myCMD.Connection = objConn;
				myCMD.CommandText = sSQL;
				myReader = (XmlReader) myCMD.ExecuteReader();
				myReader.Read();
				return true;
			}
			catch (Exception err)
			{
				string s = err.Message;
				return false;
			}
		}*/

		public XmlDocument Execute2XML_Async_Read(ref XmlReader myReader, int nStep, out int nProcessed)
		{
			try 
			{
				XmlDocument xmlResult = new XmlDocument();
				xmlResult.LoadXml(SaveXml2Str(myReader, nStep, out nProcessed));
				return xmlResult;
			}
			catch (Exception err)
			{
				string s = err.Message;
				nProcessed = -1;
				return null;
			}
		}

		public bool Execute2XML_Async_Close(ref XmlReader myReader)
		{
			try 
			{
				myReader.Close();
				return true;
			}
			catch (Exception err)
			{
				string s = err.Message;
				return false;
			}
		}

		/*GAM 2015.04.06
        public void Execute2XmlReader(string sSQL, out XmlReader reader)
		{
			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
            reader = myCMD.ExecuteXmlReader();
		}
         * */

		private string SaveXml2Str(XmlReader xml)
		{
			System.IO.StringWriter w = new System.IO.StringWriter();
			w.WriteLine("<ROOT>");
			xml.Read();
			while (xml.NodeType != XmlNodeType.None)
			{
				w.WriteLine(xml.ReadOuterXml());
			}
			w.Write("</ROOT>");
			xml.Close();
			string sXML = w.ToString();
			w.Close();
			return sXML;
		}

		private string SaveXml2Str(XmlReader xml, int nStep, out int nProcessed)
		{
			nProcessed = 0;
			System.IO.StringWriter w = new System.IO.StringWriter();
			w.WriteLine("<ROOT>");
			while (xml.NodeType != XmlNodeType.None && nProcessed < nStep)
			{
				w.WriteLine(xml.ReadOuterXml());
				nProcessed++;
			}
			w.Write("</ROOT>");
			string sXML = w.ToString();
			w.Close();
			return sXML;
		}

		public int Execute2Int(string sSQL)
		{
			return Execute2Int(sSQL, false);
		}

		public int Execute2Int(string sSQL, bool bFunc)
		{
			if(bFunc)
				sSQL=string.Format("BEGIN {0}; END;", sSQL);
			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			int iResult = -1;
			try
			{
				iResult = int.Parse(myCMD.ExecuteScalar().ToString());
			}
			catch(Exception e){
				string sErr = e.Message;
			}

			return iResult;
		}

		public void Execute(string sSQL)
		{
			Execute(sSQL, false);
		}
		
		public void Execute(string sSQL, bool bProc)
		{
			if(bProc)
				sSQL=string.Format("BEGIN {0}; END;", sSQL);
			
			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			myCMD.ExecuteNonQuery();
			
		}


		public void Execute2StrStream(string sQuery, out string sResult)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			OracleDataReader dr = new OracleCommand(sQuery, objConn).ExecuteReader();
			sb.Append("<ROOT>");
			do
			{
			while (dr.Read())
			{
				for (int i = 0; i < dr.FieldCount; i++)
				{
					sb.Append(dr[i]);
				}
			}
			} while (dr.NextResult());
			dr.Close();
			sb.Append("</ROOT>");
			sResult = sb.ToString();
		}

		public string Date2SQLShort(string sDate)
		{
			//convert date format to SQL date format...
			DateTime dt = DateTime.Parse(sDate);
			return dt.ToString("yyyy-MM-dd");
		}

		public string Execute2StrStream(string sQuery)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			OracleDataReader dr = new OracleCommand(sQuery, objConn).ExecuteReader();
			sb.Append("<ROOT>");
			do
			{
			while (dr.Read())
			{
				for (int i = 0; i < dr.FieldCount; i++)
				{
					sb.Append(dr[i]);
				}
			}
			} while (dr.NextResult());
			dr.Close();
			sb.Append("</ROOT>");
			return sb.ToString();
		}

		public string Execute2StrStream2(string sQuery)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			OracleDataReader dr = new OracleCommand(sQuery, objConn).ExecuteReader();
			do
			{
			while (dr.Read())
			{
				for (int i = 0; i < dr.FieldCount; i++)
				{
					sb.Append(dr[i]);
				}
			}
			} while (dr.NextResult());
			dr.Close();
			return sb.ToString();
		}

        public OracleConnection GetOracleConnection()
		{
			if(!CheckConnection())
				objConn = null;
			return objConn;
		}

        public string OpenConnection(OracleConnection objConnection)
        {
            string sRes = "";

            try
            {
                objConnection.Open();
                sRes = "OK";
            }
            catch (Exception ex) { sRes = ex.Message; }

            return sRes;
        }

        public int[,] Execute2IntArray(string sSQL)
		{
			///возвращает массив int
			///проверки отсутствуют
			///
			int nArrayPart = 1500;

			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader;
			myDataReader = myCMD.ExecuteReader();

			int nFieldCount = myDataReader.FieldCount;
			int nMaxCount = nArrayPart;
			int[,] nResult = new int[nFieldCount,nMaxCount];
			int nIndex=0;
			while (myDataReader.Read())
			{
				for (int i = 0; i < myDataReader.FieldCount; i++)
				{
					int nValue = int.Parse(myDataReader[i].ToString());
					nResult[i,nIndex] = nValue;
				}
				nIndex++;
				if(nIndex==nMaxCount)
				{
					//пересчет размерности массива...
					int[,] nStore = new int[nFieldCount,nMaxCount];
					for(int nStroreIndex =0;nStroreIndex<nMaxCount;nStroreIndex++)
					{
						for(int nFieldIndex=0;nFieldIndex<nFieldCount;nFieldIndex++)
						{
							nStore[nFieldIndex,nStroreIndex] = nResult[nFieldIndex,nStroreIndex];
						}
					}
					nMaxCount += nArrayPart;
					nResult = new int[nFieldCount,nMaxCount];
					for(int nStroreIndex =0;nStroreIndex<(nMaxCount-nArrayPart);nStroreIndex++)
					{
						for(int nFieldIndex=0;nFieldIndex<nFieldCount;nFieldIndex++)
						{
							nResult[nFieldIndex,nStroreIndex] = nStore[nFieldIndex,nStroreIndex];
						}
					}
				}
			}
			
			//конечный пересчет массива...
			int[,] nStore2 = new int[nFieldCount,nIndex];
			for(int nStroreIndex =0;nStroreIndex<nIndex;nStroreIndex++)
			{
				for(int nFieldIndex=0;nFieldIndex<nFieldCount;nFieldIndex++)
				{
					nStore2[nFieldIndex,nStroreIndex] = nResult[nFieldIndex,nStroreIndex];
				}
			}

			myDataReader.Close();
			return nStore2;
		}

		public string[,] Execute2StrArrayWithFiledName(string sSQL)
		{
			///возвращает массив int
			///проверки отсутствуют
			///
			int nArrayPart = 1500;

			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader = myCMD.ExecuteReader();

            int nFieldCount = myDataReader.FieldCount;
			int nMaxCount = nArrayPart;
			string[,] sResult = new string[nFieldCount,nMaxCount];
			int nIndex = 0;

            for (int j = 0; j < myDataReader.FieldCount; j++)
            {
                sResult[j, nIndex] = myDataReader.GetName(j).ToString();
            }
            nIndex++;

            while (myDataReader.Read())
            {
                for (int i = 0; i < myDataReader.FieldCount; i++)
                {
                        sResult[i, nIndex] = myDataReader.GetValue(i).ToString();
                }

                nIndex++;
                if (nIndex == nMaxCount)
                {
                    //пересчет размерности массива...
                    string[,] sStore = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < nMaxCount; nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                            sStore[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                    }
                    nMaxCount += nArrayPart;
                    sResult = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < (nMaxCount - nArrayPart); nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                        {
                            sResult[nFieldIndex, nStroreIndex] = sStore[nFieldIndex, nStroreIndex];
                        }
                    }
                }
            }

                //конечный пересчет массива...
                string[,] sStore2 = new string[nFieldCount,nIndex];
            for (int nStroreIndex = 0; nStroreIndex < nIndex; nStroreIndex++)
            {
                for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                {
                    sStore2[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                }
            }
            myDataReader.Close();
			return sStore2;
		}

        public string[,] Execute2StrArrayWithoutFiledName(string sSQL)
        {
            ///возвращает массив int
            ///проверки отсутствуют
            ///
            int nArrayPart = 1500;

            OracleCommand myCMD = new OracleCommand();
            myCMD.Connection = objConn;
            myCMD.CommandText = sSQL;
            OracleDataReader myDataReader = myCMD.ExecuteReader();

            int nFieldCount = myDataReader.FieldCount;
            int nMaxCount = nArrayPart;
            string[,] sResult = new string[nFieldCount, nMaxCount];
            int nIndex = 0;

            while (myDataReader.Read())
            {
                for (int i = 0; i < myDataReader.FieldCount; i++)
                {
                    sResult[i, nIndex] = myDataReader.GetValue(i).ToString();
                }

                nIndex++;
                if (nIndex == nMaxCount)
                {
                    //пересчет размерности массива...
                    string[,] sStore = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < nMaxCount; nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                            sStore[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                    }
                    nMaxCount += nArrayPart;
                    sResult = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < (nMaxCount - nArrayPart); nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                        {
                            sResult[nFieldIndex, nStroreIndex] = sStore[nFieldIndex, nStroreIndex];
                        }
                    }
                }
            }

            //конечный пересчет массива...
            string[,] sStore2 = new string[nFieldCount, nIndex];
            for (int nStroreIndex = 0; nStroreIndex < nIndex; nStroreIndex++)
            {
                for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                {
                    sStore2[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                }
            }
            myDataReader.Close();
            return sStore2;
        }

        public void FillData(string sSQL, int nColumns, int nRows2Show, out string[,] data)
		{
            if (nColumns + 1 < 0 || nRows2Show < 0)
            {
                data = null;
                return;
            }

			data = new string[nColumns+1, nRows2Show];

			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader;
			myDataReader = myCMD.ExecuteReader();
			string sResult = "";
			int j=0;
			while (myDataReader.Read())
			{
				for (int i =0;i<myDataReader.FieldCount;i++)
				{
					sResult = myDataReader[i].ToString();
					data[i,j] = sResult.Replace("\0","");
				}
				j++;
			}
			if ((j-1)<nRows2Show)
			{
				//case we must add NEW rows...
				for (int k=j;k<nRows2Show;k++)
				{
					for (int i=1;i<=nColumns;i++)
					{
						data[0,k]="-1";
						data[i,k]="";
					}
				}
			}
			myDataReader.Close();
		}

		public void FillDataAll(string sSQL, int nColumns, int nRows, out string[,] data)
		{
            if (nColumns + 1 < 0 || nRows < 0)
            {
                data = null;
                return;
            }

            data = new string[nColumns+1, nRows];

			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader;
			myDataReader = myCMD.ExecuteReader();
			string sResult = "";
			int j = 0;
			while (myDataReader.Read())
			{
				for (int i = 0; i < myDataReader.FieldCount;i++)
				{
					sResult = myDataReader[i].ToString();
					data[i,j] = sResult.Replace("\0","");
				}
				j++;
			}
			myDataReader.Close();
		}

		public void Execute2IntArray(string sSQL, int nRows, out int[] data)
		{
			data = new int[nRows];
			OracleCommand myCMD = new OracleCommand();
			myCMD.Connection = objConn;
			myCMD.CommandText = sSQL;
			OracleDataReader myDataReader;
			myDataReader = myCMD.ExecuteReader();
			int j = 0;
			while (myDataReader.Read())
			{
				data[j] = (int) myDataReader[0];
				j++;
			}
			myDataReader.Close();		
		}

		private string GetValueFromDataReader(object Data)
		{
			string sValue = "";
			System.Type myDataType = Data.GetType();
			switch (myDataType.FullName)
			{
				case "System.String":
					sValue = (string) Data;
					break;
				case "System.Int32":
					sValue = ((int) Data).ToString();
					break;
			}
			return sValue;
		}
        //last update from koch
        public string Execute2Str(string sSQL, bool bFunc, out string sErrMsg)
        {
            string sResult;
            sErrMsg = "";
            if (bFunc)
                sSQL = string.Format("BEGIN {0}; END;", sSQL);

            try
            {
                OracleCommand myCMD = new OracleCommand();
                myCMD.Connection = objConn;
                myCMD.CommandText = sSQL;
                OracleDataReader myDataReader;
                myDataReader = myCMD.ExecuteReader();
                sResult = "";
                while (myDataReader.Read())
                {
                    for (int i = 0; i < myDataReader.FieldCount; i++)
                    {
                        sResult += myDataReader[i];
                    }
                }
                myDataReader.Close();
            }
            catch (Exception err)
            {
                sErrMsg = err.Message;
                sResult = "";
            }
            return sResult;
        }
        public int Execute2Int(string sSQL, bool bFunc, out string sErrMsg)
        {
            sErrMsg = "";
            if (bFunc)
                sSQL = string.Format("BEGIN {0}; END;", sSQL);
            OracleCommand myCMD = new OracleCommand();
            myCMD.Connection = objConn;
            myCMD.CommandText = sSQL;
            int iResult = -1;
            try
            {
                iResult = int.Parse(myCMD.ExecuteScalar().ToString());
            }
            catch (Exception e)
            {
                sErrMsg = e.Message;
            }

            return iResult;
        }
        public void Execute(string sSQL, bool bProc, out string sErrMsg)
        {
            sErrMsg = "";
            if (bProc)
                sSQL = string.Format("BEGIN {0}; END;", sSQL);

            try
            {
                OracleCommand myCMD = new OracleCommand();
                myCMD.Connection = objConn;
                myCMD.CommandText = sSQL;
                myCMD.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                sErrMsg = e.Message;
            }
        }
        public DataSet Execute2DataSet(string sSQL, out string sErrMsg)
        {
            sErrMsg = "";
            DataSet myDataSet = new DataSet();

            try
            {
                OracleCommand myCMD = new OracleCommand();
                myCMD.Connection = objConn;
                myCMD.CommandText = sSQL;
                OracleDataAdapter myDataAdapter = new OracleDataAdapter(myCMD);
                myDataAdapter.Fill(myDataSet, "TABLE");
            }
            catch (Exception er)
            {
                sErrMsg = er.Message;
            }
            return myDataSet;
        }

        /// <summary>
        /// Call Strored Procedure
        /// </summary>
        /// <param name="sProcName"></param>
        /// <param name="listParam"></param>
        /// <returns>OK</returns>
        public string ProcCall(string sProcName, List<ProcedureParameter> listParam)
        {

            OracleCommand cmd = new OracleCommand(sProcName, objConn);
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (var param in listParam)
            {
                cmd.Parameters.Add(GetOracleParameter(param.ParameterName, param.ParameterValue, param.ParameterType, param.ParameterDirection));
            }

            cmd.ExecuteNonQuery();

            return "OK";
        }


        /// <summary>
        /// Call Strored Procedure. Rerurn OracleDataReader
        /// </summary>
        /// <param name="sProcName"></param>
        /// <param name="listParam"></param>
        /// <returns></returns>
        public string[,] ProcCallCursor(string sProcName, List<ProcedureParameter> listParam)
        {
            string cursorName = "";

            OracleCommand cmd = new OracleCommand(sProcName, objConn);
            cmd.CommandType = CommandType.StoredProcedure;

            var p = from param in listParam
                         where param.ParameterType == "REFCURSOR"
                         select param;

            /// By default cursor name is ref_cursor
            cursorName = string.IsNullOrEmpty(p.FirstOrDefault().ParameterName) ? "ref_cursor" : p.FirstOrDefault().ParameterName;


            foreach (var param in listParam)
            {
                cmd.Parameters.Add(GetOracleParameter(param.ParameterName, param.ParameterValue, param.ParameterType, param.ParameterDirection));
            }

            cmd.ExecuteNonQuery();

            var cursor_ = cmd.Parameters[cursorName].Value as OracleRefCursor;

            var myDataReader = cursor_.GetDataReader();
            int nArrayPart = 1500;
            int nFieldCount = myDataReader.FieldCount;
            int nMaxCount = nArrayPart;
            string[,] sResult = new string[nFieldCount, nMaxCount];
            int nIndex = 0;

            while (myDataReader.Read())
            {
                for (int i = 0; i < myDataReader.FieldCount; i++)
                {
                    sResult[i, nIndex] = myDataReader.GetValue(i).ToString();
                }

                nIndex++;
                if (nIndex == nMaxCount)
                {
                    //пересчет размерности массива...
                    string[,] sStore = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < nMaxCount; nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                            sStore[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                    }
                    nMaxCount += nArrayPart;
                    sResult = new string[nFieldCount, nMaxCount];
                    for (int nStroreIndex = 0; nStroreIndex < (nMaxCount - nArrayPart); nStroreIndex++)
                    {
                        for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                        {
                            sResult[nFieldIndex, nStroreIndex] = sStore[nFieldIndex, nStroreIndex];
                        }
                    }
                }
            }

            //конечный пересчет массива...
            string[,] sStore2 = new string[nFieldCount, nIndex];
            for (int nStroreIndex = 0; nStroreIndex < nIndex; nStroreIndex++)
            {
                for (int nFieldIndex = 0; nFieldIndex < nFieldCount; nFieldIndex++)
                {
                    sStore2[nFieldIndex, nStroreIndex] = sResult[nFieldIndex, nStroreIndex];
                }
            }
            myDataReader.Close();
            return sStore2;
        }

        public ParameterDirection GetParamDir(string strDir)
        {
            ParameterDirection param = ParameterDirection.Input; // By Default

            if (strDir.ToUpper() == "IN")
                param =  ParameterDirection.Input;
            if (strDir.ToUpper() == "OUT")
                param =  ParameterDirection.Output;
            if (strDir.ToUpper() == "IN/OUT")
                param = ParameterDirection.InputOutput;
            if (strDir.ToUpper() == "RETURNVALUE")
                param =  ParameterDirection.ReturnValue;

            return param;
        }

        public OracleParameter GetOracleParameter(string sParameterName, string sParameterValue, string sType, string dir)
        {
            OracleParameter orclCONN = null;
            ParameterDirection p_dir = GetParamDir(dir);


            switch (sType.ToUpper())
            {
                case "VARCHAR":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Varchar2,
                                                 sParameterValue,
                                                 p_dir);
                    break;
                case "NUMBER":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Int32,
                                                 int.Parse(sParameterValue),
                                                 p_dir);
                    break;
                case "CHAR":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Char,
                                                 sParameterValue,
                                                 p_dir);
                    break;
                case "DATE":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Date,
                                                 sParameterValue,
                                                 p_dir);
                    break;
                case "XMLTYPE":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.XmlType,
                                                 sParameterValue,
                                                 p_dir);
                    break;
                case "DOUBLE":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Double,
                                                 double.Parse(sParameterValue),
                                                 p_dir);
                    break;
                case "INT64":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Int64,
                                                 Int64.Parse(sParameterValue),
                                                 p_dir);
                    break;
                case "CLOB":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.Clob,
                                                 sParameterValue,
                                                 p_dir);
                    break;
                case "REFCURSOR":
                    orclCONN = new OracleParameter(sParameterName.ToUpper(),
                                                 OracleDbType.RefCursor,
                                                 p_dir);
                    break;
            }

            return orclCONN;
        }

    }
}
