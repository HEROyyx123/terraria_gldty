using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace terraria_gldty.Common.UI.BlackHoleStoneUI
{
    [Autoload(Side = ModSide.Client)]
    public class BlackHoleStoneUISystem : ModSystem
    {
        private UserInterface _userInterface;
        internal BlackHoleStoneUI _ui;

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
            _userInterface = new UserInterface();
            _ui = new BlackHoleStoneUI();
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
                    "terraria_gldty: BlackHoleStoneUI",
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
