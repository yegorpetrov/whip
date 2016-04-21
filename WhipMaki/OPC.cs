using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhipMaki
{
    internal enum OPC : byte
    {
        nop = 0x0,
        load = 0x1, // int32 obj_idx
        drop = 0x2,
        save = 0x3, // int32 obj_idx
        cmpeq = 0x8,
        cmpne = 0x9,
        cmpgt = 0xa,
        cmpge = 0xb,
        cmplt = 0xc,
        cmple = 0xd,
        jiz = 0x10, // int32 offset
        jnz = 0x11, // int32 offset
        jmp = 0x12, // int32 offset
        climp = 0x18, // int32 import
        clint = 0x19, // int32 offset
        ret = 0x21,
        stop = 0x28,
        set = 0x30,
        incp = 0x38, // A++
        decp = 0x39, // A−−
        pinc = 0x3a, // ++A
        pdec = 0x3b, // −−A
        add = 0x40,
        sub = 0x41,
        mul = 0x42,
        div = 0x43,
        mod = 0x44,
        band = 0x48, // A & B
        bor = 0x49, // A | B
        not = 0x4a, // !A
        bnot = 0x4b, // ~A
        neg = 0x4c, // −1*A
        bxor = 0x4d, // A ^ B
        and = 0x50, // A && B
        or = 0x51, // A || B
        shl = 0x58,
        shr = 0x59,
        make = 0x60, // int32 type
        del = 0x61, // delete
        climpn = 0x70, // int32 import, int8 n_args
    }
}
