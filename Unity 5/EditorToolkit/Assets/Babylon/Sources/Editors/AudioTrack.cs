using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Audio Track", 3)]
    public sealed class AudioTrack : EditorScriptComponent
    {
        [Header("-Sound Properties-")]

        public AudioClip audioClip = null;
        public BabylonSoundTrack soundTrack = null;
    }
}