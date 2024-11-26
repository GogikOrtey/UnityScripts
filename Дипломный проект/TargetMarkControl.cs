using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetMarkControl : MonoBehaviour
{
    // Метка направления - основа
    public GameObject MainMarkRot;

    //public GameObject Player;

    // Направление угла, которое задаётся
    [Range(0f, 180f)] public float directionOfRotation = 90f;
    // 90 = прямо сверху

    // Вот сюда нужно добавить код для управления этой шкалой
    // Т.е. например 0 = 90, 1 = 0 градусов, а 0 = 180 градусов

    // Текущий угол поворота
    private float currentRotation = 0f;
    // 0 = прямо наверху карты

    // Скорость поворота метки до нужного значения
    float speedOfRotMark = 5f;

    public PlayerController_01 PlayerController_01;

    public Image Arr_left;
    public Image Arr_right;

    void Start()
    {

    }

    // Процедуры, для управленем поворота игрока

    //public bool isRotatingRight = false;
    //public bool isRotatingLeft = false;

    //public void RotateRight()
    //{
    //    if (isCanRot)
    //    {
    //        if (isRotatingLeft == false && isRotatingRight == false)
    //        {
    //            isRotatingRight = true;

    //            statrRotPlayerRotation = Player.transform.rotation.eulerAngles.y;
    //            // Устанавливаем текущий угол поворота игрока, как начальный. Теперь мы должны повернуться направо на 90 градусов, от него

    //            print("Инициализируем поворот вправо. Начальный угол игрока = " + statrRotPlayerRotation);
    //        }
    //    }
    //}
    
    //public void RotateLeft()
    //{
    //    if (isCanRot)
    //    {
    //        if (isRotatingLeft == false && isRotatingRight == false)
    //        {
    //            isRotatingLeft = true;

    //            statrRotPlayerRotation = Player.transform.rotation.eulerAngles.y;
    //            // Устанавливаем текущий угол поворота игрока, как начальный. Теперь мы должны повернуться направо на 90 градусов, от него

    //            print("Инициализируем поворот влево. Начальный угол игрока = " + statrRotPlayerRotation);
    //        }
    //    }
    //}

    float currentPlayerRotation;
    public float statrRotPlayerRotation;

    float dill = 5f; // Ценя деления для поворота игрока относительно нужного поворота

    int timer = 0;
    bool isCanRot = false; // Мы можем поворачивать?



    void FixedUpdate()
    {
        // Добавил таймер, т.к. были небольшие проблемы с поворотом, сразу после запуска
        if (timer < 5) timer++;
        else if (timer == 5)
        {
            timer++;
            isCanRot = true;
        }

        //// ----- Работаем с поворотом игрока
        {


            //currentPlayerRotation = Player.transform.rotation.eulerAngles.y;

            //if (isRotatingRight == true)
            //{
            //    float diffAngle = Mathf.Abs(Mathf.DeltaAngle(currentPlayerRotation, statrRotPlayerRotation + 90f));

            //    if (diffAngle > dill)
            //    {
            //        directionOfRotation = 0;

            //        //print("Мы в процессе поворота вправо. Разница между текущим и требуемым углом поворота составляет: " + diffAngle);
            //    }
            //    else
            //    {
            //        // Мы повернули достаточно вправо
            //        print("Мы повернули достаточно вправо");
            //        Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, statrRotPlayerRotation + 90f, Player.transform.rotation.eulerAngles.z);

            //        directionOfRotation = 90;
            //        isRotatingRight = false;
            //    }
            //}

            //if (isRotatingLeft == true)
            //{
            //    float diffAngle = Mathf.Abs(Mathf.DeltaAngle(currentPlayerRotation, statrRotPlayerRotation - 90f));

            //    if (diffAngle > dill)
            //    {
            //        directionOfRotation = 180;

            //        //print("Мы в процессе поворота влево. Разница между текущим и требуемым углом поворота составляет: " + diffAngle);
            //    }
            //    else
            //    {
            //        // Мы повернули достаточно влево
            //        print("Мы повернули достаточно влево");
            //        Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, statrRotPlayerRotation - 90f, Player.transform.rotation.eulerAngles.z);

            //        directionOfRotation = 90;
            //        isRotatingLeft = false;
            //    }
            //}
        }


        // ----- Работаем с маркером локальной карты:

        // Получаем угол поворота
        currentRotation = MainMarkRot.transform.rotation.eulerAngles.z;

        //print("currentRotation = " + currentRotation);

        // Вычисляем разницу между текущим углом и целевым
        float delta = directionOfRotation - currentRotation;

        // Если разница больше скорости вращения, вращаем объект
        if (Mathf.Abs(delta) > speedOfRotMark)
        {
            float newRotation = currentRotation + Mathf.Sign(delta) * speedOfRotMark;

            // Задаём новый угол поворота
            MainMarkRot.transform.rotation = Quaternion.Euler(0, 0, newRotation);
        }
        else
        {
            MainMarkRot.transform.rotation = Quaternion.Euler(0, 0, directionOfRotation);
        }

        float offset = 3;
        float deletelCoeff = 10f * 100;

        int delitRot = 20; // Делитель, по которому мы определяем, сейчас будем делать маленький, или большой поворот
        // т.е. например в левой четверти: от 0 до delitRot - большой угол поворота, и от delitRot до 90 - маленький

        // Для управления персонажем
        if (directionOfRotation != 90)
        {
            // Небольшие повороты:

            if (directionOfRotation >= (90 + offset) && directionOfRotation <= (180 - delitRot))
            {
                float locCoff = (directionOfRotation - 90) / deletelCoeff;

                print("< TargetMarkControl: Небольшой поворот влево");

                PlayerController_01.PlayerControl_TurnLeft(locCoff);
                Arr_left.enabled = true;
                Arr_right.enabled = false;
            }
            else if (directionOfRotation <= (90 - offset) && directionOfRotation >= delitRot)
            {
                float locCoff = (90 - directionOfRotation) / deletelCoeff;

                print("> TargetMarkControl: Небольшой поворот вправо");

                PlayerController_01.PlayerControl_TurnRight(locCoff);
                Arr_right.enabled = true;
                Arr_left.enabled = false;
            }

            // Большие повороты:
            // Работают, только если угол > 80

            else
            if (directionOfRotation > (180 - delitRot) && directionOfRotation <= 180)
            {
                print("<< TargetMarkControl: Большой поворот влево");

                PlayerController_01.PlayerControl_TurnLeft(0.1f);
                Arr_left.enabled = true;
                Arr_right.enabled = false;
            }
            else if (directionOfRotation < delitRot && directionOfRotation >= 0)
            {
                print(">> TargetMarkControl: Большой поворот вправо");

                PlayerController_01.PlayerControl_TurnRight(0.1f);
                Arr_right.enabled = true;
                Arr_left.enabled = false;
            }
        }
        
        if (directionOfRotation > (90 - offset) && directionOfRotation < (90 + offset))
        {
            // Выключаем все значки

            Arr_right.enabled = false;
            Arr_left.enabled = false;
        }

        // Здесь я не прописал логику, как перснаж понимает, что он уже достаточно повернул, и можно больше не поворачивать
        // Эта логика будет выполнятся с использованием глобальной карты
    }
}
