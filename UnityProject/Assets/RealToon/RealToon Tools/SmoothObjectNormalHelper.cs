//SmoothObjectNomal - Helper
//MJQStudioWorks
//©2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealToon.Script
{
    [ExecuteInEditMode]
    [AddComponentMenu("RealToon/Tools/SmoothObjectNormal - Helper")]
    public class SmoothObjectNormalHelper : MonoBehaviour
    {
        [Space(25)]
        [Header("Note: Put the Object Helper inside the root bone or the root object and set it's XYZ position to 0.")]
        [Header("[RealToon - Smooth Object Normal - Helper]")]

        [Space(10)]
        [Tooltip("A material that uses 'RealToon - Smooth Object Normal' feature.")]
        public Material Material = null;

        [Tooltip("An object to help adjust the smoothed/ignored object normal.")]
        public Transform ObjectHelper = null;

        [Tooltip("The object to followed by the Object Helper")]
        public Transform TheObjectToFollow = null;

        [Space(10)]
        [Tooltip("Adjust the overall offset of the Smooth Object normal to follow the Object Helper.")]
        public float Offset = 10.0f;


        [Space(10)]
        [Tooltip("Additional position adjustment for Object Helper.")]
        public Vector3 AdditionalPositionAdjustment = new Vector3(0, 0, 0);


        void LateUpdate()
        {

            if (Material == null || ObjectHelper == null || TheObjectToFollow == null)
            { }
            else
            {
                Vector3 ObjPos = new Vector3(-ObjectHelper.transform.localPosition.x, -ObjectHelper.transform.localPosition.y, -ObjectHelper.transform.localPosition.z);
                Material.SetVector("_XYZPosition", ObjPos * Offset);
                ObjectHelper.position = TheObjectToFollow.position;

                ObjectHelper.position += AdditionalPositionAdjustment * 0.01f;
            }

        }

    }
}
