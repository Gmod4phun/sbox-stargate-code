using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Components.Stargate
{
	public class StargateMovie : StargateMilkyWay
	{
		public StargateMovie()
		{
            SoundDict = new()
            {
                { "gate_open", "stargate.movie.open" },
                { "gate_close", "stargate.movie.close" },
                { "chevron_open", "stargate.movie.chevron_open" },
                { "chevron_close", "stargate.movie.chevron_close" },
                { "dial_fail", "stargate.milkyway.dial_fail_noclose" },
                { "dial_fail_noclose", "stargate.milkyway.dial_fail_noclose" },
                { "dial_begin_9chev", "stargate.universe.dial_begin_9chev" },
                { "dial_fail_9chev", "stargate.universe.dial_fail_9chev" }
            };

            GateGlyphType = GlyphType.MILKYWAY;

            MovieDialingType = true;
            ChevronLightup = false;
		}
	}
}
