using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace terraria_gldty.Common.UI.EnchantingWorkshopUI
{
    [Autoload(Side = ModSide.Client)]
    public class EnchantingWorkshopUISystem : ModSystem
    {
        public static EnchantingWorkshopUISystem Instance { get; private set; }

        private UserInterface _userInterface;
        internal EnchantingWorkshopUI _ui;

        public void ShowUI() {
            if (_ui != null) {
                _ui.OpenUI();
            }
            _userInterface?.SetState(_ui);
        }

        public void HideUI() {
            _userInterface?.SetState(null);
        }

        public override void PostSetupContent() {
            Instance = this;
            _userInterface = new UserInterface();
            _ui = new EnchantingWorkshopUI();
            _ui.Activate();
        }

        public override void UpdateUI(GameTime gameTime) {
            if (_userInterface?.CurrentState != null) {
                _userInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "terraria_gldty: EnchantingWorkshopUI",
                    delegate {
                        if (_userInterface?.CurrentState != null) {
                            _userInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
