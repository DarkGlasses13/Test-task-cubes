using System;

namespace CubeGame
{
    public interface IGameController : IDisposable
    {
        void Bind();
    }
}