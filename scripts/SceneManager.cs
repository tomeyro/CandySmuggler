using Godot;
using System;

public class SceneManager : Node2D
{
    string targetScene;
    CanvasLayer canvasLayer;
    ColorRect colorRect;
    AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
        colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
        animationPlayer = GetNode<AnimationPlayer>("CanvasLayer/ColorRect/AnimationPlayer");

        loadScenes();
    }

    public override void _Process(float delta)
    {
        colorRect.Visible = animationPlayer.IsPlaying();
    }

    void loadScenes()
    {
        ResourceLoader.Load("res://scenes/Splash.tscn");
        ResourceLoader.Load("res://scenes/Title.tscn");
        ResourceLoader.Load("res://scenes/Room.tscn");
        ResourceLoader.Load("res://scenes/Classroom.tscn");
        ResourceLoader.Load("res://scenes/minigames/MiniGame.tscn");
        ResourceLoader.Load("res://scenes/minigames/BargainMiniGame.tscn");
    }

    public void ChangeScene(string scene)
    {
        targetScene = scene;
        animationPlayer.Play("close");
    }

    void _on_AnimationPlayer_animation_finished(string animation)
    {
        if (animation == "close")
        {
            GetTree().ChangeScene(targetScene);
            animationPlayer.Play("open");
        }
    }
}
