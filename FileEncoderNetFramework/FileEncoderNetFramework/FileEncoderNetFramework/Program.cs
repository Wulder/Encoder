using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Numerics;


namespace FileEncoderNetFramework
{
    
    class Program
    {
        static Commands coms = new Commands();
        [STAThread]
        static void Main(string[] args)
        {
            Encoder enc = new Encoder();
            coms["help"].ExecuteAction();


            while (true)
            {
                ParseLine(Console.ReadLine());   
            }
        }

        static void ParseLine(string l)
        {
            if(l.Length > 0 && l[0] == '/')
            {
                
                string[] str = l.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
                List<string> parameters = str.ToList<string>();
                parameters.RemoveAt(0);
                Command c = coms[str[0].Substring(1, str[0].Length - 1)];

                if (c.IsParameters)
                {
                    if (parameters.Count > 0)
                        c.ExecuteAction(parameters.ToArray());
                    else
                        Commands.ColorWriteLine($"Для комманды {c.Name} отсутствуют необходимые параметры (/help - справка)",ConsoleColor.Red);
                }
                else
                    c.ExecuteAction(parameters.ToArray());
            }
        }

        
        
    }
}
