// <auto-generated/>
#pragma warning disable 1591
namespace Test
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    public class TestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        #pragma warning disable 1998
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.RenderTree.RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenComponent<Test.MyComponent>(0);
            builder.AddAttribute(1, "Value", Microsoft.AspNetCore.Components.RuntimeHelpers.TypeCheck<System.Int32>(Microsoft.AspNetCore.Components.BindMethods.GetValue(ParentValue)));
            builder.AddAttribute(2, "ValueChanged", new System.Action<System.Int32>(__value => ParentValue = __value));
            builder.CloseComponent();
        }
        #pragma warning restore 1998
#line 3 "x:\dir\subdir\Test\TestComponent.cshtml"
            
    public int ParentValue { get; set; } = 42;

#line default
#line hidden
    }
}
#pragma warning restore 1591
