using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShanghaiWindy.Editor.PlayMode
{
    public class ActivateAssetHandler : MonoBehaviour
    {
        public GameObject[] targets;

        private void Awake()
        {
            foreach (var target in targets)
            {
                target.SetActive(false);
            }
        }

        private void Start()
        {
            EditorModeStartup.OnInit += () =>
            {
                foreach (var target in targets)
                {
                    target.SetActive(true);
                }
            };
        }
    }
}