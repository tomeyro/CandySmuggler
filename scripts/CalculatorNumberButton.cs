using Godot;
using System;

public class CalculatorNumberButton : Button
{
    [Signal]
    public delegate void NumberButtonPressed(CalculatorNumberButton button);

    public override void _Ready()
    {
        Connect("pressed", this, "onNumberButtonPressed");
    }

    void onNumberButtonPressed()
    {
        EmitSignal(nameof(NumberButtonPressed), this);
    }
}
