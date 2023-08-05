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
    public class M8InstAppHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList installList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M8InstAppHis()
        {
            MsgKind = MessageType.KIND.MSG8_INSTAPPHIS;
            MsgType = MessageType.TYPE.REPLY;
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
            M8InstallItem item;
            MsgDataConverter.WriteLenField(memStream, installList.Count);
            for (int i = 0; i < installList.Count; i++)
            {
                item = (M8InstallItem)installList[i];
                MsgDataConverter.WriteStringField(memStream, item.strProgram);
                MsgDataConverter.WriteStringField(memStream, item.strPath);
                MsgDataConverter.WriteStringField(memStream, item.strVersion);
                MsgDataConverter.WriteDateField(memStream, item.installTime);
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
            M8InstallItem item;
            installList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M8InstallItem();
                item.strProgram = MsgDataConverter.ReadStringField(memStream);
                item.strPath = MsgDataConverter.ReadStringField(memStream);
                item.strVersion = MsgDataConverter.ReadStringField(memStream);
                item.installTime = MsgDataConverter.ReadDateField(memStream);
                installList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M8InstallItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string strProgram = "";

        /// <summary>
        /// 
        /// </summary>
        public string strPath = "";

        /// <summary>
        /// 
        /// </summary>
        public string strVersion = "";
        /// <summary>
        /// 
        /// </summary>
        public DateTime installTime = DateTime.Now;

        /// <summary>
        /// 
        /// </summary>
        public M8InstallItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M8InstallItem(M8InstallItem item)
        {
            strProgram = item.strProgram;
            strPath = item.strPath;
            installTime = item.installTime;
        }
    }
}
