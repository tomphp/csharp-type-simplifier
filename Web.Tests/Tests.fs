module Tests

open Microsoft.Playwright
open Microsoft.Playwright.Xunit

type Tests() =
    inherit PageTest()

    member private this.VisitPage() =
        this.Page.GotoAsync("http://localhost:5058/") |> Async.AwaitTask |> Async.Ignore

    member private this.InputMessage (msg : string) =
        this.Page.GetByLabel("Message").FillAsync(msg)

    member private this.ClickShowNamespaces() =
        this.Page.GetByLabel("Show namespaces").ClickAsync()

    member private this.ClickAddLineBreaks() =
        this.Page.GetByLabel("Add line breaks").ClickAsync()

    member private this.ExpectOutput() =
        this.Expect(this.Page.GetByLabel("Result"))

    member private this.OutputText() =
        this.Page.GetByLabel("Result").InnerHTMLAsync()

    [<Xunit.Fact>]
    member this.``inputting a boring message`` () =
        task {
            do! this.VisitPage()

            do! this.InputMessage("boring message")

            do! this.ExpectOutput().ToContainTextAsync("boring message")
        }

    [<Xunit.Fact>]
    member this.``inputting a message with types`` () =
        task {
            do! this.VisitPage()

            do! this.InputMessage("The following is a type MyCompany.MyProject.MyType")

            do! this.ExpectOutput().ToContainTextAsync("The following is a type MyCompany.MyProject.MyType")
        }

    [<Xunit.Fact>]
    member this.``hiding namespaces`` () =
        task {
            do! this.VisitPage()

            do! this.InputMessage("The following is a type MyCompany.MyProject.MyType")
            do! this.ClickShowNamespaces()

            do! this.ExpectOutput().ToContainTextAsync("The following is a type MyType")
        }

    [<Xunit.Fact>]
    member this.``add line breaks`` () =
        task {
            do! this.VisitPage()

            do! this.ClickAddLineBreaks()
            do! this.InputMessage("First type is MyCompany.MyProject.MyTypeOne and second type is MyCompany.MyProject.MyTypeTwo")

            let! output = this.OutputText()
            Xunit.Assert.Matches(
                "First type is.*<br>.*"
                 + "MyCompany.*\..*MyProject.*\..*MyTypeOne.*.*<br>.*"
                 + "and second type is.*<br>.*"
                 + "MyCompany.*\..*MyProject.*\..*MyTypeTwo",
                 output
            )
        }
