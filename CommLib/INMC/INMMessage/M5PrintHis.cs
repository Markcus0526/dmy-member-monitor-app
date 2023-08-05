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
    public class M5PrintHis : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList printList = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M5PrintHis()
        {
            MsgKind = MessageType.KIND.MSG5_PRINTHIS;
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
            M5PrintItem item;
            MsgDataConverter.WriteLenField(memStream, printList.Count);
            for (int i = 0; i < printList.Count; i++)
            {
                item = (M5PrintItem)printList[i];
                MsgDataConverter.WriteDateField(memStream, item.PrintTime);
                MsgDataConverter.WriteStringField(memStream, item.Printer);
                MsgDataConverter.WriteStringField(memStream, item.PrintFile);
                MsgDataConverter.WriteStringField(memStream, item.PrintPath);
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
            M5PrintItem item;
            printList.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M5PrintItem();
                item.PrintTime = MsgDataConverter.ReadDateField(memStream);
                item.Printer = MsgDataConverter.ReadStringField(memStream);
                item.PrintFile = MsgDataConverter.ReadStringField(memStream);
                item.PrintPath = MsgDataConverter.ReadStringField(memStream);
                printList.Add(item);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class M5PrintItem
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime PrintTime;

        /// <summary>
        /// 
        /// </summary>
        public string Printer = "";

        /// <summary>
        /// 
        /// </summary>
        public string PrintFile = "";

        /// <summary>
        /// 
        /// </summary>
        public string PrintPath = "";

        /// <summary>
        /// 
        /// </summary>
        public M5PrintItem()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public M5PrintItem(M5PrintItem item)
        {
            PrintTime = item.PrintTime;
            Printer = item.Printer;
            PrintFile = item.PrintFile;
            PrintPath = item.PrintPath;
        }
    }
}
