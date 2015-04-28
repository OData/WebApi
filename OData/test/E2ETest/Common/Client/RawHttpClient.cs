using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace WebStack.QA.Common.Client
{
    public class RawHttpClient : TcpClient
    {
        private Uri uri;

        public RawHttpClient(Uri uri)
            : base(uri.Host, uri.Port)
        {
            this.uri = uri;
            var stream = this.GetStream();
            this.Writer = new StreamWriter(stream);
            this.Reader = new StreamReader(stream);
            this.Writer.AutoFlush = true;
        }

        public StreamWriter Writer
        {
            get;
            set;
        }

        public StreamReader Reader
        {
            get;
            set;
        }

        public void WriteLine(string line)
        {
            this.Writer.WriteLine(line);
            Console.WriteLine(line);
        }

        public void WriteLine(string format, params object[] arg)
        {
            this.Writer.WriteLine(format, arg);
            Console.WriteLine(format, arg);
        }

        public void WriteLine()
        {
            this.Writer.WriteLine();
            Console.WriteLine();
        }

        public string ReadToEnd()
        {
            return Reader.ReadToEnd();
        }

        public void WriteHttpMethodHeader(string method, string relativePath, string version = "HTTP/1.1")
        {
            this.WriteLine(
                string.Format(
                "{0} {1} {2}",
                method,
                uri.PathAndQuery + relativePath,
                version));
        }

        public void WriteHttpHost()
        {
            this.WriteLine("Host: " + uri.Host + ":" + uri.Port);
        }

        public void WriteBodyContent(string content, string contentType)
        {
            this.WriteLine("Content-Type: {0}", contentType);
            this.WriteLine("Content-Length: {0}", Encoding.Default.GetBytes(content).Length);
            this.WriteLine();
            this.Writer.WriteLine(content);
            this.Writer.Flush();
        }

        public void WriteConnectionClose()
        {
            this.WriteLine("Connection: close");
            this.WriteLine();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Writer != null)
                {
                    this.Writer.Dispose();
                }

                if (this.Reader != null)
                {
                    this.Reader.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
