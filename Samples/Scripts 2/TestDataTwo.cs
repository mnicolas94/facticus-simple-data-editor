using System.Collections.Generic;
using UnityEngine;

namespace Samples
{
    [CreateAssetMenu(fileName = "TestData2", menuName = "Tests/TestData2", order = 0)]
    public class TestDataTwo : ScriptableObject
    {
        [SerializeField] private List<int> _intList;
    }
}