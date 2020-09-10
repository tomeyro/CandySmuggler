using Godot;
using System;

public class MiniGame : Node2D
{
    static public string[] MiniGames = new string[] {
        "Bargain",
    };

    public Player player;
    public NPC npc;

    public int initialPrice = 0;
    public int currentPrice = 0;

    Control calculator;

    Label calculatorScreen;
    Button acceptButton;
    int acceptCostChance = 100;
    bool pressingNumber = false;
    Godot.Collections.Array<Button> buttons;
    public CandyManager candyManager;
    public Candy targetCandy;
    Candy selectedCandy;

    protected SoundManager soundManager;
    protected MoneyManager moneyManager;

    [Signal]
    public delegate void MiniGameWon(MiniGame miniGame);
    [Signal]
    public delegate void MiniGameLost(MiniGame miniGame);
    [Signal]
    public delegate void MiniGameCancelled(MiniGame miniGame);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
        if (GetType() == typeof(MiniGame)) _ReadyMiniGame();
        ConnectEvents();
    }

    void _ReadyMiniGame()
    {
        calculator = GetNode<Control>("CanvasLayer/Calculator");
        calculatorScreen = GetNode<Label>("CanvasLayer/Calculator/Screen");
        acceptButton = GetNode<Button>("CanvasLayer/Calculator/AcceptButton");
        updateCalculatorScreen();
        buildButtons();
    }

    public virtual void ConnectEvents()
    {
        Connect(nameof(MiniGameWon), player, "onMiniGameWon");
        Connect(nameof(MiniGameLost), player, "onMiniGameLost");
        Connect(nameof(MiniGameWon), npc, "onMiniGameWon");
        Connect(nameof(MiniGameLost), npc, "onMiniGameLost");
        Connect(nameof(MiniGameCancelled), npc, "onMiniGameCancelled");
        Connect(nameof(MiniGameWon), candyManager, "onMiniGameWon");
        Connect(nameof(MiniGameWon), moneyManager, "onMiniGameWon");
    }

    public static void start(Viewport root, Player thePlayer, NPC theNPC, Candy theCandy, int startingPrice = 0, string scene="res://scenes/minigames/MiniGame.tscn")
    {
        PackedScene mg_scene = GD.Load<PackedScene>(scene);
        MiniGame mg_node = mg_scene.Instance() as MiniGame;
        mg_node.startMiniGame(root, thePlayer, theNPC, theCandy, startingPrice);
    }

    public virtual void startMiniGame(Viewport root, Player thePlayer, NPC theNPC, Candy theCandy, int startingPrice)
    {
        player = thePlayer;
        npc = theNPC;
        initialPrice = startingPrice;
        currentPrice = startingPrice;
        targetCandy = theCandy;

        root.AddChild(this);
        soundManager.PlaySfx(SoundManager.Sfx.Button);

        player.inMiniGame = true;
        player.currentMiniGame = this;
        npc.inMiniGame = true;
    }

    public override void _Process(float delta)
    {
        if (!Visible) return;

        if (Input.IsActionJustPressed("cancel_minigame")) {
            cancelMiniGame();
            return;
        }

        acceptButton.Disabled = currentPrice == 0 || selectedCandy == null;

        int num = -1;
        if (Input.IsKeyPressed((int) Godot.KeyList.Key0) || Input.IsKeyPressed((int) Godot.KeyList.Kp0)) num = 0;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key1) || Input.IsKeyPressed((int) Godot.KeyList.Kp1)) num = 1;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key2) || Input.IsKeyPressed((int) Godot.KeyList.Kp2)) num = 2;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key3) || Input.IsKeyPressed((int) Godot.KeyList.Kp3)) num = 3;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key4) || Input.IsKeyPressed((int) Godot.KeyList.Kp4)) num = 4;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key5) || Input.IsKeyPressed((int) Godot.KeyList.Kp5)) num = 5;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key6) || Input.IsKeyPressed((int) Godot.KeyList.Kp6)) num = 6;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key7) || Input.IsKeyPressed((int) Godot.KeyList.Kp7)) num = 7;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key8) || Input.IsKeyPressed((int) Godot.KeyList.Kp8)) num = 8;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Key9) || Input.IsKeyPressed((int) Godot.KeyList.Kp9)) num = 9;
        else if (Input.IsKeyPressed((int) Godot.KeyList.Backspace)) num = 10;
        else pressingNumber = false;
        if (!pressingNumber && num > -1) {
            pressingNumber = true;
            if (num < 10) addNumber(num);
            else onEraseButtonPressed();
        }

        if (Input.IsActionJustPressed("minigame_ok") && !acceptButton.Disabled) {
            onAcceptButtonPressed();
        }
    }

    public void cancelMiniGame()
    {
        EmitSignal(nameof(MiniGameCancelled), this);
        CloseMiniGame();
    }

    public void winMiniGame()
    {
        EmitSignal(nameof(MiniGameWon), this);
        CloseMiniGame();
    }

    public void loseMiniGame()
    {
        EmitSignal(nameof(MiniGameLost), this);
        CloseMiniGame();
    }

    public void CloseMiniGame()
    {
        player.inMiniGame = false;
        player.currentMiniGame = null;
        npc.inMiniGame = false;
        QueueFree();
    }

    void onNumberButtonPressed(CalculatorNumberButton button)
    {
        int btnValue = Int16.Parse(button.Text);
        addNumber(btnValue);
    }

    void addNumber(int num)
    {
        soundManager.PlaySfx(SoundManager.Sfx.SfxUp);
        currentPrice = Math.Min((currentPrice * 10) + num, 9999999);
        updateCalculatorScreen();
    }

    void onEraseButtonPressed()
    {
        soundManager.PlaySfx(SoundManager.Sfx.SfxDown);
        currentPrice = Math.Max(currentPrice / 10, 0);
        updateCalculatorScreen();
    }

    void updateCalculatorScreen()
    {
        calculatorScreen.Text = Convert.ToString(currentPrice).PadLeft(7, '0');
        int earningPercentage = selectedCandy != null ? (currentPrice * 100 / selectedCandy.Cost) - 100 : 100;
        acceptCostChance = earningPercentage > 100 ? 0 : (earningPercentage == 100 ? 5 : (earningPercentage >= 50 ? 100 - earningPercentage : (earningPercentage > 0 ? 95 : 100)));
        calculator.GetNode<Label>("HighCostLabel").Visible = selectedCandy != null && acceptCostChance < 95;
    }

    void onAcceptButtonPressed()
    {
        var rand = new Random();
        if (acceptCostChance != 100 && (acceptCostChance <= 0 || rand.Next(1, 101) > acceptCostChance)) {
            loseMiniGame();
            return;
        }
        if (targetCandy != selectedCandy) {
            int match = (int) Math.Round(targetCandy.getMatchPercentage(selectedCandy));
            if (match == 0) {
                loseMiniGame();
                return;
            }
            int acceptCandyChance = Math.Min(Math.Max(match + (match >= 50 ? 25 : -25), 5), 95);
            if (rand.Next(1, 101) > acceptCandyChance) {
                loseMiniGame();
                return;
            }
        }
        string game = MiniGames[rand.Next(0, MiniGames.Length)];
        Visible = false;
        start(GetTree().Root, player, npc, selectedCandy, currentPrice, "res://scenes/minigames/" + game + "MiniGame.tscn");
        QueueFree();
    }

    void buildButtons()
    {
        var baseButton = calculator.GetNode<Button>("CandyButton");
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
            int candyQty = candy.onHand;
            if (candyQty == 0) continue;
            var countY = count > 0 ? Convert.ToInt32(Math.Ceiling(Convert.ToSingle(count) / 3f) - 1) : 0;
            var newButton = baseButton.Duplicate() as Button;
            newButton.Visible = true;
            newButton.GetNode<Sprite>("CheckSprite").Visible = selectedCandy == candy;
            newButton.GetNode<Label>("CostLabel").Text = Convert.ToString(candy.Cost).PadLeft(3, '0');
            newButton.RectPosition = new Vector2(baseButton.RectPosition.x + (48 * count) - (48 * 4 * countY), baseButton.RectPosition.y + (52 * countY));
            newButton.Icon = candy.Texture;
            calculator.AddChild(newButton);
            count++;
            buttons.Add(newButton);
            newButton.Connect("pressed", this, "onCandySelected", new Godot.Collections.Array {candy});
        }
        calculator.GetNode<Label>("CandyErrorLabel").Visible = count == 0;
        calculator.GetNode<Label>("DifferentCandyLabel").Visible = selectedCandy != null && selectedCandy != targetCandy;
    }

    void onCandySelected(Candy candy)
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        selectedCandy = candy;
        currentPrice = candy.Cost;
        updateCalculatorScreen();
        buildButtons();
    }
}
