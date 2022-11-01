using System;
using System.Collections.Generic;
using System.Text;

// -Requerimiento 1: Implementar el NOT en el IF.
// -Requerimiento 2: Validar la asignación de STRING en instrucción.
// -Requerimiento 3: Implementar la comparación de tipos de datos en Lista_IDs.   
// -Requerimiento 4: Validar los tipos de datos en la asignacipon del CIN.
// -Requerimiento 5: Implementar el cast.

namespace Sintaxis3
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;

        public Lenguaje()
        {            
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre): base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {            
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones(true);            
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(Variable.tipo TYPE, bool ejecuta)
        {          
            string name = getContenido();
            if (!l.Existe(name))
            {
                l.Inserta(name, TYPE);
                match(clasificaciones.identificador); // Validar duplicidad
            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")");
            }                
            //l.Inserta(name, TYPE);
            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                String valor = getContenido();
                if(getClasificacion() == clasificaciones.cadena) 
                {
                    if(TYPE == Variable.tipo.STRING)
                    {
                        match(clasificaciones.cadena);
                        if(ejecuta)
                        {
                             l.setValor(name,valor);
                        }
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No es posible asignar un STRING a un (" + TYPE + ") " + "(" + linea + ", " + caracter + ")");
                    }
                    
                    
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    maxBytes = Variable.tipo.CHAR;
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    
                    if(ejecuta)
                    {
                        if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                        {
                             maxBytes = tipoDatoExpresion(float.Parse(valor));
                        }

                        if(maxBytes > TYPE)
                        {
                            throw new Error(bitacora, "Error semantico: No es posible asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                        }
                    
                        l.setValor(name, valor);
                    }
                } 
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(TYPE, ejecuta);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string nombre = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo tipo;

            switch(nombre){

                case "char":
                    tipo = Variable.tipo.CHAR;
                    break;
                
                case "int":
                    tipo = Variable.tipo.INT;
                    break;
                
                case "float":
                    tipo = Variable.tipo.FLOAT;
                    break;
                
                case "string":
                    tipo = Variable.tipo.STRING;
                    break;

                default:
                    tipo = Variable.tipo.INT;
                    break;
            }
            Lista_IDs(tipo, ejecuta);
            match(clasificaciones.finSentencia);           
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);
                string name = getContenido();
                if (l.Existe(name))
                {
                     // Validar existencia
                    if(ejecuta)
                    {
                        match(clasificaciones.identificador);
                        string name2 = Console.ReadLine();
                        maxBytes = Variable.tipo.CHAR;
                        if(tipoDatoExpresion(float.Parse(name2)) > maxBytes)
                        {
                            maxBytes = tipoDatoExpresion(float.Parse(name2));
                        }

                        if(maxBytes > l.getTipoDato(name))
                        {
                            throw new Error(bitacora, "Error semantico: No es posible asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                        }
                        l.setValor(name, name2);
                    }
                     
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }            
            else
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La constante (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")"); 
                }
                
                match(clasificaciones.asignacion);

                string valor;

                if (getClasificacion() == clasificaciones.cadena) //Requerimiento 2 ya programado en lista_id
                {           
                    valor = getContenido(); 
                    if(l.getTipoDato(name) == Variable.tipo.STRING)
                    {
                         match(clasificaciones.cadena);
                         if(ejecuta)
                         {
                             l.setValor(name, valor);
                         }  
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No es posible asignar un STRING a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                    }    
                                     
                }
                else
                {               
                    //Requerimiento 3 ya programado aqui
                    maxBytes = Variable.tipo.CHAR;
                    Expresion();
                    //float resultado = s.pop(bitacora, linea, caracter);
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)  
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if( maxBytes > l.getTipoDato(name))
                    {
                         throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                    }               
                }                
                if(ejecuta)
                {
                    l.setValor(name, valor);
                }
              
                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }
        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");
            string name2 = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo TYPE;
            switch (name2)
            {
                case "int":
                TYPE= Variable.tipo.INT;
                break;

                case "float":
                TYPE= Variable.tipo.FLOAT;
                break;

                case "char":
                TYPE= Variable.tipo.CHAR;
                break;

                case "string":
                TYPE= Variable.tipo.STRING;
                break;

                default:
                TYPE= Variable.tipo.INT;
                break;
            }
            string name = getContenido(); 
            //match(clasificaciones.identificador); // Validar duplicidad
            if (!l.Existe(name) && ejecuta)
            {
                //l.Inserta(name,TYPE, true);
                match(clasificaciones.identificador); // Validar duplicidad
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La constante (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")"); 
            }
            l.Inserta(name, TYPE, true);
            match(clasificaciones.asignacion);

            string name3;
            if (getClasificacion() == clasificaciones.numero)
            {   
                name3 = getContenido();
                match(clasificaciones.numero);
                l.setValor(name, name3);
            }
            else
            {   
                name3 = getContenido();
                match(clasificaciones.cadena);
                
            }
            if(ejecuta)
            {
                l.setValor(name, name3);
            }
         
            match(clasificaciones.finSentencia);
        }

       // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if(ejecuta)
                {
                    Console.Write(getContenido());
                }
                match(clasificaciones.numero); 
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {         
                string name = getContenido();
                if(name.Contains("\""))
                {
                    name = name.Replace("\"", "");
                }                             
                if(name.Contains("\\n"))
                {
                    name = name.Replace("\\n", "\n");
                }
                if(name.Contains("\\t"))
                {
                    name = name.Replace("\\t", "\t");
                }
                if(ejecuta)
                {
                    Console.Write(name);
                }
                match(clasificaciones.cadena);
            }
            else
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    if(ejecuta)
                    {
                        Console.Write(l.getValor(name));
                    }
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                               
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            match("if");
            match("(");
            string operador = getContenido();
            bool ejecuta;
            if(operador == "!")
            {
                match(clasificaciones.operadorLogico);
                match("(");
                ejecuta = !Condicion();
                match(")");
            }
            else
            {
                ejecuta = Condicion();
            }
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }

       // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora,linea,caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora,linea,caracter);

            switch(operador)
            {
                case ">":
                    return n1 > n2;
                case ">=":
                    return n1 >= n2;
                case "<":
                    return n1 < n2;
                case "<=":
                    return n1 <= n2;
                case "==":
                    return n1 == n2;
                default:
                    return n1 != n2;
                
            }
        }
        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();                              
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);  
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "+":
                        s.push(e2+e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2-e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();                
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter); 
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "*":
                        s.push(e2*e1, bitacora, linea, caracter);                        
                        break;
                    case "/":
                        s.push(e2/e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    s.display(bitacora);
                    match(clasificaciones.identificador); // Validar existencia
                    if(l.getTipoDato(name) > maxBytes)
                    {
                        maxBytes = l.getTipoDato(name);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora); 
                if(tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                    }
                     match(clasificaciones.numero);
            }
            else
            {
                match("(");
                bool huboCast = false;
                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if(getClasificacion()== clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");
                if (huboCast)
                {
                    //Hacer un pop y convertir ese numero al tipoDato y meterlo al stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    //Para convertir un int a char divide entre 255 y el residuo es el resultado del cast
                    //Para convertir un float a int necesitamos dividir entre 65535 y el residuo es el resultado del cast
                    //Para convertir un float a otro tipo de dato hay que redondear el numero para eliminar la parte fraccional
                    //Para convertir un float a char necesitamos dividir entre 65535/256 y el residuo es el resultado del cast
                    //Para convertir un float n1 = n1;
                    n1 = cast(n1, tipoDato);
                    s.push(n1, bitacora, linea, caracter);
                    maxBytes = tipoDato;
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta)
        {
            match("for");

            match("(");

            string name = getContenido();
            if (l.Existe(name))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);

            string name2 = getContenido();
            if(l.Existe(name2))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
            }
            match(clasificaciones.incrementoTermino);
            match(")");
            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }
        
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private Variable.tipo tipoDatoExpresion(float valor) 
        {
            if(valor %1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if(valor < 256)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }
            
                return Variable.tipo.FLOAT;
            }

         private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;

            switch(tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;
                
                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;  
                    break;                  
            }

            return tipoVar;
        }
            //Hacer un pop y convertir ese numero al tipoDato y meterlo al stack
            //Para convertir un int a char divide entre 255 y el residuo es el resultado del cast
            //Para convertir un float a int necesitamos dividir entre 65535 y el residuo es el resultado del cast
            //Para convertir un float a otro tipo de dato hay que redondear el numero para eliminar la parte fraccional
            //Para convertir un float a char necesitamos dividir entre 65535/256 y el residuo es el resultado del cast
            //Para convertir un float n1 = n1;
        private float cast(float num, Variable.tipo TYPE)
        {
            switch(TYPE)
            {
                case Variable.tipo.INT:
                    if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)   //Convertimos float a int
                    {
                        num = (int) Math.Round(num % 65536);
                    }
                    break;

                case Variable.tipo.CHAR:
                    if(tipoDatoExpresion(num) == Variable.tipo.INT)  //Convertimos int a char
                    {
                        num = num % 256;
                    }
                    else if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)  //Convertimos float a char
                    {
                        num = (int) Math.Round((num % 65536) % 256);
                    }
                    else   //Convertimos otro tipo de dato 
                    {
                        num = (char) Math.Round(num);
                    }
                    break;
 
                case Variable.tipo.FLOAT: //Si convertimos de float a float se deja igual
                    if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)
                    {
                        break;
                    }
                    break;

                default:                                                    
                    num = (int) Math.Round(num);
                    break;
            }
            return num;
        }
    }
}



