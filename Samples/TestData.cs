using UnityEngine;

namespace Samples
{
    [CreateAssetMenu(fileName = "TestData", menuName = "Tests/TestData", order = 0)]
    public class TestData : ScriptableObject
    {
        [SerializeField] private string _stringField;
        [SerializeField] private int _intField;
    }
}