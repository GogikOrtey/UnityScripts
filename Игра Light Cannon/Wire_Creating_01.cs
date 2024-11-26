using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire_Creating_01 : MonoBehaviour
{
    //���� ����� ��������� � ��������� ����������� 0,0,0 - �� ��� ������������

    public GameObject[] Points = new GameObject[20];
    //public Vector3[] Points2 = new Vector3[3];

    [Tooltip("����������� �� ��� �����? [������ � ��������� ����� - ������ isKinematic]")] public bool[] Points_Is_Kinematic = new bool[20];

    public float distance = 0.77f; // �������� ���������� // ������������ � ����������. ��������� ����������, ���������� ������� ���� - ��� 0.0077
    private float sizeGizmos = 0.1f;

    public GameObject Prefab_Capsule;
    private GameObject New_Capsule;

    public GameObject Prefab_Sphere; // ������������ ��� �������� �������

    //public bool notOk = false;
    private Vector3 zero_vec = Vector3.zero;

    Vector3 A = Vector3.zero;
    Vector3 B = Vector3.zero;
    Vector3 D = Vector3.zero;

    public GameObject prefGO_Segment = null; // ���������� �����. ����� ��� �������� ������ � �������

    [Tooltip("������������ �� �����, ����������� � ������� �����������?")] public bool isEnebleDrowZeroPoints = true;
    [Tooltip("������������ ������������� �����. ���� ����� ����, �� ������������ ��� �����")] public int Limitter = 0;

    public Material OffWire;
    public Material OnWire;
    public Material StaticWire;

    public bool isEnebleDraw = true;

    List<GameObject> CreatedObjects = new List<GameObject>();

    //public Color OnWire_C;
    //public Color OffWire_C;

    void OnDrawGizmos()
    {
        // ����������� ��������� �����

        if (isEnebleDraw == true)
        {
            Gizmos.color = Color.blue;
            //Gizmos.color = Color.red;

            for (int i = 0; i < Points.Length; i++)
            {
                if ((Points[i] != null) && (Points[i].transform.localPosition != zero_vec))
                // ���� ����� ����������, � �� ��������� � ��������� ����������� 0,0,0
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(Points[i].transform.position, sizeGizmos);
                }
                else if ((Points[i].transform.localPosition == zero_vec) && (isEnebleDrowZeroPoints == true))
                {
                    // �� ������ ������, ����������� ����� ����������� � ������� �����������, ���� ��� ����

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
            // ���� ����� ����������, � �� ��������� � ��������� ����������� 0,0,0
            {
                //print("��������� ���������� ����� " + i + " = " + Points[i].transform.localPosition);

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
                            D = Vector3.MoveTowards(A, B, distance * whileStoper);       // ��� � ����� distance �� ����� � �� ����� �, � ������ ������ �� ��������, �������� �� ���� � ������
                            // ��� ��� � �� �������� ���������� ����������� �����, ������ �� �����������

                            if (   (Mathf.Abs(D.x - Points[i - 1].transform.position.x) < 0.001f)
                                && (Mathf.Abs(D.y - Points[i - 1].transform.position.y) < 0.001f)
                                && (Mathf.Abs(D.z - Points[i - 1].transform.position.z) < 0.001f)
                                && (isLoop == false))
                                // ������� �� ��������������, �� ���� ��������)
                                // ����� ��������� ������� ���������, � ���� ��� ����� - ��������, �� �� ���������� ��� ������� �� �������
                            {
                                New_Capsule = Instantiate(Prefab_Sphere, D, Quaternion.identity);
                                CreatedObjects.Add(New_Capsule);

                                // ����� ������� ��� ����� - �������, ����� ���������� � ��������� ������
                                #region aaa
                                /*
                                Quaternion prevAngle = prefGO_Segment.transform.localRotation; // ���� ���������� �����
                                Quaternion nextAngle = Points[i].transform.localRotation; // ���� ��������� �����
                                float t = 0.5f; // �������� ������������
                                Quaternion currentAngle = Quaternion.Lerp(prevAngle, nextAngle, t); // ������� ���� ������� �����
                                New_Capsule.transform.localRotation = currentAngle; // ������ ���� �������� ��� ������� �����
                                */

                                /*
                                float x1 = prefGO_Segment.transform.localEulerAngles.x; // ��������� �������� �� ��� X
                                float x2 = Points[i].transform.localEulerAngles.x; // �������� �������� �� ��� X
                                float y1 = prefGO_Segment.transform.localEulerAngles.y; // ��������� �������� �� ��� Y
                                float y2 = Points[i].transform.localEulerAngles.y; // �������� �������� �� ��� Y
                                float z1 = prefGO_Segment.transform.localEulerAngles.z; // ��������� �������� �� ��� Z
                                float z2 = Points[i].transform.localEulerAngles.z; // �������� �������� �� ��� Z
                                float t = 0.5f; // �������� ������������
                                float x3 = Mathf.Lerp(x1, x2, t); // ������� �������� �� ��� X
                                float y3 = Mathf.Lerp(y1, y2, t); // ������� �������� �� ��� Y
                                float z3 = Mathf.Lerp(z1, z2, t); // ������� �������� �� ��� Z
                                Vector3 currentAngle = new Vector3(x3, y3, z3); // ������� ���� � ���� Vector3
                                New_Capsule.transform.localEulerAngles = currentAngle; // ������ ���� �������� ��� ������� �����                                

                                print("i = " + i);
                                print("���������� ����: " + prefGO_Segment.transform.localEulerAngles);
                                print("��������� ����: " + Points[i].transform.localEulerAngles);
                                print("������� ����: " + currentAngle);
                                */
                                #endregion
                            }
                            else // ��������� ������� (�������)
                            {
                                New_Capsule = Instantiate(Prefab_Capsule, D, Quaternion.identity);  
                                CreatedObjects.Add(New_Capsule);

                                // ����� ��� ������ ���� ��������
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
                                // ������ ����� � �������� - ������ isKinematic
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
                    //print("������ ��������� ����� " + (i-2) + " � ������������ " + Points[i-2].transform.localPosition);

                    New_Capsule = Instantiate(Prefab_Sphere, Points[i - 2].transform.position, Quaternion.identity);
                    CreatedObjects.Add(New_Capsule);
                    New_Capsule.transform.localRotation = Quaternion.Euler(0, 0, 90);

                    New_Capsule.gameObject.GetComponent<ConfigurableJoint>().connectedBody = prefGO_Segment.GetComponent<Rigidbody>();

                    //if (Points_Is_Kinematic[i-2] == true)
                    // ��������� ����� � �������� - ������ isKinematic

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

    // ���� ������������ ��������� ��������� �������
    IEnumerator ProcessingElementColoringWall()
    {
        int i = 0;
        int timeStoper = 0;

        // ��� ����� ����, �� ����� ������� �������

        while (i < countOfList + 7)
        {
            Shader shader = OffWire.shader;
            Material newMat = new Material(shader); // ������ ����� �������� ������ ���

            newMat.color = Color.Lerp(OffWire.color, OnWire.color, ttt);

            if (i < countOfList)
            {
                CreatedObjects[i].gameObject.GetComponent<MeshRenderer>().material = newMat; // "�������" ��� �������� �������                
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
            isEnableFinish = true; // ������ ������� ������, ��� ������� - ����� ������� ��� ��������� �������
            //currIndexOfEnabling = 0;

            yield break;
        }
    }

    // ���� ������������ ���������� ��������� �������
    IEnumerator ProcessingDisebleElementColoringWall()
    {
        int i = 0;
        int timeStoper = 0;

        // ��� ����� ����, �� ����� ������� �������

        while (i < countOfList + 7)
        {
            Shader shader = OnWire.shader;
            Material newMat = new Material(shader); // ������ ����� �������� ������ ���

            newMat.color = Color.Lerp(OnWire.color, OffWire.color, ttt);

            if (i < countOfList)
            {
                CreatedObjects[i].gameObject.GetComponent<MeshRenderer>().material = newMat; // "�������" ��� �������� �������                
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
            isEnableFinish = false; // ������ ������� ������, ��� �������� - ����� �������� ��� ��������� �������

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
            else // ���� �� ������� ��������� ������ ���� ���, ��� ������������
            {
                if (workMode == 1)
                {
                    workMode = 0;
                    if (isStartFlageOfDisableWire == false) // �������� ���������� ������� ���������, ������ ���� �� �� ����� ��� �������
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
            // ���� ����� ����������, � �� ��������� � ��������� ����������� 0,0,0
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
                            Gizmos.DrawSphere(D, sizeGizmos);                           // ��� ��� � �� �������� ���������� ����������� �����, ������ �� �����������

                            // New_Capsule = Instantiate(Prefab_Capsule, D, Quaternion.identity);  // ������� ����� ������ ������ ����� ��������

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
            // ���� ����� ����������, � �� ��������� � ��������� ����������� 0,0,0
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
                            Gizmos.DrawSphere(D, sizeGizmos);                           // ��� ��� � �� �������� ���������� ����������� �����, ������ �� �����������

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
