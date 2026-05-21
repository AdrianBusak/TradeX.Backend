namespace TradeX.Repository.Executor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            if (args.Length > 0)
            {
                Console.WriteLine($"My args: {string.Join(',', args)}");
            }
        }
    }
}
