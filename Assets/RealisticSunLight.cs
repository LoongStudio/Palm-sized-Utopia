using UnityEngine;

public class RealisticSunLight : MonoBehaviour
{
    public Light sunLight;
    public float latitude = 30f; // 北纬30度，可根据需求动态赋值

    public WeatherType currentWeather = WeatherType.Sunny;
    
    public static float Deg2Rad(float degrees)
    {
        return degrees * Mathf.PI / 180f;
    }

    public static float Rad2Deg(float radians)
    {
        return radians * 180f / Mathf.PI;
    }

    // 天气光照因子表
    private float GetWeatherFactor()
    {
        switch (currentWeather)
        {
            case WeatherType.Sunny: return 1f;
            case WeatherType.Cloudy: return 0.7f;
            case WeatherType.Overcast: return 0.5f;
            case WeatherType.Rain: return 0.4f;
            case WeatherType.Thunderstorm: return 0.2f;
            case WeatherType.Snow: return 0.3f;
            default: return 1f;
        }
    }
    
    private float hourLerpT = 0f;      // 当前小时内的插值进度 (0 ~ 1)
    private int lastHour = -1;
    private float elev1, azi1, elev2, azi2;
    void FixedUpdate()
    {
        if (sunLight == null) return;

        GameTime gameTime = TimeManager.Instance.CurrentTime;
        int currentHour = gameTime.hour;

        // 初始化或进入新的一小时
        if (lastHour != currentHour)
        {
            lastHour = currentHour;
            hourLerpT = 0f;
            // 计算插值太阳角度
            CalculateSolarPosition(gameTime.year + 2025, gameTime.month, gameTime.day, currentHour, latitude, out elev1, out azi1);
            CalculateSolarPosition(gameTime.year + 2025, gameTime.month, gameTime.day, currentHour + 1, latitude, out elev2, out azi2);

        }
        // 累加小时内进度
        float simulatedSecondsThisFrame = Time.fixedDeltaTime * TimeManager.Instance.currentTimeScale;
        hourLerpT += simulatedSecondsThisFrame / 60;
        hourLerpT = Mathf.Clamp01(hourLerpT); // 防止溢出
        
        float elevation = Mathf.Lerp(elev1, elev2, hourLerpT);
        float azimuth = Mathf.LerpAngle(azi1, azi2, hourLerpT);
        // Debug.Log(hourLerpT + " " + elev1 + " " + azi1 + " " + elev2 + " " + azi2);
        float weatherFactor = GetWeatherFactor();

        // 计算光强
        float intensity = 0f;

        if (elevation > 0)
        {
            intensity = Mathf.Sin(elevation * Mathf.Deg2Rad) * weatherFactor;
        }
        else if (elevation > -18f)
        {
            // 夕阳至暮光阶段，强度从0渐变到某个夜光强度（比如0.1）
            float tNight = Mathf.InverseLerp(-18f, 0f, elevation);
            intensity = Mathf.Lerp(0f, 0.1f, tNight) * weatherFactor;
        }
        else
        {
            intensity = 0f; // 完全黑暗
        }

        sunLight.intensity = Mathf.Clamp01(intensity);

        // 计算光方向（太阳光或月光方向）
        Vector3 dir = Quaternion.Euler(90 - elevation, azimuth, 0) * Vector3.forward;
        sunLight.transform.rotation = Quaternion.LookRotation(dir);

        // 颜色渐变
        Color noonColor = Color.white;               // 正午白光
        Color dawnColor = new Color(1f, 0.6f, 0.4f); // 早晚暖色
        Color nightColor = new Color(0.2f, 0.3f, 0.6f); // 夜晚蓝紫色

        Color finalColor;

        if (elevation > 0)
        {
            float t = Mathf.InverseLerp(0f, 60f, elevation); // 0~60度内插值
            finalColor = Color.Lerp(dawnColor, noonColor, t);
        }
        else if (elevation > -18f)
        {
            // 夜晚渐变色：0(黄昏) 到 1(夜晚蓝色)
            float tNight = Mathf.InverseLerp(0f, -18f, elevation);
            // tNight 反转（因为 elevation 是负值），用来从暖色到夜色渐变
            finalColor = Color.Lerp(dawnColor, nightColor, 1 - tNight);
        }
        else
        {
            finalColor = Color.black; // 完全黑暗
        }

        sunLight.color = finalColor;
    }

    // 输出：太阳高度角（solarElevationAngle），太阳方位角（solarAzimuthAngle）

