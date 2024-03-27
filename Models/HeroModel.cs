using Elenigma.Scenes.ConversationScene;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elenigma.Models
{
    [Serializable]
    public class HeroModel : BattlerModel
    {
        public static Dictionary<int, long> HP_TABLE = new Dictionary<int, long>()
        {
            { 1, 20 },
            { 2, 25 },
            { 3, 30 },
            { 4, 40 },
            { 5, 50 },
            { 6, 60 },
            { 7, 70 },
            { 8, 80 },
            { 9, 90 },
            { 10, 100 },
            { 11, 120 },
            { 12, 140 },
            { 13, 160 },
            { 14, 180 },
            { 15, 200 },
            { 16, 220 },
            { 17, 240 },
            { 18, 260 },
            { 19, 280 },
            { 20, 300 },
            { 21, 320 },
            { 22, 340 },
            { 23, 360 },
            { 24, 380 },
            { 25, 400 },
            { 26, 420 },
            { 27, 440 },
            { 28, 460 },
            { 29, 480 },
            { 30, 500 }
        };

        public static Dictionary<int, int> MP_TABLE = new Dictionary<int, int>()
        {
            { 1, 2 },
            { 2, 5 },
            { 3, 8 },
            { 4, 11 },
            { 5, 14 },
            { 6, 17 },
            { 7, 20 },
            { 8, 23 },
            { 9, 26 },
            { 10, 29 },
            { 11, 32 },
            { 12, 35 },
            { 13, 38 },
            { 14, 41 },
            { 15, 44 },
            { 16, 47 },
            { 17, 50 },
            { 18, 53 },
            { 19, 56 },
            { 20, 59 },
            { 21, 62 },
            { 22, 65 },
            { 23, 68 },
            { 24, 71 },
            { 25, 74 },
            { 26, 77 },
            { 27, 80 },
            { 28, 83 },
            { 29, 86 },
            { 30, 89 }
        };

        public static Dictionary<int, long> EXP_TABLE = new Dictionary<int, long>()
        {
            { 1, 0 },
            { 2, 10 },
            { 3, 33 },
            { 4, 74 },
            { 5, 140 },
            { 6, 241 },
            { 7, 389 },
            { 8, 599 },
            { 9, 888 },
            { 10, 1276 },
            { 11, 1786 },
            { 12, 2441 },
            { 13, 3269 },
            { 14, 4299 },
            { 15, 5564 },
            { 16, 7097 },
            { 17, 8936 },
            { 18, 11120 },
            { 19, 13691 },
            { 20, 16693 },
            { 21, 20173 },
            { 22, 24180 },
            { 23, 28765 },
            { 24, 33983 },
            { 25, 39890 },
            { 26, 46546 },
            { 27, 54012 },
            { 28, 62352 },
            { 29, 71632 },
            { 30, 81921 }
        };

        public HeroModel(HeroType heroType, ClassType classType)
            : base()
        {
            HeroType.Value = heroType;
            HeroRecord heroRecord = HeroRecord.HEROES.First(x => x.Name == HeroType.Value);
            Name.Value = HeroType.Value.ToString();

            Class.Value = classType;
            ClassRecord classRecord = ClassRecord.CLASSES.First(x => x.Name == Class.Value);

            Level.Value = 1;

            HP.Value = MaxHP.Value;
            MP.Value = MaxMP.Value;

            Sprite.Value = "Actors_" + heroRecord.Sprites[Class.Value.ToString()];
            BattleSprite.Value = "Battlers_" + heroRecord.Sprites[Class.Value.ToString()];
            PortraitSprite.Value = "Portraits_" + heroRecord.Portraits[Class.Value.ToString()];
            ShadowSprite.Value = GameSprite.Actors_DroneShadow.ToString();

            Commands.Add(BattleCommand.Fight);
            Commands.Add(BattleCommand.Defend);
            Commands.Add(BattleCommand.Item);
        }

        public HeroModel(HeroType heroType, ClassType classType, int iLevel)
            : this(heroType, classType)
        {
            GrowAfterBattle(EXP_TABLE[iLevel]);

            ClassProfiles.Add(new ClassProfile() { Class = ClassType.Tank });
            ClassProfiles.Add(new ClassProfile() { Class = ClassType.Warrior });
            ClassProfiles.Add(new ClassProfile() { Class = ClassType.Mage });
            ClassProfiles.Add(new ClassProfile() { Class = ClassType.Cleric });

            int i = 1;
            foreach (BattleCommand command in ClassProfiles[(int)classType].Commands)
            {
                Commands.ModelList.Insert(i, new ModelProperty<BattleCommand>(command));
                i++;
            }

            foreach (string ability in ClassProfiles[(int)classType].Abilities)
            {
                Abilities.Add(new AbilityRecord(AbilityRecord.ABILITIES.First(x => x.Name == ability)));
            }
        }

        // intro fools
        public HeroModel(HeroType heroType, ClassType classType, int iLevel, BattleCommand sideCommand)
            : this(heroType, classType, iLevel)
        {
            Commands.ModelList.Insert(2, new ModelProperty<BattleCommand>(sideCommand));
        }

        public void Remove(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon: Weapon.Value = null; break;
                case ItemType.Shield: Shield.Value = null; break;
                case ItemType.Armor: Armor.Value = null; break;
                case ItemType.Accessory: Accessory.Value = null; break;
            }

            CalculateStats();
        }

        public void Equip(string itemName)
        {
            ItemRecord itemRecord = ItemRecord.ITEMS.First(x => x.Name == itemName);
            Equip(itemRecord);
        }

        public void Equip(ItemRecord itemRecord)
        {
            switch (itemRecord.ItemType)
            {
                case ItemType.Weapon: Weapon.Value = itemRecord; break;
                case ItemType.Shield: Shield.Value = itemRecord; break;
                case ItemType.Armor: Armor.Value = itemRecord; break;
                case ItemType.Accessory: Accessory.Value = itemRecord; break;
            }

            CalculateStats();
        }

        public void ChangeClass(ClassType newClass)
        {
            if (Class.Value == newClass) return;

            Class.Value = newClass;
            var classRecord = ClassRecord.CLASSES.First(x => x.Name == newClass);
            if (Weapon.Value != null && !Weapon.Value.UsableBy.Contains(newClass)) { GameProfile.Inventory.Add(new ItemModel(Weapon.Value)); Weapon.Value = null; }
            if (Shield.Value != null && !Shield.Value.UsableBy.Contains(newClass)) { GameProfile.Inventory.Add(new ItemModel(Shield.Value)); Shield.Value = null; }
            if (Armor.Value != null && !Armor.Value.UsableBy.Contains(newClass)) { GameProfile.Inventory.Add(new ItemModel(Armor.Value)); Armor.Value = null; }
            if (Accessory.Value != null && !Accessory.Value.UsableBy.Contains(newClass)) { GameProfile.Inventory.Add(new ItemModel(Accessory.Value)); Accessory.Value = null; }

            CalculateStats();

            HeroRecord heroRecord = HeroRecord.HEROES.First(x => x.Name == HeroType.Value);
            Sprite.Value = "Actors_" + heroRecord.Sprites[Class.Value.ToString()];
            BattleSprite.Value = "Battlers_" + heroRecord.Sprites[Class.Value.ToString()];
            PortraitSprite.Value = "Portraits_" + heroRecord.Portraits[Class.Value.ToString()];

            Commands.RemoveAll();
            Commands.Add(BattleCommand.Fight);
            foreach (BattleCommand command in ClassProfiles[(int)newClass].Commands)
            {
                Commands.Add(command);
            }
            Commands.Add(BattleCommand.Defend);
            Commands.Add(BattleCommand.Item);

            Abilities.RemoveAll();
            foreach (string ability in ClassProfiles[(int)newClass].Abilities)
            {
                Abilities.Add(new AbilityRecord(AbilityRecord.ABILITIES.First(x => x.Name == ability)));
            }
        }

        private void CalculateStats()
        {
            HeroRecord heroRecord = HeroRecord.HEROES.First(x => x.Name == HeroType.Value);
            ClassRecord classRecord = ClassRecord.CLASSES.First(x => x.Name == Class.Value);

            Strength.Value = classRecord.BaseStrength + heroRecord.StrengthModifier;
            Agility.Value = classRecord.BaseAgility + heroRecord.AgilityModifier;
            Vitality.Value = classRecord.BaseVitality + heroRecord.VitalityModifier;
            Magic.Value = classRecord.BaseMagic + heroRecord.MagicModifier;

            Attack.Value = 0;
            Hit.Value = 0;
            Defense.Value = 0;
            MagicDefense.Value = 0;
            Evade.Value = 0;
            MagicEvade.Value = 0;
            Weight.Value = 0;

            ElementWeak.RemoveAll();
            ElementStrong.RemoveAll();
            ElementImmune.RemoveAll();
            ElementAbsorb.RemoveAll();
            AilmentImmune.RemoveAll();

            if (Weapon.Value != null) ApplyEquipment(Weapon.Value);
            if (Shield.Value != null) ApplyEquipment(Shield.Value);
            if (Armor.Value != null) ApplyEquipment(Armor.Value);
            if (Accessory.Value != null) ApplyEquipment(Accessory.Value);

            long hpDelta = MaxHP.Value - HP.Value;
            int mpDelta = MaxMP.Value - MP.Value;

            MaxHP.Value = CalculateHP();
            MaxMP.Value = CalculateMP();

            HP.Value = Math.Max(1, MaxHP.Value - hpDelta);
            MP.Value = Math.Max(1, MaxMP.Value - mpDelta);

            UpdateHealthColor();
        }

        private void ApplyEquipment(ItemRecord itemRecord)
        {
            Strength.Value += itemRecord.StrengthModifier;
            Agility.Value += itemRecord.AgilityModifier;
            Vitality.Value += itemRecord.VitalityModifier;
            Magic.Value += itemRecord.MagicModifier;

            Attack.Value += itemRecord.Attack;
            Hit.Value += itemRecord.Hit;
            Evade.Value += itemRecord.Evade;
            Defense.Value += itemRecord.Defense;
            MagicEvade.Value += itemRecord.MagicEvade;
            MagicDefense.Value += itemRecord.MagicDefense;
            Weight.Value += itemRecord.Weight;

            if (itemRecord.ElementsWeak != null) ElementWeak.AddRange(itemRecord.ElementsWeak);
            if (itemRecord.ElementsStrong != null) ElementStrong.AddRange(itemRecord.ElementsStrong);
            if (itemRecord.ElementsImmune != null) ElementImmune.AddRange(itemRecord.ElementsImmune);
            if (itemRecord.ElementsAbsorb != null) ElementAbsorb.AddRange(itemRecord.ElementsAbsorb);
            if (itemRecord.AilmentsImmune != null) AilmentImmune.AddRange(itemRecord.AilmentsImmune);
        }

        private long CalculateHP()
        {
            long baseHP = HP_TABLE[Level.Value];
            return baseHP * (Vitality.Value + 32) / 32;
        }

        private int CalculateMP()
        {
            int baseMP = MP_TABLE[Level.Value];
            return baseMP * (Magic.Value + 32) / 32;
        }

        public void UpdateHealthColor()
        {
            if (HP.Value > MaxHP.Value / 8) HealthColor.Value = new Color(252, 252, 252, 255);
            else if (HP.Value > 0) HealthColor.Value = new Color(228, 0, 88, 255);
            else HealthColor.Value = new Color(136, 20, 0, 255);
        }

        public List<DialogueRecord> GrowAfterBattle(long expGained)
        {
            int oldLevel = Level.Value;

            Exp.Value = Exp.Value + expGained;

            long expThreshold = EXP_TABLE[Level.Value + 1];
            while (Exp.Value >= expThreshold)
            {
                Level.Value = Level.Value + 1;
                expThreshold = EXP_TABLE[Level.Value + 1];
                CalculateStats();
            }

            NextLevel.Value = EXP_TABLE[Level.Value + 1] - Exp.Value;

            int newLevel = Level.Value;

            List<DialogueRecord> reports = new List<DialogueRecord>();

            if (oldLevel != newLevel)
            {
                DialogueRecord dialogueRecord = new DialogueRecord()
                {
                    Text = Name + " reached level " + newLevel + "!",
                    Script = new string[] { "Sound LevelUp" }
                };

                reports.Add(dialogueRecord);
            };

            /*
            var classRecord = ClassRecord.CLASSES.First(x => x.Name == Class.Value);
            if (classRecord.LearnableAbilities != null)
            {
                var newAbility = classRecord.LearnableAbilities.FirstOrDefault(x => x.Level <= Level.Value && !Abilities.Any(y => y.Value.Name == x.Ability));
                if (newAbility != null)
                {
                    var ability = AbilityRecord.ABILITIES.First(x => x.Name == newAbility.Ability);
                    Abilities.Add(ability);

                    DialogueRecord dialogueRecord = new DialogueRecord()
                    {
                        Text = Name + " learned @" + ability.Icon + " " + newAbility.Ability + "!"
                    };

                    reports.Add(dialogueRecord);
                }
            }
            */

            return reports;
        }

        public ModelProperty<HeroType> HeroType { get; set; } = new ModelProperty<HeroType>(Models.HeroType.TheCleric);

        public ModelProperty<string> Sprite { get; set; } = new ModelProperty<string>(GameSprite.Actors_AdultMC.ToString());
        public ModelProperty<string> BattleSprite { get; set; } = new ModelProperty<string>(GameSprite.Actors_AdultMC.ToString());
        public ModelProperty<string> PortraitSprite { get; set; } = new ModelProperty<string>(GameSprite.Actors_AdultMC.ToString());
        public ModelProperty<string> ShadowSprite { get; set; } = new ModelProperty<string>();
        public ModelProperty<int> FlightHeight { get; set; } = new ModelProperty<int>(0);

        public ModelProperty<long> Exp { get; set; } = new ModelProperty<long>(0);
        public ModelProperty<long> NextLevel { get; set; } = new ModelProperty<long>(0);

        [field: NonSerialized]
        public ModelProperty<Color> NameColor { get; set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        [field: NonSerialized]
        public ModelProperty<Color> HealthColor { get; set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        [field: NonSerialized]
        public ModelProperty<Color> InitiativeColor { get; set; } = new ModelProperty<Color>(new Color(252, 252, 252, 255));
        [field: NonSerialized]
        public ModelProperty<float> Initiative { get; set; } = new ModelProperty<float>(0.0f);

        public ModelProperty<int> LastCategory { get; private set; } = new ModelProperty<int>(0);
        public ModelProperty<int> LastSlot { get; private set; } = new ModelProperty<int>(0);

        public ModelProperty<ItemRecord> Weapon { get; private set; } = new ModelProperty<ItemRecord>();
        public ModelProperty<ItemRecord> Shield { get; private set; } = new ModelProperty<ItemRecord>();
        public ModelProperty<ItemRecord> Armor { get; private set; } = new ModelProperty<ItemRecord>();
        public ModelProperty<ItemRecord> Accessory { get; private set; } = new ModelProperty<ItemRecord>();

        public ModelCollection<AbilityRecord> Abilities { get; set; } = new ModelCollection<AbilityRecord>();
        public ModelCollection<BattleCommand> Commands { get; set; } = new ModelCollection<BattleCommand>();

        public ModelProperty<AilmentType> FieldStatus { get; set; } = new ModelProperty<AilmentType>(AilmentType.Healthy);

        public ModelProperty<int> UpgradePoints { get; set; } = new ModelProperty<int>(0);
        public ModelCollection<ClassProfile> ClassProfiles { get; set; } = new ModelCollection<ClassProfile>();

    }
}
