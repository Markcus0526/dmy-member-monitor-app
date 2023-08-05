using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INMC.INMMessage;
using System.Collections;
using INMC.Communication.Inmc.Communication;

namespace INMC.INMMessage
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M1BandMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList netInfoList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M1BandMon()
        {
            MsgKind = MessageType.KIND.MSG1_BANDMON;
            MsgType = MessageType.TYPE.REQUESTREPLY;
            
        }

        //********************************************
        //ADDED BY JUJ. 2012.10.4.
        //********************************************

        /// <summary>
        /// Make byte array of inmc message
        /// </summary>
        public override byte[] GetByteArray()
        {
            base.GetByteArray();

            //Write message-specific field data
            //Add your codes here
            M1NetworkInfo netInfoItem;
            MsgDataConverter.WriteLenField(memStream, netInfoList.Count);
            for (int i = 0; i < netInfoList.Count; i++)
            {
                netInfoItem = (M1NetworkInfo)netInfoList[i];
                MsgDataConverter.WriteStringField(memStream, netInfoItem.strNetType);
                MsgDataConverter.WriteInt64(memStream, netInfoItem.nSpeed);
                MsgDataConverter.WriteInt32(memStream, netInfoItem.nUtilization);
                MsgDataConverter.WriteInt64(memStream, netInfoItem.nBytesSent);
                MsgDataConverter.WriteInt64(memStream, netInfoItem.nBytesReceive);
                MsgDataConverter.WriteInt32(memStream, netInfoItem.nUploadSpeed);
                MsgDataConverter.WriteInt32(memStream, netInfoItem.nDownloadSpeed);
                MsgDataConverter.WriteStringField(memStream, netInfoItem.strInterfaceType);
            }
            return memStream.ToArray();
        }

        /// <summary>
        /// initialize message field from byte array
        /// </summary>
        public override void Initialize(byte[] byteArray)
        {
            base.Initialize(byteArray);

            //To initialize message fields
            //Add your codes here
            M1NetworkInfo netInfoItem;
            netInfoList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                netInfoItem = new M1NetworkInfo();
                netInfoItem.strNetType = MsgDataConverter.ReadStringField(memStream);
                netInfoItem.nSpeed = MsgDataConverter.ReadInt64(memStream);
                netInfoItem.nUtilization = MsgDataConverter.ReadInt32(memStream);
                netInfoItem.nBytesSent = MsgDataConverter.ReadInt64(memStream);
                netInfoItem.nBytesReceive = MsgDataConverter.ReadInt64(memStream);
                netInfoItem.nUploadSpeed = MsgDataConverter.ReadInt32(memStream);
                netInfoItem.nDownloadSpeed = MsgDataConverter.ReadInt32(memStream);
                netInfoItem.strInterfaceType = MsgDataConverter.ReadStringField(memStream);
                netInfoList.Add(netInfoItem);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M1NetworkInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string strNetType = "";

        /// <summary>
        /// 
        /// </summary>
        public long nSpeed = 0;

        /// <summary>
        /// 
        /// </summary>
        public int nUtilization = 0;

        /// <summary>
        /// 
        /// </summary>
        public long nBytesSent = 0;

        /// <summary>
        /// 
        /// </summary>
        public long nBytesReceive = 0;

        /// <summary>
        /// 
        /// </summary>
        public int nUploadSpeed = 0;

        /// <summary>
        /// 
        /// </summary>
        public int nDownloadSpeed = 0;

        /// <summary>
        /// 
        /// </summary>
        public string strInterfaceType = "";

        /// <summary>
        /// 
        /// </summary>
        public M1NetworkInfo()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M1NetworkInfo(M1NetworkInfo item)
        {
            strNetType = item.strNetType;
            nSpeed = item.nSpeed;
            nUtilization = item.nUtilization;
            nBytesSent = item.nBytesSent;
            nBytesReceive = item.nBytesReceive;
            nUploadSpeed = item.nUploadSpeed;
            nDownloadSpeed = item.nDownloadSpeed;
            strInterfaceType = item.strInterfaceType;
        }
    }
}
