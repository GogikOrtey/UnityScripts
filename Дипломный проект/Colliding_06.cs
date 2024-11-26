using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colliding_06 : MonoBehaviour
{
    public PlayerController_01 PlayerController_01;
    public GameObject Player;
    public TargetMarkControl TargetMarkControl;

    int isRotRight = 0;     // 1 - если нужно повернуться вправо, -1 - если нужно повернуться влево
    float whRot = 0;        // На сколько градусов повернуться
    float startRot = 0;     // начальный угол поворота

    bool isNotItPriority = false;   // = true, если был приоритет > 0, и стал = 0
    bool counterStaOnWayB = false;  // Центральная метка касается пути?
    bool isOverRot = false;         // Эта переменная становится = true, когда мы закончили маленький синий поворот, но игрок не касается линии пути. Тогда мы доворачиваем его ещё сильнее

    public int currentRotate = 90;
    // 90 градусов - это направо, на глобальной карте
    // 0 градусов - это вверх

    int counterFromStayWay = 0; // Счётчик, как долго персонаж уже стоит на пути. Не больше 500.

    public NavigatorLogAddText NavigatorLogAddText;

    void Update()
    {
        if (counterStaOnWayB == true)
        {
            if (counterFromStayWay < 500)
            {
                counterFromStayWay++;
            }
        }

        if (Colliding_10.isMarkFortrCenterForward2 == true)
        {
            Colliding_10.isMarkFortrCenterForward2 = false;
            GlobalRotateLeft();
        }

        if (Colliding_11.isMarkFortrCenterForward2R == true)
        {
            Colliding_11.isMarkFortrCenterForward2R = false;
            GlobalRotateRight();
        }

        if (Colliding_12.isMarkFortrCenterForward2RDown == true)
        {
            Colliding_12.isMarkFortrCenterForward2RDown = false;
            GlobalRotateRightDown();
        }

        // Если мы сейчас не выполняем глобальных поворотов на 90 градусов вправо или влево
        if (isGlobalRotate == false)
        {
            if (PlayerController_01.isPriority == 0)
            {
                // Проверяем и задаём повороты только тогда, когда более приоритетных поворотов нет

                if (isNotItPriority == true)
                {
                    isNotItPriority = false;

                    //print("PlayerController_01.isPriority = " + PlayerController_01.isPriority);

                    print(" --- Получили приоритет на выполнение синих поворотов, isRotRight = " + isRotRight);

                    // После получения приоритета перезапускаем процедуры поворота
                    // Это должно обновить углы, и увеличить их в случае необходимости

                    if (isRotRight == -1)
                    {
                        isRotRight = 0;
                        RotateLeft();
                    }
                    else if (isRotRight == 1)
                    {
                        isRotRight = 0;
                        RotateRight();
                    }
                }

                print("isRotRight = " + isRotRight);

                if (isRotRight == 0)
                {
                    if (Colliding_13.isMarkFortrCenterForwardToCollidingGlobalRot == false)
                    {
                        if (Colliding_07.isLeftCenter == true && Colliding_08.isRightCenter == false)
                        {
                            RotateLeft();
                        }
                        else if (Colliding_07.isLeftCenter == false && Colliding_08.isRightCenter == true)
                        {
                            RotateRight();
                        }
                    }
                    else
                    {
                        print("!><! Нам нужно выполнить поворот, но недалеко спереди коллайдер глобального поворта, и мы не поворачиваем");   
                    }
                }
                else
                {
                    if (Colliding_07.isLeftCenter == false && Colliding_08.isRightCenter == false && counterStaOnWayB == true) // Если центральная метка касается пути, а правая и левая - не касаются
                    {
                        // Если мы находимся по середине пути, нам уже не нужно никуда поворачиваться

                        //isRotRight = 0;
                        //print("||| Мы находимся на середине пути, отменяем все повороты");
                        print("||| Мы находимся на середине пути");
                    }
                    else
                    {
                        float playerRot = Player.transform.rotation.eulerAngles.y;

                        if (playerRot >= 270)
                        {
                            playerRot = 360 - playerRot;
                        }

                        if (isRotRight == -1)
                        {
                            if (Mathf.Abs(playerRot - (startRot - whRot)) < 2)
                            {
                                if (counterStaOnWayB == false && isOverRot == false) // Если мы не находимся на пути, и ещё не добавляли +10 градусов к повороту
                                {
                                    isOverRot = true;

                                    if (Colliding_07.isLeftCenter == true)
                                    {
                                        whRot += 10;        // Добавляем ещё +10 градусов к повороту, но только одиин раз
                                        print("Добавили +10 градусов к углу поворота, whRot = " + whRot);
                                    }
                                    else // Если путь с другой стороны от поворота
                                    {
                                        print("Запустил процедуру поворота в другую сторону");
                                        isRotRight = 0;
                                        RotateRight();
                                    }
                                }
                                else // Если мы находимся на пути, или уже добавили +10 градусов к повороту, и повернулись на них
                                {
                                    // Завершаем поворот, если персонаж стоит на пути уже больше 20 единиц времени
                                    // Или если разница между текущим углом персонажа и прямым > 10 градусов
                                    // Фактически, код будет ждать, пока поворот станет > 10 градусов, либо когда пройдёт больше 20 тиков
                                    if ((counterStaOnWayB == true && counterFromStayWay > 30) || (Mathf.Abs(playerRot - currentRotate) > 10))
                                    {
                                        isRotRight = 0;
                                        print("Закончили поворот влево, теперь мы смотрим ровно прямо");
                                        TargetMarkControl.directionOfRotation = 90;
                                        isOverRot = false;
                                    }
                                    else
                                    {
                                        print("Дополнительно поворачиваем влево");
                                        TargetMarkControl.directionOfRotation = 90 + (whRot);
                                    }
                                }
                            }
                            else
                            {
                                print("Поворачиваем влево, whRot = " + whRot);
                                //, Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot - whRot)) = " + Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot - whRot)));
                                TargetMarkControl.directionOfRotation = 90 + (whRot);
                                //PlayerController_01.PlayerControl_TurnLeft(0.1f);
                            }
                        }
                        else if (isRotRight == 1)
                        {
                            if (Mathf.Abs(playerRot - (startRot + whRot)) < 2)
                            {
                                if (counterStaOnWayB == false && isOverRot == false)
                                {
                                    isOverRot = true;
                                    if (Colliding_08.isRightCenter == true)
                                    {
                                        whRot += 10;        // Добавляем ещё +10 градусов к повороту, но только одиин раз
                                        print("Добавили +10 градусов к углу поворота, whRot = " + whRot);
                                    }
                                    else
                                    {
                                        print("Запустил процедуру поворота в другую сторону");
                                        isRotRight = 0;
                                        RotateLeft();
                                    }
                                }
                                else
                                {
                                    //print("Mathf.Abs(playerRot - currentRotate) = " + Mathf.Abs(playerRot - currentRotate));

                                    // Завершаем поворот, если персонаж стоит на пути уже больше 20 единиц времени
                                    // Или если разница между текущим углом персонажа и прямым > 10 градусов
                                    if ((counterStaOnWayB == true && counterFromStayWay > 20) || (Mathf.Abs(playerRot - currentRotate) > 10))
                                    {
                                        isRotRight = 0;
                                        print("Закончили поворот вправо, теперь мы смотрим ровно прямо");
                                        //print("counterStaOnWay = " + counterStaOnWay);
                                        TargetMarkControl.directionOfRotation = 90;
                                        isOverRot = false;
                                    }
                                    else
                                    {
                                        print("Дополнительно поворачиваем вправо");
                                        TargetMarkControl.directionOfRotation = 90 - (whRot);
                                    }
                                }
                            }
                            else
                            {
                                //print("Поворачиваем вправо"); ///, Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot - whRot)) = " + Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot - whRot)));
                                //print("Поворачиваем вправо, Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot + whRot)) = " + Mathf.Abs(Player.transform.rotation.eulerAngles.y - (startRot + whRot)));
                                //print("Поворачиваем вправо, Mathf.Abs(Player.transform.rotation.eulerAngles.y = " + Player.transform.rotation.eulerAngles.y + ", startRot = " + startRot + ", whRot = " + whRot);
                                print("Поворачиваем вправо, whRot = " + whRot);

                                TargetMarkControl.directionOfRotation = 90 - (whRot);
                            }
                        }
                    }
                }
            }
            else
            {
                isNotItPriority = true;
            }
        }
        else
        {
            float rotY = Player.transform.rotation.eulerAngles.y;

            if (rotY >= 270)
            {
                rotY = 360 - rotY;
            }

            print("rotY персонажа = " + rotY + ", currentRotate = " + currentRotate);

            if (Mathf.Abs(rotY - currentRotate) < 2.5f)
            {
                print("Мы закончили глобальный поворот");
                isGlobalRotate = false;
                TargetMarkControl.directionOfRotation = 90;

                // Перезагружаю локальные повороты

                if (isRotRight == -1)
                {
                    isRotRight = 0;
                    RotateLeft();
                }
                else if (isRotRight == 1)
                {
                    isRotRight = 0;
                    RotateRight();
                }
            }
        }
    }

    float minLock = 10f; // Если угол отклонения от прямого направления меньше этого значения - то не запускаем процедуру поворота
    float mini_minLock = 2f;

    // Функция, которая инициализирует поворот влево
    void RotateLeft()
    {
        if (isRotRight == 0)
        {
            print("ii Поворот влево?");

            float rotY = Player.transform.rotation.eulerAngles.y;

            if (rotY >= 270)
            {
                rotY = 360 - rotY;
            }

            float currRotate = 0; // Угол, на который нужно повернуться

            currRotate = rotY - currentRotate;
            currRotate = Mathf.Abs(currRotate);

            //if (currRotate > 80)
            //{
            //    print("currRotate > 80, currRotate = " + currRotate);
            //    return;
            //}

            if (counterStaOnWayB == false)
            {
                if (currRotate < mini_minLock)
                {
                    print("&& Угол до прямого очень маленький, но игрок не находится на пути. Добавляем +10 к углу, currRotate = " + currRotate);
                    // Если угол до прямого очень маленький, но игрок не находится на пути - делаем его больше

                    currRotate += 10;
                }
            }

            if (currRotate >= minLock) // Если отклоненине больше минимального
            {
                isOverRot = false;
                whRot = currRotate;
                startRot = rotY;
                isRotRight = -1;

                print("i< Инициализируем синий поворот влево на " + currRotate + " градусов, текущий поворот игрока = " + rotY);
            }
            else if (currRotate >= mini_minLock) // Если отклонение слишком маленькое, что бы делать поворот - то мы просто немного доворачиваем игрока в нужную сторону
            {
                print("i•< Повернули налево на 0.5f");
                PlayerController_01.PlayerControl_TurnLeft(0.5f);

                // Обнуляем повороты, на всякий случай
                isRotRight = 0;
                TargetMarkControl.directionOfRotation = 90;
                isOverRot = false;
            }
            else
            {
                //Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, currentRotate, Player.transform.rotation.eulerAngles.z);
                print("х-i Угол поворота влево очень маленький, не делаем его");
            }
        }
    }

    void RotateRight()
    {
        if (isRotRight == 0)
        {
            print("ii Поворот вправо?");

            float rotY = Player.transform.rotation.eulerAngles.y;

            if (rotY >= 270)
            {
                rotY = 360 - rotY;
            }

            float currRotate = 0; // Угол, на который нужно повернуться

            currRotate = rotY - currentRotate;
            currRotate = Mathf.Abs(currRotate);

            //if (currRotate > 80)
            //{
            //    print("currRotate > 80");
            //    return;
            //}

            if (counterStaOnWayB == false)
            {
                if (currRotate < mini_minLock)
                {
                    print("&& Угол до прямого очень маленький, но игрок не находится на пути. Добавляем +10 к углу, currRotate = " + currRotate);
                    // Если угол до прямого очень маленький, но игрок не находится на пути - делаем его больше

                    //int l = 1; l = currRotate > 0 ? 1 : -1;
                    //currRotate = currRotate + (10 * l);

                    currRotate += 10;
                }
            }

            if (currRotate >= minLock) // Если отклоненине больше минимального
            {
                isOverRot = false;
                whRot = currRotate;
                startRot = rotY;
                isRotRight = 1;

                print("i> Инициализируем синий поворот вправо на " + currRotate + " градусов, текущий поворот игрока = " + rotY);
            }
            else if (currRotate >= mini_minLock) // Если отклонение слишком маленькое, что бы делать поворот - то мы просто немного доворачиваем игрока в нужную сторону
            {
                print("i•> Повернули направо на 0.5f");
                PlayerController_01.PlayerControl_TurnRight(0.5f);

                // Обнуляем повороты, на всякий случай
                isRotRight = 0;
                TargetMarkControl.directionOfRotation = 90;
                isOverRot = false;
            }
            else
            {
                //Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, currentRotate, Player.transform.rotation.eulerAngles.z);
                print("х-i Угол поворота вправо очень маленький, не делаем его, currRotate = " + currRotate);
            }
        }
    }

    bool isGlobalRotate = false;

    // Функция разворот влево на 90 градусов
    // currentRotate = 0
    // Фиксируем прошлый lastCurrentRotate угол поворота
    // Делаем максимум по развороту влево, через TargetMarkControl
    // Отключаем локальные маленькие повороты
    // Постоянно проверяем: Если текущий угол поворота персонажа - currentRotate < 5-10 по модулю, то выключаем этот поворот
    // Устанавливаем прямой угол на TargetMarkControl, повороты выключаются
    // Включаем локальные повороты - перезагружаем их

    // Эти повороты не универсальны.
    
    void GlobalRotateLeft()
    {
        // Этот поворот сработает корректно только из направления направо - наверх

        isGlobalRotate = true;  // Блокируем небольшие повороты
        currentRotate = 0;      // Угол, который должен стать у персонажа (поворот по y)
        TargetMarkControl.directionOfRotation = 180; // Помним, что в TargetMarkControl нужно передавать угол, отражённый в другую сторону
    }

    void GlobalRotateRight()
    {
        // Этот поворот сработает корректно только из направления наверх - направо

        isGlobalRotate = true; 
        currentRotate = 90;
        TargetMarkControl.directionOfRotation = 0;
    }

    void GlobalRotateRightDown()
    {
        // Этот поворот сработает корректно только из направления направо - вниз

        isGlobalRotate = true; 
        currentRotate = 180;     
        TargetMarkControl.directionOfRotation = 0; 
    }

    // Метки поворота - это скрипты Colliding_10 и Colliding_11

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "GlobalR1")
        {
            NavigatorLogAddText.AddText("Через 20 метров поверните налево");
            print("Через 20 метров поверните налево");
        } 
        
        if (collision.tag == "GlobalR2" || collision.tag == "GlobalR3")
        {
            NavigatorLogAddText.AddText("Через 20 метров поверните направо");
            print("Через 20 метров поверните направо");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "TheWay")
        {
            //print("Игрок находится на пути");
            counterStaOnWayB = true;
        }

        if (collision.tag == "TargetZone")
        {
            print("END - Мы достигли пункта назначения");
            PlayerController_01.isFinished = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "TheWay")
        {
            //print("Игрок ушёл с пути");
            counterStaOnWayB = false;
            counterFromStayWay = 0;
        }
    }
}
