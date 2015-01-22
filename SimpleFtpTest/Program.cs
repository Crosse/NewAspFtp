using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crosse.Net.NewAspFtp
{
    class Program
    {
        const string PATH_EXISTS = "/pub";
        const string PATH_DNE = "/nonesuch";
        const string DOWNLOADED_FILE = "check_mirror.py";
        const string DOWNLOADED_FILE_PATH = "/" + DOWNLOADED_FILE;
        const string DOWNLOADED_FILE_ASCII_HASH = "3bb0f0a9c9c9b3052192c617455b0eb410950961f50b44cbe50cc50f02edfab3";
        const string DOWNLOADED_FILE_BINARY_HASH = "860353297841477f81301133f7cbe2dff0242e53a8c7f9a033304cc6be111076";

        const string USERNAME = "testuser";
        const string PASSWORD = "";
        const string SERVER = "localhost";

        static void Main(string[] args)
        {
            var caf = new ClassicAspFtp();
            caf.sServerName = SERVER;
            caf.sUserID = USERNAME;
            caf.sPassword = PASSWORD;

            try
            {
                // Try to connect.
                Console.Write("Connecting...");
                if (!caf.bConnect())
                    throw new ApplicationException(caf.sErrorDesc);
                else
                    Console.WriteLine("Connected");

                // Try to verify a valid remote path.
                Console.Write("Verifying that path {0} exists...", PATH_EXISTS);
                if (!caf.bGetDir(PATH_EXISTS))
                    throw new ApplicationException(caf.sErrorDesc);
                else
                    Console.WriteLine(PATH_EXISTS + " exists");

                // Try to verify a nonexistent remote path.
                Console.Write("Verifying that path {0} does not exist...", PATH_DNE);
                if (caf.bGetDir(PATH_DNE))
                    throw new ApplicationException(String.Format("{0} shouldn't exist, but caf says it does", PATH_DNE));
                else
                    Console.WriteLine(caf.sErrorDesc);

                // Because the remote file I'm downloading is a plaintext file with
                // UNIX line endings (\n), it can be used for both the binary
                // and ascii tests. Downloading the file using ASCII transfer
                // will convert the line endings to DOS \r\n endings, which
                // changes the hash of the file.

                // Try to download a remote file using BINARY.
                Console.WriteLine("Downloading {0} using BINARY", DOWNLOADED_FILE_PATH);
                caf.lTransferType = ClassicAspFtp.TRANSFER_TYPE_BINARY;
                DownloadAndVerifyFile(caf);

                // Try to download the remote file using ASCII.
                Console.WriteLine("Downloading {0} using ASCII", DOWNLOADED_FILE_PATH);
                caf.lTransferType = ClassicAspFtp.TRANSFER_TYPE_ASCII;
                DownloadAndVerifyFile(caf);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                caf.bDisconnect();
            }

            Console.WriteLine("Done.  Review any errors.");
            Console.ReadLine();
        }

        public static void DownloadAndVerifyFile(ClassicAspFtp caf)
        {
            RemoveDownloadedFile();

            string wantedHash;
            if (caf.lTransferType == ClassicAspFtp.TRANSFER_TYPE_ASCII)
                wantedHash = DOWNLOADED_FILE_ASCII_HASH;
            else
                wantedHash = DOWNLOADED_FILE_BINARY_HASH;

            if (!caf.bGetFile(DOWNLOADED_FILE_PATH, DOWNLOADED_FILE))
                throw new ApplicationException(caf.sErrorDesc);

            // Check if the downloaded file exists.
            if (!File.Exists(DOWNLOADED_FILE))
                throw new ApplicationException(String.Format("\t{0} does not exist on the local filesystem", DOWNLOADED_FILE));
            else
                Console.WriteLine("\t{0} exists on the local filesystem", DOWNLOADED_FILE);

            // Check the SHA256 hash of the downloaded file.
            using (FileStream fs = File.OpenRead(DOWNLOADED_FILE))
            {
                var sha256 = SHA256Managed.Create();
                fs.Position = 0;
                var hashValue = sha256.ComputeHash(fs);
                var hashString = HashToString(hashValue);
                Console.WriteLine("\tComputed SHA256 hash of downloaded file: " + hashString);
                if (hashString == wantedHash)
                    Console.WriteLine("\tComputed SHA256 hash matches precomputed hash for {0}", DOWNLOADED_FILE);
                else
                {
                    Console.WriteLine("\tComputed SHA256 hash of downloaded file does NOT match precomputed hash!");
                    Console.WriteLine("\tWanted: " + wantedHash);
                    Console.WriteLine("\tGot   : " + hashString);
                    throw new ApplicationException("\tSHA256 hashes did not match");
                }
            }
        }

        public static void RemoveDownloadedFile()
        {
            if (File.Exists(DOWNLOADED_FILE))
            {
                Debug.WriteLine(String.Format("Deleting {0}", DOWNLOADED_FILE));
                File.Delete(DOWNLOADED_FILE);
            }
        }

        // Print the byte array in a readable format. 
        public static string HashToString(byte[] array)
        {
            StringBuilder s = new StringBuilder();
            int i;
            for (i = 0; i < array.Length; i++)
            {
                s.Append(String.Format("{0:x2}", array[i]));
            }
            return s.ToString();
        }
    }
}
