using Godot;
using System;

public class RoomDoor : Node2D, Interactable
{
    SoundManager soundManager;
    SceneManager sceneManager;
    CandyManager candyManager;
    MoneyManager moneyManager;

    public override void _Ready()
    {
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        sceneManager = GetNode<SceneManager>("/root/SceneManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
    }

    public bool canInteract()
    {
        return true;
    }

    public Vector2 getQuestionMarkPosition()
    {
        return GlobalPosition + new Vector2(0, -75);
    }

    public void doInteract(Player thePlayer)
    {
        candyManager.save();
        moneyManager.save();
        soundManager.PlaySfx(new SoundManager.Sfx[] { SoundManager.Sfx.Door1, SoundManager.Sfx.Door2 });
        sceneManager.ChangeScene("res://scenes/Classroom.tscn");
    }
}
