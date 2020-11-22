using Heels.Handler;
using KKAPI;
using KKAPI.Chara;
using Util.Log;

namespace Heels.Controller
{
    public class HeelsController : CharaCustomFunctionController
    {
        private HeelsHandler _handler;

        public HeelsHandler Handler => _handler ?? (_handler = new HeelsHandler(ChaControl));

        public bool GroundAnim { get; set; }

        protected override void OnCardBeingSaved(GameMode currentGameMode)
        {
        }

        protected override void OnReload(GameMode currentGameMode, bool maintainState)
        {
            ApplyHeelsData();
        }

        public void ApplyHeelsData()
        {
            if (ChaControl == null) return;
            var shoeID = ChaControl.nowCoordinate.clothes.parts[Constant.ShoeCategory].id;

            Logger.Log($"Looking for ID: \"{shoeID}\"");
            if (Values.Configs.TryGetValue(shoeID, out var shoeConfig))
                Handler.SetConfig(shoeConfig);
            else
                Handler.SetConfig();
        }

        public void DisableHover()
        {
            Handler.Hover(false);
        }

        public void EnableHover()
        {
            Handler.Hover(true);
        }


        public void UpdateHover()
        {
            Handler.UpdateStatus();
        }
    }
}