using System;
using UnityEngine;

public class CelestialLightRenderer : MonoBehaviour
{
    [Header("Light Sources")]
    public Light sunLight;
    public Light moonLight;

    [Header("Geographic Settings")]
    public float latitude = 30f;

    [Header("Simulation")]
    public float moonMaxIntensity = 0.3f;
    public float sunTwilightIntensity = 0.1f;

    private float hourLerpT = 0f;
    private int lastHour = -1;
    private float elev1, azi1, elev2, azi2;

    void FixedUpdate()
    {
        GameTime gameTime = TimeManager.Instance.CurrentTime;
        float weatherFactor = GetWeatherFactor();

        UpdateSunLight(gameTime, weatherFactor);
        UpdateMoonLight(gameTime, weatherFactor);
    }

    public static float Deg2Rad(float degrees)
    {
        return degrees * Mathf.PI / 180f;
    }

    public static float Rad2Deg(float radians)
    {
        return radians * 180f / Mathf.PI;
    }
    void UpdateSunLight(GameTime gameTime, float weatherFactor)
    {
        int currentHour = gameTime.hour;

        if (lastHour != currentHour)
        {
            lastHour = currentHour;
            hourLerpT = 0f;

            CalculateSimpleSolarPosition(gameTime.year + 2025, gameTime.month, gameTime.day, currentHour, latitude, out elev1, out azi1);
            CalculateSimpleSolarPosition(gameTime.year + 2025, gameTime.month, gameTime.day, currentHour + 1, latitude, out elev2, out azi2);
        }

        float simulatedSeconds = Time.fixedDeltaTime * TimeManager.Instance.currentTimeScale;
        hourLerpT += simulatedSeconds / 60f;
        hourLerpT = Mathf.Clamp01(hourLerpT);

        float elevation = Mathf.Lerp(elev1, elev2, hourLerpT);
        float azimuth = Mathf.LerpAngle(azi1, azi2, hourLerpT);

        float intensity = 0f;
        if (elevation > 0f)
            intensity = Mathf.Sin(elevation * Mathf.Deg2Rad) * weatherFactor;
        else if (elevation > -18f)
            intensity = Mathf.Lerp(0f, sunTwilightIntensity, Mathf.InverseLerp(-18f, 0f, elevation)) * weatherFactor;

        sunLight.intensity = Mathf.Clamp01(intensity);

        Vector3 dir = Quaternion.Euler(90 - elevation, azimuth, 0) * Vector3.forward;
        sunLight.transform.rotation = Quaternion.LookRotation(dir);

        Color finalColor = Color.black;
        if (elevation > 0)
            finalColor = Color.Lerp(new Color(1f, 0.6f, 0.4f), Color.white, Mathf.InverseLerp(0, 60f, elevation));
        else if (elevation > -18f)
            finalColor = Color.Lerp(new Color(1f, 0.6f, 0.4f), new Color(0.2f, 0.3f, 0.6f), 1 - Mathf.InverseLerp(0f, -18f, elevation));

        sunLight.color = finalColor;
    }

    void UpdateMoonLight(GameTime gameTime, float weatherFactor)
    {
        float moonElev, moonAzim, moonPhase;
        CalculateMoonPositionAndPhase(gameTime.year + 2025, gameTime.month, gameTime.day, gameTime.hour, latitude, out moonElev, out moonAzim, out moonPhase);

        if (moonElev < -10f)
        {
            moonLight.enabled = false;
            return;
        }

        moonLight.enabled = true;

        float elevationFactor = Mathf.Clamp01(Mathf.Sin(moonElev * Mathf.Deg2Rad));
        float phaseFactor = Mathf.Clamp01(1f - Mathf.Cos(moonPhase * Mathf.Deg2Rad));
        float moonIntensity = elevationFactor * weatherFactor * phaseFactor * moonMaxIntensity;
        moonLight.intensity = moonIntensity;

        Vector3 moonDir = Quaternion.Euler(90 - moonElev, moonAzim, 0) * Vector3.forward;
        moonLight.transform.rotation = Quaternion.LookRotation(moonDir);

        Color crescent = new Color(0.3f, 0.35f, 0.45f);
        Color fullMoon = new Color(0.75f, 0.8f, 1.0f);
        moonLight.color = Color.Lerp(crescent, fullMoon, phaseFactor);
    }

