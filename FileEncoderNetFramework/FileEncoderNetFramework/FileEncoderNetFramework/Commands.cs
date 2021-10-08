using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace FileEncoderNetFramework
{
    class Commands
    {
        readonly public static List<Command> coms = new List<Command>();
        public Commands()
        {
            InitializeCommands();
        }
        void InitializeCommands()
        {
            coms.Add(new Command("Help", WriteAllCommands, "Показывает список всех доступных комманд."));
            coms.Add(new Command("SaveKey", SaveKeyCommand, "Сохраняет ключ в зависимости от атрибута.", "private","public","both"));
            coms.Add(new Command("LoadKey", LoadKeyCommand, "Загружает ключ"));
            coms.Add(new Command("config", ConfigCommand, "Показывает текущую конфигурацию кодировщика RSA"));
            coms.Add(new Command("EncodeFile", EncodeFileCommand, "Шифрует выбранный файл", "copy"));
            coms.Add(new Command("DecodeFile", DecodeFileCommand, "Расшифровывает выбранный файл", "copy"));
            coms.Add(new Command("Update", UpdateCommand, "Генерирует новые приватный и публичный ключ."));
            coms.Add(new Command("About", AboutCommand, "Информация о кодировщике"));

            coms.Add(new Command("Exit", ExitCommand, "Выход из программы"));
        }

        void AboutCommand(params string[] p)
        {
            ColorWriteLine(new string('=', 100), ConsoleColor.Yellow);
            Console.WriteLine($"\nВ данном кодировщике используется алгоритм RSA с фиксированной длинной ключа(1024 бит). В отличие от гибридного шифрования, здесь используется только ассиметричное шифрование блоков по 64 байта, из-за этого нюанса размер файла может быть в два раза больше его фактического размера.");
            ColorWriteLine(new string('=', 100), ConsoleColor.Yellow);
        }// /ABOUT
        void WriteAllCommands(params string[] p)
        {
            Console.WriteLine($"{new string('-', 20)}Доступные комманды{new string('-', 20)}");
            int comnum = 1;
            foreach (Command c in coms)
            {
                Console.WriteLine($"{comnum++}. \t/{c.Name}{(c.Parameters.Count > 0 ? $" {GetParamsStr(c.Parameters)}":"")}{(c.Description == null ? "" : $" - {c.Description}")}");
            }

           
        }// /HELP
        void SaveKeyCommand(params string[] p)
        {
            
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            switch (p[0])
            {
                case "private":
                    {
                        DialogResult dr = fbd.ShowDialog();
                        if(dr == DialogResult.OK)
                        {
                            Encoder.CurrentEncoder.SaveKeyFile(fbd.SelectedPath,"Private Key", true);
                        }
                        break;
                    }
                case "public":
                    {
                        DialogResult dr = fbd.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            Encoder.CurrentEncoder.SaveKeyFile(fbd.SelectedPath, "Public Key", false);
                        }
                        break;
                    }
                case "both":
                    {
                        DialogResult dr = fbd.ShowDialog();
                        if (dr == DialogResult.OK)
                        {
                            Encoder.CurrentEncoder.SaveKeyFile(fbd.SelectedPath, "Private Key", true);
                            Encoder.CurrentEncoder.SaveKeyFile(fbd.SelectedPath, "Public Key", false);

                        }
                        break;
                    }
            }
        }// /SAVEKEY

        void UpdateCommand(params string[] p)// /UPDATE
        {
            Encoder.CurrentEncoder.GenerateKeys();
        }
        void LoadKeyCommand(params string[] p)
        {
            
            OpenFileDialog fd = new OpenFileDialog();
     
            
            DialogResult dr = fd.ShowDialog();
            if(dr == DialogResult.OK)
            {
                Encoder.CurrentEncoder.LoadKeyFile(fd.FileName); 
            }
        }// /LOADKEY

        void ConfigCommand(params string[] p)
        {
            Encoder.CurrentEncoder.GetInfo();
        }// /CONFIG

        void EncodeFileCommand(params string[] p)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool copy = false;
            if (p[0].ToUpper() == "Y")
                copy = true;
            if(p[0].ToUpper() == "N")
                copy = false;
            DialogResult dr = ofd.ShowDialog();
            if(dr == DialogResult.OK)
            {
                Encoder.CurrentEncoder.EncodeFile(ofd.FileName,64, true, copy);
            }
            
        }// / ENCODEFILE

        void DecodeFileCommand(params string[] p)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            bool copy = false;
            if (p[0].ToUpper() == "Y")
                copy = true;
            if (p[0].ToUpper() == "N")
                copy = false;
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Encoder.CurrentEncoder.DecodeFile( ofd.FileName, 128, true, copy);
            }
        }// /DECODEFILE

        void ExitCommand(params string[] p)
        {
            Environment.Exit(0);
        }// /EXIT
        string GetParamsStr(List<string> p)
        {
            string res = "";
            foreach (string parameter in p)
                res += $"[{parameter}] ";

            return res;
        }

        public Command this[string ComName]
        {
            get
            {
                if (coms.Exists(n => n.Name.ToLower() == ComName.ToLower()))
                {
                    return coms.Find(n => n.Name.ToLower() == ComName.ToLower());
                }
                else
                {
                    return Command.EmptyCommand();
                }
            }
            
        }


        public static void ColorWriteLine(string text, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.WriteLine(text);
            Console.ResetColor();
            
        }
        public static void ColorWrite(string text, ConsoleColor col)
        {
            Console.ForegroundColor = col;
            Console.Write(text);
            Console.ResetColor();

        }
    }
}
