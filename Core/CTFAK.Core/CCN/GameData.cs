﻿using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.EXE;
using CTFAK.FileReaders;
using CTFAK.Memory;
using CTFAK.MMFParser.EXE.Loaders;
using CTFAK.Utils;

namespace CTFAK.CCN;

public class GameData
{
    private int _productBuild;
    private int _productVersion;
    private short _runtimeSubversion;

    private short _runtimeVersion;
    public string AboutText;
    public string Author = "";
    public BinaryFiles BinaryFiles;
    public string Copyright;
    public string Doc;

    public string EditorFilename;
    public ExtData ExtData;
    public Extensions Extensions;

    public FontBank Fonts;
    public FrameHandles FrameHandles;

    public Dictionary<int, ObjectInfo> FrameItems = new();

    public List<Frame> Frames = new();
    public GlobalStrings GlobalStrings;
    public GlobalValues GlobalValues;

    public AppHeader Header;
    public ImageBank Images = new();

    public AppMenu Menu;
    public MusicBank Music;

    public string Name;

    public PackData PackData;
    public Shaders Shaders;
    public SoundBank Sounds;
    public string TargetFilename;
    public static event SaveHandler OnChunkLoaded;
    public static event SaveHandler OnFrameLoaded;

    public void Read(ByteReader reader)
    {
        var magic = reader.ReadAscii(4);

        if (magic == "PAMU")
        {
            Settings.Unicode = true;
        }
        else if (magic == "PAME")
        {
            Settings.Unicode = false;
        }
        else if (magic == "CRUF")
        {
            Settings.gameType |= Settings.GameType.F3;
            Settings.Unicode = false;
        }
        else Logger.LogWarning("Couldn't found any known headers: " + magic); //Header not found

        if (Core.parameters.Contains("-android"))
            Settings.gameType |= Settings.GameType.ANDROID;

        _runtimeVersion = (short)reader.ReadUInt16();
        _runtimeSubversion = (short)reader.ReadUInt16();
        _productVersion = reader.ReadInt32();
        _productBuild = reader.ReadInt32();
        Settings.Build = _productBuild;

        Logger.Log("Fusion Build: " + _productBuild);

        var chunkList = new ChunkList();
        chunkList.OnHandleChunk += (id, loader) => { chunkList.HandleChunk(id, loader, this); };
        chunkList.OnChunkLoaded += (id, loader) =>
        {
            switch (id)
            {
                case 8739: //AppHeader
                    Header = loader as AppHeader;
                    break;
                case 8740: //AppName
                    Name = (loader as AppName)?.value;
                    break;
                case 8741: //AppAuthor
                    Author = (loader as AppAuthor)?.value;
                    break;
                case 8742: //AppMenu
                    Menu = loader as AppMenu;
                    break;
                case 8744: //Extensions
                    break;
                case 8745: //FrameItems
                    FrameItems = (loader as FrameItems)?.Items;
                    break;
                case 8746: //GlobalEvents
                    break;
                case 8747: //FrameHandler
                    FrameHandles = loader as FrameHandles;
                    break;
                case 8748: //ExtData
                    ExtData = loader as ExtData;
                    break;
                case 8749: //AdditionalExtension
                    break;
                case 8750: //AppEditorFilename
                    EditorFilename = (loader as EditorFilename)?.value;
                    if (Settings.Build > 284)
                        Decryption.MakeKey(Name, Copyright, EditorFilename);
                    else
                        Decryption.MakeKey(EditorFilename, Name, Copyright);
                    break;
                case 8751: //AppTargetFilename
                    TargetFilename = (loader as TargetFilename)?.value;
                    break;
                case 8752: //AppDoc
                    break;
                case 8753: //OtherExts
                    break;
                case 8754: //GlobalValues
                    GlobalValues = loader as GlobalValues;
                    break;
                case 8755: //GlobalStrings
                    GlobalStrings = loader as GlobalStrings;
                    break;
                case 8756: //Extensions2
                    Extensions = loader as Extensions;
                    break;
                case 8757: //AppIcon
                    break;
                case 8758: //DemoVersion
                    break;
                case 8759: //SecNum
                    break;
                case 8760: //BinaryFiles
                    BinaryFiles = loader as BinaryFiles;
                    break;
                case 8761: //AppMenuImages:
                    break;
                case 8762: //AboutText
                    AboutText = (loader as AboutText)?.value;
                    break;
                case 8763: //Copyright
                    Copyright = (loader as Copyright)?.value;
                    break;
                case 8764: //GlobalValueNames
                    break;
                case 8765: //GlobalStringNames
                    break;
                case 8766: //MvtTexts
                    break;
                case 8767: //FrameItems2
                    FrameItems = (loader as FrameItems2)?.Items;
                    break;
                case 8771:
                    Shaders = loader as Shaders;
                    break;
                case 8792: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 8793: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 13107: //Frame
                    Frames.Add(loader as Frame);
                    break;
                case 26214: //ImageBank
                    Images = loader as ImageBank;
                    break;
                case 26215: //FontBank
                    Fonts = loader as FontBank;
                    break;
                case 26216: //SoundBank
                    Sounds = loader as SoundBank;
                    if (Settings.gameType == Settings.GameType.ANDROID && !Core.parameters.Contains("-nosounds"))
                        Sounds = ApkFileReader.androidSoundBank;
                    break;
                case 8790: //TwoFivePlusProperties
                    FrameItems = TwoFilePlusContainer.instance.objectsContainer;
                    break;
            }
        };
        chunkList.Read(reader);
        // reading again if we encounter an F3 game that uses a separate chunk list for images and sounds
        // it's safe to just read again
        chunkList.Read(reader);
    }

    public void Write(ByteWriter writer)
    {
        writer.WriteAscii(Settings.Unicode ? "PAMU" : "PAME");
        writer.WriteInt32(3);
        writer.WriteInt32(770);
        writer.WriteInt32(0);
        writer.WriteInt32(Settings.Build);
    }
}