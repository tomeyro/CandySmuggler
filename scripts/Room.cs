using Godot;
using System;

public class Room : Node2D
{
    SceneManager sceneManager;
    CandyManager candyManager;
    MoneyManager moneyManager;
    SaveManager saveManager;

    Popup intro;

    public override void _Ready()
    {
        sceneManager = GetNode<SceneManager>("/root/SceneManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
        saveManager = GetNode<SaveManager>("/root/SaveManager");

        intro = GetNode<Popup>("Intro");
        if (!saveManager.Read<bool>("intro.done", false)) intro.Show();
    }

    void _on_TitleScreenButton_pressed()
    {
        candyManager.save();
        moneyManager.save();
        sceneManager.ChangeScene("res://scenes/Title.tscn");
    }

    void _on_CloseIntro_pressed()
    {
        intro.Hide();
        saveManager.Write<bool>("intro.done", true);
    }

    void _on_IntroButton_pressed()
    {
        intro.Show();
    }
}
