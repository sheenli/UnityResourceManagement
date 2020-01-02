using System.Collections.Generic;
using YKFramework.ResMgr.Utils;
//using NUnit.Framework;

namespace YKFramework.ResMgr.VersionCtrl
{
    
    public class Version
    {
        public class ABInfo
        {
            public string name;
            public string sha1;
            public long size;
        }
        public string version;

        public List<ABInfo> AllAB { get; }

        public Version()
        {
            AllAB = new List<ABInfo>();
        }
        public Version(byte[] bs)
        {
            var buffer = new ByteBuffer(bs);
            version = buffer.ReadString();
            var count = buffer.ReadInt();
            AllAB = new List<ABInfo>();
            for (int i = 0; i < count; i++)
            {
                ABInfo info = new ABInfo();
                info.name = buffer.ReadString();
                info.sha1 = buffer.ReadString();
                info.size = buffer.ReadLong();
                AllAB.Add(info);
            }
            buffer.Close();
        }

        public byte[] ToBytes()
        {
            var buffer = new ByteBuffer();
            buffer.WriteString(version);
            buffer.WriteInt(AllAB.Count);
            foreach (var abInfo in AllAB)
            {
                buffer.WriteString(abInfo.name);
                buffer.WriteString(abInfo.sha1);
                buffer.WriteLong(abInfo.size);
            }

            var bt = buffer.ToBytes();
            buffer.Close();
            return bt;
        }
    }
}