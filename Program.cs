using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace _1C_Data_Exchange
{
	class Program
	{
		static void Main(string[] args)
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
