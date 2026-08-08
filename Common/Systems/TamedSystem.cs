using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace terraria_gldty.Common.Systems
{
    public class TamedSystem : ModSystem
    {
        public static TamedSystem Instance { get; private set; } // 新增单例
        public static ModKeybind OpenBookKeybind { get; private set; }
        internal UI.TamedUI tamedUI;
        private UserInterface _tamedUserInterface;

        public override void Load() {
            OpenBookKeybind = KeybindLoader.RegisterKeybind(Mod, "打开灵魂手册", "K");

            if (!Main.dedServ) {
                tamedUI = new UI.TamedUI();
                tamedUI.Activate();
                _tamedUserInterface = new UserInterface();
            }
        }
        public override void Unload()
        {
            Instance = null;
        }
        public override void UpdateUI(GameTime gameTime) {
            if (_tamedUserInterface?.CurrentState != null) {
                _tamedUserInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "TamedMinions: TamedBookUI",
                    delegate {
                        if (_tamedUserInterface?.CurrentState != null) {
                            _tamedUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public static void ToggleUI() {
            var system = ModContent.GetInstance<TamedSystem>();
            if (system._tamedUserInterface.CurrentState == null) {
                system.tamedUI.RefreshGrid();
                system._tamedUserInterface.SetState(system.tamedUI);
            } else {
                system._tamedUserInterface.SetState(null);
            }
        }
    }

    public class TamedKeyPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (TamedSystem.OpenBookKeybind.JustPressed) {
                TamedSystem.ToggleUI();
            }
        }
    }
}