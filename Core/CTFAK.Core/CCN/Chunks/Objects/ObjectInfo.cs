﻿using System;
using System.Collections.Generic;
using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects;

public class ObjectInfo : ChunkLoader
{
    public byte blend;
    public int Flags;
    public int handle;
    public int InkEffect;
    public int InkEffectValue;
    public string name;
    public int ObjectType;
    public ChunkLoader properties;
    public int Reserved;
    public Color rgbCoeff;

    public ShaderData shaderData = new();

    public override void Read(ByteReader reader)
    {
        while (true)
        {
            var newChunk = new Chunk();
            var chunkData = newChunk.Read(reader);
            var chunkReader = new ByteReader(chunkData);

            if (newChunk.Id == 32639) break;
            //Logger.Log("Object Chunk ID " + newChunk.Id);
            switch (newChunk.Id)
            {
                case 17477:
                    name = chunkReader.ReadUniversal();
                    break;
                case 17476:
                    handle = chunkReader.ReadInt16();
                    ObjectType = chunkReader.ReadInt16();
                    Flags = chunkReader.ReadInt16();
                    chunkReader.Skip(2);
                    InkEffect = chunkReader.ReadByte();
                    if (InkEffect != 1)
                    {
                        chunkReader.Skip(3);
                        var r = chunkReader.ReadByte();
                        var g = chunkReader.ReadByte();
                        var b = chunkReader.ReadByte();
                        rgbCoeff = Color.FromArgb(0, r, g, b);
                        blend = chunkReader.ReadByte();
                    }
                    else
                    {
                        var flag = chunkReader.ReadByte();
                        chunkReader.Skip(2);
                        InkEffectValue = chunkReader.ReadByte();
                    }

                    if (Settings.Old)
                    {
                        rgbCoeff = Color.White;
                        blend = 255;
                    }

                    break;
                case 17478:
                    if (ObjectType == 0) properties = new Quickbackdrop();
                    else if (ObjectType == 1) properties = new Backdrop();
                    else properties = new ObjectCommon(this);
                    properties?.Read(chunkReader);

                    break;

                case 17480:
                    shaderData.hasShader = true;
                    var shaderHandle = chunkReader.ReadInt32();
                    var numberOfParams = chunkReader.ReadInt32();
                    var shdr = Core.currentReader.getGameData().Shaders.ShaderList[shaderHandle];
                    shaderData.name = shdr.Name;
                    shaderData.ShaderHandle = shaderHandle;

                    for (var i = 0; i < numberOfParams; i++)
                    {
                        var param = shdr.Parameters[i];
                        object paramValue;
                        switch (param.Type)
                        {
                            case 0:
                                paramValue = chunkReader.ReadInt32();
                                break;
                            case 1:
                                paramValue = chunkReader.ReadSingle();
                                break;
                            case 2:
                                paramValue = chunkReader.ReadInt32();
                                break;
                            case 3:
                                paramValue = chunkReader.ReadInt32(); //image handle
                                break;
                            default:
                                paramValue = "unknownType";
                                break;
                        }

                        shaderData.parameters.Add(new ShaderParameter
                            { Name = param.Name, ValueType = param.Type, Value = paramValue });
                    }

                    break;
            }
        }
        if (String.IsNullOrEmpty(name))
            name = $"{(Constants.ObjectType)ObjectType} {handle}";
    }

    public override void Write(ByteWriter writer)
    {
        throw new NotImplementedException();
    }

    //public int shaderId;
    //public List<ByteReader> effectItems;
    public class ShaderParameter
    {
        public string Name;
        public object Value;
        public int ValueType;
    }

    public class ShaderData
    {
        public bool hasShader;
        public string name;
        public List<ShaderParameter> parameters = new();
        public int ShaderHandle;
    }
}