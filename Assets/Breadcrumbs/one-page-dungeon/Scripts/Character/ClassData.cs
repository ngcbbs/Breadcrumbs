using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.Character
{
    /// <summary>
    /// Data definition for character classes
    /// </summary>
    [CreateAssetMenu(fileName = "ClassData", menuName = "Breadcrumbs/Class Data")]
    public class ClassData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private ClassType classType;
        [SerializeField] private string className;
        [SerializeField] private string classDescription;
        
        [Header("Visual Elements")]
        [SerializeField] private Sprite classIcon;
        [SerializeField] private GameObject classModel;
        
        [Header("Base Stats")]
        [SerializeField] private int baseStrength;
        [SerializeField] private int baseDexterity;
        [SerializeField] private int baseIntelligence;
        [SerializeField] private int baseVitality;
        [SerializeField] private int baseWisdom;
        [SerializeField] private int baseLuck;
        
        [Header("Growth Stats")]
        [SerializeField] private float strengthGrowth;
        [SerializeField] private float dexterityGrowth;
        [SerializeField] private float intelligenceGrowth;
        [SerializeField] private float vitalityGrowth;
        [SerializeField] private float wisdomGrowth;
        [SerializeField] private float luckGrowth;
        
        [Header("Starting Skills")]
        [SerializeField] private List<string> startingSkills = new List<string>();
        
        [Header("Equipment Restrictions")]
        [SerializeField] private List<string> usableWeaponTypes = new List<string>();
        [SerializeField] private List<string> usableArmorTypes = new List<string>();
        
        [Header("Special Traits")]
        [SerializeField] private List<ClassTrait> specialTraits = new List<ClassTrait>();
        
        // Properties
        public ClassType ClassType => classType;
        public string ClassName => className;
        public string ClassDescription => classDescription;
        public Sprite ClassIcon => classIcon;
        public GameObject ClassModel => classModel;
        
        public int BaseStrength => baseStrength;
        public int BaseDexterity => baseDexterity;
        public int BaseIntelligence => baseIntelligence;
        public int BaseVitality => baseVitality;
        public int BaseWisdom => baseWisdom;
        public int BaseLuck => baseLuck;
        
        public float StrengthGrowth => strengthGrowth;
        public float DexterityGrowth => dexterityGrowth;
        public float IntelligenceGrowth => intelligenceGrowth;
        public float VitalityGrowth => vitalityGrowth;
        public float WisdomGrowth => wisdomGrowth;
        public float LuckGrowth => luckGrowth;
        
        public IReadOnlyList<string> StartingSkills => startingSkills;
        public IReadOnlyList<string> UsableWeaponTypes => usableWeaponTypes;
        public IReadOnlyList<string> UsableArmorTypes => usableArmorTypes;
        
        /// <summary>
        /// Get a class trait value
        /// </summary>
        /// <param name="traitName">The name of the trait</param>
        /// <param name="defaultValue">Default value if trait not found</param>
        /// <returns>The trait value or default</returns>
        public float GetTraitValue(string traitName, float defaultValue = 0f)
        {
            foreach (var trait in specialTraits)
            {
                if (trait.TraitName == traitName)
                {
                    return trait.TraitValue;
                }
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Represents a special class trait
        /// </summary>
        [System.Serializable]
        public class ClassTrait
        {
            [SerializeField] private string traitName;
            [SerializeField] private float traitValue;
            [SerializeField] private string description;
            
            public string TraitName => traitName;
            public float TraitValue => traitValue;
            public string Description => description;
        }
    }
}
