using Godot;
using System;

public class SoundManager : Node2D
{

    public enum Sfx
    {
        Button,

        SfxUp,
        SfxDown,

        Aw1,
        Aw2,
        Aw3,

        Eh1,
        Eh2,
        Eh3,

        KaChing1,
        KaChing2,

        Door1,
        Door2,

        Coin,

        SchoolBell,

        Beep,
    }

    public override void _Ready()
    {

    }

    public void PlaySfx(SoundManager.Sfx soundNode)
    {
        var node = GetNode<AudioStreamPlayer>(soundNode.ToString("G"));
        if (node.Playing) node.Stop();
        node.Play();
    }

    public void PlaySfx(SoundManager.Sfx[] soundNodes)
    {
        var rand = new Random();
        var idx = rand.Next(0, soundNodes.Length);
        PlaySfx(soundNodes[idx]);
    }
}
