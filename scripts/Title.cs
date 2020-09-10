using Godot;
using System;

public class Title : Node2D
{
    SceneManager sceneManager;
    SoundManager soundManager;
    SaveManager saveManager;
    CandyManager candyManager;
    MoneyManager moneyManager;

    Popup credits;

    int resetCounter = 0;

    public override void _Ready()
    {
        GetNode<CPUParticles2D>("CPUParticles2D").Emitting = true;
        credits = GetNode<Popup>("Credits");

        sceneManager = GetNode<SceneManager>("/root/SceneManager");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        saveManager = GetNode<SaveManager>("/root/SaveManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
    }

    void _on_StartGame_pressed()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        sceneManager.ChangeScene("res://scenes/Room.tscn");
    }

    void _on_Credits_pressed()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        credits.Show();
    }

    void _on_CloseCredits_pressed()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        credits.Hide();
    }

    void _on_Close_pressed()
    {
        soundManager.PlaySfx(SoundManager.Sfx.Button);
        GetTree().Quit();
    }

    public override void _Process(float delta)
    {
        var resetBtn = GetNode<Button>("ResetSave");
        resetBtn.Visible = saveManager.Read<Single>("counter", 0) > 0;
        resetBtn.GetNode<Label>("Label1").Visible = resetCounter == 1;
        resetBtn.GetNode<Label>("Label2").Visible = resetCounter == 2;
        resetBtn.GetNode<Label>("Label3").Visible = resetCounter == 3;
        resetBtn.GetNode<Label>("Label4").Visible = resetCounter == 4;
    }

    void onResetButtonPressed()
    {
        resetCounter += 1;
        if (resetCounter == 5) {
            saveManager.ResetData();
            candyManager.Load();
            moneyManager.Load();
        }
    }

}
