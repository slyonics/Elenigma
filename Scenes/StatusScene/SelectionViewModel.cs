using Elenigma.Main;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using Elenigma.Scenes.StatusScene;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Elenigma.Scenes.StatusScene
{
    public class SelectionViewModel : ViewModel
    {
        private StatusScene statusScene;
        ItemViewModel itemViewModel;
        SkillViewModel skillViewModel;
        HeroModel heroModel;
        int itemSlot;

        ItemController itemController;

        int confirmCooldown = 100;

        public SelectionViewModel(Scene iScene, ItemViewModel parent, int iItemSlot)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene as StatusScene;
            itemViewModel = parent;
            itemSlot = iItemSlot;

            LoadView(GameView.StatusScene_SelectionView);
        }

        public SelectionViewModel(Scene iScene, SkillViewModel parent, HeroModel user, int iItemSlot)
            : base(iScene, PriorityLevel.GameLevel)
        {
            statusScene = iScene as StatusScene;
            skillViewModel = parent;
            heroModel = user;
            itemSlot = iItemSlot;

            LoadView(GameView.StatusScene_SelectionView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (itemController != null)
            {
                if (itemController.Terminated) itemController = null;
                return;
            }

            var input = Input.CurrentInput;
            if (input.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Terminate();
            }

            if (confirmCooldown > 0) confirmCooldown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void SelectHero(ModelProperty<HeroModel> targetHero)
        {
            TargetHero = targetHero.Value;

            if (itemController != null) return;

            CommandRecord commandRecord;
            if (itemViewModel != null) commandRecord = GameProfile.Inventory[itemSlot].ItemRecord;
            else commandRecord = heroModel.Abilities[itemSlot];

            if (!EvaluateConditional(commandRecord.Conditions))
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            var targetBox = GetWidget<RadioBox>("OptionsList");
            var targetCenter = targetBox.ChildList[targetBox.Selection].AbsolutePosition + new Vector2(24, 24);

            itemController = statusScene.AddController(new ItemController(statusScene, commandRecord.FieldScript, heroModel, TargetHero, targetCenter));

            if (itemViewModel != null)
            {
                itemController.OnTerminated += new TerminationFollowup(() =>
                {
                    if (itemViewModel.ConsumeItem(itemSlot)) Terminate();
                });
            }
            else
            {
                heroModel.MP.Value = heroModel.MP.Value - ((AbilityRecord)commandRecord).Cost;
                itemController.OnTerminated += new TerminationFollowup(() =>
                {
                    if (skillViewModel.ConsumeItem(itemSlot)) Terminate();
                });
            }
        }


        public bool EvaluateConditional(string conditions)
        {
            List<string> tokens = conditions.Trim().Split(' ').Select(x => ParseParameter(x)).ToList();

            while (true)
            {
                if (tokens.Count == 1) return bool.Parse(tokens[0]);
                if (tokens.Count == 2) throw new Exception();
                else
                {
                    switch (tokens[1])
                    {
                        case "<": return EvaluateConditional((int.Parse(tokens[0]) < int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case ">": return EvaluateConditional((int.Parse(tokens[0]) > int.Parse(tokens[2])).ToString() + " " + string.Join(' ', tokens.Skip(3)));
                        case "=": return tokens[0] == tokens[2];
                        case "!=": return tokens[0] != tokens[2];
                        case "&&": return bool.Parse(tokens[0]) && EvaluateConditional(string.Join(' ', tokens.Skip(2)));
                        case "||": return bool.Parse(tokens[0]) || EvaluateConditional(string.Join(' ', tokens.Skip(2)));
                    }
                }
            }
        }

        public string ParseParameter(string parameter)
        {
            if (parameter[0] == '!' && parameter.Length > 1)
            {
                parameter = ParseParameter(parameter.Substring(1, parameter.Length - 1));
                if (parameter == "True") return "False";
                else if (parameter == "False") return "True";
                else throw new Exception();
            }
            else if (parameter[0] == '#' && parameter.Length > 1)
            {
                return GameProfile.GetInventoryCount(parameter.Substring(1, parameter.Length - 1).Replace('_', ' ')).ToString();
            }
            else if (parameter.StartsWith("$Flag."))
            {
                return GameProfile.GetSaveData<bool>(parameter.Split('.')[1]).ToString();
            }
            else if (parameter.StartsWith("$Target."))
            {
                PropertyInfo propertyInfo = TargetHero.GetType().GetProperty(parameter.Split('.')[1]);
                return (propertyInfo.GetValue(TargetHero) as IModelProperty).ToString();

            }
            else if (parameter[0] != '$') return parameter;
            else
            {

                switch (parameter)
                {
                    case "$money": return GameProfile.PlayerProfile.Money.Value.ToString();
                    case "$selection": return GameProfile.GetSaveData<string>("LastSelection");
                    default:
                        if (parameter.Contains("$random"))
                        {
                            int start = parameter.IndexOf('(');
                            int middle = parameter.IndexOf(',');
                            int end = parameter.LastIndexOf(')');

                            int randomMin = int.Parse(parameter.Substring(start + 1, middle - start - 1));
                            int randomMax = int.Parse(parameter.Substring(middle + 1, end - middle - 1));

                            return Rng.RandomInt(randomMin, randomMax).ToString();
                        }
                        break;
                }
            }

            return parameter;
        }

        public HeroModel TargetHero { get; private set; }
    }
}
