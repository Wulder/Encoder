using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncoderNetFramework
{
    class Command
    {
        public delegate void CommandAction(params string[] param);

        CommandAction act;
        public readonly string Name, Description;
        public readonly List<string> Parameters = new List<string>();
        public readonly bool IsParameters;

        public Command(string name, CommandAction act)
        {
            Name = name;
            this.act = act;
            Parameters.Clear();
            IsParameters = false;
        }

        public Command(string name, CommandAction act, string desc) : this(name,act)
        {
            Description = desc;
        }

        public Command(string name, CommandAction act, string desc, params string[] parameters) : this(name, act, desc)
        {
            Parameters = parameters.ToList<string>();
            IsParameters = true;
        }





        public void ExecuteAction(params string[] p)
        {
            act?.Invoke(p);
        }

        public static Command EmptyCommand()
        {
            return new Command("Empty", CommandAction => Console.WriteLine("Такой команды не существует!"));
        }
    }
}
