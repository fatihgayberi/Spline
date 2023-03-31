using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using System.Linq;

#region "LIST<T> ile ilgili extension metodlar"
public static class ListExtensionMethods
{
    /// <summary>
    /// verilen listedeki elemanları karıştıp yenidien sıralar
    /// </summary>
    /// <param name="ts">liste</param>
    /// <typeparam name="T">listenin tipi</typeparam>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }


    /// <summary>
    /// Verilen listeyi clonelayıp return eder
    /// </summary>
    /// <param name="source"> clonelanacak liste </param>
    /// <typeparam name="T"> listenin tipi </typeparam>
    /// <returns></returns>
    public static List<T> GetClone<T>(this List<T> source)
    {
        return source.GetRange(0, source.Count);
    }
}
#endregion



#region "T[] Array ile ilgili extension metodlar"
public static class ArrayExtensionMethods
{
    /// <summary>
    /// verilen array elemanını indexini return eder eğer listede yoksa -1 döner
    /// </summary>
    /// <param name="array"> arama yapılacak array </param>
    /// <param name="item"> indexi blunacak item </param>
    /// <typeparam name="T"> arrayin tipi </typeparam>
    /// <returns></returns>
    public static int FindArrayIndex<T>(this T[] array, T item)
    {
        return Array.FindIndex(array, val => val.Equals(item));
    }


