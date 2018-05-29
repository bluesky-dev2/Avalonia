﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Core;
using Avalonia.UnitTests;
using Xunit;

namespace Avalonia.Base.UnitTests.Data.Core
{
    public class ExpressionObserverTests_ExpressionTree
    {
        [Fact]
        public async Task IdentityExpression_Creates_IdentityObserver()
        {
            var target = new object();

            var observer = ExpressionObserver.CreateFromExpression(target, o => o);

            Assert.Equal(target, await observer.Take(1));
        }

        [Fact]
        public async Task Property_Access_Expression_Observes_Property()
        {
            var target = new Class1();

            var observer = ExpressionObserver.CreateFromExpression(target, o => o.Foo);

            Assert.Null(await observer.Take(1));

            using (observer.Subscribe(_ => {}))
            {
                target.Foo = "Test"; 
            }

            Assert.Equal("Test", await observer.Take(1));

            GC.KeepAlive(target);
        }

        [Fact]
        public void Property_Acccess_Expression_Can_Set_Property()
        {
            var data = new Class1();
            var target = ExpressionObserver.CreateFromExpression(data, o => o.Foo);

            using (target.Subscribe(_ => { }))
            {
                Assert.True(target.SetValue("baz"));
            }

            GC.KeepAlive(data);
        }

        [Fact]
        public async Task Indexer_Accessor_Can_Read_Value()
        {
            var data = new[] { 1, 2, 3, 4 };

            var target = ExpressionObserver.CreateFromExpression(data, o => o[0]);

            Assert.Equal(data[0], await target.Take(1));
        }

        [Fact]
        public async Task Indexer_Accessor_Can_Read_Complex_Index()
        {
            var data = new Dictionary<object, object>();

            var key = new object();

            data.Add(key, new object());

            var target = ExpressionObserver.CreateFromExpression(data, o => o[key]);

            Assert.Equal(data[key], await target.Take(1));
        }

        [Fact]
        public void Indexer_Can_Set_Value()
        {
            var data = new[] { 1, 2, 3, 4 };

            var target = ExpressionObserver.CreateFromExpression(data, o => o[0]);

            using (target.Subscribe(_ => { }))
            {
                Assert.True(target.SetValue(2));
            }

            GC.KeepAlive(data);
        }

        private class Class1 : NotifyingBase
        {
            private string _foo;

            public string Foo
            {
                get { return _foo; }
                set
                {
                    _foo = value;
                    RaisePropertyChanged(nameof(Foo));
                }
            }
        }
    }
}
