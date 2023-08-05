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
    public class M3PortMon : MessageBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ArrayList portRule = new ArrayList();

        /// <summary>
        /// 
        /// </summary>
        public M3PortMon()
        {
            MsgKind = MessageType.KIND.MSG3_PORTMON;
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
            M3PortRule item;
            MsgDataConverter.WriteLenField(memStream, portRule.Count);
            for (int i = 0; i < portRule.Count; i++)
            {
                item = (M3PortRule)portRule[i];
                MsgDataConverter.WriteInt32(memStream, item.nType);
                MsgDataConverter.WriteStringField(memStream, item.Direction);
                MsgDataConverter.WriteStringField(memStream, item.Name);
                MsgDataConverter.WriteStringField(memStream, item.GroupName);
                MsgDataConverter.WriteStringField(memStream, item.Description);
                MsgDataConverter.WriteStringField(memStream, item.Enabled);
                MsgDataConverter.WriteStringField(memStream, item.Action);
                MsgDataConverter.WriteStringField(memStream, item.AppPath);
                MsgDataConverter.WriteStringField(memStream, item.ServiceName);
                MsgDataConverter.WriteStringField(memStream, item.Protocol);
                MsgDataConverter.WriteStringField(memStream, item.LocalPorts);
                MsgDataConverter.WriteStringField(memStream, item.RemotePorts);
                MsgDataConverter.WriteStringField(memStream, item.IcmpTypes);
                MsgDataConverter.WriteStringField(memStream, item.LocalAddr);
                MsgDataConverter.WriteStringField(memStream, item.RemoteAddr);
                MsgDataConverter.WriteStringField(memStream, item.Profiles);
                MsgDataConverter.WriteStringField(memStream, item.Interfaces);
                MsgDataConverter.WriteStringField(memStream, item.InterfaceTypes);
                MsgDataConverter.WriteStringField(memStream, item.edge_traversal);
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
            M3PortRule item;
            portRule.Clear();
            int nItems = MsgDataConverter.ReadLenField(memStream);
            for (int i = 0; i < nItems; i++)
            {
                item = new M3PortRule();
                item.nType = MsgDataConverter.ReadInt32(memStream);
                item.Direction = MsgDataConverter.ReadStringField(memStream);
                item.Name = MsgDataConverter.ReadStringField(memStream);
                item.GroupName = MsgDataConverter.ReadStringField(memStream);
                item.Description = MsgDataConverter.ReadStringField(memStream);
                item.Enabled = MsgDataConverter.ReadStringField(memStream);
                item.Action = MsgDataConverter.ReadStringField(memStream);
                item.AppPath = MsgDataConverter.ReadStringField(memStream);
                item.ServiceName = MsgDataConverter.ReadStringField(memStream);
                item.Protocol = MsgDataConverter.ReadStringField(memStream);
                item.LocalPorts = MsgDataConverter.ReadStringField(memStream);
                item.RemotePorts = MsgDataConverter.ReadStringField(memStream);
                item.IcmpTypes = MsgDataConverter.ReadStringField(memStream);
                item.LocalAddr = MsgDataConverter.ReadStringField(memStream);
                item.RemoteAddr = MsgDataConverter.ReadStringField(memStream);
                item.Profiles = MsgDataConverter.ReadStringField(memStream);
                item.Interfaces = MsgDataConverter.ReadStringField(memStream);
                item.InterfaceTypes = MsgDataConverter.ReadStringField(memStream);
                item.edge_traversal = MsgDataConverter.ReadStringField(memStream);
                portRule.Add(item);
            }
        }
    }
}
