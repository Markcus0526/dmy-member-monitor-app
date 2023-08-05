using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using INMC.INMMessage;

namespace AgentEngine
{
    public class Win32
    {
        public const int TYPE_URL = 0x01;
        public const int TYPE_REDR = 0x02;

        public const int URL_URL_OFFSET = 104;
        public const int URL_TIME_OFFSET = 16;
        public const int REDR_URL_OFFSET = 16;

        public class history
        {
            public history()
            {
            }

            public int nType;
            public String pURL;
            public String pTitle;
            public DateTime st;
        };

        public enum NetJoinStatus
        {
            NetSetupUnknownStatus = 0,
            NetSetupUnjoined,
            NetSetupWorkgroupName,
            NetSetupDomainName
        }

        public enum TCP_TABLE_CLASS : int
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public byte localPort1;
            public byte localPort2;
            public byte localPort3;
            public byte localPort4;
            public uint remoteAddr;
            public byte remotePort1;
            public byte remotePort2;
            public byte remotePort3;
            public byte remotePort4;
            public int owningPid;

            public ushort LocalPort
            {
                get
                {
                    return BitConverter.ToUInt16(
                        new byte[2] { localPort2, localPort1 }, 0);
                }
            }

            public ushort RemotePort
            {
                get
                {
                    return BitConverter.ToUInt16(
                        new byte[2] { remotePort2, remotePort1 }, 0);
                }
            }

