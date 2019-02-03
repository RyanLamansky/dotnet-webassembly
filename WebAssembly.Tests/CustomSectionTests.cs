using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace WebAssembly
{
    /// <summary>
    /// Validates behavior of the <see cref="CustomSection"/> class.
    /// </summary>
    [TestClass]
    public class CustomSectionTests
    {
        /// <summary>
        /// Ensures that values supplied to the <see cref="CustomSection.PrecedingSection"/> property are validated.
        /// </summary>
        [TestMethod]
        public void CustomSection_PrecedingSectionValidity()
        {
            var custom = new CustomSection();

            foreach (var value in Enum.GetValues(typeof(Section)).Cast<Section>())
            {
                //All values of Section should be accepted.
                custom.PrecedingSection = value;
            }

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => custom.PrecedingSection = (Section)255);
        }
    }
}