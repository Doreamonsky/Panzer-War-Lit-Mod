//#define UNITY_STANDALONE_WIN

using UnityEngine;
#if UNITY_STANDALONE_WIN 
using System.IO;
#endif

public class ServerConsole : MonoBehaviour
{
#if UNITY_STANDALONE_WIN

    ConsoleTestWindows.ConsoleWindow console = new ConsoleTestWindows.ConsoleWindow();
    ConsoleTestWindows.ConsoleInput input = new ConsoleTestWindows.ConsoleInput();
    readonly string strInput;

    //
    // Create console window, register callbacks
    //
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        console.Initialize();
        console.SetTitle(System.Environment.CommandLine);

        input.OnInputText += OnInputText;

        Application.RegisterLogCallback(HandleLog);

        Debug.Log("Console Started");
    }

    //
    // Text has been entered into the console
    // Run it as a console command
    //
    void OnInputText(string obj)
    {
        //ConsoleSystem.Run(obj, true);
    }

    //
    // Debug.Log* callback
    //
    void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Warning) //Ignore useless warning
        {
            return;
        }

        if (type == LogType.Warning)
        {
            System.Console.ForegroundColor = System.ConsoleColor.Yellow;
        }
        else if (type == LogType.Error)
        {
            System.Console.ForegroundColor = System.ConsoleColor.Red;
        }
        else
        {
            System.Console.ForegroundColor = System.ConsoleColor.White;
        }

        // We're half way through typing something, so clear this line ..
        if (System.Console.CursorLeft != 0)
        {
            input.ClearLine();
        }

        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            System.Console.WriteLine(message);
            System.Console.WriteLine(stackTrace);


            if (!File.Exists(ConsoleTestWindows.ConsoleWindow.StartTime + "Exceptions.txt"))
            {
                FileStream fs1 = new FileStream(ConsoleTestWindows.ConsoleWindow.StartTime + "Exceptions.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(string.Format("\n {0}:Message{1} StackTrace{2}", type.ToString(), message, stackTrace));
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(ConsoleTestWindows.ConsoleWindow.StartTime + "Exceptions.txt", FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(string.Format("\n {0}:Message{1} StackTrace{2}", type.ToString(), message, stackTrace));
                sr.Close();
                fs.Close();
            }

        }
        else
        {
            System.Console.WriteLine(message);
        }


        // If we were typing something re-add it.
        input.RedrawInputLine();
    }

    //
    // Update the input every frame
    // This gets new key input and calls the OnInputText callback
    //
    void Update()
    {
        input.Update();
    }

    //
    // It's important to call console.ShutDown in OnDestroy
    // because compiling will error out in the editor if you don't
    // because we redirected output. This sets it back to normal.
    //
    void OnDestroy()
    {
        console.Shutdown();
    }
#endif

}
