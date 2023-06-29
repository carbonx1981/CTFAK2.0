﻿using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.IO.CCN.Chunks;

[ChunkLoader(0x2226, "AppMenu")]
public class AppMenu : Chunk
{
    public List<short> AccelId = new();
    public List<short> AccelKey = new();
    public List<byte> AccelShift = new();
    public List<AppMenuItem> Items = new();


    public override void Read(ByteReader reader)
    {
        var currentPosition = reader.Tell();
        var headerSize = reader.ReadUInt32();
        var menuOffset = reader.ReadInt32();
        var menuSize = reader.ReadInt32();
        if (menuSize == 0) return;
        var accelOffset = reader.ReadInt32();
        var accelSize = reader.ReadInt32();
        reader.Seek(currentPosition + menuOffset);
        reader.Skip(4);

        Load(reader);

        reader.Seek(currentPosition + accelOffset);
        AccelShift = new List<byte>();
        AccelKey = new List<short>();
        AccelId = new List<short>();
        for (var i = 0; i < accelSize / 8; i++)
        {
            AccelShift.Add(reader.ReadByte());
            reader.Skip(1);
            AccelKey.Add(reader.ReadInt16());
            AccelId.Add(reader.ReadInt16());
            reader.Skip(2);
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteInt32(20);
        writer.WriteInt32(20);
        //writer.WriteInt32(0);

        var menuDataWriter = new ByteWriter(new MemoryStream());

        foreach (var item in Items) item.Write(menuDataWriter);

        writer.WriteUInt32((uint)menuDataWriter.BaseStream.Position + 4);

        writer.WriteUInt32((uint)(24 + menuDataWriter.BaseStream.Position));
        writer.WriteInt32(AccelKey.Count * 8);
        writer.WriteInt32(0);
        writer.WriteWriter(menuDataWriter);

        for (var i = 0; i < AccelKey.Count; i++)
        {
            writer.WriteInt8(AccelShift[i]);
            writer.WriteInt8(0);
            writer.WriteInt16(AccelKey[i]);
            writer.WriteInt16(AccelId[i]);
            writer.WriteInt16(0);
        }
    }

    public void Load(ByteReader reader)
    {
        while (true)
        {
            var newItem = new AppMenuItem();
            newItem.Read(reader);
            Items.Add(newItem);

            if (ByteFlag.GetFlag(newItem.Flags, 4)) Load(reader);

            if (ByteFlag.GetFlag(newItem.Flags, 7)) break;
        }
    }
}

public class AppMenuItem : DataLoader
{
    public ushort Flags;
    public short Id;
    public string Name = "";

    public override void Read(ByteReader reader)
    {
        Flags = reader.ReadUInt16();
        if (!ByteFlag.GetFlag(Flags, 4)) Id = reader.ReadInt16();
        Name = reader.ReadWideString();
    }
    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt16(Flags);
        if (!ByteFlag.GetFlag(Flags, 4)) writer.WriteInt16(Id);

        writer.WriteUnicode(Name, true);
    }
}