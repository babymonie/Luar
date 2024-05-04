using System;
using System.Drawing;
using NLua;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using LuaRuntime;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;

using System.Text;
using System.Security.Principal;
using System.IO;
namespace LuaRuntime;
static class ProcessControl
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = 0x0001,
        SUSPEND_RESUME = 0x0002,
        GET_CONTEXT = 0x0008,
        SET_CONTEXT = 0x0010,
        SET_INFORMATION = 0x0020,
        QUERY_INFORMATION = 0x0040,
        SET_THREAD_TOKEN = 0x0080,
        IMPERSONATE = 0x0100,
        DIRECT_IMPERSONATION = 0x0200
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    public static void SuspendProcess(int pid)
    {
        var process = Process.GetProcessById(pid);
        foreach (ProcessThread pT in process.Threads)
        {
            IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                continue;
            }
            SuspendThread(pOpenThread);
            CloseHandle(pOpenThread);
        }
    }

    public static void ResumeProcess(int pid)
    {
        var process = Process.GetProcessById(pid);
        if (process.ProcessName == string.Empty) return;

        foreach (ProcessThread pT in process.Threads)
        {
            IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                continue;
            }
            var suspendCount = 0;
            do
            {
                suspendCount = ResumeThread(pOpenThread);
            } while (suspendCount > 0);
            CloseHandle(pOpenThread);
        }
    }
}
internal class Program
{
    static void Main(string[] args)
    {

        //setup environment by adding app to path

        string pathVariable = Environment.GetEnvironmentVariable("PATH") ?? "";


        if (!pathVariable.Contains(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)) && new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            pathVariable += ";" + Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // Set the modified PATH variable
            Environment.SetEnvironmentVariable("PATH", pathVariable, EnvironmentVariableTarget.Machine);

            // Notify the user or log that the directory has been added to PATH
            Console.WriteLine($"{Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)} has been added to the PATH variable.");

            Console.WriteLine("Please restart the terminal to apply the changes.");
        }
        

        if (!System.IO.Directory.Exists("libs"))
        {
            System.IO.Directory.CreateDirectory("libs");
        }

        PackageManager packageManager = new PackageManager();

        if (args.Length == 1 && args[0] == "install")
        {
            Console.WriteLine("As you know we use github to download packages, so please enter the package name in the format of 'username/repo' without quotes.");
            string packageName = Console.ReadLine();
            packageManager.InstallPackage(packageName);
            return;
        }
        else if (args.Length == 2 && args[0] == "install")
        {
            packageManager.InstallPackage(args[1]);
            return;
        }

        //uninstall 
        if (args.Length == 1 && args[0] == "uninstall")
        {
            Console.WriteLine("As you know we use github to download packages, so please enter the package name");
            string packageName = Console.ReadLine();
            packageManager.UninstallPackage(packageName);
            return;
        }
        else if (args.Length == 2 && args[0] == "uninstall")
        {
            packageManager.UninstallPackage(args[1]);
            return;
        }
       

        else if (args.Length == 1 && args[0].EndsWith(".lua"))
        {
            RunScript(args[0]);
        }
        else
        {
            Console.WriteLine("Lua Runtime Interpreter");
            Console.WriteLine("Type 'exit' to exit the interpreter");
            RuntimeManager runtimeManager = new RuntimeManager();
            Lua lua = runtimeManager.GetLuaInstance();
            lua.LoadCLRPackage();

            runtimeManager.RegisterClassesAndFunctions();
            runtimeManager.LoadExtensionsFromDirectory("libs");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (input == "exit")
                {
                    break;
                }
                else if (input == "clear")
                {
                    Console.Clear();
                }
                else
                {
                    try
                    {
                        lua.DoString(input);
                    }
                    catch (Exception e)
                    {
           

                        if (e.Message.Contains(":"))
                        {
                            Console.WriteLine(e.Message.Substring(e.Message.IndexOf(':') + 1));
                        }
                        else
                        {
                            Console.WriteLine(e.Message);
                        }



                    }

                }
            }
        }
    }

    static void RunScript(string fileName)
    {

        RuntimeManager runtimeManager = new RuntimeManager();
        Lua lua = runtimeManager.GetLuaInstance();
        lua.LoadCLRPackage();

        runtimeManager.RegisterClassesAndFunctions();
        runtimeManager.LoadExtensionsFromDirectory("libs");
        lua.DoFile(fileName);

    }
}

