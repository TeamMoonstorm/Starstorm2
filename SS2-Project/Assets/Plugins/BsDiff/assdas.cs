using System.Threading.Tasks;
using ThunderKit.Core.Paths;

namespace ThunderKit.Core.Pipelines.Jobs
{
    [PipelineSupport(typeof(Pipeline))]
    public class assdas : PipelineJob
    {
        public string OldFile;
        public string NewFile;
        public string PatchFile;

        public override Task Execute(Pipeline pipeline)
        {
            BsDiff.BsTool.Patch(OldFile.Resolve(pipeline, this), NewFile, PatchFile.Resolve(pipeline, this));

            return Task.CompletedTask;
        }
    }
}