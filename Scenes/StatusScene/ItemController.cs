using Elenigma.Models;
using Elenigma.SceneObjects;
using Elenigma.SceneObjects.Controllers;
using Elenigma.SceneObjects.Overlays;
using Elenigma.SceneObjects.Particles;
using Elenigma.Scenes.MapScene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.StatusScene
{
    public class ItemController : ScriptController
    {
        private Scene parentScene;

        HeroModel userHero;
        HeroModel targetHero;
        Vector2 targetCenter;

        public List<Particle> ParticleList { get; } = new List<Particle>();

        bool skipEffect;

        public bool Consumed = true;

        public ItemController(Scene iScene, string[] script, HeroModel iTargetPoke, Vector2 iTargetCenter, PriorityLevel priorityLevel = PriorityLevel.MenuLevel, bool iSkipEffect = false)
            : base(iScene, script, priorityLevel)
        {
            parentScene = iScene;
            targetHero = iTargetPoke;
            targetCenter = iTargetCenter;
            skipEffect = iSkipEffect;
        }

        public ItemController(Scene iScene, string[] script, HeroModel iHeroPoke, HeroModel iTargetPoke, Vector2 iTargetCenter, PriorityLevel priorityLevel = PriorityLevel.MenuLevel, bool iSkipEffect = false)
            : base(iScene, script, priorityLevel)
        {
            parentScene = iScene;
            userHero = iHeroPoke;
            targetHero = iTargetPoke;
            targetCenter = iTargetCenter;
            skipEffect = iSkipEffect;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            if (scriptParser.Finished)
            {
                if (ParticleList.Count == 0) Terminate();
            }
            else scriptParser.Update(gameTime);

            ParticleList.RemoveAll(x => x.Terminated);
        }

        public override bool ExecuteCommand(string[] tokens)
        {
            switch (tokens[0])
            {
                case "Heal": Heal(tokens); break;
                case "Replenish": Replenish(tokens); break;
                case "Revive": Revive(tokens); break;
                case "CureStatus": CureStatus(tokens); break;
                case "Effect": if (!skipEffect) Effect(tokens); break;
                case "Conversation": EventController.Conversation(tokens, scriptParser); break;

                case "ChangeMap": EventController.ChangeMap(tokens, MapScene.MapScene.Instance); break;

                default: return false;
            }

            return true;
        }

        public override string ParseParameter(string parameter)
        {
            if (parameter.StartsWith("$SaveData."))
            {
                return GameProfile.GetSaveData<string>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter[0] == '$')
            {
                switch (parameter)
                {
                    case "$partyCount": return GameProfile.PlayerProfile.Party.Count().ToString();
                    case "$targetX": return targetCenter.X.ToString();
                    case "$targetY": return targetCenter.Y.ToString();
                    case "$canHeal": return (targetHero.HP.Value > 0 && targetHero.HP.Value < targetHero.HP.Value).ToString();
                    case "$canRevive": return (targetHero.HP.Value == 0).ToString();
                    case "$poisoned": return (targetHero.StatusAilments.Any(x => x.Value == AilmentType.Poison)).ToString();
                    default: return null;
                }
            }
            else return null;
        }

        public void Heal(string[] tokens)
        {
            int healing = 1;
            if (tokens.Length == 2) healing = int.Parse(tokens[1]);
            else
            {
                int power = int.Parse(tokens[2]) + Rng.RandomInt(0, int.Parse(tokens[2]) / 8);
                int multiplier = userHero.Level.Value * userHero.Magic.Value / 256 + 4;

                healing = power * multiplier;
                if (healing < 1) healing = 1;
                if (healing > 9999) healing = 9999;
            }

            targetHero.HP.Value = Math.Min(targetHero.MaxHP.Value, targetHero.HP.Value + healing);
            targetHero.UpdateHealthColor();
        }

        public void Replenish(string[] tokens)
        {
            int replenishment = int.Parse(tokens[1]);

            targetHero.MP.Value = Math.Min(targetHero.MaxMP.Value, targetHero.MP.Value + replenishment);
            //targetHero.UpdateHealthColor();
        }

        public void Revive(string[] tokens)
        {
            targetHero.HP.Value = (int)(targetHero.MaxHP.Value * (int.Parse(tokens[1]) / 100.0f));
            targetHero.UpdateHealthColor();
        }

        public void CureStatus(string[] tokens)
        {
            if (tokens[1] == "All")
            {
                targetHero.StatusAilments.RemoveAll();
            }
            else
            {
                var status = targetHero.StatusAilments.FirstOrDefault(x => x.Value.ToString() == tokens[1]);
                targetHero.StatusAilments.Remove(status);
            }
        }

        private void Effect(string[] tokens)
        {
            Vector2 position = new Vector2(int.Parse(tokens[2]), int.Parse(tokens[3]));
            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType), tokens[1]);
            AnimationParticle animationParticle = new AnimationParticle(parentScene, position, animationType, true);

            if (tokens.Length > 4) animationParticle.AddFrameEvent(int.Parse(tokens[4]), new FrameFollowup(scriptParser.BlockScript()));

            parentScene.AddParticle(animationParticle);
            parentScene.AddOverlay(new ParticleOverlay(animationParticle));
            ParticleList.Add(animationParticle);
        }
    }
}
