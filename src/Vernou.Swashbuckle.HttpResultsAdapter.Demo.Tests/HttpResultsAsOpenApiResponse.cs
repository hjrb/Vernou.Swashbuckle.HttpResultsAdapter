namespace Vernou.Swashbuckle.HttpResultsAdapter.Demo.Tests;

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Shouldly;

public class HttpResultsAsOpenApiResponse
{
    private static readonly Lazy<OpenApiDocument> _document =
        new(() =>
        {
            var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();
            var httpResponse = client.GetAsync("/swagger/v1/swagger.json").Result;
            httpResponse.EnsureSuccessStatusCode();
            var document = new OpenApiStreamReader().Read(httpResponse.Content.ReadAsStream(), out var diagnostic);
            diagnostic.Errors.ShouldBeEmpty();
            diagnostic.Warnings.ShouldBeEmpty();
            return document;
        });
    private static OpenApiDocument Document => _document.Value;

    private static OpenApiResponses GetResponses(string path)
    {
        var responses = Document.Paths["/" + path].Operations[OperationType.Get].Responses;
        responses.ShouldNotBeNull();
        return responses;
    }

    [Theory]
    [InlineData("ok", "200")]
    [InlineData("created", "201")]
    [InlineData("accepted", "202")]
    [InlineData("acceptedatroute", "202")]
    [InlineData("nocontent", "204")]
    [InlineData("badrequest", "400")]
    [InlineData("validationproblem", "400", Skip = "Fail")]
    [InlineData("unauthorized", "401")]
    [InlineData("notfound", "404")]
    [InlineData("conflict", "409")]
    [InlineData("unprocessableentity", "422", Skip = "Fail")]

    [InlineData("ok-async", "200")]
    [InlineData("created-async", "201")]
    [InlineData("accepted-async", "202")]
    [InlineData("acceptedatroute-async", "202")]
    [InlineData("nocontent-async", "204")]
    [InlineData("badrequest-async", "400")]
    [InlineData("validationproblem-async", "400", Skip = "Fail")]
    [InlineData("unauthorized-async", "401")]
    [InlineData("notfound-async", "404")]
    [InlineData("conflict-async", "409")]
    [InlineData("unprocessableentity-async", "422", Skip = "Fail")]
    public void TypedResult(string path, string expected)
    {
        // Arrange

        var responses = GetResponses(path);

        //Assert

        var response = responses.ShouldHaveSingleItem();
        response.Key.ShouldBe(expected);
        response.Value.Content.ShouldBeEmpty();
    }


    [Theory]
    [InlineData("ok-foo", "200")]
    [InlineData("created-foo", "201")]
    [InlineData("createdatroute-foo", "201")]
    [InlineData("accepted-foo", "202")]
    [InlineData("acceptedatroute-foo", "202")]
    [InlineData("badrequest-foo", "400")]
    [InlineData("notfound-foo", "404")]
    [InlineData("conflict-foo", "409")]
    [InlineData("unprocessableentity-foo", "422")]

    [InlineData("ok-foo-async", "200")]
    [InlineData("created-foo-async", "201")]
    [InlineData("createdatroute-foo-async", "201")]
    [InlineData("accepted-foo-async", "202")]
    [InlineData("acceptedatroute-foo-async", "202")]
    [InlineData("badrequest-foo-async", "400")]
    [InlineData("notfound-foo-async", "404")]
    [InlineData("conflict-foo-async", "409")]
    [InlineData("unprocessableentity-foo-async", "422")]
    public void TypedResultOf(string path, string expected)
    {
        // Arrange

        var responses = GetResponses(path);

        //Assert

        var response = responses.ShouldHaveSingleItem();
        response.Key.ShouldBe(expected);
        response.Value.Content.Count.ShouldBeEquivalentTo(HttpResultsOperationFilter.DefaultMediaTypes.Count);
        foreach (var contentItem in response.Value.Content) {
            HttpResultsOperationFilter.DefaultMediaTypes.Contains(contentItem.Key).ShouldBeTrue();
            contentItem.Value.Schema.Type.ShouldBe("object");
            contentItem.Value.Schema.Reference.Id.ShouldBe("Foo");
        }
    }

    [Theory]
    [InlineData("ok_nocontent", new[] {"200", "204"})]
    [InlineData("ok_nocontent_notfound", new[] { "200", "204", "404" })]

    [InlineData("ok_nocontent-async", new[] { "200", "204" })]
    [InlineData("ok_nocontent_notfound-async", new[] { "200", "204", "404" })]
    public void ManyTypedResult(string path, string[] expected)
    {
        // Arrange

        var responses = GetResponses(path);

        //Assert

        responses.Count.ShouldBe(expected.Length);
        foreach(var e in expected)
        {
            var response = responses[e];
            response.Content.ShouldBeEmpty();
        }
    }
}
