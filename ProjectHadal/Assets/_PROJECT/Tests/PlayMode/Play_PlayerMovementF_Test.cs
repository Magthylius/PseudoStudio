using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hadal.Locomotion;

namespace Hadal.UnitTests.PlayMode
{
    public class Play_PlayerMovementF_Test
    {
        private GameObject testObj = null;
        private PlayerMovementF mover = null;

        [UnitySetUp]
        public IEnumerator Before_Every_Test()
        {
            testObj = new GameObject("Hoi There");
            yield return null;
            mover = testObj.AddComponent<PlayerMovementF>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After_Every_Test()
        {
            Object.Destroy(testObj);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Drag_Calculation_Is_Accurate()
        {
            //! Arrange
            mover.Accel.MaxCummulation = 40.0f;
            mover.Speed.Max = 20.0f;

            //! Act
            mover.CalculateDrag();

            //! Assert
            Assert.AreEqual(2.0f, mover.CalculatedDrag, 0.05f);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Buoyancy_Calculation_Is_Accurate()
        {
            //! Arrange
            mover.Accel.MaxCummulation = 40.0f;
            mover.Speed.Max = 20.0f;

            //! Act
            mover.CalculateDrag();

            //! Assert
            Assert.AreEqual(2.0f, mover.CalculatedDrag, 0.05f);

            yield return null;
        }
    }
}
