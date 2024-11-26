using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;
using System.Diagnostics;

using System;
using UnityEngine.UI;

using System.Linq;
using TMPro;

using System.Threading.Tasks;

public class Screen_Shots_01 : MonoBehaviour
{
    /*///////////////////////////////////////////////////
    //                 Все переменные                  //
    ///////////////////////////////////////////////////*/
    

    ////// Ссылки на объекты движка:

    public PlayerController_01 PlayerController_01; // Связь со скриптом, который управляет персонажем

    public RenderTexture LeftCam;       // Ссылки на RenderTexture с правой и левой камеры персонажа
    public RenderTexture RightCam;

    public GameObject DephMapImage;     // Ссылки на экраны для рендера в основном окне карты глубины,
    public GameObject OutLineTexture;   // Сжатой карты глубины
    public GameObject OutMarcerLine;    // И маркерной линии
    public GameObject Outp2DMapTex;     // Локальной карты
    public GameObject Outp2DMapTexOversize;     // Локальной карты

    public Text speedText;              // Текст, в котором отображается текущая скорость персонажа

    public Image Wall;                  // 4 изображения, отображающие текущие команды для персонажа
    public Image Arr_left;
    public Image Arr_right;
    public Image Em_Stop;

    public Text TPS;                    // Также как FPS

    public Color orangeColor;



    ////// Пути к файлам:

    string fileWayRoot = @"\Resources\"; // Корневая папка с доступом к скрипту, и папкам импорта

    string OpenCVScript;                 // 3 пути, к нужным нам папкам, в файловой системе
    string ExportImageWay;
    string ImportDephMapWay;
    string mainImportDephMapWay;

    Process process;                     // Метод процесса запуска скрипта. Объявляем его здесь, для того, что бы потом легко остановить

    public string newOpenCVScript;       // Дополнительный путь, к скрипу

    

    ////// Создание сжатой карту глубины:

    const int allCountPixels = 58; // Всего пикселей в выходном изображении
    float[] colArray = new float[allCountPixels]; // Массив значений - он и является сжатой картой глубины



    ////// Переменные управления:

    bool isShadowOn_In2DMap = true;    // Включены ли тени на локальной карте? 
                                        // Там алгоритм на 200+ строчек, он сильно снижает fps. Для отладки - проще отключить

    bool isDephTexMiddling = false;     // Мы усредняем текстуру карты глубины?

    bool isPrint = false;               // Для вывода отладочных сообщений

    public bool isLineScene = false;



    ////// Прочие переменные:

    bool isStartNewGame = true;         // Переменная, которая нужна, что бы обнулить предыдущую карту глубин
    bool isThisScriptOnWork = false;    // Был ли скрипт OpenCV запущен? Переменная нужна, для его корректной остановки
                                        //public TextMeshProUGUI debugText;


    // Все остальные переменные разнесены ниже, перед главными методами - что бы было проще с ними работать

    /*////////////////////////////////////////////////////
    //                   ОГЛАВЛЕНИЕ:                    //
    //                                                  //
    //  Начало программы                   110 строчка  //                                
    //  Start                              120 строчка  //                                
    //  Главный метод Update               200 строчка  //                                
    //  LOAD TEXTURE                       280 строчка  //                                
    //  Проверка наличия препятствий ----- 460 строчка  //                                
    //  Команды к персонажу                520 строчка  //                                
    //  OnPixelCompression 1               600 строчка  //                                
    //  OnPixelCompression 2 ------------- 720 строчка  //                                
    //  Generate2DMap                      800 строчка  //                                
    //  --- Конец программы ---           1400 строчка  //                                
    //                                                  //                                
    ////////////////////////////////////////////////////*/


    /*///////////////////////////////////////////////////
    //                Начало программы                 //
    ///////////////////////////////////////////////////*/

    private void Awake()
    {
        // Это разрешение экрана - для билда игры, что бы было проще отлаживать

        Screen.SetResolution(1280, 720, true); // Устанавливаем разрешение экрана
        Screen.fullScreen = false; // Устанавливаем режим оконного экрана
    }



    /*//////////////////////////////////////////////////
    //                     Start                      //
    //////////////////////////////////////////////////*/

    public void Start()
    {
        isThisScriptOnWork = true;

        // Один раз создаю белую текстуру
        CreateWhiteTexture();

        //print("____Start");

        // Тут мы прописываем пути, к папкам экспора, импорта, и к скрипту OpenCV
        if (File.Exists(Directory.GetCurrentDirectory() + @"\Diplom Project 01_Data" + fileWayRoot + @"MyC_02.exe"))
        {
            // Эти пути, если игра - уже билд
            //print("Файл существует по пути: " + newOpenCVScript);
            fileWayRoot = Directory.GetCurrentDirectory() + @"\Diplom Project 01_Data" + fileWayRoot;

            OpenCVScript = fileWayRoot + @"MyC_02.exe";
            ExportImageWay = fileWayRoot + @"_ExportImage\";
            ImportDephMapWay = fileWayRoot + @"_ImportDephMap\NewDephMap.png";

            newOpenCVScript = OpenCVScript;
        }
        else
        {
            // И эти пути, если игра ещё запускается в редакторе
            fileWayRoot = Directory.GetCurrentDirectory() + @"\Assets" + fileWayRoot;

            newOpenCVScript = fileWayRoot + @"MyC_02.exe";
            ExportImageWay = fileWayRoot + @"_ExportImage\";
            ImportDephMapWay = fileWayRoot + @"_ImportDephMap\NewDephMap.png";
        }

        mainImportDephMapWay = ImportDephMapWay;

        //debugText.text = newOpenCVScript;

        // Создаём и запускаем процесс - открытие файла .exe - который является скопмилированным скриптом OpenCV, для генерации карты глубин
        var processStartInfo = new ProcessStartInfo(newOpenCVScript);
        processStartInfo.WorkingDirectory = Path.GetDirectoryName(newOpenCVScript);
        process = Process.Start(processStartInfo); /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Проставляем значения картинкам на главном экране, соответствующим 4м выходным управляющим методам
        Wall.enabled = false;
        Arr_left.enabled = false;
        Arr_right.enabled = false;
        Em_Stop.enabled = false;

        CreateMarcerLine(); // Рисуем марекрную линию центрального препятствия
    }


    int marcerOffset = 6; // Задаю, насколько широкой будет центральная область

