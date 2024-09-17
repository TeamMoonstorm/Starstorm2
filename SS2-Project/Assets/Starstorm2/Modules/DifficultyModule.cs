using MSU;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS2
{
    public static class DifficultyModule
    {
        public static ReadOnlyDictionary<DifficultyDef, IDifficultyContentPiece> SS2Difficulties { get; private set; }
        private static Dictionary<DifficultyDef, IDifficultyContentPiece> _ss2Difficulties = new Dictionary<DifficultyDef, IDifficultyContentPiece>();

        public static ResourceAvailability moduleAvailability;

        private static IContentPieceProvider<SerializableDifficultyDef> _contentPieceProvider;

        internal static IEnumerator Init()
        {
            if (moduleAvailability.available)
                yield break;

            _contentPieceProvider = ContentUtil.CreateGenericContentPieceProvider<SerializableDifficultyDef>(SS2Main.Instance, SS2Content.SS2ContentPack);

            var enumerator = InitializeDifficultiesFromSS2();
            while (!enumerator.IsDone())
                yield return null;

            SS2Difficulties = new ReadOnlyDictionary<DifficultyDef, IDifficultyContentPiece>(_ss2Difficulties);
            _ss2Difficulties = null;

            moduleAvailability.MakeAvailable();

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private static IEnumerator InitializeDifficultiesFromSS2()
        {
            IContentPiece<SerializableDifficultyDef>[] content = _contentPieceProvider.GetContents();

            List<IContentPiece<SerializableDifficultyDef>> difficulties = new List<IContentPiece<SerializableDifficultyDef>>();

            var helper = new ParallelMultiStartCoroutine();
            foreach (var difficulty in content)
            {
                if (!difficulty.IsAvailable(_contentPieceProvider.contentPack))
                    continue;

                difficulties.Add(difficulty);
                helper.Add(difficulty.LoadContentAsync);
            }

            helper.Start();
            while (!helper.isDone)
                yield return null;

            InitializeDifficulties(difficulties);
        }

        private static void InitializeDifficulties(List<IContentPiece<SerializableDifficultyDef>> difficulties)
        {
            foreach (var difficulty in difficulties)
            {
                difficulty.Initialize();

                var asset = difficulty.asset;
                DifficultyAPI.AddDifficulty(asset);

                if (difficulty is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(_contentPieceProvider.contentPack);
                }
                if (difficulty is IDifficultyContentPiece diffContentPiece)
                {
                    _ss2Difficulties.Add(diffContentPiece.asset.DifficultyDef, diffContentPiece);
                }
            }
        }

        private static void Run_onRunDestroyGlobal(Run obj)
        {
            var index = obj.selectedDifficulty;
            var def = DifficultyCatalog.GetDifficultyDef(index);

            if(SS2Difficulties.TryGetValue(def, out var difficulty))
            {
                difficulty.OnRunEnd(obj);
            }
        }

        private static void Run_onRunStartGlobal(Run obj)
        {
            var index = obj.selectedDifficulty;
            var def = DifficultyCatalog.GetDifficultyDef(index);

            if (SS2Difficulties.TryGetValue(def, out var difficulty))
            {
                difficulty.OnRunStart(obj);
            }
        }
    }

    public interface IDifficultyContentPiece : IContentPiece<SerializableDifficultyDef>
    { 
        void OnRunStart(Run run);

        void OnRunEnd(Run run);
    }
}
