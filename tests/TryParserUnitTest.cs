using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace tests
{
    [TestClass]
    public class TryParserUnitTest
    {
        [TestMethod]
        public void DateTimeMustBeNullForIncorrectFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("13-13-2013");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DateTimeMustReturnNullWhenLargeDouble()
        {
            //Arrange+Act
            var result = TryParser.DateTime("20120401.0");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DateTimeMustReturnValidDateForLargestDouble()
        {
            //Arrange+Act
            var result = TryParser.DateTime("2958465");
            //Assert
            Assert.AreEqual(DateTime.MaxValue.Date, result);
        }


        [TestMethod]
        public void DateTimeMustReturnValidDateForSmallestDouble()
        {
            //Arrange+Act
            var result = TryParser.DateTime("-693593");
            //Assert
            Assert.AreEqual(DateTime.MinValue, result);
        }

        [TestMethod]
        public void DateTimeMustReturnNullWhenLargeNegativeDouble()
        {
            //Arrange+Act
            var result = TryParser.DateTime("-693594");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DateTimeMustWorkWithYYYYMMDDSlashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("2013/05/13");
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DateTimeMustWorkWithDDMMYYYSlashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("13/05/2013");
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DateTimeMustBeNullForMMDDYYYSlashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("05/13/2013");
            //Assert
            Assert.IsNull(result);
        }


        [TestMethod]
        public void DateTimeMustWorkWithYYYYMMDDDashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("2013-05-13");
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DateTimeMustWorkWithDDMMYYYDashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("13-05-2013","dd-MM-yyyy");
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DateTimeMustBeNullForMMDDYYYDashFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("05-13-2013");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DateTimeMustWorkWithUTCFormat()
        {
            //Arrange+Act
            var result = TryParser.DateTime("2013-05-13T00:00:00");
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DoubleDateTimeMustAlsoWork()
        {
            //Arrange+Act
            var result = TryParser.DateTime(41407);
            //Assert
            Assert.AreEqual(new DateTime(2013, 5, 13), result);
        }

        [TestMethod]
        public void DateTimeEmptyStringMustReturnNull()
        {
            //Arrange+Act
            var result = TryParser.DateTime("");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DecimalValueMustBeConvertable()
        {
            //Arrange+Act
            var result = TryParser.Nullable<decimal>("1234.12345");
            //Assert
            Assert.AreEqual(1234.12345M, result);
        }

        [TestMethod]
        public void InvalidValueForDecimalMustBeNull()
        {
            //Arrange+Act
            var result = TryParser.Nullable<decimal>("1234.12345M");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void DecimalEmptyStringMustBeNull()
        {
            //Arrange+Act
            var result = TryParser.Nullable<decimal>("");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EnumValueMustBeConvertable()
        {
            //Arrange+Act
            var result = TryParser.Nullable<CultureTypes>("AllCultures");
            //Assert
            Assert.AreEqual(CultureTypes.AllCultures, result);
        }

        [TestMethod]
        public void InvalidEnumValueMustBeNull()
        {
            //Arrange+Act
            var result = TryParser.Nullable<CultureTypes>("AllCltures");
            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EmptyStringForEnumValueMustBeNull()
        {
            //Arrange+Act
            var result = TryParser.Nullable<CultureTypes>("");
            //Assert
            Assert.IsNull(result);
        }
    }
}