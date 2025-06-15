using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;
using ShopOnline.Shared.Dtos;

namespace OrderService.Controllers;

[ApiController]
[Route("orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Order>> Create(
        [FromBody] CreateOrderDto createOrderDto)
    {
        var userIdHeader = Request.Headers["user_id"];
        var userId = createOrderDto.UserId ?? Guid.Parse(userIdHeader!);

        var order = await orderService.CreateAsync(userId, createOrderDto.Amount);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<IEnumerable<Order>> List(
        [FromHeader(Name = "user_id")] Guid userId) =>
        await orderService.ListAsync(userId);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> Get(Guid id) =>
        await orderService.GetAsync(id) is { } order
            ? Ok(order)
            : NotFound();
}