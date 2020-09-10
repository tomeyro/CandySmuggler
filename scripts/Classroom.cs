using Godot;
using System;

public class Classroom : Node2D
{
    MoneyManager moneyManager;
    CandyManager candyManager;
    SceneManager sceneManager;
    SoundManager soundManager;
    SaveManager saveManager;

    float timer = 60 * 2;  // Each "school day" lasts 2 minutes.
    bool dayEnded = false;

    Popup askEndDayPopup;

    Label timeLabel;

    Popup tutorial;

    Player player;

    public override void _Ready()
    {
        moneyManager = GetNode<MoneyManager>("/root/MoneyManager");
        candyManager = GetNode<CandyManager>("/root/CandyManager");
        sceneManager = GetNode<SceneManager>("/root/SceneManager");
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        saveManager = GetNode<SaveManager>("/root/SaveManager");

        player = GetNode<Player>("YSort/Player");

        askEndDayPopup = GetNode<Popup>("CanvasLayer/Control/AskEndDay");
        timeLabel = GetNode<Label>("CanvasLayer/Control/TimeLabel");
        tutorial = GetNode<Popup>("CanvasLayer/Tutorial");

        if (!saveManager.Read<bool>("tutorial.done", false)) tutorial.Show();
    }

    public override void _Process(float delta)
    {
        if (dayEnded || tutorial.Visible) return;

        timer = Math.Max(timer - delta, 0);
        if (timer == 0) {
            endDay();
            return;
        }

        if (Input.IsActionJustPressed("cancel_minigame") && askEndDayPopup.Visible)
            closeAskEndDay();
        else if (Input.IsActionJustPressed("minigame_ok") && askEndDayPopup.Visible)
            endDay();

        var remaining = (int) Math.Ceiling(timer);
        var seconds = remaining % 60;
        var minutes = (int) Math.Floor(remaining / 60f);
        string newTime = Convert.ToString(minutes).PadLeft(2, '0') + ':' + Convert.ToString(seconds).PadLeft(2, '0');
        if (timeLabel.Text != newTime) {
            timeLabel.Text = newTime;
            if (minutes == 0 && seconds <= 10) {
                soundManager.PlaySfx(SoundManager.Sfx.Beep);
                timeLabel.Set("custom_colors/font_color", new Color("#d33636"));
            }
        }
    }

    public void askEndDay()
    {
        askEndDayPopup.Show();
    }

    void closeAskEndDay()
    {
        askEndDayPopup.Hide();
    }

    void endDay()
    {
        dayEnded = true;
        if (player.inMiniGame && player.currentMiniGame != null) {
            player.currentMiniGame.cancelMiniGame();
        }
        moneyManager.save();
        candyManager.moveAllFromHandToBackpack();
        candyManager.save();
        soundManager.PlaySfx(SoundManager.Sfx.SchoolBell);
        sceneManager.ChangeScene("res://scenes/Room.tscn");
    }

    void _on_CloseTutorial_pressed()
    {
        tutorial.Hide();
        saveManager.Write<bool>("tutorial.done", true);
    }

    void _on_TutorialButton_pressed()
    {
        tutorial.Show();
    }
}
