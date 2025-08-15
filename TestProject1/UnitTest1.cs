using System.Net;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using FoodyApiExamPrep.Models;
using System.Text.Json;

namespace FoodyApiExamPrep
{
    [TestFixture]
    public class UnitTest1
    {
        private RestClient client;
        private static string lastCreatedFoodId;

        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIzZTQ2MTc2Zi02NTk3LTQxNDEtODk0Zi1hNjBiNGRkOWIwNjciLCJpYXQiOiIwOC8xNS8yMDI1IDEyOjAxOjMzIiwiVXNlcklkIjoiZTU4ZGEzYjUtNmMzZC00MDM5LWM4MTctMDhkZGQ4ZDg5MTBlIiwiRW1haWwiOiJpdmFuNDQ0QHlvcG1haWwuY29tIiwiVXNlck5hbWUiOiJpdmFuNDQ0IiwiZXhwIjoxNzU1MjgwODkzLCJpc3MiOiJGb29keV9BcHBfU29mdFVuaSIsImF1ZCI6IkZvb2R5X1dlYkFQSV9Tb2Z0VW5pIn0.Y61fbzdST9BS4Z26HTFqB4qpntaqXH11plylMCmev5A";

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(StaticToken),
            };
            this.client = new RestClient(options);
        }

        [Test, Order(1)]
        public void CreateFood_WithRequiredFields_ShouldReturnCreated()
        {
            var foodRequest = new FoodDTO
            {
                Name = "Test Food",
                Description = "This is a test food description.",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodRequest);

            var response = this.client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse.FoodId, Is.Not.Null.And.Not.Empty);

            lastCreatedFoodId = createResponse.FoodId;
        }

        [Test, Order(2)]
        public void EditFoodTitle_ShouldReturnSuccess()
        {
            var editRequest = new[]
            {
                new { path = "/name", op = "replace", value = "Updated Test Food" }
            };

            var request = new RestRequest($"/api/Food/Edit/{lastCreatedFoodId}", Method.Patch);
            request.AddJsonBody(editRequest);

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllFoods_ShouldReturnNonEmptyList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("name"));
        }

        [Test, Order(4)]
        public void DeleteFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedFoodId}", Method.Delete);
            var response = this.client.Execute(request);

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodRequest = new FoodDTO { Name = "", Description = "" };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodRequest);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            string fakeFoodId = "12345";
            var editRequest = new[]
            {
                new { path = "/name", op = "replace", value = "Fake Food Name" }
            };

            var request = new RestRequest($"/api/Food/Edit/{fakeFoodId}", Method.Patch);
            request.AddJsonBody(editRequest);

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editResponse.Msg, Is.EqualTo("No food revues..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeFoodId = "12345";
            var request = new RestRequest($"/api/Food/Delete/{fakeFoodId}", Method.Delete);

            var response = this.client.Execute(request);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deleteResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
