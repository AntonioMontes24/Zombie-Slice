using System;

public interface IObjective
{
    bool isComplete { get; }

    void Register(Action<string> callback);

    void Start();
}
