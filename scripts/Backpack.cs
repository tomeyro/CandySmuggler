using Godot;
using System;

public class Backpack : StaticBody2D, Interactable
{
    CandyManager candyManager;
    Popup popup;
    Player player;

    Godot.Collections.Array<Button> buttons;

    [Export]
    int questionMarkPositionX = 0;
    [Export]
    int questionMarkPositionY = -22;
    [Export]
    bool buttonsEnabled = true;

    SoundManager soundManager;

    public override void _Ready()
    {
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        candyManager.Connect("CandyMoved", this, "onCandyMoved");
        popup = GetNode<Popup>("CanvasLayer/Control/Popup");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
    }

    public bool canInteract()
    {
        return true;
    }

    public Vector2 getQuestionMarkPosition()
    {
        return GlobalPosition + new Vector2(questionMarkPositionX, questionMarkPositionY);
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
            int candyQty = candy.onBackpack;
            if (candyQty == 0) continue;
            var countY = count > 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToSingle(count) / 3f) - 1) : 0;
            var newButton = baseButton.Duplicate() as Button;
            newButton.Visible = true;
            newButton.RectPosition = new Vector2(baseButton.RectPosition.x + (48 * count) - (48 * 4 * countY), baseButton.RectPosition.y + (48 * countY));
            Label buttonLabel = newButton.GetNode<Label>("Label");
            buttonLabel.Text = "x" + candyQty;
            newButton.Disabled = !buttonsEnabled || candyQty == 0 || candyManager.getOnHandQuantity() == candyManager.maxHandQuantity;
            newButton.Icon = candy.Texture;
            popup.AddChild(newButton);
            count++;
            buttons.Add(newButton);
            newButton.Connect("pressed", this, "moveFromBackpackToHand", new Godot.Collections.Array {candy});
        }
        count = 0;
        foreach (Candy candy in candyManager.Candies)
        {
            int candyQty = candy.onHand;
            if (candyQty == 0) continue;
            var countY = count > 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToSingle(count) / 3f) - 1) : 0;
            var newButton = baseButton.Duplicate() as Button;
            newButton.Visible = true;
            newButton.RectPosition = new Vector2(baseButton.RectPosition.x + (48 * count) - (48 * 4 * countY) + (48 * 5), baseButton.RectPosition.y + (48 * countY));
            Label buttonLabel = newButton.GetNode<Label>("Label");
            buttonLabel.Text = "x" + candyQty;
            newButton.Disabled = !buttonsEnabled || candyQty == 0;
            newButton.Icon = candy.Texture;
            popup.AddChild(newButton);
            count++;
            buttons.Add(newButton);
            newButton.Connect("pressed", this, "moveFromHandToBackpack", new Godot.Collections.Array {candy});
        }
    }

    void moveFromHandToBackpack(Candy candy)
    {
        soundManager.PlaySfx(SoundManager.Sfx.SfxDown);
        candyManager.moveFromHandToBackpack(candy);
    }

    void moveFromBackpackToHand(Candy candy)
    {
        soundManager.PlaySfx(SoundManager.Sfx.SfxUp);
        candyManager.moveFromBackpackToHand(candy);
    }

    void onCandyMoved()
    {
        buildButtons();
    }

    public void doInteract(Player thePlayer)
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        buildButtons();
        popup.Show();
        player = thePlayer;
        player.inMiniGame = true;
    }

    public override void _Process(float delta)
    {
        if (!popup.Visible || player == null) return;

        if (Input.IsActionJustPressed("cancel_minigame"))
        {
            close();
        }
    }

    void close()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        popup.Hide();
        player.inMiniGame = false;
        player = null;
    }

}