    void CreateMarcerLine()
    {
        Texture2D myNewTex = new Texture2D(allCountPixels, 1);

        Color32[] allWhitePixels = Enumerable.Repeat(new Color32(255, 255, 255, 255), myNewTex.width * myNewTex.height).ToArray();
        myNewTex.SetPixels32(allWhitePixels);
        myNewTex.Apply();

        // Задаю центральную зону
        // Если какой-либо образ линий появляется в этой области, значит персонаж обязательно столкнётся с препятствием, если продолжит движение
        for (int i = (allCountPixels / 2) - marcerOffset; i < (allCountPixels / 2) + marcerOffset; i++)
        {
            myNewTex.SetPixel(i, 1, orangeColor);
        }

        myNewTex.Apply();
        OutMarcerLine.GetComponent<RawImage>().texture = myNewTex;
    }

    // Один раз создаю белую текстуру
    void CreateWhiteTexture()
    {
        whiteTex = new Texture2D(allCountPixels_toLocalMap, 100);

        Color[] whitePixels = new Color[allCountPixels_toLocalMap * 100];
        Array.Fill(whitePixels, Color.white);

        whiteTex.SetPixels(whitePixels);
        whiteTex.Apply();
    }




    /*////////////////////////////////////////////////////
    //               Главный метод Update               //
    ////////////////////////////////////////////////////*/

    // Здесь используется таймер для того, что бы вызывать MyWorkCoroutine 5 раз в секунду, а не каждый кадр
    private float timer = 0.0f;
    private float interval = 0.2f; // Интервал в секундах (5 раз в секунду = каждые 0.2 секунды)

    public void FixedUpdate()
    {
        timer += Time.deltaTime;    // Увеличиваем таймер на время, прошедшее с последнего кадра

        if (timer >= interval)      // Если таймер достиг интервала
        {
            MyWorkCoroutine();      // Вызываем функцию
            timer = 0;              // Сбрасываем таймер
        }
    }




    // Для удобства, я перенёс весь код из Update в этот метод
    void MyWorkCoroutine()
    {
        string fileName = "CamTexture_01";

        //if (isPrint) print("Запустили генерацию карты глубин" + System.DateTime.Now.ToString("HH:mm:ss:ms"));
        //if (isPrint) print("Сохранили 2 картинки с камер" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        string l_way = ExportImageWay + "Left" + fileName;
        string r_way = ExportImageWay + "Right" + fileName;

        /////// Сохраняем левую и правую картинки с камер, в изображения 

        try
        {
            // Вот тут может возникнуть ошибка, если система не даёт доступа к изменению этих файлов
            // Но мы её игнорируем
            SaveTextureToFileUtility.SaveRenderTextureToFile(LeftCam, l_way);
            SaveTextureToFileUtility.SaveRenderTextureToFile(RightCam, r_way);
        }
        catch (Exception ex)
        {
            // При ошибке ничего не делаем, просто продолжаем выполнение программы
            //UnityEngine.Debug.Log("При сохранении изображения произошла ошибка");  //: " + ex);
            //MyWorkCoroutine(); // И запускаем эту процедуру ещё раз
            return;
        }

        //if (isPrint) print("Сохранили 2 картинки с камер" + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        // Копирую изображение карты глубины с 1 на конце названия, и дальше обрабатываю уже её

        //// Получаем информацию о пути к файлу
        //var directory = Path.GetDirectoryName(mainImportDephMapWay);
        //var fileName1 = Path.GetFileNameWithoutExtension(mainImportDephMapWay);
        //var extension = Path.GetExtension(mainImportDephMapWay);

        //// Создаем новое имя файла
        //var newFileName = $"{fileName1}1{extension}";

        //// Создаем новый путь к файлу
        //var newFilePath = Path.Combine(directory, newFileName);

        //// Копируем файл
        //File.Copy(mainImportDephMapWay, newFilePath, true);

        //// Обновляем переменную ImportDephMapWay новым путем
        //ImportDephMapWay = newFilePath;

        /////// Загружаем карту глубины, и производим с ней нужные манипуляции

        try
        {
            LoadTexture();  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// ВКЛЮЧИТЬ!
        }
        catch (Exception ex)
        {
            //UnityEngine.Debug.Log("При загрузке изображения произошла ошибка");  //: " + ex);
            //MyWorkCoroutine(); // И запускаем эту процедуру ещё раз
            return;
        }

        print("LoadTexture выполнилась");

        //if (isPrint) print("Обработка карты глубины завершена" + System.DateTime.Now.ToString("HH:mm:ss:ms"));
    }




    /*////////////////////////////////////////////////////
    //                   LOAD TEXTURE                   //
    ////////////////////////////////////////////////////*/

    // Эта переменная, для того, что бы обновлять локальную карту не каждый запуск, а раз в несколько запусков основной процедуры
    int countOnOffRet_Generate2DMap = 0;

    Texture2D buferDephTex; // Буферная текстура карты глубины, которая используется для усреднения

    float pervSummPixelOfDephMapTexture = 0;

    int stopCounter = 0; // Какое здесь число - столько раз пропустится загрузка карты глубины из файла

