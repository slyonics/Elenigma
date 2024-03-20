namespace Elenigma.Main
{
    public enum GameView
    {
        BattleScene_BattleView,
        BattleScene_CommandView,
        BattleScene_TargetView,
        ConversationScene_ConversationView,
        ConversationScene_ConversationView2,
        ConversationScene_ConversationView3,
        ConversationScene_SelectionView,
        CrawlerScene_MapView,
        CreditsScene_CreditsView,
        IntroScene_IntroView,
        StatusScene_ItemView,
        StatusScene_StatusView,
        StatusScene_SystemView,
        TitleScene_ContinueView,
        TitleScene_SettingsView,
        TitleScene_TitleView,

        None = -1
    }

    public enum GameSound
    {
        Back,
        Blip,
        Chest,
        Confirm,
        Cursor,
        dialogue_auto_scroll,
        Door,
        Error,
        footsteps_dirt_1,
        footsteps_dirt_2,
        footsteps_dirt_3,
        footsteps_dirt_4,
        footsteps_grass_1,
        footsteps_grass_2,
        footsteps_grass_3,
        footsteps_grass_4,
        GetItem,
        Pickup,
        Ready,
        Selection,
        Slash,
        Slash1,
        Slash2,
        Slash3,
        Slash4,
        Talk,
        wall_bump,
        wall_enter,
        Wood,

        None = -1
    }

    public enum GameMusic
    {
        Awakening,
        Elenigma,
        Home,
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
        ItemData,

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
        Wall,
        WallPlus,

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
        MiniMap,
        Target,
        Title,
        TurningBook,
        Up,
        UpSelected,
        YouAreHere,
        Actors_AdultMC,
        Actors_BigDogFamiliar,
        Actors_Blank,
        Actors_Cat,
        Actors_Chest,
        Actors_Dad,
        Actors_DogFamiliar,
        Actors_DroneShadow,
        Actors_HumanFamiliar,
        Actors_Mom,
        Actors_NPCs,
        Actors_Rival,
        Actors_Sis,
        Actors_Slyph,
        Actors_Stump,
        Actors_Sword,
        Actors_Townie2,
        Actors_Townie3,
        Actors_Townie4,
        Actors_Twinkle,
        Actors_Undine,
        Actors_WaterMC,
        Actors_WindMC,
        Actors_YoungMC,
        Background_Blank,
        Background_Montage,
        Background_Splash,
        Background_Title,
        Particles_Exclamation,
        Particles_Gust,
        Particles_Miasma,
        Particles_Smoke,
        Portraits_AdultMC,
        Portraits_Barbarian,
        Portraits_BigDogFamiliar,
        Portraits_DarkGoddess,
        Portraits_DivineBeast,
        Portraits_DogFamiliar,
        Portraits_Druid,
        Portraits_HumanFamiliar,
        Portraits_IceMage,
        Portraits_LightGod,
        Portraits_Map,
        Portraits_MontageBackground,
        Portraits_MontageRunning1,
        Portraits_MontageRunning2,
        Portraits_Regions,
        Portraits_Slyph,
        Portraits_Sword,
        Portraits_Undine,
        Portraits_YoungMC,
        Tiles_GroundTiles,
        Tiles_TownHouses1,
        Walls_BlackboardClassroomWall,
        Walls_Blank,
        Walls_ClassroomFloor,
        Walls_ClassroomWall,
        Walls_DoorClassroomWall,
        Walls_DoorFoyerWall,
        Walls_DoubleDoorFoyerWall,
        Walls_Foundry_C_Base,
        Walls_Foundry_C_Lit,
        Walls_Foundry_D_Base,
        Walls_Foundry_D_BaseDark,
        Walls_Foundry_D_SPC,
        Walls_Foundry_F_Base,
        Walls_Foundry_F_Lit,
        Walls_Foundry_W_Base,
        Walls_Foundry_W_BaseFlat,
        Walls_Foundry_W_Dark,
        Walls_Foundry_W_DarkFlat,
        Walls_Foundry_W_DarkPipes,
        Walls_Foundry_W_DarkPipesB,
        Walls_FoyerFloor,
        Walls_LightCeiling,
        Walls_LockerFoyerWall,
        Walls_Office_C_Base,
        Walls_Office_C_Hole,
        Walls_Office_C_HoleB,
        Walls_Office_C_HoleC,
        Walls_Office_C_Light,
        Walls_Office_D_Base,
        Walls_Office_D_Overgrown,
        Walls_Office_D_SPC,
        Walls_Office_F_Base,
        Walls_Office_F_Petals,
        Walls_Office_F_PetalsB,
        Walls_Office_W_Base,
        Walls_Office_W_PL1,
        Walls_Office_W_W1,
        Walls_Office_W_W2,
        Walls_PlainCeiling,
        Walls_PlainFoyerWall,
        Walls_WindowClassroomWall,
        Walls_WindowFoyerWall,
        Widgets_BattleFrame,
        Widgets_BattleWindow,
        Widgets_Blank,
        Widgets_ChatBoxes,
        Widgets_ClassicFrame,
        Widgets_ClassicWindow,
        Widgets_Convobox,
        Widgets_DarkFrame,
        Widgets_LabelGlow,
        Widgets_LightFrame,
        Widgets_MagicFrame,
        Widgets_MagicFrameSelected,
        Widgets_MagicLabel,
        Widgets_MagicSelected,
        Widgets_MagicWindow,
        Widgets_MapFrame,
        Widgets_MonFrame,
        Widgets_MonSelected,
        Widgets_PartyLead,
        Widgets_PokeFrame,
        Widgets_PokeFrameSelected,
        Widgets_PortraitFrame,
        Widgets_SelectedFrame,
        Widgets_SmallConvo,
        Widgets_TechFrame,
        Widgets_TechFrameSelected,
        Widgets_TechGlow,
        Widgets_TechLabel,
        Widgets_TechSelected,
        Widgets_TechWindow,
        Widgets_ThinPanel,
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
        Widgets_Icons_Undine,
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
        SecretMeadow,
        TechWorldIntro,
        TechWorldRuins,
        TutorialMap,

        None = -1
    }

}
