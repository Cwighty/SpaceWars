using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpaceWars.Logic;
using SpaceWars.Web.Types;

namespace SpaceWars.Web.Controllers;

[ApiController]
[Route("[controller]")]
public partial class GameController(ILogger<GameController> logger, Game game, IOptions<GameConfig> gameConfig) : ControllerBase
{
    [LoggerMessage(6, LogLevel.Warning, "Player {name} failed to join game. Too many players")] partial void LogTooManyPlayers(string name, ILogger<GameController> logger);

    [HttpGet("join")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JoinGameResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JoinGameResponse>> JoinGame(string name)
    {
        try
        {
            var joinResult = game.Join(name);
            return new JoinGameResponse(
                joinResult.Token.ToString(),
                joinResult.Location.ToApiLocation(),
                game.State.ToString(),
                game.BoardHeight,
                game.BoardWidth
            );
        }
        catch (TooManyPlayersException)
        {
            LogTooManyPlayers(name, logger);
            return Problem("Cannot join game, too many players.", statusCode: 400, title: "Too many players");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error joining game");
            return Problem("Error joining game", statusCode: 500, title: "Join Game Error");
        }
    }

    [HttpGet("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResult))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> StartGame(string password)
    {
        if (gameConfig.Value.Password == password)
        {
            game.Start();
            return Ok();
        }
        return BadRequest();
    }

    [HttpGet("state")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GameStateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<GameStateResponse> GameStateResponse()
    {
        return new GameStateResponse(game.State.ToString(), game.PlayerLocations.Select(l => l.ToApiLocation()));
    }

    [HttpGet("playermessages")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlayerMessageResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IEnumerable<PlayerMessageResponse>> GetPlayerMessagesAsync(string token)
    {
        var player = game.GetPlayerByToken(new PlayerToken(token));
        return player.GetMessages().Select(m => new PlayerMessageResponse(m.Type.ToString(), m.Message));
    }
}


public static class Extensions
{

    public static Types.Location ToApiLocation(this Logic.Location gameLocation) => new Types.Location(gameLocation.X, gameLocation.Y);

}