using System;
using System.Net;
namespace RPCClient
{
    public class NetListener
    {
        public static void Listener1(string[] prefixes)
        {                        
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");           
            HttpListener listener = new HttpListener();            
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }            
            listener.Start();
            Console.WriteLine("Listener Escutando...");            
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;            
            HttpListenerResponse response = context.Response;            
            string responseString = "Ok";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);            
            output.Close();            
            listener.Stop();
        }
    }
}