    float GetWeatherFactor()
    {
        // 可扩展：接入天气系统（晴天=1，阴天=0.6，雨=0.3）
        return 1.0f;
    }
    public void CalculateSimpleSolarPosition(
        int year, int month, int day, int hour,
        float latitude,
        out float solarElevationAngle,
        out float solarAzimuthAngle)
    {
        // 简单季节判断，决定白昼长度偏移量
        float declinationOffset = month switch
        {
            3 or 4 or 5 => 10f,   // 春：适中
            6 or 7 or 8 => 23.5f, // 夏：白昼长
            9 or 10 or 11 => -10f, // 秋：适中
            12 or 1 or 2 => -23.5f, // 冬：白昼短
            _ => 0f
        };

        // 日出在 6 点，日落在 18 点，基础白昼 12 小时
        // 随季节偏移 ±2 小时（23.5/6 ≈ 4）
        float seasonalShift = declinationOffset / 6f;
        float sunrise = 6f - seasonalShift;
        float sunset = 18f + seasonalShift;

        float dayProgress = Mathf.InverseLerp(sunrise, sunset, hour);
        dayProgress = Mathf.Clamp01(dayProgress);

        // 正午太阳角度接近 (90 - 纬度 + declinationOffset)
        float maxElevation = 90f - Mathf.Abs(latitude - declinationOffset);

        // 使用正弦插值模拟一天内高度变化（0 到 max）
        solarElevationAngle = Mathf.Sin(dayProgress * Mathf.PI) * maxElevation;

        // 太阳方位角：从东（90°）升起，西（270°）落下
        solarAzimuthAngle = Mathf.Lerp(90f, 270f, dayProgress);
    }

    public void CalculateSolarPosition(
        int year, int month, int day, int hour,
        float latitude,
        out float solarElevationAngle,
        out float solarAzimuthAngle)
    {
        // 0. 加上经度补偿，单位为分钟（每度差值 4 分钟）
        float longitude = 120f; // 例如中国东八区大约120°
        float standardMeridian = 15f * 8; // 东八区为 120°
        float timeCorrection = 4f * (longitude - standardMeridian);

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
        // float trueSolarTime = (hour * 60f + eqOfTime + 4f * 0 /* 经度补偿为0假设在经线中心 */) % 1440;
        float trueSolarTime = (hour * 60f + eqOfTime + timeCorrection) % 1440f;

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

    void CalculateMoonSimplePositionAndPhase(int year, int month, int day, int hour, float latitude, out float elevation, out float azimuth, out float phase)
    {
        float jd = 367 * year - (int)((7 * (year + (month + 9) / 12)) / 4) + (int)((275 * month) / 9) + day + 1721013.5f + hour / 24f;
        float M = Mathf.Repeat(134.963f + 13.064993f * (jd - 2451545.0f), 360f);
        phase = M;

        float hourAngle = ((hour / 24f) * 360f) - 180f;
        elevation = 30f * Mathf.Sin(hourAngle * Mathf.Deg2Rad);
        azimuth = Mathf.Repeat(hourAngle + 180f, 360f);
    }
    
    void CalculateMoonPositionAndPhase(int year, int month, int day, int hour, float latitude,
        out float elevation, out float azimuth, out float phase)
    {
        // ==== 1. 计算简化月相 ====
        // 模拟从新月 -> 满月 -> 新月，周期 29.5 天
        DateTime date = new DateTime(year, month, day);
        DateTime knownNewMoon = new DateTime(2000, 1, 6); // 一个已知新月日期（任意）
        double daysSinceNewMoon = (date - knownNewMoon).TotalDays;
        double synodicMonth = 29.530588; // 月相周期
        double moonAge = daysSinceNewMoon % synodicMonth;
        phase = (float)(moonAge / synodicMonth); // 0 = 新月，0.5 = 满月

        // ==== 2. 月亮升起时间比太阳晚约 50 分钟/天 ====
        // 模拟月亮在子夜时分（24:00）位于最高点，并每天推迟约 50 分钟
        float moonHourOffset = (float)(moonAge * 0.85); // 约等于天数 * 50分钟 / 60分钟
        float moonHour = (hour - moonHourOffset + 24f) % 24f;

        // ==== 3. 计算高度与方位 ====
        // 使用 sin 曲线模拟：正午最高，清晨和晚上低或不可见
        float moonElevationMax = 90f - Mathf.Abs(latitude); // 最大仰角
        float t = Mathf.InverseLerp(0f, 24f, moonHour);
        elevation = Mathf.Sin(t * Mathf.PI) * moonElevationMax;

        // 方位角模拟：东升西落
        azimuth = Mathf.Lerp(90f, 270f, t);
    }

}
