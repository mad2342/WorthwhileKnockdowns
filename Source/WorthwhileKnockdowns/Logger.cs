using System;
using System.IO;



namespace WorthwhileKnockdowns
{
    public class Logger
    {
        static string filePath = $"{WorthwhileKnockdowns.ModDirectory}/WorthwhileKnockdowns.log";
        public static void LogError(Exception ex)
        {
            if (WorthwhileKnockdowns.DebugLevel >= 1)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[WorthwhileKnockdowns @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine("Message: " + ex.Message + "<br/>" + Environment.NewLine + "StackTrace: " + ex.StackTrace + "" + Environment.NewLine);
                    writer.WriteLine("----------------------------------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        public static void LogLine(String line)
        {
            if (WorthwhileKnockdowns.DebugLevel >= 2)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    var prefix = "[WorthwhileKnockdowns @ " + DateTime.Now.ToString() + "]";
                    writer.WriteLine(prefix + line);
                }
            }
        }
    }
}
