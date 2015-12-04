using NServiceBus.Features;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RoutingSlips : Feature
    {
        public RoutingSlips()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register("RoutingSlipBehavior", typeof(Router), "Unpacks routing slip and forwards message to next destination");
        }
    }
}