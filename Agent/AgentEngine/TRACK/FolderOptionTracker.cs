using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using AgentEngine;

namespace AgentEngine
{
    /************************************************************************/
    // FolderOptionTracker class  derived from PollingTracker
    // function : monitor action which user manipulate "folder options" of control panel.
    // "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"
    /************************************************************************/
    public class FolderOptionTracker : AgentEngine.PollingTracker
    {
        public FolderOptionTracker(AgentEngine.TrackLog pTrackLog)
        {
#if CONFIG_SERVICE
            bIsAdmin = true;
#endif
            trackLog = pTrackLog;
        }

        protected override bool Init()
        {
            shellState = new Win32.SHELLSTATE();
            Win32.SHGetSetSettings(ref shellState, Win32.SSF.SSF_SHOWALLOBJECTS | Win32.SSF.SSF_SHOWEXTENSIONS | Win32.SSF.SSF_SHOWSUPERHIDDEN, false);
            return true;
        }

        protected override bool TrackLog()
        {
            Win32.SHELLSTATE current_state = new Win32.SHELLSTATE();
            string szLog;

            Win32.SHGetSetSettings(ref current_state, Win32.SSF.SSF_SHOWALLOBJECTS | Win32.SSF.SSF_SHOWEXTENSIONS | Win32.SSF.SSF_SHOWSUPERHIDDEN, false);

            /* SSF_SHOWALLOBJECTS flag check. member is fShowAllObjects  */
            if (shellState.fShowAllObjects != current_state.fShowAllObjects)
            {
                if (current_state.fShowAllObjects != 0)
                    szLog = "\"Show hidden files\" checked.";
                else
                    szLog = "\"Show hidden files\" cleared.";

                trackLog.WriteTrackLog(LOG_TYPE.LOG_FOLDER_OPT_CHANGE, szLog);
                shellState.fShowAllObjects = current_state.fShowAllObjects;
            }

            /* SSF_SHOWEXTENSIONS flag check. member is fShowExtensions  */
            if (shellState.fShowExtensions != current_state.fShowExtensions)
            {
                if (current_state.fShowExtensions != 0)
                    szLog = "\"Hide extension for known file types\" cleared.";
                else
                    szLog = "\"Hide extension for known file types\" checked.";

                trackLog.WriteTrackLog(LOG_TYPE.LOG_FOLDER_OPT_CHANGE, szLog);
                shellState.fShowExtensions = current_state.fShowExtensions;
            }

            /* SSF_SHOWSUPERHIDDEN flag check. member is  fShowSuperHidden */
            if (shellState.fShowSuperHidden != current_state.fShowSuperHidden)
            {
                if (current_state.fShowSuperHidden != 0)
                    szLog = "\"Hide protected operating system files\" cleared.";
                else
                    szLog = "\"Hide protected operating system files\" checked.";

                trackLog.WriteTrackLog(LOG_TYPE.LOG_FOLDER_OPT_CHANGE, szLog);
                shellState.fShowSuperHidden = current_state.fShowSuperHidden;
            }
                        
            return true;
        }

	    /* Variables */
        private Win32.SHELLSTATE shellState;        
#if CONFIG_SERVICE
        public bool       bIsAdmin;
#endif
    }
}
