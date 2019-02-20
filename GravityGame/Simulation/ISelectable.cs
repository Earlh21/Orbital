using SFML.System;

namespace GravityGame
{
    public interface ISelectable
    {
        bool Contains(Vector2f point);
        bool IsSelected { get; set; }
        bool IsSelectable { get; }
    }
}