using Microsoft.Extensions.Options;

namespace SpaceWars.Web;
public class CompetitionTimer : SpaceWars.Logic.ITimer
{
    private Action tickAction;
    private Timer timer;

    public CompetitionTimer(IOptions<GameConfig> gameConfig)
    {
        Frequency = TimeSpan.FromMilliseconds(gameConfig.Value.TickFrequencyMilliseconds);
    }

    public TimeSpan Frequency { get; set; }

    public void RegisterAction(Action action)
    {
        tickAction = action;
    }

    public void Start()
    {
        if (tickAction == null)
        {
            throw new InvalidOperationException("Timer cannot start without a registered tick action.");
        }

        timer = new Timer(state => tickAction.Invoke(), null, 0, (int)Frequency.TotalMilliseconds);
    }

    public void Stop()
    {
        timer?.Dispose();
        timer = null;
    }
}