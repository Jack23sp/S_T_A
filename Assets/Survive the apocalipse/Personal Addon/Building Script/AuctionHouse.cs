using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AuctionHouse : NetworkBehaviour
{
    public SyncListAuctionHouse actualInBid = new SyncListAuctionHouse();
    public SyncListAuctionHouse FinshedBid = new SyncListAuctionHouse();
    public SyncListAuctionHouse itemToRestituite = new SyncListAuctionHouse();
}