public interface IRuntimeExtension
{
    void RegisterClassesAndFunctions(RuntimeManager runtimeManager);

}


public class RuntimeManager 
{

    private Lua luaS;
    private RuntimeFunctions runtimeFunctions = new RuntimeFunctions();

    public RuntimeManager(Lua lua = null)
    {
        luaS = lua ?? new Lua(); // If no Lua instance is provided, create a new one
    }

    public Lua GetLuaInstance()
    {
        return luaS;
    }


    public void registerClass(Object obj, string name)
    {
        luaS[name] = obj;

      
    }
    public void RegisterExtension(IRuntimeExtension extension)
    {
        extension.RegisterClassesAndFunctions(this);
    }
    public void LoadExtensionsFromDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("Directory does not exist.");
            return;
        }

        string[] dllFiles = Directory.GetFiles(directoryPath, "*.dll");

        foreach (string dllFile in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);

                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IRuntimeExtension).IsAssignableFrom(type))
                    {
                        IRuntimeExtension extension = Activator.CreateInstance(type) as IRuntimeExtension;
                        extension?.RegisterClassesAndFunctions(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading DLL '{dllFile}': {ex.Message}");
            }
        }
    }
    public void RegisterClassesAndFunctions()
    {
        // Load the CLR package to enable interaction with .NET types
        luaS.LoadCLRPackage();

        RuntimeFunctions runtimeClasses = new RuntimeFunctions();

        registerClass(new FS(), "fs");
        registerClass(new ProcessesByTheDay(), "proc");
        registerClass(new Keyboard(), "keyboard");
        registerClass(new Request(), "Request");
        registerClass(new JSON(), "JSON");
        registerClass(new convert(), "Convert");
        registerClass(new LuaHttpServer(), "HttpServer");
        registerClass(new Mouse(), "Mouse");
        registerClass(new SocketServer(), "SocketServer");
        registerClass(new SocketClient(), "SocketClient");





        RegisterFunction("sleep", new Action<int>(runtimeClasses.sleep));
        RegisterFunction("SetInterval", new Action<Action, int>(runtimeClasses.SetInterval));
        RegisterFunction("SetTimeout", new Action<Action, int>(runtimeClasses.SetTimeout));
        RegisterFunction("WriteAndInput", new Action<string>(runtimeClasses.WriteAndInput));
        RegisterFunction("Input", new Action(runtimeClasses.Input));
        RegisterFunction("Clear", new Action(runtimeClasses.Clear));
        RegisterFunction("Exit", new Action(runtimeClasses.Exit));
       









    }  
   
    public void RegisterFunction<T>(string name, Action<T> action)
    {
        MethodInfo methodInfo = runtimeFunctions.GetType().GetMethod(name);

        if (methodInfo != null)
        {
            luaS.RegisterFunction(name, this, methodInfo);

        }
        else
        {
            Console.WriteLine("Method " + name + " not found");
            return;
        }
    }

    public void RegisterFunction(string name, Action action)
    {
        MethodInfo methodInfo = runtimeFunctions.GetType().GetMethod(name);

        if (methodInfo != null)
        {
            luaS.RegisterFunction(name, this, methodInfo);
        }
        else
        {
            Console.WriteLine("Method " + name + " not found");
            return;
        }
    }
    public void RegisterFunction(string name, Action<Action, int> action)
    {
        MethodInfo methodInfo = runtimeFunctions.GetType().GetMethod(name);

        if (methodInfo != null)
        {
            luaS.RegisterFunction(name, this, methodInfo);

        }
        else
        {
            Console.WriteLine("Method " + name + " not found");
            return;
        }

    }
    public class FS
    {
        public string ReadFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public void WriteFile(string path, string content)
        {
            System.IO.File.WriteAllText(path, content);
        }

        public void AppendFile(string path, string content)
        {
            System.IO.File.AppendAllText(path, content);
        }

        public void DeleteFile(string path)
        {
            System.IO.File.Delete(path);
        }

        public void CreateDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            System.IO.Directory.Delete(path);
        }

        public static void CopyFile(string source, string destination)
        {
            System.IO.File.Copy(source, destination);
        }

        public static void MoveFile(string source, string destination)
        {
            System.IO.File.Move(source, destination);
        }

        public static void MoveDirectory(string source, string destination)
        {
            System.IO.Directory.Move(source, destination);
        }

        public static string[] GetFiles(string path)
        {
            return System.IO.Directory.GetFiles(path);
        }

        public static string[] GetDirectories(string path)
        {
            return System.IO.Directory.GetDirectories(path);
        }

        public string GetFileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }

        public string GetDirectoryName(string path)
        {
            return System.IO.Path.GetDirectoryName(path);
        }

        public string GetExtension(string path)
        {
            return System.IO.Path.GetExtension(path);
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        public string GetFullPath(string path)
        {
            return System.IO.Path.GetFullPath(path);
        }

        public static string GetRandomFileName()
        {
            return System.IO.Path.GetRandomFileName();
        }

        public static string GetTempFileName()
        {
            return System.IO.Path.GetTempFileName();
        }

        public static string GetTempPath()
        {
            return System.IO.Path.GetTempPath();
        }

        public bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public void SetCurrentDirectory(string path)
        {
            System.IO.Directory.SetCurrentDirectory(path);
        }

        public static string GetCurrentDirectory()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        public string GetParent(string path)
        {
            return System.IO.Directory.GetParent(path).FullName;
        }

        public string GetRoot(string path)
        {
            return System.IO.Path.GetPathRoot(path);
        }

        public static string GetLogicalDrives()
        {
            return string.Join(", ", System.IO.Directory.GetLogicalDrives());
        }

        public string GetCreationTime(string path)
        {
            return System.IO.File.GetCreationTime(path).ToString();
        }

        public void CreateFile(string path)
        {
            System.IO.File.Create(path);
        }

        public void CreateFile(string path, int bufferSize)
        {
            System.IO.File.Create(path, bufferSize);
        }

        public void CreateFile(string path, int bufferSize, FileOptions options)
        {
            System.IO.File.Create(path, bufferSize, options);
        }






    }




    public class ProcessesByTheDay
    {

        public void Start(string path)
        {
            Process.Start(path);
        }

        public void Start(string path, string arguments)
        {
            Process.Start(path, arguments);
        }



        //process methods
        public void Kill(int id)
        {
            Process.GetProcessById(id).Kill();

        }
        public void Kill(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.Kill();
            }
        }

        public void KillAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                process.Kill();
            }
        }


        public bool IsRunning(int id)
        {
            try
            {
                Process.GetProcessById(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //find process by name and return the id
        public int FindByName(string name)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == name)
                {
                    return process.Id;
                }
            }
            return -1;
        }



        //find process by id and return the name
        public string FindByInt(int id)
        {
            try
            {
                return Process.GetProcessById(id).ProcessName;
            }
            catch (Exception)
            {
                return "";
            }
        }


        public void WaitForExit(int id)
        {
            Process.GetProcessById(id).WaitForExit();
        }
        public void WaitForExit(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.WaitForExit();
            }
        }
        public void WaitForExitAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                process.WaitForExit();
            }
        }
        public void CloseMainWindow(int id)
        {
            Process.GetProcessById(id).CloseMainWindow();
        }
        public void CloseMainWindow(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.CloseMainWindow();
            }
        }
        public void CloseMainWindowAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                process.CloseMainWindow();
            }
        }
        public void Close(int id)
        {
            Process.GetProcessById(id).Close();
        }
        public void Close(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                process.Close();
            }
        }
        public void CloseAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                process.Close();
            }
        }
        public bool Exists(int id)
        {
            try
            {
                Process.GetProcessById(id);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
        public bool Exists(string name)
        {
            return Process.GetProcessesByName(name).Length > 0;
        }
        public bool ExistsAll()
        {
            return Process.GetProcesses().Length > 0;
        }
        public void Suspend(int id)
        {
            var process = Process.GetProcessById(id);
            ProcessControl.SuspendProcess(id);
        }
        public void Suspend(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                ProcessControl.SuspendProcess(process.Id);
            }
        }
        public void SuspendAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                ProcessControl.SuspendProcess(process.Id);
            }
        }
        public void Resume(int id)
        {
            ProcessControl.ResumeProcess(id);
        }
        public void Resume(string name)
        {
            foreach (Process process in Process.GetProcessesByName(name))
            {
                ProcessControl.SuspendProcess(process.Id);
            }
        }
        public void ResumeAll()
        {
            foreach (Process process in Process.GetProcesses())
            {
                ProcessControl.SuspendProcess(process.Id);
            }


        }
    }

    //keyboard and mouse class
    public class Keyboard
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008,
        }



        /// <summary>
        /// DirectX key list collected out from the gamespp.com list by me.
        /// </summary>
        public enum DirectXKeyStrokes
        {
            DIK_ESCAPE = 0x01,
            DIK_1 = 0x02,
            DIK_2 = 0x03,
            DIK_3 = 0x04,
            DIK_4 = 0x05,
            DIK_5 = 0x06,
            DIK_6 = 0x07,
            DIK_7 = 0x08,
            DIK_8 = 0x09,
            DIK_9 = 0x0A,
            DIK_0 = 0x0B,
            DIK_MINUS = 0x0C,
            DIK_EQUALS = 0x0D,
            DIK_BACK = 0x0E,
            DIK_TAB = 0x0F,
            DIK_Q = 0x10,
            DIK_W = 0x11,
            DIK_E = 0x12,
            DIK_R = 0x13,
            DIK_T = 0x14,
            DIK_Y = 0x15,
            DIK_U = 0x16,
            DIK_I = 0x17,
            DIK_O = 0x18,
            DIK_P = 0x19,
            DIK_LBRACKET = 0x1A,
            DIK_RBRACKET = 0x1B,
            DIK_RETURN = 0x1C,
            DIK_LCONTROL = 0x1D,
            DIK_A = 0x1E,
            DIK_S = 0x1F,
            DIK_D = 0x20,
            DIK_F = 0x21,
            DIK_G = 0x22,
            DIK_H = 0x23,
            DIK_J = 0x24,
            DIK_K = 0x25,
            DIK_L = 0x26,
            DIK_SEMICOLON = 0x27,
            DIK_APOSTROPHE = 0x28,
            DIK_GRAVE = 0x29,
            DIK_LSHIFT = 0x2A,
            DIK_BACKSLASH = 0x2B,
            DIK_Z = 0x2C,
            DIK_X = 0x2D,
            DIK_C = 0x2E,
            DIK_V = 0x2F,
            DIK_B = 0x30,
            DIK_N = 0x31,
            DIK_M = 0x32,
            DIK_COMMA = 0x33,
            DIK_PERIOD = 0x34,
            DIK_SLASH = 0x35,
            DIK_RSHIFT = 0x36,
            DIK_MULTIPLY = 0x37,
            DIK_LMENU = 0x38,
            DIK_SPACE = 0x39,
            DIK_CAPITAL = 0x3A,
            DIK_F1 = 0x3B,
            DIK_F2 = 0x3C,
            DIK_F3 = 0x3D,
            DIK_F4 = 0x3E,
            DIK_F5 = 0x3F,
            DIK_F6 = 0x40,
            DIK_F7 = 0x41,
            DIK_F8 = 0x42,
            DIK_F9 = 0x43,
            DIK_F10 = 0x44,
            DIK_NUMLOCK = 0x45,
            DIK_SCROLL = 0x46,
            DIK_NUMPAD7 = 0x47,
            DIK_NUMPAD8 = 0x48,
            DIK_NUMPAD9 = 0x49,
            DIK_SUBTRACT = 0x4A,
            DIK_NUMPAD4 = 0x4B,
            DIK_NUMPAD5 = 0x4C,
            DIK_NUMPAD6 = 0x4D,
            DIK_ADD = 0x4E,
            DIK_NUMPAD1 = 0x4F,
            DIK_NUMPAD2 = 0x50,
            DIK_NUMPAD3 = 0x51,
            DIK_NUMPAD0 = 0x52,
            DIK_DECIMAL = 0x53,
            DIK_F11 = 0x57,
            DIK_F12 = 0x58,
            DIK_F13 = 0x64,
            DIK_F14 = 0x65,
            DIK_F15 = 0x66,
            DIK_KANA = 0x70,
            DIK_CONVERT = 0x79,
            DIK_NOCONVERT = 0x7B,
            DIK_YEN = 0x7D,
            DIK_NUMPADEQUALS = 0x8D,
            DIK_CIRCUMFLEX = 0x90,
            DIK_AT = 0x91,
            DIK_COLON = 0x92,
            DIK_UNDERLINE = 0x93,
            DIK_KANJI = 0x94,
            DIK_STOP = 0x95,
            DIK_AX = 0x96,
            DIK_UNLABELED = 0x97,
            DIK_NUMPADENTER = 0x9C,
            DIK_RCONTROL = 0x9D,
            DIK_NUMPADCOMMA = 0xB3,
            DIK_DIVIDE = 0xB5,
            DIK_SYSRQ = 0xB7,
            DIK_RMENU = 0xB8,
            DIK_HOME = 0xC7,
            DIK_UP = 0xC8,
            DIK_PRIOR = 0xC9,
            DIK_LEFT = 0xCB,
            DIK_RIGHT = 0xCD,
            DIK_END = 0xCF,
            DIK_DOWN = 0xD0,
            DIK_NEXT = 0xD1,
            DIK_INSERT = 0xD2,
            DIK_DELETE = 0xD3,
            DIK_LWIN = 0xDB,
            DIK_RWIN = 0xDC,
            DIK_APPS = 0xDD,
            DIK_BACKSPACE = DIK_BACK,
            DIK_NUMPADSTAR = DIK_MULTIPLY,
            DIK_LALT = DIK_LMENU,
            DIK_CAPSLOCK = DIK_CAPITAL,
            DIK_NUMPADMINUS = DIK_SUBTRACT,
            DIK_NUMPADPLUS = DIK_ADD,
            DIK_NUMPADPERIOD = DIK_DECIMAL,
            DIK_NUMPADSLASH = DIK_DIVIDE,
            DIK_RALT = DIK_RMENU,
            DIK_UPARROW = DIK_UP,
            DIK_PGUP = DIK_PRIOR,
            DIK_LEFTARROW = DIK_LEFT,
            DIK_RIGHTARROW = DIK_RIGHT,
            DIK_DOWNARROW = DIK_DOWN,
            DIK_PGDN = DIK_NEXT,

            // Mined these out of nowhere.
            DIK_LEFTMOUSEBUTTON = 0x100,
            DIK_RIGHTMOUSEBUTTON = 0x101,
            DIK_MIDDLEWHEELBUTTON = 0x102,
            DIK_MOUSEBUTTON3 = 0x103,
            DIK_MOUSEBUTTON4 = 0x104,
            DIK_MOUSEBUTTON5 = 0x105,
            DIK_MOUSEBUTTON6 = 0x106,
            DIK_MOUSEBUTTON7 = 0x107,
            DIK_MOUSEWHEELUP = 0x108,
            DIK_MOUSEWHEELDOWN = 0x109,
        }

        /// <summary>
        /// Sends a directx key.
        /// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
        /// </summary>
        /// <param name="key"></param>
        /// <param name="KeyUp"></param>
        /// <param name="inputType"></param>
        public void SendKey(DirectXKeyStrokes key, bool KeyUp, InputType inputType)
        {
            uint flagtosend;
            if (KeyUp)
            {
                flagtosend = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode);
            }
            else
            {
                flagtosend = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode);
            }

            Input[] inputs =
            {
            new Input
            {
                type = (int) inputType,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = (ushort) key,
                        dwFlags = flagtosend,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            }
        };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        /// <summary>
        /// Sends a directx key.
        /// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
        /// </summary>
        /// <param name="key"></param>
        /// <param name="KeyUp"></param>
        /// <param name="inputType"></param>
        public void SendKey(ushort key, bool KeyUp, InputType inputType)
        {
            uint flagtosend;
            if (KeyUp)
            {
                flagtosend = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode);
            }
            else
            {
                flagtosend = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode);
            }

            Input[] inputs =
            {
            new Input
            {
                type = (int) inputType,
                u = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = key,
                        dwFlags = flagtosend,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            }
        };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct MouseInput
        {
            public readonly int dx;
            public readonly int dy;
            public readonly uint mouseData;
            public readonly uint dwFlags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public readonly uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct HardwareInput
        {
            public readonly uint uMsg;
            public readonly ushort wParamL;
            public readonly ushort wParamH;
        }
    }

    //mouse

    internal class Mouse
    {
    [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
        public static void MoveMouse(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }
        public static void MoveMouseRelative(int x, int y)
        {
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }
        public static void LeftClick()
        {
            mouse_event(0x0002, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
            mouse_event(0x0004, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
        }
        public static void RightClick()
        {
            mouse_event(0x0008, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
            mouse_event(0x0010, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
        }
        public static void MiddleClick()
        {
            mouse_event(0x0020, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
            mouse_event(0x0040, Cursor.Position.X, Cursor.Position.Y, 0, GetMessageExtraInfo().ToInt32());
        }
        public static void ScrollUp()
        {
            mouse_event(0x0800, Cursor.Position.X, Cursor.Position.Y, 120, GetMessageExtraInfo().ToInt32());
        }
        public static void ScrollDown()
        {
            mouse_event(0x0800, Cursor.Position.X, Cursor.Position.Y, -120, GetMessageExtraInfo().ToInt32());
        }
    }


    internal class Request
    {
        private readonly HttpClient _httpClient;

        public Request()
        {
            _httpClient = new HttpClient();

        }

        public string Get(string url)
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {

                return ex.ToString();

            }
        }

        public string Post(string url, string content)
        {
            try
            {
                HttpContent httpContent = new StringContent(content);
                HttpResponseMessage response = _httpClient.PostAsync(url, httpContent).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                return ex.ToString();
            }
        }

        public string Put(string url, string content)
        {
            try
            {
                HttpContent httpContent = new StringContent(content);
                HttpResponseMessage response = _httpClient.PutAsync(url, httpContent).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                return ex.ToString();
            }
        }

        public string Delete(string url)
        {
            try
            {
                HttpResponseMessage response = _httpClient.DeleteAsync(url).Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                return ex.ToString();
            }
        }
    }

    internal class JSON
    {
        //stringify, parse, etc

        public string Stringify(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public object Parse(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }

        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        public object Deserialize(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        }
    }
    public class convert
    {

        public string ToString(object obj)
        {
            return obj.ToString();

        }

        public int ToInt(object obj)
        {
            return Convert.ToInt32(obj);
        }

        public double ToDouble(object obj)
        {
            return Convert.ToDouble(obj);
        }

        public float ToFloat(object obj)
        {
            return Convert.ToSingle(obj);
        }

        public bool ToBool(object obj)
        {
            return Convert.ToBoolean(obj);
        }

        public char ToChar(object obj)
        {
            return Convert.ToChar(obj);
        }

        public byte ToByte(object obj)
        {
            return Convert.ToByte(obj);
        }

        public short ToShort(object obj)
        {
            return Convert.ToInt16(obj);
        }



    }

    private class HttpServer
    {
        private HttpListener listener;
        private Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Task>> routes;

        public HttpServer()
        {
            listener = new HttpListener();

            routes = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Task>>();

        }

        public void Start(string prefix)
        {
            if (!listener.IsListening)
            {
                listener.Prefixes.Add(prefix);
                listener.Start();
                Console.WriteLine("HTTP server started");
                Task.Run(() => HandleRequests());
            }
            else
            {
                Console.WriteLine("HTTP server is already running");
            }
        }

        public void Stop()
        {
            if (listener.IsListening)
            {
                listener.Stop();
                Console.WriteLine("HTTP server stopped");
            }
            else
            {
                Console.WriteLine("HTTP server is not running");
            }
        }

        public void AddRoute(string method, string route, Func<HttpListenerRequest, HttpListenerResponse, Task> handler)
        {
            string key = $"{method}:{route}";
            routes[key] = handler;
        }

        private async void ProcessRequest(HttpListenerContext context)
        {
            string method = context.Request.HttpMethod.ToUpper();
            string url = context.Request.Url.AbsolutePath;
            string key = $"{method}:{url}";

            if (routes.ContainsKey(key))
            {
                Func<HttpListenerRequest, HttpListenerResponse, Task> handler = routes[key];
                await handler(context.Request, context.Response);
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
            }
        }


        private async Task HandleRequests()
        {
            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine("HTTP listener stopped: " + ex.Message);
                }
            }
        }

    }
    public class LuaHttpServer
    {
        private HttpServer httpServer;

        public LuaHttpServer()
        {
            httpServer = new HttpServer();
        }

        public void Start(string prefix)
        {
            httpServer.Start(prefix);
        }

        public void Stop()
        {
            httpServer.Stop();
        }

        public void AddGetRoute(string route, LuaFunction handler)
        {
            httpServer.AddRoute("GET", route, async (req, res) =>
            {
                handler.Call(req, res);
            });
        }

        public void AddPostRoute(string route, LuaFunction handler)
        {
            httpServer.AddRoute("POST", route, async (req, res) =>
            {
                handler.Call(req, res);
            });
        }

        public void AddPutRoute(string route, LuaFunction handler)
        {
            httpServer.AddRoute("PUT", route, async (req, res) =>
            {
                handler.Call(req, res);
            });
        }

        public void AddDeleteRoute(string route, LuaFunction handler)
        {
            httpServer.AddRoute("DELETE", route, async (req, res) =>
            {
                handler.Call(req, res);
            });
        }





        public string GetRequestHeader(HttpListenerRequest req, string headerName)
        {
            // Get the value of a request header
            return req.Headers.Get(headerName);
        }

        public string GetRequestBody(HttpListenerRequest req)
        {
            // Get the body of a request
            using (StreamReader reader = new StreamReader(req.InputStream))
            {
                return reader.ReadToEnd();
            }
        }

        public void SetResponseHeader(HttpListenerResponse res, string headerName, string headerValue)
        {
            // Set a response header
            res.Headers.Add(headerName, headerValue);
        }

        public void SetResponseBody(HttpListenerResponse res, string body)
        {
            // Set the body of a response
            using (StreamWriter writer = new StreamWriter(res.OutputStream))
            {
                writer.Write(body);
            }
        }

        public void SetResponseStatus(HttpListenerResponse res, int statusCode)
        {
            // Set the status code of a response
            res.StatusCode = statusCode;
        }




    }



    //socket server and client
    public class SocketServer
    {
        private TcpListener server;
        private List<TcpClient> clients;
        private bool running;

        public SocketServer()
        {
            server = new TcpListener(IPAddress.Any, 0);
            clients = new List<TcpClient>();
            running = false;
        }

        public void Start()
        {
            if (!running)
            {
                server.Start();
                running = true;
                Task.Run(() => AcceptClients());
            }
        }

        public void Stop()
        {
            if (running)
            {
                server.Stop();
                running = false;
            }
        }

        public void Send(int index, string message)
        {
            if (index >= 0 && index < clients.Count)
            {
                TcpClient client = clients[index];
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        public void SendAll(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (TcpClient client in clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }

        private async Task AcceptClients()
        {
            while (running)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);
                Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            while (running)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
            clients.Remove(client);
            client.Close();
        }
    }

    public class SocketClient
    {
        private TcpClient client;
        private NetworkStream stream;

        public SocketClient()
        {
            client = new TcpClient();
        }

        public void Connect(string host, int port)
        {
            client.Connect(host, port);
            stream = client.GetStream();
        }

        public void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public string Receive()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public void Close()
        {
            client.Close();
        }
    }

    //database




}

public class RuntimeFunctions
{


    public void sleep(int ms)
    {
        Thread.Sleep(ms);
    }


    public void SetInterval(Action action, int ms)
    {
        System.Threading.Timer timer = new System.Threading.Timer((e) =>
        {
            action();
        }, null, 0, ms);
    }

    public void SetTimeout(Action action, int ms)
    {
        System.Threading.Timer timer = new System.Threading.Timer((e) =>
        {
            action();
        }, null, ms, System.Threading.Timeout.Infinite);
    }


    public void WriteAndInput(string text)
    {
        Console.Write(text);
        Console.ReadLine();
    }

    public void Input()
    {
        Console.ReadLine();
    }

   
    public void Clear()
    {
        Console.Clear();
    }

    public void Exit()
    {
        Environment.Exit(0);
    }






}
public class PackageManager
{
    public void InstallPackage(string packageName)
    {
        var url1 = "https://raw.githubusercontent.com/"+ packageName.Split('/')[0] + "/" + packageName.Split('/')[1] + "/main/" + packageName.Split('/')[1] + ".dll";
        var url2 = "https://raw.githubusercontent.com/" + packageName.Split('/')[0] + "/" + packageName.Split('/')[1] + "/main/" + packageName.Split('/')[1] + ".deps.json";
        WebClient webClient = new WebClient();
        try
        {
            webClient.DownloadFile(url1, "libs/" + packageName.Split('/')[1] + ".dll");
            webClient.DownloadFile(url2, "libs/" + packageName.Split('/')[1] + ".deps.json");

            Console.WriteLine("Package downloaded successfully.");

            Console.WriteLine("Please restart the program to use the package.");


        }

        catch (Exception ex)
        {
            Console.WriteLine("We could not download the package. Please check the package name and try again.");
            Console.WriteLine(ex.Message);
        }



    }

    public void UninstallPackage(string packageName)
    {
        try
        {
            File.Delete("libs/" + packageName + ".dll");
            File.Delete("libs/" + packageName + ".deps.json");
            Console.WriteLine("Package uninstalled successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("We could not uninstall the package. Please check the package name and try again.");
            Console.WriteLine(ex.Message);
        }
    }
}