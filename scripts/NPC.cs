using Godot;
using System;


public interface Interactable
{
    bool canInteract();
    void doInteract(Player player);
    Vector2 getQuestionMarkPosition();
}
public class NPC : Player, Interactable
{

    Node2D speechBubble;

    Candy wantCandy;

    [Export]
    float wantChance = 1;
    int failedChecks = 0;
    [Export]
    float failedCheckIncrements = 0.05f;
    float candyTimeout = 0;
    [Export]
    float candyTimeoutSeconds = 4;

    CPUParticles2D happyParticles;
    CPUParticles2D madParticles;
    CPUParticles2D sadParticles;

    [Export]
    float requestFromAllCandiesChance = 0.1f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        speechBubble = GetNode<Node2D>("SpeechBubble");

        happyParticles = GetNode<CPUParticles2D>("HappyParticles");
        madParticles = GetNode<CPUParticles2D>("MadParticles");
        sadParticles = GetNode<CPUParticles2D>("SadParticles");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (inMiniGame) return;

        Vector2 move_vector = new Vector2(0, 0);

        updateMovementStateFromMovement(move_vector);
        updateSideStateFromMovement(move_vector);

        checkWantCandy(delta);
    }

    public bool canInteract()
    {
        return wantCandy != null;
    }

    public void doInteract(Player player)
    {
        if (wantCandy == null)
            return;
        candyTimeout = candyTimeoutSeconds;
        MiniGame.start(GetTree().Root, player, this, wantCandy);
        wantCandy = null;
    }

    public Vector2 getQuestionMarkPosition()
    {
        return GlobalPosition + new Vector2(15, -55);
    }

    public void _on_Timer_timeout()
    {
        if (wantCandy == null && candyTimeout == 0f) {
            GD.Randomize();
            double rand = GD.RandRange(0, 1);
            float checkValue = wantChance + (failedChecks * failedCheckIncrements);
            if (rand <= checkValue) {
                // 10% chance to ask for a candy that's not on the backpack
                if (GD.RandRange(0, 1) <= requestFromAllCandiesChance)
                    wantCandy = candyManager.getRandomCandy();
                else
                    wantCandy = candyManager.getRandomAvailableCandy();
            }
            failedChecks = wantCandy != null ? 0 : (failedChecks + 1);
        }
    }

    public void checkWantCandy(float delta)
    {
        candyTimeout = Math.Max(candyTimeout - delta, 0f);
        if (speechBubble == null)
            return;
        speechBubble.Visible = wantCandy != null;
        if (speechBubble.Visible)
            speechBubble.GetNode<Sprite>("CandySprite").Texture = wantCandy.Texture;
    }

    void onMiniGameCancelled(MiniGame miniGame)
    {
        sadParticles.Emitting = true;
        soundManager.PlaySfx(new SoundManager.Sfx[] { SoundManager.Sfx.Aw1, SoundManager.Sfx.Aw2, SoundManager.Sfx.Aw3 });
    }

    void onMiniGameWon(MiniGame miniGame)
    {
        happyParticles.Emitting = true;
    }

    void onMiniGameLost(MiniGame miniGame)
    {
        madParticles.Emitting = true;
        soundManager.PlaySfx(new SoundManager.Sfx[] { SoundManager.Sfx.Eh1, SoundManager.Sfx.Eh2, SoundManager.Sfx.Eh3 });
    }
}
