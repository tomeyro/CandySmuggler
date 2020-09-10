using Godot;
using System;

public class Store : Node2D, Interactable
{
    SoundManager soundManager;
    CandyManager candyManager;
    MoneyManager moneyManager;
    Popup popup;
    Player player;
    Godot.Collections.Array<Button> buttons;

    public override void _Ready()
    {
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        candyManager.Connect("CandyMoved", this, "onCandyMoved");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");

        popup = GetNode<Popup>("CanvasLayer/Control/Popup");
    }

    public override void _Process(float delta)
    {
        if (!popup.Visible || player == null) return;

        if (Input.IsActionJustPressed("cancel_minigame"))
        {
            close();
        }
    }

    public bool canInteract()
    {
        return true;
    }

    public Vector2 getQuestionMarkPosition()
    {
        return Vector2.Zero;
    }

    public void doInteract(Player thePlayer)
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        buildButtons();
        popup.Show();
        player = thePlayer;
        player.inMiniGame = true;
    }

    void _on_CloseButton_pressed()
    {
        close();
    }

    void close()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        popup.Hide();
        player.inMiniGame = false;
        player = null;
    }

    void buildButtons()
    {
        var baseButton = popup.GetNode<Button>("Button");
        baseButton.Visible = false;
        if (buttons != null) {
            foreach (Button button in buttons)
            {
                button.QueueFree();
            }
        }
        buttons = new Godot.Collections.Array<Button>{};
        var count = 0;
        foreach (Candy candy in candyManager.Candies)
        {
            if (!candy.unlocked) continue;
            var countY = count > 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToSingle(count) / 3f) - 1) : 0;
            var newButton = baseButton.Duplicate() as Button;
            newButton.Visible = true;
            newButton.RectPosition = new Vector2(baseButton.RectPosition.x + (48 * count) - (48 * 4 * countY), baseButton.RectPosition.y + (48 * countY));
            Label buttonLabel = newButton.GetNode<Label>("Label");
            Sprite moneySprite = newButton.GetNode<Sprite>("MoneySprite");
            moneySprite.Visible = true;
            buttonLabel.Text = "" + candy.Cost;
            newButton.Disabled = candyManager.getBackpackQuantity() == candyManager.maxBackpackQuantity || moneyManager.money < candy.Cost;
            newButton.Icon = candy.Texture;
            popup.AddChild(newButton);
            count++;
            buttons.Add(newButton);
            newButton.Connect("pressed", this, "purchaseCandy", new Godot.Collections.Array {candy});
        }
        count = 0;
        foreach (Candy candy in candyManager.Candies)
        {
            int candyQty = candy.onBackpack;
            if (candyQty == 0) continue;
            var countY = count > 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToSingle(count) / 3f) - 1) : 0;
            var newButton = baseButton.Duplicate() as Button;
            newButton.Visible = true;
            newButton.RectPosition = new Vector2(baseButton.RectPosition.x + (48 * count) - (48 * 4 * countY) + (48 * 5), baseButton.RectPosition.y + (48 * countY));
            Label buttonLabel = newButton.GetNode<Label>("Label");
            Sprite moneySprite = newButton.GetNode<Sprite>("MoneySprite");
            moneySprite.Visible = false;
            buttonLabel.Text = "x" + candyQty;
            newButton.Disabled = candyQty == 0;
            newButton.Icon = candy.Texture;
            popup.AddChild(newButton);
            count++;
            buttons.Add(newButton);
            newButton.Connect("pressed", this, "sellCandy", new Godot.Collections.Array {candy});
        }
    }

    void purchaseCandy(Candy candy)
    {
        soundManager.PlaySfx(SoundManager.Sfx.KaChing1);
        moneyManager.takeMoney(candy.Cost);
        candyManager.moveFromStoreToBackpack(candy);
    }

    void sellCandy(Candy candy)
    {
        soundManager.PlaySfx(SoundManager.Sfx.KaChing2);
        moneyManager.addMoney(candy.Cost);
        candyManager.moveFromBackpackToStore(candy);
    }

    void onCandyMoved()
    {
        buildButtons();
    }
}
