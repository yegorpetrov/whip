using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WhipMaki
{
    class MakiReader : BinaryReader
    {
        public MakiReader(Stream s) : base(s) { }

        public override string ReadString() =>
            Encoding.ASCII.GetString(ReadBytes(ReadUInt16()));

        public Guid ReadGuid() => new Guid(ReadBytes(16));

        public IEnumerable<T> ReadSectionOf<T>(Func<MakiReader, T> f)
        {
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                yield return f(this);
            }
        }
    }
}
