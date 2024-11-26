using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaser : MonoBehaviour
{

    public Material material;
    //public List<Vector3> LocalLaserIndces = new List<Vector3>();

    public GameObject prefabCylynder;
    public GameObject clonnedCylynder;

    public int countVert;

    private Item Item;
    public static int countOfRay;

    public GameObject[] LightRenderRay;
    public Camera MainOnceCam;

    public RaycastHit OnCamHit;
    public Vector3 rayOrigin;

    public Vector3 CamCenterHit;

    Vector3 pos, dir;
    public List<Vector3> laserIndces = new List<Vector3>();

    public LayerMask layerMaskWithoutPlayer = 1;

    [Tooltip("Если эта переменная = true, то луч всегда включён")] public bool isTestMode = false;

    void Start()
    {
        countOfRay = Item.countOfRay;

        CamCenterHit = new Vector3();

        LightRenderRay = new GameObject[countOfRay];

        for (int i = 0; i < countOfRay; i++)
        {
            LightRenderRay[i] = Instantiate(prefabCylynder, this.gameObject.transform.position, Quaternion.identity);
            LightRenderRay[i].SetActive(false);
        }

        //isShooting = true;

        //int ignoreWeaponMask = 1 << 8;
        //layerMaskWithoutPlayer = ~ignoreWeaponMask;
    }

    private void Update()
    {
        if (isTestMode == false)
        {
            if (Input.GetMouseButton(0))
            {
                isShooting = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isShooting = false;
                //print("Кнопка мыши отпущена");
                clearLightBeams();
            }
        }
        else
        {
            isShooting = true;
        }
    }

    public bool isShooting = false;

    void OnWillRenderObject() 
    {
        if (isShooting)
        {
            currentRecurceRay = 0;
            isRayDontWr = true;

            laserIndces.Clear();
            LaserBeam(gameObject.transform.position, gameObject.transform.right, material);
            //LocalLaserIndces = laserIndces; // Для отладки в инспекторе

            if (isRayDontWr == false) laserIndces.RemoveAt(laserIndces.Count - 1);

            RenderLightRay();
        }
    }

    public void clearLightBeamsOnOtherScripts()
    {
        if (isShooting == true)
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

        for (int i = 0; i< countVert; i++)
        {
            Gizmos.DrawSphere(laserIndces[i], 0.2f);
        }

        print("Current vertex count = " + countVert);
       
        //if (CamCenterHit != null)
        //{
        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawSphere(CamCenterHit, 0.2f);
        //}
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

        // Если это конченая точка первого луча, то вместо стандартного райкаста мы заменяем её на райкаст в центр экрана
        if (laserIndces.Count == 1)
        {
            if (isRayDontWr == true)
            {
                rayOrigin = MainOnceCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)); // Нахожу точку центра экрана

                if (Physics.Raycast(rayOrigin, MainOnceCam.transform.forward, out OnCamHit, 250f, layerMaskWithoutPlayer))
                {
                    if (Physics.Raycast(ray, out hit, 50, layerMaskWithoutPlayer))
                    {
                        if (OnCamHit.distance - hit.distance > 5f) // Если стандартный луч сильно меньше чем луч из середины экрана, то используем стандартный луч
                        {
                            print("Обнаружена проблема несоответствия луча с прицелом");
                            // Не до конца работает так как надо, но в общем, проблемы не видно
                            isRayDontWr = false;
                            StandartRaycast(ray, hit);
                        }
                    }

                    CheckHit(OnCamHit, dir);
                }
                else
                {
                    laserIndces.Add(ray.GetPoint(50));
                }
            }
            else
            {
                StandartRaycast(ray, hit);
            }
        }
        else // Во всех остальных случаях пользуемся стандартным райкастом
        {
            StandartRaycast(ray, hit);
        }

        //StandartRaycast(ray, hit);
    }

    void StandartRaycast(Ray ray, RaycastHit hit)
    {
        if (Physics.Raycast(ray, out hit, 50, layerMaskWithoutPlayer))
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
