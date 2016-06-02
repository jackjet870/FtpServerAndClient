using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FtpDotNet
{        
    /// <summary>
    /// Provides easy access to common operations against FTP servers
    /// </summary>    
    public class FtpConnection
    {
        #region Private Variables

        /// <summary>
        /// The username used to connect to the FTP server
        /// </summary>
        private string mUserName = "";

        /// <summary>
        /// The password used to connect to the FTP server
        /// </summary>        
        private string mPassword = "";
        
        /// <summary>
        /// Determines if transfers should be performed using binary mode Default (true)
        /// </summary>
        private bool mUseBinaryMode = true;
        
        /// <summary>
        /// The DNS or IP address to the FTP server 
        /// </summary>
        private string mHost = "" ;

        /// <summary>
        /// The portnumber for the FTP server
        /// </summary>
        private int mPort = 21;

        /// <summary>
        /// The remote directory on the FTP server
        /// </summary>
        private string mRemoteDirectory = "";

        /// <summary>
        /// Local directory to be used when uploading and downloading files
        /// </summary>
        private string mLocalDirectory = "" ;

        /// <summary>
        /// The number of milliseconds to wait before a request to the FTP server times out (Default Infinite(-1))
        /// </summary>
        private int mTimeOut = System.Threading.Timeout.Infinite;

        
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class
        /// </summary>
        public FtpConnection(){}

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class
        /// </summary>
        /// <param name="host">The DNS or IP address to the FTP server</param>
        /// <param name="port">The portnumber for the FTP server</param>
        /// <param name="userName">The username used to connect to the FTP server</param>
        /// <param name="password">The password used to connect to the FTP server</param>
        public FtpConnection(string host,int port,string userName, string password)
        {
            mHost = host;
            mPort = port;
            mUserName = userName;
            mPassword = password;
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Receives status messages from the <see cref="FtpConnection"/> during operations
        /// </summary>
        /// <example>
        /// This example sets up an event handler for the MessageReceived event.
        /// <code>
        /// void connection_MessageReceived(object sender, FtpConnectionEventArgs e)
        /// {
        ///     Console.WriteLine(e.Message);
        /// }
        /// </code>
        /// </example>
        public event FtpConnectionEventHandler MessageReceived;

        #endregion  

        #region Private Methods
        /// <summary>
        /// Creates a new <see cref="FtpWebRequest"/>
        /// </summary>
        /// <param name="method">The <see cref="FtpWebRequest.Method"/></param>
        /// <param name="uri">The uri string for the request</param>
        /// <returns><see cref="FtpWebRequest"/></returns>
        private FtpWebRequest CreateWebRequest(string method, string uri)
        {
            UriBuilder uriBuilder = new UriBuilder(uri);
            uriBuilder.Port = mPort;
            FtpWebRequest ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(uriBuilder.Uri);
            ftpWebRequest.Method = method;
            ftpWebRequest.UseBinary = mUseBinaryMode;
            ftpWebRequest.KeepAlive = false;
            ftpWebRequest.Timeout = mTimeOut;
            ftpWebRequest.Credentials = new NetworkCredential(mUserName, mPassword);
            return ftpWebRequest;
        }

        /// <summary>
        /// Sends a message to the listeners of the <see cref="MessageReceived"/> event
        /// </summary>
        /// <param name="message">The message to send</param>
        private void OnMessageReceived(string message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new FtpConnectionEventArgs(message.Replace(Environment.NewLine, string.Empty)));
            }
        }

        /// <summary>
        /// Gets the name of the local directory
        /// </summary>
        /// <returns>directory name</returns>
        private string GetLocalDirectoryName()
        {
            string directory = "";
            if (mLocalDirectory.Length > 0)
            {
                if (!Directory.Exists(mLocalDirectory))
                    Directory.CreateDirectory(mLocalDirectory);
                directory = mLocalDirectory;
            }
            else
                directory = GetExecutingAssemblyPath();

            return directory;
        }

        /// <summary>
        /// Formats a <see cref="WebException"/> as a message back to the event listener
        /// </summary>
        /// <param name="ex">The exeption to format</param>
        private void HandleException(WebException ex)
        {
            OnMessageReceived(string.Format("Status: {0} {1}", (ex.Response as FtpWebResponse).StatusCode, (ex.Response as FtpWebResponse).StatusDescription));
            throw ex;
        }

        #endregion

        #region Private Static Methods
        
        /// <summary>
        /// Checks to see if the configured logpath is releative or absolute
        /// </summary>
        /// <returns>True if the path is relative, otherwise false</returns>
        private static bool IsPathRelative(string path)
        {
            Regex regex = new Regex(@"\w:\\");
            return !regex.Match(path).Success;

        }

        /// <summary>
        /// Returns the location of the excuting assembly
        /// </summary>
        /// <returns>The local operation system representation of the executing assembly location</returns>
        private static string GetExecutingAssemblyPath()
        {
            Uri uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            return uri.LocalPath;
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Sets/Gets portnumber for the FTP server
        /// </summary>
        public int Port
        {
            get { return mPort; }
            set { mPort = value; }
        }

        /// <summary>
        /// Sets/Gets number of milliseconds to wait before a request to the FTP server times out (Default Infinite(-1))
        /// </summary>
        public int TimeOut
        {
            get { return mTimeOut; }
            set { mTimeOut = value; }
        }

        /// <summary>
        /// Sets/Gets the directory to be used when uploading and downloading files
        /// </summary>
        /// <remarks>
        /// If the local directory is not supplied, upload and download will operate
        /// on the execting assembly path.
        /// </remarks>
        public string LocalDirectory
        {
            get { return mLocalDirectory; }
            set { mLocalDirectory = value; }
        }

        /// <summary>
        /// Sets/Gets the remote directory on the FTP server
        /// </summary>       
        public string RemoteDirectory
        {
            get { return mRemoteDirectory; }
            set { mRemoteDirectory = value; }
        }

        /// <summary>
        /// Sets/Gets the DNS or IP address to the FTP server (ex ftp://myftpserver.com)
        /// </summary>
        public string Host
        {
            get { return mHost; }
            set { mHost = value; }
        }
	
        /// <summary>
        /// Sets/Gets if transfers should be performed using binary mode Default (true) 
        /// </summary>
        public bool UseBinaryMode
        {
            get { return mUseBinaryMode; }
            set { mUseBinaryMode = value; }
        }
	
        /// <summary>
        /// Sets/Gets the password used to connect to the FTP server
        /// </summary>
        public string Password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }
	

        /// <summary>
        /// Sets/Gets the username used to connect to the FTP server
        /// </summary>
        public string UserName
        {
            get { return mUserName; }
            set { mUserName = value; }
        }

        #endregion

        #region Public Methods

        /// <overloads>
        /// <summary>
        /// Uploads a file to the FTP Server
        /// </summary>                
        /// <example>
        /// The following example uploads a file to a FTP server     
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     connection.Upload(@"C:\Temp\House.jpg", "House.jpg");
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        ///</overloads>
        /// <param name="fileName">The name of the file to upload</param>        
        public void Upload(string fileName)
        {            
            Upload(fileName, Path.GetFileName(fileName));
        }

        /// <param name="fileName">The name of the file to upload</param>
        /// <param name="remoteFileName">The name of the destination file</param>        
        public void Upload(string fileName,string remoteFileName)
        {
            
            //Mark the beginning of the transfer
            DateTime startTime = DateTime.Now;
            
            //Check to see if we have a relative or absolute path to the file to transfer
            if (IsPathRelative(fileName))
                fileName = Path.Combine(GetLocalDirectoryName(), fileName);

            string remotePath = mHost + "/" + mRemoteDirectory + "/" + remoteFileName;
            OnMessageReceived(string.Format("Uploading {0} to {1}",fileName,remotePath));


            //Create a request for the upload
            FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.UploadFile, remotePath);

            //Get a reference to the upload stream
            Stream uploadStream = ftpWebRequest.GetRequestStream();

            FileStream fileStream = null;
            
            try
            {
                //Create a filestream for the local file to upload
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                byte[] buffer = new byte[1024];
                int bytesRead;
                int bytesTotalWritten = 0;
                //Start uploading
                while (true)
                {
                    bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    uploadStream.Write(buffer, 0, bytesRead);
                    bytesTotalWritten += bytesRead;
                }
                
                //Close the upload stream
                uploadStream.Close();
                uploadStream.Dispose();

                //Close the local file stream
                fileStream.Close();
                fileStream.Dispose();

                //Get the respone from the server
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                OnMessageReceived(string.Format("Status: {0} {1}", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));                                                
                //Report the number of bytes transferred
                OnMessageReceived(string.Format("Transferred {0} bytes in {1} seconds", bytesTotalWritten.ToString(), DateTime.Now.Subtract(startTime).Seconds));
            }

            finally
            {
                if (uploadStream != null)
                {
                    uploadStream.Close();
                    uploadStream.Dispose();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

        }

        /// <overloads>
        /// <summary>
        /// Downloads a file from the FTP server
        /// </summary>                
        /// <example>
        /// The following example downloads a file from a FTP server     
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     connection.Download("House.jpg",@"C:\Temp\House.jpg");
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        ///</overloads>
        /// <param name="fileName">The name of the remote file to download</param>
        public void Download(string fileName)
        {
            Download(fileName, Path.Combine(GetExecutingAssemblyPath(), Path.GetFileName(fileName)));            
        }
        
        /// <param name="fileName">The name of the remote file to download</param>
        /// <param name="localFileName">The name of the local file to be created</param>
        public void Download(string fileName, string localFileName)
        {

            //Mark the beginning of the transfer
            DateTime startTime = DateTime.Now;
            
            //Get a temporary file to download to.
            string tempFileName = Path.GetTempFileName();

            string remotePath = mHost + "/" + mRemoteDirectory + "/" + fileName;

            OnMessageReceived(string.Format("Downloading {0} to {1}", fileName, remotePath));

            //Create a request for the upload
            FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.DownloadFile, remotePath);

            //Get a reference to the download stream
            FtpWebResponse ftpWebResponse  = (FtpWebResponse)ftpWebRequest.GetResponse();
            Stream downloadStream = ftpWebResponse.GetResponseStream();
            
            FileStream fileStream = null;    
            
            try
            {
                fileStream = new FileStream(tempFileName, FileMode.Append);

                byte[] buffer = new byte[1024];
                int bytesRead = 0 ;
                int bytesTotalRead = 0;

                while (true)
                {                    
                    bytesRead = downloadStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    fileStream.Write(buffer, 0, bytesRead);
                    bytesTotalRead += bytesRead;
                }
                
                //Close the download stream
                downloadStream.Close();

                //Close the local file stream
                fileStream.Close();

                //Get the respone from the server                
                OnMessageReceived(string.Format("Status: {0} {1}",ftpWebResponse.StatusCode,ftpWebResponse.StatusDescription));
                OnMessageReceived(string.Format("Transferred {0} bytes in {1} seconds", bytesTotalRead.ToString(), DateTime.Now.Subtract(startTime).Seconds));


                //Make sure that the local file does not exist
                if (File.Exists(localFileName))
                    File.Delete(localFileName);
                File.Move(tempFileName, localFileName);
            }
            catch 
            {
                //If an error occours, we clean up the temporary file
                //Close the local file stream
                fileStream.Close();                
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
                throw;
            }

            finally
            {
                if (downloadStream != null)
                {
                    downloadStream.Close();                    
                }
                if (fileStream != null)
                {
                    fileStream.Close();                    
                }
            }
        }

        /// <overloads>
        /// <summary>
        /// Downloads multible files from the FTP server 
        /// </summary>
        /// <example>
        /// The following example downloads all jpg files from a FTP server     
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     connection.DownloadFiles("*.jpg");
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        /// </overloads>
        public void DownloadFiles()
        {
            DownloadFiles("*.*");
        }

        
        /// <param name="filter">The filter to apply when downloading files (ex *.txt)</param>    
        public void DownloadFiles(string filter)
        {
            List<string> files = ListDirectory(filter);
            foreach (string file in files)
            {
                Download(file, Path.Combine(GetLocalDirectoryName(), file));
            }
        }

        /// <overloads>
        /// <summary>
        /// Lists the contents of the <seealso cref="FtpConnection.RemoteDirectory"/>
        /// </summary>
        /// <returns>A list of files in the <seealso cref="FtpConnection.RemoteDirectory"/></returns>
        /// <example>
        /// The following example lists all jpg files on a FTP server     
        /// <code>
        /// <![CDATA[
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     List<string> files = connection.ListDirectory(*.jpg);
        ///     foreach (string file in files)
        ///     {
        ///         Console.WriteLine(File);
        ///     }
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// </overloads>        
        public List<string> ListDirectory()
        {
            return ListDirectory("*.*");
        }
        
        /// <param name="filter">The filter to apply when listing files (ex *.txt)</param>        
        public List<string> ListDirectory(string filter)
        {
            
            List<string> files = new List<string>();
            string line = "";
            string remotePath = mHost + "/" + mRemoteDirectory + "/" + filter;
            StreamReader responseStream = null; ;
            
            try
            {
                //Create a request for directory listing
                FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.UploadFileWithUniqueName, remotePath);

                //Get a reference to the response stream
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                responseStream = new StreamReader(ftpWebResponse.GetResponseStream());

                while ((line = responseStream.ReadLine()) != null)
                {
                    files.Add(line);
                }
                OnMessageReceived(string.Format("Status: {0} {1} (FTP NLIST)", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));
            }
            catch (WebException ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();
            }
            
            //Return the list of files
            return files;
        }
        
        /// <summary>
        /// Deletes a file from the FTP server
        /// </summary>
        /// <param name="fileName">The name of the file to delete</param>
        /// <example>
        /// The following example deletes a file from a FTP server     
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     connection.DeleteFile("House.jpg");
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        public void DeleteFile(string fileName)
        {
            string remotePath = mHost + "/" + mRemoteDirectory + "/" + fileName;
            try
            {
                
                FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.DeleteFile, remotePath);
                //get the response from the server
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                OnMessageReceived(string.Format("Status: {0} {1}", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));
                OnMessageReceived(string.Format("Deleted file {0}",remotePath));
            }
            catch (WebException ex)
            {
                HandleException(ex);
            }
        }
        
        /// <summary>
        /// Gets the size of a file on the FTP server
        /// </summary>
        /// <param name="fileName">The name of the remote file</param>
        /// <returns>The size of the file in bytes</returns> 
        /// <example>
        /// The following example gets the size of a file on a FTP server
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     Console.WriteLine(connection.GetFileSize("House.jpg").ToString());
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        public Int64 GetFileSize(string fileName)
        {
            string remotePath = mHost + "/" + mRemoteDirectory + "/" + fileName;
            Int64 fileSize = 0;
            try
            {

                FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.GetFileSize, remotePath);
                //get the response from the server
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                fileSize = ftpWebResponse.ContentLength;
                OnMessageReceived(string.Format("Status: {0} {1}", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));                
            }
            catch (WebException ex)
            {
                HandleException(ex);
            }
            return fileSize;
        }

        /// <summary>
        /// Gets a timestamp indicating what time the remote file was modified
        /// </summary>
        /// <param name="fileName">The name of the remote file</param>
        /// <returns>The date and time for when the file was last modified</returns> 
        /// <example>
        /// The following example gets the timestamp of a file on a FTP server
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     Console.WriteLine(connection.GetFileTimeStamp("House.jpg").ToString());
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        public DateTime GetFileTimeStamp(string fileName)
        {
            string remotePath = mHost + "/" + mRemoteDirectory + "/" + fileName;
            DateTime lastModified = DateTime.MinValue;
            try
            {

                FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.GetDateTimestamp, remotePath);
                //get the response from the server
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                lastModified = ftpWebResponse.LastModified;
                OnMessageReceived(string.Format("Status: {0} {1}", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));
            }
            catch (WebException ex)
            {
                HandleException(ex);                
            }
            return lastModified;
        }

        /// <summary>
        /// Creates a new directory on the FTP Server
        /// </summary>
        /// <param name="directoryName">The name of the directory to create</param>
        /// <example>
        /// The following example creates a new directory on a FTP server
        ///<code>
        /// try
        /// {
        ///     FtpConnection connection = new FtpConnection();
        ///     connection.MessageReceived += new FtpConnectionEventHandler(connection_MessageReceived);
        ///     connection.Host = "ftp://ftp.myserver.com";
        ///     connection.UserName = "username";
        ///     connection.Password = "password";
        ///     connection.RemoteDirectory = "/MyDirectory";
        ///     connection.MakeDirectory("MyNewDirectory");
        /// }
        /// catch (WebException ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine(ex.ToString());
        /// }
        ///</code>
        ///</example>
        public void MakeDirectory(string directoryName)
        {
            string remotePath = mHost + "/" + mRemoteDirectory + "/" + directoryName;           
            try
            {

                FtpWebRequest ftpWebRequest = CreateWebRequest(WebRequestMethods.Ftp.MakeDirectory, remotePath);
                //get the response from the server
                FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();                
                OnMessageReceived(string.Format("Status: {0} {1}", ftpWebResponse.StatusCode, ftpWebResponse.StatusDescription));
            }
            catch (WebException ex)
            {
                HandleException(ex);
            }
        }
        #endregion
    }
}
