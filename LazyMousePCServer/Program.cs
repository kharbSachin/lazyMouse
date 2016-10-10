using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace LazyMousePCServer
{
    class Program
    {

        static Socket listeningSocket;
        static Socket socket;
        static Thread thrReadRequest;
        //static int iPort = 4444;
        static int iConnectionQueue = 100;

        static void Main(string[] args)
        {
            Console.WriteLine(IPAddress.Parse(getLocalIPAddress()).ToString());
            IPEndPoint end=new IPEndPoint(IPAddress.Parse(getLocalIPAddress()), 8080);
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //listeningSocket.Bind(new IPEndPoint(0, iPort));                                
                listeningSocket.Bind(end);
                listeningSocket.Listen(iConnectionQueue);
                Console.WriteLine("Running on port "+ end.Port.ToString());
                thrReadRequest = new Thread(new ThreadStart(getRequest));
                thrReadRequest.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Winsock error: " + e.ToString());
                //Console.WriteLine(SocketErrorCode);

                Console.ReadKey();
                //throw;
            }
        }
        SendInputClass sc = new SendInputClass();
        
        static public void Operate(string s)
        {   int x,y;
        SendInputClass ass = new SendInputClass();


        try
        {
            string[] sarr = s.Split(',');
            //getting input type

            foreach (string word in sarr)
            { Console.WriteLine(word); }
            int.TryParse(sarr[1], out x);
            int.TryParse(sarr[2], out y);
            if (sarr[0] == "move")
            {
                ass.MoveMouseButton(x, y);
            }
            else if (sarr[0] == "lmdn")
            {
                ass.LeftMouseClick(x, y);
            }
            else
            {
                ass.RightMouseClick(x, y);
            }
             Console.WriteLine("x=" + x.ToString() + "  y=" + y.ToString());
        }
        catch (Exception e)
        { Console.WriteLine(e.ToString());
        }//  if()
        }
        static private void getRequest()
        {
            int i = 0;
                i++;
               // Console.WriteLine("Outside Try i = {0}", i.ToString());
           
            while(true)    
            { try
                               {
                        socket = listeningSocket.Accept();

                      

                        //black magic

                        byte[] buffer = new byte[socket.SendBufferSize];
                       
                        int iBufferLength = socket.Receive(buffer, 0, buffer.Length, 0);
                        //Console.WriteLine("Received {0}", iBufferLength);
                        Array.Resize(ref buffer, iBufferLength);
                        string formattedBuffer = Encoding.ASCII.GetString(buffer);
                        Operate(formattedBuffer);       
            //            Console.WriteLine("{0}", formattedBuffer);

                        if (formattedBuffer == "quit")
                        {
                            socket.Close();
                            listeningSocket.Close();
                         
                           Environment.Exit(0);
                        }

                //sun rha hai na tu ?
                        if (formattedBuffer == "are koi hai ?")
                        {
                            Console.WriteLine("Haan bol");
                        }

                      //  Console.WriteLine("Inside Try i = {0}", i.ToString());
                        Thread.Sleep(5);
                    }
                    catch (Exception e)
                    {
                        //socket.Close();
                        Console.WriteLine("Receiving error: " + e.ToString());
                        Console.ReadKey();
                        //throw;
                    }

                    finally
                    {
                        socket.Close();
                        //listeningsocket.close();
                    }
        }
            
        }

        static private string getLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
    public class SendInputClass
    {

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);



        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

        public  void MoveMouseButton(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;

            mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y); 
            mouseInput.mkhi.mi.mouseData = 0;

            //getting current cursor location
            System.Drawing.Point p;
         GetCursorPos(out p);
            //    SetCursorPos(x, y);


        //    
         Console.WriteLine("x=" + p.X.ToString() + "y=" + p.Y.ToString());
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            //
            //returning cursor to previous position
           // SetCursorPos(p.X, p.Y);
        }
        public void LeftMouseClick(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;

            mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y);
            mouseInput.mkhi.mi.mouseData = 0;

            
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
              SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
              mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
             SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

        }
        public void RightMouseClick(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;

            mouseInput.mkhi.mi.dx = CalculateAbsoluteCoordinateX(x);
            mouseInput.mkhi.mi.dy = CalculateAbsoluteCoordinateY(y);
            mouseInput.mkhi.mi.mouseData = 0;


            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

        }
     
        enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1,
        }
        
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

       public int CalculateAbsoluteCoordinateX(int x)
        {
            return (x * 65536) / GetSystemMetrics(SystemMetric.SM_CXSCREEN);
        }

        public int CalculateAbsoluteCoordinateY(int y)
        {
            return (y * 65536) / GetSystemMetrics(SystemMetric.SM_CYSCREEN);
        }
    }
}