﻿using System;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters;

public class Group : ParameterCommon
{
    private const string groupWords = "mqojhm:qskjhdsmkjsmkdjhq\u0063clkcdhdlkjhd";
    public ushort Flags;
    public ushort Id;
    public string Name;
    public long Offset;
    public int Password;
    public byte[] Unk1;
    public byte[] Unk2;

    private static short wrapSingleChar(short value)
    {
        value = (short)(value & 0xFF);
        if (value > 127) value -= 256;

        return value;
    }

    public static int generateChecksum(string name, string pass)
    {
        var v4 = 0x3939;
        foreach (var c in name) v4 += Convert.ToInt16(c) ^ 0x7FFF;

        var v5 = 0;
        foreach (var c in pass)
        {
            v4 += wrapSingleChar((short)(Convert.ToInt16(groupWords[v5]) + (Convert.ToInt16(c) ^ 0xC3))) ^ 0xF3;
            v5++;
            if (v5 > groupWords.Length)
                v5 = 0;
        }

        return v4;
    }

    public override void Read(ByteReader reader)
    {
        Offset = reader.Tell() - 24;
        Flags = reader.ReadUInt16();
        Id = reader.ReadUInt16();
        Name = reader.ReadWideString();
        if (Settings.Build >= 293) Name = "Group " + Id;
        //Name = "InvalidGroup_" + Id;
        Unk1 = reader.ReadBytes(190 - Name.Length * 2);
        //Password = reader.ReadInt32();
        //reader.ReadInt16();
    }

    public override void Write(ByteWriter Writer)
    {
        Writer.WriteUInt16(Flags);
        Writer.WriteUInt16(Id);
        Writer.WriteUnicode(Name, true);
        Writer.WriteBytes(Unk1);

        Password = generateChecksum(Name, "");
        Writer.WriteInt32(Password);
        Writer.WriteInt16(0);
    }

    public override string ToString()
    {
        return $"Group: {Name}";
    }
}