    void LoadTexture()
    {
        if (stopCounter > 0)
        {
            stopCounter--;
            print("Пропускаем загрузку карты глубины");
            return;
        }

        string imagePath = ImportDephMapWay; 
        
        // Загрузка изображения в экземпляр Texture2D
        Texture2D tex = new Texture2D(2, 2); // Создаем экземпляр Texture2D

        byte[] fileData = File.ReadAllBytes(imagePath); // Читаем данные изображения из файла

        tex.LoadImage(fileData); // Загружаем данные в Texture2D



        if (isStartNewGame)
        {
            // Если мы запускаем этот скрипт впервые в этой сессии, то нам нужно очистить предыдущие значения карты глубины
            isStartNewGame = false;

            return; // Перезапускаем загрузку карты глубины с начала
            // Благодаря этому, скрипт OpenCV успевает создать карту глубины, и в начале работы нашего алгоритма мы получаем чистую новую карту
        }



        // Здесь - проверка на то, нормальная ли текстура загрузилась
        // Я суммирую все значения пикселей из текущей текстуры, и сравниваю их с суммой значений суммы пикселей на прошлом шаге
        // Если разница > 20% - то эту текстуру не обрабатываем

        float sum = 0;
        Color[] pixels = tex.GetPixels();
        foreach (Color pixel in pixels)
        {
            sum += pixel.r;
        }

        if (pervSummPixelOfDephMapTexture == 0 || sum == 0)
        {
            pervSummPixelOfDephMapTexture = sum;
        }
        else
        {
            if (sum > 1000)
            {
                if (Mathf.Abs(sum - pervSummPixelOfDephMapTexture) / pervSummPixelOfDephMapTexture > 0.20f)
                {
                    //print("Ошибка карты глубины! Изображение некорректное. perv_summ = " + pervSummPixelOfDephMapTexture + ", sum = " + sum + ", разница: " + Mathf.Abs(sum - pervSummPixelOfDephMapTexture) / pervSummPixelOfDephMapTexture);
                    print("Ошибка карты глубины. Пропускаем этот тик");
                    pervSummPixelOfDephMapTexture = sum;

                    // if (buferDephTex != null) DephMapImage.GetComponent<RawImage>().texture = buferDephTex; // Устанавливаю буферное изображение в окошко вида карты глубины, но не обрабатываю его дальше
                    // Это некрасиво, по этому убрал

                    return;
                }                
            }

            // Если sum < 1000, то скорее всего мы смотрим на пустую карту глубины - тогда пропускаем накопившиеся ошибки 

            pervSummPixelOfDephMapTexture = sum;
        }


        if (buferDephTex == null)
        {
            // Если буферная карта пустая, то мы загружаем в неё текущую карту глубины

            buferDephTex = new Texture2D(2, 2);
            buferDephTex.LoadImage(fileData); // Загружаем данные в Texture2D
        }

        // Усреднение изображение карты глубины
        // Здесь мы берём пиксели из загруженной карты глубины, затем из буфера, складываем и делим по полам значение каждого пикселя
        // Это даёт эффект более плавного появления препятствий
        if (isDephTexMiddling)
        {
            // Предполагается, что tex и buferDephTex уже определены и имеют одинаковые размеры
            int width = tex.width;
            int height = tex.height;

            // Создаем новый массив цветов для хранения усредненных значений
            Color[] averageColors = new Color[width * height];

            // Получаем пиксели из обеих текстур
            Color[] texColors = tex.GetPixels();
            Color[] buferDephTexColors = buferDephTex.GetPixels();

            for (int i = 0; i < width * height; i++)
            {
                // Усредняем значения пикселей
                float averageR = (texColors[i].r + buferDephTexColors[i].r) / 2;
                float averageG = (texColors[i].g + buferDephTexColors[i].g) / 2;
                float averageB = (texColors[i].b + buferDephTexColors[i].b) / 2;

                // Сохраняем усредненный цвет в новом массиве
                averageColors[i] = new Color(averageR, averageG, averageB);
            }

            // Применяем новые цвета к текстуре tex
            tex.SetPixels(averageColors);
            tex.Apply();

            // Копирование текстуры из tex в buferDephTex
            Graphics.CopyTexture(tex, buferDephTex);
        }

        /// Вот здесь новый код, как я убираю всё до линии горизонта на карте глубины                          ////////////// НЕ ДОДЕЛАЛ. Доделать позже, если я хочу использовать текстуру на земле
        // Это для нахождения высоты точки, которую нуно вычитать из карты глубины, что бы пол не учитывался

        //float localAdder = 0;
        //int errorsCounter = 0;
        //float errorValue = 0.1f; // Значение ошибки цвета пикселя, до которого мы игнорируем его

        //for (int h = 0; h < tex.height; h++)
        //{
        //    for (int w = 0; w < tex.width; w++)
        //    {
        //        float currPixel = tex.GetPixel(w, h).r;

        //        localAdder = currPixel;
        //        localAdder = (float)Math.Round(localAdder, 2);

        //        if (h >= 18 && h <= 132)
        //        {
        //            double yCoeff = -0.005486725 * (h - 17) + 0.87548672;

        //            double adder_avg = localAdder - yCoeff;

        //            if (Math.Abs(adder_avg) < errorValue)
        //            {
        //                //print("Среднее значение по высоте " + h + ", не превышает допустимых отклонений, отклонение = " + adder_avg);

        //                // Устанавливаю этот пиксель в чёрный
        //                tex.SetPixel(w, h, Color.black);
        //            }
        //            else
        //            {
        //                // Не меняю значение пикселя - а стоит ли? - наверное нет
        //                //print("!!! По высоте " + h + " отклонение на " + adder_avg + ", yCoeff = " + yCoeff + ", localAdder = " + localAdder);
        //                errorsCounter++;
        //            }

        //        }
        //    }
        //}

        //print("В этом снимке карты глубины было " + errorsCounter + " ошибочных пикселей");

        //tex.Apply();

        ///

        print("Загрузили Карту глубины");

        DephMapImage.GetComponent<RawImage>().texture = tex; // Выгружаем изображение карты глубин в одно из окон, в интерфейсе программы

        //if (isPrint) print("Обновили текстуру " + System.DateTime.Now.ToString("HH:mm:ss:ms"));

        OnPixelCompression(tex); // Сжимаем текстуру карты глубины в вертикальные линии

        // Обновление локальной карты срабатывает кажды 3й вызов процедуры LoadTexture
        // Т.е. примерно 1-2 раза в секунду
        if (countOnOffRet_Generate2DMap < 2) countOnOffRet_Generate2DMap++;
        else
        {
            countOnOffRet_Generate2DMap = 0;
            OnPixelCompression2(tex); 
            Generate2DMap();
        }        

        CheckObstacles(); // Проверяем наличие препятствий, используя сжатую карту глубин
    }




    /*////////////////////////////////////////////////////
    //           Проверка наличия препятствий           //
    ////////////////////////////////////////////////////*/

    public bool thereIsObstacle = false;    // В центральной зоне есть препятствие?
    public bool isEmergyStop = false;       // Препятствие в центральной зоне угрожающе близко?

    // Значения, для определения, в какую сторонй следует поворачиваться
    public float leftNoiseValue;
    public float rightNoiseValue;

    public int whatTurn = 0; // Определяемся, в какую сторону будем поворачиваться

    // Если значение цвета пикселя на сжатой карте глубин больше
    float doorstepObst = 0.45f;  // этого значения - значит замедляем робота
    float emergyObstacle = 0.9f; // - значит останавливаем робота, он только вращается

    // Из рассчёта, что 1.0 - это белый пиксель, ближайшая точка по расстоянию к роботу

