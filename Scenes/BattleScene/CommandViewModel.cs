using Microsoft.Xna.Framework.Graphics;
using Elenigma.Models;
using Elenigma.SceneObjects.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Scenes.BattleScene
{
    public class CommandViewModel : ViewModel
    {
        BattleScene battleScene;

        TargetViewModel targetSelector;

        RadioBox commandList;
        RadioBox subCommandList;
        Panel mpCounter;

        public CommandViewModel(BattleScene iScene, BattlePlayer iBattlePlayer)
            : base(iScene, PriorityLevel.GameLevel)
        {
            ConfirmCooldown = false;

            battleScene = iScene;

            ActivePlayer = iBattlePlayer;

            AvailableCommands = ActivePlayer.HeroModel.Commands;
            
            int commandHeight = Math.Max(60, AvailableCommands.Count() * 10 + 16);
            CommandBounds.Value = new Rectangle(-90, Math.Min(30, 90 - commandHeight), 60, commandHeight);

            LoadView(GameView.BattleScene_CommandView);

            commandList = GetWidget<RadioBox>("CommandList");
            subCommandList = GetWidget<RadioBox>("SubCommandList");
            mpCounter = GetWidget<Panel>("MPCounter");

            (commandList.ChildList[0] as RadioButton).RadioSelect();

            if (ActivePlayer.Stats.StatusAilments.ModelList.Any(x => x.Value == AilmentType.Confusion))
            {
                int index = ActivePlayer.HeroModel.Commands.ModelList.FindIndex(x => x.Value == BattleCommand.Magic);
                if (index >= 0) commandList.ChildList[index].Enabled = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (battleScene.BattleOver || ActivePlayer.Dead)
            {
                Terminate();
                return;
            }

            if (targetSelector != null)
            {
                if (targetSelector.Terminated)
                {
                    targetSelector = null;
                    if (!ActivePlayer.Ready)
                    {
                        Terminate();
                        return;
                    }
                    else
                    {
                        CancelCooldown = true;

                        var lastCommand = AvailableCommands[commandList.Selection];
                        if (lastCommand != BattleCommand.Fight && lastCommand != BattleCommand.Scope)
                        {
                            subCommandList.Enabled = true;
                            subCommandList.Visible = true;

                            if (lastCommand != BattleCommand.Throw && lastCommand != BattleCommand.Item)
                            {
                                mpCounter.Visible = true;
                            }
                        }
                        else
                        {
                            commandList.Enabled = true;
                            subCommandList.Enabled = false;
                            subCommandList.Visible = false;
                            mpCounter.Visible = false;
                        }
                    }
                }
                else return;
            }

            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel) && !CancelCooldown)
            {
                if (subCommandList.Enabled)
                {
                    Audio.PlaySound(GameSound.Back);
                    subCommandList.Enabled = false;
                    subCommandList.Visible = false;
                    commandList.Enabled = true;
                    mpCounter.Visible = false;
                }
            }

            CancelCooldown = false;
        }

        public static bool ConfirmCooldown = false;
        public bool CancelCooldown = false;

        public void SelectCommand(object parameter)
        {
            if (targetSelector != null) return;

            if (ConfirmCooldown)
            {
                ConfirmCooldown = false;
                return;
            }

            BattleCommand battleCommand;
            if (parameter is IModelProperty)
            {
                battleCommand = (BattleCommand)((IModelProperty)parameter).GetValue();
            }
            else battleCommand = (BattleCommand)parameter;

            ItemModel.ThrowMode = false;

            switch (battleCommand)
            {
                case BattleCommand.Fight:
                    commandList.Enabled = false;

                    CommandRecord weaponRecord = ActivePlayer.HeroModel.Weapon.Value;
                    if (weaponRecord == null) weaponRecord = new ItemRecord(ItemRecord.ITEMS.First(x => x.Name == "Fists"));

                    targetSelector = new TargetViewModel(battleScene, ActivePlayer, weaponRecord, BattleCommand.Fight);
                    battleScene.AddOverlay(targetSelector);
                    break;

                case BattleCommand.Scope:
                    commandList.Enabled = false;

                    CommandRecord scopeRecord = new CommandRecord()
                    {
                        Animation = "Guarding",
                        Targetting = TargetType.OneEnemy,
                        BattleScript = new string[] { "Announce Scope", "Animate Point", "Analyze", "Idle" }
                    };

                    targetSelector = new TargetViewModel(battleScene, ActivePlayer, scopeRecord, BattleCommand.Scope);
                    battleScene.AddOverlay(targetSelector);
                    break;

                case BattleCommand.Magic:
                    {
                        commandList.Enabled = false;

                        ItemModel.CurrentUser = ActivePlayer.HeroModel;

                        List<ModelProperty<ItemModel>> subCommands = new List<ModelProperty<ItemModel>>();
                        foreach (var ability in ActivePlayer.HeroModel.Abilities.ModelList.Where(x => x.Value.CommandCategory == BattleCommand.Magic).Select(x => new ItemModel(x.Value)))
                        {
                            subCommands.Add(new ModelProperty<ItemModel>(ability));
                        }

                        AvailableSubCommands.ModelList = subCommands;

                        subCommandList.Enabled = true;
                        subCommandList.Visible = true;

                        mpCounter.Visible = true;

                        if (!Input.MOUSE_MODE && AvailableSubCommands.Count() > 0)
                        {
                            (subCommandList.ChildList[0] as RadioButton).RadioSelect();
                        }
                    }
                    break;

                case BattleCommand.Defend:
                    ActivePlayer.Defending = true;
                    ActivePlayer.EndTurn();
                    ActivePlayer.Animate("Guarding");
                    ActivePlayer.ResetInitiative();
                    break;

                case BattleCommand.Item:
                    {
                        commandList.Enabled = false;

                        AvailableSubCommands.ModelList = GameProfile.Inventory.ModelList;

                        subCommandList.Enabled = true;
                        subCommandList.Visible = true;

                        if (!Input.MOUSE_MODE && AvailableSubCommands.Count() > 0)
                        {
                            (subCommandList.ChildList[0] as RadioButton).RadioSelect();
                        }
                    }
                    break;

                case BattleCommand.Throw:
                    {
                        ItemModel.ThrowMode = true;

                        commandList.Enabled = false;

                        AvailableSubCommands.ModelList = GameProfile.Inventory.ModelList;

                        subCommandList.Enabled = true;
                        subCommandList.Visible = true;

                        if (!Input.MOUSE_MODE && AvailableSubCommands.Count() > 0)
                        {
                            (subCommandList.ChildList[0] as RadioButton).RadioSelect();
                        }
                    }
                    break;
            }
        }

        public void SelectSubCommand(object parameter)
        {
            if (targetSelector != null) return;

            ItemModel battleCommand;
            if (parameter is IModelProperty)
            {
                battleCommand = (ItemModel)((IModelProperty)parameter).GetValue();
            }
            else battleCommand = (ItemModel)parameter;

            AbilityRecord abilityRecord = battleCommand.ItemRecord as AbilityRecord;
            if (abilityRecord != null && ActivePlayer.HeroModel.MP.Value < abilityRecord.Cost)
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            ItemRecord itemRecord = battleCommand.ItemRecord as ItemRecord;
            if (itemRecord != null && !battleCommand.BattleUsable)
            {
                Audio.PlaySound(GameSound.Error);
                return;
            }

            Audio.PlaySound(GameSound.Selection);

            subCommandList.Enabled = false;
            subCommandList.Visible = false;

            targetSelector = new TargetViewModel(battleScene, ActivePlayer, battleCommand, AvailableCommands[commandList.Selection]);
            battleScene.AddOverlay(targetSelector);
        }


        public BattlePlayer ActivePlayer { get; set; }

        public ModelCollection<BattleCommand> AvailableCommands { get; set; } = new ModelCollection<BattleCommand>();
        public ModelProperty<Rectangle> CommandBounds { get; set; } = new ModelProperty<Rectangle>();

        public ModelCollection<ItemModel> AvailableSubCommands { get; set; } = new ModelCollection<ItemModel>();

        public ModelProperty<int> CurrentMP { get => ActivePlayer.HeroModel.MP; }

        public bool SubmenuActive { get => subCommandList.Enabled; }
    }
}
