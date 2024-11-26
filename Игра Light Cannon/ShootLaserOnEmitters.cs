using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaserOnEmitters : MonoBehaviour
{
    /*
        Немного упрощённая версия скрипта райкаста и рендера лучей.
        Написана для эмиттеров (излучателей)
    */

    [Tooltip("Включён ли излучатель в данный момент?")] public bool isActive = false;
    private bool buferActive;

    public Material material;

    public GameObject prefabCylynder;
    public GameObject clonnedCylynder;

    [Tooltip("Количество вершин (отражений лучей), в данный момент. Переменная нужна для отладки")] public int countVert;

    [Tooltip("Маленький цветной кружок в верхней правой части объекта")] public GameObject Indicator;

    private Item Item;
    [Tooltip("Максимально возможное количество лучей (при отражениях)")] public static int countOfRay;

    [Tooltip("Массив цилиндров (лучей). Они создаются один раз, а потом только включаются и отключаются")] public GameObject[] LightRenderRay;

    private RaycastHit OnCamHit;
    private Vector3 rayOrigin;

    Vector3 pos, dir;
    [Tooltip("Массив всех точек, на пути луча")] public List<Vector3> laserIndces = new List<Vector3>();
    
    public Color orange; // Цвет задаём в редакторе HSW(28, 100, 100)

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
                print("Активировали излучатели");
                cc = 10;
            }
        }

        if (cc > 0) cc--;

        if (buferActive != isActive)
        {
            buferActive = isActive;

            if (isActive == false)
            {
                clearLightBeams(); // Чистим все лучи, если излучатель выключили
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

    [Tooltip("Видит ли меня хоть одна камера? Если да, то лучи рендерятся со скоростью частоты кадров. " +
        "Если нет - со скоростью фиксированного обновления. Эта переменная обновляется, только если излучатель включён")] public bool isLookedMe; 
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

    bool isRayDontWr = true; // = true, когда стандартный луч перекрывается препятствием

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
        //print("Луч №" + currentRecurceRay + " касается объекта " + hitInfo.collider.gameObject.name + " с тегом '" + hitInfo.collider.gameObject.tag + "'" + 
        //    " длинна луча: " + hitInfo.distance + " нормаль поверхности луча: " + hitInfo.normal);

        if (hitInfo.collider.gameObject.tag == "Mirror")
        {
            Vector3 pos1;
            Vector3 dir;
            Vector3 hitNormal = hitInfo.normal;

            pos1 = hitInfo.point;

            dir = Vector3.Reflect(direction, hitNormal);

            //print("+ Луч №" + currentRecurceRay + " коснулся зеркала. Начальное направление: " + direction + ", зеркальное напрваление: " + dir);

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