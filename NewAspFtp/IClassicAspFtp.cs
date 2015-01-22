using System;
using System.Runtime.InteropServices;
namespace Crosse.Net.NewAspFtp
{
    [ComVisible(true)]
    [Guid("cba2301e-eb95-4736-9bf5-1679edf3a420")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    interface IClassicAspFtp
    {
        bool bOverwrite { get; set; }
        bool bPassiveMode { get; set; }
        long lAccessType { get; set; }
        long lErrorNum { get; }
        long lFileAccess { get; set; }
        long lGetDirCount();
        long lTransferType { get; set; }
        string sError { get; }
        string sErrorDesc { get; }
        string sPassword { get; set; }
        string sServerName { get; set; }
        string sUserID { get; set; }

        bool bCloseFile();
        bool bConnect();
        bool bDeleteFile(string strFile);
        bool bDisconnect();
        bool bGetDir(string strDir, int? intType);
        bool bGetFile(string strSourceFile, string strTargetFile);
        bool bMakeDir(string strDir);
        bool bOpenFile(string strFileName);
        bool bPutFile(string strSourceFile, string strTargetFile);
        bool bQDeleteFile(string strServerName, string strUserID, string strPassword, string strFile);
        bool bQGetFile(string strServerName, string strUserID, string strPassword, string strSourceFile, string strTargetFile, long lngTransferType, bool blnOverWrite);
        bool bQMakeDir(string strServerName, string strUserID, string strPassword, string strDir);
        bool bQPutFile(string strServerName, string strUserID, string strPassword, string strSourceFile, string strTargetFile, long lngTransferType);
        bool bQRemoveDir(string strServerName, string strUserID, string strPassword, string strDir);
        bool bQRename(string strServerName, string strUserID, string strPassword, string strCurrentName, string strNewName);
        bool bRemoveDir(string strDir);
        bool bRename(string strCurrentName, string strNewName);
        bool bSetCurrentDir(string strDirName);
        bool bWriteFile(string strData);
        string sConvertCrLf(string strData);
        string sGetCurrentDir();
        string sGetDirName(int intItem);
        string sListDir(string strDir, int intType);
        string sReadFile();
    }
}
