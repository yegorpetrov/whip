using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whip.Scripting
{
    class ScriptReader : BinaryReader
    {
        public ScriptReader(byte[] exe)
            : base(new MemoryStream(exe))
        {

        }

        public override string ReadString()
        {
            return Encoding.ASCII.GetString(ReadBytes(ReadUInt16()));
        }

        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        public bool ReadHeader()
        {
            var fg = Encoding.ASCII.GetString(ReadBytes(2));
            var version = ReadBytes(6);
            return fg == "FG";
        }

        public IEnumerable<Guid> ReadTypeTable()
        {
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                yield return ReadGuid();
            }
        }

        /// <summary>
        /// Read call table
        /// </summary>
        /// <returns>Type idx, call name</returns>
        public IEnumerable<Tuple<ushort, string>> ReadCallTable()
        {
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                var type = (ushort)(ReadInt16() & 0xFF); // TODO Why?
                var dummy = ReadInt16();
                var name = ReadString();
                yield return new Tuple<ushort, string>(type, name);
            }
        }

        public IEnumerable<object> ReadObjectTable(Guid[] guids)
        {
            bool nullRead = false;
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                var typeIdx = ReadByte();
                var isPrimitive = ReadByte() == 0;
                var sub = ReadInt16();
                var data = ReadBytes(8);
                var isGlobal = ReadByte() != 0;
                var isSystem = ReadByte() != 0;

                if (isPrimitive)
                {
                    switch (typeIdx)
                    {
                        case 0: // Void
                        case 1: // Event
                            yield return null;
                            break;
                        case 2: // Int
                            yield return nullRead ?
                                (object)BitConverter.ToInt32(data, 0) : null;
                            nullRead = true;
                            break;
                        case 3: // Float
                            yield return BitConverter.ToSingle(data, 0);
                            break;
                        case 4: // Double
                            yield return BitConverter.ToDouble(data, 0);
                            break;
                        case 5: // Bool
                            yield return BitConverter.ToBoolean(data, 0);
                            break;
                        case 6: // Strings are in the next section
                        case 7: // Object?
                            yield return null;
                            break;
                    }
                }
                else
                {
                    yield return isSystem ? (object)guids[typeIdx] : null;
                }
            }
        }

        /// <summary>
        /// Read string constants for the object table
        /// </summary>
        /// <returns>Object idx, string</returns>
        public IEnumerable<Tuple<int, string>> ReadStringTable()
        {
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                yield return new Tuple<int, string>(ReadInt32(), ReadString());
            }
        }

        /// <summary>
        /// Read listener entry points
        /// </summary>
        /// <returns>Object idx, call idx, code offset</returns>
        public IEnumerable<Tuple<int, int, int>> ReadListenerTable()
        {
            for (int i = 0, _i = ReadInt32(); i < _i; i++)
            {
                yield return new Tuple<int, int, int>(
                    ReadInt32(), ReadInt32(), ReadInt32());
            }
        }

        public byte[] ReadBytecode()
        {
            return ReadBytes(ReadInt32());
        }
    }
}