    /// <summary>
    /// verilen arraydeki elemanları karıştıp yenidien sıralar
    /// </summary>
    /// <param name="array"> clonelanacak array </param>
    /// <typeparam name="T"> arrayin tipi </typeparam>
    public static void Shuffle<T>(this T[] array)
    {
        System.Random rng = new System.Random();

        int n = array.Length;

        while (n > 1)
        {
            int k = rng.Next(n);
            n--;
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
#endregion



#region "Transformlar ile ilgili extension metodlar"
public static class TransformExtensionMethods
{
    /// <summary>
    /// Transform verilerini arguman olarak verilen transform ile aynı yapar
    /// </summary>
    /// <param name="fromTR"></param>
    /// <param name="toTR"></param>
    public static void TransformCopy(this Transform fromTR, Transform toTR)
    {
        if (fromTR == null)
        {
            return;
        }

        if (toTR == null)
        {
            return;
        }

        fromTR.position = toTR.position;
        fromTR.localScale = toTR.localScale;
        fromTR.rotation = toTR.rotation;
    }
}
#endregion



#region "GameObject ile ilgili extension metodlar"
public static class GameObjectExtensionMethods
{
    /// <summary>
    /// verilen gameobject null ise false return eder değil ise setactive durumunu degistirir
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="isSetActive"></param>
    /// <returns></returns>
    public static bool SetActiveNullCheck(this GameObject thisGameObject, bool isSetActive)
    {
        if (thisGameObject == null)
        {
            return false;
        }

        thisGameObject.SetActive(isSetActive);

        return true;
    }

    /// <summary>
    /// verilen gameobject null ise false return eder değil ise setactive durumunu degistirir
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="isSetActive"></param>
    /// <returns></returns>
    public static bool SetActiveNullCheck(this Transform thisTransform, bool isSetActive)
    {
        if (thisTransform == null)
        {
            return false;
        }

        if (thisTransform.gameObject == null)
        {
            return false;
        }

        thisTransform.gameObject.SetActive(isSetActive);

        return true;
    }
}
#endregion



#region "Class objeleri ile ilgili extension metodlar"
public static class ClassObjectExtensionMethods
{
    /// <summary>
    /// Class objesini clonelar
    /// </summary>
    /// <param name="obj"> clonelanacak obje </param>
    /// <typeparam name="T"> classın kendisi </typeparam>
    /// <returns></returns>
    public static T CreateDeepCopy<T>(this T obj)
    {
        if (obj == null)
        {
            return default;
        }

        using (var ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(ms);
        }
    }
}
#endregion



#region "LayerMask ile ilgili extension metodlar"
public static class LayerMaskExtensionMethods
{
    /// <summary>
    /// verilen layeri int degerine döndürür
    /// </summary>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public static int LayerMask2Int(LayerMask layerMask)
    {
        return (int)Mathf.Log(layerMask.value, 2);
    }
}
#endregion



#region "Float ile ilgili extension metodlar"
public static class FloatExtensionMethods
{
    /// <summary>
    /// flaot degerin a-b aralığından x-y aralığına remapler
    /// </summary>
    /// <param name="value"></param>
    /// <param name="from1"></param>
    /// <param name="to1"></param>
    /// <param name="from2"></param>
    /// <param name="to2"></param>
    /// <returns></returns>
    public static float FloatRemap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }


    /// <summary>
    /// verilen max min degerlerin yüzdelik degerini hesaplar
    /// </summary>
    /// <param name="percent">yüzde kaçını istiyorsun</param>
    /// <param name="minValue">min deger</param>
    /// <param name="maxValue">max deger</param>
    /// <returns></returns>
    public static float Percent2FloatValue(float percent, float minValue, float maxValue)
    {
        return minValue + ((maxValue - minValue) * (percent / 100));
    }


    /// <summary>
    /// max değerin yüzde kaç olduğunu return eder
    /// </summary>
    /// <param name="value"> değer </param>
    /// <param name="maxValue"> max değer </param>
    /// <returns></returns>
    public static float FloatValue2Percent(this float value, float maxValue)
    {
        if (value < 0)
        {
            return 0f;
        }

        return (value / maxValue) * 100;
    }
}
#endregion



#region "Color ile ilgili extension metodlar"
public static class ColorExtensionMethods
{
    /// <summary>
    /// verilen max min renklerine göre yüzdelik degerdeki rengi hesaplar
    /// </summary>
    /// <param name="percent"> yüzde kaçını istiyorsun </param>
    /// <param name="minColor"> max Color </param>
    /// <param name="maxColor"> min Color </param>
    /// <returns></returns>
    public static Color32 ColorPercentRate(float percent, Color32 minColor, Color32 maxColor)
    {
        float r = FloatExtensionMethods.Percent2FloatValue(percent, minColor.r, maxColor.r) / 256;
        float g = FloatExtensionMethods.Percent2FloatValue(percent, minColor.g, maxColor.g) / 256;
        float b = FloatExtensionMethods.Percent2FloatValue(percent, minColor.b, maxColor.b) / 256;
        float a = FloatExtensionMethods.Percent2FloatValue(percent, minColor.a, maxColor.a) / 256;

        return new Color(r, g, b, a);
    }
}
#endregion



#region "String ile ilgili extension metodlar"
public static class StringExtensionMethods
{
    /// <summary>
    /// string içerisindeki arguman olarka verilen karakteri siler
    /// </summary>
    /// <param name="strValue"></param>
    /// <param name="removeCharacter"></param>
    /// <returns></returns>
    public static string[] StringSplit(this string strValue, string removeCharacter)
    {
        return strValue.Split(new string[] { removeCharacter }, StringSplitOptions.None);
    }


    /// <summary>
    /// string içerisindeki verilen indexler arasındaki karakterleri siler
    /// </summary>
    /// <param name="source"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string Slice(this string source, int start, int end)
    {
        int sourceLength = source.Length;

        string newString = "";

        if (start < 0)
        {
            start = 0;
        }

        for (int i = start; i < end; i++)
        {
            if (i >= sourceLength)
            {
                return newString;
            }

            newString += source[i];
        }

        return newString;
    }


    /// <summary>
    /// Stringin dönüştürülecek tipi
    /// </summary>
    public enum StringCovenvertType
    {
        NONE,

        /// <summary> stringi kalinlastirir </summary>
        [Tooltip("stringi kalinlastirir")]
        Bold,

        /// <summary> stringi italiklestirir </summary>
        [Tooltip("stringi italiklestirirs")]
        Italic,

        /// <summary> stringin altini cizilir </summary>
        [Tooltip("stringin altini cizilir")]
        UnderLine
    }

    /// <summary>
    /// string donusumleri icin yardımcı struct
    /// </summary>
    private struct ConvertorStrings
    {
        public const string boldStart = "<b>";
        public const string boldEnd = "</b>";

        public const string italicStart = "<i>";
        public const string italicEnd = "</i>";

        public const string underLineStart = "<u>";
        public const string underLineEnd = "</u>";
    }

    /// <summary>
    /// stringi verilen tipe donusturur
    /// </summary>
    /// <param name="strMessage"> string </param>
    /// <param name="covenvertType"> donusturulecek tip </param>
    /// <returns></returns>
    public static string StringCovenverter(this string strMessage, StringCovenvertType covenvertType)
    {
        if (covenvertType == StringCovenvertType.NONE)
        {
            return strMessage;
        }

        if (string.IsNullOrWhiteSpace(strMessage))
        {
            return strMessage;
        }

        if (covenvertType == StringCovenvertType.Bold)
        {
            return ConvertorStrings.boldStart + strMessage + ConvertorStrings.boldEnd;
        }
        else if (covenvertType == StringCovenvertType.Italic)
        {
            return ConvertorStrings.italicStart + strMessage + ConvertorStrings.italicEnd;
        }
        else if (covenvertType == StringCovenvertType.UnderLine)
        {
            return ConvertorStrings.underLineStart + strMessage + ConvertorStrings.underLineEnd;
        }

        return strMessage;
    }
}
#endregion



#region "Birim dönüştürme ile ilgili extension metodlar"
public static class UnitConversionExtensionMethods
{
    /// <summary>
    /// verilen saniyeyi dakikaya çevirir
    /// </summary>
    /// <param name="minute"> dönüştürülecek dakika </param>
    /// <returns></returns>
    public static float Minute2Second(this float minute)
    {
        return minute * 60f;
    }


    /// <summary>
    /// verilen dakikayı saniyeye çevirip string formatında return eder
    /// </summary>
    /// <param name="minute"></param>
    /// <returns></returns>
    public static string Second2Minute(this float minute, string strSplitCharacter)
    {
        float residualValue = minute % 60;

        float minuteArea = (minute - residualValue) / 60;

        string strRightArea = "";
        string strLeftArea = "";

        if (minuteArea < 10)
        {
            strRightArea = "0" + minuteArea.ToString();
        }
        else
        {
            strRightArea = minuteArea.ToString();
        }

        if (residualValue < 10)
        {
            strLeftArea = "0" + ((int)residualValue).ToString();
        }
        else
        {
            strLeftArea = ((int)residualValue).ToString();
        }

        return strRightArea + strSplitCharacter + strLeftArea;
    }
}
#endregion



#region "Enum ile ilgili extension metodlar"
public static class EnumExtensionMethods
{
    /// <summary>
    /// Verilen enumun boyutunu return eder
    /// </summary>
    /// <typeparam name="T"> Enumun kendisi </typeparam>
    /// <returns></returns>
    public static int EnumCount<T>()
    {
        return Enum.GetValues(typeof(T)).Length;
    }
}
#endregion



#region "Toggle ile ilgili extension metodlar"
public static class ToggleExtensionMethods
{
    /// <summary>
    /// Toggle grup içindeki aktif toggle ı return eder
    /// </summary>
    /// <param name="toggleGroup"> Toggle Group </param>
    /// <returns></returns>
    public static Toggle GetActiveToggle(this ToggleGroup toggleGroup)
    {
        return toggleGroup.ActiveToggles().FirstOrDefault();
    }
}
#endregion

#region "Geomertik hesaplamalar ile ilgili metodlar"
public static class GeometricExtensionMethods
{
    /// <summary>
    /// XY düzlemi üzerindeverilen merkez noktasından radius içerisinde bir nokta üretir
    /// </summary>
    /// <param name="centerPos"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector3 CircleInRandomPosition(Vector3 centerPos, float radius)
    {
        float halfRadius = radius * Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f));
        float rand = UnityEngine.Random.Range(0f, 1f);

        float theta = rand * 2f * Mathf.PI;

        Vector3 randPos;

        randPos.x = centerPos.x + halfRadius * Mathf.Cos(theta);
        randPos.y = centerPos.y + halfRadius * Mathf.Sin(theta);
        randPos.z = 0f;

        return randPos;
    }


