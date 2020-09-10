using Godot;
using System;
using System.Linq;

enum MovementStates {
    walking,
    idle,
}
enum SideStates {
    front,
    back,
}

public class Player : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export]
    private NodePath detectorAreaPath;
    private Area2D detectorArea;
    private Area2D[] interactableAreas;

    [Export]
    private NodePath questionMarkPath;
    private Sprite questionMark;
    Vector2 questionMarkOriginalPos;

    protected AnimationPlayer movement_player;
    protected AnimationPlayer turning_player;

    protected Node2D sprites;

    private float speed = 10000f;
    private bool running = false;

    [Export]
    private float runMultiplier = 2f;

    private MovementStates movement_state = MovementStates.idle;
    private SideStates side_state = SideStates.front;

    public bool inMiniGame = false;
    public MiniGame currentMiniGame;
    CPUParticles2D moneyParticles;

    protected SaveManager saveManager;
    protected MoneyManager moneyManager;
    protected CandyManager candyManager;
    protected SoundManager soundManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        movement_player = GetNode<AnimationPlayer>("Animations/Movement");
        turning_player = GetNode<AnimationPlayer>("Animations/Turning");

        sprites = GetNode<Node2D>("Sprites");

        if (detectorAreaPath != null)
            detectorArea = GetNode<Area2D>(detectorAreaPath);
        if (questionMarkPath != null) {
            questionMark = GetNode<Sprite>(questionMarkPath);
            questionMarkOriginalPos = questionMark.Position;
        }

        interactableAreas = new Area2D[0];

        saveManager = GetNode<SaveManager>("/root/SaveManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");

        if (GetType() == typeof(Player)) _ReadyPlayer();
    }

    public void _ReadyPlayer()
    {
        moneyParticles = GetNode<CPUParticles2D>("MoneyParticles");
        moneyManager.Connect(nameof(MoneyManager.MoneyChanged), this, "updateMoney");
        updateMoney();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (inMiniGame) return;

        Vector2 move_vector = new Vector2(0, 0);
        if (Input.IsActionPressed("move_up"))
            move_vector.y = -1f;
        if (Input.IsActionPressed("move_down"))
            move_vector.y = 1f;
        if (Input.IsActionPressed("move_left"))
            move_vector.x = -1f;
        if (Input.IsActionPressed("move_right"))
            move_vector.x = 1f;
        // running = Input.IsActionPressed("run");

        updateMovementStateFromMovement(move_vector);
        updateSideStateFromMovement(move_vector);
        if (move_vector.x != 0 || move_vector.y != 0) {
            MoveAndSlide(move_vector * speed * delta * (running ? runMultiplier : 1));
        }

        checkInteractable();
        if (Input.IsActionJustPressed("interact"))
            startInteraction();
    }

    public void updateMovementStateFromMovement(Vector2 move_vector)
    {
        bool moving = (move_vector.x != 0f || move_vector.y != 0f);
        if (moving && (movement_state != MovementStates.walking || !movement_player.IsPlaying())) {
            sprites.Scale = new Vector2(sprites.Scale.x, 1f);
            movement_player.Play("walking");
        }
        if (!moving && (movement_state != MovementStates.idle || !movement_player.IsPlaying())) {
            sprites.Rotation = 0f;
            movement_player.Play("idle");
        }
        movement_player.PlaybackSpeed = moving && running ? runMultiplier : 1f;
        movement_state = moving ? MovementStates.walking : MovementStates.idle;
    }

    protected void updateSideStateFromMovement(Vector2 move_vector)
    {
        if (move_vector.y < 0 && side_state == SideStates.front) {
            side_state = SideStates.back;
            turning_player.Stop();
            turning_player.Play("turn");
        }
        if (move_vector.y > 0 && side_state == SideStates.back) {
            side_state = SideStates.front;
            turning_player.Stop();
            turning_player.Play("turn");
        }
    }

    public void updateSideSprite()
    {
        sprites.GetNode<Sprite>("SpriteFront").Visible = side_state == SideStates.front;
        sprites.GetNode<Sprite>("SpriteBack").Visible = side_state == SideStates.back;
    }

    public void _on_Area2D_area_entered(Area2D area)
    {
        Area2D[] newList = new Area2D[interactableAreas.Length + 1];
        interactableAreas.CopyTo(newList, 0);
        newList[interactableAreas.Length] = area;
        interactableAreas = newList;
    }

    public void _on_Area2D_area_exited(Area2D area)
    {
        if (interactableAreas.Contains(area)) {
            Area2D[] newList = new Area2D[interactableAreas.Length - 1];
            int idx = 0;
            foreach (Area2D interactableArea in interactableAreas) {
                if (area != interactableArea) {
                    newList[idx++] = interactableArea;
                }
            }
            interactableAreas = newList;
        }
    }

    public Interactable getInteractable()
    {
        if (interactableAreas.Length == 0)
            return null;
        float closestDistance = -1f;
        Area2D closestArea = null;
        foreach (Area2D area in interactableAreas) {
            Node2D parent = area.GetParent<Node2D>();
            if (!(parent is Interactable) || !(parent as Interactable).canInteract())
                continue;
            // If we are above the interactable we need to be facing front.
            if (GlobalPosition.y < parent.GlobalPosition.y && side_state == SideStates.back)
                continue;
            // If we are below the interactable we need to be facing back.
            if (parent.GlobalPosition.y < GlobalPosition.y && side_state == SideStates.front)
                continue;
            float distance = GlobalPosition.DistanceTo(area.GlobalPosition);
            if (closestDistance < 0 || distance < closestDistance) {
                closestDistance = distance;
                closestArea = area;
            }
        }
        if (closestArea == null)
            return null;
        return closestArea.GetParent<Interactable>();
    }

    private void startInteraction()
    {
        Interactable interactable = getInteractable();
        if (interactable == null)
            return;
        interactable.doInteract(this);
    }

    private void checkInteractable()
    {
        Interactable interactable = getInteractable();
        if (questionMark != null) {
            questionMark.Visible = false;
            if (interactable != null) {
                questionMark.Visible = true;
                var newPos = interactable.getQuestionMarkPosition();
                if (newPos == Vector2.Zero)
                    questionMark.Position = questionMarkOriginalPos;
                else
                    questionMark.GlobalPosition = newPos;
            }
        }
    }

    void onMiniGameWon(MiniGame miniGame)
    {
        moneyParticles.Emitting = true;
        soundManager.PlaySfx(new SoundManager.Sfx[] { SoundManager.Sfx.KaChing1, SoundManager.Sfx.KaChing2 });
    }

    void onMiniGameLost(MiniGame miniGame)
    {
    }

    void updateMoney()
    {
        var moneyLabel = GetNode<Label>("UI/Control/MoneyLabel");
        moneyLabel.Text = Convert.ToString(moneyManager.money).PadLeft(10, '0');
    }
}
