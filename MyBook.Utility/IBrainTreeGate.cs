using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Utility
{
    public interface IBrainTreeGate
    {
        IBraintreeGateway CreateGateway();
        IBraintreeGateway GetGateway();
    }
}
