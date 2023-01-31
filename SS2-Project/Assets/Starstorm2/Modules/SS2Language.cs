using Moonstorm.Loaders;

namespace Moonstorm.Starstorm2
{
    public class SS2Language : LanguageLoader<SS2Language>
    {
        public override string AssemblyDir => SS2Assets.Instance.AssemblyDir;

        public override string LanguagesFolderName => "SS2Lang";

        ///Due to the nature of the language system in ror, we cannot load our language file using system initializer, as its too late.
        internal void Init()
        {
            LoadLanguages();
            TMProEffects.Init();
        }
    }
}
