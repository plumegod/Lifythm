using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioSystem._Scripts.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace AudioSystem._Scripts.UseExample
{
    public class AudioExample : MonoBehaviour
    {
        [FormerlySerializedAs("audioType")] [SerializeField]private EasyAudioType easyAudioType;
        private List<string> audioNames;

        private void Awake()
        {
            audioNames = new List<string>();
        }

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                var audioName = EasyAudioSystem.Instance.CreateAudioSource(easyAudioType);
                audioNames.Add(audioName);
                Debug.Log(audioName);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                await EasyAudioSystem.Instance.ChargeAudioSource(audioNames[0], AudioState.Play);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                EasyAudioSystem.Instance.RemoveAudioSource(audioNames[0]);
                audioNames.RemoveAt(0);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                var newaudioType = Instantiate(easyAudioType);
                await EasyAudioSystem.Instance.FastAudioSource(newaudioType);
            }
        }
    }
}