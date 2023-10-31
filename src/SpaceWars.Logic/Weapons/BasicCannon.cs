﻿using System.Numerics;

namespace SpaceWars.Logic.Weapons;

public class BasicCannon : Weapon
{
    public BasicCannon() : base("Basic Cannon")
    {
        Ranges = [
            new WeaponRange(200, 100),
        ];
        Power = 50;
        Cost = 200;
        ShotCost = 25;
        ChargeTurns = 1;
    }

    public override void Fire(Player player, GameMap map)
    {
        var maxWeaponRange = Ranges.Last().Distance;
        var playersInRange = map.GetPlayersInRange(player, maxWeaponRange);
        if (TryHit(player, playersInRange, out var result))
        {
            var (hitPlayer, distance) = result.Value;
            hitPlayer.Ship.Health -= Power;
        }
    }

    public bool TryHit(Player player, IEnumerable<Player> playersInRange, out (Player hitPlayer, int distance)? result)
    {
        Vector2 playerLocation = new Vector2(player.Ship.Location.X, player.Ship.Location.Y);
        var headingRadians = MathF.PI / 180 * (360 - player.Ship.Heading);
        var playerHeading = Vector2.Transform(new Vector2(0, 1), Matrix3x2.CreateRotation(headingRadians));
        playerHeading = Vector2.Normalize(playerHeading);


        foreach (var otherPlayer in playersInRange)
        {
            var targetPosition = otherPlayer.Ship.Location;
            var boxSize = 1;
            // Calculate the four corners of the bounding box around the target.
            Vector2 topRight = new Vector2(targetPosition.X + boxSize, targetPosition.Y + boxSize);
            Vector2 bottomLeft = new Vector2(targetPosition.X - boxSize, targetPosition.Y - boxSize);

            // Check if the ship's forward vector intersects with the bounding box.
            bool inFront = RayIntersectsAABB(bottomLeft, topRight, playerLocation, playerHeading);

            if (inFront)
            {
                var otherLocation = new Vector2(otherPlayer.Ship.Location.X, otherPlayer.Ship.Location.Y);
                var distance = (int)Vector2.Distance(playerLocation, otherLocation);
                result = (otherPlayer, distance);
                return true;
            }
        }

        result = null;
        return false;
    }



    private bool RayIntersectsAABB(Vector2 boxMin, Vector2 boxMax, Vector2 rayOrigin, Vector2 rayDirection)
    {
        var tMin = (boxMin.X - rayOrigin.X) / rayDirection.X;
        var tMax = (boxMax.X - rayOrigin.X) / rayDirection.X;

        if (tMin > tMax)
        {
            (tMin, tMax) = (tMax, tMin);
        }

        var tyMin = (boxMin.Y - rayOrigin.Y) / rayDirection.Y;
        var tyMax = (boxMax.Y - rayOrigin.Y) / rayDirection.Y;

        if (tyMin > tyMax)
        {
            (tyMin, tyMax) = (tyMax, tyMin);
        }

        if (tMin > tyMax || tyMin > tMax)
        {
            return false;
        }

        return true;
    }
}