    /// <summary>
    /// iki noktanın orta noktasını bulur
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    public static Vector2 GetMidPoint(Vector2 point1, Vector2 point2)
    {
        return (point1 + point2) / 2;
    }


    /// <summary>
    /// noktanın bir noktaya göre simetriğini alır
    /// </summary>
    /// <param name="point"></param>
    /// <param name="centerPoint"></param>
    /// <returns></returns>
    public static Vector2 PointSimetric(this Vector2 point, Vector2 centerPoint)
    {
        return new Vector2(2 * centerPoint.x - point.x, 2 * centerPoint.y - point.y);
    }


    /// <summary>
    /// noktanın bir çember üzerinde angle kadar simetrigini return eder
    /// </summary>
    /// <param name="point"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector3 PointCircleSimetric(this Vector3 point, float angle)
    {
        return point * Mathf.Cos(angle);
    }
    /// <summary>
    /// noktanın bir çember üzerinde angle kadar simetrigini return eder
    /// </summary>
    /// <param name="point"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector2 PointCircleSimetric(this Vector2 point, float angle)
    {
        return point * Mathf.Cos(angle);
    }


    /// <summary>
    /// obje ile verilen pozisyon arasındaki açıyı hesaplar
    /// </summary>
    /// <param name="centerPos"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public static float TargetAngleDistance(this Vector3 centerPos, Vector3 targetPos)
    {
        float AngleRad = Mathf.Atan2(targetPos.y - centerPos.y, targetPos.x - centerPos.x);
        float angle = (180 / Mathf.PI) * AngleRad;

        return angle;
    }
}
#endregion