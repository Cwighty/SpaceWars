using Microsoft.Extensions.Options;
using SpaceWars.Web;

namespace SpaceWars.Tests.Logic.GameplayTests;

public class CompetitionTimerTests
{
    [Fact]
    public void TimerShouldNotStartWithoutRegisteredTickAction()
    {
        IOptions<GameConfig> options = Options.Create(new GameConfig { TickFrequencyMilliseconds = 333 });
        SpaceWars.Logic.ITimer timer = new CompetitionTimer(options);

        Assert.Throws<InvalidOperationException>(() => timer.Start());
    }
}

