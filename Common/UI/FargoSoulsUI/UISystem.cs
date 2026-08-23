using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using terraria_gldty.Common.Systems;
using terraria_gldty.Common.ModIntegration.FargoSoulsHelper;

namespace terraria_gldty.Common.UI.FargoSoulsUI
{
    public class UISystem : ModSystem
    {
        public static UISystem Instance => ModContent.GetInstance<UISystem>();
        public UserInterface TransmuterUserInterface;
        public ResonanceTransmuterUI TransmuterUI;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                TransmuterUI = new ResonanceTransmuterUI();
                TransmuterUI.Activate();
                TransmuterUserInterface = new UserInterface();
            }
        }

        public void OpenUI()
        {
            // 【核心修复】打开 UI 时强制初始化，确保 Fargo 列表被加载
            FargoSoulsHelper.Initialize();

            TransmuterUserInterface?.SetState(TransmuterUI);
            TransmuterUI?.RefreshTargetList();
        }

        public void CloseUI()
        {
            // 关闭 UI 时清理还原物品，防止落入槽位
            if (TransmuterUI != null)
            {
                if (!TransmuterUI.InputItem.IsAir)
                {
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("UI_Close"), TransmuterUI.InputItem, TransmuterUI.InputItem.stack);
                    TransmuterUI.InputItem.TurnToAir();
                }
                if (!TransmuterUI.CatalystItem.IsAir)
                {
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("UI_Close"), TransmuterUI.CatalystItem, TransmuterUI.CatalystItem.stack);
                    TransmuterUI.CatalystItem.TurnToAir();
                }
            }

            TransmuterUserInterface?.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (TransmuterUserInterface?.CurrentState != null)
            {
                TransmuterUserInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "terraria_gldty: Resonance Transmuter UI",
                    delegate
                    {
                        if (TransmuterUserInterface?.CurrentState != null)
                        {
                            TransmuterUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}