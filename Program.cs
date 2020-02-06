using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;
using System.Net;
using System.Web;

using System.Xml.Xsl;

namespace _1C_Data_Exchange
{
	class Program
	{
		public static string pathSaveXMLCommand { get; set; }
		public static string pathReadXMLResult { get; set; }

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

			//listener.Stop();
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

					Console.WriteLine("-------------->");

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
							string value = HttpUtility.UrlDecode(kvPair[1]);
							postParams.Add(key, value);
							Console.WriteLine(key + " = " + value);
						}
					}

					string guid = Guid.NewGuid().ToString();
					string cmd = request.QueryString["cmd"];

					Save(guid, cmd);

					HttpListenerResponse response = context.Response;
					Stream output = response.OutputStream;

					string pathToRead = pathReadXMLResult + guid + ".xml";
					int timeout = 8000;

					Console.WriteLine("start timeout " + DateTime.Now.ToString());

					while (timeout > 0)
					{
						if (File.Exists(pathToRead))
						{
							Read(pathToRead, output);
							

							Console.WriteLine("Ok");

							File.Delete(pathToRead);

							break;
						}
						else
						{
							timeout -= 400;

							Console.WriteLine(DateTime.Now.TimeOfDay.ToString());
							Thread.Sleep(50);
						}
					}
					
					Console.WriteLine("end timeout " + DateTime.Now.ToString());

					try
					{
						output.Close();
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
				}

				//Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
				Thread.Sleep(10);
			}
		}

		static void Read(string pathToRead, Stream output)
		{
			XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
			xslCompiledTransform.Load(@"../../first.xslt");

			XsltArgumentList xsltArgumentList = new XsltArgumentList();

			xslCompiledTransform.Transform(pathToRead, xsltArgumentList, output);
		}

		static void Save(string guid, string cmd)
		{

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";
			xml += "<root \n" +
				"datetime=\"" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "\" \n" +
				"guid=\"" + guid + "\" \n" +
				"fileback=\"" + guid + "\" \n" +
				"cmd=\"" + cmd + "\" \n" +
				">\n";

			xml += "<var name=\"var1\"><![CDATA[text]]></var>\n";
			xml += "<var name=\"var2\"><![CDATA[text]]></var>\n";

			xml += "</root>\n";

			File.WriteAllText(pathSaveXMLCommand + guid + ".xml", xml);
		}
	}
}