/*using System;
using System.Collections.Generic;
using System.Text;

// Requerimiento 1: Implementar el not en el if
// Requerimiento 2: Validar la asignacion de strings en instruccion
// Requerimiento 3: Implementar la comparacion de tipos de datos en lista_ids
// Requerimiento 4: Validar los tipos de datos en la asignacion del cin
// Requerimiento 5: Implementar el cast

namespace Sintaxis3
{
    class Lenguaje: Sintaxis
    {
        Stack s;
        ListaVariables l;
        Variable.tipo maxBytes;
        public Lenguaje()
        {            
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        public Lenguaje(string nombre): base(nombre)
        {
            s = new Stack(5);
            l = new ListaVariables();
            Console.WriteLine("Iniciando analisis gramatical.");
        }

        // Programa -> Libreria Main
        public void Programa()
        {
            Libreria();
            Main();
            l.imprime(bitacora);
        }

        // Libreria -> (#include <identificador(.h)?> Libreria) ?
        private void Libreria()
        {            
            if (getContenido() == "#")
            {
                match("#");
                match("include");
                match("<");
                match(clasificaciones.identificador);

                if (getContenido() == ".")
                {
                    match(".");
                    match("h");
                }

                match(">");

                Libreria();
            }
        }

        // Main -> tipoDato main() BloqueInstrucciones 
        private void Main()
        {
            match(clasificaciones.tipoDato);
            match("main");
            match("(");
            match(")");

            BloqueInstrucciones(true);            
        }

        // BloqueInstrucciones -> { Instrucciones }
        private void BloqueInstrucciones(bool ejecuta)
        {
            match(clasificaciones.inicioBloque);

            Instrucciones(ejecuta);

            match(clasificaciones.finBloque);
        }

        // Lista_IDs -> identificador (= Expresion)? (,Lista_IDs)? 
        private void Lista_IDs(Variable.tipo TYPE, bool ejecuta)
        {          
            string name = getContenido();
            if (!l.Existe(name))
            {
                l.Inserta(name, TYPE);
                match(clasificaciones.identificador); // Validar duplicidad
            }
            else
            {
                // Levantar excepción
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")");
            }                
            l.Inserta(name, TYPE);
            if (getClasificacion() == clasificaciones.asignacion)
            {
                match(clasificaciones.asignacion);
                String valor = getContenido();
                if(getClasificacion() == clasificaciones.cadena) 
                {
                    if(TYPE == Variable.tipo.STRING)
                    {
                        match(clasificaciones.cadena);
                        if(ejecuta)
                        {
                             l.setValor(name,valor);
                        }
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No es posible asignar un STRING a un (" + TYPE + ") " + "(" + linea + ", " + caracter + ")");
                    }
                    
                    
                }
                else
                {
                    //Requerimiento 3
                    Expresion();
                    maxBytes = Variable.tipo.CHAR;
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    
                    if(ejecuta)
                    {
                        if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)
                        {
                             maxBytes = tipoDatoExpresion(float.Parse(valor));
                        }

                        if(maxBytes > TYPE)
                        {
                            throw new Error(bitacora, "Error semantico: No es posible asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                        }
                    
                        l.setValor(name, valor);
                    }
                } 
            }

            if (getContenido() == ",")
            {
                match(",");
                Lista_IDs(TYPE, ejecuta);
            }
        }

        // Variables -> tipoDato Lista_IDs; 
        private void Variables(bool ejecuta)
        {
            string name = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo TYPE;
            switch (name)
            {
                case "int":
                TYPE= Variable.tipo.INT;
                break;

                case "float":
                TYPE= Variable.tipo.FLOAT;
                break;

                case "char":
                TYPE= Variable.tipo.CHAR;
                break;

                case "string":
                TYPE= Variable.tipo.STRING;
                break;

                default:
                TYPE= Variable.tipo.INT;
                break;
            }
            Lista_IDs(TYPE, ejecuta);
            match(clasificaciones.finSentencia);           
        }

        // Instruccion -> (If | cin | cout | const | Variables | asignacion) ;
        private void Instruccion(bool ejecuta)
        {
            if (getContenido() == "do")
            {
                DoWhile(ejecuta);
            }
            else if (getContenido() == "while")
            {
                While(ejecuta);
            }
            else if (getContenido() == "for")
            {
                For(ejecuta);
            }
            else if (getContenido() == "if")
            {
                If(ejecuta);
            }
            else if (getContenido() == "cin")
            {
                // Requerimiento 4
                match("cin");
                match(clasificaciones.flujoEntrada);
                string name = getContenido();
                if (l.Existe(name))
                {
                     // Validar existencia
                    if(ejecuta)
                    {
                        match(clasificaciones.identificador);
                        string name2 = Console.ReadLine();
                        maxBytes = Variable.tipo.CHAR;
                        if(tipoDatoExpresion(float.Parse(name2)) > maxBytes)
                        {
                            maxBytes = tipoDatoExpresion(float.Parse(name2));
                        }

                        if(maxBytes > l.getTipoDato(name))
                        {
                            throw new Error(bitacora, "Error semantico: No es posible asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                        }
                        l.setValor(name, name2);
                    }
                     
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "cout")
            {
                match("cout");
                ListaFlujoSalida(ejecuta);
                match(clasificaciones.finSentencia);
            }
            else if (getContenido() == "const")
            {
                Constante(ejecuta);
            }
            else if (getClasificacion() == clasificaciones.tipoDato)
            {
                Variables(ejecuta);
            }            
            else
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    match(clasificaciones.identificador); // Validar existencia
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La constante (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")"); 
                }
                
                match(clasificaciones.asignacion);

                string valor;

                if (getClasificacion() == clasificaciones.cadena) //Requerimiento 2 ya programado en lista_id
                {           
                    valor = getContenido(); 
                    if(l.getTipoDato(name) == Variable.tipo.STRING)
                    {
                         match(clasificaciones.cadena);
                         if(ejecuta)
                         {
                             l.setValor(name, valor);
                         }  
                    }
                    else
                    {
                        throw new Error(bitacora, "Error semantico: No es posible asignar un STRING a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                    }    
                                     
                }
                else
                {               
                    //Requerimiento 3 ya programado aqui
                    maxBytes = Variable.tipo.CHAR;
                    Expresion();
                    //float resultado = s.pop(bitacora, linea, caracter);
                    valor = s.pop(bitacora, linea, caracter).ToString();
                    if(tipoDatoExpresion(float.Parse(valor)) > maxBytes)  
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(valor));
                    }
                    if( maxBytes > l.getTipoDato(name))
                    {
                         throw new Error(bitacora, "Error semantico: No se puede asignar un " + maxBytes + " a un (" + l.getTipoDato(name) + ") " + "(" + linea + ", " + caracter + ")");
                    }               
                }                
                if(ejecuta)
                {
                    l.setValor(name, valor);
                }
              
                match(clasificaciones.finSentencia);
            }
        }

        // Instrucciones -> Instruccion Instrucciones?
        private void Instrucciones(bool ejecuta)
        {
            Instruccion(ejecuta);

            if (getClasificacion() != clasificaciones.finBloque)
            {
                Instrucciones(ejecuta);
            }
        }

        // Constante -> const tipoDato identificador = numero | cadena;
        private void Constante(bool ejecuta)
        {
            match("const");
            string name2 = getContenido();
            match(clasificaciones.tipoDato);
            Variable.tipo TYPE;
            switch (name2)
            {
                case "int":
                TYPE= Variable.tipo.INT;
                break;

                case "float":
                TYPE= Variable.tipo.FLOAT;
                break;

                case "char":
                TYPE= Variable.tipo.CHAR;
                break;

                case "string":
                TYPE= Variable.tipo.STRING;
                break;

                default:
                TYPE= Variable.tipo.INT;
                break;
            }
            string name = getContenido(); 
            //match(clasificaciones.identificador); // Validar duplicidad
            if (!l.Existe(name) && ejecuta)
            {
                //l.Inserta(name,TYPE, true);
                match(clasificaciones.identificador); // Validar duplicidad
                
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La constante (" + name + ") está duplicada  " + "(" + linea + ", " + caracter + ")"); 
            }
            l.Inserta(name, TYPE, true);
            match(clasificaciones.asignacion);

            string name3;
            if (getClasificacion() == clasificaciones.numero)
            {   
                name3 = getContenido();
                match(clasificaciones.numero);
                l.setValor(name, name3);
            }
            else
            {   
                name3 = getContenido();
                match(clasificaciones.cadena);
                
            }
            if(ejecuta)
            {
                l.setValor(name, name3);
            }
         
            match(clasificaciones.finSentencia);
        }

        // ListaFlujoSalida -> << cadena | identificador | numero (ListaFlujoSalida)?
        private void ListaFlujoSalida(bool ejecuta)
        {
            match(clasificaciones.flujoSalida);

            if (getClasificacion() == clasificaciones.numero)
            {
                if(ejecuta)
                {
                    Console.Write(getContenido());
                }
                match(clasificaciones.numero); 
            }
            else if (getClasificacion() == clasificaciones.cadena)
            {         
                string name = getContenido();
                if(name.Contains("\""))
                {
                    name = name.Replace("\"", "");
                }                             
                if(name.Contains("\\n"))
                {
                    name = name.Replace("\\n", "\n");
                }
                if(name.Contains("\\t"))
                {
                    name = name.Replace("\\t", "\t");
                }
                if(ejecuta)
                {
                    Console.Write(name);
                }
                match(clasificaciones.cadena);
            }
            else
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    if(ejecuta)
                    {
                        Console.Write(l.getValor(name));
                    }
                    match(clasificaciones.identificador); // Validar existencia 
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                               
            }

            if (getClasificacion() == clasificaciones.flujoSalida)
            {
                ListaFlujoSalida(ejecuta);
            }
        }

        // If -> if (Condicion) { BloqueInstrucciones } (else BloqueInstrucciones)?
        private void If(bool ejecuta2)
        {
            match("if");
            match("(");
            string operador = getContenido();
            bool ejecuta;
            if(operador == "!")
            {
                match(clasificaciones.operadorLogico);
                match("(");
                ejecuta = !Condicion();
                match(")");
            }
            else
            {
                ejecuta = Condicion();
            }
            match(")");
            BloqueInstrucciones(ejecuta && ejecuta2);

            if (getContenido() == "else")
            {
                match("else");
                BloqueInstrucciones(!ejecuta && ejecuta2);
            }
        }

        // Condicion -> Expresion operadorRelacional Expresion
        private bool Condicion()
        {
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n1 = s.pop(bitacora,linea,caracter);
            string operador = getContenido();
            match(clasificaciones.operadorRelacional);
            maxBytes = Variable.tipo.CHAR;
            Expresion();
            float n2 = s.pop(bitacora,linea,caracter);

            switch(operador)
            {
                case ">":
                    return n1 > n2;
                case ">=":
                    return n1 >= n2;
                case "<":
                    return n1 < n2;
                case "<=":
                    return n1 <= n2;
                case "==":
                    return n1 == n2;
                default:
                    return n1 != n2;
                
            }
        }

        // x26 = (3+5)*8-(10-4)/2;
        // Expresion -> Termino MasTermino 
        private void Expresion()
        {
            Termino();
            MasTermino();
        }
        // MasTermino -> (operadorTermino Termino)?
        private void MasTermino()
        {
            if (getClasificacion() == clasificaciones.operadorTermino)
            {
                string operador = getContenido();                              
                match(clasificaciones.operadorTermino);
                Termino();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter);  
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "+":
                        s.push(e2+e1, bitacora, linea, caracter);
                        break;
                    case "-":
                        s.push(e2-e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Termino -> Factor PorFactor
        private void Termino()
        {
            Factor();
            PorFactor();
        }
        // PorFactor -> (operadorFactor Factor)?
        private void PorFactor()
        {
            if (getClasificacion() == clasificaciones.operadorFactor)
            {
                string operador = getContenido();                
                match(clasificaciones.operadorFactor);
                Factor();
                float e1 = s.pop(bitacora, linea, caracter), e2 = s.pop(bitacora, linea, caracter); 
                // Console.Write(operador + " ");

                switch(operador)
                {
                    case "*":
                        s.push(e2*e1, bitacora, linea, caracter);                        
                        break;
                    case "/":
                        s.push(e2/e1, bitacora, linea, caracter);
                        break;                    
                }

                s.display(bitacora);
            }
        }
        // Factor -> identificador | numero | ( Expresion )
        private void Factor()
        {
            if (getClasificacion() == clasificaciones.identificador)
            {
                string name = getContenido();
                if (l.Existe(name))
                {
                    s.push(float.Parse(l.getValor(getContenido())), bitacora, linea, caracter);
                    s.display(bitacora);
                    match(clasificaciones.identificador); // Validar existencia
                    if(l.getTipoDato(name) > maxBytes)
                    {
                        maxBytes = l.getTipoDato(name);
                    }
                }
                else
                {
                    throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
                }
                
            }
            else if (getClasificacion() == clasificaciones.numero)
            {
                // Console.Write(getContenido() + " ");
                s.push(float.Parse(getContenido()), bitacora, linea, caracter);
                s.display(bitacora); 
                if(tipoDatoExpresion(float.Parse(getContenido())) > maxBytes)
                    {
                        maxBytes = tipoDatoExpresion(float.Parse(getContenido()));
                    }
                     match(clasificaciones.numero);
            }
            else
            {
                match("(");
                bool huboCast = false;
                Variable.tipo tipoDato = Variable.tipo.CHAR;
                if(getClasificacion()== clasificaciones.tipoDato)
                {
                    huboCast = true;
                    tipoDato = determinarTipoDato(getContenido());
                    match(clasificaciones.tipoDato);
                    match(")");
                    match("(");
                }
                Expresion();
                match(")");
                if (huboCast)
                {
                    //Hacer un pop y convertir ese numero al tipoDato y meterlo al stack
                    float n1 = s.pop(bitacora, linea, caracter);
                    //Para convertir un int a char divide entre 255 y el residuo es el resultado del cast
                    //Para convertir un float a int necesitamos dividir entre 65535 y el residuo es el resultado del cast
                    //Para convertir un float a otro tipo de dato hay que redondear el numero para eliminar la parte fraccional
                    //Para convertir un float a char necesitamos dividir entre 65535/256 y el residuo es el resultado del cast
                    //Para convertir un float n1 = n1;
                    n1 = cast(n1, tipoDato);
                    s.push(n1, bitacora, linea, caracter);
                    maxBytes = tipoDato;
                }
            }
        }

        // For -> for (identificador = Expresion; Condicion; identificador incrementoTermino) BloqueInstrucciones
        private void For(bool ejecuta)
        {
            match("for");

            match("(");

            string name = getContenido();
            if (l.Existe(name))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")");
            }
            match(clasificaciones.asignacion);
            Expresion();
            match(clasificaciones.finSentencia);

            Condicion();
            match(clasificaciones.finSentencia);

            string name2 = getContenido();
            if(l.Existe(name2))
            {
                match(clasificaciones.identificador); // Validar existencia
            }
            else
            {
                throw new Error(bitacora, "Error de sintaxis: La variable (" + name + ") no fue declarada  " + "(" + linea + ", " + caracter + ")"); 
            }
            match(clasificaciones.incrementoTermino);
            match(")");
            BloqueInstrucciones(ejecuta);
        }

        // While -> while (Condicion) BloqueInstrucciones
        private void While(bool ejecuta)
        {
            match("while");

            match("(");
            Condicion();
            match(")");

            BloqueInstrucciones(ejecuta);
        }
        
        // DoWhile -> do BloqueInstrucciones while (Condicion);
        private void DoWhile(bool ejecuta)
        {
            match("do");

            BloqueInstrucciones(ejecuta);

            match("while");

            match("(");
            Condicion();
            match(")");
            match(clasificaciones.finSentencia);
        }

        private Variable.tipo tipoDatoExpresion(float valor) 
        {
            if(valor %1 != 0)
            {
                return Variable.tipo.FLOAT;
            }
            else if(valor < 256)
            {
                return Variable.tipo.CHAR;
            }
            else if (valor < 65535)
            {
                return Variable.tipo.INT;
            }
            
                return Variable.tipo.FLOAT;
            }

         private Variable.tipo determinarTipoDato(string tipoDato)
        {
            Variable.tipo tipoVar;

            switch(tipoDato)
            {
                case "int":
                    tipoVar = Variable.tipo.INT;
                    break;
                
                case "float":
                    tipoVar = Variable.tipo.FLOAT;
                    break;

                case "string":
                    tipoVar = Variable.tipo.STRING;
                    break;

                default:
                    tipoVar = Variable.tipo.CHAR;  
                    break;                  
            }

            return tipoVar;
        }
        //Para convertir un int a char divide entre 255 y el residuo es el resultado del cast
        //Para convertir un float a int necesitamos dividir entre 65535 y el residuo es el resultado del cast
        //Para convertir un float a otro tipo de dato hay que redondear el numero para eliminar la parte fraccional
        //Para convertir un float a char necesitamos dividir entre 65535/256 y el residuo es el resultado del cast
        //Para convertir un float n1 = n1;
        private float cast(float num, Variable.tipo TYPE)
        {
            switch(TYPE)
            {
                case Variable.tipo.INT:
                    if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)   //Convertimos float a int
                    {
                        num = (int) Math.Round(num % 65536);
                    }
                    break;

                case Variable.tipo.CHAR:
                    if(tipoDatoExpresion(num) == Variable.tipo.INT)     //Convertimos int a char
                    {
                        num = num % 256;
                    }
                    else if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)  //Convertimos float a char
                    {
                        num = (int) Math.Round((num % 65536) % 256);
                    }
                    else                                                   //Convertimos otro tipo de dato 
                    {
                        num = (char) Math.Round(num);
                    }
                    break;
 
                case Variable.tipo.FLOAT:                                 //Si convertimos de float a float se deja igual
                    if(tipoDatoExpresion(num) == Variable.tipo.FLOAT)
                    {
                        break;
                    }
                    break;

                default:                                                    
                    num = (int) Math.Round(num);
                    break;
            }
            return num;
        }
    }
    
}*/