            public String State
            {
                get
                {
                    return Win32.convert_state(state);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            MIB_TCPROW_OWNER_PID table;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, int reserved);

        public static String convert_state(uint state)
        {
            String strg_state = "";
            switch (state)
            {
                case MIB_TCP_STATE_CLOSED: strg_state = "CLOSED"; break;
                case MIB_TCP_STATE_LISTEN: strg_state = "LISTEN"; break;
                case MIB_TCP_STATE_SYN_SENT: strg_state = "SYN_SENT"; break;
                case MIB_TCP_STATE_SYN_RCVD: strg_state = "SYN_RCVD"; break;
                case MIB_TCP_STATE_ESTAB: strg_state = "ESTAB"; break;
                case MIB_TCP_STATE_FIN_WAIT1: strg_state = "FIN_WAIT1"; break;
                case MIB_TCP_STATE_FIN_WAIT2: strg_state = "FIN_WAIT2"; break;
                case MIB_TCP_STATE_CLOSE_WAIT: strg_state = "CLOSE_WAIT"; break;
                case MIB_TCP_STATE_CLOSING: strg_state = "CLOSING"; break;
                case MIB_TCP_STATE_LAST_ACK: strg_state = "LAST_ACK"; break;
                case MIB_TCP_STATE_TIME_WAIT: strg_state = "TIME_WAIT"; break;
                case MIB_TCP_STATE_DELETE_TCB: strg_state = "DELETE_TCB"; break;
            }
            return strg_state;
        }

        public static MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
        {
            MIB_TCPROW_OWNER_PID[] tTable;
            int AF_INET = 2;
            // IP_v4
            int buffSize = 0;

            // how much memory do we need?
            uint ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            if (ret != 0 && ret != 122) // 122 insufficient buffer size
                throw new Exception("bad ret on check " + ret);
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = GetExtendedTcpTable(buffTable, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                if (ret != 0)
                    throw new Exception("bad ret " + ret);
                // get the number of entries in the table
                MIB_TCPTABLE_OWNER_PID tab = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                tTable = new MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

                for (int i = 0; i < tab.dwNumEntries; i++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tTable[i] = tcpRow;
                    // next entry
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            finally
            {
                // Free the Memory
                Marshal.FreeHGlobal(buffTable);
            }
            return tTable;
        }

        private const int MIB_TCP_STATE_CLOSED = 1;
        private const int MIB_TCP_STATE_LISTEN = 2;
        private const int MIB_TCP_STATE_SYN_SENT = 3;
        private const int MIB_TCP_STATE_SYN_RCVD = 4;
        private const int MIB_TCP_STATE_ESTAB = 5;
        private const int MIB_TCP_STATE_FIN_WAIT1 = 6;
        private const int MIB_TCP_STATE_FIN_WAIT2 = 7;
        private const int MIB_TCP_STATE_CLOSE_WAIT = 8;
        private const int MIB_TCP_STATE_CLOSING = 9;
        private const int MIB_TCP_STATE_LAST_ACK = 10;
        private const int MIB_TCP_STATE_TIME_WAIT = 11;
        private const int MIB_TCP_STATE_DELETE_TCB = 12;
        public const int ErrorSuccess = 0;

        [DllImport("Netapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int NetGetJoinInformation(
            string server, 
            out string domain, 
            out NetJoinStatus status);

        [DllImport("Netapi32.dll")]
        public static extern int NetApiBufferFree(string Buffer);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(
            IntPtr htok, 
            bool disall, 
            ref TokPriv1Luid newst, 
            int len, 
            IntPtr prev, 
            IntPtr relen);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(
            string host, 
            string name, 
            ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool OpenProcessToken(
            IntPtr h, 
            int acc, 
            ref IntPtr phtok);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int TOKEN_QUERY = 0x00000008;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetLastError();

        public const uint ERROR_OPERATION_ABORTED = 995;
        public const uint ERROR_NOT_ALL_ASSIGNED = 1300;

        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_READ = 0x0010;

        /// <summary>
        /// wtsapi32.dll
        /// </summary>

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        public static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(
            IntPtr hServer, 
            int sessionId, 
            WTS_INFO_CLASS wtsInfoClass, 
            out IntPtr ppBuffer, 
            out uint pBytesReturned);

        [DllImport("wtsapi32.dll")]
        public static extern bool WTSEnumerateSessions(
            IntPtr hServer, 
            int Reserved, 
            int Version,
            ref IntPtr ppSessionInfo,
            ref int pCount);

        [DllImport("Wtsapi32.dll", CharSet = CharSet.Auto)]
        public static extern bool WTSWaitSystemEvent(
            IntPtr hServer, 
            uint evenMask, 
            out uint pEventFlags);

        [DllImport("Wtsapi32.dll", CharSet = CharSet.Auto)]
        public static extern UInt32 WTSQueryUserToken(
            Int32 SessionId, 
            ref IntPtr phToken);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        public extern static bool DuplicateTokenEx(
            IntPtr hExistingToken,
            Int32 dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpTokenAttributes,//ref SECURITY_ATTRIBUTES lpTokenAttributes,
            Int32 ImpersonationLevel,
            Int32 TokenType,
            ref IntPtr phNewToken);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern  bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,//String lpApplicationName, 
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,//SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,//SECURITY_ATTRIBUTES lpThreadAttributes, 
            bool bInheritHandle,
            Int32 dwCreationFlags, 
            IntPtr lpEnvironment,
            string lpCurrentDirectory,  //String lpCurrentDirectory, 
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }
        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }
        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public Int32 dwProcessId;
            public Int32 dwThreadId;
        }

        public const uint WTS_EVENT_NONE = 0x00000000; // return no event
        public const uint WTS_EVENT_CREATE = 0x00000001; // new WinStation created
        public const uint WTS_EVENT_DELETE = 0x00000002; // existing WinStation deleted
        public const uint WTS_EVENT_RENAME = 0x00000004; // existing WinStation renamed
        public const uint WTS_EVENT_CONNECT = 0x00000008; // WinStation connect to client
        public const uint WTS_EVENT_DISCONNECT = 0x00000010; // WinStation logged on without
        public const uint WTS_EVENT_LOGON = 0x00000020; // user logged on to existing
        public const uint WTS_EVENT_LOGOFF = 0x00000040; // user logged off from
        public const uint WTS_EVENT_STATECHANGE = 0x00000080; // WinStation state change
        public const uint WTS_EVENT_LICENSE = 0x00000100; // license state change
        public const uint WTS_EVENT_ALL = 0x7FFFFFFF; // wait for all event types
        public const uint WTS_EVENT_FLUSH = 0x80000000; // unblock all waiters

        public const int IDLE_PRIORITY_CLASS = 0x40;
        public const int NORMAL_PRIORITY_CLASS = 0x20;
        public const int HIGH_PRIORITY_CLASS = 0x80;
        public const int REALTIME_PRIORITY_CLASS = 0x100;

        public const int CREATE_NEW_CONSOLE = 0x00000010;
        public const int MAXIMUM_ALLOWED = 0x02000000;

        public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        public struct WTS_CLIENT_ADDRESS
        {
            public UInt32 AddressFamily;  // AF_INET, AF_IPX, AF_NETBIOS, AF_UNSPEC
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;    // client network address
        }

        internal static void WTSFreeMemory(object pOldResource)
        {
            throw new NotImplementedException();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [DllImport("shell32.dll")]
        public extern static void SHGetSetSettings(
            ref SHELLSTATE lpss,
            SSF dwMask, 
            bool bSet);

        [Flags]
        public enum SSF
        {
            SSF_SHOWALLOBJECTS = 0x00000001,
            SSF_SHOWEXTENSIONS = 0x00000002,
            SSF_HIDDENFILEEXTS = 0x00000004,
            SSF_SERVERADMINUI = 0x00000004,
            SSF_SHOWCOMPCOLOR = 0x00000008,
            SSF_SORTCOLUMNS = 0x00000010,
            SSF_SHOWSYSFILES = 0x00000020,
            SSF_DOUBLECLICKINWEBVIEW = 0x00000080,
            SSF_SHOWATTRIBCOL = 0x00000100,
            SSF_DESKTOPHTML = 0x00000200,
            SSF_WIN95CLASSIC = 0x00000400,
            SSF_DONTPRETTYPATH = 0x00000800,
            SSF_SHOWINFOTIP = 0x00002000,
            SSF_MAPNETDRVBUTTON = 0x00001000,
            SSF_NOCONFIRMRECYCLE = 0x00008000,
            SSF_HIDEICONS = 0x00004000,
            SSF_FILTER = 0x00010000,
            SSF_WEBVIEW = 0x00020000,
            SSF_SHOWSUPERHIDDEN = 0x00040000,
            SSF_SEPPROCESS = 0x00080000,
            SSF_NONETCRAWLING = 0x00100000,
            SSF_STARTPANELON = 0x00200000,
            SSF_SHOWSTARTPAGE = 0x00400000
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct SHELLSTATE
        {
            //BOOL fShowAllObjects:1;  
            //BOOL fShowExtensions:1;  
            //BOOL fNoConfirmRecycle:1;  
            //BOOL fShowSysFiles:1;  
            //BOOL fShowCompColor:1;  
            //BOOL fDoubleClickInWebView:1;  
            //BOOL fDesktopHTML:1;  
            //BOOL fWin95Classic:1;  
            //BOOL fDontPrettyPath:1;  
            //BOOL fShowAttribCol:1;  
            //BOOL fMapNetDrvBtn:1;  
            //BOOL fShowInfoTip:1;  
            //BOOL fHideIcons:1;  
            //BOOL fWebView:1;  
            //BOOL fFilter:1;  
            //BOOL fShowSuperHidden:1;  
            //BOOL fNoNetCrawling:1;  
            ///public uint bitvector1;//32bits  

            //DWORD dwWin95Unused;  
            //UINT uWin95Unused;  
            //LONG lParamSort;  
            //int iSortDirection;  
            //UINT version;  
            //UINT uNotUsed;  


            //BOOL fSepProcess:1;  
            //BOOL fStartPanelOn:1;  
            //BOOL fShowStartPage:1;  
            //BOOL fAutoCheckSelect:1;  
            //BOOL fIconsOnly:1;  
            //BOOL fShowTypeOverlay:1;  
            //UINT fSpareFlags:13;  
            /// <summary> 
            ///  public uint bitvector2;//32bits  
            /// </summary> 

            public uint bitvector1;//32bits  
            public uint dwWin95Unused;
            public uint uWin95Unused;
            public int lParamSort;
            public int iSortDirection;
            public uint version;
            public uint uNotUsed;
            public uint bitvector2;//32bits  

            public uint fShowAllObjects
            {
                get
                {
                    return ((uint)((this.bitvector1 & 1u)));
                }
                set
                {
                    this.bitvector1 = ((uint)((value | this.bitvector1)));
                }
            }
            public uint fShowExtensions
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2u) / 2)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2) | this.bitvector1)));
                }
            }
            public uint fShowSysFiles
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8u) / 8)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8) | this.bitvector1)));
                }
            }
            public uint fShowSuperHidden
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8000u) / 8000u)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8000u) | this.bitvector1)));
                }
            }
        }

        public enum SID_NAME_USE 
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3,
        }

        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }


        [DllImport("userenv.dll", SetLastError = true)]
        public static extern bool CreateEnvironmentBlock(
            ref IntPtr lpEnvironment, 
            IntPtr hToken, 
            bool bInherit);

        public enum PROCESS_INFORMATION_CLASS : int
        {
            ProcessBasicInformation,
            ProcessQuotaLimits,
            ProcessIoCounters,
            ProcessVmCounters,
            ProcessTimes,
            ProcessBasePriority,
            ProcessRaisePriority,
            ProcessDebugPort,
            ProcessExceptionPort,
            ProcessAccessToken,
            ProcessLdtInformation,
            ProcessLdtSize,
            ProcessDefaultHardErrorMode,
            ProcessIoPortHandlers,
            ProcessPooledUsageAndLimits,
            ProcessWorkingSetWatch,
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup,
            ProcessPriorityClass,
            ProcessWx86Information,
            ProcessHandleCount,
            ProcessAffinityMask,
            ProcessPriorityBoost,
            ProcessDeviceMap,
            ProcessSessionInformation,
            ProcessForegroundInformation,
            ProcessWow64Information,
            ProcessImageFileName,
            ProcessLUIDDeviceMapsEnabled,
            ProcessBreakOnTermination,
            ProcessDebugObjectHandle,
            ProcessDebugFlags,
            ProcessHandleTracing,
            ProcessIoPriority,
            ProcessExecuteFlags,
            ProcessResourceManagement,
            ProcessCookie,
            ProcessImageInformation,
            ProcessCycleTime,
            ProcessPagePriority,
            ProcessInstrumentationCallback,
            ProcessThreadStackAllocation,
            ProcessWorkingSetWatchEx,
            ProcessImageFileNameWin32,
            ProcessImageFileMapping,
            ProcessAffinityUpdateMode,
            ProcessMemoryAllocationMode,
            MaxProcessInfoClass
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_BASIC_INFORMATION
        {
            public int ExitStatus;
            public int PebBaseAddress;
            public int AffinityMask;
            public int BasePriority;
            public int UniqueProcessId;
            public int InheritedFromUniqueProcessId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PEB
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	        public uint[] dwFiller; //4
	        public uint dwInfoBlockAddress;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct INFOBLOCK
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	        public uint[] dwFiller; //[16]
            public ushort wLength;
            public ushort wMaxLength;
            public uint dwCmdLineAddress;
        };

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public struct SYSTEMTIME
        {
            public Int16 Year;
            public Int16 Month;
            public Int16 DayOfWeek;
            public Int16 Day;
            public Int16 Hour;
            public Int16 Minute;
            public Int16 Second;
            public Int16 Milliseconds;
        }

        //Retrieves information about an object in the file system.
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public const uint SHGFI_ATTR_SPECIFIED =
            0x20000;
        public const uint SHGFI_ATTRIBUTES = 0x800;
        public const uint SHGFI_PIDL = 0x8;
        public const uint SHGFI_DISPLAYNAME =
            0x200;
        public const uint SHGFI_USEFILEATTRIBUTES
            = 0x10;
        public const uint FILE_ATTRIBUTRE_NORMAL =
            0x4000;
        public const uint SHGFI_EXETYPE = 0x2000;
        public const uint SHGFI_SYSICONINDEX =
            0x4000;
        public const uint ILC_COLORDDB = 0x1;
        public const uint ILC_MASK = 0x0;
        public const uint ILD_TRANSPARENT = 0x1;
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;
        public const uint SHGFI_SHELLICONSIZE =
            0x4;
        public const uint SHGFI_SMALLICON = 0x1;
        public const uint SHGFI_TYPENAME = 0x400;
        public const uint SHGFI_ICONLOCATION =
            0x1000;


        // ************ ScreenCapture ***************** //
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        public const Int32 CURSOR_SHOWING = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 
            public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 
            public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 
            public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 
            public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(Int32 ptr);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", EntryPoint = "CopyIcon")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetIconInfo")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        /// /////////////////////////////////////////////////////////////////////
        [DllImport("gdi32.dll", EntryPoint = "CreateDC")]
        public static extern IntPtr CreateDC(IntPtr lpszDriver, string lpszDevice, IntPtr lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest,
                                         int yDest, int wDest,
                                         int hDest, IntPtr hdcSource,
                                         int xSrc, int ySrc, int RasterOp);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap
                                    (IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        public const int SRCCOPY = 0xCC0020;

        /// <summary>
        /// filter function
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        /// <summary>
        /// check if windows visible
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// enumarator on all desktop windows
        /// </summary>
        /// <param name="hDesktop"></param>
        /// <param name="lpEnumCallbackFunction"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
        // *************************************************************************** //
    }



    /// <summary>
    /// Contains information about a file object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };



    /// <summary>
    /// The helper class to sort in ascending order by FileTime(LastVisited).
    /// </summary>
    public class SortFileTimeAscendingHelper : IComparer
    {
        int IComparer.Compare(object a, object b)
        {
            M7WebHisItem c1 = (M7WebHisItem)a;
            M7WebHisItem c2 = (M7WebHisItem)b;

            return c1.strAccessTime.CompareTo(c2.strAccessTime);
        }

        public static IComparer SortFileTimeAscending()
        {
            return (IComparer)new SortFileTimeAscendingHelper();
        } 
    }
}
