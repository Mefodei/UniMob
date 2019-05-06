using UnityEngine;

namespace UniMob.Pools
{
    public sealed class PoolID : MonoBehaviour
    {
        [SerializeField] private int prefabInstanceID;

        // ReSharper disable once ConvertToAutoProperty
        public int PrefabInstanceID
        {
            get => prefabInstanceID;
            set => prefabInstanceID = value;
        }
    }
}