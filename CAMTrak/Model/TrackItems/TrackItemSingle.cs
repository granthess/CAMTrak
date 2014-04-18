using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region Design Info
/************************************************************************************
 *    TrackItemSingle -- Single Track Item                                            
 ************************************************************************************
 * Implements a track section that has only two end points that can be connected   
 *                                                                                  
 * Shape is determined by the two end points and one of the following:             
 *   + Straight flag
 *   + Radius of curve
 *   + B-Spline control points
 *   
 * If any of the above values is set, the other values are not available.  It is
 * expected that 90% of all track will be placed as B-Spline so it will be the
 * default value.
 *                                                                                  
 * Each end point has a TrackBedProperties value that is shared when connected to  
 * another track item.  This is used to specify the trackbed and tie geometry      
 * values.  Some of these values are interpolated from end to end (if different)   
 * and others must be the same for the whole track.                                
 * 
 * In addition, the track item has a TrackBedMasterProperties value that can be    
 * used to override the endpoint values (for changing from wood to concrete ties,  
 * for example.
 * 
 * When the track item is selected, the outline changes to a brighter color and the
 * drag handles are set to visible to allow manipulation.  
 *  
 * If one or more endpoints (some items can have many endpoints) are connected, the
 * move and rotate handles will not be available.  All connected items must be placed
 * in a TrackItemGroup to be moved or rotated.
 * 
 * When moving by the main move handle, the entire track item moves without changing
 * size or orientation.  If an endpoint reaches snap-to distance of another 
 * endpoint the orientation of the track item will change to match the other item.
 * 
 * When rotating by the main rotate handle, the entire track item rotates without 
 * moving.
 * 
 * With Straight mode track, the only handles will be the move, rotate and endpoint
 * move handles.  If an endpoint is moved, the other endpoint remains stationary
 * and the track length and angle are adjusted.  When moving an endpoint to snap to
 * another item's endpoint, the track segment will rotate and keep the last 
 * length value.  If one endpoint is connected, moving the other endpoint will be
 * constrained to only change the length of track.
 * 
 * With Radius mode track, the only handles will be the move, rotate and endpoint move
 * handles.  If an endpoint is moved, the other endpoint remains stationary and the
 * motion is constrained to follow the arc.  The Radius and the Arc angle can be set
 * via the properties inspector to change the size as well.
 * 
 * With the B-Spline mode track, the handles will be the move, rotate, endpoint move
 * and endpoint control move.  When moving an endpoint with a control point, the two
 * points will move as a unit.
 *                                                                                  
 ************************************************************************************/
#endregion

namespace CAMTrak.Model.TrackItems
{
    public class TrackItemSingle : TrackItemBase
    {
        public TrackItemSingle(EditDocument Parent)
            : base(Parent)
        {

        }
    }
}
