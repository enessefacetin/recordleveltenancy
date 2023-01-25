using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public string TenantId { get; set; }

        public Tenant Tenant { get; set; }
    }
}