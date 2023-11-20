using SpaceWars.Logic.Actions;
using System.Text;

namespace SpaceWars.Logic;

public class Player : IEquatable<Player?>
{
    private Queue<GamePlayAction> actions;
    private Queue<PlayerMessage> playerMessages; 
    public Player(string name, Ship ship)
    {
        Name = name;
        Ship = ship;
        Token = PlayerToken.Generate();
        actions = new Queue<GamePlayAction>();
        playerMessages = new Queue<PlayerMessage>();
    }

    public string Name { get; }

    //token (is this only a concern on the api?)
    //hud url (is this only a concern on the api?)

    public Ship Ship { get; }
    public PlayerToken Token { get; }

    public void EnqueueAction(GamePlayAction action)
    {
        actions.Enqueue(action);
    }

    public GamePlayAction? DequeueAction()
    {
        if (actions.Any())
            return actions.Dequeue();
        return null;
    }

    public void EnqueueMessage(PlayerMessage message)
    {
        playerMessages.Enqueue(message);
    }

    public IEnumerable<PlayerMessage> GetMessages()
    {
        var messages = new List<PlayerMessage>();
        while (playerMessages.Any())
        {
            messages.Add(playerMessages.Dequeue());
        }
        return messages;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Player);
    }

    public bool Equals(Player? other)
    {
        return other is not null &&
               EqualityComparer<Queue<GamePlayAction>>.Default.Equals(actions, other.actions) &&
               Name == other.Name &&
               EqualityComparer<Ship>.Default.Equals(Ship, other.Ship);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(actions, Name, Ship);
    }

    public static bool operator ==(Player? left, Player? right)
    {
        return EqualityComparer<Player>.Default.Equals(left, right);
    }

    public static bool operator !=(Player? left, Player? right)
    {
        return !(left == right);
    }
}

public record PlayerMessage (PlayerMessageType Type, string Message);

public enum PlayerMessageType
{
    RadarSweepResult,
}

public class PlayerToken : IEquatable<PlayerToken?>
{
    public string Value { get; init; }

    public PlayerToken(string value)
    {
        Value = value;
    }

    public static PlayerToken Generate()
    {
        return new PlayerToken(IdGenerator.GetNextId());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PlayerToken);
    }

    public bool Equals(PlayerToken? other)
    {
        return other is not null &&
               Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(PlayerToken? left, PlayerToken? right)
    {
        return EqualityComparer<PlayerToken>.Default.Equals(left, right);
    }

    public static bool operator !=(PlayerToken? left, PlayerToken? right)
    {
        return !(left == right);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Originally from https://github.com/dotnet/aspnetcore/blob/main/src/Servers/Kestrel/shared/CorrelationIdGenerator.cs
/// The .NET Foundation licenses this file to you under the MIT license.
/// </summary>
internal static class IdGenerator
{
    // Base32 encoding - in ascii sort order for easy text based sorting
    private static readonly char[] chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

    // Seed the _lastConnectionId for this application instance with
    // the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001
    // for a roughly increasing _lastId over restarts
    private static long _lastId = DateTime.UtcNow.Ticks;

    public static string GetNextId() => GenerateRandomId();// GenerateId(Interlocked.Increment(ref _lastId));


    private static string GenerateRandomId(int length = 16)
    {
        StringBuilder id = new();
        for (int i = 0; i < length; i++)
        {
            id.Append(chars[Random.Shared.Next(chars.Length)]);
        }
        return id.ToString();
    }

    private static string GenerateId(long id)
    {
        return string.Create(13, id, (buffer, value) =>
        {
            char[] encode32Chars = chars;

            buffer[12] = encode32Chars[value & 31];
            buffer[11] = encode32Chars[(value >> 5) & 31];
            buffer[10] = encode32Chars[(value >> 10) & 31];
            buffer[9] = encode32Chars[(value >> 15) & 31];
            buffer[8] = encode32Chars[(value >> 20) & 31];
            buffer[7] = encode32Chars[(value >> 25) & 31];
            buffer[6] = encode32Chars[(value >> 30) & 31];
            buffer[5] = encode32Chars[(value >> 35) & 31];
            buffer[4] = encode32Chars[(value >> 40) & 31];
            buffer[3] = encode32Chars[(value >> 45) & 31];
            buffer[2] = encode32Chars[(value >> 50) & 31];
            buffer[1] = encode32Chars[(value >> 55) & 31];
            buffer[0] = encode32Chars[(value >> 60) & 31];
        });
    }
}