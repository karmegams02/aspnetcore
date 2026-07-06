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
using TestServer;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Components.E2ETests.ServerRenderingTests.FormHandlingTests;

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

    [Fact]
    public void Ssr_LogoutStyleFlow_FiresOnSubmitAndPreservesUrl()
    {
        Navigate($"{ServerPathBase}/forms/handler-form-ssr");

        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        var form = Browser.Exists(By.CssSelector("form"));
        Browser.Equal("post", () => form.GetDomAttribute("method"));
        Browser.Exists(By.CssSelector("form input[type=hidden][name=__RequestVerificationToken]"));

        Browser.Click(By.Id("send"));

        Browser.Exists(By.Id("logged-out"));

        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }

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
