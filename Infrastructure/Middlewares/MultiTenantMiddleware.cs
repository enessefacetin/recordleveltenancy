using System;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Middlewares
{
    public class MultiTenantMiddleware
    {
        private static readonly string TenantHeaderName = "X-TenantName";

        private readonly RequestDelegate _next;
        
        public MultiTenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the Tenant Name from the Header:
            if (context.Request.Headers.ContainsKey(TenantHeaderName))
            {
                string tenantName = context.Request.Headers[TenantHeaderName];
                var dbContext = context.RequestServices.GetService<AppDbContext>();
                // It's probably OK for the Tenant Name to be empty, which may or may not be valid for your scenario.
                if (!string.IsNullOrWhiteSpace(tenantName))
                {
                    var tenantNameString = tenantName.ToString();

                    var tenant = await dbContext.Tenants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Name == tenantNameString, context.RequestAborted);

                    if (tenant == null)
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid Tenant Name", context.RequestAborted);

                        return;
                    }

                    // We know the Tenant, so set it in the TenantExecutionContext:
                    dbContext.TenantId = tenant.TenantId;
                }
            }else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Tenant Name Not Provided", context.RequestAborted);

                return;
            }
            await _next(context);
        }
        
    }
    
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MultiTenantMiddleware>();
        }
    }
}