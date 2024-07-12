using Inventory.EventSourcing.Api.Projectors;
using Inventory.EventSourcing.Api.Services;
using Inventory.EventSourcing.Api.Services.Abstractions;
using Marten;
using Marten.Events.Projections;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Weasel.Core;


var builder = WebApplication.CreateBuilder(args);

var serializer = new Marten.Services.JsonNetSerializer();
serializer.EnumStorage = EnumStorage.AsString;
serializer.Customize(_ =>
{
    _.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
    _.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
});

builder.Services.AddMarten(o =>
{
    o.Connection(builder.Configuration.GetConnectionString("Default"));
    o.Serializer(serializer);
    //o.Projections.Add<WarehouseProjection>(ProjectionLifecycle.Inline);
    o.Projections.Add<InventoryProjector>(ProjectionLifecycle.Inline);
    o.Projections.Add<ProductTransactionProjection>(ProjectionLifecycle.Inline);
    o.Projections.Add<CartProjector>(ProjectionLifecycle.Inline);
    o.Projections.Add<OrderSummaryProjector>(ProjectionLifecycle.Inline);
});

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IProductTransactionService, ProductTransactionService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-commerce API", Version = "v1" });

    c.TagActionsBy(api => new List<string> { api.RelativePath.Split('/')[0] });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-commerce API V1");
    });
}
#region Cart Endpoints

// Endpoint to add items to the shopping cart by product ID, name, and price
app.MapPost("/cart/add/{userId}/{productId}/{productName}/{price}", async (ICartService cartService, Guid userId, Guid productId, string productName, decimal price) =>
{
    return await cartService.AddItemToCartAsync(userId, productId, productName, price);
});

// Endpoint to remove items from the shopping cart by product ID
app.MapDelete("/cart/remove/{userId}/{productId}", async (ICartService cartService, Guid userId, Guid productId) =>
{
    return await cartService.RemoveItemFromCartAsync(userId, productId);
});

// Endpoint to update the quantity of an item in the cart by product ID and new quantity
app.MapPut("/cart/update-quantity/{userId}/{productId}/{quantity}", async (ICartService cartService, Guid userId, Guid productId, int quantity) =>
{
    return await cartService.UpdateCartItemQuantityAsync(userId, productId, quantity);
});

// Endpoint to update the price of an item in the cart by product ID and new price
app.MapPut("/cart/update-price/{userId}/{productId}/{price}", async (ICartService cartService, Guid userId, Guid productId, decimal price) =>
{
    return await cartService.UpdateCartItemPriceAsync(userId, productId, price);
});

// Endpoint to update shipping information with specific address and phone number inputs
app.MapPut("/cart/update-shipping/{userId}/{address}/{phoneNumber}", async (ICartService cartService, Guid userId, string address, string phoneNumber) =>
{
    return await cartService.UpdateShippingInformationAsync(userId, address, phoneNumber);
});

// Endpoint to get all cart items and their total for a given user ID
app.MapGet("/cart/{userId}", async (HttpContext context, ICartService cartService, Guid userId) =>
{
    var result = await cartService.GetCartDetailsAsync(userId);
    await context.Response.WriteAsJsonAsync(result);
});


// Endpoint to get the event stream for a user's cart
app.MapGet("/cart/events/{userId}", async (ICartService cartService, Guid userId) =>
{
    return await cartService.GetCartEventStreamAsync(userId);
});

#endregion
#region Order Endpoints

// Endpoint to create an order
app.MapPost("/order/create/{userId}", async (IOrderService orderService, Guid userId) =>
{
    return await orderService.CreateOrderAsync(userId);
});

// Endpoint to cancel an order
app.MapDelete("/order/cancel/{userId}/{orderId}", async (IOrderService orderService, Guid userId, Guid orderId) =>
{
    return await orderService.CancelOrderAsync(userId, orderId);
});

// Endpoint to get orders for a given user ID
app.MapGet("/order/{userId}", async (IOrderService orderService, Guid userId) =>
{
    return await orderService.GetOrdersAsync(userId);
});

// Endpoint to complete and submit an order
app.MapPost("/order/submit/{userId}", async (IOrderService orderService, Guid userId) =>
{
    return await orderService.SubmitOrderAsync(userId);
});

#endregion
#region Inventory Endpoints

// Endpoint to add a new product to inventory
app.MapPost("/inventory/add/{name}/{warehouseId}", async (IInventoryService inventoryService, string name, Guid warehouseId) =>
{
    return await inventoryService.AddProductToInventoryAsync(name, warehouseId);
});

// Endpoint to retrieve product details including last quantity and transactions
app.MapGet("/inventory/product/{productId}/{warehouseId}", async (IInventoryService inventoryService, Guid productId, Guid warehouseId) =>
{
    return await inventoryService.GetProductDetailsAsync(productId, warehouseId);
});

// Endpoint to remove a product from inventory
app.MapDelete("/inventory/remove/{productId}/{warehouseId}", async (IInventoryService inventoryService, Guid productId, Guid warehouseId) =>
{
    return await inventoryService.RemoveProductFromInventoryAsync(productId, warehouseId);
});

// Endpoint to update product information (name and price)
app.MapPut("/inventory/update/{productId}/{name}/{warehouseId}", async (IInventoryService inventoryService, Guid productId, string name, Guid warehouseId) =>
{
    return await inventoryService.UpdateProductInfoAsync(productId, warehouseId, name);
});

// Endpoint to add a sale transaction for a product from a specific warehouse
app.MapPost("/inventory/sale/add/{warehouseId}/{productId}/{quantity}/{price}", async (IProductTransactionService inventoryService, Guid warehouseId, Guid productId, int quantity, double price) =>
{
    return await inventoryService.SaleProductAsync(warehouseId, productId, quantity, price);
});

// Endpoint to add a purchase transaction for a product to a specific warehouse
app.MapPost("/inventory/purchase/add/{warehouseId}/{productId}/{quantity}/{price}", async (IProductTransactionService inventoryService, Guid warehouseId, Guid productId, int quantity, double price) =>
{
    return await inventoryService.PurchaseProductAsync(warehouseId, productId, quantity, price);
});


app.MapPost("/inventory/transfer/{sourceWarehouseId}/{targetWarehouseId}/{productId}/{quantity}", async (IProductTransactionService inventoryService, Guid sourceWarehouseId, Guid targetWarehouseId, Guid productId, int quantity) =>
{
    return await inventoryService.TransferProductBetweenWarehousesAsync(sourceWarehouseId, targetWarehouseId, productId, quantity);
});

app.MapGet("/warehouses", async (IWarehouseService warehouseService) =>
{
    var warehouses = await warehouseService.GetAllWarehousesAsync();
    return Results.Ok(warehouses);
});
//app.MapGet("/warehouses/{warehouseId}/products", async (Guid warehouseId, IWarehouseService warehouseService) =>
//{
//    var products = await warehouseService.GetWarehouseProductsAsync(warehouseId);
//    return products is not null ? Results.Ok(products) : Results.NotFound();
//});

#endregion

app.Run();

