﻿using System;
using System.Collections.Generic;
using System.IO;
using CTFAK.IO.CCN;
using CTFAK.IO.Common.Events;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.IO.MFA;

public class MFAEvents : DataLoader
{
    
    //TODO Clean this up. I'm too lazy to do it rn
    public const string EventData = "Evts";
    public const string CommentData = "Rems";
    public const string ObjectData = "EvOb";
    public const string EventEditorData = "EvCs";
    public const string ObjectListData = "EvEd";
    public const string TimeListData = "EvEd";
    public const string EditorPositionData = "EvTs";
    public const string EditorLineData = "EvLs";
    public const string UnknownEventData = "E2Ts";
    public const string EventEnd = "!DNE";
    public uint CaretType;
    public uint CaretX;
    public uint CaretY;
    public uint CommentDataLen;
    public List<Comment> Comments = new();
    public ushort ConditionWidth;
    public int EditorDataUnk;
    public uint EventDataLen;
    public uint EventLine;
    public uint EventLineY;
    public List<string> Folders = new();
    public ushort FrameType;
    public List<EventGroup> Items = new();
    public uint LineItemType;
    public uint LineY;
    public List<ushort> ObjectFlags = new();
    public List<ushort> ObjectHandles = new();
    public ushort ObjectHeight;
    public List<EventObject> Objects = new();
    public List<ushort> ObjectTypes = new();
    public byte[] Saved;
    public ushort Version;
    public uint X;
    public uint Y;

    public override void Read(ByteReader reader)
    {
        Version = reader.ReadUInt16();
        FrameType = reader.ReadUInt16();
        Items = new List<EventGroup>();

        while (true)
        {
            var name = reader.ReadAscii(4);
            if (name == EventData)
            {
                EventDataLen = reader.ReadUInt32();
                var end = (uint)(reader.Tell() + EventDataLen);
                while (true)
                {
                    var evGrp = new EventGroup();
                    evGrp.isMFA = true;
                    evGrp.Read(reader);
                    Items.Add(evGrp);
                    if (reader.Tell() >= end) break;
                }
            }
            else if (name == CommentData)
            {
                try
                {
                    CommentDataLen = reader.ReadUInt32();
                    Comments = new List<Comment>();
                    var comment = new Comment();
                    comment.Read(reader);
                    Comments.Add(comment);
                }
                catch
                {
                    //What the fuck?

                    /*
                    import code
                    code.interact(local = locals())
                    */
                }
            }
            else if (name == ObjectData)
            {
                Objects = new List<EventObject>();
                var len = reader.ReadUInt32();
                for (var i = 0; i < len; i++)
                {
                    var eventObject = new EventObject();
                    eventObject.Read(reader);
                    Objects.Add(eventObject);
                }
            }
            else if (name == EventEditorData)
            {
                EditorDataUnk = reader.ReadInt32();
                ConditionWidth = reader.ReadUInt16();
                ObjectHeight = reader.ReadUInt16();
                reader.Skip(12);
            }
            else if (name == ObjectListData)
            {
                var count = reader.ReadInt16();
                var realCount = count;
                if (count == -1) realCount = reader.ReadInt16();

                ObjectTypes = new List<ushort>();
                for (var i = 0; i < realCount; i++) ObjectTypes.Add(reader.ReadUInt16());
                ObjectHandles = new List<ushort>();
                for (var i = 0; i < realCount; i++) ObjectHandles.Add(reader.ReadUInt16());
                ObjectFlags = new List<ushort>();
                for (var i = 0; i < realCount; i++) ObjectFlags.Add(reader.ReadUInt16());

                if (count == -1)
                {
                    Folders = new List<string>();
                    var folderCount = reader.ReadUInt16();
                    for (var i = 0; i < folderCount; i++) Folders.Add(reader.AutoReadUnicode());
                }
            }
            else if (name == TimeListData)
            {
                throw new NotImplementedException("I don't like no timelist");
            }
            else if (name == EditorPositionData)
            {
                reader.ReadInt16();
                X = reader.ReadUInt32();
                Y = reader.ReadUInt32();
                CaretType = reader.ReadUInt32();
                CaretX = reader.ReadUInt32();
                CaretY = reader.ReadUInt32();
            }
            else if (name == EditorLineData)
            {
                reader.ReadInt16();
                LineY = reader.ReadUInt32();
                LineItemType = reader.ReadUInt32();
                EventLine = reader.ReadUInt32();
                EventLineY = reader.ReadUInt32();
            }
            else if (name == UnknownEventData)
            {
                reader.Skip(12);
            }
            else if (name == EventEnd)
            {
                break;
            }
            else
            {
                Logger.Log("Unknown Group: " +
                           name); //throw new NotImplementedException("Fuck Something is Broken: "+name);
            }
        }
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt16(Version);
        writer.WriteUInt16(FrameType);
        if (Items.Count > 0)
        {
            writer.WriteAscii(EventData);

            var newWriter = new ByteWriter(new MemoryStream());
            //Writer.WriteUInt32(EventDataLen);

            foreach (var eventGroup in Items)
            {
                eventGroup.isMFA = true;
                eventGroup.Write(newWriter);
            }

            writer.WriteUInt32((uint)newWriter.BaseStream.Position);
            writer.WriteWriter(newWriter);
        }

        if (Objects?.Count > 0)
        {
            writer.WriteAscii(ObjectData);
            writer.WriteUInt32((uint)Objects.Count);
            foreach (var eventObject in Objects) eventObject.Write(writer);
        }

        if (ObjectTypes != null)
        {
            writer.WriteAscii(ObjectListData);
            writer.WriteInt16(-1);
            writer.WriteInt16((short)ObjectTypes.Count);
            foreach (var objectType in ObjectTypes) writer.WriteUInt16(objectType);

            foreach (var objectHandle in ObjectHandles) writer.WriteUInt16(objectHandle);

            foreach (var objectFlag in ObjectFlags) writer.WriteUInt16(objectFlag);

            writer.WriteUInt16((ushort)Folders.Count);
            foreach (var folder in Folders) writer.AutoWriteUnicode(folder);
        }

        // if (X != 0)
        {
            writer.WriteAscii(EditorPositionData);
            writer.WriteInt16(10);
            writer.WriteInt32((int)X);
            writer.WriteInt32((int)Y);
            writer.WriteUInt32(CaretType);
            writer.WriteUInt32(CaretX);
            writer.WriteUInt32(CaretY);
        }
        // if (LineY != 0)
        {
            writer.WriteAscii(EditorLineData);
            writer.WriteInt16(10);
            writer.WriteUInt32(LineY);
            writer.WriteUInt32(LineItemType);
            writer.WriteUInt32(EventLine);
            writer.WriteUInt32(EventLineY);
        }
        writer.WriteAscii(UnknownEventData);
        writer.WriteInt8(8);
        writer.Skip(9);
        writer.WriteInt16(0);

        writer.WriteAscii(EventEditorData);
        // Writer.Skip(4+2*2+4*3);
        writer.WriteInt32(EditorDataUnk);
        writer.WriteInt16((short)ConditionWidth);
        writer.WriteInt16((short)ObjectHeight);
        writer.Skip(12);

        writer.WriteAscii(EventEnd);

        // Writer.WriteBytes(_cache);

        //TODO: Fix commented part
        //
        // if (Comments != null)
        // {
        //     Console.WriteLine("Writing Comments");
        //     Writer.WriteAscii(CommentData);
        //     foreach (Comment comment in Comments)
        //     {
        //         comment.Write(Writer);
        //     }
        // }
    }
}

