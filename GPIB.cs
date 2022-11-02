using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrologixGPIB
{
	public class GPIB : IDisposable
	{
		public string Host { get; }
		public int Address { get; }
		public int Timeout { get; }

		readonly TcpClient client;
		readonly NetworkStream stream;

		public bool Connected { get => client != null && client.Connected; }

		static string EscapeString(string str) => str
			.Replace("\u001B", "\u001B\u001B")
			.Replace("\r", "\u001B\r")
			.Replace("\n", "\u001B\n")
			.Replace("+", "\u001B+");

		public GPIB(string host, int address, int timeout = 3000, bool configureAdapter = true)
		{
			Host = host;
			Address = address;
			Timeout = timeout;

			client = new TcpClient(host, 1234) { NoDelay = true };
			stream = client.GetStream();

			stream.WriteLine("++savecfg 0");
			stream.WriteLine($"++addr {address}");

			if (configureAdapter)
			{
				stream.WriteLine("++mode 1");
				stream.WriteLine("++auto 0");
				stream.WriteLine("++eoi 1");
				stream.WriteLine("++eos 2");
				stream.WriteLine("++eot_enable 0");
				stream.WriteLine("++eot_char 0");
				stream.WriteLine($"++read_tmo_ms {timeout}");
			}

			stream.WriteLine("++ifc");
		}

		public void Send(string line) =>
			stream.WriteLine(EscapeString(line));

		public string ReceiveLine()
		{
			stream.WriteLine("++read eoi");
			return stream.ReadLine();
		}

		public string Query(string query)
		{
			Send(query);
			return ReceiveLine();
		}

		public byte[] ReceiveAllData(int? timeout = null)
		{
			stream.WriteLine("++read eoi");

			var data = new List<byte>();

			while (true)
			{
				Task.Delay(timeout ?? Timeout).Wait();
				var dataAvailable = client.Available;
				if (dataAvailable == 0) break;
				var dataBlock = stream.ReadBytes(dataAvailable);
				data.AddRange(dataBlock);
			}

			return data.ToArray();
		}

		public byte[] BinaryQuery(string query, int? timeout = null)
		{
			Send(query);
			return ReceiveAllData(timeout);
		}

		public void Local() =>
			stream.WriteLine("++loc");

		public void Reset() =>
			stream.WriteLine("++clr");

		public void Dispose()
		{
			stream.Dispose();
			client.Dispose();
		}
	}
}
