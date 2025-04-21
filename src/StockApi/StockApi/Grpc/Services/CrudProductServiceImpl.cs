using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StockApi.Domain.Entities;
using StockApi.Grpc.Protos;
using StockApi.Infrastructure;
using System.Globalization;

namespace StockApi.Grpc.Services
{
    public class CrudProductServiceImpl : CrudProductService.CrudProductServiceBase
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Product> _products;

        public CrudProductServiceImpl(AppDbContext context)
        {
            _context = context;
            _products = context.Set<Product>();
        }

        public override async Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
        {
            try
            {
                ExecuteInputValidation(request.Name, request.Description, request.Price, request.Quantity, request.Category.ToString());
                
                Product product = ConvertToProduct(request);

                await _products.AddAsync(product);
                await _context.SaveChangesAsync();

                return ConvertToProductResponse(product);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error: {ex.Message}"));
            }

            static Product ConvertToProduct(CreateProductRequest request)
            {
                return new Product(
                    request.Name,
                    request.Description,
                    decimal.Parse(request.Price, CultureInfo.InvariantCulture),
                    (Domain.Enums.CategoryEnum)request.Category,
                    request.Quantity);
            }
        }

        public override async Task<ProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out Guid productId))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "The product Id is invalid."));

                ExecuteInputValidation(request.Name, request.Description, request.Price, request.Quantity, request.Category.ToString());
                Product product = await GetProductFromDbById(productId);

                product.Update(
                    request.Name,
                    request.Description,
                    decimal.Parse(request.Price, CultureInfo.InvariantCulture),
                    (Domain.Enums.CategoryEnum)request.Category,
                    request.Quantity);

                _products.Update(product);
                await _context.SaveChangesAsync();

                return ConvertToProductResponse(product);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error: {ex.Message}"));
            }
        }

        public override async Task<ProductResponse> GetProductById(ProductIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out Guid productId))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "The product Id is invalid."));
       
                Product product = await GetProductFromDbById(productId);

                return ConvertToProductResponse(product);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error: {ex.Message}"));
            }
        }

        public override async Task<ProductListResponse> GetProducts(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {

            ProductListResponse productList = new();

            var products = await _products.ToListAsync();

            foreach (var product in products)
            {
                productList.Products.Add(ConvertToProductResponse(product));
            }

            return productList;
        }

        public override async Task<ProductDeletedResponse> DeleteProduct(ProductIdRequest request, ServerCallContext context)
        {
            try
            {
                if (!Guid.TryParse(request.Id, out Guid productId))
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "The product Id is invalid."));

                Product product = await GetProductFromDbById(productId);

                _products.Remove(product);
                await _context.SaveChangesAsync();

                return new ProductDeletedResponse { Message = $"Product {product.Name} excluded with success." };
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error: {ex.Message}"));
            }
        }

        #region Auxiliary Methods
        private void ExecuteInputValidation(string name, string description, string price, int quantity, string category)
        {
            if (string.IsNullOrEmpty(name))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "The product name cannot be empty."));

            if (string.IsNullOrEmpty(description))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "The product description cannot be empty."));

            if (string.IsNullOrEmpty(price))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "The product price cannot be empty."));

            if (quantity < 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Quantity cannot be negative."));

            if (!Enum.TryParse(typeof(CategoryEnum), category, out object? value))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "The product category is not reconized."));
        }

        private ProductResponse ConvertToProductResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Description = product.Description,
                Price = product.Price.ToString(),
                Quantity = product.Quantity,
                Category = Enum.GetName(typeof(CategoryEnum), product.Category),
            };
        }

        private async Task<Product> GetProductFromDbById(Guid productId)
        {
            var product = await _products.Where(x => x.Id == productId).FirstOrDefaultAsync();

            if (product == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {productId} was not found."));

            return product;
        }
        #endregion
    }
}
