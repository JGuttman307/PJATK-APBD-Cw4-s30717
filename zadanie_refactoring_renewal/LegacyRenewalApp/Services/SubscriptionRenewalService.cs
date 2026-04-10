using System;
using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly SubscriptionPlanRepository _planRepository;
        private readonly DiscountCalculator _discountCalculator;
        private readonly IEnumerable<IPaymentFeeModel> _paymentStrategies;
        private readonly ISupportFeeCalculator _supportFeeCalculator;
        private readonly ITaxRateCalculator _taxCalculator;
        private readonly IBillingGateway _billingGateway;

        public SubscriptionRenewalService(
            CustomerRepository customerRepository,
            SubscriptionPlanRepository planRepository,
            DiscountCalculator discountCalculator,
            IEnumerable<IPaymentFeeModel> paymentStrategies,
            ISupportFeeCalculator supportFeeCalculator,
            ITaxRateCalculator taxCalculator,
            IBillingGateway billingGateway)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _discountCalculator = discountCalculator;
            _paymentStrategies = paymentStrategies;
            _supportFeeCalculator = supportFeeCalculator;
            _taxCalculator = taxCalculator;
            _billingGateway = billingGateway;
        }
        public SubscriptionRenewalService() //żeby działało wszelakie Payment metody
            : this(
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                new DiscountCalculator(new List<IDiscountModel>()),
                new List<IPaymentFeeModel>
                {
                    new CardPayment(),
                    new BankTransferPayment(),
                    new PaypalPayment(),
                    new InvoicePayment()
                },
                new SupportFeeCalculator(),
                new TaxRateCalculator(),
                new BillingGateway()
            )
        {
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            Validate(customerId, planCode, seatCount, paymentMethod);

            var normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            var normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;

            var (discountAmount, notes) =
                _discountCalculator.Calculate(baseAmount, customer, plan, seatCount, useLoyaltyPoints);

            decimal subtotalAfterDiscount = baseAmount - discountAmount;

            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            decimal supportFee = _supportFeeCalculator.Calculate(normalizedPlanCode, includePremiumSupport, ref notes);

            decimal paymentFee = CalculatePaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee, ref notes);

            decimal taxRate = _taxCalculator.GetRate(customer.Country);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;

            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2),
                DiscountAmount = Math.Round(discountAmount, 2),
                SupportFee = Math.Round(supportFee, 2),
                PaymentFee = Math.Round(paymentFee, 2),
                TaxAmount = Math.Round(taxAmount, 2),
                FinalAmount = Math.Round(finalAmount, 2),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                _billingGateway.SendEmail(
                    customer.Email,
                    "Subscription renewal invoice",
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.");
            }

            return invoice;
        }

        private decimal CalculatePaymentFee(string method, decimal amount, ref string notes)
        {
            foreach (var strategy in _paymentStrategies)
            {
                if (strategy.Supports(method))
                {
                    notes += strategy.Note + " ";
                    return strategy.Calculate(amount);
                }
            }

            throw new ArgumentException("Unsupported payment method");
        }

        private void Validate(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0)
                throw new ArgumentException("Customer id must be positive");

            if (string.IsNullOrWhiteSpace(planCode))
                throw new ArgumentException("Plan code is required");

            if (seatCount <= 0)
                throw new ArgumentException("Seat count must be positive");

            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("Payment method is required");
        }
    }
}