using System;
using System.Diagnostics;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NWaveform
{
    [TestFixtureFor(typeof(DelegateCommand))]
    // ReSharper disable InconsistentNaming
    internal class DelegateCommand_Should
    {
        [Test]
        public void Validate_Parameter()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand((Action)null));
            Assert.Throws<ArgumentNullException>(() => new DelegateCommand(null, Substitute.For<Predicate<object>>()));
        }

        [Test]
        public void Add_And_Remove_Events()
        {
            var action = Substitute.For<Action<object>>();
            var sut = new DelegateCommand(action);

            EventHandler handler = (sender, args) => { };
            sut.CanExecuteChanged += handler;
            sut.CanExecuteChanged -= handler;
        }

        [Test]
        public void Not_Call_Event()
        {
            var dummy = Substitute.For<object>();
            var action = Substitute.For<Action<object>>();
            var sut = new DelegateCommand(action);

            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            EventHandler handler = (sender, args) => dummy.ToString();
            sut.CanExecuteChanged += handler;
            sut.CanExecuteChanged -= handler;
            dummy.DidNotReceiveWithAnyArgs().ToString();
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        [Test]
        public void Execute_Action_If_Predicate_Is_True()
        {
            var obj = new object();
            var pred = Substitute.For<Predicate<object>>();
            pred.Invoke(obj).ReturnsForAnyArgs(o => true);
            var action = Substitute.For<Action<object>>();
            var sut = new DelegateCommand(action, pred);
            sut.Execute(obj);

            action.Received().Invoke(obj);
        }

        [Test]
        public void Not_Execute_Action_If_Predicate_Is_False()
        {
            var obj = new object();
            var pred = Substitute.For<Predicate<object>>();
            pred.Invoke(obj).ReturnsForAnyArgs(o => false);
            var action = Substitute.For<Action<object>>();

            var sut = new DelegateCommand(action, pred);
            sut.Execute(obj);

            action.DidNotReceiveWithAnyArgs().Invoke(obj);
        }

        [Test]
        public void Execute_Action()
        {
            var obj = new object();
            var action = Substitute.For<Action<object>>();
            var sut = new DelegateCommand(action);
            sut.Execute(obj);

            action.Received().Invoke(obj);
        }

        [Test]
        public void Pass_context_to_CanExecute()
        {
            var person = new Person { Name = "Greta", IsMale = false };
            var sut = new DelegateCommand<Person>(p => { }, p => p.IsMale);

            sut.CanExecute(person).Should().BeFalse();

            person.Name = "dude";
            person.IsMale = true;

            sut.CanExecute(person).Should().BeTrue();
        }

        private class Person
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Name { get; set; }
            public bool IsMale { get; set; }
        }

        [Test]
        public void Cast_safely_to_context_in_CanExecute_and_Execute()
        {
            var sut = new StringCommand();
            sut.CanExecute("hello").Should().BeTrue();
            sut.CanExecute(string.Empty).Should().BeFalse();

            sut.CanExecute(null).Should().BeFalse();
            sut.CanExecute(42).Should().BeFalse();
            sut.CanExecute(new Uri("http://some/server")).Should().BeFalse();
            sut.CanExecute(DateTime.UtcNow).Should().BeFalse();

            sut.Execute("hello");
            sut.Execute(null);
        }

        private class StringCommand : DelegateCommand<string>
        {
            public StringCommand() { DelegateTo(DoFoo, CanFoo); }
            private static bool CanFoo(string str) { return !string.IsNullOrWhiteSpace(str); }
            private static void DoFoo(string sr) { Trace.TraceInformation("From Foo: {0}", sr); }
        }

    }
}