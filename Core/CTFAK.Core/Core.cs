﻿using System;
using System.IO;
using CTFAK.CCN.Chunks;
using CTFAK.FileReaders;
using CTFAK.Utils;
using Joveler.Compression.ZLib;

namespace CTFAK
{
    public delegate void SaveHandler(int index, int all);
    public delegate void LoggerHandler(string output);
    public delegate void SimpleMessage<T>(T data);
    public delegate T2 SimpleMessage<T,T2>(T data);
    public class Core
    {
        public static IFileReader currentReader;
        public static string parameters;
        public static string path;
        public static void Init()
        {
            ChunkList.Init();
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Console.WriteLine(e.ExceptionObject.GetType());
                //NativeLib.MessageBox((IntPtr)0, $"{e.Exception.ToString()}", "ERROR", 0);
            };
            ZLibInit.GlobalInit("x64\\zlibwapi.dll");
            String libraryFile = Path.Combine(Path.GetDirectoryName(typeof(Core).Assembly.Location), "x64",
                "CTFAK-Native.dll");
            NativeLib.LoadLibrary(libraryFile);
        }
    }
}