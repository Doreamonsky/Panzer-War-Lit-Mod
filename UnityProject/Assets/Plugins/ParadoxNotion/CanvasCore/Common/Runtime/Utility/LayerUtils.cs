/// Modified version of https://wiki.unity3d.com/index.php/LayerMaskExtensions

using UnityEngine;
using System.Collections.Generic;

namespace ParadoxNotion
{
    public static class LayerUtils
    {
        ///Create LayerMask from layer names
        public static LayerMask CreateFromNames(params string[] layerNames) { return LayerNamesToMask(layerNames); }

        ///Create LayerMask from layer numbers
        public static LayerMask CreateFromNumbers(params int[] layerNumbers) { return LayerNumbersToMask(layerNumbers); }

        ///Layer names to LayerMask
        public static LayerMask LayerNamesToMask(params string[] layerNames) {
            LayerMask ret = (LayerMask)0;
            foreach ( var name in layerNames ) {
                ret |= ( 1 << LayerMask.NameToLayer(name) );
            }
            return ret;
        }

        ///Layer numbers to LayerMask
        public static LayerMask LayerNumbersToMask(params int[] layerNumbers) {
            LayerMask ret = (LayerMask)0;
            foreach ( var layer in layerNumbers ) {
                ret |= ( 1 << layer );
            }
            return ret;
        }

        ///Inverse LayerMask
        public static LayerMask Inverse(this LayerMask mask) { return ~mask; }

        ///Adds layer names to LayerMask
        public static LayerMask AddToMask(this LayerMask mask, params string[] layerNames) { return mask | LayerNamesToMask(layerNames); }

        ///Remove layer names from LayerMask
        public static LayerMask RemoveFromMask(this LayerMask mask, params string[] layerNames) {
            LayerMask invertedOriginal = ~mask;
            return ~( invertedOriginal | LayerNamesToMask(layerNames) );
        }

        ///Does LayerMask contain any target layers (by name)
        public static bool ContainsAnyLayer(this LayerMask mask, params string[] layerNames) {
            if ( layerNames == null ) { return false; }
            for ( var i = 0; i < layerNames.Length; i++ ) {
                if ( mask == ( mask | ( 1 << LayerMask.NameToLayer(layerNames[i]) ) ) ) {
                    return true;
                }
            }
            return false;
        }

        ///Does LayerMask contain all target layers (by name)
        public static bool ContainsAllLayers(this LayerMask mask, params string[] layerNames) {
            if ( layerNames == null ) { return false; }
            for ( var i = 0; i < layerNames.Length; i++ ) {
                if ( !( mask == ( mask | ( 1 << LayerMask.NameToLayer(layerNames[i]) ) ) ) ) {
                    return false;
                }
            }
            return true;
        }

        ///Return layer names in/from LayerMask
        public static string[] MaskToNames(this LayerMask mask) {
            var output = new List<string>();

            for ( int i = 0; i < 32; ++i ) {
                int shifted = 1 << i;
                if ( ( mask & shifted ) == shifted ) {
                    string layerName = LayerMask.LayerToName(i);
                    if ( !string.IsNullOrEmpty(layerName) ) {
                        output.Add(layerName);
                    }
                }
            }
            return output.ToArray();
        }

        ///Redable LayerMask names
        public static string MaskToString(this LayerMask mask) { return MaskToString(mask, ", "); }

        ///Redable LayerMask names by delimiter
        public static string MaskToString(this LayerMask mask, string delimiter) { return string.Join(delimiter, MaskToNames(mask)); }
    }
}