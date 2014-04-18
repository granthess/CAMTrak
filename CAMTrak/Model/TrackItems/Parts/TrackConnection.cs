using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMTrak.Model.TrackItems.Parts
{
    /******************************************************************
     * A TrackConnection hooks two trackitems together at a pair of
     * corresponding TrackEndpoints.
     * 
     * It contains a TrackEndpointPosition that applies to both 
     * TrackItems' Endpoints.
     * 
     * It coordinates the TrackEndpointSplinePosition values for
     * each endpoint to ensure that angles are matched correctly
     ******************************************************************/
    public class TrackConnection
    {
        public TrackEndpoint ItemA { get; private set; }
        public TrackEndpoint ItemB { get; private set; }



    }
}
