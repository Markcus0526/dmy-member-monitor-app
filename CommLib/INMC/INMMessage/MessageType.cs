using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INMC.INMMessage
{
    /// <summary>
    /// Represents text style of messages.
    /// </summary>
    [Serializable]
    public class MessageType
    {
        /// <summary>
        /// 
        /// </summary>
        public enum KIND
        {
            /// <summary>
            /// 
            /// </summary>
            MSG_NOEFFECT = 0,

            /// <summary>
            /// 
            /// </summary>
            MSG0_AGENTINFO = 1,

            ///
            MSG0_TIMESTAMP,

            /// <summary>
            /// 
            /// </summary>
            MSG1_PROHIBITSET,

            /// <summary>
            /// 
            /// </summary>
            MSG1_APPDISABLESET,

            /// <summary>
            /// 
            /// </summary>
            MSG1_BANDMON,
            
            /// <summary>
            /// 
            /// </summary>
            MSG2_NETALLOWSET,

            /// <summary>
            /// 
            /// </summary>
            MSG2_NETAPPMON,

            /// <summary>
            /// 
            /// </summary>
            MSG3_FIREWALLSET,

            /// <summary>
            /// 
            /// </summary>
            MSG3_PORTSET,

            /// <summary>
            /// 
            /// </summary>
            MSG3_PORTMON,

            /// <summary>
            /// 
            /// </summary>
            MSG4_REALAPPMON,

            /// <summary>
            /// 
            /// </summary>
            MSG4_REALSCREENMON,

            /// <summary>
            /// 
            /// </summary>
            MSG5_FILEHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG5_PRINTHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG5_MAILHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG6_MSGSEND,

            /// <summary>
            /// 
            /// </summary>
            MSG6_COMLOCK,

            /// <summary>
            /// 
            /// </summary>
            MSG6_COMSHUT,

            /// <summary>
            /// 
            /// </summary>
            MSG6_RDPMON,

            /// <summary>
            /// 
            /// </summary>
            MSG6_USERSET,

            /// <summary>
            /// 
            /// </summary>
            MSG6_PROCMON,

            /// <summary>
            /// 
            /// </summary>
            MSG6_PROCACT,

            /// <summary>
            /// 
            /// </summary>
            MSG7_WEBHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG7_NETAPPHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG8_DEVICEMON,

            /// <summary>
            /// 
            /// </summary>
            MSG8_INSTAPPHIS,

            /// <summary>
            /// 
            /// </summary>
            MSG8_PROCMON,

            /// <summary>
            /// 
            /// </summary>
            MSG8_CHANGEHIS,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum TYPE
        {
            /// <summary>
            /// 
            /// </summary>
            REQUEST = 0,

            /// <summary>
            /// 
            /// </summary>
            REPLY,

            /// <summary>
            /// 
            /// </summary>
            STREAM,

            /// <summary>
            /// 
            /// </summary>
            REQUESTREPLY,

            /// <summary>
            /// 
            /// </summary>
            REQUESTSTOP,

            /// <summary>
            /// 
            /// </summary>
            INITSET,
            /// <summary>
            /// 
            /// </summary>
            COUNT
        }
    }
}
