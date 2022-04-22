using Braintree;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Utility
{
    public class BrainTreeGate : IBrainTreeGate
    {
        public BrainTreeSettingsx Options { get; set; }

        public BrainTreeGate(IOptions<BrainTreeSettingsx> options)
        {
            Options = options.Value;
        }

        private IBraintreeGateway BraintreeGateway { get; set; }

        public IBraintreeGateway CreateGateway()
        {
            return new BraintreeGateway(Options.Environment, Options.MerchantId, Options.PublicKey, Options.PrivateKey);

        }

        public IBraintreeGateway GetGateway()
        {
            if(BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }
            return BraintreeGateway;
        }

    }
}
