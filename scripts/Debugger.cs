using Godot;
using System;

public class Debugger : Node
{
    SaveManager saveManager;
    CandyManager candyManager;
    MoneyManager moneyManager;

    public override void _Ready()
    {
        PauseMode = PauseModeEnum.Process;
        saveManager = GetNode<SaveManager>("/root/SaveManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("debug_print"))
            Print();
    }

    void Print()
    {
        GD.Print("-------------");
        GD.Print("SaveManager:");
        GD.Print(saveManager);
        GD.Print("-------------");
        GD.Print("CandyManager:");
        GD.Print(candyManager);
        GD.Print("-------------");
        GD.Print("MoneyManager:");
        GD.Print(moneyManager);
    }
}
