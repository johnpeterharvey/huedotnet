using huedotnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace huedotnettest
{
    /// <summary>
    ///This is a test class for HSVRGBTest and is intended
    ///to contain all HSVRGBTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HSVRGBTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        ///Black - rgb 0,0,0
        ///</summary>
        [TestMethod()]
        public void ConvertToRGBTestBlack()
        {
            HSVRGB target = new HSVRGB();
            int r, g, b;
            target.ConvertToRGB(180.0, 0.0, 0.0, out r, out g, out b);
            Assert.AreEqual(0, r);
            Assert.AreEqual(0, g);
            Assert.AreEqual(0, b);
        }

        /// <summary>
        ///White - rgb 255,255,255
        ///</summary>
        [TestMethod()]
        public void ConvertToRGBTestWhite()
        {
            HSVRGB target = new HSVRGB();
            int r, g, b;
            target.ConvertToRGB(180.0, 0.0, 1.0, out r, out g, out b);
            Assert.AreEqual(255, r);
            Assert.AreEqual(255, g);
            Assert.AreEqual(255, b);
        }

        /// <summary>
        ///Green
        ///</summary>
        [TestMethod()]
        public void ConvertToRGBTestGreen()
        {
            HSVRGB target = new HSVRGB();
            int r, g, b;
            target.ConvertToRGB(120.0, 1.0, 0.250, out r, out g, out b);
            Assert.AreEqual(0, r);
            Assert.AreEqual(127, g);
            Assert.AreEqual(0, b);
        }

        /// <summary>
        ///Red
        ///</summary>
        [TestMethod()]
        public void ConvertToRGBTestRed()
        {
            HSVRGB target = new HSVRGB();
            int r, g, b;
            target.ConvertToRGB(0.0, 1.0, 0.5, out r, out g, out b);
            Assert.AreEqual(255, r);
            Assert.AreEqual(0, g);
            Assert.AreEqual(0, b);
        }
    }
}
