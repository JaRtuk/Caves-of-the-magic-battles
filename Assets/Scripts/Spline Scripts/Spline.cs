// using UnityEngine;
// using System.Collections.Generic;

// public class Spline : MonoBehaviour
// {
//     public List<Transform> controlPoints = new List<Transform>();
//     public bool isLoop = true;

//     // Возвращает позицию на сплайне в интервале [0, 1]
//     public Vector3 GetPointAt(float t)
//     {
//         if (controlPoints.Count < 2) return Vector3.zero;

//         // Если сплайн зациклен, корректируем t и количество сегментов
//         int p0, p1, p2, p3;
//         int pointCount = isLoop ? controlPoints.Count : controlPoints.Count - 3;
//         float segmentFraction = 1f / pointCount;

//         // Находим индекс сегмента и локальный t для этого сегмента
//         int segmentIndex = Mathf.FloorToInt(t * pointCount);
//         float localT = (t - (segmentIndex * segmentFraction)) * pointCount;

//         // Получаем индексы контрольных точек для сплайна Catmull-Rom
//         if (isLoop)
//         {
//             p0 = segmentIndex % controlPoints.Count;
//             p1 = (segmentIndex + 1) % controlPoints.Count;
//             p2 = (segmentIndex + 2) % controlPoints.Count;
//             p3 = (segmentIndex + 3) % controlPoints.Count;
//         }
//         else
//         {
//             p0 = Mathf.Clamp(segmentIndex, 0, controlPoints.Count - 1);
//             p1 = Mathf.Clamp(segmentIndex + 1, 0, controlPoints.Count - 1);
//             p2 = Mathf.Clamp(segmentIndex + 2, 0, controlPoints.Count - 1);
//             p3 = Mathf.Clamp(segmentIndex + 3, 0, controlPoints.Count - 1);
//         }

//         // Вычисляем позицию по формуле Catmull-Rom
//         return GetCatmullRomPosition(localT, controlPoints[p0].position, controlPoints[p1].position, controlPoints[p2].position, controlPoints[p3].position);
//     }

//     // Формула Catmull-Rom сплайна
//     private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
//     {
//         Vector3 a = 0.5f * (2f * p1);
//         Vector3 b = 0.5f * (p2 - p0);
//         Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
//         Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

//         return a + (b * t) + (c * t * t) + (d * t * t * t);
//     }

//     // Отрисовка сплайна в редакторе для наглядности
//     private void OnDrawGizmos()
//     {
//         if (controlPoints.Count < 2) return;

//         Gizmos.color = Color.white;
//         int segments = 50;
//         for (int i = 0; i < segments; i++)
//         {
//             float t1 = i / (float)segments;
//             float t2 = (i + 1) / (float)segments;

//             Vector3 pos1 = GetPointAt(t1);
//             Vector3 pos2 = GetPointAt(t2);

//             Gizmos.DrawLine(pos1, pos2);
//         }
//     }
    
//     // Метод для получения направления (касательной) в точке t
//     public Vector3 GetDirectionAt(float t)
//     {
//         // Вычисляем направление как производную от позиции
//         // Используем небольшую дельту для численного дифференцирования
//         float delta = 0.001f;
//         float t1 = Mathf.Clamp01(t - delta);
//         float t2 = Mathf.Clamp01(t + delta);
        
//         Vector3 point1 = GetPointAt(t1);
//         Vector3 point2 = GetPointAt(t2);
        
//         return (point2 - point1).normalized;
//     }

//     // Альтернативный метод для более точного вычисления направления (аналитическая производная Catmull-Rom)
//     public Vector3 GetExactDirectionAt(float t)
//     {
//         if (controlPoints.Count < 2) return Vector3.forward;

//         int p0, p1, p2, p3;
//         int pointCount = isLoop ? controlPoints.Count : controlPoints.Count - 3;
//         float segmentFraction = 1f / pointCount;

//         int segmentIndex = Mathf.FloorToInt(t * pointCount);
//         float localT = (t - (segmentIndex * segmentFraction)) * pointCount;

//         if (isLoop)
//         {
//             p0 = segmentIndex % controlPoints.Count;
//             p1 = (segmentIndex + 1) % controlPoints.Count;
//             p2 = (segmentIndex + 2) % controlPoints.Count;
//             p3 = (segmentIndex + 3) % controlPoints.Count;
//         }
//         else
//         {
//             p0 = Mathf.Clamp(segmentIndex, 0, controlPoints.Count - 1);
//             p1 = Mathf.Clamp(segmentIndex + 1, 0, controlPoints.Count - 1);
//             p2 = Mathf.Clamp(segmentIndex + 2, 0, controlPoints.Count - 1);
//             p3 = Mathf.Clamp(segmentIndex + 3, 0, controlPoints.Count - 1);
//         }

//         return GetCatmullRomDerivative(localT, controlPoints[p0].position, controlPoints[p1].position, controlPoints[p2].position, controlPoints[p3].position).normalized;
//     }

