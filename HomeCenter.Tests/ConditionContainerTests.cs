using HomeCenter.Model.Conditions;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    [TestClass]
    public class ConditionContainerTests : ReactiveTest
    {
        [TestMethod]
        public async Task Validate_WhenEmptyExpression_ShouldAddANDExpressionAndValidate()
        {
            var c1 = Mock.Of<IValidable>();
            var c2 = Mock.Of<IValidable>();
            Mock.Get(c1).Setup(c => c.Validate()).ReturnsAsync(true);
            Mock.Get(c2).Setup(c => c.Validate()).ReturnsAsync(true);
            var container = new ConditionContainer();
            container.Conditions.Add(c1);
            container.Conditions.Add(c2);

            var result = await container.Validate();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Validate_WhenEmptyExpressionAndOneInverted_ShouldAddANDExpressionAndValidate()
        {
            var c1 = Mock.Of<IValidable>();
            var c2 = Mock.Of<IValidable>();
            Mock.Get(c1).Setup(c => c.Validate()).ReturnsAsync(true);
            Mock.Get(c2).Setup(c => c.Validate()).ReturnsAsync(true);
            Mock.Get(c1).Setup(c => c.IsInverted).Returns(true);
            Mock.Get(c2).Setup(c => c.IsInverted).Returns(false);
            var container = new ConditionContainer();
            container.Conditions.Add(c1);
            container.Conditions.Add(c2);

            var result = await container.Validate();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Validate_WithProperExpression_ShouldValidate()
        {
            var c1 = Mock.Of<IValidable>();
            var c2 = Mock.Of<IValidable>();
            Mock.Get(c1).Setup(c => c.Validate()).ReturnsAsync(true);
            Mock.Get(c2).Setup(c => c.Validate()).ReturnsAsync(false);
            var container = new ConditionContainer()
            {
                Expression = "C1 and (not C2)"
            };
            container.Conditions.Add(c1);
            container.Conditions.Add(c2);

            var result = await container.Validate();

            Assert.IsTrue(result);
        }
    }
}