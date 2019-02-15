using SFML.System;

namespace GravityGame
{
    public interface ISelectable
    {
        Vector2f Position { get; }
        float Radius { get; }
        bool IsSelected { get; set; }
    }
}