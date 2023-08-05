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
    public class M8DeviceMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList computer = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList diskdrive = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList display = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList ide = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList keyboard = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList mice_pointing = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList monitor = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList network = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList other = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList ports = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList processor = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList sound = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList system = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public ArrayList universal = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M8DeviceMon()
        {
            MsgKind = MessageType.KIND.MSG8_DEVICEMON;
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
            int i;

            MsgDataConverter.WriteLenField(memStream, computer.Count);
            for (i = 0; i < computer.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)computer[i]);

            MsgDataConverter.WriteLenField(memStream, diskdrive.Count);
            for (i = 0; i < diskdrive.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)diskdrive[i]);

            MsgDataConverter.WriteLenField(memStream, display.Count);
            for (i = 0; i < display.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)display[i]);

            MsgDataConverter.WriteLenField(memStream, ide.Count);
            for (i = 0; i < ide.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)ide[i]);

            MsgDataConverter.WriteLenField(memStream, keyboard.Count);
            for (i = 0; i < keyboard.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)keyboard[i]);

            MsgDataConverter.WriteLenField(memStream, mice_pointing.Count);
            for (i = 0; i < mice_pointing.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)mice_pointing[i]);

            MsgDataConverter.WriteLenField(memStream, monitor.Count);
            for (i = 0; i < monitor.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)monitor[i]);

            MsgDataConverter.WriteLenField(memStream, network.Count);
            for (i = 0; i < network.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)network[i]);

            MsgDataConverter.WriteLenField(memStream, other.Count);
            for (i = 0; i < other.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)other[i]);

            MsgDataConverter.WriteLenField(memStream, ports.Count);
            for (i = 0; i < ports.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)ports[i]);

            MsgDataConverter.WriteLenField(memStream, processor.Count);
            for (i = 0; i < processor.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)processor[i]);

            MsgDataConverter.WriteLenField(memStream, sound.Count);
            for (i = 0; i < sound.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)sound[i]);

            MsgDataConverter.WriteLenField(memStream, system.Count);
            for (i = 0; i < system.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)system[i]);

            MsgDataConverter.WriteLenField(memStream, universal.Count);
            for (i = 0; i < universal.Count; i++)
                MsgDataConverter.WriteStringField(memStream, (string)universal[i]);

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
            int i, nItems;

            nItems = MsgDataConverter.ReadLenField(memStream);
            computer.Clear();
            for (i = 0; i < nItems; i++)
                computer.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            diskdrive.Clear();
            for (i = 0; i < nItems; i++)
                diskdrive.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            display.Clear();
            for (i = 0; i < nItems; i++)
                display.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            ide.Clear();
            for (i = 0; i < nItems; i++)
                ide.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            keyboard.Clear();
            for (i = 0; i < nItems; i++)
                keyboard.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            mice_pointing.Clear();
            for (i = 0; i < nItems; i++)
                mice_pointing.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            monitor.Clear();
            for (i = 0; i < nItems; i++)
                monitor.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            network.Clear();
            for (i = 0; i < nItems; i++)
                network.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            other.Clear();
            for (i = 0; i < nItems; i++)
                other.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            ports.Clear();
            for (i = 0; i < nItems; i++)
                ports.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            processor.Clear();
            for (i = 0; i < nItems; i++)
                processor.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            sound.Clear();
            for (i = 0; i < nItems; i++)
                sound.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            system.Clear();
            for (i = 0; i < nItems; i++)
                system.Add(MsgDataConverter.ReadStringField(memStream));

            nItems = MsgDataConverter.ReadLenField(memStream);
            universal.Clear();
            for (i = 0; i < nItems; i++)
                universal.Add(MsgDataConverter.ReadStringField(memStream));
        }
    }
}
