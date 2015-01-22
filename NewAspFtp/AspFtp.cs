using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.FtpClient;
using System.Net;
using System.IO;
using System.Diagnostics;

/// <summary>
/// Contains classes used to connect to FTP servers.
/// </summary>
namespace Crosse.Net.NewAspFtp
{
    public class AspFtp : IDisposable
    {

        FtpClient client = null;

        /// <summary>
        /// Gets or sets the username for this connection.
        /// </summary>
        /// <value>
        /// The username to use.
        /// </value>
        public string UserName { get; set; }


        /// <summary>
        /// Gets or sets the password to use for this connection.
        /// </summary>
        /// <value>
        /// The password to use.
        /// </value>
        public string Password { get; set; }


        /// <summary>
        /// Gets or sets the name of the remote server.
        /// </summary>
        /// <value>
        /// The name of the remote server.
        /// </value>
        public string ServerName { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether FTP transfers are performed using <c>ASCII</c> or <c>BINARY</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if transfers should use the <c>BINARY</c> transfer method; otherwise, <c>false</c>.
        /// </value>
        public bool TransferBinary { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether <see cref="GetFile"/> should overwrite existing local files.
        /// </summary>
        /// <value>
        ///   <c>true</c> if <see cref="Getfile"/> should overwrite existing local files; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether passive mode should be used for this connection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if passive mode should be enabled; otherwise, <c>false</c>.
        /// </value>
        public bool PassiveMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is connected to a remote FTP server.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is connected to a remote FTP server; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                if (client == null)
                    return false;
                else
                    return client.IsConnected;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspFtp"/> class.
        /// </summary>
        public AspFtp()
        {
            UserName = "";
            Password = "";
            ServerName = "";
            TransferBinary = false;
        }

        /// <summary>
        /// Connects to remote FTP server.
        /// </summary>
        /// <returns><c>true</c> if the connection attempt succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <see cref="ServerName"/> cannot be resolved.</exception>
        public bool Connect()
        {
            if (IsConnected)
                return true;

            var addrs = Dns.GetHostAddresses(ServerName);
            if (addrs.Length == 0)
            {
                throw new ArgumentException("Server name required");
            }

            client = new FtpClient();
            client.Host = ServerName;
            client.Credentials = new NetworkCredential(UserName, Password);
            client.DataConnectionType = PassiveMode ? FtpDataConnectionType.AutoPassive : FtpDataConnectionType.AutoActive;

            client.Connect();
            if (!client.IsConnected)
            {
                client.Dispose();
            }

            return client.IsConnected;
        }

        /// <summary>
        /// Disconnects from the remote server.
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                client.Disconnect();
                client.Dispose();
            }
        }