    void CheckObstacles() 
    {
        //print("Отдаём команды игроку из скрипта скриншота");

        thereIsObstacle = false;    // Требуется замедление
        isEmergyStop = false;       // Требуется полная остановка

        leftNoiseValue = 0f;
        rightNoiseValue = 0f;

        // Прохожу по всем значениям в массиве, которые попадают под маркер центрального препятствия
        for (int i = (allCountPixels / 2) - marcerOffset; i < (allCountPixels / 2) + marcerOffset; i++)
        {
            if (colArray[i] > doorstepObst) thereIsObstacle = true;     // Если есть любое препятствие
            if (colArray[i] > emergyObstacle) isEmergyStop = true;  // Если есть угрожающе близкое препятствие
        }

        if (thereIsObstacle == true) // Если препятствие всё-же есть, рассчитываю, в какой стороне меньше коэффициент близости препятствия
        {
            float bufer = 0;

            for (int i = 0; i < (allCountPixels / 2) - marcerOffset; i++)
            {
                bufer += colArray[i];
            }

            leftNoiseValue = (bufer / (allCountPixels / 2));                                                                                                  

            bufer = 0;

            for (int i = (allCountPixels / 2) + marcerOffset + 1; i < allCountPixels; i++)
            {
                bufer += colArray[i];
            }

            rightNoiseValue = (bufer / ((allCountPixels / 2) - 1));

            //print("Препятствие впереди! leftNoiseValue = " + leftNoiseValue + ", rightNoiseValue = " + rightNoiseValue);
        }

        if (Colliding_14_Player.isCollidingStaris == false)
        {
            // Отдаю команды, только если мы не находимся в зоне лестницы
            DodgingObstacles(); // Метод, в котором я, при необходимости, отдаю команды персонажу
        }
    }




    /*/////////////////////////////////////////////////////
    //                Команды к персонажу                //
    /////////////////////////////////////////////////////*/

    int emStopCounterInit = 2;
    public int emStopCounter = 2;       // Делаю задержку для полной остановки, так как на карте глубины могут всплывать быстро исчезающие шумы

    void DodgingObstacles()
    {
        bool wall_p = false;
        bool arr_l = false;
        bool arr_r = false;
        bool em_st = false;

        if (thereIsObstacle == false)
        {
            PlayerController_01.PlayerControl_StandartSpeedMove(); // Если препятствий нет
            whatTurn = 0;
        }
        else // Если они есть
        {
            if (isEmergyStop == true) // Если нужна полная остановка
            {
                if (emStopCounter > 0)
                {
                    emStopCounter--;
                    //print("Недостаточно данных для полной остановки");
                }
                else
                {
                    PlayerController_01.PlayerControl_EmergyStop(); // Если это всё-таки не помехи, и остановка действительно требуется
                    //print("Полная остановка выполнена");
                    em_st = true;
                }
            }
            else
            {
                PlayerController_01.PlayerControl_Suspend(); // Если будет достаточно только замедления
                wall_p = true;
            }

            if (whatTurn == 0) // Если направление поворота ещё не выбрано
            {
                if (leftNoiseValue > rightNoiseValue)
                {
                    //print("Поворачиваемя направо");
                    PlayerController_01.PlayerControl_TurnRight(1, 100); 
                    arr_r = true;
                }
                else
                {
                    //print("Поворачиваемя налево");
                    PlayerController_01.PlayerControl_TurnLeft(1, 100); 
                    arr_l = true;
                }
            }
        }

        if (isEmergyStop == false)
        {
            if (emStopCounter < emStopCounterInit) emStopCounter++;
        }

        speedText.text = "Скорость = " + (Math.Round(PlayerController_01.Speed, 1) * 4).ToString() + " км/ч"; // Также округляю для лучшего результата

        // Включаю и выключаю картинки, в главном окне:
        Wall.enabled = wall_p;
        Arr_left.enabled = arr_l;
        Arr_right.enabled = arr_r;
        Em_Stop.enabled = em_st;
    }




    /*/////////////////////////////////////////////////////
    //                OnPixelCompression 1               //
    /////////////////////////////////////////////////////*/

    int loadTimer = 10;

    // Главная процедура сжатия пикселей на карте глубины, которая в дальнейшем используется для проверки на препятствия спереди
    Texture2D OnPixelCompression(Texture2D texture)
    {
        int currCountUse = 0;           // Число рядов, которые мы прошли. 10 рядов суммируются в одну ячейку в массиве
        int currIndOfMass = 0;          // Индекс в массиве        
        int countNoisyPixels = 50; //20 // Сколько шумных пикселей мы пропустим в блоке, перед распознаванием самого большого значения //////////////////////// !!!!!! Протестировать на глобальной карте
        int noDetection = countNoisyPixels;

        //if (isLineScene == true)
        //{
        //    countNoisyPixels = 50;
        //}

        Array.Clear(colArray, 0, colArray.Length); // Сначала очищаем массив

        if (loadTimer > 0)
        {
            loadTimer--;
        }

        for (int w = 0; w < texture.width; w++)
        {
            float adder = 0;

            for (int h = 0; h < texture.height; h++)
                //for (int h = (texture.height / 2) + 10; h < texture.height; h++)
            {
                float currPixel = texture.GetPixel(w, h).r;

                if (noDetection > 0)
                {
                    if (currPixel > doorstepObst)
                    {
                        noDetection--;
                    }
                }
                else
                {
                    if (currPixel > colArray[currIndOfMass]) colArray[currIndOfMass] = currPixel;
                }
            }

            currCountUse++;

            if (currCountUse >= 10) // Если мы уже прошли 10 рядов, то переходим на заполнение следующей ячейки в массиве
            {
                currCountUse = 0;
                currIndOfMass++;
                noDetection = countNoisyPixels;
            }
        }

        //// Это для нахождения высоты точки, которую нуно вычитать из карты глубины, что бы пол не учитывался
        //// Этот код - для отладки. Он работает.
        //if (loadTimer == 0)
        //{
        //    float adder = 0;

        //    for (int h = 0; h < texture.height; h++)
        //    {              
        //        for (int w = 0; w < texture.width; w++) 
        //        {
        //            float currPixel = texture.GetPixel(w, h).r;

        //            adder += currPixel;
        //        }

        //        if (true) // Если мы уже прошли 10 строчек, то переходим на заполнение следующей ячейки в массиве
        //        {
        //            adder = (adder / texture.width);
        //            currCountUse_2 = 0;

        //            int th = (h);
        //            //margArr[w] = adder; // Сначала узнаю, когда по высоте начинается земля

        //            adder = (float)Math.Round(adder, 2);

        //            // Вот здесь функция которая убирает значения пола:

        //            if (h >= 18 && h <= 132)
        //            {
        //                double yCoeff = -0.005486725 * (h - 17) + 0.87548672;

        //                double adder_avg = adder - yCoeff;

        //                if (Math.Abs(adder_avg) < 0.05f)
        //                {
        //                    print("Среднее значение по высоте " + h + ", не превышает допустимых отклонений, yCoeff = " + yCoeff + ", adder = " + adder + ", adder_avg = " + adder_avg);
        //                }
        //                else 
        //                {
        //                    print("!!! По высоте " + h + " отклонение на " + adder_avg + ", yCoeff = " + yCoeff + ", adder = " + adder);
        //                }

        //            }

        //            //print("adder[" + th + "] = " + adder);
        //            adder = 0;
        //        }
        //    }

        //    loadTimer--;
        //}

        Texture2D myTex = new Texture2D(58, 1);

        for (int w1 = 0; w1 < 58; w1++)
        {
            Color newPixelColor = new Color(colArray[w1], colArray[w1], colArray[w1], 1f);
            myTex.SetPixel(w1, 0, newPixelColor);
        }

        myTex.Apply();

        OutLineTexture.GetComponent<RawImage>().texture = myTex;

        return myTex;
    }




