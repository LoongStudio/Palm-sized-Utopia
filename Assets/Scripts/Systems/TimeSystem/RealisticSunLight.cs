using UnityEngine;

public class RealisticSunLight : MonoBehaviour
{
    public CelestialLightRenderer celestialLightRenderer;
    
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Overcast,
        Rain,
        Thunderstorm,
        Snow
    }
    
}
