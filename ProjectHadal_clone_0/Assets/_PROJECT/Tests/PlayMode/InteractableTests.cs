/*using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hadal.Tests
{
    public class InteractableTests
    {
        private static GameObject spawnedObject;

        private class InteractMethod
        {
            private static Interactable interactable;

            [UnityTest]
            public IEnumerator Interact_Changes_Cube_Object_State()
            {
                InteractCube cube = spawnedObject.GetComponent<InteractCube>();
                Material aCheck = interactable.GetComponent<Renderer>().material;
                Material bCheck = null;
                yield return null;
                Material a = cube.cubeColor;
                interactable.Interact();
                bCheck = interactable.GetComponent<Renderer>().material;
                yield return null;

                Assert.AreNotEqual(aCheck, bCheck);
            }

            [UnitySetUp]
            public IEnumerator SetupBeforeEachTest()
            {
                spawnedObject = TestUtils.GetPrefab<GameObject>("Interactable/Cube").Instantiate0().AsGObject();
                yield return null;
                interactable = spawnedObject.GetComponent<Interactable>();
                yield return null;
            }
        }
    }
}*/