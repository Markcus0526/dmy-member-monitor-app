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
    public class M5FileHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList fileList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M5FileHis()
        {
            MsgKind = MessageType.KIND.MSG5_FILEHIS;
            MsgType = MessageType.TYPE.REQUEST;
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
            M5FileItem item;
            MsgDataConverter.WriteLenField(memStream, fileList.Count);
            for (int i = 0; i < fileList.Count; i++)
            {
                item = (M5FileItem)fileList[i];
                MsgDataConverter.WriteDateField(memStream, item.ActionTime);
                MsgDataConverter.WriteStringField(memStream, item.Action);
                MsgDataConverter.WriteStringField(memStream, item.FileType);
                MsgDataConverter.WriteStringField(memStream, item.FileName);
                MsgDataConverter.WriteStringField(memStream, item.Path);
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
            M5FileItem item;
            fileList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M5FileItem();
                item.ActionTime = MsgDataConverter.ReadDateField(memStream);
                item.Action = MsgDataConverter.ReadStringField(memStream);
                item.FileType = MsgDataConverter.ReadStringField(memStream);
                item.FileName = MsgDataConverter.ReadStringField(memStream);
                item.Path = MsgDataConverter.ReadStringField(memStream);
                fileList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M5FileItem
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime ActionTime;

        /// <summary>
        /// 
        /// </summary>
        public string Action = "";

        /// <summary>
        /// 
        /// </summary>
        public string FileType = "";

        /// <summary>
        /// 
        /// </summary>
        public string FileName = "";

        /// <summary>
        /// 
        /// </summary>
        public string Path = "";

        /// <summary>
        /// 
        /// </summary>
        public M5FileItem()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M5FileItem(M5FileItem item)
        {
            ActionTime = item.ActionTime;
            Action = item.Action;
            FileType = item.FileType;
            FileName = item.FileName;
            Path = item.Path;
        }
    }
}
