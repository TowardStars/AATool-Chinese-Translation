using System.Linq;
using AATool.Data.Progress;

namespace AATool.Data.Objectives.Complex
{
    class Cats : MultipartObjective
    {
        private const string TwoByTwo = "minecraft:husbandry/bred_all_animals";
        private const string Cat = "minecraft:cat";

        public Cats() : base()
        {
            this.Icon = "complete_catalogue";
        }

        public override string AdvancementId => "minecraft:husbandry/complete_catalogue";
        public override string Criterion => "Cat";
        public override string Action => "Tame";
        public override string PastAction => "Tamed";
        protected override string ModernBaseTexture => "complete_catalogue";
        protected override string OldBaseTexture => "complete_catalogue";
        
        private bool catalogueComplete;
        private bool breedCats;

        protected override void UpdateAdvancedState(ProgressState progress)
        {
            base.UpdateAdvancedState(progress);
            this.catalogueComplete = progress.AdvancementCompleted(this.AdvancementId);
            this.breedCats = progress.CriterionCompleted(TwoByTwo, Cat);
            this.CompletionOverride &= this.breedCats;
        }


        protected override void ClearAdvancedState()
        {
            base.ClearAdvancedState();
            this.breedCats = false;
        }

        protected override string GetLongStatus()
        {
            if (this.CompletionOverride)
                return "完成百猫全书";

            if (this.RemainingCriteria.Count is 1)
                return $"最后一种猫咪:\n{this.RemainingCriteria.First()}";
            
            if (this.catalogueComplete && !this.breedCats)
                return "需要繁殖猫咪";

            return $"驯服的猫咪\n{this.CurrentCriteria}\0/\0{this.RequiredCriteria}";
        }
    }
}
