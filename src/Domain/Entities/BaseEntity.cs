using System;

namespace Domain.Entities
{
    public abstract class BaseEntity
    {
        //Treating as unique transaction/authorize id
        public Guid Id { get; set; }
    }
}