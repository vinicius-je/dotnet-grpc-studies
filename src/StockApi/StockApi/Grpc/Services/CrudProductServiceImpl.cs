using Grpc.Core;
using StockApi.Domain.Entities;
using StockApi.Grpc.Protos;

namespace StockApi.Grpc.Services
{
    public class CrudProductServiceImpl : CrudProductService.CrudProductServiceBase
    {
        private readonly List<ProductResponse> productResponses = new();

        public override Task<ProductResponse> CreateProduct(ProductRequest request, ServerCallContext context)
        {
            var product = new ProductResponse
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Quantity = request.Quantity,
                Price = request.Price,
                Category = request.Category.ToString(),
            };
            productResponses.Add(product);
            return Task.FromResult(product);
        }

    }
}
