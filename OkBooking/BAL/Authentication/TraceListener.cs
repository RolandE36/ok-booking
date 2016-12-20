using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.Authentication {
	public class TraceListener : ITraceListener {
		public void Trace(string traceType, string traceMessage) {
			CreateXMLTextFile(traceType, traceMessage);
		}

		private void CreateXMLTextFile(string fileName, string traceContent)
		{
			return; // TODO: Need to investigate is this logging necessary.

			try {
				if (!Directory.Exists(@"..\\TraceOutput")) {
					Directory.CreateDirectory(@"..\\TraceOutput");
				}

				System.IO.File.WriteAllText(@"..\\TraceOutput\\" + fileName + DateTime.Now.Ticks + ".txt", traceContent);
			} catch (IOException ex) {
				Console.WriteLine(ex.Message);
			}
		}
	}
}
