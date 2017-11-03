using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Audio Track", 10)]
    public sealed class AudioTrack : EditorScriptComponent
    {
        [Header("[Sound Properties]")]

        public AudioClip audioClip = null;
        public BabylonSoundTrack soundTrack = null;
    }
}