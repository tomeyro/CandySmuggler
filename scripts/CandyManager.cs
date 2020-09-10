using Godot;
using System;
using System.Linq;

public class Candy : Node
{
    public string CandyName;
    public int Cost;
    public string[] Attributes;
    public int Index;
    public Texture Texture;

    public bool unlocked = false;
    public int onHand = 0;
    public int onBackpack = 0;

    public enum Locations {
        Hand,
        Backpack,
    }

    public Candy(string name, int cost, string[] attributes)
    {
        CandyName = name;
        Cost = cost;
        string[] splitName = CandyName.ToLower().Split(" ");
        Attributes = new string[splitName.Length + attributes.Length];
        splitName.CopyTo(Attributes, 0);
        attributes.CopyTo(Attributes, splitName.Length);
    }

    public Candy(string name, int cost) : this(name, cost, new string[] {}) {}

    public float getMatchPercentage(Candy candy)
    {
        float attrCount = 0;
        float matchCount = 0;
        foreach (string attr in Attributes)
        {
            attrCount += 1;
            if (Array.IndexOf(candy.Attributes, attr) > -1)
            {
                matchCount += 1;
            }
        }
        return attrCount > 0f ? ((matchCount * 100f) / attrCount) : 100f;
    }

    public override string ToString()
    {
        return $"Candy '{CandyName}' ({Index}):\n  - Cost: {Cost}\n  - Unlocked: {unlocked}\n  - OnHand: {onHand}\n  - OnBackpack: {onBackpack}";
    }
}

public class CandyManager : Node
{
    SaveManager saveManager;

    public Candy[] Candies = new Candy[] {
        new Candy("Strawberry Candy", 5, new string[] {"small", "hard"}),
        new Candy("Milk Candy", 25, new string[] {"small", "chewable"}),
        new Candy("Sour Candy", 100, new string[] {"small", "hard"}),
        new Candy("Bubble Gum", 200, new string[] {"small", "chewable"}),
        new Candy("Gummy Bear", 400, new string[] {"small", "chewable"}),
        new Candy("Mint Candy", 750, new string[] {"small", "hard"}),
    };

    public Godot.Collections.Array unlockedCandies;

    public Godot.Collections.Dictionary backpackCandies;
    public int maxBackpackQuantity;

    public Godot.Collections.Dictionary onHandCandies;
    public int maxHandQuantity;

    [Signal]
    public delegate void CandyMoved();

    public override void _Ready()
    {
        saveManager = GetNode<SaveManager>("/root/SaveManager");
        Load();
    }

    public void Load()
    {
        int idx = 0;
        foreach (Candy candy in Candies) {
            candy.Index = idx++;
            string res_name = "res://resources/imgs/candies/" + candy.CandyName + ".png";
            Texture texture = (Texture)GD.Load(res_name);
            candy.Texture = texture;
            candy.unlocked = saveManager.Read<bool>("candy." + candy.Index + ".unlocked", candy.Index <= 5);
            candy.onBackpack = Convert.ToInt32(saveManager.Read<Single>("candy." + candy.Index + ".onBackpack", 0));
            candy.onHand = 0;
        }
        maxHandQuantity = Convert.ToInt32(saveManager.Read<Single>("maxHandQuantity", 4));
        maxBackpackQuantity = Convert.ToInt32(saveManager.Read<Single>("maxBackpackQuantity", 20));
    }

    public Candy getRandomCandy()
    {
        var unlockedCandies = (
            from candy in Candies.AsQueryable()
            where candy.unlocked == true
            select candy
        ).ToArray();
        return unlockedCandies[(new Random()).Next(0, unlockedCandies.Length)];
    }

    public Candy getRandomAvailableCandy()
    {
        var unlockedCandies = (
            from candy in Candies.AsQueryable()
            where candy.unlocked == true && (candy.onBackpack > 0 || candy.onHand > 0)
            select candy
        ).ToArray();
        if (unlockedCandies.Length == 0) return getRandomCandy();
        return unlockedCandies[(new Random()).Next(0, unlockedCandies.Length)];
    }

    public int getOnHandQuantity()
    {
        int sum = 0;
        foreach (Candy candy in Candies)
            sum += candy.onHand;
        return sum;
    }

    public int getBackpackQuantity()
    {
        int sum = 0;
        foreach (Candy candy in Candies)
            sum += candy.onBackpack;
        return sum;
    }

    bool reduceBackpack(Candy candy)
    {
        if (candy.onBackpack > 0) {
            candy.onBackpack -= 1;
            return true;
        }
        return false;
    }

    bool reduceOnHand(Candy candy)
    {
        if (candy.onHand > 0) {
            candy.onHand -= 1;
            return true;
        }
        return false;
    }

    public void moveFromBackpackToHand(Candy candy)
    {
        if (getOnHandQuantity() < maxHandQuantity && reduceBackpack(candy)) {
            candy.onHand += 1;
            EmitSignal(nameof(CandyMoved));
        }
    }

    public void moveFromHandToBackpack(Candy candy)
    {
        if (getBackpackQuantity() < maxBackpackQuantity && reduceOnHand(candy)) {
            candy.onBackpack += 1;
            EmitSignal(nameof(CandyMoved));
        }
    }

    public void moveAllFromHandToBackpack()
    {
        foreach (Candy candy in Candies) {
            if (candy.onHand > 0) {
                candy.onBackpack += candy.onHand;
                candy.onHand = 0;
            }
        }
    }

    public void moveFromStoreToBackpack(Candy candy)
    {
        if (getBackpackQuantity() < maxBackpackQuantity) {
            candy.onBackpack += 1;
            EmitSignal(nameof(CandyMoved));
        }
    }

    public void moveFromBackpackToStore(Candy candy)
    {
        if (reduceBackpack(candy))
            EmitSignal(nameof(CandyMoved));
    }

    void onMiniGameWon(MiniGame miniGame)
    {
        if (reduceOnHand(miniGame.targetCandy))
            EmitSignal(nameof(CandyMoved));
    }

    public void save()
    {
        saveManager.Write<Single>("maxBackpackQuantity", maxBackpackQuantity);
        saveManager.Write<Single>("maxHandQuantity", maxHandQuantity);
        foreach (Candy candy in Candies) {
            saveManager.Write<bool>("candy." + candy.Index + ".unlocked", candy.unlocked);
            saveManager.Write<Single>("candy." + candy.Index + ".onBackpack", candy.onBackpack);
        }
    }

    public override string ToString()
    {
        string str = "Max Backpack Qty: " + maxBackpackQuantity + "\nMax On Hand Qty: " + maxHandQuantity;
        foreach (Candy candy in Candies) {
            str += "\n" + candy;
        }
        return str;
    }

}
