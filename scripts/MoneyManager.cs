using Godot;
using System;

public class MoneyManager : Node
{
    SaveManager saveManager;

    public int money = 0;

    [Signal]
    public delegate void MoneyChanged();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        saveManager = GetNode<SaveManager>("/root/SaveManager");
        Load();
    }

    public void Load()
    {
        money = Convert.ToInt32(Math.Max(saveManager.Read<Single>("money", 50), 0));
    }

    public void addMoney(int add)
    {
        money = Math.Min(money + add, 99999999);
        EmitSignal(nameof(MoneyChanged));
    }

    public void takeMoney(int take)
    {
        money = Math.Max(money - take, 0);
        EmitSignal(nameof(MoneyChanged));
    }

    void onMiniGameWon(MiniGame miniGame)
    {
        addMoney(miniGame.currentPrice);
    }

    public void save()
    {
        saveManager.Write<Single>("money", money);
    }

    public override string ToString()
    {
        return "Current Money: " + money;
    }

}