    public void CalculateSolarPosition(
        int year, int month, int day, int hour,
        float latitude,
        out float solarElevationAngle,
        out float solarAzimuthAngle)
    {
        // 1. 计算“儒略日”（Julian Day）
        int A = Mathf.FloorToInt(year / 100f);
        int B = 2 - A + Mathf.FloorToInt(A / 4f);

        float JD = Mathf.FloorToInt(365.25f * (year + 4716)) +
                   Mathf.FloorToInt(30.6001f * (month + 1)) +
                   day + B - 1524.5f + hour / 24f;

        // 2. 计算儒略世纪数（Julian Century）
        float JC = (JD - 2451545f) / 36525f;

        // 3. 太阳几何平均黄经（degrees）
        float geomMeanLongSun = (280.46646f + JC * (36000.76983f + JC * 0.0003032f)) % 360f;

        // 4. 太阳平均异常角
        float geomMeanAnomSun = 357.52911f + JC * (35999.05029f - 0.0001537f * JC);

        // 5. 太阳光行差修正
        float sunEqOfCenter = (Mathf.Sin(Deg2Rad(geomMeanAnomSun)) * (1.914602f - JC * (0.004817f + 0.000014f * JC)) +
                               Mathf.Sin(Deg2Rad(2 * geomMeanAnomSun)) * (0.019993f - 0.000101f * JC) +
                               Mathf.Sin(Deg2Rad(3 * geomMeanAnomSun)) * 0.000289f);

        // 6. 太阳真实黄经
        float sunTrueLong = geomMeanLongSun + sunEqOfCenter;

        // 7. 太阳视黄经（带光行差修正）
        float omega = 125.04f - 1934.136f * JC;
        float sunAppLong = sunTrueLong - 0.00569f - 0.00478f * Mathf.Sin(Deg2Rad(omega));

        // 8. 太阳黄赤交角
        float meanObliqEcliptic = 23f + (26f + ((21.448f - JC * (46.815f + JC * (0.00059f - JC * 0.001813f))) / 60f)) / 60f;
        float obliqCorr = meanObliqEcliptic + 0.00256f * Mathf.Cos(Deg2Rad(omega));

        // 9. 太阳赤纬
        float sunDeclination = Rad2Deg(
            Mathf.Asin(
                Mathf.Sin(Deg2Rad(obliqCorr)) * Mathf.Sin(Deg2Rad(sunAppLong))
            )
        );

        // 10. 计算太阳时角(Hour Angle)
        float varY = Mathf.Tan(Deg2Rad(obliqCorr / 2)) * Mathf.Tan(Deg2Rad(obliqCorr / 2));
        float eqOfTime = 4f * Rad2Deg(varY * Mathf.Sin(2f * Deg2Rad(geomMeanLongSun)) - 2f * Mathf.Sin(Deg2Rad(geomMeanAnomSun)) + 4f * varY * Mathf.Sin(Deg2Rad(geomMeanAnomSun)) * Mathf.Cos(2f * Deg2Rad(geomMeanLongSun)) - 0.5f * varY * varY * Mathf.Sin(4f * Deg2Rad(geomMeanLongSun)) - 1.25f * Mathf.Sin(2f * Deg2Rad(geomMeanAnomSun)));

        // 11. 计算真太阳时（True Solar Time，单位分钟）
        float trueSolarTime = (hour * 60f + eqOfTime + 4f * 0 /* 经度补偿为0假设在经线中心 */) % 1440;

        // 12. 计算太阳时角
        float hourAngle = (trueSolarTime / 4f < 0) ? trueSolarTime / 4f + 180f : trueSolarTime / 4f - 180f;

        // 13. 太阳高度角
        solarElevationAngle = Rad2Deg(
            Mathf.Asin(
                Mathf.Sin(Deg2Rad(latitude)) * Mathf.Sin(Deg2Rad(sunDeclination)) +
                Mathf.Cos(Deg2Rad(latitude)) * Mathf.Cos(Deg2Rad(sunDeclination)) * Mathf.Cos(Deg2Rad(hourAngle))
            )
        );

        // 14. 太阳方位角
        float azimuthRad = Mathf.Acos((Mathf.Sin(Deg2Rad(sunDeclination)) - Mathf.Sin(Deg2Rad(solarElevationAngle)) * Mathf.Sin(Deg2Rad(latitude))) / (Mathf.Cos(Deg2Rad(solarElevationAngle)) * Mathf.Cos(Deg2Rad(latitude))));
        solarAzimuthAngle = (hourAngle > 0) ? Rad2Deg(azimuthRad) + 180f : 540f - Rad2Deg(azimuthRad);
        solarAzimuthAngle %= 360f;
    }
    
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
