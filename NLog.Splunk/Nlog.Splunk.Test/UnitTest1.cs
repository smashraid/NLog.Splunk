using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Config;
using NLog.Fluent;
using NLog.Splunk;
using NUnit.Framework;

namespace Nlog.Splunk.Test
{
    [TestFixture]
    public class LogTest
    {
        [SetUp]
        public void Init()
        {

            ConfigurationItemFactory.Default.Targets.RegisterDefinition("Splunk", typeof(SplunkTarget));
        }

        [Test]
        public void TestMethod1()
        {
            Log.Debug()
                .Message("This is a test")
                .Property("New", "NewProp1")
                .Property("Test.Property1", "TestVal1")
                .Property("Other.Prop3", "OtherVal3")
                .Property("Test.Property2", "TestVal2")
                .Property("Test.Property3", "TestVal3")
                .Property("Other.Prop1", "OtherVal1")
                .Property("Other.Prop2", "OtherVal2")
                .Property("New", "NewProp2")
                .Write();
        }
    }
}
