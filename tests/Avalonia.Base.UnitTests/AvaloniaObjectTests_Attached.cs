// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using Xunit;

namespace Avalonia.Base.UnitTests
{
    public class AvaloniaObjectTests_Attached
    {
        [Fact]
        public void AddOwnered_Property_Retains_Default_Value()
        {
            var target = new Class2();

            Assert.Equal("foodefault", target.GetValue(Class2.FooProperty));
        }

        [Fact]
        public void AddOwnered_Property_Retains_Validation()
        {
            var target = new Class2();

            Assert.Throws<IndexOutOfRangeException>(() => target.SetValue(Class2.FooProperty, "throw"));
        }

        private class Class1 : AvaloniaObject
        {
            public static readonly AttachedProperty<string> FooProperty =
                AvaloniaProperty.RegisterAttached<Class1, AvaloniaObject, string>(
                    "Foo",
                    "foodefault",
                    validate: ValidateFoo);

            private static string ValidateFoo(AvaloniaObject arg1, string arg2)
            {
                if (arg2 == "throw")
                {
                    throw new IndexOutOfRangeException();
                }

                return arg2;
            }
        }

        private class Class2 : AvaloniaObject
        {
            public static readonly AttachedProperty<string> FooProperty =
                Class1.FooProperty.AddOwner<Class2>();
        }
    }
}
