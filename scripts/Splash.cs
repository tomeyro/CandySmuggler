using Godot;
using System;

public class Splash : Node2D
{
    SceneManager sceneManager;

    float time = 0;
    bool done = false;

    public override void _Ready()
    {
        sceneManager = GetNode<SceneManager>("/root/SceneManager");
    }

    public override void _Process(float delta)
    {
        if (done) return;
        time += delta;
        if (time >= 3)
        {
            sceneManager.ChangeScene("res://scenes/Title.tscn");
            done = true;
        }
    }
}
