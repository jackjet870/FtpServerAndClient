using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using log4net;

namespace SharpFtpServer
{
    public class FtpServer : IDisposable
    {
        ILog _log = LogManager.GetLogger(typeof(FtpServer));

        private bool _disposed = false;
        private bool _listening = false;

        private TcpListener _listener;
        private List<ClientConnection> _activeConnections;

        private IPEndPoint _localEndPoint;
        string serverPath = "C:\\FtpServer";

        public string ServerPath
        {
            get { return serverPath; }
            set { serverPath = value; }
        }

        public FtpServer()
            : this(IPAddress.Any, 21)
        {
        }

        public FtpServer(IPAddress ipAddress, int port)
        {
            _localEndPoint = new IPEndPoint(ipAddress, port);
        }

        public void Start()
        {
            _listener = new TcpListener(_localEndPoint);

            _log.Info("#Version: 1.0");
            _log.Info("#Fields: date time c-ip c-port cs-username cs-method cs-uri-stem sc-status sc-bytes cs-bytes s-name s-port");

            _listening = true;
            _listener.Start();

            _activeConnections = new List<ClientConnection>();
            try
            {
                _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Stop()
        {
            _log.Info("Stopping FtpServer");

            _listening = false;
            _listener.Stop();

            _listener = null;
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            if (_listening)
            {
                _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);

                TcpClient client = _listener.EndAcceptTcpClient(result);

                ClientConnection connection = new ClientConnection(client);
                connection.ServerPath = this.ServerPath;
                _activeConnections.Add(connection);

                ThreadPool.QueueUserWorkItem(connection.HandleClient, client);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    foreach (ClientConnection conn in _activeConnections)
                    {
                        conn.Dispose();
                    }
                }
            }

            _disposed = true;
        }
    }
}
