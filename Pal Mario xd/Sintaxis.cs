using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Sintaxis3
{
    class Sintaxis: Lexico
    {
        protected int caracterAnterior;
        public Sintaxis()
        {
            Console.WriteLine("Iniciando analisis sintactico.");
            caracterAnterior = 0;
            NextToken();
        }

        public Sintaxis(string nombre): base(nombre)
        {
            Console.WriteLine("Iniciando analisis sintactico.");
            caracterAnterior = 0;
            NextToken();
        }

        public void match(string espera)
        {
            // Console.WriteLine(getContenido() + " = " + espera);
            if (espera == getContenido())
            {                
                NextToken();
                caracterAnterior = caracter;
            }
            else
            {                
                throw new Error(bitacora, "Error de sintaxis: Se espera un " + espera + " (" + linea + ", " + caracter + ")");
            }
        }

        public void match(clasificaciones espera)
        {
            // Console.WriteLine(getContenido() + " = " + espera);
            if (espera == getClasificacion())
            {
                caracterAnterior = caracter;
                NextToken();                
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: Se espera un " + espera + " (" + linea + ", " + caracter + ")");
            }
        }

        
    }
}