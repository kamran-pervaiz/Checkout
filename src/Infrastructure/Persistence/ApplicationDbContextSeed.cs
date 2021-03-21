using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enum;

namespace Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedSampleDataAsync(ApplicationDbContext context)
        {
            // Seed, if necessary
            if (context != null && !context.Customers.Any())
            {
                context.Customers.Add(
                    new Customer
                    {
                        Id = new Guid("bc97198e-7ff2-45d1-96b8-408781ffb878"),
                        CardNumber = "4000 0000 0000 0259",
                        BankAmount = 10,
                        Currency = "gbp",
                        Cvv = 123,
                        ExpiryMonth = DateTime.Now.Month.ToString(),
                        ExpiryYear = DateTime.Now.Year.ToString(),
                        Status = Status.Refunded,
                        TransactionHistories = new List<TransactionHistory>
                        {
                            new TransactionHistory
                            {
                                Amount = 10,
                                Type = TransactionType.Authorize
                            },
                            new TransactionHistory
                            {
                                Amount = 10,
                                Type = TransactionType.Refund
                            }
                        }
                    });

                context.Customers.Add(new Customer
                {
                    Id = new Guid("e96e72cf-2cc3-4b8c-9dec-b056979dccaa"),
                    CardNumber = "4000 0000 0000 3238",
                    BankAmount = 10,
                    Currency = "gbp",
                    Cvv = 123,
                    ExpiryMonth = DateTime.Now.Month.ToString(),
                    ExpiryYear = DateTime.Now.Year.ToString(),
                    Status = Status.Void,
                    TransactionHistories = new List<TransactionHistory>
                    {
                        new TransactionHistory
                        {
                            Amount = 10,
                            Type = TransactionType.Authorize
                        }
                    }
                });

                await context.SaveChangesAsync();
            }
        }
    }
}