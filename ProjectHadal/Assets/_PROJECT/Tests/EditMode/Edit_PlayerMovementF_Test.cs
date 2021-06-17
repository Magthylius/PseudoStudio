using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hadal.Locomotion;

namespace Hadal.UnitTests.EditMode
{
    public class Edit_PlayerMovementF_Test
    {
        public class Drag_Calculation
        {
            [SetUp]
            public void Before_Every_Test()
            {
                
            }

            [Test]
            public void One_Is_One()
            {
                //! Arrange
                int variableA = 1;
                int variableB = 1;

                //! Act
                // logic
                var i = variableA + variableB;

                //! Assert

                Assert.AreEqual(1, 1);
            }

            [Test]
            public void Three_Is_Four()
            {
                Assert.AreNotEqual(3, 4);
            }
        }

        public class Physics
        {

        }
    }
}
