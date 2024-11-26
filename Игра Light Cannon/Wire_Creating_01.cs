using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire_Creating_01 : MonoBehaviour
{
    //Если точка находится в локальных координатах 0,0,0 - то она игнорируется

    public GameObject[] Points = new GameObject[20];
    //public Vector3[] Points2 = new Vector3[3];

    [Tooltip("Фиксировать ли эту точку? [Первая и последняя точка - всегда isKinematic]")] public bool[] Points_Is_Kinematic = new bool[20];

    public float distance = 0.77f; // желаемое расстояние // Настраиватся в инспекторе. Идеальное расстояние, полученное опытным путём - это 0.0077
    private float sizeGizmos = 0.1f;

    public GameObject Prefab_Capsule;
    private GameObject New_Capsule;

    public GameObject Prefab_Sphere; // Используется для ключевых звеньев

    //public bool notOk = false;
    private Vector3 zero_vec = Vector3.zero;

    Vector3 A = Vector3.zero;
    Vector3 B = Vector3.zero;
    Vector3 D = Vector3.zero;

    public GameObject prefGO_Segment = null; // Пердыдущее звено. Нужно для создание связей в цепочке

    [Tooltip("Отрисовывать ли точки, находящиеся в нулевых координатах?")] public bool isEnebleDrowZeroPoints = true;
    [Tooltip("Ограничитель использования точек. Если равен нулю, то используются все точки")] public int Limitter = 0;

    public Material OffWire;
    public Material OnWire;
    public Material StaticWire;

    public bool isEnebleDraw = true;

    List<GameObject> CreatedObjects = new List<GameObject>();

    //public Color OnWire_C;
    //public Color OffWire_C;

    void OnDrawGizmos()
    {
        // Отрисовываю положение точек

        if (isEnebleDraw == true)
        {
            Gizmos.color = Color.blue;
            //Gizmos.color = Color.red;

            for (int i = 0; i < Points.Length; i++)
            {
                if ((Points[i] != null) && (Points[i].transform.localPosition != zero_vec))
                // Если точка существует, и не находится в локальных координатах 0,0,0
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);
                }
                else if ((Points[i].transform.localPosition == zero_vec) && (isEnebleDrowZeroPoints == true))
                {
                    // На всякий случай, отрисовываю точки находящиеся в нулевых координатах, если они есть

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);
                }
            }
        }
    }

    bool isLoop = false;
    void Start()
    {
        //OnWire_C = OnWire.color;
        //OffWire_C = OffWire.color;

        //print("OnWire_C = " + OnWire_C);
        //print("OffWire_C = " + OffWire_C);

        //Gizmos.color = Color.red;

        if (Limitter == 0) Limitter = Points.Length;

        for (int i = 0; i < Points.Length; i++)
        {
            if ((Points[i] != null) && (Points[i].transform.localPosition != zero_vec) && (i < Limitter))
            // Если точка существует, и не находится в локальных координатах 0,0,0
            {
                //print("Локальные координаты точки " + i + " = " + Points[i].transform.localPosition);

                //Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);

                if (i > 0)
                {
                    A = Points[i - 1].transform.position;
                    B = Points[i].transform.position;

                    int whileStoper = 0;

                    while (true)
                    {
                        if (whileStoper >= 20000) break;

                        if ((distance * whileStoper) < Vector3.Distance(A, B))
                        {
                            D = Vector3.MoveTowards(A, B, distance * whileStoper);       // Иду с шагом distance от точки А до точки В, и создаю звенья из префабов, связывая их друг с другом
                            // Вот тут я не сохраняю координаты вычисленных точек, просто их отрисовываю

                            if (   (Mathf.Abs(D.x - Points[i - 1].transform.position.x) < 0.001f)
                                && (Mathf.Abs(D.y - Points[i - 1].transform.position.y) < 0.001f)
                                && (Mathf.Abs(D.z - Points[i - 1].transform.position.z) < 0.001f)
                                && (isLoop == false))
                                // Немного не оптимизировано, но зато работает)
                                // Здесь генератор звеньев проверяет, и если это звено - ключевое, то он генерирует его немного по другому
                            {
                                New_Capsule = Instantiate(Prefab_Sphere, D, Quaternion.identity);
                                CreatedObjects.Add(New_Capsule);

                                // Задаю поворот для звена - средним, между предыдущим и следующим звеном
                                #region aaa
                                /*
                                Quaternion prevAngle = prefGO_Segment.transform.localRotation; // угол предыдущей точки
                                Quaternion nextAngle = Points[i].transform.localRotation; // угол следующей точки
                                float t = 0.5f; // параметр интерполяции
                                Quaternion currentAngle = Quaternion.Lerp(prevAngle, nextAngle, t); // средний угол текущей точки
                                New_Capsule.transform.localRotation = currentAngle; // задаем угол поворота для текущей точки
                                */

                                /*
                                float x1 = prefGO_Segment.transform.localEulerAngles.x; // начальное значение по оси X
                                float x2 = Points[i].transform.localEulerAngles.x; // конечное значение по оси X
                                float y1 = prefGO_Segment.transform.localEulerAngles.y; // начальное значение по оси Y
                                float y2 = Points[i].transform.localEulerAngles.y; // конечное значение по оси Y
                                float z1 = prefGO_Segment.transform.localEulerAngles.z; // начальное значение по оси Z
                                float z2 = Points[i].transform.localEulerAngles.z; // конечное значение по оси Z
                                float t = 0.5f; // параметр интерполяции
                                float x3 = Mathf.Lerp(x1, x2, t); // среднее значение по оси X
                                float y3 = Mathf.Lerp(y1, y2, t); // среднее значение по оси Y
                                float z3 = Mathf.Lerp(z1, z2, t); // среднее значение по оси Z
                                Vector3 currentAngle = new Vector3(x3, y3, z3); // средний угол в виде Vector3
                                New_Capsule.transform.localEulerAngles = currentAngle; // задаем угол поворота для текущей точки                                

                                print("i = " + i);
                                print("Пердыдущий угол: " + prefGO_Segment.transform.localEulerAngles);
                                print("Следующий угол: " + Points[i].transform.localEulerAngles);
                                print("Средний угол: " + currentAngle);
                                */
                                #endregion
                            }
                            else // Генерирую капсулу (сегмент)
                            {
                                New_Capsule = Instantiate(Prefab_Capsule, D, Quaternion.identity);  
                                CreatedObjects.Add(New_Capsule);

                                // Задаю ему нужный угол поворота
                                New_Capsule.transform.LookAt(Points[i].transform.position);
                                New_Capsule.transform.eulerAngles += new Vector3(90, 0, 0);
                            }

                            //New_Capsule.gameObject.GetComponent<MeshRenderer>().material = OffWire;

                            if (prefGO_Segment != null)
                            {
                                New_Capsule.gameObject.GetComponent<ConfigurableJoint>().connectedBody = prefGO_Segment.GetComponent<Rigidbody>();
                                prefGO_Segment = New_Capsule;
                            }
                            else
                            {
                                prefGO_Segment = New_Capsule;
                            }

                            if ((Mathf.Abs(New_Capsule.transform.position.x - Points[i-1].transform.position.x) < 0.001f)
                            && (Mathf.Abs(New_Capsule.transform.position.y - Points[i-1].transform.position.y) < 0.001f)
                            && (Mathf.Abs(New_Capsule.transform.position.z - Points[i-1].transform.position.z) < 0.001f))
                            {
                                if ((Points_Is_Kinematic[i-1] == true) || (i == 1))
                                // Первая точка в маршруте - всегда isKinematic
                                {
                                    New_Capsule.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                                    New_Capsule.gameObject.GetComponent<MeshRenderer>().material = StaticWire;
                                }
                            }

                            //print("D = " + D);

                            //Points[2].transform.position = D;
                        }
                        else break;
                        whileStoper++;
                    }
                }
            }
            if (i > 0)
            {
                if (((i == Limitter) || ((Points[i - 1].transform.localPosition == zero_vec))))// && (Points.Length < i-2) && (i-2 > 0))
                {
                    //print("Генерю последнюю точку " + (i-2) + " с координатами " + Points[i-2].transform.localPosition);

                    New_Capsule = Instantiate(Prefab_Sphere, Points[i - 2].transform.position, Quaternion.identity);
                    CreatedObjects.Add(New_Capsule);
                    New_Capsule.transform.localRotation = Quaternion.Euler(0, 0, 90);

                    New_Capsule.gameObject.GetComponent<ConfigurableJoint>().connectedBody = prefGO_Segment.GetComponent<Rigidbody>();

                    //if (Points_Is_Kinematic[i-2] == true)
                    // Последняя точка в маршруте - всегда isKinematic

                    New_Capsule.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    New_Capsule.gameObject.GetComponent<MeshRenderer>().material = StaticWire;

                    break;
                }
            }
        }
    }

    public int countOfList = -1;

    private bool isStartFlageOfDisableWire = true;

    public bool isEnable = false;
    public bool isEnableFinish = false;

    private float ttt = 0.125f;

    // Цикл постепенного включения сегментов провода
    IEnumerator ProcessingElementColoringWall()
    {
        int i = 0;
        int timeStoper = 0;

        // Тут делаю цикл, но через простой перебор

        while (i < countOfList + 7)
        {
            Shader shader = OffWire.shader;
            Material newMat = new Material(shader); // Создаём новый материал каждый раз

            newMat.color = Color.Lerp(OffWire.color, OnWire.color, ttt);

            if (i < countOfList)
            {
                CreatedObjects[i].gameObject.GetComponent<MeshRenderer>().material = newMat; // "Включаю" все элементы провода                
            }

            if (i - 7 < countOfList)
            {
                i++;
            }

            if (((i - 1) >= 0) && (i - 1 < countOfList))
            {
                CreatedObjects[i-1].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.25f);
            }
            if (((i - 2) >= 0) && (i - 2 < countOfList))
            {
                CreatedObjects[i-2].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.375f);
            }
            if (((i - 3) >= 0) && (i - 3 < countOfList))
            {
                CreatedObjects[i-3].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.5f);
            }
            if (((i - 4) >= 0) && (i - 4 < countOfList))
            {
                CreatedObjects[i-4].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.625f);
            }
            if (((i - 5) >= 0) && (i - 5 < countOfList))
            {
                CreatedObjects[i - 5].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.75f);
            }
            if (((i - 6) >= 0) && (i - 6 < countOfList))
            {
                CreatedObjects[i - 6].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 0.875f);
            }
            if (((i - 7) >= 0) && (i - 7 < countOfList))
            {
                CreatedObjects[i - 7].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OffWire.color, OnWire.color, 1f);
            }

            if (timeStoper == 1)
            {
                timeStoper = 0;

                //yield return new WaitForFixedUpdate();
                //yield return null;
                //yield return new WaitForSeconds(0.001f);
                yield return new WaitForFixedUpdate();
                //yield return new WaitForSecondsRealtime(0.001f);
            }
            else
            {
                timeStoper++;
            }
        }

        if (i == countOfList + 7)
        {
            isEnableFinish = true; // Провод передаёт сигнал, что включен - когда включен его последний сегмент
            //currIndexOfEnabling = 0;

            yield break;
        }
    }

    // Цикл постепенного выключения сегментов провода
    IEnumerator ProcessingDisebleElementColoringWall()
    {
        int i = 0;
        int timeStoper = 0;

        // Тут делаю цикл, но через простой перебор

        while (i < countOfList + 7)
        {
            Shader shader = OnWire.shader;
            Material newMat = new Material(shader); // Создаём новый материал каждый раз

            newMat.color = Color.Lerp(OnWire.color, OffWire.color, ttt);

            if (i < countOfList)
            {
                CreatedObjects[i].gameObject.GetComponent<MeshRenderer>().material = newMat; // "Включаю" все элементы провода                
            }

            if (i - 7 < countOfList)
            {
                i++;
            }

            if (((i - 1) >= 0) && (i - 1 < countOfList))
            {
                CreatedObjects[i - 1].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.25f);
            }
            if (((i - 2) >= 0) && (i - 2 < countOfList))
            {
                CreatedObjects[i - 2].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.375f);
            }
            if (((i - 3) >= 0) && (i - 3 < countOfList))
            {
                CreatedObjects[i - 3].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.5f);
            }
            if (((i - 4) >= 0) && (i - 4 < countOfList))
            {
                CreatedObjects[i - 4].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.625f);
            }
            if (((i - 5) >= 0) && (i - 5 < countOfList))
            {
                CreatedObjects[i - 5].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.75f);
            }
            if (((i - 6) >= 0) && (i - 6 < countOfList))
            {
                CreatedObjects[i - 6].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 0.875f);
            }
            if (((i - 7) >= 0) && (i - 7 < countOfList))
            {
                CreatedObjects[i - 7].gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(OnWire.color, OffWire.color, 1f);
            }

            if (timeStoper == 1)
            {
                timeStoper = 0;

                //yield return new WaitForFixedUpdate();
                //yield return null;
                //yield return new WaitForSeconds(0.001f);
                yield return new WaitForFixedUpdate();
                //yield return new WaitForSecondsRealtime(0.001f);
            }
            else
            {
                timeStoper++;
            }
        }

        if (i == countOfList + 7)
        {
            isEnableFinish = false; // Провод передаёт сигнал, что выключен - когда выключен его последний сегмент

            yield break;
        }
    }

    int workMode = 0;

    public void FixedUpdate()
    {
        if (countOfList <= 0)
        {
            countOfList = CreatedObjects.Count;
        }
        else
        {
            if (isEnable == true)
            {
                if (workMode == 0)
                {
                    workMode = 1;

                    StartCoroutine(ProcessingElementColoringWall());

                    isStartFlageOfDisableWire = false;
                }
            }
            else // Одна из куротин сработает только один раз, при переключении
            {
                if (workMode == 1)
                {
                    workMode = 0;
                    if (isStartFlageOfDisableWire == false) // Анимация вЫключения провода сработает, только если он до этого был включён
                    {
                        StartCoroutine(ProcessingDisebleElementColoringWall());
                    }
                }
            }
        }
    }

    /*
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        Gizmos.color = Color.red;

        for (int i = 0; i < Points.Length; i++)
        {
            if ((Points[i] != null) && (Points[i].transform.localPosition != zero_vec))
            // Если точка существует, и не находится в локальных координатах 0,0,0
            {
                Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);

                if (i > 0)
                {
                    A = Points[i-1].transform.position;
                    B = Points[i].transform.position;

                    int whileStoper = 0;

                    while (true)
                    {
                        if (whileStoper >= 2000) break;

                        if ((distance * whileStoper) < Vector3.Distance(A, B))
                        {
                            D = Vector3.MoveTowards(A, B, distance * whileStoper);
                            Gizmos.DrawSphere(D, sizeGizmos);                           // Вот тут я не сохраняю координаты вычисленных точек, просто их отрисовываю

                            // New_Capsule = Instantiate(Prefab_Capsule, D, Quaternion.identity);  // Наверно стоит делать массив таких объектов

                            //print("D = " + D);

                            //Points[2].transform.position = D;
                        }
                        else break;
                        whileStoper++;
                    }
                }
            }
        }
    }
    */

    /*
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        Gizmos.color = Color.red;

        for (int i = 0; i < Points.Length; i++)
        {
            if ((Points[i] != null) && (Points[i].transform.localPosition != zero_vec))
            // Если точка существует, и не находится в локальных координатах 0,0,0
            {
                Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);

                if (i > 0)
                {
                    A = Points[i-1].transform.position;
                    B = Points[i].transform.position;

                    int whileStoper = 0;

                    while (true)
                    {
                        if (whileStoper >= 2000) break;

                        if ((distance * whileStoper) < Vector3.Distance(A, B))
                        {
                            D = Vector3.MoveTowards(A, B, distance * whileStoper);
                            Gizmos.DrawSphere(D, sizeGizmos);                           // Вот тут я не сохраняю координаты вычисленных точек, просто их отрисовываю

                            //print("D = " + D);

                            //Points[2].transform.position = D;
                        }
                        else break;
                        whileStoper++;
                    }
                }
            }
        }
    }
    */
}
