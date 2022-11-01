using System;

namespace Sintaxis3
{
    class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                using (Lenguaje l = new Lenguaje("C:\\Archivos C#\\suma.cpp"))                
                {
                    /*while (!l.FinDeArchivo())
                    {
                        l.NextToken();
                    }*/
                    l.Programa();

                }
            }
            catch (Error e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey();
        }
    }
}