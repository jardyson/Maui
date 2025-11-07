using log4net;
using log4net.Config;

namespace GlobalDeclar
{
    public class XLog
    {
        private static ILog mlog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static bool mInit = false;
        public static EventHandler msgcallback;
        public static void  IsInt()
        {
            if (mInit) return;
            if (!mInit)
            {
                try
                {
                    XmlConfigurator.ConfigureAndWatch(new FileInfo("log4.config"));
                    mInit = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"log4初始化错误！{ex.Message}");
                }
                
            }
        }
        public static void Info(string message)
        {
            IsInt();
            mlog.Info(message);

            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}", null);
        }
        public static void Warn(string message) {
            IsInt();
            mlog.Warn(message);
            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}", null);
        }
        public static void Error(Exception message)
        {
            IsInt();
            mlog.Error(message);
            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message.Message} \r\n {message.StackTrace}", null);
        }

        public static void Error(string message)
        {
            IsInt();
            mlog.Error(message);
            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}", null);
        }
        public static void Fatal(string message) {
            IsInt();
            mlog.Fatal(message);
            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}", null);
        }
        public static void Debug(string message)
        {
            IsInt();
            mlog.Debug(message);
            msgcallback?.Invoke($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}", null);
        }
    }
}
