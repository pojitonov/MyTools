using System;

namespace Example3
{
    public interface IInput
    {
        event Action<MovementDirection> InputEvent;
    }
}