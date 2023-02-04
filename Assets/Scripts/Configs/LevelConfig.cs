using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu]
    public class LevelConfig : ScriptableObject
    {
        public List<EnemyConfig> Enemies;
    }
}