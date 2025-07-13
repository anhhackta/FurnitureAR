using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "Furniture/FurnitureDatabase")]
public class FurnitureDatabase : ScriptableObject
{
    [System.Serializable]
    public class FurnitureData
    {
        public string name;
        public GameObject prefab;
        public Material[] availableMaterials;
        public Sprite thumbnail;
    }

    public FurnitureData[] furnitureItems;
}