//     // Производная Catmull-Rom сплайна (аналитическое вычисление)
//     private Vector3 GetCatmullRomDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
//     {
//         // Производная формулы: -0.5*p0 + 0.5*p2 + t*(2*p0 - 5*p1 + 4*p2 - p3) + 1.5*t^2*(-p0 + 3*p1 - 3*p2 + p3)
//         Vector3 a = 0.5f * (-p0 + p2);
//         Vector3 b = 2f * p0 - 5f * p1 + 4f * p2 - p3;
//         Vector3 c = 1.5f * (-p0 + 3f * p1 - 3f * p2 + p3);
        
//         return a + t * b + 1.5f * t * t * c;
//     }
// }


using UnityEngine;
using System.Collections.Generic;

public class Spline : MonoBehaviour
{
    public List<Transform> controlPoints = new List<Transform>();
    public bool isLoop = true;
    public Color gizmoColor = Color.white;

    public Vector3 GetPointAt(float t)
    {
        if (controlPoints.Count < 2) return transform.position;

        int p0, p1, p2, p3;
        int pointCount = isLoop ? controlPoints.Count : controlPoints.Count - 3;
        
        if (pointCount <= 0) return transform.position;
        
        float segmentFraction = 1f / pointCount;
        int segmentIndex = Mathf.FloorToInt(t * pointCount);
        float localT = (t - (segmentIndex * segmentFraction)) * pointCount;

        if (isLoop)
        {
            p0 = segmentIndex % controlPoints.Count;
            p1 = (segmentIndex + 1) % controlPoints.Count;
            p2 = (segmentIndex + 2) % controlPoints.Count;
            p3 = (segmentIndex + 3) % controlPoints.Count;
        }
        else
        {
            p0 = Mathf.Clamp(segmentIndex, 0, controlPoints.Count - 1);
            p1 = Mathf.Clamp(segmentIndex + 1, 0, controlPoints.Count - 1);
            p2 = Mathf.Clamp(segmentIndex + 2, 0, controlPoints.Count - 1);
            p3 = Mathf.Clamp(segmentIndex + 3, 0, controlPoints.Count - 1);
        }

        return GetCatmullRomPosition(localT, controlPoints[p0].position, controlPoints[p1].position, 
                                    controlPoints[p2].position, controlPoints[p3].position);
    }

    public Vector3 GetDirectionAt(float t)
    {
        float delta = 0.001f;
        float t1 = Mathf.Clamp01(t - delta);
        float t2 = Mathf.Clamp01(t + delta);
        
        Vector3 point1 = GetPointAt(t1);
        Vector3 point2 = GetPointAt(t2);
        
        return (point2 - point1).normalized;
    }

    public Vector3 GetExactDirectionAt(float t)
    {
        if (controlPoints.Count < 2) return Vector3.forward;

        int p0, p1, p2, p3;
        int pointCount = isLoop ? controlPoints.Count : controlPoints.Count - 3;
        
        if (pointCount <= 0) return Vector3.forward;
        
        float segmentFraction = 1f / pointCount;
        int segmentIndex = Mathf.FloorToInt(t * pointCount);
        float localT = (t - (segmentIndex * segmentFraction)) * pointCount;

        if (isLoop)
        {
            p0 = segmentIndex % controlPoints.Count;
            p1 = (segmentIndex + 1) % controlPoints.Count;
            p2 = (segmentIndex + 2) % controlPoints.Count;
            p3 = (segmentIndex + 3) % controlPoints.Count;
        }
        else
        {
            p0 = Mathf.Clamp(segmentIndex, 0, controlPoints.Count - 1);
            p1 = Mathf.Clamp(segmentIndex + 1, 0, controlPoints.Count - 1);
            p2 = Mathf.Clamp(segmentIndex + 2, 0, controlPoints.Count - 1);
            p3 = Mathf.Clamp(segmentIndex + 3, 0, controlPoints.Count - 1);
        }

        return GetCatmullRomDerivative(localT, controlPoints[p0].position, controlPoints[p1].position, 
                                      controlPoints[p2].position, controlPoints[p3].position).normalized;
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 0.5f * (2f * p1);
        Vector3 b = 0.5f * (p2 - p0);
        Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
        Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

        return a + (b * t) + (c * t * t) + (d * t * t * t);
    }

    private Vector3 GetCatmullRomDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 0.5f * (-p0 + p2);
        Vector3 b = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 c = 1.5f * (-p0 + 3f * p1 - 3f * p2 + p3);
        
        return a + t * b + 1.5f * t * t * c;
    }

    private void OnDrawGizmos()
    {
        if (controlPoints.Count < 2) return;

        Gizmos.color = gizmoColor;
        int segments = 50;
        
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;

            Vector3 pos1 = GetPointAt(t1);
            Vector3 pos2 = GetPointAt(t2);

            Gizmos.DrawLine(pos1, pos2);
        }

        // Draw control points
        Gizmos.color = Color.red;
        foreach (Transform point in controlPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.1f);
        }
    }
}