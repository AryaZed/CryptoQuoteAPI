using CryptoQuoteAPI.Endpoints;
using CryptoQuoteAPI.Models.CoinMarketCap;
using FluentValidation;

namespace CryptoQuoteAPI.Services.CoinMarketCap.Validations
{
    public class GetCryptoQuoteRequestValidator : AbstractValidator<GetCryptoQuoteRequest>
    {
        public GetCryptoQuoteRequestValidator()
        {
            RuleFor(x => x.CryptoCode)
                .NotEmpty().WithMessage("CryptoCode is required.")
                .Matches("^[A-Z]{3,5}$").WithMessage("CryptoCode must be 3 to 5 uppercase letters.");

            RuleForEach(x => x.Currencies)
                .Matches("^[A-Z]{3}$").WithMessage("Currency codes must be 3 uppercase letters.");
        }
    }

    public class CryptoCodeUpdateRequestValidator : AbstractValidator<CryptoCodeUpdateRequest>
    {
        public CryptoCodeUpdateRequestValidator()
        {
            RuleFor(x => x.CryptoCode)
                .NotEmpty().WithMessage("CryptoCode is required.")
                .Matches("^[A-Z]{3,5}$").WithMessage("CryptoCode must be 3 to 5 uppercase letters.");
        }
    }
}
