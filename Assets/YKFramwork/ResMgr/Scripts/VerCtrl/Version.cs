using System.Collections.Generic;
//using NUnit.Framework;
using YKFramwork.ResMgr.Utils;

namespace YKFramwork.ResMgr.VersionCtrl
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
    }
}