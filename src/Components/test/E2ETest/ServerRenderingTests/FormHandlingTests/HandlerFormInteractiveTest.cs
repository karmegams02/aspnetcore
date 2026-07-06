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

public class HandlerFormInteractiveTest
    : ServerTestBase<BasicTestAppServerSiteFixture<RazorComponentEndpointsStartup<App>>>
{
    public HandlerFormInteractiveTest(
        BrowserFixture browserFixture,
        BasicTestAppServerSiteFixture<RazorComponentEndpointsStartup<App>> serverFixture,
        ITestOutputHelper output)
        : base(browserFixture, serverFixture, output)
    {
    }

    public override Task InitializeAsync()
        => InitializeAsync(BrowserFixture.StreamingContext);

    [Theory]
    [InlineData("server")]
    [InlineData("webassembly")]
    public void Interactive_FormSubmitsAndOnSubmitFires(string target)
    {
        Navigate($"{ServerPathBase}/forms/handler-form-interactive-{target}");

        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        Browser.Click(By.Id("send"));

        Browser.Exists(By.Id("onsubmit-fired"));

        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }
}
