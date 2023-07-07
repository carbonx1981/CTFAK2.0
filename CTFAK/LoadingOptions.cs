﻿namespace CTFAK;

public class LoadingOptions
{
    public static LoadingOptions Default => new();

    public bool LoadImages { get; set; } = true;

    public bool LoadSounds { get; set; } = true;
}