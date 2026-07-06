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

// End-to-end coverage for the interactive scenarios described in the issue
// request: with the form rendered under an interactive render mode the JS
// branch should run, the OnSubmit callback should fire, and the page must
// not perform a navigation. Both InteractiveServer and InteractiveWebAssembly
// share the same expected behavior; the InlineData mirrors the pattern used
// by AntiforgeryTests.CanUseAntiforgeryAfterInitialRender.
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

        // Wait for the circuit (or WASM runtime) to actually be interactive.
        Browser.Exists(By.Id("ready"));
        Browser.Exists(By.Id("interactive"));

        var urlBeforeClick = Browser.FindElement(By.Id("url")).Text;

        Browser.Click(By.Id("send"));

        // OnSubmit fired - this is the JS branch developers typically land on.
        Browser.Exists(By.Id("onsubmit-fired"));

        // PreventDefault is set, so the page must not navigate. The URL element
        // is re-rendered by the interactive component on every change, so its
        // value staying equal to the pre-click value is a strong signal that
        // the native submission was suppressed.
        var urlAfterClick = Browser.FindElement(By.Id("url")).Text;
        Browser.Equal(urlBeforeClick, () => urlAfterClick);
    }
}
