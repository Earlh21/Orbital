using NUnit.Framework;
using SFML.System;

namespace GravityGame.Test
{
    [TestFixture]
    public class VectorFieldTest
    {
        [Test]
        public void TestGetIndex()
        {
            VectorField field = new VectorField(new Rectangle(0, 0, 4, 4), 4, 4);
            Assert.AreEqual(new Vector2i(0, 0), field.GetIndex(new Vector2f(0.5f, 0.5f)));
            Assert.AreEqual(new Vector2i(1, 0), field.GetIndex(new Vector2f(1.5f, 0.5f)));
            Assert.AreEqual(new Vector2i(0, 1), field.GetIndex(new Vector2f(0.5f, 1.5f)));
            Assert.AreEqual(new Vector2i(3, 3), field.GetIndex(new Vector2f(3.5f, 3.5f)));
            Assert.AreEqual(new Vector2i(2, 0), field.GetIndex(new Vector2f(2.5f, 0.5f)));
        }
    }
}