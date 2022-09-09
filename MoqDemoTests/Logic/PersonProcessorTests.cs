using System;
using System.Collections.Generic;
using Autofac.Extras.Moq;
using DemoLibrary.Logic;
using DemoLibrary.Models;
using DemoLibrary.Utilities;
using Moq;
using NUnit.Framework;

namespace MoqDemoTests.Logic
{
    // Nuget packages: Moq and Autofac
    [TestFixture]
    public class PersonProcessorTests
    {
        [TestCase("6'8\"", true, 80)]
        [TestCase("6\"8'", false, 0)]
        [TestCase("six'eight\"", false, 0)]
        public void ConvertHeightTextToInches_VariousOptions(string heightText, bool expectedIsValid, double expectedHeightInInches)
        {
            PersonProcessor processor = new PersonProcessor(null);

            var actual = processor.ConvertHeightTextToInches(heightText);

            Assert.AreEqual(expectedIsValid, actual.isValid);
            Assert.AreEqual(expectedHeightInInches, actual.heightInInches);
        }

        [TestCase("Tim", "Corey", "6'8\"", 80)]
        [TestCase("Charitry", "Corey", "5'4\"", 64)]
        public void CreatePerson_Successful(string firstName, string lastName, string heightText, double expectedHeight)
        {
            PersonProcessor processor = new PersonProcessor(null);

            PersonModel expected = new PersonModel
            {
                FirstName = firstName,
                LastName = lastName,
                HeightInInches = expectedHeight,
                Id = 0
            };

            var actual = processor.CreatePerson(firstName, lastName, heightText);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
            Assert.AreEqual(expected.HeightInInches, actual.HeightInInches);

        }

        [TestCase("Tim#", "Corey", "6'8\"", "firstName")]
        [TestCase("Charitry", "C88ey", "5'4\"", "lastName")]
        [TestCase("Jon", "Corey", "SixTwo", "heightText")]
        [TestCase("", "Corey", "5'11\"", "firstName")]
        public void CreatePerson_ThrowsException(string firstName, string lastName, string heightText, string expectedInvalidParameter)
        {
            PersonProcessor processor = new PersonProcessor(null);

            ArgumentException ex = Assert.Throws<ArgumentException>(() => processor.CreatePerson(firstName, lastName, heightText));
            if (ex is ArgumentException argEx)
            {
                Assert.AreEqual(expectedInvalidParameter, argEx.ParamName);
            }
        }

        [Test]
        public void LoadPeople_ValidCall()
        {
            var expected = GetSamplePeople();
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<ISqliteDataAccess>()
                    .Setup(x => x.LoadData<PersonModel>("select * from Person"))
                    .Returns(expected);

                PersonProcessor cut = mock.Create<PersonProcessor>();

                var actual = cut.LoadPeople();
                Assert.True(actual != null);
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void SavePerson_ValidCall()
        {
            PersonModel person = GetSamplePerson();
            string sql = "insert into Person (FirstName, LastName, HeightInInches) " + "values (@FirstName, @LastName, @HeightInInches)";

            using (var mock = AutoMock.GetLoose())
            {
                var m = mock.Mock<ISqliteDataAccess>();
                m.Setup(x => x.SaveData<PersonModel>(person, sql));

                PersonProcessor cut = mock.Create<PersonProcessor>();
                cut.SavePerson(person);

                m.Verify(x => x.SaveData(person, sql), Times.Once);
            }
        }

        private List<PersonModel> GetSamplePeople()
        {
            return new List<PersonModel> { new PersonModel { Id = 1, FirstName = "Vlad", LastName = "Bogo" } };
        }

        private PersonModel GetSamplePerson() => new PersonModel { Id = 1, FirstName = "Vlad", LastName = "Bogo", HeightInInches = 80 };
    }
}