        /// <summary>
        /// Deletes the file on the remote server.
        /// </summary>
        /// <param name="filename">The filename to delete.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="filename"/> is successfully deleted; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.Net.FtpClient.FtpException">
        /// Thrown if an error occurs during the FTP operation.
        /// </exception>
        public bool DeleteFile(string filename)
        {
            if (!IsConnected)
                throw new FtpException("No available connection.", -1);

            bool result = false;
            try
            {
                client.DeleteFile(filename);
                result = true;
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return result;
        }

        /// <summary>
        /// Enumerates a directory listing on the remote server.
        /// </summary>
        /// <param name="directory">The directory to enumerate.</param>
        /// <returns></returns>
        /// <exception cref="System.Net.FtpClient.FtpException">Thrown when the FTP operation fails.</exception>
        public string[] EnumerateDirectory(string directory)
        {
            if (!IsConnected)
                return null;

            string[] items;
            try
            {
                if (String.IsNullOrEmpty(directory))
                    items = client.GetNameListing();
                else
                    items = client.GetNameListing(directory);

            }
            catch (Exception e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return items;
        }

        /// <summary>
        /// Copies a file from the remote server to the local machine.
        /// </summary>
        /// <param name="remoteFilePath">The remote file path.</param>
        /// <param name="localFilePath">The local file path.</param>
        /// <returns>
        ///   <c>true</c> if the operation succeeeds; otherwise, <c>false</c>.
        /// </returns>
        public bool GetFile(string remoteFilePath, string localFilePath)
        {
            if (!IsConnected)
                return false;

            var xferType = TransferBinary ? FtpDataType.Binary : FtpDataType.ASCII;
            bool result = false;

            FileMode mode = Overwrite ? FileMode.Create : FileMode.CreateNew;

            using (Stream from = client.OpenRead(remoteFilePath, xferType))
            using (Stream to = File.Open(localFilePath, mode, FileAccess.Write))
            {
                if (TransferBinary)
                    result = TransferFileBinary(from, to);
                else
                    result = TransferFileAscii(from, to);
            }
            return result;
        }

        /// <summary>
        /// Opens a remote file.
        /// </summary>
        /// <param name="remoteFilePath">The remote file to open.</param>
        /// <param name="access">The <see cref="System.IO.FileAccess"/> to use when opening the file.</param>
        /// <returns>A <see cref="System.IO.Stream"/> representing the remote file.</returns>
        /// <exception cref="System.NotImplementedException">File access is restricted to read or write only</exception>
        public Stream OpenFile(string remoteFilePath, FileAccess access)
        {
            if (!IsConnected)
                return null;

            var xferType = TransferBinary ? FtpDataType.Binary : FtpDataType.ASCII;
            Stream s = null;
            try
            {
                switch (access)
                {
                    case FileAccess.Read:
                        s = client.OpenRead(remoteFilePath, xferType);
                        break;
                    case FileAccess.Write:
                        s = client.OpenWrite(remoteFilePath, xferType);
                        break;
                    case FileAccess.ReadWrite:
                        throw new NotImplementedException("File access is restricted to read or write only");
                }
            }
            catch (FtpCommandException e)
            {
                if (s != null)
                    s.Close();

                throw new FtpException(e.Message, e.HResult);
            }

            return s;
        }

        private bool TransferFileBinary(Stream from, Stream to)
        {
            Debug.WriteLine("Initiating BINARY transfer");
            bool result = false;

            BinaryReader reader = new BinaryReader(from);
            BinaryWriter writer = new BinaryWriter(to);
            try
            {
                if (from.Length <= int.MaxValue)
                    writer.Write(reader.ReadBytes((int)from.Length));
                else
                {
                    long pos = 0;
                    while (pos < from.Length)
                    {
                        byte[] bytes = reader.ReadBytes(int.MaxValue);
                        writer.Write(bytes);
                        pos += bytes.Length;
                    }
                }
                result = true;
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (reader != null)
                    reader.Close();
            }

            return result;
        }

        private bool TransferFileAscii(Stream from, Stream to)
        {
            Debug.WriteLine("Initiating ASCII transfer");
            bool result = false;

            StreamReader reader = new StreamReader(from);
            StreamWriter writer = new StreamWriter(to);

            try
            {
                while (!reader.EndOfStream)
                {
                    writer.WriteLine(reader.ReadLine());
                }
                result = true;
            }
            catch (Exception e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (reader != null)
                    reader.Close();
            }

            return result;
        }

        /// <summary>
        /// Copies a local file to the remote server.
        /// </summary>
        /// <param name="localFilePath">The local file to copy to the remote server.</param>
        /// <param name="remoteFilePath">The name to use for the file on the remote server.</param>
        /// <returns><c>true</c> if the operation succeeeds; otherwise, <c>false</c>.</returns>
        public bool PutFile(string localFilePath, string remoteFilePath)
        {
            if (!IsConnected)
                return false;

            var xferType = TransferBinary ? FtpDataType.Binary : FtpDataType.ASCII;
            bool result = false;

            using (Stream from = File.OpenRead(localFilePath))
            using (Stream to = client.OpenWrite(remoteFilePath, xferType))
            {
                if (TransferBinary)
                    result = TransferFileBinary(from, to);
                else
                    result = TransferFileAscii(from, to);
            }
            return result;
        }

        /// <summary>
        /// Creates a directory on the remote server.
        /// </summary>
        /// <param name="remotePath">The remote path to create.</param>
        /// <returns><c>true</c> if the operation succeeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Net.FtpClient.FtpException"></exception>
        public bool CreateDirectory(string remotePath)
        {
            if (!IsConnected)
                return false;

            try
            {
                client.CreateDirectory(remotePath);
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return true;
        }

        /// <summary>
        /// Removes a directory on the remote server.
        /// </summary>
        /// <param name="path">The path to remove.</param>
        /// <returns><c>true</c> if the operation succeeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Net.FtpClient.FtpException"></exception>
        public bool RemoveDirectory(string path)
        {
            if (!IsConnected)
                return false;

            try
            {
                client.DeleteDirectory(path);
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return true;
        }

        /// <summary>
        /// Gets the current working directory on the remote server.
        /// </summary>
        /// <returns>The current remote working directory./returns>
        /// <exception cref="System.Net.FtpClient.FtpException"></exception>
        public string GetCurrentDirectory()
        {
            if (!IsConnected)
                return String.Empty;

            string result = "";
            try
            {
                result = client.GetWorkingDirectory();
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }

            return result;
        }

        /// <summary>
        /// Sets the current directory on the remote server.
        /// </summary>
        /// <param name="remotePath">The remote path to set.</param>
        /// <returns><c>true</c> if the operation succeeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Net.FtpClient.FtpException"></exception>
        public bool SetCurrentDirectory(string remotePath)
        {
            if (!IsConnected)
                return false;

            try
            {
                client.SetWorkingDirectory(remotePath);
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return true;
        }

        /// <summary>
        /// Renames a file or directory on the remote server.
        /// </summary>
        /// <param name="currentName">The current name of the item on the remote server to rename.</param>
        /// <param name="newName">The new name of the item.</param>
        /// <returns><c>true</c> if the operation succeeeds; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.Net.FtpClient.FtpException"></exception>
        public bool Rename(string currentName, string newName)
        {
            if (!IsConnected)
                return false;

            try
            {
                client.Rename(currentName, newName);
            }
            catch (FtpCommandException e)
            {
                throw new FtpException(e.Message, e.HResult);
            }
            return true;
        }

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            client.Dispose();
            client = null;
        }
        #endregion
    }
}
