using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Security.Principal;
using MainServer;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Threading;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Appender;
using log4net.Layout;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Net;
using System.Diagnostics;

namespace XMLStructurizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SynchronizationContext _context;
        DBAccess _db;
        ORA ora = new ORA();
        bool needSort = false;
        private int RequestCounter = 0;
        private string ReqEncData = File.ReadAllText("Test.txt");


        public static String parseXML(String XML)
        {
            String Result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                document.LoadXml(XML);
                writer.Formatting = System.Xml.Formatting.Indented;
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                mStream.Position = 0;
                StreamReader sReader = new StreamReader(mStream);

                String FormattedXML = sReader.ReadToEnd();

                Result = FormattedXML;
            }
            catch (XmlException ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Result;
        }

        public string parseJSON(string xml)
        {
            string result = null;
            try
            {
                JArray o = JArray.Parse(xml);
                result = o.ToString();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {

                if (radioButton1XML.Checked)
                {
                    richTextBox1.Text = parseXML(richTextBox2.Text);
                }

                if (radioButton2JSON.Checked)
                {
                    richTextBox1.Text = parseJSON(richTextBox2.Text);
                }

                if (!(radioButton1XML.Checked || radioButton2JSON.Checked))
                    MessageBox.Show("Choose any mode");

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }


        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void WriteToRichBox(string data)
        {
            _context.Send(state =>
            {
                richTextBox1.Text += data + Environment.NewLine;
            }, null);
        }

        private void WriteToDataGridView(string[,] data)
        {

            _context.Send(state =>
            {

                richTextBox1.Text = string.Join("\n", data) + "\n";

            }, null);
    }

        private void button2_Click(object sender, EventArgs e)
        {

        }


        private void AppendSDRData(string path, string data)
        {
            if (data.EndsWith(Environment.NewLine))
                File.AppendAllText(path, data);
            else
                File.AppendAllText(path, data + Environment.NewLine);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            _context = SynchronizationContext.Current;

            _db = new DBAccess(DBAccess.eType.Oracle);


            Text = _db.Login2SQL("Data source=localhost:1521/orcl;", "rpa", "rpa");

            //Text = _db.Login2SQL("Data source=192.168.1.33;Initial Catalog=Kirill;Connect Timeout=3600;", "SA", "123");
            //Text = ora.Login2SQL("Data source=localhost:1521/orcl;", "ipus", "ipus");
        }

        private List<Thread> lsThread = new List<Thread>();
        public static Object _lock = new Object();
        public static int variable = 0;

        private  void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                new Thread(() =>
              {
                  // Run 30 request through each 5 seconds
                  for (int i = 0; i < 30; i++)
                  {
                      WriteToRichBox(GetHttpRequest("http://10.253.195.21/site/sitexml", "xmldata=" + File.ReadAllText("Test.txt")));

                      Thread.Sleep(5000);
                  }


              }).Start();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        HttpWebRequest HttpWbRqst { get; set; }
        HttpWebResponse HttWbRspn { get; set; }
        //StreamReader strRd { get; set; }
        StringBuilder lol { get; set; }
        Stream stream { get; set; }


        /// <summary>
        /// Make HTTP post request with BODY data
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public string GetHttpRequest(string url, string parameter)
        {
            try
            {
                lol = new StringBuilder();
                HttpWbRqst = (HttpWebRequest)WebRequest.Create(url);

                WriteToRichBox("(HttpWebRequest)WebRequest.Create(url);");
                HttpWbRqst.ContentType = "application/x-www-form-urlencoded";
                HttpWbRqst.Method = "POST";
                HttpWbRqst.Timeout = Timeout.Infinite;
                HttpWbRqst.ReadWriteTimeout = Timeout.Infinite;
                HttpWbRqst.ServicePoint.Expect100Continue = false;
                HttpWbRqst.SendChunked = true;
                WriteToRichBox("SendChunked complete");

                byte[] bytes = System.Text.Encoding.ASCII.GetBytes("xmldata=" + parameter);
                HttpWbRqst.ContentLength = bytes.Length;
                WriteToRichBox("Bytes length : " + bytes.Length);

                using (var os = HttpWbRqst.GetRequestStream())// создаем поток
                {
                    WriteToRichBox("System.IO.Stream os = HttpWbRqst.GetRequestStream(); completed ");
                    WriteToRichBox(" GetRequestStream timeout is : " + os.CanTimeout.ToString());
                    os.Write(bytes, 0, bytes.Length);
                    os.Close();
                }

                HttWbRspn = (HttpWebResponse) HttpWbRqst.GetResponse();
                WriteToRichBox("Ответ от сервера...");
                if (HttWbRspn == null) { throw new Exception("null"); }
                using (stream = HttWbRspn.GetResponseStream())
                {
                    WriteToRichBox("GetResponse выполнился");
                    if (stream == null) { throw new Exception("null stream"); }

                    WriteToRichBox("GetResponseStream1 is : " + stream.CanRead.ToString());

                    byte[] bSTOP = new byte[5];
                    bSTOP[0] = 0;
                    bSTOP[1] = 0;
                    bSTOP[2] = 0;
                    bSTOP[3] = 0;
                    bSTOP[4] = 0;

                    WriteToRichBox("Начало выполнения цикла while");
                    while (true)
                    {
                        bSTOP[0] = bSTOP[1];
                        bSTOP[1] = bSTOP[2];
                        bSTOP[2] = bSTOP[3];
                        bSTOP[3] = bSTOP[4];
                        bSTOP[4] = (byte)stream.ReadByte();
                        if (bSTOP[0] == 100 && bSTOP[1] == 116 && bSTOP[2] == 100 && bSTOP[3] == 34 && bSTOP[4] == 62) //we read header!
                            break;
                    }
                    
                    WriteToRichBox("Конец выполнения цикла while");

                    WriteToRichBox("GetResponseStream2 is : " + stream.CanRead.ToString());

                    using (StreamReader strRd = new StreamReader(stream))
                    {
                        lol.Append(strRd.ReadToEnd());
                    }
                    WriteToRichBox("Конец запроса");
                }

                return lol.ToString();
            }
            catch (Exception ex)
            {
                WriteToRichBox("Exception type handle : " + ex.GetType().Name);
                WriteToRichBox(ex.Message);
                throw new Exception(ex.Message);
            }
            finally
            {
                HttWbRspn.Dispose();
                HttWbRspn.Close();
                
                if (stream != null)
                    stream.Dispose();
               

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            new Thread(() =>
           {
               Dictionary<int, string> dic = new Dictionary<int, string>();


               dic.Add(1,"one");
               dic.Add(2, "two");
               dic.Add(3, "free");
               dic.Add(4, "four");


           }).Start();

        }

        private string ReconnectToOracle(ref ORA myOra)
        {
            Thread.Sleep(5000);
            string status = "";
            try
            {
                WriteToRichBox("Reconnecting to otacle...");

                status = myOra.Login2SQL("Data source=localhost:1521/orcl;", "ipus", "ipus");

                return status;
            }
            catch (Exception ex) { _log.Error(ex.Message); return status; }
        }
    }
        
}
