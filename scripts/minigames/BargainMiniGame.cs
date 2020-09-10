using Godot;
using System;

public class BargainMiniGame : MiniGame
{
    float convince = 0f;

    string[] convinceColors = new string[] {
        "#d33636",  // From Red
        "#d4533a",
        "#d46e3d",
        "#d6a344",
        "#d6bc48",
        "#d7d44c",
        "#b0d853",
        "#9dd957",
        "#8ada5a",
        "#68db62",  // To Green
    };

    ColorRect convinceBar;

    Label offerLabel;
    Label costLabel;
    Color costOriginalColor;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        offerLabel = GetNode<Label>("CanvasLayer/Control/StudentsOffer");
        GetNode<Label>("CanvasLayer/Control/PlayersOffer").Text = "" + initialPrice;
        costLabel = GetNode<Label>("CanvasLayer/Control/CandyCost");
        costLabel.Text = "" + targetCandy.Cost;
        costOriginalColor = (Color) costLabel.Get("custom_colors/font_color");
        convinceBar = GetNode<ColorRect>("CanvasLayer/Control/ConvinceBar");
    }

    public override void startMiniGame(Viewport root, Player thePlayer, NPC theNPC, Candy theCandy, int startingPrice)
    {
        base.startMiniGame(root, thePlayer, theNPC, theCandy, startingPrice);

        var random = (new Random()).Next(-2, 3) * 10;
        currentPrice = Math.Min(Math.Max((initialPrice * 60 / 100) + (theCandy.Cost * random / 100), 1), initialPrice);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (!Visible) return;

        if (Input.IsActionJustPressed("cancel_minigame"))
            cancelMiniGame();

        if (convince >= 100f) {
            soundManager.PlaySfx(SoundManager.Sfx.Coin);
            currentPrice += 1;
            convince -= 100f;
        }

        convince -= delta;
        int priceDiff = initialPrice - currentPrice;
        if (Input.IsActionJustPressed("interact"))
            convince += priceDiff > 75 ? 100f : ((currentPrice < targetCandy.Cost || priceDiff > 50) ? 75f : (priceDiff > 10 ? 60f : (priceDiff > 1 ? Math.Max(60f - (10f * (10 - priceDiff)), 20f) : 15f)));
        else
            convince -= 0.5f;
        convince = Math.Max(convince, 0f);

        Color convinceColor = new Color(convinceColors[Math.Max((Convert.ToInt32(Math.Floor(Math.Min(convince, 100f))) / 10) - 1, 0)]);
        offerLabel.Set("custom_colors/font_color", convinceColor);

        var convinceBarShader = convinceBar.Material as ShaderMaterial;
        convinceBarShader.SetShaderParam("percentage", Math.Min(convince, 100f));
        convinceBarShader.SetShaderParam("red", convinceColor.r);
        convinceBarShader.SetShaderParam("green", convinceColor.g);
        convinceBarShader.SetShaderParam("blue", convinceColor.b);

        offerLabel.Text = "" + currentPrice;

        costLabel.Set("custom_colors/font_color", currentPrice == targetCandy.Cost ? costOriginalColor : new Color(convinceColors[currentPrice < targetCandy.Cost ? 0 : 9]));

        if (currentPrice == initialPrice || Input.IsActionJustPressed("minigame_ok"))
            onAcceptButtonPressed();
    }

    void onAcceptButtonPressed()
    {
        winMiniGame();
    }

}
