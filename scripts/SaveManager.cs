using Godot;
using System;

public class SignalParameter : Node
{
    object Value;

    public SignalParameter(object value)
    {
        Value = value;
    }

    public T GetValue<T>()
    {
        return (T) Value;
    }
}

public class SaveManager : Node
{
    Godot.Collections.Dictionary data;

    const string FilePath = "user://savegame.json";

    public bool Dirty = false;

    [Signal]
    public delegate void DataSaved();
    [Signal]
    public delegate void DataChanged(string key, SignalParameter oldValue, SignalParameter newValue);

    public override void _Ready()
    {
        Load();
    }

    public void Load()
    {
        var file = new File();
        if (!file.FileExists(FilePath)) {
            GenerateData();
            return;
        }
        file.Open(FilePath, File.ModeFlags.Read);
        data = (Godot.Collections.Dictionary) JSON.Parse(file.GetAsText()).Result;
    }

    void GenerateData()
    {
        data = new Godot.Collections.Dictionary()
        {
            {"version", "1.0"},
            {"counter", Convert.ToSingle(0)},
        };
        Save(false);
    }

    public void ResetData()
    {
        GenerateData();
    }

    public void Save(bool increaseCounter = true)
    {
        var file = new File();
        if (increaseCounter)
            Write<Single>("counter", Read<Single>("counter", 0) + 1, false);
        file.Open(FilePath, File.ModeFlags.Write);
        file.StoreString(ToString());
        file.Close();
        Dirty = false;
        EmitSignal(nameof(DataSaved));
    }

    public void Reset()
    {
        GenerateData();
    }

    public T Read<T>(string key, T defaultValue = default(T))
    {
        Godot.Collections.Dictionary dict = data;
        string[] keys = key.Split(".", true);
        if (!data.Contains(key)) return defaultValue;
        var result = data[key];
        return (T) result;
    }

    public bool Write<T>(string key, T value, bool save = true)
    {
        T oldValue = Read<T>(key);
        if (data.Contains(key))
            data.Remove(key);
        data.Add(key, value);
        if (save) Save();
        else Dirty = true;
        EmitSignal(nameof(DataChanged), key, new SignalParameter(oldValue), new SignalParameter(value));
        return true;
    }

    public override string ToString()
    {
        return JSON.Print(data, "  ");
    }
}
