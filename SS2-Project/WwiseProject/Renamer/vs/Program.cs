using System.IO;

namespace SoundbankRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            var infoFile = File.ReadAllText(Path.Combine(args[0], "SoundbanksInfo.xml"));
            infoFile = infoFile.Replace("Init", args[1]);
            File.WriteAllText(Path.Combine(args[0], "SoundbanksInfo.xml"), infoFile);

            File.Move(Path.Combine(args[0], "Init.bnk"), Path.Combine(args[0], $"{args[1]}.bnk"), true);
            System.Windows.Forms.Application.Exit();
        }
    }
}