public class Comment : DataLoader
{
    public uint Handle;
    public string Value;

    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadUInt32();
        Value = reader.AutoReadUnicode();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32(Handle);
        writer.AutoWriteUnicode(Value);
    }
}

public class EventObject : DataLoader
{
    public string Code;
    public ushort Flags;
    public uint Handle;
    public string IconBuffer;
    public uint InstanceHandle;
    public uint ItemHandle;
    public ushort ItemType;
    public string Name;
    public ushort ObjectType;
    public ushort SystemQualifier;
    public string TypeName;

    public override void Read(ByteReader reader)
    {
        Handle = reader.ReadUInt32();
        ObjectType = reader.ReadUInt16();
        ItemType = reader.ReadUInt16();
        Name = reader.AutoReadUnicode(); //Not Sure
        TypeName = reader.AutoReadUnicode(); //Not Sure
        Flags = reader.ReadUInt16();
        if (ObjectType == 1) //FrameItemType
        {
            ItemHandle = reader.ReadUInt32();
            InstanceHandle = reader.ReadUInt32();
        }
        else if (ObjectType == 2) //ShortcutItemType
        {
            Code = reader.ReadAscii(4);
            Logger.Log("Code: " + Code);
            if (Code == "OIC2") //IconBufferCode
                IconBuffer = reader.AutoReadUnicode();
        }

        if (ObjectType == 3) //SystemItemType
            SystemQualifier = reader.ReadUInt16();
    }

    public override void Write(ByteWriter writer)
    {
        writer.WriteUInt32(Handle);
        writer.WriteUInt16(ObjectType);
        writer.WriteUInt16(ItemType);
        writer.AutoWriteUnicode(Name); //Not Sure
        writer.AutoWriteUnicode(TypeName); //Not Sure
        writer.WriteUInt16(Flags);
        if (ObjectType == 1)
        {
            writer.WriteUInt32(ItemHandle);
            writer.WriteUInt32(InstanceHandle);
        }
        else if (ObjectType == 2)
        {
            // Code = "OIC2";
            writer.WriteAscii(Code);
            if (Code == "OIC2") writer.AutoWriteUnicode(IconBuffer);
        }

        if (ObjectType == 3) writer.WriteUInt16(SystemQualifier);
    }
}