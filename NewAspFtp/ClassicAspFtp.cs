using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crosse.Net.NewAspFtp
{
    [ComVisible(true)]
    [Guid("75baf20b-d2fe-440b-afca-378ecf3ff689")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("ClassicAspFtp")]
    public class ClassicAspFtp : IClassicAspFtp
    {
        #region Constants
        const int GENERIC_ERROR = -2;

        public const int ACCESS_TYPE_PRECONFIG = 0;
        public const int ACCESS_TYPE_DIRECT = 1;
        public const int ACCESS_TYPE_PROXY = 2;

        public const int TRANSFER_TYPE_ASCII = 1;
        public const int TRANSFER_TYPE_BINARY = 2;

        public const int FILE_ACCESS_WRITE = 1;
        public const int FILE_ACCESS_READ = 2;

        public const int ATTRIBUTE_READONLY = 1;
        public const int ATTRIBUTE_HIDDEN = 2;
        public const int ATTRIBUTE_SYSTEM = 4;
        public const int ATTRIBUTE_DIRECTORY = 16;
        public const int ATTRIBUTE_ARCHIVE = 32;
        public const int ATTRIBUTE_NORMAL = 128;
        public const int ATTRIBUTE_TEMPORARY = 256;
        public const int ATTRIBUTE_COMPRESSED = 2048;
        public const int ATTRIBUTE_OFFLINE = 4096;
        #endregion

        #region Properties
        [ComVisible(true)]
        public bool bOverwrite { get { return af.Overwrite; } set { af.Overwrite = value; } }

        [ComVisible(true)]
        public bool bPassiveMode { get { return af.PassiveMode; } set { af.PassiveMode = value; } }

        [ComVisible(true)]
        public long lErrorNum { get; protected set; }

        /// <summary>
        /// Connection method to use for FTP session. If Preconfig is specified,
        /// AspFTP retrieves the proxy or direct configuration from the Windows
        /// Registry. If Direct, all server names are resolved locally. If Proxy,
        /// AspFTP passes requests to the proxy specified unless, a proxy bypass
        /// list is supplied and the name to be resolved bypasses the proxy. By
        /// default, lAccessType is ACCESS_TYPE_DIRECT.
        /// </summary>
        [ComVisible(true)]
        public long lAccessType
        {
            get { return ACCESS_TYPE_DIRECT; }
            set
            {
                if (value != ACCESS_TYPE_DIRECT)
                    throw new NotImplementedException("Only ACCESS_TYPE_DIRECT is supported.");
            }
        }


        private long fileAccess;
        [ComVisible(true)]
        public long lFileAccess
        {
            get { return fileAccess; }
            set
            {
                if (value < FILE_ACCESS_WRITE || value > FILE_ACCESS_READ)
                    throw new ArgumentOutOfRangeException();
                fileAccess = value;
            }
        }

        [ComVisible(true)]
        public long lTransferType
        {
            get { return af.TransferBinary ? TRANSFER_TYPE_BINARY : TRANSFER_TYPE_ASCII; }
            set
            {
                if (value < TRANSFER_TYPE_ASCII || value > TRANSFER_TYPE_BINARY)
                    throw new ArgumentOutOfRangeException();

                bool t = (value == TRANSFER_TYPE_BINARY) ? true : false;
                Debug.WriteLine("Setting transfer type to " + (t ? "BINARY" : "ASCII"));
                af.TransferBinary = t;
            }
        }

        private long lastError;
        private string lastErrorDescription;

        [ComVisible(true)]
        public string sError { get { return String.Format("{0}: {1}", lastError, lastErrorDescription); } }

        [ComVisible(true)]
        public string sErrorDesc { get { return lastErrorDescription; } }

        [ComVisible(true)]
        public string sUserID { get { return af.UserName; } set { af.UserName = value; } }

        [ComVisible(true)]
        public string sPassword { get { return af.Password; } set { af.Password = value; } }

        [ComVisible(true)]
        public string sServerName { get { return af.ServerName; } set { af.ServerName = value; } }

        /* Unimplemented:
         *  sProxyBypass - "Specifies the Proxy Bypass string to use during an FTP session. This property is only used when lAccessType <> ACCESS_TYPE_DIRECT. By default, sProxyBypass is Null."
         *  sProxyName - "Specifies name of the proxy server (or servers) to use during an FTP session. This property is only used when lAccessType <> ACCESS_TYPE_DIRECT. By default, sProxyName is Null."
         */
        #endregion

        AspFtp af;
        string[] dirItems;

        public ClassicAspFtp()
        {
            af = new AspFtp();

            bOverwrite = true;
            bPassiveMode = true;
            lAccessType = ACCESS_TYPE_DIRECT;
            lErrorNum = 0;
            lFileAccess = FILE_ACCESS_WRITE;
            lTransferType = TRANSFER_TYPE_ASCII;

            ClearErrors();

        }

        /// <summary>
        /// The bCloseFile() method attempts to close a file that was opened for
        /// reading or writing using the <see cref="bOpenFile"/> method.
        /// </summary>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bCloseFile() { throw new NotImplementedException(); }

        /// <summary>
        /// The bConnect method attempts to connect to the server specified by
        /// the <see cref="sServerName"/> property using the <see cref="sUserID"/> and <see cref="sPassword"/> properties
        /// for the user name and password, respectively.
        /// </summary>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bConnect()
        {
            ClearErrors();

            bool result = false;
            try
            {
                result = af.Connect();
            }
            catch (Exception e)
            {
                SetLastError(e);
            }
            return result;
        }

        /// <summary>
        /// The bDeleteFile method attempts to delete the file specified in <paramref name="strFile"/>
        /// from the server specified by the <see cref="sServerName"/> property using the <see cref="sUserID"/>
        /// and <see cref="sPassword"/> properties for the user name and password, respectively.
        /// </summary>
        /// <param name="strFile">The file to delete.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bDeleteFile(string strFile)
        {
            if (!bConnect())
            {
                SetLastError(-1, "No available connection.");
                return false;
            }
            ClearErrors();

            bool result = false;
            try
            {
                result = af.DeleteFile(strFile);
            }
            catch (FtpException e)
            {
                SetLastError(e);
            }
            return result;
        }

        /// <summary>
        /// The bDisconnect method attempts to disconnect and end any FTP session
        /// currently running.
        /// </summary>
        /// <returns>Always returns true.</returns>
        [ComVisible(true)]
        public bool bDisconnect() { af.Disconnect(); return true; }

        /// <summary>
        /// The bGetDir method attempts to enumerate the directory contents specified
        /// in <paramref name="strDir"/>. If <paramref name="strDir"/> is not provided,
        /// the current directory is enumerated.
        /// </summary>
        /// <param name="strDir">The directory on the remote server to check.</param>
        /// <param name="intType">The "type" of file to check. UNUSED.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bGetDir(string strDir = null, Nullable<int> intType = 0)
        {
            if (!bConnect())
            {
                SetLastError(-1, "No available connection.");
                return false;
            }
            ClearErrors();

            // JMU's use of this library does not entail using intType at all.
            if (intType.HasValue)
                throw new ArgumentException("Usage of intType is not supported");

            try
            {
                dirItems = af.EnumerateDirectory(strDir);
                if (dirItems.Length == 0)
                    SetLastError(0, "Empty Directory");
                //return (dirItems.Length > 0) ? true : false;
                return true;
            }
            catch (Exception e)
            {
                SetLastError(e);
            }

            return false;
        }

        /// <summary>
        /// The bGetFile method attempts to Get (receive) the file specified in
        /// <paramref name="strSourceFile"/> from the server specified by the <see cref="sServerName"/>
        /// property using the <see cref="sUserID"/> and <see cref="sPassword"/> properties for the user name and
        /// password, respectively.
        /// 
        /// <paramref name="strTargetFile"/> specifies name of the file to create on the local system.
        /// If <paramref name="strTargetFile"/> already exists and bOverWrite is False, an error occurs.
        /// </summary>
        /// <param name="strSourceFile">The remote file to receive from the remote system.</param>
        /// <param name="strTargetFile">The name of the file to create on the local system.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bGetFile(string strSourceFile, string strTargetFile)
        {
            if (!bConnect())
            {
                SetLastError(-1, "Could not connect to server");
                return false;
            }
            ClearErrors();

            bool result = false;
            try
            {
                result = af.GetFile(strSourceFile, strTargetFile);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }

            return result;
        }

        /// <summary>
        /// The bMakeDir method attempts to create the directory specified in <paramref name="strDir"/>
        /// on the server specified by the <see cref="sServerName"/> property using the <see cref="sUserID"/>
        /// and <see cref="sPassword"/> properties for the user name and password, respectively.
        /// </summary>
        /// <param name="strDir">The directory to create on the remote server.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bMakeDir(string strDir)
        {
            if (!bConnect())
            {
                SetLastError(-1, "No available connection.");
                return false;
            }

            bool result = false;
            try
            {
                result = af.CreateDirectory(strDir);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }
            return result;
        }

        /// <summary>
        /// The bOpenFile method attempts to open the file specified in <paramref name="strFileName"/>
        /// on the the server specified by the <see cref="sServerName"/> property using the
        /// <see cref="sUserID"/> and <see cref="sPassword"/> properties for the user name and password, respectively.
        /// </summary>
        /// <param name="strFileName">The file on the remote server to open.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bOpenFile(string strFileName) { throw new NotImplementedException(); }

        /// <summary>
        /// The bPutFile method attempts to Put (send) the file specified in
        /// <paramref name="strSourceFile"/> to the server specified by the <see cref="sServerName"/> property using
        /// the <see cref="sUserID"/> and <see cref="sPassword"/> properties for the user name and password, respectively.
        /// 
        /// <paramref name="strTargetFile"/> specifies name of the file to create on the remote system.
        /// If strTargetFile already exists, the existing file is replaced.
        /// </summary>
        /// <param name="strSourceFile">The local file to send to the remote system.</param>
        /// <param name="strTargetFile">The name of the file to create on the remote system.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bPutFile(string strSourceFile, string strTargetFile)
        {
            if (!bConnect())
            {
                SetLastError(-1, "Could not connect to server");
                return false;
            }
            ClearErrors();

            bool result = false;
            try
            {
                result = af.PutFile(strSourceFile, strTargetFile);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }

            return result;
        }

        #region "Quick" Methods.  These methods are simply stubs that connect to the server specified, call the "regular" method, then disconnect.
        private bool QuickConnectSetup(string serverName, string user, string password)
        {
            if (af.IsConnected)
                af.Disconnect();

            this.sServerName = serverName;
            this.sUserID = user;
            this.sPassword = password;

            if (!bConnect())
            {
                SetLastError(-1, "Could not connect to server");
                return false;
            }
            return true;
        }

        [ComVisible(true)]
        public bool bQDeleteFile(string strServerName, string strUserID, string strPassword, string strFile)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            var result = bDeleteFile(strFile);
            bDisconnect();

            return result;
        }

        [ComVisible(true)]
        public bool bQGetFile(string strServerName, string strUserID, string strPassword,
            string strSourceFile, string strTargetFile, long lngTransferType, bool blnOverWrite)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            this.lTransferType = lngTransferType;
            this.bOverwrite = blnOverWrite;

            var result = bGetFile(strSourceFile, strTargetFile);
            bDisconnect();
            return result;
        }

        [ComVisible(true)]
        public bool bQMakeDir(string strServerName, string strUserID, string strPassword, string strDir)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            var result = bMakeDir(strDir);
            bDisconnect();
            return result;
        }

        [ComVisible(true)]
        public bool bQPutFile(string strServerName, string strUserID, string strPassword,
            string strSourceFile, string strTargetFile, long lngTransferType)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            this.lTransferType = lngTransferType;

            var result = bPutFile(strSourceFile, strTargetFile);
            bDisconnect();
            return result;
        }

        [ComVisible(true)]
        public bool bQRemoveDir(string strServerName, string strUserID, string strPassword, string strDir)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            var result = bRemoveDir(strDir);
            bDisconnect();
            return result;
        }

        [ComVisible(true)]
        public bool bQRename(string strServerName, string strUserID, string strPassword,
            string strCurrentName, string strNewName)
        {
            if (!QuickConnectSetup(strServerName, strUserID, strPassword))
                return false;

            var result = bRename(strCurrentName, strNewName);
            bDisconnect();
            return result;
        }
        #endregion

        /// <summary>
        /// The bRemoveDir method attempts to remove the directory specified in
        /// <paramref name="strDir"/> from the server specified by the <see cref="sServerName"/> property using
        /// the <see cref="sUserID"/> and <see cref="sPassword"/> properties for the user name and password,
        /// respectively. If strDir is not empty, an error occurs.
        /// </summary>
        /// <param name="strDir">The directory to remove on the remote server.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bRemoveDir(string strDir)
        {
            if (!bConnect())
            {
                SetLastError(-1, "Could not connect to server");
                return false;
            }
            ClearErrors();

            bool result = false;
            try
            {
                result = af.RemoveDirectory(strDir);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }

            return result;
        }

        /// <summary>
        /// The bRename method attempts to rename the directory or file specified
        /// in <paramref name="strCurrentName"/> to <paramref name="strNewName"/> on the server specified by the
        /// <see cref="sServerName"/> property using the <see cref="sUserID"/> and <see cref="sPassword"/> properties for
        /// the user name and password, respectively.
        /// </summary>
        /// <param name="strCurrentName">The current name of the file on the remote server.</param>
        /// <param name="strNewName">The new name of the file on the remote server.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bRename(string strCurrentName, string strNewName)
        {
            if (!bConnect())
            {
                SetLastError(-1, "Could not connect to server.");
                return false;
            }

            bool result = false;
            try
            {
                result = af.Rename(strCurrentName, strNewName);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }
            return result;
        }

        /// <summary>
        /// The bSetCurrentDir method attempts to make the directory specified in
        /// <paramref name="strDirName"/> the current directory on the server specified by the
        /// <see cref="sServerName"/> property using the <see cref="sUserID"/> and <see cref="sPassword"/> properties for
        /// the user name and password, respectively.
        /// </summary>
        /// <param name="strDirName">The directory on the remote server to change to.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bSetCurrentDir(string strDirName)
        {
            if (!bConnect())
            {
                SetLastError(-1, "No available connection.");
                return false;
            }

            return af.SetCurrentDirectory(strDirName);
        }

        /// <summary>
        /// The bWriteFile method attempts to write to the file opened by the
        /// bOpenFile method. You can write to a file opened by the bOpenFile
        /// method until you close the file using the bCloseFile method.
        /// 
        /// Use this method when wanting to write incrementally to a remote file,
        /// create a remote file without a local file to send, or you want to
        /// provide progress information for writing a file.
        /// </summary>
        /// <param name="strData">The data to write to the remote file.</param>
        /// <returns><code>true</code> if the operation succeeded; otherwise, <code>false</code>.</returns>
        [ComVisible(true)]
        public bool bWriteFile(string strData) { throw new NotImplementedException(); }

        /// <summary>
        /// The lGetDirCount method returns the number of items in the current
        /// directory collection that was created using the bGetDir method.
        /// </summary>
        /// <returns>Number of items.</returns>
        [ComVisible(true)]
        public long lGetDirCount()
        {
            return dirItems.Length;
        }

        /// <summary>
        /// The sConvertCrLf method converts all occurances of Cr+Lf
        /// (Chr$(10)+Chr$(13)) into the HTML linebreak tag (&lt;br&gt;).
        /// 
        /// This is included to provide an easy way to use the directory listing
        /// returned from the sListDir method.
        /// </summary>
        /// <param name="strData">The string data to convert.</param>
        /// <returns>Returns strData converted.</returns>
        [ComVisible(true)]
        public string sConvertCrLf(string strData) { throw new NotImplementedException(); }

        /// <summary>
        /// The sGetCurrentDir method returns the name of the current directory
        /// on the remote system.
        /// </summary>
        /// <returns>Returns the name of the current directory.</returns>
        [ComVisible(true)]
        public string sGetCurrentDir()
        {
            if (!bConnect())
            {
                SetLastError(-1, "No available connection.");
                return String.Empty;
            }

            return af.GetCurrentDirectory();
        }

        /// <summary>
        /// The sGetDirName method returns the name of the item specified by <paramref name="intItem"/>.
        /// 
        /// This method is used after calling the bGetDir method to enumerate a remote directory.
        /// </summary>
        /// <param name="intItem">The name of the nth item in the array of items returned by bGetDir.</param>
        /// <returns>Returns the name of the item specified in intItem.</returns>
        [ComVisible(true)]
        public string sGetDirName(int intItem) { throw new NotImplementedException(); }

        /// <summary>
        /// The sListDir method returns a semi-colon (;) delimited string of
        /// all the items in the directory specified in <paramref name="strDir"/> on the server
        /// specified by the <see cref="sServerName"/> property using the <see cref="sUserID"/> and <see cref="sPassword"/>
        /// properties for the user name and password, respectively.
        /// 
        /// The VBScript Split function or the JScript split method to parse the
        /// individual items from the return string.
        /// 
        /// Alternatively, if you want an HTML compatible directory listing,
        /// use the ConvertCrLf method.
        /// </summary>
        /// <param name="strDir">The directory on the remote server to enumerate.</param>
        /// <param name="intItem">The type of files to enumerate.  UNUSED.</param>
        /// <returns>Returns a semicolon delimited string of all the items in the directory specified in strDir.</returns>
        [ComVisible(true)]
        public string sListDir(string strDir = null, int intType = 0)
        {
            bConnect();
            ClearErrors();

            // JMU's use of this library does not entail using intType at all.
            if (intType > 0)
                throw new ArgumentException("Usage of intType is not supported");

            try
            {
                dirItems = af.EnumerateDirectory(strDir);
                if (dirItems.Length == 0)
                    SetLastError(0, "Empty Directory");
                return String.Join(";", dirItems);
            }
            catch (Exception e)
            {
                SetLastError(e);
            }

            return "";
        }

        /// <summary>
        /// The sReadFile method attempts to read the contents of the file opened
        /// by the bOpenFile method. The ENTIRE file is read and returned.
        /// 
        /// Use this method when wanting to get the contents of a remote file
        /// directly into a string.
        /// </summary>
        /// <returns>Returns the contents of the file specfied in the most recent bOpenFile statement.</returns>
        [ComVisible(true)]
        public string sReadFile() { throw new NotImplementedException(); }


        /// <summary>
        /// Sets the "last error" state as implemented by ASPFTP
        /// </summary>
        /// <param name="e">An <see cref="System.Exception"/> to use to set the error state.</param>
        private void SetLastError(Exception e)
        {
            lastError = e.HResult;
            lastErrorDescription = e.Message;
        }

        /// <summary>
        /// Sets the "last error" state as implemented by ASPFTP.
        /// </summary>
        /// <param name="hResult">The numerical value of the error.</param>
        /// <param name="message">The string message to associate with this error.</param>
        private void SetLastError(int hResult, string message)
        {
            lastError = hResult;
            lastErrorDescription = message;
        }

        /// <summary>
        /// Clears any errors set with <see cref="SetLastError"/>.
        /// </summary>
        private void ClearErrors()
        {
            lastErrorDescription = "";
            lastError = 0;
        }
    }
}
