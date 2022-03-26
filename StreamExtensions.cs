using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrologixGPIB
{
	static class StreamExtensions
	{
		public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

		public static void WriteLine(this Stream stream, string text)
		{
			using var writer = new StreamWriter(stream, UTF8, 4096, true);
			writer.WriteLine(text);
		}

		public static string ReadLine(this Stream stream)
		{
			var reader = new StreamReader(stream, UTF8, false, 4096, true);
			return reader.ReadLine();
		}

		public static long CopyBlockTo(this Stream inStream, Stream outStream, long length)
		{
			byte[] buffer = new byte[4096];
			long bytesCopied = 0;

			while (true)
			{
				var bytesRead = inStream.Read(buffer, 0, (int)Math.Min(length - bytesCopied, buffer.Length));
				outStream.Write(buffer, 0, bytesRead);
				bytesCopied += bytesRead;
				if (bytesCopied >= length || bytesRead == 0)
					break;
			}

			return bytesCopied;
		}

		public static byte[] ReadBytes(this Stream stream, long length)
		{
			using var buffer = new MemoryStream();
			stream.CopyBlockTo(buffer, length);
			return buffer.ToArray();
		}
	}
}