    /*/////////////////////////////////////////////////////
    //                OnPixelCompression 2               //
    /////////////////////////////////////////////////////*/

    // Для локальной карты:
    const int allCountPixels_toLocalMap = 116;  // Всего пикселей в выходном изображении
    float[] colArray_toLocalMap = new float[allCountPixels_toLocalMap];
    float doorstep_toLocalMap = 0.1f;           // Порог распознавания препятствий. Чем значение меньше - тем раньше препятствие появится на текстуре из линий
    int countNoisyPixels_toLocalMap = 150;      // 75

    // Также сжимает карту глубины, но разрешение в 2 раза больше, и чуствительность - намного выше
    Texture2D OnPixelCompression2(Texture2D texture)
    {
        // Отличается от процедуры версии 1, что здесь в 2 раза больше ширина изображения - не 58, а 116 пикселей

        int currCountUse = 0;               // Число рядов, которые мы прошли. 10 рядов суммируются в одну ячейку в массиве
        int currIndOfMass = 0;              // Индекс в массиве        
        int noDetection = countNoisyPixels_toLocalMap;

        Array.Clear(colArray_toLocalMap, 0, colArray_toLocalMap.Length); // Сначала очищаем массив

        for (int w = 0; w < texture.width; w++)
        {
            for (int h = 0; h < texture.height; h++)
            //for (int h = (texture.height / 2) + 10; h < texture.height; h++)
            {
                float currPixel = texture.GetPixel(w, h).r;

                if (noDetection > 0)
                {
                    if (currPixel > doorstep_toLocalMap)
                    {
                        noDetection--;
                    }
                }
                else
                {
                    if (currPixel > colArray_toLocalMap[currIndOfMass]) colArray_toLocalMap[currIndOfMass] = currPixel;
                }
            }

            currCountUse++;

            if (currCountUse >= 5) // Если мы уже прошли 5 рядов, то переходим на заполнение следующей ячейки в массиве
            {
                currCountUse = 0;
                currIndOfMass++;

                noDetection = countNoisyPixels_toLocalMap;
            }
        }

        Texture2D myTex = new Texture2D(116, 1);

        for (int w1 = 0; w1 < 116; w1++)
        {
            Color newPixelColor = new Color(colArray_toLocalMap[w1], colArray_toLocalMap[w1], colArray_toLocalMap[w1], 1f);
            myTex.SetPixel(w1, 0, newPixelColor);
        }

        myTex.Apply();

        //OutLineTexture.GetComponent<RawImage>().texture = myTex; // Здесь она вставляется в окно в редакторе
        return myTex;
    }




    /*////////////////////////////////////////////////////
    //                   Generate2DMap                  //
    ////////////////////////////////////////////////////*/

    Texture2D resultTex;

    float massMultipler = 2.6f; // Задаёт коэффициент, для перевода значения цвета пикселя, в расстояние до преграды, в метрах 
    // Этот коэффициент точный

    // Создаем матрицу 116*100, где 116 - это количество столбцов, а 100 - это шкала от 0 до 10 метров (расстояние до препятствия)
    bool[,] matrix = new bool[allCountPixels_toLocalMap, 100];

    Texture2D whiteTex; // Белая текстура, что бы каждый раз не заполнять её заново // Я создаю её в методе Start

