using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;


namespace HttpServer
{
    public class Server
    {
        private int port;
        private Thread serverThread;
        private string directory;
        private HttpListener httpListener;
        private static string FilePath;
        private const string Folder = "/ServerFolder";
        public string LocalHost;

        public int Port
        {
            get
            {
                return port;
            }
            private set { }
        }


        public Server(int port, string filePath)
        {

            ConfigureServerParameters(port);
            GetFile(filePath);
        }

        public Server() { }



        public void ListenToPort()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:"+port.ToString()+"/sort/");
            httpListener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = httpListener.GetContext();
                    ProcessRequest(context, FilePath);
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine(ex.Message);

                }
            }

        }

        private void ConfigureServerParameters(int port)
        {
            this.port = port;
            directory = Environment.CurrentDirectory + Folder;
            serverThread = new Thread(ListenToPort);
            serverThread.Start();
        }



        public void GetFile(string filePath)
        {
                FilePath = filePath;
        } 



        private void Stop()
        {
            httpListener.Stop();
            serverThread.Abort();
        }


        // directory + column + ".csv"
        void WriteByteToCSVFile(string filePath, byte[] csvData)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(csvData);
                bw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }           
        }



        private void ProcessRequest(HttpListenerContext context, string file)
        {
            string requestString = context.Request.Url.ToString();
            var stringUrl = requestString.Substring(7);
            var UrlArray = stringUrl.Split('/');
            LocalHost = UrlArray[0];
            Console.WriteLine("Listening to:{0}", LocalHost);
            if (String.IsNullOrEmpty(requestString))
                throw new NullReferenceException("requestString is null");
            var queryParam = context.Request.Url.Query.Substring(1);
            GetFile(file);
            if (String.IsNullOrEmpty(FilePath))
                throw new NullReferenceException("filepath is null");
            var bytes = File.ReadAllBytes(FilePath); 
            var webResponse = Post.PostData("http://localhost", bytes);
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string postedData = responseReader.ReadToEnd();
            var PostedDataByte = Encoding.ASCII.GetBytes(postedData);
            if (!File.Exists(@queryParam + ".csv"))
                WriteByteToCSVFile(@queryParam + ".csv", PostedDataByte);
            var csvLines = ProcessCSVFile.SortCSVFile(@queryParam + ".csv", queryParam);
            var csvString = ProcessCSVFile.ListToCSVString(csvLines, ProcessCSVFile.Delimiter);
            var csvBytes = Encoding.ASCII.GetBytes(csvString);
            if (File.Exists(@queryParam + ".csv") && !String.IsNullOrEmpty(csvString))
            {
                try
                {
                    context.Response.ContentType = "text/csv";
                    context.Response.ContentLength64 = csvBytes.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(@queryParam + ".csv").ToString("r"));
                    context.Response.OutputStream.Write(csvBytes, 0, csvBytes.Length);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            context.Response.OutputStream.Close();
        }

    }
}
