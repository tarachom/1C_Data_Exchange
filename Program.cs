using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Net;
using System.Web;

namespace _1C_Data_Exchange
{
	class Program
	{
		public static string pathSaveXMLCommand { get; set; }
		public static string pathReadXMLResult { get; set; }

		public static object locker;

		public static List<HttpListenerContext> list { get; set; }



		static void Main(string[] args)
		{
			pathSaveXMLCommand = @"C:\1C_XML\Input\";
			pathReadXMLResult = @"C:\1C_XML\Output\";

			list = new List<HttpListenerContext>();

			Thread threadWebServer = new Thread(new ThreadStart(WebServer));
			threadWebServer.Start();

			Thread thread = new Thread(new ThreadStart(Test));
			thread.Start();

			
		}

		static void WebServer()
		{
			HttpListener listener = new HttpListener();

			listener.Prefixes.Add("http://localhost:5455/");

			listener.Start();
			Console.WriteLine("Listening...");

			while (true)
			{
				HttpListenerContext context = listener.GetContext();

				Console.WriteLine("Add list context");

				lock (list)
				{
					list.Add(context);
				}
			}

			listener.Stop();
		}

		static void Test()
		{
			while (true)
			{
				HttpListenerContext context = null;

				lock (list)
				{
					if (list.Count > 0)
					{
						context = list[0];
						list.RemoveAt(0);
					}
				}

				if (context != null)
				{
					HttpListenerRequest request = context.Request;
					
					foreach (string key in request.QueryString.AllKeys)
					{
						Console.WriteLine(key + " = " + request.QueryString[key].ToString());
					}					

					if (request.HttpMethod == "POST")
					{
						string documentContents;
						using (Stream receiveStream = request.InputStream)
						{
							using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
							{
								documentContents = readStream.ReadToEnd();
							}
						}
						Console.WriteLine($"Recived request for {request.Url}");
						Console.WriteLine(documentContents);

						Dictionary<string, string> postParams = new Dictionary<string, string>();
						string[] rawParams = documentContents.Split('&');
						foreach (string param in rawParams)
						{
							string[] kvPair = param.Split('=');
							string key = kvPair[0];
							string value =  HttpUtility.UrlDecode(kvPair[1]);
							postParams.Add(key, value);
							Console.WriteLine(key + " = " + value);
						}
					}

					

					HttpListenerResponse response = context.Response;

					string responseString =
						"<HTML><BODY>" + "" +
					    "<form method=\"post\">First name: <input type=\"text\" name=\"firstname\" /><br />Last name: <input type=\"text\" name=\"lastname\" /><input type=\"submit\" value=\"Submit\" /></form>" +
					    "</BODY></HTML>";

					byte[] buffer = Encoding.UTF8.GetBytes(responseString);

					response.ContentLength64 = buffer.Length;
					Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					output.Close();
				}

				//Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
				Thread.Sleep(10);
			}
		}



		static void Save()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";
			xml += "<root \n" +
				"datetime=\"" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "\" \n" +
				"guid=\"" + Guid.NewGuid() + "\" \n" +
				"fileback=\"" + Guid.NewGuid() + "\" \n" +
				"cmd=\"" + "test" + "\" \n" +
				">\n";

			xml += "<var name=\"var1\"><![CDATA[text]]></var>\n";
			xml += "<var name=\"var2\"><![CDATA[text]]></var>\n";

			xml += "</root>\n";

			File.WriteAllText(@"C:\1C_XML\Input\" + Guid.NewGuid() + ".xml", xml);
		}
	}
}
