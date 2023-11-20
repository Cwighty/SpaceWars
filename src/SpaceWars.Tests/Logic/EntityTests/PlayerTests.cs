namespace SpaceWars.Tests.Logic.EntityTests;

public class PlayerTests
{
    [Fact]
    public void Player_WhenCreated_HasName()
    {
        var player = new Player("Player 1", new Ship());
        Assert.Equal("Player 1", player.Name);
    }

    [Fact]
    public void Can_Get_PlayerMessages()
    {
        var player = new Player("Player 1", new Ship());
        player.EnqueueMessage(new PlayerMessage(PlayerMessageType.RadarSweepResult, "{test result 1}"));
        player.EnqueueMessage(new PlayerMessage(PlayerMessageType.RadarSweepResult, "{test result 2}"));

        var messages = player.GetMessages();

        Assert.Equal(2, messages.Count());
        Assert.Equal("{test result 1}", messages.First().Message);
        Assert.Equal("RadarSweepResult", messages.First().Type.ToString());
        Assert.Equal("{test result 2}", messages.Last().Message);

        Assert.False(player.GetMessages().Any());
    }
}
