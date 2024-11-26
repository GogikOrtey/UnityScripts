using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaserOnEmitters : MonoBehaviour
{
    /*
        ������� ���������� ������ ������� �������� � ������� �����.
        �������� ��� ��������� (�����������)
    */

    [Tooltip("������� �� ���������� � ������ ������?")] public bool isActive = false;
    private bool buferActive;

    public Material material;

    public GameObject prefabCylynder;
    public GameObject clonnedCylynder;

    [Tooltip("���������� ������ (��������� �����), � ������ ������. ���������� ����� ��� �������")] public int countVert;

    [Tooltip("��������� ������� ������ � ������� ������ ����� �������")] public GameObject Indicator;

    private Item Item;
    [Tooltip("����������� ��������� ���������� ����� (��� ����������)")] public static int countOfRay;

    [Tooltip("������ ��������� (�����). ��� ��������� ���� ���, � ����� ������ ���������� � �����������")] public GameObject[] LightRenderRay;

    private RaycastHit OnCamHit;
    private Vector3 rayOrigin;

    Vector3 pos, dir;
    [Tooltip("������ ���� �����, �� ���� ����")] public List<Vector3> laserIndces = new List<Vector3>();
    
    public Color orange; // ���� ����� � ��������� HSW(28, 100, 100)

    void Start()
    {
        countOfRay = Item.countOfRay;

        LightRenderRay = new GameObject[countOfRay];

        for (int i = 0; i < countOfRay; i++)
        {
            LightRenderRay[i] = Instantiate(prefabCylynder, this.gameObject.transform.position, Quaternion.identity);
            LightRenderRay[i].SetActive(false);
        }

        buferActive = isActive;

        Indicator.GetComponent<MeshRenderer>().material.color = orange;
    }

    int cc = 0;

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Keypad3))
        {
            if (cc == 0)
            {
                isActive = !isActive;
                print("������������ ����������");
                cc = 10;
            }
        }

        if (cc > 0) cc--;

        if (buferActive != isActive)
        {
            buferActive = isActive;

            if (isActive == false)
            {
                clearLightBeams(); // ������ ��� ����, ���� ���������� ���������
                Indicator.GetComponent<MeshRenderer>().material.color = orange;
            }
            else
            {
                Indicator.GetComponent<MeshRenderer>().material.color = Color.green;
            }
        }

        if (isActive)
        {
            if (lookMeOffset > 0)
            {
                lookMeOffset--;
                isLookedMe = true;
            }
            else
            {
                isLookedMe = false;
                CastRayAndPaintRays();
            }
        }
    }

    [Tooltip("����� �� ���� ���� ���� ������? ���� ��, �� ���� ���������� �� ��������� ������� ������. " +
        "���� ��� - �� ��������� �������������� ����������. ��� ���������� �����������, ������ ���� ���������� �������")] public bool isLookedMe; 
    private int lookMeOffset;

    void OnWillRenderObject()
    {
        if (isActive)
        {
            lookMeOffset = 5;
            CastRayAndPaintRays();
        }
    }

    private void CastRayAndPaintRays()
    {
        currentRecurceRay = 0;
        isRayDontWr = true;

        laserIndces.Clear();
        LaserBeam(gameObject.transform.position, gameObject.transform.right, material);

        if (isRayDontWr == false) laserIndces.RemoveAt(laserIndces.Count - 1);

        RenderLightRay();
    }

    public void clearLightBeamsOnOtherScripts()
    {
        if (isActive == true)
        {
            clearLightBeams();
        }
    }

    public void clearLightBeams()
    {
        for (int i = 0; i < countOfRay; i++)
        {
            LightRenderRay[i].SetActive(false);
        }

        laserIndces.Clear();
    }

    public void clearLightBeamsOnly()
    {
        for (int i = laserIndces.Count - 1; i < countOfRay; i++)
        {
            LightRenderRay[i].SetActive(false);
        }
    }


    public void RenderLightRay()
    {
        clearLightBeamsOnly();

        for (int i = 0; i < Mathf.Min(countOfRay, laserIndces.Count - 1); i++) // i < laserIndces.Count
        {
            LightRenderRay[i].SetActive(true);
            LightRenderRay[i].transform.position = laserIndces[i];
            LightRenderRay[i].transform.LookAt(laserIndces[i + 1]);
            float dist = Vector3.Distance(laserIndces[i], laserIndces[i + 1]);
            dist = dist / 2;
            LightRenderRay[i].transform.localScale = new Vector3(LightRenderRay[i].transform.localScale.x, LightRenderRay[i].transform.localScale.y, dist);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        int countVert = laserIndces.Count;

        for (int i = 0; i < countVert; i++)
        {
            Gizmos.DrawSphere(laserIndces[i], 0.2f);
        }

        print("Current vertex count = " + countVert + ", Name emitters: " + this.gameObject.name);
    }

    public void LaserBeam(Vector3 pos, Vector3 dir, Material material)
    {
        this.pos = pos;
        this.dir = dir;

        CastRay(pos, dir);
    }

    bool isRayDontWr = true; // = true, ����� ����������� ��� ������������� ������������

    void CastRay(Vector3 pos, Vector3 dir)
    {
        laserIndces.Add(pos);

        Ray ray = new Ray(pos, dir);
        RaycastHit hit = new RaycastHit();

        StandartRaycast(ray, hit);
    }

    void StandartRaycast(Ray ray, RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, 50, 1))
        {
            CheckHit(hit, ray.direction);
        }
        else
        {
            laserIndces.Add(ray.GetPoint(50));
        }
    }

    int currentRecurceRay;

    void CheckHit(RaycastHit hitInfo, Vector3 direction)
    {
        //print("��� �" + currentRecurceRay + " �������� ������� " + hitInfo.collider.gameObject.name + " � ����� '" + hitInfo.collider.gameObject.tag + "'" + 
        //    " ������ ����: " + hitInfo.distance + " ������� ����������� ����: " + hitInfo.normal);

        if (hitInfo.collider.gameObject.tag == "Mirror")
        {
            Vector3 pos1;
            Vector3 dir;
            Vector3 hitNormal = hitInfo.normal;

            pos1 = hitInfo.point;

            dir = Vector3.Reflect(direction, hitNormal);

            //print("+ ��� �" + currentRecurceRay + " �������� �������. ��������� �����������: " + direction + ", ���������� �����������: " + dir);

            if (currentRecurceRay < countOfRay)
            {
                currentRecurceRay++;
                CastRay(pos1, dir);
            }
        }
        else
        {
            laserIndces.Add(hitInfo.point);
        }
    }
}