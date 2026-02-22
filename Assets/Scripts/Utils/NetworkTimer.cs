using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    public class NetworkTimer
    {
        float timer;
        public float MinTimeBetweenTicks { get; }
        public int CurrentTick { get; private set; }
    
        public NetworkTimer()
        {
            MinTimeBetweenTicks = 1f / NetworkManager.Singleton.NetworkTickSystem.TickRate;
        }

        public void Update(float deltaTime)
        {
            timer += deltaTime;
        }

        public bool ShouldTick()
        {
            if (timer >= MinTimeBetweenTicks)
            {
                timer -= MinTimeBetweenTicks;
                CurrentTick++;
                return true;
            }

            return false;
        }
    }
}
