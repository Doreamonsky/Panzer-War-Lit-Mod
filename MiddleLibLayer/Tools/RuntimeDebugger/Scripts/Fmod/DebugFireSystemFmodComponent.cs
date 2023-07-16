using ShanghaiWindy.Core;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class DebugFireSystemFmodComponent : BaseFireSystemFmodComponent
    {
        public FmodSoundDebugger debugger;

        public void Initialize()
        {
            OnInitialized();
        }
        
        protected override BulletFmodSound GetBulletSound()
        {
            return debugger.bulletSound;
        }

        protected override Transform GetFFPoint()
        {
            return debugger.transform;
        }

        protected override bool IsZoomState()
        {
            return debugger.isZoom;
        }

        protected override bool IsBot()
        {
            return debugger.isBot;
        }
    }
}