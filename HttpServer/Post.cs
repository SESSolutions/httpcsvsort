using System;
using System.IO;
using System.Net;


namespace HttpServer
{
    static public class Post
    {

        public static HttpWebResponse PostData(string url, byte[] csvData)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            if (request == null)
                throw new NullReferenceException("request is not a http request");
            request.Method = "POST";
            request.ContentType = "text/csv";
            request.UserAgent = "Anybody";
            request.ContentLength = csvData.Length;

            
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(csvData, 0, csvData.Length);
                requestStream.Close();
            }

            return request.GetResponse() as HttpWebResponse;
        }
    }
}
