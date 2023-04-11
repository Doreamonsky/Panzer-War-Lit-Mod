using System;
using ShanghaiWindy.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class FmodSoundDebugger : MonoBehaviour
    {
        public enum eDebugType
        {
            BulletSound
        }

        [SHWLabelText("调试类型 - Debug Type")] public eDebugType debugType = eDebugType.BulletSound;

        [SHWLabelText("是否人机 - Is Bot")] public bool isBot = false;

        private bool IsBulletSound => debugType == eDebugType.BulletSound;

        [SHWLabelText("开火音效 - Bullet Sound")] [ShowIf(nameof(IsBulletSound))]
        public BulletFmodSound bulletSound;

        [SHWLabelText("射击间隔 - Fire Invernal")] [ShowIf(nameof(IsBulletSound))]
        public float fireInterval = 10f;

        [SHWLabelText("是否开镜 - Is Zoom")] [ShowIf(nameof(IsBulletSound))]
        public bool isZoom = false;

        private DebugFireSystemFmodComponent m_FireComponent;

        private void Awake()
        {
            EditorModeStartup.OnInit += () =>
            {
                if (IsBulletSound)
                {
                    StartCoroutine(AsyncCreateBulletSound());
                }
            };
        }

        private IEnumerator AsyncCreateBulletSound()
        {
            m_FireComponent = new DebugFireSystemFmodComponent
            {
                debugger = this
            };
            m_FireComponent.OnInitialized(null);

            while (true)
            {
                m_FireComponent.Play();
                yield return new WaitForSeconds(fireInterval);
            }
        }


        private void Update()
        {
            if (m_FireComponent != null)
            {
                m_FireComponent.OnUpdated();
            }
        }

        private void OnDestroy()
        {
            if (m_FireComponent != null)
            {
                m_FireComponent.OnDestroyed();
            }
        }
    }
}