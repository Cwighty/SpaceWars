namespace SpaceWars.Logic;

public interface ITimer
{
    void RegisterAction(Action action);
    TimeSpan Frequency { get; set; }
    void Start();
    void Stop();
}
