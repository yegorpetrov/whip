using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki
{
    public class Maki
    {
        public readonly IReadOnlyList<Guid> Guids;
        public readonly IReadOnlyList<Import> Imports;
        public readonly IReadOnlyList<object> Objects;
        public readonly IReadOnlyList<Listener> Listeners;
        public readonly IReadOnlyList<byte> Code;

        public Maki(byte[] exe)
        {
            using (var ms = new MemoryStream(exe))
            using (var mr = new MakiReader(ms))
            {
                var header = mr.ReadUInt16();
                var version = mr.ReadBytes(6);
                Guids = mr.ReadSectionOf(m => m.ReadGuid()).ToList();
                Imports = mr.ReadSectionOf(ReadImport).ToList();

                var objects = mr.ReadSectionOf(ReadObject).ToList();
                var strings = mr.ReadSectionOf(ReadString).ToList();
                Objects = (
                    from obj in objects.Select(Tuple.Create<object, int>)
                    join str in strings on obj.Item2 equals str.Item1 into pre
                    from p in pre.DefaultIfEmpty()
                    select obj.Item1 ?? p?.Item2).ToList();

                Listeners = mr.ReadSectionOf(ReadListener).ToList();
                Code = mr.ReadBytes(mr.ReadInt32());
            }
        }

        public struct Import
        {
            public Maki Maki;
            public int TypeIdx;
            public string Name;

            public override string ToString() =>
                string.Format("t{0}.{1}", TypeIdx, Name);
        }

        public struct Listener
        {
            public Maki Maki;

            public int
                ObjIdx,
                CallIdx,
                Offset;

            public override string ToString() =>
                string.Format("{0}.{1} @ {2}", ObjIdx, CallIdx, Offset);
        }

        Import ReadImport(MakiReader mr)
        {
            return new Import()
            {
                Maki = this,
                TypeIdx = mr.ReadInt32() - 0x100,
                Name = mr.ReadString()
            };
        }

        bool nullRead;

        object ReadObject(MakiReader mr)
        {
            var typeIdx = mr.ReadByte();
            var isPrimitive = !mr.ReadBoolean();
            var sub = mr.ReadInt16();
            var data = mr.ReadBytes(8);
            var isGlobal = mr.ReadBoolean();
            var isStatic = mr.ReadBoolean();

            if (isPrimitive)
            {
                switch (typeIdx)
                {
                    case 2:
                        var result = nullRead ?
                            BitConverter.ToInt32(data, 0) :
                            default(object);
                        nullRead = true;
                        return result;
                    case 3:
                        return BitConverter.ToSingle(data, 0);
                    case 4:
                        return BitConverter.ToDouble(data, 0);
                    case 5:
                        return BitConverter.ToBoolean(data, 0);
                    default:
                        return null;
                }
            }
            else return isStatic ? Guids[typeIdx] : default(object);
        }

        static Tuple<int, string> ReadString(MakiReader mr)
        {
            return new Tuple<int, string>(mr.ReadInt32(), mr.ReadString());
        }

        Listener ReadListener(MakiReader mr)
        {
            return new Listener()
            {
                Maki = this,
                ObjIdx = mr.ReadInt32(),
                CallIdx = mr.ReadInt32(),
                Offset = mr.ReadInt32()
            };
        }
    }
}
