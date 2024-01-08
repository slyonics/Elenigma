namespace Elenigma.Main
{
    public enum GameView
    {
        ConversationScene_ConversationView,
        ConversationScene_ConversationView2,
        ConversationScene_ConversationView3,
        ConversationScene_SelectionView,
        CreditsScene_CreditsView,
        TitleScene_ContinueView,
        TitleScene_SettingsView,
        TitleScene_TitleView,

        None = -1
    }

    public enum GameSound
    {
        a_mainmenuconfirm,
        a_mainmenuselection,
        Back,
        BattleStart,
        Blip,
        Bonk,
        Bounce,
        Chest,
        Claw,
        Confirm,
        Construct,
        Counter,
        Cure,
        Cursor,
        dialogue_auto_scroll,
        Drop,
        Encounter,
        EnemyDeath,
        Error,
        Eruption,
        Fire,
        Fireball,
        Freeze,
        GetItem,
        Heal,
        Ice,
        Laser,
        LevelUp,
        menu_cursor_change,
        menu_select,
        Miss,
        move_selection_cursor,
        Pickup,
        Ready,
        Rest,
        Savepoint,
        Screech,
        Selection,
        Skeleton,
        Slash,
        Talk,
        Thunder,

        None = -1
    }

    public enum GameMusic
    {
        BeyondtheHills,
        BlastingThroughtheSky,
        ChoiceEncounter,
        ExploringtheDepths,
        FireStreaming,
        NewDestinations,
        Sanctuary,
        Selection,

        None = -1
    }

    public enum GameData
    {
        ConversationData,

        None = -1
    }

    public enum GameShader
    {
        BattleEnemy,
        BattleIntro,
        BattlePlayer,
        ColorFade,
        DayNight,
        Default,
        HeatDistortion,
        Pinwheel,
        Portrait,

        None = -1
    }

    public enum GameSprite
    {
        Ailments,
        Clouds,
        Down,
        DownSelected,
        Enter,
        Gamepad,
        Keyboard,
        Title,
        Up,
        UpSelected,
        Actors_AdultMC,
        Actors_AdvF,
        Actors_AdvM,
        Actors_Airship,
        Actors_BarF,
        Actors_BarM,
        Actors_BattlerShadow,
        Actors_Beast,
        Actors_Blank,
        Actors_BlueDarkKnight,
        Actors_Cat,
        Actors_Chest,
        Actors_CrystalBottom,
        Actors_CrystalTop,
        Actors_DroneShadow,
        Actors_GreenTurtle,
        Actors_HntF,
        Actors_HntM,
        Actors_MinF,
        Actors_MinM,
        Actors_Pilot,
        Actors_Plant,
        Actors_PrgF,
        Actors_PrgM,
        Actors_RedClown,
        Actors_RedCultist,
        Actors_RedReaper,
        Actors_SchF,
        Actors_SchM,
        Actors_Slyph,
        Actors_Sword,
        Actors_Twinkle,
        Actors_Undine,
        Actors_WarF,
        Actors_WarM,
        Actors_WaterMC,
        Actors_WindMC,
        Actors_YellowDarkKnight,
        Actors_YoungMC,
        Background_Blank,
        Background_Cave,
        Background_Desert,
        Background_Eclipse,
        Background_Forest,
        Background_Plains,
        Background_Splash,
        Background_Title,
        Background_Tower,
        Background_Wasteland,
        Particles_Exclamation,
        Particles_Gust,
        Particles_Smoke,
        Portraits_AdultMC,
        Portraits_AdvF,
        Portraits_AdvM,
        Portraits_BarF,
        Portraits_BarM,
        Portraits_HntF,
        Portraits_HntM,
        Portraits_MinF,
        Portraits_MinM,
        Portraits_PrgF,
        Portraits_PrgM,
        Portraits_SchF,
        Portraits_SchM,
        Portraits_WarF,
        Portraits_WarM,
        Tiles_Tl_Dungeon_A1,
        Tiles_Tl_Dungeon_A2,
        Tiles_Tl_Dungeon_A4,
        Tiles_Tl_Dungeon_A5,
        Tiles_Tl_Dungeon_B,
        Tiles_Tl_Dungeon_C,
        Tiles_Tl_Inside_A1,
        Tiles_Tl_Inside_A2,
        Tiles_Tl_Inside_A4,
        Tiles_Tl_Inside_A5,
        Tiles_Tl_Inside_B,
        Tiles_Tl_Inside_C,
        Tiles_Tl_Outside_A1,
        Tiles_Tl_Outside_A2,
        Tiles_Tl_Outside_A3,
        Tiles_Tl_Outside_A4,
        Tiles_Tl_Outside_A5,
        Tiles_Tl_Outside_B,
        Tiles_Tl_Outside_C,
        Tiles_Tl_Table,
        Tiles_Tl_Vehicle_A2,
        Tiles_Tl_Vehicle_A4,
        Tiles_Tl_Vehicle_A5,
        Tiles_Tl_Vehicle_B,
        Tiles_Tl_Vehicle_C,
        Tiles_Tl_World_A1,
        Tiles_Tl_World_A2,
        Tiles_Tl_World_B,
        Widgets_BattleFrame,
        Widgets_BattleWindow,
        Widgets_Blank,
        Widgets_ClassicFrame,
        Widgets_ClassicWindow,
        Widgets_Convobox,
        Widgets_DarkFrame,
        Widgets_LabelGlow,
        Widgets_MagicFrame,
        Widgets_MagicFrameSelected,
        Widgets_MagicLabel,
        Widgets_MagicSelected,
        Widgets_MagicWindow,
        Widgets_MonFrame,
        Widgets_MonSelected,
        Widgets_PartyLead,
        Widgets_PokeFrame,
        Widgets_PokeFrameSelected,
        Widgets_SelectedFrame,
        Widgets_SmallConvo,
        Widgets_TechFrame,
        Widgets_TechFrameSelected,
        Widgets_TechGlow,
        Widgets_TechLabel,
        Widgets_TechSelected,
        Widgets_TechWindow,
        Background_Eclipse_Eclipse0,
        Widgets_Buttons_ClearPanel,
        Widgets_Buttons_GamePanel,
        Widgets_Buttons_GamePanelOpaque,
        Widgets_Buttons_GamePanelSelected,
        Widgets_Buttons_Panel,
        Widgets_Buttons_SelectedPanel,
        Widgets_Buttons_Technology,
        Widgets_Gauges_HealthBar,
        Widgets_Gauges_TechGauge,
        Widgets_Gauges_TechGaugeBackground,
        Widgets_Gauges_TechGaugeBar,
        Widgets_Gauges_TechSlider,
        Widgets_Icons_Armor,
        Widgets_Icons_AutoRevive,
        Widgets_Icons_Axe,
        Widgets_Icons_Blank,
        Widgets_Icons_Bow,
        Widgets_Icons_Bracelet,
        Widgets_Icons_Chest,
        Widgets_Icons_Club,
        Widgets_Icons_Dark,
        Widgets_Icons_Fire,
        Widgets_Icons_Greatsword,
        Widgets_Icons_Holy,
        Widgets_Icons_Ice,
        Widgets_Icons_Icons,
        Widgets_Icons_Mace,
        Widgets_Icons_Necklace,
        Widgets_Icons_None,
        Widgets_Icons_Potion,
        Widgets_Icons_Robe,
        Widgets_Icons_Shield,
        Widgets_Icons_Slyph,
        Widgets_Icons_Staff,
        Widgets_Icons_Sword,
        Widgets_Icons_Tome,
        Widgets_Icons_Vest,
        Widgets_Images_Pointer,
        Widgets_Images_Proceed,
        Widgets_Images_Settings,
        Widgets_Textplate_ClearPanel,
        Widgets_Textplate_Panel,
        Widgets_Windows_ClearPanel,
        Widgets_Windows_Panel,

        None = -1
    }

    public enum GameMap
    {
        Airship,
        InsideCrystal,
        ManaspringCave,
        ManaspringCave2,
        ManaspringCave3,
        ManaspringCave4,
        ManaspringCave5,
        Overworld,
        TechWorldIntro,
        Tower,
        Tower2,
        Tower3,

        None = -1
    }

}
