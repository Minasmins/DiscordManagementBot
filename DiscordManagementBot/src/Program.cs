namespace DangerBotNamespace
{
    internal class Program
    {
        static void Main(string[] Args)
        {
            var bot = new DangerBot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}

