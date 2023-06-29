﻿using System;
using System.Collections.Generic;
using CTFAK.Attributes;
using CTFAK.Memory;

namespace CTFAK.IO.CCN.Chunks;

[ChunkLoader(8771, "Shaders")]
public class Shaders : Chunk

{
    public Dictionary<int, Shader> ShaderList;

    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var count = reader.ReadInt32();
        var offsets = new List<int>();
        ShaderList = new Dictionary<int, Shader>();
        for (var i = 0; i < count; i++) offsets.Add(reader.ReadInt32());

        foreach (var offset in offsets)
        {
            reader.Seek(start + offset);
            var shader = new Shader();
            shader.Read(reader);
            ShaderList.Add(offsets.IndexOf(offset), shader);
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class Shader : DataLoader
{
    public int BackgroundTexture;
    public string Data;
    public string Name;
    public List<ShaderParameter> Parameters = new();

    public override void Read(ByteReader reader)
    {
        var start = reader.Tell();
        var nameOffset = reader.ReadInt32();
        var dataOffset = reader.ReadInt32();
        var parameterOffset = reader.ReadInt32();
        BackgroundTexture = reader.ReadInt32();
        reader.Seek(start + nameOffset);
        Name = reader.ReadAscii();
        reader.Seek(start + dataOffset);
        Data = reader.ReadAscii();
        if (parameterOffset != 0)
        {
            parameterOffset = (int)(parameterOffset + start);
            reader.Seek(parameterOffset);
            var paramCount = reader.ReadInt32();

            for (var i = 0; i < paramCount; i++)
            {
                var newParameter = new ShaderParameter();
                Parameters.Add(newParameter);
            }

            var typeOffset = reader.ReadInt32();
            var namesOffset = reader.ReadInt32();
            reader.Seek(parameterOffset + typeOffset);
            foreach (var parameter in Parameters) parameter.Type = reader.ReadByte();
            reader.Seek(parameterOffset + namesOffset);
            foreach (var parameter in Parameters) parameter.Name = reader.ReadAscii();
        }
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }
}

public class ShaderParameter : DataLoader
{
    public string Name;
    public int Type;
    public int Value;

    public override void Read(ByteReader reader)
    {
        throw new NotImplementedException();
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    public string GetValueType()
    {
        switch (Type)
        {
            case 0: return "int";
            case 1: return "float";
            case 2: return "int_float4";
            case 3: return "image";
            default: return "unk";
        }
    }
}