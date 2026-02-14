using UnityEngine;

namespace Gameplay.Core
{
    public interface ITickable
    {
        void Tick(float deltaTime);
    }
}