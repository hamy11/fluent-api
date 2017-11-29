using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 10.7, Id = new Guid()};
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(x => x.ToUpper())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name).Using(x => x.PadLeft(6))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(4)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Exclude(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }


        [Test]
        public void ShouldExcludeTypes_WhenExcludingDouble()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<double>();
            var str = printer.PrintToString(person);
            str.Should().NotContain("Height");
        }

        [Test]
        public void ShouldAppendCulture_WhenNumericProperty()
        {
            var culture = new CultureInfo("pt-BR");
            var cultured = person.Age.ToString(culture);
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(culture);
            var str = printer.PrintToString(person);
            str.Should().Contain(cultured);
        }

        [Test]
        public void ShouldExcludeProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(x => x.Age);
            var str = printer.PrintToString(person);
            str.Should().NotContain("Age");
        }

        [Test]
        public void ShouldAppendSerializationType_WhenStringType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.ToUpper());
            var str = printer.PrintToString(person);
            str.Should().Contain("ALEX");
        }

        [Test]
        public void ShouldAppendSerializationType_WhenChooseProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).Using(x => x + x + x);
            var str = printer.PrintToString(person);
            str.Should().Contain("AlexAlexAlex");
        }

        [Test]
        public void ShouldTrimm_WhenStringType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.ToUpper())
                .Printing<string>().TrimmedToLength(2);
            var str = printer.PrintToString(person);
            str.Should().Contain("AL").And.NotContain("ALEX");
        }

        [Test]
        public void ShouldTrimm_AfterTypePrintingFunctions()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.Insert(0, "pasted"))
                .Printing(p => p.Name).Using(x => x + x + x)
                .Printing<string>().TrimmedToLength(13);
            var str = printer.PrintToString(person);
            str.Should().Contain($"Name = pastedAlexAle{Environment.NewLine}");
        }
    }
}