    // Получает массив значений цветов сжатых пикселей colArray
    // Затем, преобразует значение цвета в расстояние до объекта, в метрах
    // Дальше, выгружает точку расстояния в матрицу, и эту матрицу преобразовывает в локальную карту
    void Generate2DMap()
    {
        matrix = new bool[allCountPixels_toLocalMap, 100]; // Очищаю матрицу

        // Создаю рабочую текстуру мини-карты
        resultTex = new Texture2D(116, 100);

        resultTex.SetPixels(whiteTex.GetPixels());
        resultTex.Apply();

        // Преобразую значения цвета сжатых пикселей, в расстояние до объекта
        for (int i = 0; i < colArray_toLocalMap.Length; i++)
        {
            if (colArray_toLocalMap[i] != 0)
            {
                colArray_toLocalMap[i] = (1 / (colArray_toLocalMap[i])) * massMultipler;
                colArray_toLocalMap[i] = (float)Math.Round(colArray_toLocalMap[i], 2);    // Округляю до 2х знаков, после запятой, что бы было проще считать это как метры
            }
        }

        // Прохожусь по массиву расстояний, выгружаю его в матрицу, с шагом в 0.1 метр. Затем, матрицу преобразую в изображение
        for (int i = 0; i < colArray_toLocalMap.Length; i++)
        {
            int index = (int)Math.Round(colArray_toLocalMap[i] * 10); // Приводим значения float от 0 до 10 к шкале значений int от 0 до 100

            if ((index > 0) && (index < 100))
            {
                int j = index;
                //j = 50; // Для тестирования Sin функции

                // переводим значения из массива в матрицу: В матрице устанавливаем пиксель = true
                {
                    matrix[i, j] = true; // Устанавливаем соответствующий пиксель в true
                }

                // Изменяем линию горизонта на карте с прямой на закруглённую = 60 градусам, как fov у камеры
                {
                    // Преобразование индекса i в градусы от 60 до 120, затем в радианы
                    double angleInRadians = ((i * 60 / 115.0) + 60) * Math.PI / 180.0;

                    //double angleInRadians = (i * 180 / 57.0) * Math.PI / 180.0; // Преобразование к дапазону от 0 до 180 градусов

                    double curSin = Math.Sin(angleInRadians);

                    curSin = Math.Round(curSin, 2); // Округляю, для упрощения вычислений

                    int newVal = 100 - (j - (int)(j * curSin));
                    newVal = newVal - (100 - j);

                    matrix[i, j] = false;

                    if (newVal >= 0 && newVal < 100)
                    {
                        matrix[i, newVal] = true;
                        //resultTex.SetPixel(i, newVal, Color.black); // Если значение в матрице true, устанавливаем пиксель в черный цвет
                    }
                }
            }
        }

        //// Версия без фильтрации одиноких точек
        //for (int i = 0; i < colArray_toLocalMap.Length; i++)
        //{
        //    for (int j = 0; j < 100; j++)
        //    {
        //        if (matrix[i, j] == true)
        //        {
        //            resultTex.SetPixel(i, j, Color.black); // Если значение в матрице true, устанавливаем пиксель в черный цвет
        //        }
        //    }
        //}

        //int leavePixels = 0;
        int dimm = 2;

        // Фильтрую одиного стоящие точки (у которых в радиусе квадрата 5х5 нет других чёрных точек) - удаляю их
        for (int i = 0; i < colArray_toLocalMap.Length; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                if (matrix[i, j] == true)
                {
                    bool hasNeighbor = false;

                    // Проверяем соседние ячейки в радиусе 2х ячеек
                    for (int di = i - dimm; di <= i + dimm; di++)
                    {
                        for (int dj = j - dimm; dj <= j + dimm; dj++)
                        {
                            // Проверяем, что не выходим за границы матрицы
                            if (di >= 0 && di < colArray_toLocalMap.Length && dj >= 0 && dj < 100)
                            {
                                // Условие для того, что бы центральная точка не проверялась
                                if ((di != i) && (dj != j)) 
                                {
                                    // Если находим соседнюю ячейку со значением true, то устанавливаем флаг hasNeighbor в true
                                    if (matrix[di, dj] == true)
                                    {
                                        hasNeighbor = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (hasNeighbor)
                        {
                            break;
                        }
                    }

                    // Если есть соседняя ячейка со значением true, то устанавливаем пиксель в черный цвет
                    if (hasNeighbor)
                    {
                        resultTex.SetPixel(i, j, Color.black);
                    }
                    //else
                    //{
                    //    leavePixels++;
                    //}
                }
            }
        }

        //if (leavePixels > 0)
        //{
        //    print("Пропустили пикселей: " + leavePixels);
        //}

        Texture2D resultTexOversize;

        int resultTexWidth = resultTex.width;
        int resultTexHeight = resultTex.height;

        // Создаём текстуру с увеличенным резмером
        {
            // Создаем новую текстуру с добавлением белых пикселей
            resultTexOversize = new Texture2D(resultTexWidth + 58 * 2, resultTexHeight + 23);

            // Заполняем новую текстуру белыми пикселями
            for (int y = 0; y < resultTexOversize.height; ++y)
            {
                for (int x = 0; x < resultTexOversize.width; ++x)
                {
                    resultTexOversize.SetPixel(x, y, Color.white);
                }
            }

            // Копируем пиксели из исходной текстуры в новую
            for (int y = 0; y < resultTexHeight; ++y)
            {
                for (int x = 0; x < resultTexWidth; ++x)
                {
                    resultTexOversize.SetPixel(x + 58, y, resultTex.GetPixel(x, y));
                }
            }

            // Применяем изменения к текстуре
            resultTexOversize.Apply();
        }

        int resultTexOversizeWidth = resultTexOversize.width;
        int resultTexOversizeHeight = resultTexOversize.height;


        // Тень с увеличенным размером изображения
        if (isShadowOn_In2DMap)
        {
            Color shadowColor = new Color(0.7f, 0.7f, 0.7f); // серый цвет для тени

            // Рисуем тень
            for (int i = 0; i < resultTexOversizeWidth; i++)
            {
                for (int j = 0; j < resultTexOversizeHeight; j++)
                {
                    Color pixelColor = resultTexOversize.GetPixel(i, j);

                    if (pixelColor == Color.black) // если пиксель является объектом
                    {
                        float dx = i - (resultTexOversizeWidth / 2); // Координата источника света по X
                        float dy = j - 0; // Координата источника света по Y

                        float angle = Mathf.Atan2(dy, dx);

                        int shadowLength = resultTexOversizeWidth + resultTexOversizeHeight;

                        // рисуем тень
                        for (int k = 2; k <= shadowLength; k++)
                        {
                            int shadowX = i + (int)(k * Mathf.Cos(angle));
                            int shadowY = j + (int)(k * Mathf.Sin(angle));

                            // проверяем, что координаты тени не выходят за пределы изображения
                            if (shadowX >= 0 && shadowX < resultTexOversizeWidth && shadowY >= 0 && shadowY < resultTexOversizeHeight)
                            {
                                resultTexOversize.SetPixel(shadowX, shadowY, shadowColor);

                                float expansionCoefficient = 0.4f; // коэффициент расширения тени

                                // рисуем расширяющуюся тень слева и справа от основной линии тени
                                for (int l = 1; l <= Math.Ceiling(k * expansionCoefficient); l++)
                                {
                                    int shadowLeftX = shadowX - (int)Math.Ceiling(l * Mathf.Sin(angle));
                                    int shadowLeftY = shadowY + (int)Math.Ceiling(l * Mathf.Cos(angle));

                                    int shadowRightX = shadowX + (int)Math.Ceiling(l * Mathf.Sin(angle));
                                    int shadowRightY = shadowY - (int)Math.Ceiling(l * Mathf.Cos(angle));

                                    // проверяем, что координаты тени не выходят за пределы изображения
                                    if (shadowLeftX >= 0 && shadowLeftX < resultTexOversizeWidth && shadowLeftY >= 0 && shadowLeftY < resultTexOversizeHeight)
                                    {
                                        // Проверяю, что я не закрашиваю тенью чёрный пиксель
                                        if (resultTexOversize.GetPixel(shadowLeftX, shadowLeftY) != Color.black)
                                        {
                                            resultTexOversize.SetPixel(shadowLeftX, shadowLeftY, shadowColor);
                                        }
                                    }

                                    if (shadowRightX >= 0 && shadowRightX < resultTexOversizeWidth && shadowRightY >= 0 && shadowRightY < resultTexOversizeHeight)
                                    {
                                        if (resultTexOversize.GetPixel(shadowLeftX, shadowLeftY) != Color.black)
                                        {
                                            resultTexOversize.SetPixel(shadowRightX, shadowRightY, shadowColor);
                                        }
                                    }
                                }
                            }
                            else if (shadowX >= resultTexOversizeWidth && shadowY >= resultTexOversizeHeight)
                            {
                                // Выхожу из цикла только тогда, когда тень полностью нарисована
                                break;
                            }
                        }
                    }
                }
            }

            // Находим выбитые пиксели тени, и закрашиваем их
            for (int i = 0; i < resultTexOversizeWidth; i++)
            {
                for (int j = 0; j < resultTexOversizeHeight; j++)
                {
                    int rCount = 0; // Количество теневых пикселей рядом с этим
                    int dimm2 = 1;  // Радиус поиска теневых пикселей

                    if (resultTexOversize.GetPixel(i, j) == Color.white)
                    {
                        //print("Нашли белый пиксель");

                        // Проверяем соседние ячейки в радиусе 1 ячейки
                        for (int di = i - dimm2; di <= i + dimm2; di++)
                        {
                            for (int dj = j - dimm2; dj <= j + dimm2; dj++)
                            {
                                // Проверяем, что не выходим за границы матрицы
                                if (di >= 0 && di < resultTexOversizeWidth && dj >= 0 && dj < resultTexOversizeHeight)
                                {
                                    float tolerance = 0.01f; // допуск

                                    Color pixelColor = resultTexOversize.GetPixel(di, dj);
                                    if (Mathf.Abs(pixelColor.r - shadowColor.r) <= tolerance &&
                                        Mathf.Abs(pixelColor.g - shadowColor.g) <= tolerance &&
                                        Mathf.Abs(pixelColor.b - shadowColor.b) <= tolerance)
                                    {
                                        rCount++;
                                        //print("Соседняя точка - теневая");
                                        //print("pixelColor.r = " + pixelColor.r + ", pixelColor.g = " + pixelColor.g + ", pixelColor.b = " + pixelColor.b);

                                        if (rCount >= 4)
                                        {
                                            //print("Нашли теневую незакрашенную точку");
                                            resultTexOversize.SetPixel(i, j, shadowColor);

                                            di = i + dimm2 + 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Тени для маленькой карты
        {

            //if (isShadowOn_In2DMap)
            //{
            //    Color shadowColor = new Color(0.7f, 0.7f, 0.7f); // серый цвет для тени

            //    // Рисуем тень
            //    for (int i = 0; i < resultTexWidth; i++)
            //    {
            //        for (int j = 0; j < resultTexHeight; j++)
            //        {
            //            Color pixelColor = resultTex.GetPixel(i, j);

            //            if (pixelColor == Color.black) // если пиксель является объектом
            //            {
            //                float dx = i - (resultTexWidth / 2); // lightSourceX - координата источника света по X
            //                float dy = j - 0; // lightSourceY - координата источника света по Y

            //                float angle = Mathf.Atan2(dy, dx);

            //                int shadowLength = resultTexWidth + resultTexHeight;

            //                // рисуем тень
            //                for (int k = 2; k <= shadowLength; k++)
            //                {
            //                    int shadowX = i + (int)(k * Mathf.Cos(angle));
            //                    int shadowY = j + (int)(k * Mathf.Sin(angle));

            //                    // проверяем, что координаты тени не выходят за пределы изображения
            //                    if (shadowX >= 0 && shadowX < resultTexWidth && shadowY >= 0 && shadowY < resultTexHeight)
            //                    {
            //                        resultTex.SetPixel(shadowX, shadowY, shadowColor);

            //                        float expansionCoefficient = 0.4f; // коэффициент расширения тени

            //                        // рисуем расширяющуюся тень слева и справа от основной линии тени
            //                        for (int l = 1; l <= Math.Ceiling(k * expansionCoefficient); l++)
            //                        {
            //                            int shadowLeftX = shadowX - (int)Math.Ceiling(l * Mathf.Sin(angle));
            //                            int shadowLeftY = shadowY + (int)Math.Ceiling(l * Mathf.Cos(angle));

            //                            int shadowRightX = shadowX + (int)Math.Ceiling(l * Mathf.Sin(angle));
            //                            int shadowRightY = shadowY - (int)Math.Ceiling(l * Mathf.Cos(angle));

            //                            // проверяем, что координаты тени не выходят за пределы изображения
            //                            if (shadowLeftX >= 0 && shadowLeftX < resultTexWidth && shadowLeftY >= 0 && shadowLeftY < resultTexHeight)
            //                            {
            //                                // Проверяю, что я не закрашиваю тенью чёрный пиксель
            //                                if (resultTex.GetPixel(shadowLeftX, shadowLeftY) != Color.black)
            //                                {
            //                                    resultTex.SetPixel(shadowLeftX, shadowLeftY, shadowColor);
            //                                }
            //                            }

            //                            if (shadowRightX >= 0 && shadowRightX < resultTexWidth && shadowRightY >= 0 && shadowRightY < resultTexHeight)
            //                            {
            //                                if (resultTex.GetPixel(shadowLeftX, shadowLeftY) != Color.black)
            //                                {
            //                                    resultTex.SetPixel(shadowRightX, shadowRightY, shadowColor);
            //                                }
            //                            }
            //                        }
            //                    }
            //                    else if (shadowX >= resultTexWidth && shadowY >= resultTexHeight)
            //                    {
            //                        // Выхожу из цикла только тогда, когда тень полностью нарисована
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    // Находим выбитые пиксели тени, и закрашиваем их
            //    for (int i = 0; i < colArray_toLocalMap.Length; i++)
            //    {
            //        for (int j = 0; j < 100; j++)
            //        {
            //            int rCount = 0; // Количество теневых пикселей рядом с этим
            //            int dimm2 = 1;  // Радиус поиска теневых пикселей

            //            if (resultTex.GetPixel(i, j) == Color.white)
            //            {
            //                //print("Нашли белый пиксель");

            //                // Проверяем соседние ячейки в радиусе 1 ячейки
            //                for (int di = i - dimm2; di <= i + dimm2; di++)
            //                {
            //                    for (int dj = j - dimm2; dj <= j + dimm2; dj++)
            //                    {
            //                        // Проверяем, что не выходим за границы матрицы
            //                        if (di >= 0 && di < colArray_toLocalMap.Length && dj >= 0 && dj < 100)
            //                        {
            //                            float tolerance = 0.01f; // допуск

            //                            Color pixelColor = resultTex.GetPixel(di, dj);
            //                            if (Mathf.Abs(pixelColor.r - shadowColor.r) <= tolerance &&
            //                                Mathf.Abs(pixelColor.g - shadowColor.g) <= tolerance &&
            //                                Mathf.Abs(pixelColor.b - shadowColor.b) <= tolerance)
            //                            {
            //                                rCount++;
            //                                //print("Соседняя точка - теневая");
            //                                //print("pixelColor.r = " + pixelColor.r + ", pixelColor.g = " + pixelColor.g + ", pixelColor.b = " + pixelColor.b);

            //                                if (rCount >= 4)
            //                                {
            //                                    //print("Нашли теневую незакрашенную точку");
            //                                    resultTex.SetPixel(i, j, shadowColor);

            //                                    di = i + dimm2 + 1;
            //                                    break;
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        // Многопоточность - не работает как надо
        {
            /*
        
        if (isShadowOn_In2DMap)
        {
            Color shadowColor = new Color(0.7f, 0.7f, 0.7f); // серый цвет для тени

            // Преобразование текстуры в матрицу
            int[,] matrix_texture = new int[resultTex.width, resultTex.height];
            for (int i = 0; i < resultTex.width; i++)
            {
                for (int j = 0; j < resultTex.height; j++)
                {
                    Color pixelColor = resultTex.GetPixel(i, j);
                    if (pixelColor == Color.white)
                        matrix_texture[i, j] = 0;
                    else if (pixelColor == shadowColor)
                        matrix_texture[i, j] = 1;
                    else if (pixelColor == Color.black)
                        matrix_texture[i, j] = 2;
                }
            }

            //// Выполнение операций в параллельных потоках
            //Parallel.For(0, resultTex.width, i =>
            //{
            //    for (int j = 0; j < resultTex.height; j++)
            //    {
            //        // ваш код, но теперь используйте matrix[i, j] вместо resultTex.GetPixel(i, j)
            //    }
            //});

            int resultTexWidth = resultTex.width;
            int resultTexHeight = resultTex.height;

            Parallel.For(0, resultTexWidth, i =>
            {
                print("i = " + i);
                for (int j = 0; j < resultTexHeight; j++)
                {
                    int pixelColor_int = matrix_texture[i, j];

                    if (pixelColor_int == 2) // если пиксель является объектом
                    {
                        float dx = i - (resultTexWidth / 2); // lightSourceX - координата источника света по X
                        float dy = j - 0; // lightSourceY - координата источника света по Y

                        float angle = Mathf.Atan2(dy, dx);

                        int shadowLength = resultTexWidth + resultTexHeight;

                        // рисуем тень
                        for (int k = 2; k <= shadowLength; k++)
                        {
                            int shadowX = i + (int)(k * Mathf.Cos(angle));
                            int shadowY = j + (int)(k * Mathf.Sin(angle));

                            // проверяем, что координаты тени не выходят за пределы изображения
                            if (shadowX >= 0 && shadowX < resultTexWidth && shadowY >= 0 && shadowY < resultTexHeight)
                            {
                                matrix_texture[shadowX, shadowY] = 1;

                                float expansionCoefficient = 0.4f; // коэффициент расширения тени

                                // рисуем расширяющуюся тень слева и справа от основной линии тени
                                for (int l = 1; l <= Math.Ceiling(k * expansionCoefficient); l++)
                                {
                                    int shadowLeftX = shadowX - (int)Math.Ceiling(l * Mathf.Sin(angle));
                                    int shadowLeftY = shadowY + (int)Math.Ceiling(l * Mathf.Cos(angle));

                                    int shadowRightX = shadowX + (int)Math.Ceiling(l * Mathf.Sin(angle));
                                    int shadowRightY = shadowY - (int)Math.Ceiling(l * Mathf.Cos(angle));

                                    // проверяем, что координаты тени не выходят за пределы изображения
                                    if (shadowLeftX >= 0 && shadowLeftX < resultTexWidth && shadowLeftY >= 0 && shadowLeftY < resultTexHeight)
                                    {
                                        // Проверяю, что я не закрашиваю тенью чёрный пиксель
                                        if (matrix_texture[shadowLeftX, shadowLeftY] != 2)
                                        {
                                            matrix_texture[shadowLeftX, shadowLeftY] = 1;
                                        }
                                    }

                                    if (shadowRightX >= 0 && shadowRightX < resultTexWidth && shadowRightY >= 0 && shadowRightY < resultTexHeight)
                                    {
                                        if (matrix_texture[shadowLeftX, shadowLeftY] != 2) 
                                        {
                                            matrix_texture[shadowRightX, shadowRightY] = 1;
                                        }
                                    }
                                }
                            }
                            else if (shadowX >= resultTexWidth && shadowY >= resultTexHeight)
                            {
                                // Выхожу из цикла только тогда, когда тень полностью нарисована
                                break;
                            }
                        }
                    }
                }
            });

            // Находим выбитые пиксели тени, и закрашиваем их
            for (int i = 0; i < colArray_toLocalMap.Length; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    int rCount = 0; // Количество теневых пикселей рядом с этим
                    int dimm2 = 1;  // Радиус поиска теневых пикселей

                    if (matrix_texture[i, j] == 0)
                    {
                        //print("Нашли белый пиксель");

                        // Проверяем соседние ячейки в радиусе 1 ячейки
                        for (int di = i - dimm2; di <= i + dimm2; di++)
                        {
                            for (int dj = j - dimm2; dj <= j + dimm2; dj++)
                            {
                                // Проверяем, что не выходим за границы матрицы
                                if (di >= 0 && di < colArray_toLocalMap.Length && dj >= 0 && dj < 100)
                                {
                                    if (matrix_texture[di, dj] == 1)
                                    {
                                        rCount++;
                                        //print("Соседняя точка - теневая");
                                        //print("pixelColor.r = " + pixelColor.r + ", pixelColor.g = " + pixelColor.g + ", pixelColor.b = " + pixelColor.b);

                                        if (rCount >= 4)
                                        {
                                            //print("Нашли теневую незакрашенную точку");
                                            matrix_texture[i, j] = 1;


                                            di = i + dimm2 + 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Преобразование матрицы обратно в текстуру
            for (int i = 0; i < resultTex.width; i++)
            {
                for (int j = 0; j < resultTex.height; j++)
                {
                    if (matrix_texture[i, j] == 0)
                        resultTex.SetPixel(i, j, Color.white);
                    else if (matrix_texture[i, j] == 1)
                        resultTex.SetPixel(i, j, shadowColor);
                    else if (matrix_texture[i, j] == 2)
                        resultTex.SetPixel(i, j, Color.black);
                }
            }
        }
            */
        }



        // Применяем изменения к текстуре
        resultTex.Apply();
        resultTexOversize.Apply();

        // Изменяю сглаживание, что бы оно было точечным, а не размытым
        resultTex.filterMode = FilterMode.Point;
        resultTexOversize.filterMode = FilterMode.Point;

        // Вставляю эту текстуру в объект на экране
        //Outp2DMapTex.GetComponent<RawImage>().texture = resultTex;

        Outp2DMapTexOversize.GetComponent<RawImage>().texture = resultTexOversize;

        //print("Отрендерили изображение карты");
    }




    void OnApplicationQuit() // При закрытии игры, не забываем отключить скрипт генерации карту глубин
    {
        // Если что, этот метод выполняется всегда, даже когда этот скрипт отключён

        if (isThisScriptOnWork == true)
        {
            //print("____End");
            process.Kill();
            //process.CloseMainWindow();
            //process.Close();
            process.WaitForExit(); // Ожидание завершения процесса
        }
    }
}
