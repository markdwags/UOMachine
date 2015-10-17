using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UOMachine;
using UOMachine.Data;
using UOMachine.Macros;

namespace UnitTests
{
    [TestClass]
    public class ClilocTest
    {
        [TestCategory("Data Format"), TestCategory("Data Format\\Cliloc"), TestMethod]
        public void ClilocTest70022()
        {
            string path = @"D:\Clients\7.0.2.2";
            Cliloc.Initialize(path);
            string cliloc = Cliloc.GetProperty(1061638);
            Assert.AreEqual(cliloc, "A House Sign");
            string clilocParam = Cliloc.GetLocalString(1062028, new string[] { "This structure is slightly worn." });
            Assert.AreEqual(clilocParam, "Condition: This structure is slightly worn.");
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Cliloc"), TestMethod]
        public void ClilocTest70200()
        {
            string path = @"D:\Clients\7.0.20.0";
            Cliloc.Initialize(path);
            string cliloc = Cliloc.GetProperty(1061638);
            Assert.AreEqual(cliloc, "A House Sign");
            string clilocParam = Cliloc.GetLocalString(1062028, new string[] { "This structure is slightly worn." });
            Assert.AreEqual(clilocParam, "Condition: This structure is slightly worn.");
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Cliloc"), TestMethod]
        public void ClilocTest70351()
        {
            string path = @"D:\Clients\7.0.35.1";
            Cliloc.Initialize(path);
            string cliloc = Cliloc.GetProperty(1061638);
            Assert.AreEqual(cliloc, "A House Sign");
            string clilocParam = Cliloc.GetLocalString(1062028, new string[] { "This structure is slightly worn." });
            Assert.AreEqual(clilocParam, "Condition: This structure is slightly worn.");
        }

        [TestCategory("Data Format"), TestCategory("Data Format\\Cliloc"), TestMethod]
        public void ClilocTest70450()
        {
            string path = @"D:\Clients\7.0.45.0";
            Cliloc.Initialize(path);
            string cliloc = Cliloc.GetProperty(1061638);
            Assert.AreEqual(cliloc, "A House Sign");
            string clilocParam = Cliloc.GetLocalString(1062028, new string[] { "This structure is slightly worn." });
            Assert.AreEqual(clilocParam, "Condition: This structure is slightly worn.");
        }

        [TestCategory( "Data Format" ), TestCategory( "Data Format\\Cliloc" ), TestMethod]
        public void ClilocTest70462()
        {
            string path = @"D:\Clients\7.0.46.2";
            Cliloc.Initialize( path );
            string cliloc = Cliloc.GetProperty( 1061638 );
            Assert.AreEqual( cliloc, "A House Sign" );
            string clilocParam = Cliloc.GetLocalString( 1062028, new string[] { "This structure is slightly worn." } );
            Assert.AreEqual( clilocParam, "Condition: This structure is slightly worn." );
        }
    }
}
