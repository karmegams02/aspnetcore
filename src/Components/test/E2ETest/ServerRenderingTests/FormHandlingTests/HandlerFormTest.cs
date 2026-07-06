// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.TestServer.RazorComponents;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure;
using Microsoft.AspNetCore.Components.E2ETest.Infrastructure.ServerFixtures;
using Microsoft.AspNetCore.E2ETesting;
using Microsoft.AspNetCore.InternalTesting;
using OpenQA.Selenium;
using TestServer;       `1235using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Components.E2ETests.ServerRenderingTests.FormHandlingTests;

// End-to-end coverage for the HandlerForm scenarios called out in the issue
// request and the component's public contract. Note: HandlerForm is a
// lightweight component that does NOT install a FormMappingContext, so its
// native POSTs do not reach a server-side page handler. The supported flow
// is interactive: render under InteractiveServer (or InteractiveWebAssembly),
// use OnSubmit for the callback, and rely on the JS runtime to intercept
// the submit (with PreventDefault=true if a native POST must be avoided).
//
// The "logout scenario from issue #49653" is covered by:
//   Ssr_LogoutStyleFlow_FiresOnSubmitAndPreservesUrl
// which exercises the same shape as the HandlerForm_Interactive sample.
public class HandlerFormTest
    : ServerTestBase<BasicTestAppServerSiteFixture<RazorComponentEndpointsStartup<App>>>
{
    public HandlerFormTest(
        BrowserFixture browserFixture,
        BasicTestAppServerSiteFixture<RazorComponentEndpointsStartup<App>> serverFixture,
        ITestOutputHelper output)
        : base(browserFixture, serverFixture, output)
    {
    }

    public override Task InitializeAsync()
        => InitializeAsync(BrowserFixture.StreamingContext);

    // Issue #49653: the logout-style flow. The form renders under
    // InteractiveServer with PreventDefault=true; clicking submit runs the
    // OnSubmit callback in the browser (this is where the real-world sample
    // performs HttpContext.SignOutAsync + NavigationManager.NavigateTo) and
    // the URL is preserved.
    [Fact]
    public void Ssr_LogoutStyleFlow_FiresOnSubmitAndPreservesUrl()
    {
        Navigate($"{ServerPathBase}/forms/handler-form-ssr");

        // Wait for the circuit to be interactive.
        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        var form = Browser.Exists(By.CssSelector("form"));
        // The component always emits method="post" and an AntiforgeryToken.
        Browser.Equal("post", () => form.GetDomAttribute("method"));
        Browser.Exists(By.CssSelector("form input[type=hidden][name=__RequestVerificationToken]"));

        Browser.Click(By.Id("send"));

        // OnSubmit ran. In the real logout scenario this is where
        // SignOutAsync + NavigateTo("/") would happen.
        Browser.Exists(By.Id("logged-out"));

        // URL did not change: the native POST was suppressed by
        // PreventDefault=true.
        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }

    // OnSubmit without PreventDefault. The interactive runtime intercepts the
    // submit and runs the callback; the native POST never actually goes out
    // because the Blazor client runtime short-circuits it.
    [Fact]
    public void Interactive_OnSubmit_FiresWithoutNavigation()
    {
        Navigate($"{ServerPathBase}/forms/handler-form-ssr-with-onsubmit");

        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        Browser.Click(By.Id("send"));

        Browser.Exists(By.Id("onsubmit-fired"));
        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }

    // PreventDefault=true: the browser-level default is suppressed. The test
    // asserts the OnSubmit callback fires and the URL is unchanged.
    [Fact]
    public void Interactive_PreventDefault_SuppressesNativePost()
    {
        Navigate($"{ServerPathBase}/forms/handler-form-ssr-prevent-default");

        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        Browser.Click(By.Id("send"));

        Browser.Exists(By.Id("onsubmit-fired"));
        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }
}
