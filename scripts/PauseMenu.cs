using Godot;
using System;

public class PauseMenu : CanvasLayer
{
    Popup pausePopup;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pausePopup = GetNode<Popup>("PausePopup");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause")) {
            GetTree().Paused = !GetTree().Paused;
            if (GetTree().Paused)
                pausePopup.Show();
            else
                pausePopup.Hide();
        }
    }
}
