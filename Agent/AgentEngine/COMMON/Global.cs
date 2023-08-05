using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AgentEngine
{
    public class Global
    {
        #region CONST_VALUES
        public const uint MAX_SESSION = 10;

        public const int WM_USER = 0x400;
        public const int WM_USER_TASK_SET_EVENT = WM_USER + 30;
        public const int WM_USER_TASK_GET_EVENT = WM_USER + 31;
        public const int WM_ENDSESSION = 0x0016;

        public const uint MAX_EVENT_LOG_SOURCE = 16;
        public const uint MAX_ADMIN_ITEM_LIST_SIZE = 64;
        public const uint MAX_PROCESS_ENTRY = 128;
        public const uint MAX_INSTALL_LIST = 512;

        public const uint IDT_INIT_TIMER = 400;
        public const uint IDT_CHILD_TIMER = 401;
        public const uint IDT_PROCESS_TRACKER = 402;
        public const uint IDT_EVENT_TRACKER = 403;
        public const uint IDT_SERVICE_TRACKER = 404;
        public const uint IDT_INSTALL_TRACKER = 405;
        public const uint IDT_FOLDER_TRACKER = 406;
        public const uint IDT_TASK_TRACKER = 407;
        public const uint IDT_LICENSE_TIMER = 408;

        private const int LIMIT_LICENSE_TIME = 100;
        private const String LICENSE_FILE = "tracklic.dat";

        private const String CRC_FORMAT = "{0}+{1}-{2}-{3}+{4}+{5}-{6}";
        private const String SAVE_FORMAT = "{0:D7}{1:D4}{2:D2}{3:D2}{4:D2}{5:D2}{6:D2}{7}";

        #endregion

        #region ENUM_STRUCT_DEFINE
        //////////////////////////////////////////////////////////////////////////
        // EntryStatus
        //////////////////////////////////////////////////////////////////////////
        public enum ENTRY_STATUS
        {
            UNUSED_ENTRY,
            USED_ENTRY,
            EXIST_ENTRY,
            CREATED_ENTRY,
        }

        //////////////////////////////////////////////////////////////////////////
        // TrackStatus
        //////////////////////////////////////////////////////////////////////////
        public enum TRACK_STATUS
        {
            TRACK_START,
            TRACK_STOP,
            TRACK_EXIT
        }

        //////////////////////////////////////////////////////////////////////////
        // ProcessEntry Structure
        //////////////////////////////////////////////////////////////////////////
        public struct ProcessEntry
        {
            public int dwPID;				// Process ID
            public int dwSID;			// Session ID

            public ENTRY_STATUS nStatus;	// Entry Status

            public String strProcName; 		// Process Name
            public String strProcPath;

            public DateTime strStartTime;
            public DateTime strEndTime;
        }

        public struct ProcessInfo
        {
            public String strProcName; 		// Process Name
            public PerformanceCounter pfCounter;
        }
        #endregion

        #region FILE_PROCESS_FUNCS
        static uint iSuccNum = 0;
        public static String GetSqlServerName()
        {
            String strFileName;
            String strServer = "";

            try
            {
                strFileName = System.Environment.SystemDirectory + @"\" + AgentEngine.Properties.Resources.INMSINFO_FILE_NAME;

                StreamReader sr = new StreamReader(strFileName);

                strServer = sr.ReadLine();
                sr.Close();
            }
            catch (System.Exception e)
            {
                strServer = "";
                ComMisc.LogErrors(e.ToString());
            }

            return strServer;
        }

        public static bool SetSqlServerName(String strFileName, String strServerName)
        {
            bool blRet = true;
            uint nId = 0;

            try
            {
                if (File.Exists(strFileName))
                {
                    StreamReader sr = new StreamReader(strFileName);

                    if (sr != null)
                    {
                        String strId = sr.ReadLine();
                        nId = (uint)Convert.ToInt32(strId);
                        sr.Close();
                    }
                }

                StreamWriter sw = new StreamWriter(strFileName, false);

                if (sw != null)
                {
                    String strText;
                    strText = String.Format("{0}\r\n{1}", nId, strServerName);
                    sw.Write(strText);
                    sw.Close();
                }
            }
            catch (System.Exception e)
            {
                blRet = false;
                ComMisc.LogErrors(e.ToString());
            }

            return blRet;
        }

        public static uint GetWriteIndex(String strFileName, String strLogFileName)
        {
            uint nRetIndex = 0;
            uint iRead = 0;
            String[] strTexts = new String[2];
            String text = "";
            strTexts[0] = strTexts[1] = "";

            try
            {
                if (File.Exists(strFileName))
                {
                    StreamReader streamRead = new StreamReader(strFileName, false);

                    if (streamRead != null)
                    {
                        while (!streamRead.EndOfStream && iRead < 2)
                        {
                            strTexts[iRead++] = streamRead.ReadLine();
                        }

                        iSuccNum = (uint)Convert.ToInt32(strTexts[0]);
                        streamRead.Close();
                    }
                }
                else
                {
                    iSuccNum = 0;
                    FileStream fs = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite);
                    if (fs != null)
                        fs.Close();
                }

                if (!File.Exists(strLogFileName))
                    iSuccNum = 0;

                nRetIndex = iSuccNum + 1;

                StreamWriter streamWrite = new StreamWriter(strFileName, false);

                if (streamWrite != null)
                {
                    text = String.Format("{0}\r\n{1}", nRetIndex, strTexts[1]);
                    streamWrite.Write(text);
                    streamWrite.Close();
                }
            }
            catch (System.Exception e)
            {
                nRetIndex = iSuccNum + 1;
                ComMisc.LogErrors(e.ToString());
            }

            iSuccNum = nRetIndex;
            return nRetIndex;
        }
        #endregion
        #region LICENSE_FUNCS
        private static void EncodeLicense(ref Byte[] szBuffer, uint dwTotalSecond, ref DateTime CurrentTime, uint dwBufCRC32)
        {
            String strData;
            int year, mon, day;
            int hour, min, sec;
            int i = 0;

            year = CurrentTime.Year;
            mon = CurrentTime.Month;
            day = CurrentTime.Day;
            hour = CurrentTime.Hour;
            min = CurrentTime.Minute;
            sec = CurrentTime.Second;

            year -= 2000;
            mon += 9;
            day += 21;
            hour += 4;
            min += 19;

            strData = String.Format(SAVE_FORMAT, dwTotalSecond, year, mon,
                day, hour, min, sec, dwBufCRC32);

            for (i = 0; i < strData.Length; i++)
                szBuffer[i] = (Byte)strData[i];

            for (; i < szBuffer.Length; i++)
                szBuffer[i] = 0;
        }

        private static String ByteArrToString(Byte[] szBuffer)
        {
            string strRet = "";
            int index = 0;
            while (szBuffer[index] != 0)
            {
                strRet += (Char)szBuffer[index++];
            }

            return strRet;
        }

        private static uint StringToUint(String strValue)
        {
            String strSubValue = "";
            uint uRetVal = 0;

            for (int i = 0; i < strValue.Length; i++)
            {
                Byte bValue = (Byte)strValue[i];
                if (bValue < 0x30 ||
                    bValue > 0x39)
                    break;

                strSubValue += strValue[i];
            }

            if (strSubValue != "")
                uRetVal = uint.Parse(strSubValue);

            return uRetVal;
        }
        //////////////////////////////////////////////////////////////////////////
        // Function Name	: DecodeLicense
        //////////////////////////////////////////////////////////////////////////
        private static void DecodeLicense(ref Byte[] szBuffer, ref uint pdwTimeCounter, ref int nYear, ref int nMonth,
                           ref int nDay, ref int nHour, ref int nMinute, ref int nSecond, ref uint pdwCRC)
        {
            String substr;
            String str;
            str = ByteArrToString(szBuffer);

            if (str.Length < 7)
                return;
            substr = str.Substring(0, 7);
            str = str.Substring(7, str.Length - 7);
            pdwTimeCounter = uint.Parse(substr);

            if (str.Length < 4)
                return;
            substr = str.Substring(0, 4);
            str = str.Substring(4, str.Length - 4);
            nYear = int.Parse(substr);

            if (str.Length < 2)
                return;
            substr = str.Substring(0, 2);
            str = str.Substring(2, str.Length - 2);
            nMonth = int.Parse(substr);

            if (str.Length < 2)
                return;
            substr = str.Substring(0, 2);
            str = str.Substring(2, str.Length - 2);
            nDay = int.Parse(substr);

            if (str.Length < 2)
                return;
            substr = str.Substring(0, 2);
            str = str.Substring(2, str.Length - 2);
            nHour = int.Parse(substr);

            if (str.Length < 2)
                return;
            substr = str.Substring(0, 2);
            str = str.Substring(2, str.Length - 2);
            nMinute = int.Parse(substr);

            if (str.Length < 2)
                return;
            substr = str.Substring(0, 2);
            str = str.Substring(2, str.Length - 2);
            nSecond = int.Parse(substr);

            pdwCRC = StringToUint(str);

            nYear += 2000;
            nMonth -= 9;
            nDay -= 21;
            nHour -= 4;
            nMinute -= 19;
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: LoadLicenseData
        //////////////////////////////////////////////////////////////////////////
        private static bool LoadLicenseData(ref Byte[] szData)
        {
            FileStream LicenseFile = null;
            String szPath;

            Byte[] szBuffer = new Byte[0xFF];
            szPath = System.Environment.SystemDirectory;
            szPath += @"\";
            szPath += LICENSE_FILE;

            if (!File.Exists(szPath))
                return false;

            LicenseFile = new FileStream(szPath, FileMode.Open, FileAccess.Read);
            if (LicenseFile == null)
                return false;

            LicenseFile.Read(szData, 0, 0xFF);
            LicenseFile.Close();

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // Function Name	: WriteLicenseData
        //////////////////////////////////////////////////////////////////////////
        private static bool WriteLicenseData(uint dwTimeCounter)
        {
            FileStream LicenseFile = null;
            String szPath;

            Byte[] szBuffer = new Byte[0xFF];

            szPath = System.Environment.SystemDirectory;
            szPath += @"\";
            szPath += LICENSE_FILE;

            if (!File.Exists(szPath))
                return false;

            LicenseFile = new FileStream(szPath, FileMode.Create, FileAccess.Write);
            if (LicenseFile == null)
                return false;

            /* Write License Information */
            DateTime CurrentTime = DateTime.Now;

            // Calculate CRC
            String strCRC = String.Format(CRC_FORMAT, dwTimeCounter,
                CurrentTime.Year, CurrentTime.Month, CurrentTime.Day,
                CurrentTime.Hour, CurrentTime.Minute, CurrentTime.Second);

            int i, j, nLen = strCRC.Length;
            for (i = 0; i < nLen; i++)
                szBuffer[i] = (Byte)strCRC[i];
            
            for (i = nLen, j = 0; j < nLen; i++, j += 3)
                szBuffer[i] = szBuffer[j];

            szBuffer[i] = 0;
            CRC crcData = new CRC(ref szBuffer, (uint)i);
            uint dwBufCRC32 = crcData.GetCRC();

            EncodeLicense(ref szBuffer, dwTimeCounter, ref CurrentTime, dwBufCRC32);

            LicenseFile.Write(szBuffer, 0, szBuffer.Length);
            LicenseFile.Close();

            return true;
        }

        public static bool CheckValidLicense()
        {
            /* Check License */
            Byte[] szData = new Byte[0xFF];
            uint dwCRC = 0, dwTimeCounter = 0;

            int nYear = 0, nMonth = 0, nDay = 0, nHour = 0, nMinute = 0, nSecond = 0;

            if (!LoadLicenseData(ref szData))
                return false;

            // Check CRC
            DecodeLicense(ref szData, ref dwTimeCounter, ref nYear, ref nMonth,
                ref nDay, ref nHour, ref nMinute, ref nSecond, ref dwCRC);

            String strData;
            strData = String.Format(CRC_FORMAT, dwTimeCounter, nYear, nMonth, nDay, nHour, nMinute, nSecond);

            int i, j, nLen = strData.Length;
            for (i = 0; i < nLen; i++)
                szData[i] = (byte)strData[i];

            for (i = nLen, j = 0; j < nLen; i++, j += 3)
                szData[i] = szData[j];

            szData[i] = 0;
            CRC crcData = new CRC(ref szData, (uint)i);
            uint dwBufCRC32 = crcData.GetCRC();

            if (dwTimeCounter < LIMIT_LICENSE_TIME || dwBufCRC32 != dwCRC)
                return false;

            // Check Last Start Time
            DateTime CurrentTime = DateTime.Now;
            DateTime LastStartTime = new DateTime(nYear, nMonth, nDay, nHour, nMinute, nSecond);

            if (CurrentTime < LastStartTime)
                return false;

            int lDays = (CurrentTime.Year - LastStartTime.Year) * 365 + (CurrentTime.Month - LastStartTime.Month) * 31 + (CurrentTime.Day - LastStartTime.Day);
            int dwGapTime = (CurrentTime.Second - LastStartTime.Second +
                60 * (CurrentTime.Minute - LastStartTime.Minute +
                60 * (CurrentTime.Hour - LastStartTime.Hour +
                24 * lDays)));

            if (dwGapTime <= 0)
                return false;

            int nRemainTime = dwTimeCounter > dwGapTime ? (int)(dwTimeCounter - dwGapTime) : 0;
            if (nRemainTime <= LIMIT_LICENSE_TIME)
                return false;

            if (!WriteLicenseData((uint)nRemainTime))
                return false;

            return true;
        }
        #endregion

        public class FileDirectoryEnumerable : System.Collections.IEnumerable
        {
            private bool bolReturnstringType = true;

            public bool ReturnstringType
            {
                get { return bolReturnstringType; }
                set { bolReturnstringType = value; }
            }

            private string strSearchPattern = "*";

            public string SearchPattern
            {
                get { return strSearchPattern; }
                set { strSearchPattern = value; }
            }
            private string strSearchPath = null;

            public string SearchPath
            {
                get { return strSearchPath; }
                set { strSearchPath = value; }
            }

            private bool bolSearchForFile = true;

            public bool SearchForFile
            {
                get { return bolSearchForFile; }
                set { bolSearchForFile = value; }
            }
            private bool bolSearchForDirectory = true;

            public bool SearchForDirectory
            {
                get { return bolSearchForDirectory; }
                set { bolSearchForDirectory = value; }
            }

            private bool bolThrowIOException = true;

            public bool ThrowIOException
            {
                get { return this.bolThrowIOException; }
                set { this.bolThrowIOException = value; }
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                FileDirectoryEnumerator e = new FileDirectoryEnumerator();
                e.ReturnstringType = this.bolReturnstringType;
                e.SearchForDirectory = this.bolSearchForDirectory;
                e.SearchForFile = this.bolSearchForFile;
                e.SearchPath = this.strSearchPath;
                e.SearchPattern = this.strSearchPattern;
                e.ThrowIOException = this.bolThrowIOException;
                myList.Add(e);
                return e;
            }

            public void Close()
            {
                foreach (FileDirectoryEnumerator e in myList)
                {
                    e.Close();
                }
                myList.Clear();
            }

            private System.Collections.ArrayList myList = new System.Collections.ArrayList();

        }//public class FileDirectoryEnumerable : System.Collections.IEnumerable

        /// <summary>        
        public class FileDirectoryEnumerator : System.Collections.IEnumerator
        {
            private object objCurrentObject = null;

            private bool bolIsEmpty = false;

            public bool IsEmpty
            {
                get { return bolIsEmpty; }
            }
            private int intSearchedCount = 0;

            public int SearchedCount
            {
                get { return intSearchedCount; }
            }
            private bool bolIsFile = true;

            public bool IsFile
            {
                get { return bolIsFile; }
            }
            private int intLastErrorCode = 0;

            public int LastErrorCode
            {
                get { return intLastErrorCode; }
            }

            public string Name
            {
                get
                {
                    if (this.objCurrentObject != null)
                    {
                        if (objCurrentObject is string)
                            return (string)this.objCurrentObject;
                        else
                            return ((System.IO.FileSystemInfo)this.objCurrentObject).Name;
                    }
                    return null;
                }
            }

            public System.IO.FileAttributes Attributes
            {
                get { return (System.IO.FileAttributes)myData.dwFileAttributes; }
            }

            public System.DateTime CreationTime
            {
                get
                {
                    long time = ToLong(myData.ftCreationTime_dwHighDateTime, myData.ftCreationTime_dwLowDateTime);
                    System.DateTime dtm = System.DateTime.FromFileTimeUtc(time);
                    return dtm.ToLocalTime();
                }
            }

            public System.DateTime LastAccessTime
            {
                get
                {
                    long time = ToLong(myData.ftLastAccessTime_dwHighDateTime, myData.ftLastAccessTime_dwLowDateTime);
                    System.DateTime dtm = System.DateTime.FromFileTimeUtc(time);
                    return dtm.ToLocalTime();
                }
            }

            public System.DateTime LastWriteTime
            {
                get
                {
                    long time = ToLong(myData.ftLastWriteTime_dwHighDateTime, myData.ftLastWriteTime_dwLowDateTime);
                    System.DateTime dtm = System.DateTime.FromFileTimeUtc(time);
                    return dtm.ToLocalTime();
                }
            }

            public long FileLength
            {
                get
                {
                    if (this.bolIsFile)
                        return ToLong(myData.nFileSizeHigh, myData.nFileSizeLow);
                    else
                        return 0;
                }
            }

            private bool bolThrowIOException = true;

            public bool ThrowIOException
            {
                get { return this.bolThrowIOException; }
                set { this.bolThrowIOException = value; }
            }
            private bool bolReturnstringType = true;

            public bool ReturnstringType
            {
                get { return bolReturnstringType; }
                set { bolReturnstringType = value; }
            }

            private string strSearchPattern = "*";

            public string SearchPattern
            {
                get { return strSearchPattern; }
                set { strSearchPattern = value; }
            }
            private string strSearchPath = null;

            public string SearchPath
            {
                get { return strSearchPath; }
                set { strSearchPath = value; }
            }

            private bool bolSearchForFile = true;

            public bool SearchForFile
            {
                get { return bolSearchForFile; }
                set { bolSearchForFile = value; }
            }
            private bool bolSearchForDirectory = true;

            public bool SearchForDirectory
            {
                get { return bolSearchForDirectory; }
                set { bolSearchForDirectory = value; }
            }

            public void Close()
            {
                this.CloseHandler();
            }

            public object Current
            {
                get { return objCurrentObject; }
            }

            public bool MoveNext()
            {
                bool success = false;
                while (true)
                {
                    if (this.bolStartSearchFlag)
                        success = this.SearchNext();
                    else
                        success = this.StartSearch();
                    if (success)
                    {
                        if (this.UpdateCurrentObject())
                            return true;
                    }
                    else
                    {
                        this.objCurrentObject = null;
                        return false;
                    }
                }
            }

            public void Reset()
            {
                if (this.strSearchPath == null)
                    throw new System.ArgumentNullException("SearchPath can not null");
                if (this.strSearchPattern == null || this.strSearchPattern.Length == 0)
                    this.strSearchPattern = "*";

                this.intSearchedCount = 0;
                this.objCurrentObject = null;
                this.CloseHandler();
                this.bolStartSearchFlag = false;
                this.bolIsEmpty = false;
                this.intLastErrorCode = 0;
            }


            [Serializable,
                System.Runtime.InteropServices.StructLayout
                    (System.Runtime.InteropServices.LayoutKind.Sequential,
                    CharSet = System.Runtime.InteropServices.CharSet.Auto
                    ),
                System.Runtime.InteropServices.BestFitMapping(false)]
            private struct WIN32_FIND_DATA
            {
                public int dwFileAttributes;
                public int ftCreationTime_dwLowDateTime;
                public int ftCreationTime_dwHighDateTime;
                public int ftLastAccessTime_dwLowDateTime;
                public int ftLastAccessTime_dwHighDateTime;
                public int ftLastWriteTime_dwLowDateTime;
                public int ftLastWriteTime_dwHighDateTime;
                public int nFileSizeHigh;
                public int nFileSizeLow;
                public int dwReserved0;
                public int dwReserved1;
                [System.Runtime.InteropServices.MarshalAs
                     (System.Runtime.InteropServices.UnmanagedType.ByValTStr,
                     SizeConst = 260)]
                public string cFileName;
                [System.Runtime.InteropServices.MarshalAs
                     (System.Runtime.InteropServices.UnmanagedType.ByValTStr,
                     SizeConst = 14)]
                public string cAlternateFileName;
            }

            [System.Runtime.InteropServices.DllImport
                 ("kernel32.dll",
                 CharSet = System.Runtime.InteropServices.CharSet.Auto,
                 SetLastError = true)]
            private static extern IntPtr FindFirstFile(string pFileName, ref WIN32_FIND_DATA pFindFileData);

            [System.Runtime.InteropServices.DllImport
                 ("kernel32.dll",
                 CharSet = System.Runtime.InteropServices.CharSet.Auto,
                 SetLastError = true)]
            private static extern bool FindNextFile(IntPtr hndFindFile, ref WIN32_FIND_DATA lpFindFileData);

            [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool FindClose(IntPtr hndFindFile);

            private static long ToLong(int height, int low)
            {
                long v = (uint)height;
                v = v << 0x20;
                v = v | ((uint)low);
                return v;
            }

            private static void WinIOError(int errorCode, string str)
            {
                switch (errorCode)
                {
                    case 80:
                        throw new System.IO.IOException("IO_FileExists :" + str);
                    case 0x57:
                        throw new System.IO.IOException("IOError:" + MakeHRFromErrorCode(errorCode));
                    case 0xce:
                        throw new System.IO.PathTooLongException("PathTooLong:" + str);
                    case 2:
                        throw new System.IO.FileNotFoundException("FileNotFound:" + str);
                    case 3:
                        throw new System.IO.DirectoryNotFoundException("PathNotFound:" + str);
                    case 5:
                        throw new UnauthorizedAccessException("UnauthorizedAccess:" + str);
                    case 0x20:
                        throw new System.IO.IOException("IO_SharingViolation:" + str);
                }
                throw new System.IO.IOException("IOError:" + MakeHRFromErrorCode(errorCode));
            }

            private static int MakeHRFromErrorCode(int errorCode)
            {
                return (-2147024896 | errorCode);
            }

            private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            private System.IntPtr intSearchHandler = INVALID_HANDLE_VALUE;

            private WIN32_FIND_DATA myData = new WIN32_FIND_DATA();

            private bool bolStartSearchFlag = false;

            private void CloseHandler()
            {
                if (this.intSearchHandler != INVALID_HANDLE_VALUE)
                {
                    FindClose(this.intSearchHandler);
                    this.intSearchHandler = INVALID_HANDLE_VALUE;
                }
            }

            private bool StartSearch()
            {
                bolStartSearchFlag = true;
                bolIsEmpty = false;
                objCurrentObject = null;
                intLastErrorCode = 0;

                string strPath = System.IO.Path.Combine(strSearchPath, this.strSearchPattern);
                this.CloseHandler();
                intSearchHandler = FindFirstFile(strPath, ref myData);
                if (intSearchHandler == INVALID_HANDLE_VALUE)
                {
                    intLastErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    if (intLastErrorCode == 2)
                    {
                        bolIsEmpty = true;
                        return false;
                    }
                    if (this.bolThrowIOException)
                        WinIOError(intLastErrorCode, strSearchPath);
                    else
                        return false;
                }
                return true;
            }

            private bool SearchNext()
            {
                if (bolStartSearchFlag == false)
                    return false;
                if (bolIsEmpty)
                    return false;
                if (intSearchHandler == INVALID_HANDLE_VALUE)
                    return false;
                intLastErrorCode = 0;
                if (FindNextFile(intSearchHandler, ref myData) == false)
                {
                    intLastErrorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    this.CloseHandler();
                    if (intLastErrorCode != 0 && intLastErrorCode != 0x12)
                    {
                        if (this.bolThrowIOException)
                            WinIOError(intLastErrorCode, strSearchPath);
                        else
                            return false;
                    }
                    return false;
                }
                return true;
            }//private bool SearchNext()


            private bool UpdateCurrentObject()
            {
                if (intSearchHandler == INVALID_HANDLE_VALUE)
                    return false;
                bool Result = false;
                this.objCurrentObject = null;
                if ((myData.dwFileAttributes & 0x10) == 0)
                {
                    this.bolIsFile = true;
                    if (this.bolSearchForFile)
                        Result = true;
                }
                else
                {
                    this.bolIsFile = false;
                    if (this.bolSearchForDirectory)
                    {
                        if (myData.cFileName == "." || myData.cFileName == "..")
                            Result = false;
                        else
                            Result = true;
                    }
                }
                if (Result)
                {
                    if (this.bolReturnstringType)
                        this.objCurrentObject = myData.cFileName;
                    else
                    {
                        string p = System.IO.Path.Combine(this.strSearchPath, myData.cFileName);
                        if (this.bolIsFile)
                        {
                            this.objCurrentObject = new System.IO.FileInfo(p);
                        }
                        else
                        {
                            this.objCurrentObject = new System.IO.DirectoryInfo(p);
                        }
                    }
                    this.intSearchedCount++;
                }
                return Result;
            }//private bool UpdateCurrentObject()          

        }//public class FileDirectoryEnumerator : System.Collections.IEnumerator


        #region Signitures imported from http://pinvoke.net

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        [Flags()]
        enum SLGP_FLAGS
        {
            /// <summary>Retrieves the standard short (8.3 format) file name</summary>
            SLGP_SHORTPATH = 0x1,
            /// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
            SLGP_UNCPRIORITY = 0x2,
            /// <summary>Retrieves the raw path name. A raw path is something that might not exist and may include environment variables that need to be expanded</summary>
            SLGP_RAWPATH = 0x4
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public long ftCreationTime;
            public long ftLastAccessTime;
            public long ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [Flags()]
        enum SLR_FLAGS
        {
            /// </summary>
            SLR_NO_UI = 0x1,
            /// <summary>Obsolete and no longer used</summary>
            SLR_ANY_MATCH = 0x2,
            /// <summary>If the link object has changed, update its path and list of identifiers.
            /// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
            /// whether or not the link object has changed.</summary>
            SLR_UPDATE = 0x4,
            /// <summary>Do not update the link information</summary>
            SLR_NOUPDATE = 0x8,
            /// <summary>Do not execute the search heuristics</summary>
            SLR_NOSEARCH = 0x10,
            /// <summary>Do not use distributed link tracking</summary>
            SLR_NOTRACK = 0x20,
            /// <summary>Disable distributed link tracking. By default, distributed link tracking tracks
            /// removable media across multiple devices based on the volume name. It also uses the
            /// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
            /// has changed. Setting SLR_NOLINKINFO disables both types of tracking.</summary>
            SLR_NOLINKINFO = 0x40,
            /// <summary>Call the Microsoft Windows Installer</summary>
            SLR_INVOKE_MSI = 0x80
        }


        /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
        interface IShellLinkW
        {
            /// <summary>Retrieves the path and file name of a Shell link object</summary>
            void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);
            /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
            void GetIDList(out IntPtr ppidl);
            /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
            void SetIDList(IntPtr pidl);
            /// <summary>Retrieves the description string for a Shell link object</summary>
            void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
            void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            /// <summary>Sets the name of the working directory for a Shell link object</summary>
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
            void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            /// <summary>Sets the command-line arguments for a Shell link object</summary>
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            /// <summary>Retrieves the hot key for a Shell link object</summary>
            void GetHotkey(out short pwHotkey);
            /// <summary>Sets a hot key for a Shell link object</summary>
            void SetHotkey(short wHotkey);
            /// <summary>Retrieves the show command for a Shell link object</summary>
            void GetShowCmd(out int piShowCmd);
            /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
            void SetShowCmd(int iShowCmd);
            /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
            void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath, out int piIcon);
            /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            /// <summary>Sets the relative path to the Shell link object</summary>
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
            void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);
            /// <summary>Sets the path and file name of a Shell link object</summary>
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);

        }

        [ComImport, Guid("0000010c-0000-0000-c000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersist
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
        }


        [ComImport, Guid("0000010b-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistFile : IPersist
        {
            new void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();

            [PreserveSig]
            void Load([In, MarshalAs(UnmanagedType.LPWStr)]
            string pszFileName, uint dwMode);

            [PreserveSig]
            void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

            [PreserveSig]
            void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            [PreserveSig]
            void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
        }

        const uint STGM_READ = 0;
        const int MAX_PATH = 260;

        // CLSID_ShellLink from ShlGuid.h 
        [
            ComImport(),
            Guid("00021401-0000-0000-C000-000000000046")
        ]
        public class ShellLink
        {
        }


        //////////////////////////////////////////////////////////////////////////
        // Function Name	: SetPrivilege
        //////////////////////////////////////////////////////////////////////////
        public static bool SetPrivilege(IntPtr hToken, string lpszPrivilege, bool bEnablePrivilege)
        {
            Win32.TokPriv1Luid tp;
            long luid = 0;

            if (!Win32.LookupPrivilegeValue(null, lpszPrivilege, ref luid))        // receives LUID of privilege
            {
                //printf("LookupPrivilegeValue error: %u\n", Win32.GetLastError());
                return false;
            }

            tp.Count = 1;
            tp.Luid = luid;
            if (bEnablePrivilege)
                tp.Attr = Win32.SE_PRIVILEGE_ENABLED;
            else
                tp.Attr = 0;

            // Enable the privilege or disable all privileges.

            if (!Win32.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                //printf("AdjustTokenPrivileges error: %u\n", GetLastError());
                return false;
            }

            if (Win32.GetLastError() == Win32.ERROR_NOT_ALL_ASSIGNED)
            {
                //printf("The token does not have the specified privilege. \n");
                return false;
            }

            return true;
        }

        #endregion

        public static string ResolveShortcut(string filename)
        {
            ShellLink link = new ShellLink();
            ((IPersistFile)link).Load(filename, STGM_READ);
            // TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
            // ((IShellLinkW)link).Resolve(hwnd, 0) 
            StringBuilder sb = new StringBuilder(MAX_PATH);
            WIN32_FIND_DATAW data = new WIN32_FIND_DATAW();
            ((IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);
            return sb.ToString();
        }

        public static byte[] WriteStringBytes(String str, FileStream fs)
        {
            str += "\r\n";
            byte[] info = new UTF8Encoding().GetBytes(str);
            fs.Write(info, 0, info.Length);
            return info;
        }

        public static int bMatchPattern(byte[] pBuf, int nstart)
        {
            if (pBuf[nstart] == 0x55 && pBuf[nstart + 1] == 0x52 && pBuf[nstart + 2] == 0x4c && pBuf[nstart + 3] == 0x20 &&
                pBuf[nstart + 5] == 0x00 && pBuf[nstart + 6] == 0x00)
            {
                return Win32.TYPE_URL;
            }

            return 0;

        }

        public static bool bMatchFinishItem(byte[] pBuf, int nstart)
        {
            if (pBuf[nstart] == 0xEF && pBuf[nstart + 1] == 0xBE)
            {
                return true;
            }

            return false;
        }

        public static bool bMatchTitle(byte[] pBuf, int nstart)
        {
            if (pBuf[nstart] == 0x10 && pBuf[nstart + 1] == 0x1F)
            {
                return true;
            }

            return false;
        }

        public static Win32.history getURL(byte[] pBuf, int nType, ref int nReadSize)
        {
            Win32.history retitem = new Win32.history();

            if (nType == Win32.TYPE_URL)
            {
                long lValue = (long)BitConverter.ToInt64(pBuf,nReadSize + Win32.URL_TIME_OFFSET);
                double dValue = lValue;
                DateTime dtZone = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dtZone = dtZone.AddSeconds(dValue / 10000000);//10^7 ,AddSeconds method needs a double
                dtZone = dtZone.ToLocalTime();

                retitem.st = dtZone;
                retitem.nType = Win32.TYPE_URL;

                nReadSize += Win32.URL_URL_OFFSET;
            }
            else
            {
                return null;
            }

            int i = 0;
            while (pBuf[nReadSize + i] != 0)
            {
                i++;
            }

            if (i > 1024)
                return null;

            retitem.pURL = Encoding.UTF8.GetString(pBuf, nReadSize, i);

            if (retitem.pURL.IndexOf("@") == -1)
                return null;

            retitem.pURL = retitem.pURL.Substring(retitem.pURL.IndexOf("@") + 1);
            nReadSize = nReadSize + i;

            retitem.pTitle = "";

            bool bFinishItem = false;
            i = 0;
            while ((bFinishItem = bMatchFinishItem(pBuf, nReadSize + i)) == false)
            {
                if (bMatchTitle(pBuf, nReadSize + i) == true)
                    break;
                i++;
            }

            if (bFinishItem == false)
            {
                nReadSize += i;
                nReadSize += 2;

                i = 0;
                while (pBuf[nReadSize + i] != 0 || pBuf[nReadSize + i + 1] != 0)
                {
                    i++;
                }

                if (i % 2 == 1)
                    i++;
                retitem.pTitle = Encoding.Unicode.GetString(pBuf, nReadSize, i);
                nReadSize = nReadSize + i;
            }
            return retitem;
        }

        // HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\USBSTOR\Start Key is used to change usb state. 3: enable, 4: disable
        public static String subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

        public static void SetAllowUAC(bool bEnable)
        {
            // Open Registry Key
            RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true);

            int nEnable = 1;
            if (!bEnable) nEnable = 0;

            // Set Start Key Value
            key.SetValue("EnableLUA", nEnable);

            // Close Registry Key
            key.Close();
        }
    }

    #region POLLINGTRACKER
    /************************************************************************/
    // PollingTracker
    /************************************************************************/
    public class PollingTracker
    {
        public PollingTracker() { trackLog = new TrackLog(); bInit = false; }

        public void Start() { bInit = Init(); }
        public bool IsStart() { return bInit; }

        public void Stop() { bInit = false; }
        public bool IsStop() { return !bInit; }

        public bool OnTracking() { return !IsStart() ? false : TrackLog(); }

        protected virtual bool Init() { return true; }
        protected virtual bool TrackLog() { return true; }

        protected bool bInit;
        protected TrackLog trackLog;
    }
    #endregion

}
